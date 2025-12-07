/*
 * DAMAGE UP STATUS EFFECT SYSTEM DOCUMENTATION
 * 
 * How it works:
 * This system handles DMG_UP status effect with separate percentage and duration tracking.
 * Damage percentage determines damage amplification, duration determines how long it lasts.
 * The system intercepts damage actions to apply damage bonus.
 * 
 * Design reasoning:
 * Damage buffs multiply outgoing damage by a percentage.
 * Percentage and duration are tracked separately for clarity.
 * We modify damage in PRE phase before it's processed.
 * Duration is tracked in a separate dictionary and counted down each turn.
 * 
 * Integration:
 * - Performs ApplyDamageUpGA actions to grant damage buffs
 * - Subscribes to DealDamageGA PRE events to modify outgoing damage
 * - StatusEffectTickSystem calls TickDamageUp() each turn for duration countdown
 * - Stacks represent damage percentage, duration tracked separately
 * 
 * Used by:
 * - Dagát ng Kabisayaan Buff (+15% DMG)
 * - Blessing (+20% DMG for 2 turns)
 * - Generic damage buffs
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// System that handles DMG_UP status effect with separate percentage and duration
/// </summary>
public class DamageUpSystem : MonoBehaviour
{
    /// <summary>
    /// Tracks damage percentage for each combatant (key = instance ID)
    /// </summary>
    private Dictionary<int, int> damagePercentages = new Dictionary<int, int>();
    
    /// <summary>
    /// Tracks remaining duration for each combatant (key = instance ID)
    /// </summary>
    private Dictionary<int, int> damageDurations = new Dictionary<int, int>();
    
    /// <summary>
    /// Lookup to get combatant reference from instance ID
    /// </summary>
    private Dictionary<int, CombatantView> combatantLookup = new Dictionary<int, CombatantView>();

    private void OnEnable()
    {
        // Register performer for ApplyDamageUpGA
        ActionSystem.AttachPerformer<ApplyDamageUpGA>(ApplyDamageUpPerformer);
        
        // Subscribe to damage PRE events to modify damage from buffed combatants
        ActionSystem.SubscribeReaction<DealDamageGA>(OnDamageAboutToBeDealt, ReactionTiming.PRE);
    }

    private void OnDisable()
    {
        // Unregister performer
        ActionSystem.DetachPerformer<ApplyDamageUpGA>();
        
        // Unsubscribe from reactions
        ActionSystem.UnsubscribeReaction<DealDamageGA>(OnDamageAboutToBeDealt, ReactionTiming.PRE);
    }

    /// <summary>
    /// Performs ApplyDamageUpGA action to grant damage buffs
    /// </summary>
    private IEnumerator ApplyDamageUpPerformer(ApplyDamageUpGA action)
    {
        foreach (var target in action.Targets)
        {
            if (target == null) continue;
            
            int instanceId = target.GetInstanceID();
            
            // Store or update damage data
            damagePercentages[instanceId] = action.DamagePercentage;
            damageDurations[instanceId] = action.Duration;
            combatantLookup[instanceId] = target;
            
            // Apply status effect with percentage as stacks (for UI display)
            target.AddStatusEffect(StatusEffectType.DMG_UP, action.DamagePercentage);
            
            Debug.Log($"[DamageUpSystem] {target.name} gains +{action.DamagePercentage}% damage for {action.Duration} turns");
        }
        
        yield return null;
    }

    /// <summary>
    /// Modifies outgoing damage if the caster has DMG_UP status
    /// </summary>
    /// <param name="damageAction">The damage action to potentially modify</param>
    /// <remarks>
    /// Checks if the damage caster has DMG_UP status. If so, increases damage by the stack percentage.
    /// Handles both uniform damage (Amount) and per-target damage (PerTargetDamages).
    /// </remarks>
    private void OnDamageAboutToBeDealt(DealDamageGA damageAction)
    {
        // Check if there's a caster and they have DMG_UP
        if (damageAction.Caster == null) return;
        
        int instanceId = damageAction.Caster.GetInstanceID();
        if (!damagePercentages.ContainsKey(instanceId)) return;
        
        int damagePercent = damagePercentages[instanceId];
        
        // Calculate damage multiplier (e.g., 20% = 1.2x damage)
        float damageMultiplier = 1f + (damagePercent / 100f);
        
        // Apply damage multiplier
        bool hasPerTargetDamages = damageAction.PerTargetDamages != null && 
                                   damageAction.PerTargetDamages.Count == damageAction.Targets.Count;
        
        if (hasPerTargetDamages)
        {
            // Modify per-target damages
            for (int i = 0; i < damageAction.PerTargetDamages.Count; i++)
            {
                damageAction.PerTargetDamages[i] *= damageMultiplier;
            }
            Debug.Log($"[DamageUpSystem] {damageAction.Caster.name}'s +{damagePercent}% DMG amplified per-target damage!");
        }
        else
        {
            // Modify uniform damage
            damageAction.Amount *= damageMultiplier;
            Debug.Log($"[DamageUpSystem] {damageAction.Caster.name}'s +{damagePercent}% DMG amplified damage (multiplier: {damageMultiplier}x)");
        }
    }

    /// <summary>
    /// Reduces damage buff duration by 1 turn for a combatant
    /// Called by StatusEffectTickSystem at the end of each turn
    /// </summary>
    /// <param name="combatant">The combatant whose damage buff duration to reduce</param>
    public void TickDamageUp(CombatantView combatant)
    {
        if (combatant == null) return;
        
        int instanceId = combatant.GetInstanceID();
        if (!damageDurations.ContainsKey(instanceId)) return;
        
        damageDurations[instanceId]--;
        
        if (damageDurations[instanceId] <= 0)
        {
            // Damage buff expired, remove it
            int damagePercent = damagePercentages[instanceId];
            combatant.RemoveStatusEffect(StatusEffectType.DMG_UP, damagePercent);
            
            damagePercentages.Remove(instanceId);
            damageDurations.Remove(instanceId);
            combatantLookup.Remove(instanceId);
            
            Debug.Log($"[DamageUpSystem] {combatant.name}'s +{damagePercent}% damage buff expired");
        }
        else
        {
            Debug.Log($"[DamageUpSystem] {combatant.name}'s damage buff duration: {damageDurations[instanceId]} turns remaining");
        }
    }

    /// <summary>
    /// Gets the damage multiplier for a combatant based on their damage buff
    /// </summary>
    /// <param name="instanceId">Instance ID of the combatant</param>
    /// <returns>Damage multiplier (1.0 = no buff, 1.5 = +50% damage)</returns>
    public float GetDamageMultiplier(int instanceId)
    {
        if (damagePercentages.TryGetValue(instanceId, out int percentage))
        {
            // Convert percentage to multiplier (e.g., 20% = 1.2x)
            return 1f + (percentage / 100f);
        }
        
        return 1f; // No damage buff
    }
}
