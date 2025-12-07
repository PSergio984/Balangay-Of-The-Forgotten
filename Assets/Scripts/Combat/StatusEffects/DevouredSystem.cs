/*
 * DEVOURED STATUS EFFECT SYSTEM DOCUMENTATION
 * 
 * How it works:
 * This system handles DEVOURED status effect - a damage-over-time (DoT) debuff.
 * Integrated with StatusEffectTickSystem to apply damage at turn end.
 * At the end of each turn, affected combatants take fixed damage equal to stack count.
 * The status effect duration decreases by 1 each turn until it expires.
 * 
 * Design reasoning:
 * DEVOURED represents a persistent draining effect that deals fixed damage per turn.
 * Unlike percentage-based effects, this deals flat damage based on stacks.
 * Stacks represent both damage per turn AND duration (30 stacks = 30 damage per turn for 1 turn).
 * Integrated with StatusEffectTickSystem for centralized DoT management.
 * 
 * Integration:
 * - Called by StatusEffectTickSystem.TickStatusEffects() at turn end
 * - Deals damage through ActionSystem for proper chaining
 * - Automatically decreases stacks each turn
 * - Works with existing status effect tracking system
 * 
 * Setup:
 * 1. Add DEVOURED ticking logic to StatusEffectTickSystem.TickStatusEffects()
 * 2. This system provides utility methods for checking DEVOURED state
 * 
 * Used by:
 * - Draining/vampiric abilities
 * - Life steal effects
 * - Persistent damage auras
 */

using UnityEngine;

/// <summary>
/// System that handles DEVOURED status effect (DoT damage)
/// </summary>
/// <remarks>
/// DEVOURED deals fixed damage at the end of each turn.
/// Stacks represent damage per turn (30 stacks = 30 damage).
/// Duration automatically decreases each turn.
/// This system is integrated with StatusEffectTickSystem for centralized DoT processing.
/// </remarks>
public class DevouredSystem : MonoBehaviour
{
    // NOTE: DEVOURED ticking is handled by StatusEffectTickSystem.
    // Add this to StatusEffectTickSystem.TickStatusEffects():
    //
    // // DEVOURED: apply DoT damage and reduce stack
    // int devouredStacks = combatant.GetStatusEffectStacks(StatusEffectType.DEVOURED);
    // if (devouredStacks > 0)
    // {
    //     DealDamageGA dotDamage = new DealDamageGA(devouredStacks, new List<CombatantView> { combatant }, null);
    //     ActionSystem.Instance.AddReaction(dotDamage);
    //     combatant.RemoveStatusEffect(StatusEffectType.DEVOURED, 1);
    // }

    /// <summary>
    /// Calculates total damage a combatant will take from DEVOURED over remaining duration
    /// </summary>
    /// <param name="combatant">The combatant with DEVOURED</param>
    /// <returns>Total remaining DoT damage</returns>
    public static int GetRemainingDamage(CombatantView combatant)
    {
        if (combatant == null) return 0;
        
        int devouredStacks = combatant.GetStatusEffectStacks(StatusEffectType.DEVOURED);
        
        // Current implementation: stacks = damage per turn, duration = 1 turn
        // Total damage = current stacks (will be dealt next turn start)
        return devouredStacks;
    }

    /// <summary>
    /// Checks if a combatant has the DEVOURED debuff
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <returns>True if DEVOURED is active</returns>
    public static bool IsDevoured(CombatantView combatant)
    {
        if (combatant == null) return false;
        return combatant.GetStatusEffectStacks(StatusEffectType.DEVOURED) > 0;
    }
}
