using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

/* DEFENSE IGNORE SYSTEM DOCUMENTATION
 * 
 * How it works:
 * This system handles defense ignore buffs that make attacks ignore a percentage of enemy defense.
 * When a combatant with defense ignore deals damage, the system recalculates damage with reduced defense.
 * Uses StatusEffectType.DEFENSE_IGNORE for tick-based duration management.
 * 
 * Design reasoning:
 * Defense ignore is applied during damage calculation (PRE phase) by recalculating damage with ignored defense.
 * This system tracks which combatants have defense ignore and for how long.
 * Integrated with StatusEffectTickSystem for automatic duration management.
 * 
 * Integration:
 * - Attaches performer for ApplyDefenseIgnoreGA
 * - Subscribes to DealDamageGA PRE events to apply defense ignore
 * - Uses DamageCalculator to properly recalculate damage with ignored defense
 * - Integrated with StatusEffectTickSystem for duration management
 * 
 * Used by:
 * - Daybreak Fury (Apolaki): Ignores 20% DEF for 1 turn
 */

/// <summary>
/// System that handles defense ignore buffs for combatants
/// </summary>
public class DefenseIgnoreSystem : MonoBehaviour
{
    /// <summary>
    /// Tracks defense ignore percentage per combatant (instanceId -> percentage as decimal)
    /// </summary>
    private static Dictionary<int, float> defenseIgnorePercentages = new Dictionary<int, float>();
    
    /// <summary>
    /// Tracks duration per combatant (instanceId -> turns remaining)
    /// </summary>
    private static Dictionary<int, int> defenseIgnoreDurations = new Dictionary<int, int>();
    
    /// <summary>
    /// Lookup for combatant references (instanceId -> CombatantView)
    /// </summary>
    private static Dictionary<int, CombatantView> combatantLookup = new Dictionary<int, CombatantView>();

    /// <summary>
    /// Tracks if we've subscribed to prevent double-subscription
    /// </summary>
    private bool isSubscribed = false;

