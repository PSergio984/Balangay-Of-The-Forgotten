using System.Collections.Generic;
using UnityEngine;

/* HIT TARGET TRACKER DOCUMENTATION
 * 
 * Purpose: Tracks which combatants were successfully hit by damage in the current move sequence
 * 
 * How it works:
 * - DamageSystem records targets that were hit (not missed) when processing DealDamageGA
 * - Target modes can query this tracker to get only hit targets
 * - Tracker is cleared at the start of each move sequence
 * 
 * Design reasoning:
 * Some moves need to apply effects only to targets that were successfully hit by a previous
 * effect in the same move (e.g., "Each target hit by this attack gets Devoured").
 * This system enables that by tracking hit targets across effects in the same move.
 * 
 * Integration:
 * - Used by DamageSystem to record hit targets
 * - Used by TargetsHitByPreviousEffectTM to query hit targets
 * - Cleared by EnemySystem at the start of each move
 * 
 * Used by:
 * - Lunar Devour: Apply Devoured only to targets hit by the damage
 */

/// <summary>
/// Static tracker for combatants hit by damage in the current move sequence
/// </summary>
public static class HitTargetTracker
{
    /// <summary>
    /// Dictionary mapping each caster to their set of hit targets
    /// This allows multiple enemies to track hits simultaneously without interference
    /// </summary>
    private static Dictionary<CombatantView, HashSet<CombatantView>> hitTargetsByCaster = new Dictionary<CombatantView, HashSet<CombatantView>>();

    /// <summary>
    /// Starts tracking for a new move sequence
    /// </summary>
    /// <param name="caster">The combatant performing the move</param>
    public static void StartMoveSequence(CombatantView caster)
    {
        if (caster == null)
        {
            Debug.LogWarning("[HitTargetTracker] StartMoveSequence called with null caster");
            return;
        }

        // Initialize or clear the hit targets set for this caster
        if (!hitTargetsByCaster.ContainsKey(caster))
        {
            hitTargetsByCaster[caster] = new HashSet<CombatantView>();
        }
        else
        {
            hitTargetsByCaster[caster].Clear();
        }

        Debug.Log($"[HitTargetTracker] Started tracking for {caster.name}");
    }

    /// <summary>
    /// Records a target as being hit by damage
    /// </summary>
    /// <param name="target">The combatant that was hit</param>
    /// <param name="damageAmount">The damage amount (0 = miss, >0 = hit)</param>
    /// <param name="caster">The combatant dealing the damage</param>
    public static void RecordHit(CombatantView target, float damageAmount, CombatantView caster)
    {
        // Only record if:
        // 1. Target is not null
        // 2. Caster is not null
        // 3. Damage > 0 (not a miss)
        // 4. Caster has an active move sequence
        if (target == null || caster == null || damageAmount <= 0f)
        {
            return;
        }

        // Initialize caster's hit set if it doesn't exist (defensive check)
        if (!hitTargetsByCaster.ContainsKey(caster))
        {
            hitTargetsByCaster[caster] = new HashSet<CombatantView>();
        }

        hitTargetsByCaster[caster].Add(target);
        Debug.Log($"[HitTargetTracker] Recorded {target.name} as hit by {caster.name} (damage: {damageAmount})");
    }

    /// <summary>
    /// Gets all combatants that were hit in the specified caster's move sequence
    /// </summary>
    /// <param name="caster">The caster whose hit targets to retrieve</param>
    /// <returns>List of combatants that were successfully hit by this caster</returns>
    public static List<CombatantView> GetHitTargets(CombatantView caster)
    {
        if (caster == null)
        {
            Debug.LogWarning("[HitTargetTracker] GetHitTargets called with null caster");
            return new List<CombatantView>();
        }

        // Get hit targets for this specific caster
        if (!hitTargetsByCaster.ContainsKey(caster))
        {
            Debug.Log($"[HitTargetTracker] No hit targets recorded for {caster.name}");
            return new List<CombatantView>();
        }

        // Filter out null/destroyed targets
        var validTargets = new List<CombatantView>();
        foreach (var target in hitTargetsByCaster[caster])
        {
            if (target != null && target)
            {
                validTargets.Add(target);
            }
        }

        return validTargets;
    }

    /// <summary>
    /// Clears the tracker for a specific caster (called at end of move sequence)
    /// </summary>
    /// <param name="caster">The caster whose tracking should be cleared</param>
    public static void Clear(CombatantView caster)
    {
        if (caster != null && hitTargetsByCaster.ContainsKey(caster))
        {
            hitTargetsByCaster[caster].Clear();
        }
    }

    /// <summary>
    /// Clears all trackers (called at end of all enemy moves)
    /// </summary>
    public static void ClearAll()
    {
        hitTargetsByCaster.Clear();
    }

    /// <summary>
    /// Gets the count of hit targets for a specific caster
    /// </summary>
    /// <param name="caster">The caster to check</param>
    /// <returns>Number of valid hit targets</returns>
    public static int GetHitTargetCount(CombatantView caster)
    {
        if (caster == null || !hitTargetsByCaster.ContainsKey(caster))
        {
            return 0;
        }

        int count = 0;
        foreach (var target in hitTargetsByCaster[caster])
        {
            if (target != null && target) count++;
        }
        return count;
    }
}
