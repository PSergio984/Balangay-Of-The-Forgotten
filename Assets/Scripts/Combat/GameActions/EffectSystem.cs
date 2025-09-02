using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* EFFECT SYSTEM DOCUMENTATION
 * 
 * Purpose: Handles the execution of card effects and abilities in the game
 * 
 * How it works:
 * - When a card is played, its effects are converted into PerformEffectGA actions
 * - This system processes those actions and converts them into concrete game actions
 * - Examples: Damage effects become DealDamageGA, heal effects become HealGA, etc.
 * 
 * Integration: Works with ActionSystem to queue and execute effect-based actions
 */
/// <summary>
/// Runs card effects when cards are played in the game
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Changes card effects into actions that the game can understand</para>
/// 
/// <para><strong>What it does:</strong> This system takes card effects 
/// (like damage, healing, buffs) and turns them into game actions. When 
/// you play a card, its effects get sent here to be changed into actions 
/// like DealDamageGA or HealGA that other parts of the game can use.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Player plays a card</item>
/// <item>Card effects get wrapped in PerformEffectGA actions</item>
/// <item>This system gets those actions from ActionSystem</item>
/// <item>Effects get changed into real actions (damage becomes DealDamageGA, heal becomes HealGA)</item>
/// <item>Real actions get sent back to ActionSystem to happen</item>
/// </list>
/// 
/// <para><strong>Needs:</strong> ActionSystem must be in the scene to work</para>
/// 
/// <para><strong>Works with:</strong> ActionSystem to keep card effects separate from how they work</para>
/// 
/// <para><strong>How to use:</strong> Put this script on any GameObject in the scene. It will set itself up automatically</para>
/// </remarks>
public class EffectSystem : MonoBehaviour
{
    /// <summary>
    /// Sets up this system when the GameObject turns on
    /// </summary>
    /// <remarks>
    /// Unity calls this automatically when the GameObject goes from off to on.
    /// This makes sure the system is ready to handle effect actions right away.
    /// </remarks>
    private void OnEnable()
    {
        // Register this system to handle PerformEffectGA actions when they occur
        ActionSystem.AttachPerformer<PerformEffectGA>(PerformEffectPerformer);
    }

    /// <summary>
    /// Cleans up this system when the GameObject turns off
    /// </summary>
    /// <remarks>
    /// Unity calls this automatically when the GameObject goes from on to off.
    /// This stops memory leaks and makes sure the system shuts down cleanly.
    /// </remarks>
    void OnDisable()
    {
        // Unregister the effect handling method to prevent memory leaks
        ActionSystem.DetachPerformer<PerformEffectGA>();
    }
    
    /// <summary>
    /// Takes effect actions from cards and turns them into real game actions
    /// </summary>
    /// <param name="performEffectGA">The effect action that has the card effect to run</param>
    /// <returns>IEnumerator for running as a coroutine, waits one frame after finishing</returns>
    /// <remarks>
    /// This is the main method that changes effects into actions. It takes card
    /// effects and turns them into specific GameActions that other systems can use.
    /// Runs as a coroutine to work properly with the action system timing.
    /// </remarks>
    private IEnumerator PerformEffectPerformer(PerformEffectGA performEffectGA)
    {
        // Get the specific game action that this effect should perform (damage, heal, etc.)
        GameAction effectAction = performEffectGA.Effect.GetGameAction();
        // Add the effect's action to the action queue to be processed by other systems
        ActionSystem.Instance.AddReaction(effectAction);
        // Wait one frame before continuing to ensure proper coroutine execution flow
        yield return null;
    }
}
