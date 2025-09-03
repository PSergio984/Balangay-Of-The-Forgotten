using UnityEngine;
using System;

/* PERK CONDITION DOCUMENTATION
 * 
 * How it works:
 * - Abstract base class that defines when perks should trigger
 * - Child classes implement specific trigger conditions (enemy attacks, card plays, etc.)
 * - Handles subscribing to and unsubscribing from game events
 * - Provides additional condition checking beyond just the event type
 * 
 * Design reasoning:
 * - Abstract design allows many different trigger types without changing perk system
 * - Separates "when to listen" (subscription) from "should it actually trigger" (condition check)
 * - ReactionTiming field lets conditions choose when during an action they should respond
 * - Each condition manages its own event subscriptions for clean separation
 * 
 * Integration: Used by PerkData, extended by specific condition classes like OnEnemyAttackCondition
 */

/// <summary>
/// Base class that defines when a perk should trigger and how it listens for game events
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Provides the framework for perk triggering conditions</para>
/// 
/// <para><strong>How it works:</strong> Child classes inherit from this to define specific 
/// trigger conditions. Each condition type handles its own event subscription (like listening 
/// for enemy attacks) and can add extra requirements beyond just the event happening.</para>
/// 
/// <para><strong>Why abstract:</strong> Different perks need different triggers - some activate 
/// on enemy attacks, others on card plays, others on health changes. This base class lets 
/// the perk system work with any trigger type without knowing the specifics.</para>
/// 
/// <para><strong>Used by:</strong> PerkData stores one of these, perk system calls the methods</para>
/// </remarks>
public abstract class PerkCondition 
{
    /// <summary>
    /// When during an action this condition should be checked
    /// </summary>
    /// <remarks>
    /// Controls whether the perk triggers before the action happens (PRE) or after (POST).
    /// For example, damage reduction might need PRE timing, while healing might use POST.
    /// </remarks>
    [SerializeField] protected ReactionTiming reactionTiming;
    
    /// <summary>
    /// Sets up event listening for this condition type
    /// </summary>
    /// <param name="reaction">The perk effect function to call when condition is met</param>
    /// <remarks>
    /// Each child class implements this to subscribe to the specific game events it cares about.
    /// When those events happen, the reaction function gets called to trigger the perk effect.
    /// </remarks>
    public abstract void SubscribeCondition(Action<GameAction> reaction);
    
    /// <summary>
    /// Stops listening for events when perk is removed or disabled
    /// </summary>
    /// <param name="reaction">The same perk effect function that was subscribed</param>
    /// <remarks>
    /// Cleans up event subscriptions to prevent memory leaks and unwanted triggers.
    /// Must match exactly with what was passed to SubscribeCondition.
    /// </remarks>
    public abstract void UnsubscribeCondition(Action<GameAction> reaction);
    
    /// <summary>
    /// Additional check to see if the perk should actually trigger
    /// </summary>
    /// <param name="gameAction">The action that happened</param>
    /// <returns>True if the perk should trigger, false if not</returns>
    /// <remarks>
    /// Allows conditions to have extra requirements beyond just the event type.
    /// For example, "only when attack damage is above 5" or "only when health is low".
    /// Returns true by default if no extra conditions are needed.
    /// </remarks>
    public abstract bool SubConditionIsMet(GameAction gameAction);
}
