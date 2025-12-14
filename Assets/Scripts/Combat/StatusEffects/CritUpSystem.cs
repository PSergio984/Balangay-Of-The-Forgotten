/*
 * CRIT UP STATUS EFFECT SYSTEM DOCUMENTATION
 * 
 * How it works:
 * This system handles CRIT_UP status effect with separate percentage and duration tracking.
 * Crit percentage determines crit chance increase, duration determines how long it lasts.
 * The system provides utility methods for crit chance calculation.
 * 
 * Design reasoning:
 * Crit buffs increase critical hit chance by a percentage.
 * Percentage and duration are tracked separately for clarity.
 * Duration is tracked in a separate dictionary and counted down each turn.
 * 
 * Integration:
 * - Performs ApplyCritUpGA actions to grant crit buffs
 * - Provides GetCritChanceBonus() method for damage/attack systems
 * - StatusEffectTickSystem calls TickCritUp() each turn for duration countdown
 * - Stacks represent crit percentage, duration tracked separately
 * 
 * Used by:
 * - Lunar Strike (+55% crit chance for 1 turn)
 * - Solar Flare Slash (+55% crit chance for 1 turn)
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// System that handles CRIT_UP status effect with separate percentage and duration
/// </summary>
public class CritUpSystem : MonoBehaviour
{
    /// <summary>
    /// Tracks crit percentage for each combatant (key = instance ID)
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
        // Register the performer for ApplyCritUpGA
        ActionSystem.AttachPerformer<ApplyCritUpGA>(ApplyCritUpPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyCritUpGA>();
    }

    /// <summary>
    /// Applies CRIT_UP status to all targets
    /// </summary>
    private IEnumerator ApplyCritUpPerformer(ApplyCritUpGA action)
    {
        foreach (var target in action.Targets)
        {
            if (target == null) continue;
            
            int instanceId = target.GetInstanceID();
            
            // Check if already has crit up - take the higher percentage
            if (critPercentages.TryGetValue(instanceId, out int existingPercent))
            {
                if (action.CritPercentage > existingPercent)
                {
                    critPercentages[instanceId] = action.CritPercentage;
                    critDurations[instanceId] = action.Duration;
                    
                    // Remove old stacks and set to new duration (UI shows duration, not percentage)
                    int currentStacks = target.GetStatusEffectStacks(StatusEffectType.CRIT_UP);
                    if (currentStacks > 0)
                    {
                        target.RemoveStatusEffect(StatusEffectType.CRIT_UP, currentStacks);
                    }
                    target.AddStatusEffect(StatusEffectType.CRIT_UP, action.Duration);
                    
                    Debug.Log($"[CritUpSystem] {target.name}'s crit buff upgraded to +{action.CritPercentage}% for {action.Duration} turns");
                }
                else
                {
                    // Refresh duration if same or lower percentage
                    critDurations[instanceId] = Mathf.Max(critDurations[instanceId], action.Duration);
                    // Remove old stacks and set to new duration
                    int currentStacks = target.GetStatusEffectStacks(StatusEffectType.CRIT_UP);
                    if (currentStacks > 0)
                    {
                        target.RemoveStatusEffect(StatusEffectType.CRIT_UP, currentStacks);
                    }
                    target.AddStatusEffect(StatusEffectType.CRIT_UP, critDurations[instanceId]);
                    Debug.Log($"[CritUpSystem] {target.name}'s crit buff duration refreshed to {critDurations[instanceId]} turns");
                }
            }
            else
            {
                // New crit buff
                critPercentages[instanceId] = action.CritPercentage;
                critDurations[instanceId] = action.Duration;
                combatantLookup[instanceId] = target;
                
                // Apply status effect with duration as stacks (for UI display) - shows remaining turns
                target.AddStatusEffect(StatusEffectType.CRIT_UP, action.Duration);
                
                Debug.Log($"[CritUpSystem] {target.name} gains +{action.CritPercentage}% crit chance for {action.Duration} turns");
            }
        }
        
        yield return null;
    }

    /// <summary>
    /// Ticks down the crit buff duration for a combatant
    /// Called by StatusEffectTickSystem at the end of each turn
    /// </summary>
    /// <param name="combatant">The combatant to tick</param>
    public void TickCritUp(CombatantView combatant)
    {
        if (combatant == null) return;
        
        int instanceId = combatant.GetInstanceID();
        if (!critDurations.ContainsKey(instanceId)) return;
        
        // Decrease duration
        critDurations[instanceId]--;
        
        int critPercent = critPercentages[instanceId];
        
        if (critDurations[instanceId] <= 0)
        {
            // Crit buff expired
            critPercentages.Remove(instanceId);
            critDurations.Remove(instanceId);
            combatantLookup.Remove(instanceId);
            
            // Remove status effect from visual display (use current stack count, not percentage)
            int currentStacks = combatant.GetStatusEffectStacks(StatusEffectType.CRIT_UP);
            if (currentStacks > 0)
            {
                combatant.RemoveStatusEffect(StatusEffectType.CRIT_UP, currentStacks);
            }
            
            Debug.Log($"[CritUpSystem] {combatant.name}'s +{critPercent}% crit buff expired");
        }
        else
        {
            // Update UI to show remaining duration (not percentage)
            // Remove old stacks first, then set to new duration
            int currentStacks = combatant.GetStatusEffectStacks(StatusEffectType.CRIT_UP);
            if (currentStacks > 0)
            {
                combatant.RemoveStatusEffect(StatusEffectType.CRIT_UP, currentStacks);
            }
            combatant.AddStatusEffect(StatusEffectType.CRIT_UP, critDurations[instanceId]);
            Debug.Log($"[CritUpSystem] {combatant.name}'s crit buff duration: {critDurations[instanceId]} turns remaining");
        }
    }

    /// <summary>
    /// Gets the critical hit chance bonus for a combatant
    /// </summary>
    /// <param name="instanceId">Instance ID of the combatant</param>
    /// <returns>Crit chance bonus as decimal (e.g., 0.55 = +55% crit chance)</returns>
    public float GetCritChanceBonus(int instanceId)
    {
        if (critPercentages.TryGetValue(instanceId, out int percentage))
        {
            return percentage / 100f;
        }
        
        return 0f; // No crit buff
    }
    
    /// <summary>
    /// Gets the critical hit chance bonus for a combatant
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <returns>Crit chance bonus as decimal (e.g., 0.55 = +55% crit chance)</returns>
    public float GetCritChanceBonus(CombatantView combatant)
    {
        if (combatant == null) return 0f;
        return GetCritChanceBonus(combatant.GetInstanceID());
    }

    /// <summary>
    /// Checks if an attack should crit based on the attacker's crit chance
    /// </summary>
    /// <param name="attacker">The attacking combatant</param>
    /// <param name="baseCritChance">Base crit chance (0.0 to 1.0)</param>
    /// <returns>True if the attack crits</returns>
    public bool RollForCrit(CombatantView attacker, float baseCritChance = 0f)
    {
        float totalCritChance = baseCritChance + GetCritChanceBonus(attacker);
        
        // Cap crit chance at 100%
        totalCritChance = Mathf.Min(totalCritChance, 1f);
        
        float roll = Random.value;
        bool isCrit = roll <= totalCritChance;
        
        if (isCrit)
        {
            Debug.Log($"[CritUpSystem] {attacker.name} scored a CRITICAL HIT! (roll: {roll:F2}, chance: {totalCritChance:F2})");
        }
        
        return isCrit;
    }

    /// <summary>
    /// Gets the total crit chance including base and bonus
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <param name="baseCritChance">Base crit chance</param>
    /// <returns>Total crit chance as decimal</returns>
    public float GetTotalCritChance(CombatantView combatant, float baseCritChance = 0f)
    {
        float totalCritChance = baseCritChance + GetCritChanceBonus(combatant);
        return Mathf.Min(totalCritChance, 1f); // Cap at 100%
    }
}
