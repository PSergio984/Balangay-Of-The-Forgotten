/*
 * SHIELD STATUS EFFECT SYSTEM DOCUMENTATION
 * 
 * How it works:
 * This system handles the SHIELD status effect.
 * Shields provide temporary HP that absorbs damage before real HP.
 * Shields expire after a set number of turns.
 * 
 * Design reasoning:
 * Shields differ from Armor:
 * - Armor is consumed 1:1 with damage
 * - Shields have a total HP pool that expires over time
 * We intercept damage actions to apply shield absorption first.
 * 
 * Integration:
 * - Attaches performer for ApplyShieldGA
 * - Subscribes PRE to DealDamageGA to absorb damage with shields
 * - Tracks shield duration separately from stacks
 * 
 * Used by:
 * - Fortify (Bagani): Gain shield equal to +30% max HP for 2 turns
 * - Guardian's Oath (Bagani): Shield allies for 25% of caster's current HP
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// System that processes the SHIELD status effect
/// </summary>
public class ShieldStatusEffectSystem : MonoBehaviour
{
    /// <summary>
    /// Tracks shield amounts per combatant instance ID (prevents memory leaks from destroyed objects)
    /// </summary>
    private static Dictionary<int, int> shieldAmounts = new Dictionary<int, int>();
    
    /// <summary>
    /// Tracks shield duration per combatant instance ID
    /// </summary>
    private static Dictionary<int, int> shieldDurations = new Dictionary<int, int>();
    
    /// <summary>
    /// Lookup table to retrieve CombatantView from instance ID (for status effect updates)
    /// </summary>
    private static Dictionary<int, CombatantView> combatantLookup = new Dictionary<int, CombatantView>();

    private void OnEnable()
    {
        // Register the performer for ApplyShieldGA
        ActionSystem.AttachPerformer<ApplyShieldGA>(ApplyShieldPerformer);
        
        // Subscribe to damage to intercept and apply shield absorption
        ActionSystem.SubscribeReaction<DealDamageGA>(OnDamageReceived, ReactionTiming.PRE);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyShieldGA>();
        ActionSystem.UnsubscribeReaction<DealDamageGA>(OnDamageReceived, ReactionTiming.PRE);
    }

    /// <summary>
    /// Applies SHIELD status to all targets
    /// </summary>
    /// <param name="action">The shield action to process</param>
    /// <returns>Coroutine for action processing</returns>
    private IEnumerator ApplyShieldPerformer(ApplyShieldGA action)
    {
        foreach (var shieldTarget in action.ShieldTargets)
        {
            var target = shieldTarget.Target;
            int amount = shieldTarget.Amount;
            
            if (target == null) continue;
            
            int instanceId = target.GetInstanceID();
            combatantLookup[instanceId] = target;
            
            // Apply shield amount
            if (action.CanStack && shieldAmounts.ContainsKey(instanceId))
            {
                shieldAmounts[instanceId] += amount;
                // Update duration to the higher value
                if (shieldDurations.ContainsKey(instanceId))
                {
                    shieldDurations[instanceId] = Mathf.Max(shieldDurations[instanceId], action.Duration);
                }
                // Remove all SHIELD stacks and re-add with new total for UI sync
                int currentStacks = target.GetStatusEffectStacks(StatusEffectType.SHIELD);
                if (currentStacks > 0)
                {
                    target.RemoveStatusEffect(StatusEffectType.SHIELD, currentStacks);
                }
                target.AddStatusEffect(StatusEffectType.SHIELD, shieldAmounts[instanceId]);
                Debug.Log($"[ShieldSystem] {target.name} gains {amount} SHIELD (stacked) for {action.Duration} turns. Total: {shieldAmounts[instanceId]}");
            }
            else
            {
                // Remove existing shield status if present
                if (shieldAmounts.ContainsKey(instanceId))
                {
                    target.RemoveStatusEffect(StatusEffectType.SHIELD, shieldAmounts[instanceId]);
                }
                shieldAmounts[instanceId] = amount;
                shieldDurations[instanceId] = action.Duration;
                // Add SHIELD status indicator (stacks represent shield amount for UI)
                target.AddStatusEffect(StatusEffectType.SHIELD, amount);
                Debug.Log($"[ShieldSystem] {target.name} gains {amount} SHIELD for {action.Duration} turns. Total: {shieldAmounts[instanceId]}");
            }
            // TODO: Play shield VFX
            yield return null;
        }
    }

    /// <summary>
    /// Intercepts damage and applies shield absorption
    /// </summary>
    /// <param name="action">The damage action to potentially modify</param>
    private void OnDamageReceived(DealDamageGA action)
    {
        // Process each target for shield absorption
        for (int i = 0; i < action.Targets.Count; i++)
        {
            var target = action.Targets[i];
            if (target == null) continue;
            
            int instanceId = target.GetInstanceID();
            if (!shieldAmounts.ContainsKey(instanceId) || shieldAmounts[instanceId] <= 0) continue;
            
            int currentShield = shieldAmounts[instanceId];
            
            // Check if per-target damage is available for THIS target (may change during loop)
            bool hasPerTargetDamages = action.PerTargetDamages != null && 
                                       action.PerTargetDamages.Count > i;
            
            // Get damage for this target
            float damageToTarget = hasPerTargetDamages ? action.PerTargetDamages[i] : action.Amount;
            
            if (damageToTarget <= 0) continue;
            
            // Calculate absorption as integer to prevent shield drift
            int absorbedInt = Mathf.RoundToInt(Mathf.Min(damageToTarget, currentShield));
            float remainingDamage = damageToTarget - absorbedInt;
            int remainingShield = currentShield - absorbedInt;
            
            // Update shield amount
            shieldAmounts[instanceId] = remainingShield;
            
            // Update status effect stacks for UI
            if (remainingShield <= 0)
            {
                target.RemoveStatusEffect(StatusEffectType.SHIELD, currentShield);
                shieldAmounts.Remove(instanceId);
                shieldDurations.Remove(instanceId);
                combatantLookup.Remove(instanceId);
                Debug.Log($"[ShieldSystem] {target.name}'s shield is BROKEN! Absorbed {absorbedInt} damage.");
            }
            else
            {
                // Remove the absorbed amount from status stacks
                target.RemoveStatusEffect(StatusEffectType.SHIELD, absorbedInt);
                Debug.Log($"[ShieldSystem] {target.name}'s shield absorbed {absorbedInt} damage. Remaining: {remainingShield}");
            }
            
            // Update the action's damage for this target
            // Re-check current state since we may have just created the list
            bool currentlyHasPerTargetDamages = action.PerTargetDamages != null && 
                                                action.PerTargetDamages.Count > i;
            
            if (currentlyHasPerTargetDamages)
            {
                action.PerTargetDamages[i] = remainingDamage;
            }
            else
            {
                // Create per-target damages list if needed
                if (action.PerTargetDamages == null)
                {
                    action.PerTargetDamages = new List<float>();
                    for (int j = 0; j < action.Targets.Count; j++)
                    {
                        action.PerTargetDamages.Add(j == i ? remainingDamage : action.Amount);
                    }
                }
                else
                {
                    // Ensure the list is large enough before assigning
                    while (action.PerTargetDamages.Count <= i)
                    {
                        action.PerTargetDamages.Add(action.Amount);
                    }
                    action.PerTargetDamages[i] = remainingDamage;
                }
            }
        }
    }

    /// <summary>
    /// Gets the current shield amount for a combatant
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <returns>Current shield HP</returns>
    public static int GetShieldAmount(CombatantView combatant)
    {
        if (combatant == null) return 0;
        int instanceId = combatant.GetInstanceID();
        return shieldAmounts.ContainsKey(instanceId) ? shieldAmounts[instanceId] : 0;
    }

    /// <summary>
    /// Decrements shield duration at end of turn
    /// Should be called at end of each combatant's turn
    /// </summary>
    /// <param name="combatant">The combatant whose shield duration should decrement</param>
    public static void DecrementShieldDuration(CombatantView combatant)
    {
        if (combatant == null) return;
        
        int instanceId = combatant.GetInstanceID();
        if (!shieldDurations.ContainsKey(instanceId)) return;
        
        shieldDurations[instanceId]--;
        
        if (shieldDurations[instanceId] <= 0)
        {
            // Shield expired
            int expiredAmount = shieldAmounts.ContainsKey(instanceId) ? shieldAmounts[instanceId] : 0;
            combatant.RemoveStatusEffect(StatusEffectType.SHIELD, expiredAmount);
            shieldAmounts.Remove(instanceId);
            shieldDurations.Remove(instanceId);
            combatantLookup.Remove(instanceId);
            Debug.Log($"[ShieldSystem] {combatant.name}'s shield EXPIRED!");
        }
    }

    /// <summary>
    /// Clears all shield tracking data (for combat reset)
    /// </summary>
    public static void ClearAllShields()
    {
        // Remove status effects before clearing tracking data
        foreach (var kvp in shieldAmounts)
        {
            if (combatantLookup.TryGetValue(kvp.Key, out var combatant) && combatant != null)
            {
                combatant.RemoveStatusEffect(StatusEffectType.SHIELD, kvp.Value);
            }
        }
        shieldAmounts.Clear();
        shieldDurations.Clear();
        combatantLookup.Clear();
    }
}
