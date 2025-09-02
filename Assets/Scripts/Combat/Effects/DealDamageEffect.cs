using System.Collections.Generic;
using UnityEngine;

/* DEAL DAMAGE EFFECT DOCUMENTATION
 * 
 * Purpose: Card effect that deals damage to all enemies
 * 
 * How it works:
 * - Contains a damage amount that can be set in Inspector
 * - Targets all enemies currently on the battlefield
 * - Creates a DealDamageGA action when the card is played
 * - Part of the card effects system for damage-dealing cards
 * 
 * Integration: Inherits from Effects base class, works with card system and enemy targeting
 */

/// <summary>
/// Card effect that deals damage to all enemies on the battlefield
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Creates damage actions that hurt all enemies when card is played</para>
/// 
/// <para><strong>What it does:</strong> This effect is used on cards that deal damage to enemies. 
/// When a card with this effect is played, it creates a damage action that targets all 
/// enemies currently on the battlefield. The damage amount can be set in the Inspector 
/// to match the card's intended power level.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Player plays a card that has this effect attached</item>
/// <item>Card system calls GetGameAction() to get the effect</item>
/// <item>This method gets list of all current enemies from EnemySystem</item>
/// <item>Creates a DealDamageGA action with damage amount and enemy targets</item>
/// <item>Returns the action to be processed by ActionSystem</item>
/// </list>
/// 
/// <para><strong>Examples:</strong></para>
/// <list type="bullet">
/// <item>"Fireball" card - deals 5 damage to all enemies</item>
/// <item>"Lightning Storm" card - deals 3 damage to all enemies</item>
/// <item>"Meteor" card - deals 8 damage to all enemies</item>
/// <item>Any area-of-effect damage spell</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> Effects base class, EnemySystem for targeting, DealDamageGA for damage</para>
/// 
/// <para><strong>How to use:</strong> Attach to card prefabs, set damage amount in Inspector</para>
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
    /// Creates a damage action that targets all enemies with the specified damage amount
    /// </summary>
    /// <returns>DealDamageGA action that will hurt all current enemies</returns>
    /// <remarks>
    /// This method is called by the card system when the card effect should be executed.
    /// Gets all enemies currently on the battlefield and creates a damage action that 
    /// will hurt each of them with the specified damage amount.
    /// </remarks>
    public override GameAction GetGameAction()
    {
        // Get all enemies currently on the battlefield as damage targets
        List<CombatantView> targets = new(EnemySystem.Instance.EnemyViews);
        // Create a damage action with the damage amount and all enemies as targets
        DealDamageGA dealDamageGA = new(damageAmount, targets);
        // Return the damage action to be processed
        return dealDamageGA;
    }
}
