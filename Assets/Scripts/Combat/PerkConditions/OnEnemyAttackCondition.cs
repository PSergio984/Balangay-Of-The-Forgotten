using UnityEngine;
using System;

/* ON ENEMY ATTACK CONDITION DOCUMENTATION
 * 
 * How it works:
 * - Triggers perks when enemies attack the player
 * - Subscribes to AttackHeroGA actions to detect enemy attacks
 * - Can add additional conditions like minimum damage thresholds
 * - Uses the timing specified in the base class for when to trigger
 * 
 * Design reasoning:
 * - Specific implementation for attack-based perks (damage reduction, counterattack, etc.)
 * - Separates attack detection from perk effects for flexibility
 * - Comment shows potential for damage-based conditions
 * - Simple implementation allows easy extension for complex attack conditions
 * 
 * Integration: Used in PerkData via SerializeReference, triggers when enemies attack player
 */

/// <summary>
/// Perk condition that triggers when enemies attack the player
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Enables perks that respond to enemy attacks</para>
/// 
/// <para><strong>How it works:</strong> Listens for AttackHeroGA actions (when enemies 
/// attack the player) and triggers the associated perk when those attacks happen. 
/// Can be extended to add requirements like minimum damage amounts.</para>
/// 
/// <para><strong>Examples:</strong></para>
/// <list type="bullet">
/// <item>Damage reduction perks that activate when attacked</item>
/// <item>Counterattack perks that strike back after being hit</item>
/// <item>Healing perks that activate after taking damage</item>
/// <item>Shield or armor perks that trigger on incoming attacks</item>
/// </list>
/// 
/// <para><strong>Used by:</strong> PerkData can store this condition type for attack-reactive perks</para>
/// </remarks>
public class OnEnemyAttackCondition : PerkCondition
{
    /// <summary>
    /// Checks if additional conditions are met beyond just being attacked
    /// </summary>
    /// <param name="gameAction">The attack action that happened</param>
    /// <returns>True if the perk should trigger, false if not</returns>
    /// <remarks>
    /// Currently always returns true, but could be extended to check things like:
    /// - Attack damage amount (only trigger if damage is above X)
    /// - Player health level (only trigger when health is low)
    /// - Attack type or source (only trigger for certain enemy types)
    /// </remarks>
    public override bool SubConditionIsMet(GameAction gameAction)
    {
        // Could add conditions like: if attack damage >= minimumDamage
        // Or if player health <= healthThreshold
        return true;
    }

    /// <summary>
    /// Sets up listening for enemy attack events
    /// </summary>
    /// <param name="reaction">The perk effect to trigger when attacked</param>
    /// <remarks>
    /// Subscribes to AttackHeroGA actions using the timing specified in reactionTiming.
    /// When enemies attack the player, the reaction function will be called.
    /// </remarks>
    public override void SubscribeCondition(Action<GameAction> reaction)
    {
        // Listen for when enemies attack the player
        ActionSystem.SubscribeReaction<AttackHeroGA>(reaction, reactionTiming);
    }

    /// <summary>
    /// Stops listening for enemy attack events
    /// </summary>
    /// <param name="reaction">The same perk effect function that was subscribed</param>
    /// <remarks>
    /// Unsubscribes from AttackHeroGA actions to clean up when perk is removed.
    /// Prevents memory leaks and unwanted perk triggers.
    /// </remarks>
    public override void UnsubscribeCondition(Action<GameAction> reaction)
    {
        // Stop listening for enemy attacks
        ActionSystem.UnsubscribeReaction<AttackHeroGA>(reaction, reactionTiming);
    }
}
