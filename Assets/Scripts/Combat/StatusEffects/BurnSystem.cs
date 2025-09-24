/*
 * BURN SYSTEM DOCUMENTATION
 * 
 * How it works:
 * This system processes ApplyBurnGA actions to create the burn damage effect. When
 * a combatant takes burn damage, this system handles the visual effects, applies
 * the damage, and reduces burn stacks by 1. Creates a complete burn damage experience
 * with fire effects and proper stack management.
 * 
 * Design reasoning:
 * We separate burn processing from the turn system to keep effects focused. This
 * system only handles burn damage execution while other systems handle when to
 * apply it. The visual effects make burn feel impactful and different from regular
 * damage. Auto-reducing burn stacks creates the "ticking down" effect.
 * 
 * Integration:
 * - Processes ApplyBurnGA actions created by HeroSystem turn reactions
 * - Creates visual burn effects at target location
 * - Applies damage through the standard damage system
 * - Automatically reduces burn stacks after applying damage
 * 
 * How to use:
 * - Attach to a GameObject in the scene
 * - Assign a burn VFX prefab in the inspector
 * - System automatically handles all burn actions
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// System that processes burn damage effects with visuals and stack management
/// </summary>
public class BurnSystem : MonoBehaviour
{
    /// <summary>
    /// Visual effect prefab that plays when burn damage is applied
    /// </summary>
    /// <remarks>
    /// This should be a particle system or animated object that represents fire/burn.
    /// Gets instantiated at the target's position when burn damage happens.
    /// Assign a fire effect prefab in the Inspector.
    /// </remarks>
    [SerializeField] private GameObject burnVFX;

    /// <summary>
    /// Subscribe to handle ApplyBurnGA actions when this system starts
    /// </summary>
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ApplyBurnGA>(ApplyBurnPerformer);
    }

    /// <summary>
    /// Unsubscribe from actions when this system stops to prevent errors
    /// </summary>
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyBurnGA>();
    }

    /// <summary>
    /// Processes a burn damage action with visual effects and damage application
    /// </summary>
    /// <param name="applyBurnGA">The action containing burn damage amount and target</param>
    /// <returns>Coroutine that applies burn damage with timing and effects</returns>
    /// <remarks>
    /// This method creates the complete burn damage experience:
    /// 1. Spawns fire visual effects at target location
    /// 2. Creates a DealDamageGA action for proper damage processing (including armor)
    /// 3. Reduces burn stacks by 1 (burn "ticks down")
    /// 4. Waits for effects to finish before continuing
    /// 
    /// IMPROVEMENT: Now uses DealDamageGA instead of direct damage to ensure
    /// proper armor calculation and death handling through the standard damage system.
    /// The damage equals the burn stacks, so more stacks = more damage per turn.
    /// After taking damage, burn stacks automatically reduce by 1.
    /// </remarks>
    private IEnumerator ApplyBurnPerformer(ApplyBurnGA applyBurnGA)
    {
        // Get the target that will take burn damage
        CombatantView target = applyBurnGA.Target;
        // Create fire visual effects at the target's position
        Instantiate(burnVFX, target.transform.position, Quaternion.identity);
        
        // Create a proper damage action instead of direct damage
        // This ensures armor calculation and death handling work correctly
        List<CombatantView> burnTargets = new() { target };
        DealDamageGA burnDamageGA = new(applyBurnGA.BurnDamage, burnTargets, null);
        ActionSystem.Instance.AddReaction(burnDamageGA);
        
        // Reduce burn stacks by 1 (burn effect "ticks down")
        target.RemoveStatusEffect(StatusEffectType.BURN, 1);
        // Wait for visual effects to finish before continuing
        yield return new WaitForSeconds(1f);
    }
}
