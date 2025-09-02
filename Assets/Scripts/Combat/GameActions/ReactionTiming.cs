using UnityEngine;

/* REACTION TIMING DOCUMENTATION
 * 
 * Purpose: Defines when triggered reactions should happen during action processing
 * 
 * How it works:
 * - PRE means reactions happen before the main action
 * - POST means reactions happen after the main action
 * - Used by ActionSystem to organize reaction processing order
 * - Essential for proper timing of triggered effects and abilities
 * 
 * Integration: Used by ActionSystem for reaction timing, passive abilities, triggered effects
 */

/// <summary>
/// Enum to specify when a reaction should trigger during an action
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Controls the timing of when reactions and triggered effects happen</para>
/// 
/// <para><strong>What it does:</strong> This enum defines whether a reaction should happen 
/// before (PRE) or after (POST) the main action. This is crucial for game mechanics because 
/// the order of effects matters. For example, gaining energy before playing a card vs. 
/// drawing a card after dealing damage - the timing changes the gameplay significantly.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>PRE reactions happen in the preparation phase before the main action</item>
/// <item>POST reactions happen in the cleanup phase after the main action</item>
/// <item>ActionSystem uses this to organize when each reaction triggers</item>
/// <item>Ensures predictable and consistent effect ordering</item>
/// </list>
/// 
/// <para><strong>Examples:</strong></para>
/// <list type="bullet">
/// <item>PRE: "Whenever you play a card, gain 1 energy first"</item>
/// <item>POST: "Whenever you deal damage, draw a card afterwards"</item>
/// <item>PRE: "Before attacking, gain 2 attack power"</item>
/// <item>POST: "After healing, restore 1 stamina"</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> ActionSystem for reaction processing, passive abilities, triggered effects</para>
/// 
/// <para><strong>How to use:</strong> Specify when registering reactions with ActionSystem.SubscribeReaction()</para>
/// </remarks>
// Enum to specify when a reaction should trigger during an action
// Pre = before the main action, Post = after the main action
public enum ReactionTiming
{
    /// <summary>
    /// Happens before the main action executes
    /// </summary>
    /// <remarks>
    /// Use this for effects that should happen first, before the main action.
    /// Example: "whenever you play a card, gain 1 energy" - the energy gain 
    /// should happen before the card effect so you can use that energy.
    /// </remarks>
    PRE,  // Happens before the main action (like "whenever you play a card, gain 1 energy")
    
    /// <summary>
    /// Happens after the main action completes
    /// </summary>
    /// <remarks>
    /// Use this for effects that should happen as a result of the main action.
    /// Example: "whenever you deal damage, draw a card" - the card draw should 
    /// happen after the damage is dealt, as a consequence of the damage.
    /// </remarks>
    POST  // Happens after the main action (like "whenever you deal damage, draw a card")
}

