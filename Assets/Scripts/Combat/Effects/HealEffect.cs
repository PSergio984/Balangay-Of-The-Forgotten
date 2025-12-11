using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Card and perk effect that heals specified targets and optionally heals the caster (self-heal, lifesteal, etc.)
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Creates healing actions for cards and perks, supporting ally heal, self-heal, percent heal, and lifesteal.</para>
/// <para><strong>Examples:</strong></para>
/// <list type="bullet">
/// <item>Heal ally for 100 + 50% MAG, heal self for 50% of that amount</item>
/// <item>Heal all allies for 50 + 100% MAG</item>
/// <item>Heal self for 25% max HP</item>
/// <item>Lifesteal: Heal self for a percent of damage dealt</item>
/// </list>
/// </remarks>
/// <summary>
/// Card and perk effect that heals specified targets and optionally heals the caster (self-heal, lifesteal, etc.).
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Creates healing actions for cards and perks, supporting ally heal, self-heal, percent heal, and lifesteal.</para>
/// <para><strong>How it works:</strong> Calculates healing for each target using flat, magic-scaled, and percent-based formulas. Optionally heals the caster for a percent of the total healed (lifesteal/self-heal).</para>
/// <para><strong>Examples:</strong></para>
/// <list type="bullet">
/// <item>Heal ally for 100 + 50% MAG, heal self for 50% of that amount</item>
/// <item>Heal all allies for 50 + 100% MAG</item>
/// <item>Heal self for 25% max HP</item>
/// <item>Lifesteal: Heal self for a percent of damage dealt</item>
/// </list>
/// <para><strong>Inspector Fields:</strong></para>
/// <list type="bullet">
/// <item><b>baseHeal</b>: Flat healing amount</item>
/// <item><b>MagicAmp</b>: Multiplier for caster's MagicPower</item>
/// <item><b>percentHeal</b>: Percent of target's max HP to heal</item>
/// <item><b>selfHealPercent</b>: Percent of total heal to also heal the caster</item>
/// </list>
/// </remarks>
public class HealEffect : Effects
{

        /// <summary>
        /// Flat healing amount added to the total heal. Set in Inspector.
        /// <br/>Total Heal = baseHeal + (MagicAmp × caster.MagicPower)
        /// </summary>
        [SerializeField] private float baseHeal = 0f;
        /// <summary>
        /// Multiplier for caster's MagicPower. Set in Inspector. Use for magic-based or hybrid heals.
        /// </summary>
        [SerializeField] private float MagicAmp = 1f;
        /// <summary>
        /// Percent heal based on target's max HP (0.25 = 25% max HP heal). Set in Inspector. Use for percent-based heals.
        /// </summary>
        [SerializeField] private float percentHeal = 0f;
        /// <summary>
        /// Percent of the heal amount to also heal the caster (self-heal, lifesteal, etc.). Set in Inspector. Use for lifesteal or self-heal effects.
        /// </summary>
        [SerializeField] private float selfHealPercent = 0f;


        /// <summary>
        /// Creates a healing action for the specified targets and caster.
        /// </summary>
        /// <param name="targets">List of combatants to heal</param>
        /// <param name="caster">Combatant performing the heal (used for magic scaling and self-heal)</param>
        /// <returns>HealGA action containing all healing plans</returns>
        /// <remarks>
        /// Calculates healing for each target using flat, magic, and percent formulas. Optionally heals the caster for a percent of the total healed (lifesteal/self-heal).
        /// </remarks>
        public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
        {
            // Defensive check: Ensure caster is not null
            if (caster == null)
            {
                Debug.LogError($"[HealEffect] Caster is null! Cannot calculate healing. Targets: {(targets != null ? targets.Count.ToString() : "null")}");
                // Return empty heal action with filtered targets
                List<HealTarget> emptyHealTargets = new List<HealTarget>();
                if (targets != null)
                {
                    foreach (var target in targets)
                    {
                        if (target != null) emptyHealTargets.Add(new HealTarget(target, 0));
                    }
                }
                return new HealGA(emptyHealTargets, null);
            }

            // Defensive check: Ensure targets list is valid
            if (targets == null || targets.Count == 0)
            {
                Debug.LogWarning($"[HealEffect] No targets provided for healing. Caster: {caster.name}");
                // Return heal action with only self-heal if applicable
                List<HealTarget> emptyTargetHealList = new List<HealTarget>();
                if (selfHealPercent > 0f)
                {
                    float selfHealAmount = baseHeal * selfHealPercent;
                    emptyTargetHealList.Add(new HealTarget(caster, selfHealAmount));
                }
                return new HealGA(emptyTargetHealList, caster);
            }

            Debug.Log($"[HealEffect] Processing heal effect. Caster: {caster.name} (Magic: {caster.MagicPower}), Base Heal: {baseHeal}, Magic Amp: {MagicAmp}, Targets: {targets.Count}");
            
            // List of healing plans for each target
            List<HealTarget> healTargets = new();
            // Calculate base heal amount (flat + magic scaling)
            float totalHeal = baseHeal + (MagicAmp * caster.MagicPower);
            
            Debug.Log($"[HealEffect] Total base heal calculated: {totalHeal} (Base: {baseHeal} + Magic: {MagicAmp * caster.MagicPower})");

            // Heal each target
            foreach (var target in targets)
            {
                float healAmount = totalHeal;
                // Add percent-based healing if specified
                if (percentHeal > 0f)
                {
                    healAmount += target.MaxHealth * percentHeal;
                }
                healTargets.Add(new HealTarget(target, healAmount));
            }

            // Self-heal logic (lifesteal, heal self for % of total heal)
            if (selfHealPercent > 0f)
            {
                float selfHealAmount = 0f;
                foreach (var ht in healTargets)
                {
                    selfHealAmount += ht.Amount * selfHealPercent;
                }
                healTargets.Add(new HealTarget(caster, selfHealAmount));
            }

            // Return healing action for processing
            return new HealGA(healTargets, caster);
        }
    }

