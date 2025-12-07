/*
 * ATTACK UP STATUS EFFECT SYSTEM DOCUMENTATION
 * 
 * How it works:
 * This system handles ATTACK_UP status effect with separate percentage and duration tracking.
 * Attack percentage determines damage amplification, duration determines how long it lasts.
 * The system intercepts damage actions to apply attack bonus.
 * 
 * Design reasoning:
 * Attack buffs increase outgoing damage by a percentage.
 * Percentage and duration are tracked separately for clarity.
 * We modify damage in PRE phase before it's processed.
 * Duration is tracked in a separate dictionary and counted down each turn.
 * 
 * Integration:
 * - Performs ApplyAttackUpGA actions to grant attack buffs
 * - Subscribes to DealDamageGA PRE events to modify outgoing damage
 * - StatusEffectTickSystem calls TickAttackUp() each turn for duration countdown
 * - Stacks represent attack percentage, duration tracked separately
 * 
 * Used by:
 * - Daybreak Fury (+40% ATK for 1 turn)
 * - Apolaki enrage (+40% ATK for 3 turns)
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// System that handles ATTACK_UP status effect with separate percentage and duration
/// </summary>
public class AttackUpSystem : MonoBehaviour
{
    /// <summary>
    /// Tracks attack percentage for each combatant (key = instance ID)
    /// </summary>
    private Dictionary<int, int> attackPercentages = new Dictionary<int, int>();
    
    /// <summary>
    /// Tracks remaining duration for each combatant (key = instance ID)
    /// </summary>
    private Dictionary<int, int> attackDurations = new Dictionary<int, int>();
    
    /// <summary>
    /// Lookup to get combatant reference from instance ID
    /// </summary>
    private Dictionary<int, CombatantView> combatantLookup = new Dictionary<int, CombatantView>();

    private void OnEnable()
    {
        // Register the performer for ApplyAttackUpGA
        ActionSystem.AttachPerformer<ApplyAttackUpGA>(ApplyAttackUpPerformer);
        
        // Subscribe to damage PRE events to modify ATK-based damage
        ActionSystem.SubscribeReaction<DealDamageGA>(OnDamageAboutToBeDealt, ReactionTiming.PRE);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyAttackUpGA>();
        ActionSystem.UnsubscribeReaction<DealDamageGA>(OnDamageAboutToBeDealt, ReactionTiming.PRE);
    }

    /// <summary>
    /// Applies ATTACK_UP status to all targets
    /// </summary>
    private IEnumerator ApplyAttackUpPerformer(ApplyAttackUpGA action)
    {
        foreach (var target in action.Targets)
        {
            if (target == null) continue;
            
            int instanceId = target.GetInstanceID();
            
            // Check if already has attack up - take the higher percentage
            if (attackPercentages.TryGetValue(instanceId, out int existingPercent))
            {
                if (action.AttackPercentage > existingPercent)
                {
                    attackPercentages[instanceId] = action.AttackPercentage;
                    attackDurations[instanceId] = action.Duration;
                    
                    // Update visual stacks to new value
                    int diff = action.AttackPercentage - existingPercent;
                    target.AddStatusEffect(StatusEffectType.ATTACK_UP, diff);
                    
                    Debug.Log($"[AttackUpSystem] {target.name}'s attack buff upgraded to +{action.AttackPercentage}% for {action.Duration} turns");
                }
                else
                {
                    // Refresh duration if same or lower percentage
                    attackDurations[instanceId] = Mathf.Max(attackDurations[instanceId], action.Duration);
                    Debug.Log($"[AttackUpSystem] {target.name}'s attack buff duration refreshed to {attackDurations[instanceId]} turns");
                }
            }
            else
            {
                // New attack buff
                attackPercentages[instanceId] = action.AttackPercentage;
                attackDurations[instanceId] = action.Duration;
                combatantLookup[instanceId] = target;
                
                // Apply status effect with percentage as stacks (for UI display)
                target.AddStatusEffect(StatusEffectType.ATTACK_UP, action.AttackPercentage);
                
                Debug.Log($"[AttackUpSystem] {target.name} gains +{action.AttackPercentage}% attack for {action.Duration} turns");
            }
        }
        
        yield return null;
    }

    /// <summary>
    /// Modifies outgoing damage if the caster has ATTACK_UP status
    /// </summary>
    private void OnDamageAboutToBeDealt(DealDamageGA damageAction)
    {
        // Check if there's a caster and they have ATTACK_UP
        if (damageAction.Caster == null) return;
        
        int instanceId = damageAction.Caster.GetInstanceID();
        if (!attackPercentages.TryGetValue(instanceId, out int attackPercent)) return;
        
        // Calculate attack multiplier (e.g., 40% = 1.4x damage)
        float attackMultiplier = 1f + (attackPercent / 100f);
        
        // Apply attack multiplier to damage
        bool hasPerTargetDamages = damageAction.PerTargetDamages != null && 
                                   damageAction.PerTargetDamages.Count == damageAction.Targets.Count;
        
        if (hasPerTargetDamages)
        {
            // Modify per-target damages
            for (int i = 0; i < damageAction.PerTargetDamages.Count; i++)
            {
                damageAction.PerTargetDamages[i] *= attackMultiplier;
            }
            Debug.Log($"[AttackUpSystem] {damageAction.Caster.name}'s +{attackPercent}% ATTACK_UP amplified per-target damage!");
        }
        else
        {
            // Modify uniform damage
            damageAction.Amount *= attackMultiplier;
            Debug.Log($"[AttackUpSystem] {damageAction.Caster.name}'s +{attackPercent}% ATTACK_UP amplified damage (multiplier: {attackMultiplier}x)");
        }
    }

    /// <summary>
    /// Ticks down the attack buff duration for a combatant
    /// Called by StatusEffectTickSystem at the end of each turn
    /// </summary>
    /// <param name="combatant">The combatant to tick</param>
    public void TickAttackUp(CombatantView combatant)
    {
        if (combatant == null) return;
        
        int instanceId = combatant.GetInstanceID();
        if (!attackDurations.ContainsKey(instanceId)) return;
        
        // Decrease duration
        attackDurations[instanceId]--;
        
        int attackPercent = attackPercentages[instanceId];
        
        if (attackDurations[instanceId] <= 0)
        {
            // Attack buff expired
            attackPercentages.Remove(instanceId);
            attackDurations.Remove(instanceId);
            combatantLookup.Remove(instanceId);
            
            // Remove all stacks from visual display
            combatant.RemoveStatusEffect(StatusEffectType.ATTACK_UP, attackPercent);
            
            Debug.Log($"[AttackUpSystem] {combatant.name}'s +{attackPercent}% attack buff expired");
        }
        else
        {
            Debug.Log($"[AttackUpSystem] {combatant.name}'s attack buff duration: {attackDurations[instanceId]} turns remaining");
        }
    }

    /// <summary>
    /// Gets the attack multiplier for a combatant
    /// </summary>
    /// <param name="instanceId">Instance ID of the combatant</param>
    /// <returns>Attack multiplier (1.0 = no buff, 1.4 = +40% attack)</returns>
    public float GetAttackMultiplier(int instanceId)
    {
        if (attackPercentages.TryGetValue(instanceId, out int percentage))
        {
            return 1f + (percentage / 100f);
        }
        
        return 1f; // No attack buff
    }
}
