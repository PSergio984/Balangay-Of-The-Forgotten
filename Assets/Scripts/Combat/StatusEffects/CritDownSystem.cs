/*
 * CRIT DOWN STATUS EFFECT SYSTEM DOCUMENTATION
 * 
 * How it works:
 * This system handles CRIT_DOWN status effect with separate percentage and duration tracking.
 * Crit reduction determines crit chance decrease, duration determines how long it lasts.
 * The system provides utility methods for crit chance calculation.
 * 
 * Design reasoning:
 * Crit debuffs decrease the target's critical hit chance.
 * Percentage and duration are tracked separately for clarity.
 * Duration is tracked in a separate dictionary and counted down each turn.
 * 
 * Integration:
 * - Performs ApplyCritDownGA actions to apply crit debuffs
 * - StatusEffectTickSystem calls TickCritDown() each turn for duration countdown
 * - Stacks represent crit reduction percentage, duration tracked separately
 * 
 * Used by:
 * - Dull Senses (-30% crit for 2 turns)
 * - Blind (-50% crit for 1 turn)
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// System that handles CRIT_DOWN status effect with separate percentage and duration
/// </summary>
public class CritDownSystem : MonoBehaviour
{
    /// <summary>
    /// Tracks crit reduction percentage for each combatant (key = instance ID)
    /// </summary>
    private Dictionary<int, int> critPercentages = new Dictionary<int, int>();
    
    /// <summary>
    /// Tracks remaining duration for each combatant (key = instance ID)
    /// </summary>
    private Dictionary<int, int> critDurations = new Dictionary<int, int>();
    
    /// <summary>
    /// Lookup to get combatant reference from instance ID
    /// </summary>
    private Dictionary<int, CombatantView> combatantLookup = new Dictionary<int, CombatantView>();

    private void OnEnable()
    {
        // Register the performer for ApplyCritDownGA
        ActionSystem.AttachPerformer<ApplyCritDownGA>(ApplyCritDownPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyCritDownGA>();
    }

    /// <summary>
    /// Applies CRIT_DOWN status to all targets
    /// </summary>
    private IEnumerator ApplyCritDownPerformer(ApplyCritDownGA action)
    {
        foreach (var target in action.Targets)
        {
            if (target == null) continue;
            
            int instanceId = target.GetInstanceID();
            
            // Check if already has crit down - take the higher reduction
            if (critPercentages.TryGetValue(instanceId, out int existingPercent))
            {
                if (action.CritPercentage > existingPercent)
                {
                    critPercentages[instanceId] = action.CritPercentage;
                    critDurations[instanceId] = action.Duration;
                    
                    // Remove old stacks and set to new duration (UI shows duration, not percentage)
                    int currentStacks = target.GetStatusEffectStacks(StatusEffectType.CRIT_DOWN);
                    if (currentStacks > 0)
                    {
                        target.RemoveStatusEffect(StatusEffectType.CRIT_DOWN, currentStacks);
                    }
                    target.AddStatusEffect(StatusEffectType.CRIT_DOWN, action.Duration);
                    
                    Debug.Log($"[CritDownSystem] {target.name}'s crit debuff worsened to -{action.CritPercentage}% for {action.Duration} turns");
                }
                else
                {
                    // Refresh duration if same or lower percentage
                    critDurations[instanceId] = Mathf.Max(critDurations[instanceId], action.Duration);
                    // Remove old stacks and set to new duration
                    int currentStacks = target.GetStatusEffectStacks(StatusEffectType.CRIT_DOWN);
                    if (currentStacks > 0)
                    {
                        target.RemoveStatusEffect(StatusEffectType.CRIT_DOWN, currentStacks);
                    }
                    target.AddStatusEffect(StatusEffectType.CRIT_DOWN, critDurations[instanceId]);
                    Debug.Log($"[CritDownSystem] {target.name}'s crit debuff duration refreshed to {critDurations[instanceId]} turns");
                }
            }
            else
            {
                // New crit debuff
                critPercentages[instanceId] = action.CritPercentage;
                critDurations[instanceId] = action.Duration;
                combatantLookup[instanceId] = target;
                
                // Apply status effect with duration as stacks (for UI display) - shows remaining turns
                target.AddStatusEffect(StatusEffectType.CRIT_DOWN, action.Duration);
                
                Debug.Log($"[CritDownSystem] {target.name} receives -{action.CritPercentage}% crit chance for {action.Duration} turns");
            }
        }
        
        yield return null;
    }

    /// <summary>
    /// Ticks down the crit debuff duration for a combatant
    /// Called by StatusEffectTickSystem at the end of each turn
    /// </summary>
    /// <param name="combatant">The combatant to tick</param>
    public void TickCritDown(CombatantView combatant)
    {
        if (combatant == null) return;
        
        int instanceId = combatant.GetInstanceID();
        if (!critDurations.ContainsKey(instanceId)) return;
        
        // Decrease duration
        critDurations[instanceId]--;
        
        int critPercent = critPercentages[instanceId];
        
        if (critDurations[instanceId] <= 0)
        {
            // Crit debuff expired
            critPercentages.Remove(instanceId);
            critDurations.Remove(instanceId);
            combatantLookup.Remove(instanceId);
            
            // Remove status effect from visual display (use current stack count, not percentage)
            int currentStacks = combatant.GetStatusEffectStacks(StatusEffectType.CRIT_DOWN);
            if (currentStacks > 0)
            {
                combatant.RemoveStatusEffect(StatusEffectType.CRIT_DOWN, currentStacks);
            }
            
            Debug.Log($"[CritDownSystem] {combatant.name}'s -{critPercent}% crit debuff expired");
        }
        else
        {
            // Update UI to show remaining duration (not percentage)
            // Remove old stacks first, then set to new duration
            int currentStacks = combatant.GetStatusEffectStacks(StatusEffectType.CRIT_DOWN);
            if (currentStacks > 0)
            {
                combatant.RemoveStatusEffect(StatusEffectType.CRIT_DOWN, currentStacks);
            }
            combatant.AddStatusEffect(StatusEffectType.CRIT_DOWN, critDurations[instanceId]);
            Debug.Log($"[CritDownSystem] {combatant.name}'s crit debuff duration: {critDurations[instanceId]} turns remaining");
        }
    }

    /// <summary>
    /// Gets the crit chance reduction for a combatant (penalty)
    /// </summary>
    /// <param name="instanceId">Instance ID of the combatant</param>
    /// <returns>Crit chance penalty as decimal (0.30 = -30% crit chance)</returns>
    public float GetCritChancePenalty(int instanceId)
    {
        if (critPercentages.TryGetValue(instanceId, out int percentage))
        {
            return percentage / 100f;
        }
        
        return 0f; // No crit debuff
    }
    
    /// <summary>
    /// Gets the crit chance penalty for a combatant
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <returns>Crit penalty as decimal (e.g., 0.30 = -30% crit chance)</returns>
    public float GetCritChancePenalty(CombatantView combatant)
    {
        if (combatant == null) return 0f;
        return GetCritChancePenalty(combatant.GetInstanceID());
    }
}