    private void OnEnable()
    {
        // Register performer for ApplyDefenseIgnoreGA
        ActionSystem.AttachPerformer<ApplyDefenseIgnoreGA>(ApplyDefenseIgnorePerformer);
        
        // Subscribe to damage PRE events to apply defense ignore
        if (!isSubscribed)
        {
            ActionSystem.SubscribeReaction<DealDamageGA>(OnDamageAboutToBeDealt, ReactionTiming.PRE);
            isSubscribed = true;
        }
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyDefenseIgnoreGA>();
        
        // WORKAROUND: Manually remove subscription using reflection to work around ActionSystem.UnsubscribeReaction bug
        // ActionSystem.UnsubscribeReaction creates a new wrapper that doesn't match the original,
        // so we manually access the private dictionary and remove our wrapper
        if (isSubscribed)
        {
            try
            {
                // Get ActionSystem type to access private static fields
                System.Type actionSystemType = typeof(ActionSystem);
                
                // Get the preSubs dictionary (private static field)
                var preSubsField = actionSystemType.GetField("preSubs", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                
                if (preSubsField != null)
                {
                    var preSubs = preSubsField.GetValue(null) as System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.List<System.Action<GameAction>>>;
                    
                    if (preSubs != null && preSubs.ContainsKey(typeof(DealDamageGA)))
                    {
                        var reactions = preSubs[typeof(DealDamageGA)];
                        var wrappersToRemove = new List<System.Action<GameAction>>();
                        
                        // Identify our wrapper by checking if wrapper.Target == this
                        // This works for instance method delegates (which is what we use)
                        foreach (var wrapper in reactions.ToList()) // ToList to avoid modification during iteration
                        {
                            try
                            {
                                // Check if this wrapper belongs to this instance
                                if (ReferenceEquals(wrapper.Target, this))
                                {
                                    wrappersToRemove.Add(wrapper);
                                }
                            }
                            catch
                            {
                                // Skip wrappers that throw exceptions during inspection
                            }
                        }
                        
                        // Remove identified wrappers
                        bool removed = false;
                        foreach (var wrapper in wrappersToRemove)
                        {
                            if (reactions.Remove(wrapper))
                            {
                                removed = true;
                            }
                        }
                        
                        // Clean up empty lists
                        if (reactions.Count == 0)
                        {
                            preSubs.Remove(typeof(DealDamageGA));
                        }
                        
                        if (removed)
                        {
                            Debug.Log("[DefenseIgnoreSystem] Successfully removed subscription using reflection workaround");
                        }
                        else
                        {
                            Debug.LogWarning("[DefenseIgnoreSystem] Could not identify our subscription wrapper to remove (wrapper.Target != this). This is a known ActionSystem.UnsubscribeReaction bug. The subscription may leak memory until ActionSystem is fixed.");
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[DefenseIgnoreSystem] Failed to manually clean up subscription via reflection: {e.Message}. This is a known ActionSystem.UnsubscribeReaction bug that affects all systems.");
            }
            
            isSubscribed = false;
        }
    }

    /// <summary>
    /// Applies defense ignore buff to all targets
    /// </summary>
    /// <param name="action">The defense ignore action to process</param>
    /// <returns>Coroutine for action processing</returns>
    private IEnumerator ApplyDefenseIgnorePerformer(ApplyDefenseIgnoreGA action)
    {
        foreach (var target in action.Targets)
        {
            if (target == null) continue;
            
            int instanceId = target.GetInstanceID();
            
            // Convert percentage to decimal (20% = 0.2)
            float ignorePercent = action.DefenseIgnorePercentage / 100f;
            
            // Store defense ignore data
            defenseIgnorePercentages[instanceId] = ignorePercent;
            defenseIgnoreDurations[instanceId] = action.Duration;
            combatantLookup[instanceId] = target;
            
            // Apply status effect for UI display and tick system integration
            target.AddStatusEffect(StatusEffectType.DEFENSE_IGNORE, action.Duration);
            
            Debug.Log($"[DefenseIgnoreSystem] {target.name} gains {action.DefenseIgnorePercentage}% defense ignore for {action.Duration} turn(s)");
            
            yield return null;
        }
    }

    /// <summary>
    /// Applies defense ignore to damage if the caster has the buff
    /// </summary>
    /// <param name="damageAction">The damage action to potentially modify</param>
    /// <remarks>
    /// FIXED: Properly recalculates damage using DamageCalculator with ignored defense.
    /// Works backwards from current damage to estimate skill power, then recalculates with ignored defense.
    /// </remarks>
    private void OnDamageAboutToBeDealt(DealDamageGA damageAction)
    {
        // Check if there's a caster with defense ignore
        if (damageAction.Caster == null) return;
        
        int instanceId = damageAction.Caster.GetInstanceID();
        if (!defenseIgnorePercentages.ContainsKey(instanceId)) return;
        
        float ignorePercent = defenseIgnorePercentages[instanceId];
        
        Debug.Log($"[DefenseIgnoreSystem] {damageAction.Caster.name} has {ignorePercent * 100}% defense ignore - recalculating damage");
        
        // FIXED: Properly recalculate damage with ignored defense using the damage formula
        // Formula: DMG = (skillPower * amp * crit) / (coeff * (1 + DEF * 0.01))
        // Rearranging: (skillPower * amp * crit) = originalDamage * coeff * (1 + originalDef * 0.01)
        // New damage: newDamage = (skillPower * amp * crit) / (coeff * (1 + ignoredDef * 0.01))
        // Simplifies to: newDamage = originalDamage * (1 + originalDef * 0.01) / (1 + ignoredDef * 0.01)
        // This works regardless of skillPower, amp, crit, or coeff values!
        
        float coefficient = damageAction.Caster is EnemyView ? 1.5f : 1.0f;
        
        if (damageAction.PerTargetDamages != null && damageAction.PerTargetDamages.Count == damageAction.Targets.Count)
        {
            // Per-target damage: recalculate each target's damage with ignored defense
            for (int i = 0; i < damageAction.Targets.Count; i++)
            {
                var target = damageAction.Targets[i];
                if (target == null) continue;
                
                float originalDamage = damageAction.PerTargetDamages[i];
                float targetDefense = target.Defense;
                
                // Apply defense ignore to get effective defense
                float ignoredDefense = DamageCalculator.ApplyDefenseIgnore(targetDefense, ignorePercent);
                
                // Recalculate using the simplified formula
                // This accounts for all multipliers (skillPower, amp, crit, coeff) without needing to know them
                float originalDenominator = coefficient * (1f + targetDefense * 0.01f);
                float newDenominator = coefficient * (1f + ignoredDefense * 0.01f);
                float newDamage = originalDamage * (originalDenominator / newDenominator);
                
                damageAction.PerTargetDamages[i] = newDamage;
                
                Debug.Log($"[DefenseIgnoreSystem] Target {target.name}: DEF {targetDefense} → {ignoredDefense} (ignored {ignorePercent * 100}%), damage {originalDamage:F1} → {newDamage:F1}");
            }
        }
        else
        {
            // Uniform damage: use average defense of all targets
            float avgDefense = 0f;
            int validTargets = 0;
            foreach (var target in damageAction.Targets)
            {
                if (target != null)
                {
                    avgDefense += target.Defense;
                    validTargets++;
                }
            }
            
            if (validTargets > 0)
            {
                avgDefense /= validTargets;
                float originalDamage = damageAction.Amount;
                
                // Apply defense ignore
                float ignoredDefense = DamageCalculator.ApplyDefenseIgnore(avgDefense, ignorePercent);
                
                // Recalculate using the simplified formula
                float originalDenominator = coefficient * (1f + avgDefense * 0.01f);
                float newDenominator = coefficient * (1f + ignoredDefense * 0.01f);
                float newDamage = originalDamage * (originalDenominator / newDenominator);
                
                damageAction.Amount = newDamage;
                Debug.Log($"[DefenseIgnoreSystem] Average DEF {avgDefense} → {ignoredDefense} (ignored {ignorePercent * 100}%), damage {originalDamage:F1} → {newDamage:F1}");
            }
        }
    }

    /// <summary>
    /// Reduces defense ignore duration for a combatant (called by StatusEffectTickSystem)
    /// </summary>
    /// <param name="combatant">The combatant whose defense ignore should tick</param>
    public static void TickDefenseIgnore(CombatantView combatant)
    {
        if (combatant == null) return;
        
        int instanceId = combatant.GetInstanceID();
        if (!defenseIgnoreDurations.ContainsKey(instanceId)) return;
        
        int remainingTurns = defenseIgnoreDurations[instanceId] - 1;
        
        if (remainingTurns <= 0)
        {
            // Remove expired buff
            defenseIgnorePercentages.Remove(instanceId);
            defenseIgnoreDurations.Remove(instanceId);
            combatantLookup.Remove(instanceId);
            combatant.RemoveStatusEffect(StatusEffectType.DEFENSE_IGNORE, combatant.GetStatusEffectStacks(StatusEffectType.DEFENSE_IGNORE));
            Debug.Log($"[DefenseIgnoreSystem] {combatant.name}'s defense ignore expired");
        }
        else
        {
            defenseIgnoreDurations[instanceId] = remainingTurns;
            // Update status effect stacks to match remaining turns
            int currentStacks = combatant.GetStatusEffectStacks(StatusEffectType.DEFENSE_IGNORE);
            if (currentStacks != remainingTurns)
            {
                combatant.RemoveStatusEffect(StatusEffectType.DEFENSE_IGNORE, currentStacks);
                combatant.AddStatusEffect(StatusEffectType.DEFENSE_IGNORE, remainingTurns);
            }
        }
    }

    /// <summary>
    /// Gets the defense ignore percentage for a combatant
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <returns>Defense ignore percentage as decimal (0.2 = 20%), or 0 if none</returns>
    public static float GetDefenseIgnore(CombatantView combatant)
    {
        if (combatant == null) return 0f;
        
        int instanceId = combatant.GetInstanceID();
        if (defenseIgnorePercentages.ContainsKey(instanceId))
        {
            return defenseIgnorePercentages[instanceId];
        }
        
        return 0f;
    }
}
