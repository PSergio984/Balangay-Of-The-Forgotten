/*
 * DEFENSE DOWN STATUS EFFECT SYSTEM DOCUMENTATION
 * 
 * How it works:
 * This system handles DEFENSE_DOWN status effect with separate percentage and duration tracking.
 * Defense reduction determines how much more damage the target takes.
 * Duration determines how long the debuff lasts.
 * 
 * Design reasoning:
 * Defense debuffs amplify damage by reducing target's damage reduction.
 * Percentage and duration are tracked separately for clarity.
 * Duration is tracked in a separate dictionary and counted down each turn.
 * 
 * Integration:
 * - Performs ApplyDefenseDownGA actions to apply defense debuffs
 * - Subscribes to DealDamageGA in PRE phase to modify damage calculations
 * - StatusEffectTickSystem calls TickDefenseDown() each turn for duration countdown
 * - Stacks represent defense reduction percentage, duration tracked separately
 * 
 * Used by:
 * - Bonecrack ability (-30% defense for 1 turn)
 * - Moonfall ability (-20% defense for 2 turns)
 * - Serpent's Coil (-15% defense for 2 turns)
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// System that handles DEFENSE_DOWN status effect with separate percentage and duration
/// </summary>
public class DefenseDownSystem : MonoBehaviour
{
    /// <summary>
    /// Tracks defense reduction percentage for each combatant (key = instance ID)
    /// </summary>
    private Dictionary<int, int> defensePercentages = new Dictionary<int, int>();
    
    /// <summary>
    /// Tracks remaining duration for each combatant (key = instance ID)
    /// </summary>
    private Dictionary<int, int> defenseDurations = new Dictionary<int, int>();
    
    /// <summary>
    /// Lookup to get combatant reference from instance ID
    /// </summary>
    private Dictionary<int, CombatantView> combatantLookup = new Dictionary<int, CombatantView>();

    private void OnEnable()
    {
        // Register the performer for ApplyDefenseDownGA
        ActionSystem.AttachPerformer<ApplyDefenseDownGA>(ApplyDefenseDownPerformer);
        
        // Subscribe to PRE phase of damage dealing
        ActionSystem.SubscribeReaction<DealDamageGA>(OnDamage, ReactionTiming.PRE);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyDefenseDownGA>();
        ActionSystem.UnsubscribeReaction<DealDamageGA>(OnDamage, ReactionTiming.PRE);
    }

    /// <summary>
    /// Applies DEFENSE_DOWN status to all targets
    /// </summary>
    private IEnumerator ApplyDefenseDownPerformer(ApplyDefenseDownGA action)
    {
        foreach (var target in action.Targets)
        {
            if (target == null) continue;
            
            int instanceId = target.GetInstanceID();
            
            // Check if already has defense down - take the higher reduction
            if (defensePercentages.TryGetValue(instanceId, out int existingPercent))
            {
                if (action.DefensePercentage > existingPercent)
                {
                    defensePercentages[instanceId] = action.DefensePercentage;
                    defenseDurations[instanceId] = action.Duration;
                    
                    // Update visual stacks to new value
                    int diff = action.DefensePercentage - existingPercent;
                    target.AddStatusEffect(StatusEffectType.DEFENSE_DOWN, diff);
                    
                    Debug.Log($"[DefenseDownSystem] {target.name}'s defense debuff worsened to -{action.DefensePercentage}% for {action.Duration} turns");
                }
                else
                {
                    // Refresh duration if same or lower percentage
                    defenseDurations[instanceId] = Mathf.Max(defenseDurations[instanceId], action.Duration);
                    Debug.Log($"[DefenseDownSystem] {target.name}'s defense debuff duration refreshed to {defenseDurations[instanceId]} turns");
                }
            }
            else
            {
                // New defense debuff
                defensePercentages[instanceId] = action.DefensePercentage;
                defenseDurations[instanceId] = action.Duration;
                combatantLookup[instanceId] = target;
                
                // Apply status effect with percentage as stacks (for UI display)
                target.AddStatusEffect(StatusEffectType.DEFENSE_DOWN, action.DefensePercentage);
                
                Debug.Log($"[DefenseDownSystem] {target.name} receives -{action.DefensePercentage}% defense for {action.Duration} turns");
            }
        }
        
        yield return null;
    }

    /// <summary>
    /// Handles damage modification when DEFENSE_DOWN is present on targets
    /// </summary>
    private void OnDamage(GameAction action)
    {
        DealDamageGA damageAction = action as DealDamageGA;
        if (damageAction == null) return;

        // Check if using per-target damage (multiple targets with different damage values)
        bool hasPerTargetDamage = damageAction.PerTargetDamages != null && damageAction.PerTargetDamages.Count > 0;

        if (hasPerTargetDamage)
        {
            // Modify each target's damage individually
            for (int i = 0; i < damageAction.Targets.Count; i++)
            {
                CombatantView target = damageAction.Targets[i];
                int instanceId = target.GetInstanceID();
                
                if (!defensePercentages.TryGetValue(instanceId, out int defenseDownPercent)) continue;

                float originalDamage = damageAction.PerTargetDamages[i];
                float defenseMultiplier = GetDefenseMultiplier(defenseDownPercent);
                float modifiedDamage = originalDamage * defenseMultiplier;

                damageAction.PerTargetDamages[i] = modifiedDamage;

                Debug.Log($"[DefenseDownSystem] {target.name} has -{defenseDownPercent}% DEFENSE_DOWN. Damage increased: {originalDamage:F1} → {modifiedDamage:F1}");
            }
        }
        else
        {
            // Apply uniform damage modification to all targets
            // Find the highest defense down among targets
            int maxDefenseDown = 0;
            CombatantView targetWithMaxDebuff = null;

            foreach (var target in damageAction.Targets)
            {
                int instanceId = target.GetInstanceID();
                if (defensePercentages.TryGetValue(instanceId, out int defenseDownPercent))
                {
                    if (defenseDownPercent > maxDefenseDown)
                    {
                        maxDefenseDown = defenseDownPercent;
                        targetWithMaxDebuff = target;
                    }
                }
            }

            if (maxDefenseDown > 0)
            {
                float originalDamage = damageAction.Amount;
                float defenseMultiplier = GetDefenseMultiplier(maxDefenseDown);
                float modifiedDamage = originalDamage * defenseMultiplier;

                damageAction.Amount = modifiedDamage;

                Debug.Log($"[DefenseDownSystem] Targets have DEFENSE_DOWN (max: -{maxDefenseDown}% on {targetWithMaxDebuff.name}). Damage increased: {originalDamage:F1} → {modifiedDamage:F1}");
            }
        }
    }

    /// <summary>
    /// Ticks down the defense debuff duration for a combatant
    /// Called by StatusEffectTickSystem at the end of each turn
    /// </summary>
    /// <param name="combatant">The combatant to tick</param>
    public void TickDefenseDown(CombatantView combatant)
    {
        if (combatant == null) return;
        
        int instanceId = combatant.GetInstanceID();
        if (!defenseDurations.ContainsKey(instanceId)) return;
        
        // Decrease duration
        defenseDurations[instanceId]--;
        
        int defensePercent = defensePercentages[instanceId];
        
        if (defenseDurations[instanceId] <= 0)
        {
            // Defense debuff expired
            defensePercentages.Remove(instanceId);
            defenseDurations.Remove(instanceId);
            combatantLookup.Remove(instanceId);
            
            // Remove all stacks from visual display
            combatant.RemoveStatusEffect(StatusEffectType.DEFENSE_DOWN, defensePercent);
            
            Debug.Log($"[DefenseDownSystem] {combatant.name}'s -{defensePercent}% defense debuff expired");
        }
        else
        {
            Debug.Log($"[DefenseDownSystem] {combatant.name}'s defense debuff duration: {defenseDurations[instanceId]} turns remaining");
        }
    }

    /// <summary>
    /// Calculates the defense multiplier based on DEFENSE_DOWN percentage
    /// </summary>
    /// <param name="defenseDownPercent">Defense down percentage</param>
    /// <returns>Damage multiplier (e.g., 1.30 for 30% = +30% damage taken)</returns>
    public static float GetDefenseMultiplier(int defenseDownPercent)
    {
        return 1.0f + (defenseDownPercent / 100f);
    }

    /// <summary>
    /// Gets the defense reduction for a combatant
    /// </summary>
    /// <param name="instanceId">Instance ID of the combatant</param>
    /// <returns>Defense reduction as percentage (e.g., 30 = -30% defense)</returns>
    public int GetDefenseReduction(int instanceId)
    {
        if (defensePercentages.TryGetValue(instanceId, out int percentage))
        {
            return percentage;
        }
        
        return 0; // No defense debuff
    }
}
