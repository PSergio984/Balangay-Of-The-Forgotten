using System.Collections.Generic;
using UnityEngine;

/* DEAL DAMAGE EFFECT DOCUMENTATION
 * 
 * How it works:
 * - Contains a damage amount that can be set in Inspector
 * - Uses provided targets instead of auto-targeting all enemies  
 * - Creates a DealDamageGA action with proper caster tracking when effect triggers
 * - Part of the card effects system for damage-dealing cards and perk effects
 * 
 * Design reasoning:
 * - Flexible targeting allows both card effects and perk effects to use same damage logic
 * - Caster parameter enables perk system to track who is dealing damage
 * - Works with any target list instead of hardcoded "all enemies"
 * - Same effect can be used by cards, perks, and other game systems
 * 
 * Integration: Inherits from Effects base class, works with perk system and card system
 */

/// <summary>
/// Card and perk effect that deals damage to specified targets
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Creates damage actions for both card effects and perk effects</para>
/// 
/// <para><strong>What it does:</strong> This effect is used on cards and perks that deal damage. 
/// When triggered, it creates a damage action that targets whoever was specified in the 
/// targets list. The damage amount can be set in the Inspector to match the intended 
/// power level. Now works with the perk system for reactive damage effects.</para>
/// 
/// <para><strong>Perk system integration:</strong> When perks use this effect, they can 
/// target specific characters (like "damage the attacker") instead of just all enemies. 
/// The caster parameter ensures the perk system knows who is dealing the damage.</para>
/// 
/// <para><strong>Examples:</strong></para>
/// <list type="bullet">
/// <item>"Fireball" card - deals 5 damage to selected targets</item>
/// <item>Counter-attack perk - deals 3 damage back to attacking enemy</item>
/// <item>Revenge perk - deals damage to whoever hurt the player</item>
/// <item>Area damage perk - deals damage to all enemies when triggered</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> Effects base class, perk system for reactive damage, card system</para>
/// </remarks>
public class DealDamageEffect : Effects
{
        /// <summary>
        /// Flat skill damage added to the total damage. Set in Inspector.
        /// <br/>Total Damage = baseDamage + (AttackAmp × caster.AttackPower) + (MagicAmp × caster.MagicPower)
        /// </summary>
        [SerializeField] private float baseDamage = 0f;
        /// <summary>
        /// Damage amplification multiplier (1.0 = 100%, 1.2 = +20% damage, etc.). Set in Inspector.
        /// </summary>
        [SerializeField] private float damageAmplification = 1.0f; // Multiplier for skills like Sunburst Nova
        /// <summary>
        /// Chance to hit (0-1, e.g. 1 = 100% hit, 0.8 = 80% hit). Set in Inspector.
        /// </summary>
        [SerializeField] private float accuracy = 1f; // 1 = 100% hit
        /// <summary>
        /// Chance to crit (0-1, e.g. 0.25 = 25% crit chance). Set in Inspector.
        /// </summary>
        [SerializeField] private float critChance = 0f; // 0 = no crit
        /// <summary>
        /// Percent of target's defense ignored (0 = normal, 0.2 = ignore 20% DEF). Set in Inspector.
        /// </summary>
        [SerializeField] private float defenseIgnore = 0f; // 0 = normal, 0.2 = ignore 20% DEF
        /// <summary>
        /// Multiplier for caster's AttackPower. Set in Inspector.
        /// <br/>Use for physical or hybrid attacks. Set to 0 for pure magic moves.
        /// </summary>
        [SerializeField] private float AttackAmp = 1f; // Multiplier for caster's AttackPower
        /// <summary>
        /// Multiplier for caster's MagicPower. Set in Inspector.
        /// <br/>Use for magical or hybrid attacks. Set to 0 for pure physical moves.
        /// </summary>
        [SerializeField] private float MagicAmp = 1f; // Multiplier for caster's MagicPower
   
   

    /// <summary>
    /// Creates a damage action that targets the specified characters with caster tracking
    /// </summary>
    /// <param name="targets">Who should receive the damage</param>
    /// <param name="caster">Who is dealing the damage</param>
    /// <returns>DealDamageGA action that will hurt the specified targets</returns>
    /// <remarks>
    /// This method is called by both the card system and perk system when the effect should be executed.
    /// Uses the provided targets list instead of hardcoded enemy targeting, making it flexible for 
    /// both card effects (target all enemies) and perk effects (target specific characters).
    /// Includes caster parameter so the perk system can track who is dealing damage.
    /// 
    /// UPDATED: Now uses refactored DamageCalculator methods (IsHit, IsCriticalHit, GetCriticalMultiplier,
    /// CalculateSkillPower, CalculateFinalDamage) for improved clarity and consistency.
    /// </remarks>
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        // Step 1: Check if attack hits using refactored method
        bool hit = DamageCalculator.IsHit(accuracy);
        if (!hit)
        {
            Debug.Log($"[DealDamageEffect] Attack MISSED! Returning 0 damage.");
            // Filter out null targets for consistency
            List<CombatantView> filteredMissTargets = new List<CombatantView>(targets.Count);
            for (int i = 0; i < targets.Count; i++)
            {
                var target = targets[i];
                if (target == null)
                    continue;
                filteredMissTargets.Add(target);
                // DamagePopUp will be created in DamageSystem per target
            }
            return new DealDamageGA(0, filteredMissTargets, caster);
        }

        // Step 2: Calculate skill power using refactored method
        float skillPower = DamageCalculator.CalculateSkillPower(
            baseDamage, 
            AttackAmp, 
            MagicAmp, 
            caster.AttackPower, 
            caster.MagicPower
        );

        // Step 3: Determine coefficient based on attacker type
        float coefficient = caster is EnemyView ? 1.5f : 1.0f;

        // Step 4: Check for critical hit and get multiplier using refactored methods
        bool isCrit = DamageCalculator.IsCriticalHit(critChance);
        float critMultiplier = isCrit ? DamageCalculator.GetCriticalMultiplier(caster is HeroView) : 1.0f;

        // Step 5 & 6: Calculate per-target final damage using each target's defense
        // This ensures AoE attacks correctly account for each target's individual defense value
        // Build filtered lists of valid targets and their corresponding damages
        List<CombatantView> filteredTargets = new List<CombatantView>(targets.Count);
        List<float> filteredDamages = new List<float>(targets.Count);
        for (int i = 0; i < targets.Count; i++)
        {
            var target = targets[i];
            if (target == null)
            {
                continue; // Skip null targets entirely
            }

            // Apply defense ignore if specified
            float targetDefense = target.Defense;
            if (defenseIgnore > 0f)
            {
                targetDefense = DamageCalculator.ApplyDefenseIgnore(targetDefense, defenseIgnore);
            }

            // Calculate final damage for this specific target
            float finalDamage = DamageCalculator.CalculateFinalDamage(
                skillPower,
                damageAmplification,
                coefficient,
                targetDefense,
                critMultiplier
            );

            Debug.Log($"[DealDamageEffect] Final damage for target {i}: {finalDamage} (SkillPower={skillPower}, Coeff={coefficient}, Def={targetDefense}, Crit={critMultiplier})");
            filteredTargets.Add(target);
            filteredDamages.Add(finalDamage);
            // DamagePopUp will be created in DamageSystem per target
        }

        // Use the per-target damage constructor to properly apply defense-based calculations
        // Only valid (non-null) targets and their damages are included
        return new DealDamageGA(filteredDamages, filteredTargets, caster);
    }
}
