/*
 * STUN STATUS EFFECT SYSTEM DOCUMENTATION
 * 
 * How it works:
 * This system handles the STUN status effect.
 * When a combatant has STUN stacks, they skip their turn.
 * STUN decrements each turn and is removed when stacks reach 0.
 * 
 * Design reasoning:
 * STUN is a control effect that prevents actions.
 * Unlike RESTING (voluntary skip), STUN is forced by enemies.
 * We check for STUN at turn start and force a skip if present.
 * 
 * Integration:
 * - Attaches performer for ApplyStunGA to apply stun effects
 * - Exposes static methods IsStunned() and DecrementStun() for external callers
 * - Turn manager/combat loop should call IsStunned() at turn start to check if combatant should skip
 * - Turn manager/combat loop should call DecrementStun() after a stunned turn ends
 * - Works with CombatantView's status effect tracking
 * 
 * Used by:
 * - Skyhammer (Bathala): 70% chance to stun 1 player for 1 turn
 * - Thunderous Decree (Bathala): 50% chance to stun 2 players for 1 turn
 * - Radiant Charge (Apolaki): 30% chance to stun 1 enemy
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// System that processes the STUN status effect
/// </summary>
public class StunStatusEffectSystem : MonoBehaviour
{
    private void OnEnable()
    {
        // Register the performer for ApplyStunGA
        ActionSystem.AttachPerformer<ApplyStunGA>(ApplyStunPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyStunGA>();
    }

    /// <summary>
    /// Applies STUN status to all targets
    /// </summary>
    /// <param name="action">The stun action to process</param>
    /// <returns>Coroutine for action processing</returns>
    private IEnumerator ApplyStunPerformer(ApplyStunGA action)
    {
        foreach (var target in action.Targets)
        {
            if (target == null) continue;
            
            // Add STUN stacks equal to duration
            target.AddStatusEffect(StatusEffectType.STUN, action.Duration);
            
            Debug.Log($"[StunSystem] {target.name} is STUNNED for {action.Duration} turn(s)!");
            
            // Play stun VFX if available
            // TODO: Add visual feedback for stun
            
            yield return null;
        }
    }

    /// <summary>
    /// Checks if a combatant is stunned and should skip their turn
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <returns>True if the combatant is stunned and should skip their turn</returns>
    public static bool IsStunned(CombatantView combatant)
    {
        if (combatant == null) return false;
        return combatant.GetStatusEffectStacks(StatusEffectType.STUN) > 0;
    }

    /// <summary>
    /// Decrements STUN stacks at the end of a stunned turn
    /// Should be called after the turn is skipped
    /// </summary>
    /// <param name="combatant">The combatant whose stun should decrement</param>
    public static void DecrementStun(CombatantView combatant)
    {
        if (combatant == null) return;
        
        int currentStacks = combatant.GetStatusEffectStacks(StatusEffectType.STUN);
        if (currentStacks > 0)
        {
            combatant.RemoveStatusEffect(StatusEffectType.STUN, 1);
            Debug.Log($"[StunSystem] {combatant.name} stun reduced to {currentStacks - 1}");
            
            if (currentStacks - 1 <= 0)
            {
                Debug.Log($"[StunSystem] {combatant.name} is no longer stunned!");
            }
        }
    }
}
