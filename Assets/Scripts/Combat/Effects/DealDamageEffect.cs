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
    public override GameAction GetGameAction(List<CombatantView> targets,CombatantView caster)
    {

        // Create a damage action with the damage amount, specified targets, and caster info
        DealDamageGA dealDamageGA = new(damageAmount, targets,caster);
        // Return the damage action to be processed
        return dealDamageGA;
    }
}
