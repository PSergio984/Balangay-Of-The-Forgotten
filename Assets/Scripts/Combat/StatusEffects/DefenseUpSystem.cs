/*
 * DEFENSE UP STATUS EFFECT SYSTEM DOCUMENTATION
 * 
 * How it works:
 * This system handles DEFENSE_UP status effect with separate percentage and duration tracking.
 * Defense percentage determines damage reduction, duration determines how long it lasts.
 * The system intercepts damage calculations to apply defense bonus.
 * 
 * Design reasoning:
 * Defense buffs reduce incoming damage by percentage.
 * Percentage and duration are tracked separately for clarity.
 * Stacks represent the defense percentage (50 stacks = 50% damage reduction).
 * Duration is tracked in a separate dictionary and counted down each turn.
 * 
 * Integration:
 * - Performs ApplyDefenseUpGA actions to grant defense buffs
 * - Subscribes to DealDamageGA PRE events to modify incoming damage
 * - StatusEffectTickSystem calls TickDefenseUp() each turn for duration countdown
 * - Stacks represent defense percentage, duration tracked separately
 * 
 * Used by:
 * - Last Stand (+50% defense for 3 turns when HP <20%)
 * - Daragang Magayon Buff (+25% defense to two players)
 * - Heaven's Mandate (Bathala: +30% DEF for 3 turns)
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// System that handles DEFENSE_UP status effect with separate percentage and duration
/// </summary>
public class DefenseUpSystem : MonoBehaviour
{
    /// <summary>
    /// Tracks defense percentage for each combatant (key = instance ID)
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
        // Register performer for ApplyDefenseUpGA
        ActionSystem.AttachPerformer<ApplyDefenseUpGA>(ApplyDefenseUpPerformer);
        
        // Subscribe to damage PRE events to modify defense
        ActionSystem.SubscribeReaction<DealDamageGA>(OnDamageAboutToBeDealt, ReactionTiming.PRE);
    }

    private void OnDisable()
    {
        // Unregister performer
        ActionSystem.DetachPerformer<ApplyDefenseUpGA>();
        
        // Unsubscribe from reactions
        ActionSystem.UnsubscribeReaction<DealDamageGA>(OnDamageAboutToBeDealt, ReactionTiming.PRE);
    }

    /// <summary>
    /// Performs ApplyDefenseUpGA action to grant defense buffs
    /// </summary>
    private IEnumerator ApplyDefenseUpPerformer(ApplyDefenseUpGA action)
    {
        foreach (var target in action.Targets)
        {
            if (target == null) continue;
            
            int instanceId = target.GetInstanceID();
            
            // Store or update defense data
            defensePercentages[instanceId] = action.DefensePercentage;
            defenseDurations[instanceId] = action.Duration;
            combatantLookup[instanceId] = target;
            
            // Apply status effect with percentage as stacks (for UI display) and custom name for consolidated sprite
            target.AddStatusEffect(StatusEffectType.DEFENSE_UP, action.DefensePercentage, action.CustomName);
            
            Debug.Log($"[DefenseUpSystem] {target.name} gains +{action.DefensePercentage}% defense for {action.Duration} turns");
        }
        
        yield return null;
    }

    /// <summary>
    /// Modifies incoming damage when DEFENSE_UP is present on targets
    /// </summary>
    /// <param name="damageAction">The damage action to potentially modify</param>
    /// <remarks>
    /// Checks each target for DEFENSE_UP. If present, reduces damage they take
    /// by the defense percentage. Higher defense = less damage taken.
    /// This is a damage reduction applied during the PRE phase.
    /// </remarks>
    private void OnDamageAboutToBeDealt(DealDamageGA damageAction)
    {
        // Check if using per-target damage (multiple targets with different damage values)
        bool hasPerTargetDamage = damageAction.PerTargetDamages != null && damageAction.PerTargetDamages.Count > 0;

        if (hasPerTargetDamage)
        {
            // Modify each target's damage individually
            for (int i = 0; i < damageAction.Targets.Count; i++)
            {
                CombatantView target = damageAction.Targets[i];
                if (target == null) continue;

                int instanceId = target.GetInstanceID();
                if (defensePercentages.ContainsKey(instanceId))
                {
                    int defensePercent = defensePercentages[instanceId];
                    float originalDamage = damageAction.PerTargetDamages[i];
                    float damageReduction = GetDamageReduction(defensePercent);
                    float modifiedDamage = originalDamage * (1f - damageReduction);

                    damageAction.PerTargetDamages[i] = modifiedDamage;

                    Debug.Log($"[DefenseUpSystem] {target.name} has +{defensePercent}% defense (-{defensePercent}% damage taken). Damage reduced: {originalDamage:F1} → {modifiedDamage:F1}");
                }
            }
        }
        else
        {
            // Apply uniform damage modification to all targets
            // Find the highest defense up among targets
            int maxDefenseUp = 0;
            CombatantView targetWithMaxBuff = null;

            foreach (var target in damageAction.Targets)
            {
                if (target == null) continue;

                int instanceId = target.GetInstanceID();
                if (defensePercentages.ContainsKey(instanceId))
                {
                    int defensePercent = defensePercentages[instanceId];
                    if (defensePercent > maxDefenseUp)
                    {
                        maxDefenseUp = defensePercent;
                        targetWithMaxBuff = target;
                    }
                }
            }

            if (maxDefenseUp > 0)
            {
                float originalDamage = damageAction.Amount;
                float damageReduction = GetDamageReduction(maxDefenseUp);
                float modifiedDamage = originalDamage * (1f - damageReduction);

                damageAction.Amount = modifiedDamage;

                Debug.Log($"[DefenseUpSystem] Targets have DEFENSE_UP (max: +{maxDefenseUp}% on {targetWithMaxBuff.name}). Damage reduced: {originalDamage:F1} → {modifiedDamage:F1}");
            }
        }
    }

    /// <summary>
    /// Reduces defense duration by 1 turn for a combatant
    /// Called by StatusEffectTickSystem at the end of each turn
    /// </summary>
    /// <param name="combatant">The combatant whose defense duration to reduce</param>
    public void TickDefenseUp(CombatantView combatant)
    {
        if (combatant == null) return;
        
        int instanceId = combatant.GetInstanceID();
        if (!defenseDurations.ContainsKey(instanceId)) return;
        
        defenseDurations[instanceId]--;
        
        if (defenseDurations[instanceId] <= 0)
        {
            // Defense buff expired, remove it
            int defensePercent = defensePercentages[instanceId];
            combatant.RemoveStatusEffect(StatusEffectType.DEFENSE_UP, defensePercent);
            
            defensePercentages.Remove(instanceId);
            defenseDurations.Remove(instanceId);
            combatantLookup.Remove(instanceId);
            
            Debug.Log($"[DefenseUpSystem] {combatant.name}'s +{defensePercent}% defense buff expired");
        }
        else
        {
            Debug.Log($"[DefenseUpSystem] {combatant.name}'s defense buff duration: {defenseDurations[instanceId]} turns remaining");
        }
    }

    /// <summary>
    /// Gets the damage reduction percentage for a given defense percentage
    /// </summary>
    /// <param name="defensePercent">The defense percentage (e.g., 50 = 50%)</param>
    /// <returns>Damage reduction as decimal (0.5 = 50% reduction)</returns>
    /// <remarks>
    /// Defense percentage directly translates to damage reduction:
    /// - 50% defense = 50% damage reduction = 0.5
    /// - 30% defense = 30% damage reduction = 0.3
    /// Capped at 90% to prevent near-invulnerability
    /// </remarks>
    public static float GetDamageReduction(int defensePercent)
    {
        float reduction = defensePercent / 100f;
        return Mathf.Min(reduction, 0.9f); // Cap at 90% reduction
    }

    /// <summary>
    /// Gets the effective defense multiplier for a combatant
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <returns>Defense multiplier (1.0 = no buff, 1.5 = +50% defense)</returns>
    /// <remarks>
    /// DEPRECATED: This method is kept for backwards compatibility.
    /// New code should use the ApplyDefenseUpGA system instead.
    /// </remarks>
    public static float GetDefenseMultiplier(CombatantView combatant)
    {
        if (combatant == null) return 1f;
        
        int defenseUpStacks = combatant.GetStatusEffectStacks(StatusEffectType.DEFENSE_UP);
        return 1f + (defenseUpStacks / 100f);
    }
}
