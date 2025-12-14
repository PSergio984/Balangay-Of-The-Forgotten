using System.Collections.Generic;
using UnityEngine;

/* TARGETS HIT BY PREVIOUS EFFECT TARGET MODE DOCUMENTATION
 * 
 * Purpose: Target mode that selects only combatants that were successfully hit by a previous
 *          damage effect in the same move sequence
 * 
 * How it works:
 * - Queries HitTargetTracker to get combatants hit by previous damage
 * - Only returns targets that were actually hit (not missed)
 * - Works within the same move sequence (effects processed in order)
 * 
 * Design reasoning:
 * Some moves need conditional targeting: "Apply effect only to targets hit by previous attack"
 * This enables moves like Lunar Devour where Devoured is applied only to hit targets.
 * 
 * Integration:
 * - Uses HitTargetTracker to get hit targets
 * - Requires caster to be set (passed from EnemySystem)
 * - Works with EnemySystem move sequence tracking
 * 
 * Used by:
 * - Lunar Devour: Apply Devoured only to heroes hit by the 80% MAG damage
 */

/// <summary>
/// Target mode that selects only combatants hit by a previous damage effect in the same move
/// </summary>
[System.Serializable]
public class TargetsHitByPreviousEffectTM : TargetMode
{
    /// <summary>
    /// The caster of the move (used to verify we're in the same move sequence)
    /// This is set by EnemySystem when creating the PerformEffectGA
    /// </summary>
    private CombatantView moveCaster;

    /// <summary>
    /// Sets the caster for this move sequence
    /// </summary>
    /// <param name="caster">The combatant performing the move</param>
    public void SetCaster(CombatantView caster)
    {
        moveCaster = caster;
    }

    /// <summary>
    /// Returns only combatants that were successfully hit by previous damage in this move
    /// </summary>
    /// <returns>List of combatants hit by previous damage effect, or empty list if none</returns>
    /// <remarks>
    /// This method queries HitTargetTracker to get targets that were hit (not missed)
    /// by a previous DealDamageGA in the same move sequence. If no targets were hit,
    /// or if we're not in a valid move sequence, returns an empty list.
    /// </remarks>
    public override List<CombatantView> GetTargets()
    {
        if (moveCaster == null)
        {
            Debug.LogWarning("[TargetsHitByPreviousEffectTM] Move caster is null - cannot determine hit targets. Returning empty list.");
            return new List<CombatantView>();
        }

        var hitTargets = HitTargetTracker.GetHitTargets(moveCaster);
        
        if (hitTargets.Count == 0)
        {
            Debug.Log($"[TargetsHitByPreviousEffectTM] No targets were hit by previous effect in this move sequence");
        }
        else
        {
            Debug.Log($"[TargetsHitByPreviousEffectTM] Found {hitTargets.Count} target(s) hit by previous effect");
        }

        return hitTargets;
    }
}
