using UnityEngine;

/* PERFORM EFFECT GAME ACTION DOCUMENTATION
 * 
 * Purpose: Wraps card effects so they can be processed by the action system
 * 
 * How it works:
 * - When a card is played, its effects get wrapped in this action
 * - EffectSystem processes this action and converts effects to real game actions
 * - Acts as a bridge between card effects and the action system
 * 
 * Integration: Works with EffectSystem and ActionSystem to run card effects
 */

/// <summary>
/// Game action that holds a card effect to be processed and executed
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Wraps card effects so the action system can handle them</para>
/// 
/// <para><strong>What it does:</strong> This action carries card effects (like damage, 
/// heal, draw cards) from when a card is played to the EffectSystem. It's like 
/// a package that holds the effect until it can be opened and turned into 
/// a real game action.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Card gets played and its effects are identified</item>
/// <item>Each effect gets wrapped in a PerformEffectGA action</item>
/// <item>EffectSystem receives these actions</item>
/// <item>EffectSystem unwraps the effects and turns them into real actions</item>
/// <item>Real actions get sent back to ActionSystem to happen</item>
/// </list>
/// 
/// <para><strong>Needs:</strong> EffectSystem must be in the scene to process this action</para>
/// 
/// <para><strong>Works with:</strong> EffectSystem to convert effects, ActionSystem for processing</para>
/// 
/// <para><strong>How to use:</strong> Create this action with a card effect when a card is played</para>
/// </remarks>
public class PerformEffectGA : GameAction
{
    /// <summary>
    /// The card effect that needs to be processed and executed
    /// </summary>
    /// <remarks>
    /// This property holds the actual effect from a card (damage, heal, etc.).
    /// The EffectSystem will take this effect and convert it into a specific game action.
    /// </remarks>
    public Effects Effect { get; set; }

    /// <summary>
    /// Creates a new effect performance action
    /// </summary>
    /// <param name="effect">The card effect to be processed</param>
    /// <remarks>
    /// Creates an action that wraps a card effect for processing by the EffectSystem.
    /// The effect will be converted into appropriate game actions when processed.
    /// </remarks>
    public PerformEffectGA(Effects effect)
    {
        // Store the effect that needs to be processed
        Effect = effect;
    }
}
