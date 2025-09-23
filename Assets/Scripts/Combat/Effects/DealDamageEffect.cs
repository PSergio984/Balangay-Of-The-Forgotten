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
   /// How much damage this effect deals to each target
   /// </summary>
   /// <remarks>
   /// The amount of damage that will be dealt to all enemies when this effect triggers.
   /// Set this value in the Inspector to match the card's intended power.
   /// </remarks>
   [SerializeField] private int damageAmount;

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
    /// </remarks>
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        // Example values for demonstration; replace with actual stat retrieval in your game
        float dmgAmp = 1.0f; // e.g., 1 + sum of all %DMG amplifications
        // Set coefficient dynamically: 1 for HeroView (Player→Boss), 1.5 for EnemyView (Boss→Player)
        float coefficient = 1.0f;
        if (caster is EnemyView)
            coefficient = 1.5f;
        else if (caster is HeroView)
            coefficient = 1.0f;
        float critMultiplier = 1.0f;
        float accuracy = 1.0f; // 0-1, e.g., 0.75 for 75% accuracy
        float critChance = 0.0f; // 0-1, e.g., 0.25 for 25% crit chance


        bool hit = DamageCalculator.CalculateAccuracy(accuracy);
        int finalDamage = 0;
        if (!hit)
        {
            // Missed attack, all targets take 0 damage
            DealDamageGA missGA = new(0, targets, caster);
            return missGA;
        }

        // Crit check ONCE for the whole attack
        bool isCrit = DamageCalculator.CalculateCrit(critChance);
        // Set crit multiplier based on crit and context
        if (isCrit)
        {
            if (caster is HeroView)
                critMultiplier = 1.5f;
            else if (caster is EnemyView)
                critMultiplier = 1.2f;
            else
                critMultiplier = 1.0f;
        }
        else
        {
            critMultiplier = 1.0f;
        }

        // For simplicity, use the first target's defense for group attacks (customize as needed)
        float defTarget = 0f;
        float defIncrease = 0f;
        float defDecreased = 0f;
        float defIgnored = 0f;
        if (targets.Count > 0)
         {
             // Try to get defense from the first target
             var firstTarget = targets[0];
             // If CombatantView has a Defense property, use it directly
             // Otherwise, check for HeroView or EnemyView and get their defense
             if (firstTarget is HeroView hero)
             {
                 defTarget = hero.Defense; // Assumes HeroView has Defense property
             }
             else if (firstTarget is EnemyView enemy)
             {
                 defTarget = enemy.Defense; // Assumes EnemyView has Defense property
             }
             else
             {
                 // If CombatantView has Defense, use reflection as fallback (not recommended for perf)
                 var defProp = firstTarget.GetType().GetProperty("Defense");
                 if (defProp != null)
                 {
                     defTarget = (float)System.Convert.ChangeType(defProp.GetValue(firstTarget), typeof(float));
                 }
             }
         }
        float defFinal = DamageCalculator.CalculateFinalDefense(defTarget, defIncrease, defDecreased, defIgnored);
        finalDamage = DamageCalculator.CalculateDamage(damageAmount, dmgAmp, coefficient, defFinal, critMultiplier);

        // All targets take the same damage if hit
        DealDamageGA dealDamageGA = new(finalDamage, targets, caster);
        return dealDamageGA;
    }
}
