/*
 * STATUS EFFECT SYSTEM DOCUMENTATION
 * 
 * How it works:
 * This system processes AddStatusEffectGA actions and actually applies the status
 * effects to combatants. When cards or perks want to give someone armor or burn,
 * they create an action and this system handles the execution. Simple processor
 * that bridges between actions and combatant status effect changes.
 * 
 * Design reasoning:
 * We separate the action creation from the execution to keep things clean. Cards
 * and effects just need to create actions - they don't need to know how to apply
 * status effects. This system handles all the actual application logic and can
 * add visual effects or other processing as needed.
 * 
 * Integration:
 * - Listens for AddStatusEffectGA actions from the ActionSystem
 * - Calls AddStatusEffect on target combatants to apply effects
 * - Can be extended to add visual effects when status effects are applied
 */

using System.Collections;
using UnityEngine;

/// <summary>
/// System that processes status effect actions and applies them to combatants
/// </summary>
public class StatusEffectSystem : MonoBehaviour
{
    /// <summary>
    /// Subscribe to handle AddStatusEffectGA actions when this system starts
    /// </summary>
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AddStatusEffectGA>(AddStatusEffectPerformer);
    }

    /// <summary>
    /// Unsubscribe from actions when this system stops to prevent errors
    /// </summary>
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AddStatusEffectGA>();
    }

    /// <summary>
    /// Processes an AddStatusEffectGA action by applying the effect to all targets
    /// </summary>
    /// <param name="addStatusEffectGA">The action containing effect type, stacks, and targets</param>
    /// <returns>Coroutine that applies effects to each target</returns>
    /// <remarks>
    /// Goes through each target and adds the specified status effect with the given
    /// stack count. Currently has a placeholder for adding visual effects when
    /// status effects are applied - this could show particles or animations.
    /// </remarks>
    private IEnumerator AddStatusEffectPerformer(AddStatusEffectGA addStatusEffectGA)
    {
        // Apply the status effect to each target in the list
        foreach (var target in addStatusEffectGA.Targets)
        {
            // Add the specified effect with the specified stack count
            target.AddStatusEffect(addStatusEffectGA.StatusEffectType, addStatusEffectGA.StackCount);
            // Placeholder for visual effects - could add particles, sounds, etc.
            yield return null; // ADD VFX for adding status effects
        }
    }
}
