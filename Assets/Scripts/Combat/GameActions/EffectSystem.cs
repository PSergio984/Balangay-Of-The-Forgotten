using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/* EFFECT SYSTEM DOCUMENTATION
 * 
 * How it works:
 * - When a card is played or perk triggers, effects are converted into PerformEffectGA actions
 * - This system processes those actions and converts them into concrete game actions
 * - Passes caster information so effects know who triggered them
 * - Examples: Damage effects become DealDamageGA, heal effects become HealGA, etc.
 * 
 * Design reasoning:
 * - Separates effect definition from effect execution for cleaner code
 * - Works for both card effects and perk effects using the same processing
 * - Caster tracking enables perk system to know who caused what
 * - Single system handles all effect types consistently
 * 
 * Integration: Works with ActionSystem, supports both card system and perk system
 */
/// <summary>
/// Runs card and perk effects when they are triggered in the game
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Changes effects into actions that the game can understand</para>
/// 
/// <para><strong>What it does:</strong> This system takes effects from cards and perks 
/// (like damage, healing, buffs) and turns them into game actions. When you play a card 
/// or when a perk triggers, their effects get sent here to be changed into actions 
/// like DealDamageGA or HealGA that other parts of the game can use.</para>
/// 
/// <para><strong>Perk system integration:</strong> Works exactly the same for perk effects 
/// as for card effects. When a perk triggers, its effect goes through this same system. 
/// The caster information gets passed through so the resulting actions know who 
/// triggered them originally.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Player plays a card or perk condition is met</item>
/// <item>Effects get wrapped in PerformEffectGA actions with target and caster info</item>
/// <item>This system gets those actions from ActionSystem</item>
/// <item>Effects get changed into real actions (damage becomes DealDamageGA, etc.)</item>
/// <item>Real actions get sent back to ActionSystem to happen</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> ActionSystem, card system, perk system - handles all effect processing</para>
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
    /// Takes effect actions from cards and perks and turns them into real game actions
    /// </summary>
    /// <param name="performEffectGA">The effect action that has the effect to run</param>
    /// <returns>IEnumerator for running as a coroutine, waits one frame after finishing</returns>
    /// <remarks>
    /// This is the main method that changes effects into actions. It takes effects from
    /// both cards and perks and turns them into specific GameActions that other systems can use.
    /// Passes along the caster information so the resulting actions know who triggered them.
    /// Runs as a coroutine to work properly with the action system timing.
    /// </remarks>
    private IEnumerator PerformEffectPerformer(PerformEffectGA performEffectGA)
    {
        // Defensive check: Ensure PerformEffectGA is valid
        if (performEffectGA == null)
        {
            Debug.LogError("[EffectSystem] PerformEffectGA is null! Cannot process effect.");
            yield return null;
            yield break;
        }

        // Defensive check: Ensure Effect is not null
        if (performEffectGA.Effect == null)
        {
            Debug.LogError($"[EffectSystem] PerformEffectGA has null Effect! Cannot process. Targets: {(performEffectGA.Targets != null ? performEffectGA.Targets.Count.ToString() : "null")}");
            yield return null;
            yield break;
        }

        // Get the specific game action that this effect should perform (damage, heal, etc.)
        // Pass the targets and caster info so the effect knows who is involved
        CombatantView caster = CurrentHeroUtil.GetCurrentHero();
        
        // Validate caster - GetCurrentHero() can return null when no heroes exist or index is out of bounds
        if (caster == null)
        {
            Debug.LogWarning($"[EffectSystem] GetCurrentHero() returned null while processing effect {performEffectGA.Effect.GetType().Name}. " +
                           $"This may occur when no heroes exist or current hero index is invalid. Effect will proceed with null caster.");
        }
        
        GameAction effectAction = null;
        bool hasError = false;
        try
        {
            effectAction = performEffectGA.Effect.GetGameAction(performEffectGA.Targets, caster);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[EffectSystem] Exception while processing effect {performEffectGA.Effect.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            hasError = true;
        }
        
        // If an error occurred, exit early (cannot yield in catch block, so we check flag here)
        if (hasError)
        {
            yield return null;
            yield break;
        }
        
        // Only add the action if it's not null (some effects like ConditionalEffect return null when condition isn't met)
        if (effectAction != null)
        {
            // Add the effect's action to the action queue to be processed by other systems
            ActionSystem.Instance.AddReaction(effectAction);
        }
        else
        {
            Debug.Log($"[EffectSystem] Effect {performEffectGA.Effect.GetType().Name} returned null action (condition not met or no valid targets)");
        }
        
        // Wait one frame before continuing to ensure proper coroutine execution flow
        yield return null;
    }
}
