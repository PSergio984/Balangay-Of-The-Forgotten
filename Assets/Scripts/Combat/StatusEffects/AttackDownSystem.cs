/*
 * ATTACK DOWN STATUS EFFECT SYSTEM DOCUMENTATION
 * 
 * How it works:
 * This system handles ATTACK_DOWN status effect with separate percentage and duration tracking.
 * Attack reduction determines damage decrease, duration determines how long it lasts.
 * The system intercepts damage actions to apply attack penalty.
 * 
 * Design reasoning:
 * Attack debuffs decrease outgoing damage by a percentage.
 * Percentage and duration are tracked separately for clarity.
 * We modify damage in PRE phase before it's processed.
 * Duration is tracked in a separate dictionary and counted down each turn.
 * 
 * Integration:
 * - Performs ApplyAttackDownGA actions to apply attack debuffs
 * - Subscribes to DealDamageGA PRE events to reduce outgoing damage
 * - StatusEffectTickSystem calls TickAttackDown() each turn for duration countdown
 * - Stacks represent attack reduction percentage, duration tracked separately
 * 
 * Used by:
 * - Weaken (-30% ATK for 2 turns)
 * - Enfeeble (-20% ATK for 3 turns)
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// System that handles ATTACK_DOWN status effect with separate percentage and duration
/// </summary>
public class AttackDownSystem : MonoBehaviour
{
    /// <summary>
    /// Tracks attack reduction percentage for each combatant (key = instance ID)
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
        // Register the performer for ApplyAttackDownGA
        ActionSystem.AttachPerformer<ApplyAttackDownGA>(ApplyAttackDownPerformer);
        
        // Subscribe to damage PRE events to reduce ATK-based damage
        ActionSystem.SubscribeReaction<DealDamageGA>(OnDamageAboutToBeDealt, ReactionTiming.PRE);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyAttackDownGA>();
        ActionSystem.UnsubscribeReaction<DealDamageGA>(OnDamageAboutToBeDealt, ReactionTiming.PRE);
    }

    /// <summary>
    /// Applies ATTACK_DOWN status to all targets
    /// </summary>
    private IEnumerator ApplyAttackDownPerformer(ApplyAttackDownGA action)
    {
        foreach (var target in action.Targets)
        {
            if (target == null) continue;
            
            int instanceId = target.GetInstanceID();
            
            // Check if already has attack down - take the higher reduction
            if (attackPercentages.TryGetValue(instanceId, out int existingPercent))
            {
                if (action.AttackPercentage > existingPercent)
                {
                    attackPercentages[instanceId] = action.AttackPercentage;
                    attackDurations[instanceId] = action.Duration;
                    
                    // Remove old stacks and set to new duration (UI shows duration, not percentage)
                    int currentStacks = target.GetStatusEffectStacks(StatusEffectType.ATTACK_DOWN);
                    if (currentStacks > 0)
                    {
                        target.RemoveStatusEffect(StatusEffectType.ATTACK_DOWN, currentStacks);
                    }
                    target.AddStatusEffect(StatusEffectType.ATTACK_DOWN, action.Duration);
                    
                    Debug.Log($"[AttackDownSystem] {target.name}'s attack debuff worsened to -{action.AttackPercentage}% for {action.Duration} turns");
                }
                else
                {
                    // Refresh duration if same or lower percentage
                    attackDurations[instanceId] = Mathf.Max(attackDurations[instanceId], action.Duration);
                    // Remove old stacks and set to new duration
                    int currentStacks = target.GetStatusEffectStacks(StatusEffectType.ATTACK_DOWN);
                    if (currentStacks > 0)
                    {
                        target.RemoveStatusEffect(StatusEffectType.ATTACK_DOWN, currentStacks);
                    }
                    target.AddStatusEffect(StatusEffectType.ATTACK_DOWN, attackDurations[instanceId]);
                    Debug.Log($"[AttackDownSystem] {target.name}'s attack debuff duration refreshed to {attackDurations[instanceId]} turns");
                }
            }
            else
            {
                // New attack debuff
                attackPercentages[instanceId] = action.AttackPercentage;
                attackDurations[instanceId] = action.Duration;
                combatantLookup[instanceId] = target;
                
                // Apply status effect with duration as stacks (for UI display) - shows remaining turns
                target.AddStatusEffect(StatusEffectType.ATTACK_DOWN, action.Duration);
                
                Debug.Log($"[AttackDownSystem] {target.name} receives -{action.AttackPercentage}% attack for {action.Duration} turns");
            }
        }
        
        yield return null;
    }

    /// <summary>
    /// Reduces outgoing damage if the caster has ATTACK_DOWN status
    /// </summary>
    private void OnDamageAboutToBeDealt(DealDamageGA damageAction)
    {
        // Check if there's a caster and they have ATTACK_DOWN
        if (damageAction.Caster == null) return;
        
        int instanceId = damageAction.Caster.GetInstanceID();
        if (!attackPercentages.TryGetValue(instanceId, out int attackPercent)) return;
        
        // Calculate attack multiplier (e.g., 30% reduction = 0.7x damage)
        float attackMultiplier = 1f - (attackPercent / 100f);
        attackMultiplier = Mathf.Max(0.1f, attackMultiplier); // Minimum 10% damage
        
        // Apply attack reduction to damage
        bool hasPerTargetDamages = damageAction.PerTargetDamages != null && 
                                   damageAction.PerTargetDamages.Count == damageAction.Targets.Count;
        
        if (hasPerTargetDamages)
        {
            // Modify per-target damages
            for (int i = 0; i < damageAction.PerTargetDamages.Count; i++)
            {
                damageAction.PerTargetDamages[i] *= attackMultiplier;
            }
            Debug.Log($"[AttackDownSystem] {damageAction.Caster.name}'s -{attackPercent}% ATTACK_DOWN reduced per-target damage!");
        }
        else
        {
            // Modify uniform damage
            damageAction.Amount *= attackMultiplier;
            Debug.Log($"[AttackDownSystem] {damageAction.Caster.name}'s -{attackPercent}% ATTACK_DOWN reduced damage (multiplier: {attackMultiplier}x)");
        }
    }

    /// <summary>
    /// Ticks down the attack debuff duration for a combatant
    /// Called by StatusEffectTickSystem at the end of each turn
    /// </summary>
    /// <param name="combatant">The combatant to tick</param>
    public void TickAttackDown(CombatantView combatant)
    {
        if (combatant == null) return;
        
        int instanceId = combatant.GetInstanceID();
        if (!attackDurations.ContainsKey(instanceId)) return;
        
        // Decrease duration
        attackDurations[instanceId]--;
        
        int attackPercent = attackPercentages[instanceId];
        
        if (attackDurations[instanceId] <= 0)
        {
            // Attack debuff expired
            attackPercentages.Remove(instanceId);
            attackDurations.Remove(instanceId);
            combatantLookup.Remove(instanceId);
            
            // Remove status effect from visual display (use current stack count, not percentage)
            int currentStacks = combatant.GetStatusEffectStacks(StatusEffectType.ATTACK_DOWN);
            if (currentStacks > 0)
            {
                combatant.RemoveStatusEffect(StatusEffectType.ATTACK_DOWN, currentStacks);
            }
            
            Debug.Log($"[AttackDownSystem] {combatant.name}'s -{attackPercent}% attack debuff expired");
        }
        else
        {
            // Update UI to show remaining duration (not percentage)
            // Remove old stacks first, then set to new duration
            int currentStacks = combatant.GetStatusEffectStacks(StatusEffectType.ATTACK_DOWN);
            if (currentStacks > 0)
            {
                combatant.RemoveStatusEffect(StatusEffectType.ATTACK_DOWN, currentStacks);
            }
            combatant.AddStatusEffect(StatusEffectType.ATTACK_DOWN, attackDurations[instanceId]);
            Debug.Log($"[AttackDownSystem] {combatant.name}'s attack debuff duration: {attackDurations[instanceId]} turns remaining");
        }
    }

    /// <summary>
    /// Gets the attack reduction for a combatant
    /// </summary>
    /// <param name="instanceId">Instance ID of the combatant</param>
    /// <returns>Attack reduction as decimal (0.3 = -30% attack)</returns>
    public float GetAttackReduction(int instanceId)
    {
        if (attackPercentages.TryGetValue(instanceId, out int percentage))
        {
            return percentage / 100f;
        }
        
        return 0f; // No attack debuff
    }
}
