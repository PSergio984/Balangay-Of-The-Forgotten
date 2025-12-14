using System.Collections.Generic;
using UnityEngine;

/* DEAL PERCENTAGE CURRENT HP DAMAGE EFFECT DOCUMENTATION
 * 
 * How it works:
 * This effect deals damage based on a percentage of each target's CURRENT HP.
 * Unlike DealDamageEffect which uses flat/stat-based damage, this uses percentage-based damage.
 * Perfect for abilities like "Tide of Night" that deal 30% of current HP.
 * 
 * Design reasoning:
 * Some abilities need to deal damage based on target's current HP percentage.
 * This is different from max HP percentage or flat damage.
 * Uses DamageCalculator.CalculateCurrentHPDamage for consistency.
 * 
 * Integration:
 * - Inherits from Effects base class
 * - Uses DamageCalculator for calculation
 * - Creates DealDamageGA for processing
 * 
 * Used by:
 * - Tide of Night (Mayari): 30% current HP damage to all heroes
 */

/// <summary>
/// Effect that deals damage based on percentage of target's current HP
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Deals percentage-based damage using target's current HP</para>
/// 
/// <para><strong>What it does:</strong> This effect calculates damage as a percentage of each 
/// target's current health. For example, if a target has 500 HP and the percentage is 0.3 (30%), 
/// they take 150 damage. This is useful for abilities that scale with the target's remaining health.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>For each target, calculates: damage = target.CurrentHealth × damagePercentage</item>
/// <item>Uses DamageCalculator.CalculateCurrentHPDamage for consistency</item>
/// <item>Creates DealDamageGA with per-target damage amounts</item>
/// <item>Damage bypasses normal defense calculation (direct HP percentage)</item>
/// </list>
/// 
/// <para><strong>Examples:</strong></para>
/// <list type="bullet">
/// <item>Tide of Night: 30% current HP damage to all heroes (damagePercentage = 0.3)</item>
/// <item>Execute: 50% current HP damage to low-health targets (damagePercentage = 0.5)</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> DamageCalculator, DealDamageGA, ActionSystem</para>
/// </remarks>
[System.Serializable]
public class DealPercentageCurrentHPDamageEffect : Effects
{
    /// <summary>
    /// Percentage of target's current HP to deal as damage (0.3 = 30% current HP)
    /// </summary>
    [SerializeField]
    [Tooltip("Percentage of target's current HP to deal as damage (0.3 = 30%, 0.5 = 50%)")]
    [Range(0f, 1f)]
    private float damagePercentage = 0.3f;

    /// <summary>
    /// Creates a damage action that deals percentage-based current HP damage to targets
    /// </summary>
    /// <param name="targets">Who should receive the damage</param>
    /// <param name="caster">Who is dealing the damage</param>
    /// <returns>DealDamageGA action with per-target percentage damage</returns>
    /// <remarks>
    /// Calculates damage for each target based on their current HP.
    /// Uses DamageCalculator.CalculateCurrentHPDamage for consistency.
    /// Creates DealDamageGA with per-target damage list for proper processing.
    /// </remarks>
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning("[DealPercentageCurrentHPDamageEffect] No targets provided!");
            return new DealDamageGA(0, new List<CombatantView>(), caster);
        }

        // Calculate per-target damage based on their current HP
        List<CombatantView> filteredTargets = new List<CombatantView>(targets.Count);
        List<float> perTargetDamages = new List<float>(targets.Count);

        foreach (var target in targets)
        {
            if (target == null || target.CurrentHealth <= 0)
            {
                continue; // Skip null or dead targets
            }

            // Calculate damage as percentage of current HP
            int damage = DamageCalculator.CalculateCurrentHPDamage(target.CurrentHealth, damagePercentage);
            
            filteredTargets.Add(target);
            perTargetDamages.Add(damage);
            
            Debug.Log($"[DealPercentageCurrentHPDamageEffect] {caster?.name ?? "Unknown"} deals {damage} damage ({damagePercentage:P0} of {target.name}'s {target.CurrentHealth} current HP)");
        }

        if (filteredTargets.Count == 0)
        {
            Debug.LogWarning("[DealPercentageCurrentHPDamageEffect] No valid targets after filtering!");
            return new DealDamageGA(0, new List<CombatantView>(), caster);
        }

        // Create DealDamageGA with per-target damage amounts
        return new DealDamageGA(perTargetDamages, filteredTargets, caster);
    }
}
