/*
 * TEMPORARY HP STATUS EFFECT SYSTEM DOCUMENTATION
 * 
 * How it works:
 * This system handles TEMP_HP status effect that provides temporary hit points.
 * Temporary HP absorbs damage before real HP is affected.
 * Stacks represent the amount of temporary HP (30 stacks = 30 temp HP).
 * 
 * Design reasoning:
 * Temporary HP is a shield mechanic that absorbs damage without healing.
 * Unlike ARMOR (which provides damage reduction), TEMP_HP is a flat damage absorption.
 * Temp HP is consumed when taking damage and doesn't regenerate.
 * When temp HP is depleted, remaining damage carries over to real HP.
 * 
 * Integration:
 * - Subscribes to DealDamageGA PRE phase to intercept and absorb damage
 * - Automatically reduces temp HP stacks when absorbing damage
 * - Works with existing status effect tracking system
 * - Complements SHIELD (which may have different mechanics)
 * 
 * Used by:
 * - Protective abilities that grant temporary shields
 * - Tank abilities for damage mitigation
 * - Support abilities to protect allies
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// System that handles TEMP_HP status effect (temporary hit points)
/// </summary>
/// <remarks>
/// TEMP_HP provides temporary hit points that absorb damage before real HP.
/// Stacks represent temp HP amount (30 stacks = 30 temp HP).
/// Temp HP is consumed when absorbing damage.
/// </remarks>
public class TempHpSystem : MonoBehaviour
{
    private void OnEnable()
    {
        // Subscribe to PRE phase of damage dealing to intercept damage
        ActionSystem.SubscribeReaction<DealDamageGA>(OnDamage, ReactionTiming.PRE);
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        ActionSystem.UnsubscribeReaction<DealDamageGA>(OnDamage, ReactionTiming.PRE);
    }

    /// <summary>
    /// Handles damage absorption when TEMP_HP is present on targets
    /// </summary>
    private void OnDamage(GameAction action)
    {
        DealDamageGA damageAction = action as DealDamageGA;
        if (damageAction == null) return;

        // Check if using per-target damage (multiple targets with different damage values)
        bool hasPerTargetDamage = damageAction.PerTargetDamages != null && damageAction.PerTargetDamages.Count > 0;

        if (hasPerTargetDamage)
        {
            // Handle each target's temp HP individually
            for (int i = 0; i < damageAction.Targets.Count; i++)
            {
                CombatantView target = damageAction.Targets[i];
                float incomingDamage = damageAction.PerTargetDamages[i];
                
                int tempHpStacks = target.GetStatusEffectStacks(StatusEffectType.TEMP_HP);

                if (tempHpStacks > 0)
                {
                    // Calculate damage absorption
                    int damageAbsorbed = Mathf.Min(tempHpStacks, Mathf.RoundToInt(incomingDamage));
                    float remainingDamage = incomingDamage - damageAbsorbed;
                    int remainingTempHp = tempHpStacks - damageAbsorbed;

                    // Update the damage amount
                    damageAction.PerTargetDamages[i] = remainingDamage;

                    Debug.Log($"[TempHpSystem] {target.name}'s TEMP_HP absorbed {damageAbsorbed} damage. Damage: {incomingDamage} → {remainingDamage}, Temp HP: {tempHpStacks} → {remainingTempHp}");

                    // Schedule temp HP update
                    UpdateTempHp(target, remainingTempHp);
                }
            }
        }
        else
        {
            // Handle uniform damage for all targets
            foreach (var target in damageAction.Targets)
            {
                float incomingDamage = damageAction.Amount;
                int tempHpStacks = target.GetStatusEffectStacks(StatusEffectType.TEMP_HP);

                if (tempHpStacks > 0)
                {
                    // Calculate damage absorption
                    int damageAbsorbed = Mathf.Min(tempHpStacks, Mathf.RoundToInt(incomingDamage));
                    float remainingDamage = incomingDamage - damageAbsorbed;
                    int remainingTempHp = tempHpStacks - damageAbsorbed;

                    // Update the damage amount
                    damageAction.Amount = remainingDamage;

                    Debug.Log($"[TempHpSystem] {target.name}'s TEMP_HP absorbed {damageAbsorbed} damage. Damage: {incomingDamage} → {remainingDamage}, Temp HP: {tempHpStacks} → {remainingTempHp}");

                    // Schedule temp HP update
                    UpdateTempHp(target, remainingTempHp);
                }
            }
        }
    }

    /// <summary>
    /// Updates or removes temp HP after damage absorption
    /// </summary>
    private void UpdateTempHp(CombatantView target, int newTempHp)
    {
        if (newTempHp <= 0)
        {
            // Remove temp HP when depleted
            target.RemoveStatusEffect(StatusEffectType.TEMP_HP, target.GetStatusEffectStacks(StatusEffectType.TEMP_HP));
        }
        else
        {
            // Update temp HP stacks by removing all and adding new amount
            int currentStacks = target.GetStatusEffectStacks(StatusEffectType.TEMP_HP);
            target.RemoveStatusEffect(StatusEffectType.TEMP_HP, currentStacks);
            target.AddStatusEffect(StatusEffectType.TEMP_HP, newTempHp);
        }
    }

    /// <summary>
    /// Gets the current temp HP amount for a combatant
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <returns>Current temp HP amount</returns>
    public static int GetTempHp(CombatantView combatant)
    {
        if (combatant == null) return 0;
        return combatant.GetStatusEffectStacks(StatusEffectType.TEMP_HP);
    }

    /// <summary>
    /// Calculates total effective HP (real HP + temp HP)
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <returns>Total effective HP</returns>
    public static int GetEffectiveHp(CombatantView combatant)
    {
        if (combatant == null) return 0;
        
        int currentHp = combatant.CurrentHealth;
        int tempHp = GetTempHp(combatant);
        
        return currentHp + tempHp;
    }

    /// <summary>
    /// Checks if a combatant has temporary HP
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <returns>True if temp HP is active</returns>
    public static bool HasTempHp(CombatantView combatant)
    {
        if (combatant == null) return false;
        return combatant.GetStatusEffectStacks(StatusEffectType.TEMP_HP) > 0;
    }
}
