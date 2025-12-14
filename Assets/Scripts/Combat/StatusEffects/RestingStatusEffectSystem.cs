/*
 * RESTING STATUS EFFECT SYSTEM DOCUMENTATION
 * 
 * How it works:
 * This system handles the RESTING and CHARGING status effects.
 * When a combatant has these statuses, they skip their turn.
 * CHARGING also provides a damage multiplier for the next attack.
 * 
 * Design reasoning:
 * RESTING is a voluntary skip (cost of powerful abilities).
 * CHARGING is a skip that buffs the next attack (Shadow Dive).
 * We track the charging multiplier separately from status stacks.
 * 
 * Integration:
 * - Attaches performer for SkipTurnGA
 * - Tracks charging multiplier for damage boost
 * - Works with turn system to enforce skipping
 * 
 * Used by:
 * - Celestial Judgement (Bathala): Rest 1 turn after
 * - Sunburst Nova (Apolaki): Skip 1 turn after
 * - Shadow Dive (Bakunawa): Skip 1 turn, next attack deals 2x damage
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// System that processes RESTING and CHARGING status effects
/// </summary>
public class RestingStatusEffectSystem : MonoBehaviour
{
    /// <summary>
    /// Tracks charging multiplier per combatant
    /// </summary>
    private static Dictionary<CombatantView, float> chargingMultipliers = new Dictionary<CombatantView, float>();

    private void OnEnable()
    {
        // Register the performer for SkipTurnGA
        ActionSystem.AttachPerformer<SkipTurnGA>(SkipTurnPerformer);
        
        // Subscribe to damage actions to apply charging bonus
        ActionSystem.SubscribeReaction<DealDamageGA>(OnDealDamage, ReactionTiming.PRE);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<SkipTurnGA>();
        ActionSystem.UnsubscribeReaction<DealDamageGA>(OnDealDamage, ReactionTiming.PRE);
        
        // Clear static dictionary to prevent stale references across scene loads
        chargingMultipliers.Clear();
    }

    /// <summary>
    /// Applies RESTING or CHARGING status to a combatant
    /// </summary>
    /// <param name="action">The skip turn action to process</param>
    /// <returns>Coroutine for action processing</returns>
    private IEnumerator SkipTurnPerformer(SkipTurnGA action)
    {
        if (action.Target == null)
        {
            Debug.LogWarning("[RestingSystem] Target is null, skipping.");
            yield break;
        }
        
        if (action.IsCharging)
        {
            // Apply CHARGING status
            action.Target.AddStatusEffect(StatusEffectType.CHARGING, action.TurnsToSkip);
            chargingMultipliers[action.Target] = action.ChargingMultiplier;
            
            Debug.Log($"[RestingSystem] {action.Target.name} is CHARGING for {action.TurnsToSkip} turn(s). Next attack: {action.ChargingMultiplier}x damage");
        }
        else
        {
            // Apply RESTING status
            action.Target.AddStatusEffect(StatusEffectType.RESTING, action.TurnsToSkip);
            
            Debug.Log($"[RestingSystem] {action.Target.name} is RESTING for {action.TurnsToSkip} turn(s).");
        }
        
        // TODO: Play charging/resting VFX
        
        yield return null;
    }

    /// <summary>
    /// Applies charging damage bonus when the charged combatant deals damage
    /// </summary>
    /// <param name="action">The damage action to potentially modify</param>
    private void OnDealDamage(DealDamageGA action)
    {
        // Check if the caster has a charging multiplier
        if (action.Caster == null) return;
        if (!chargingMultipliers.ContainsKey(action.Caster)) return;
        
        float multiplier = chargingMultipliers[action.Caster];
        
        // Apply the multiplier to the damage (handle both uniform and per-target)
        bool hasPerTargetDamages = action.PerTargetDamages != null && 
                                   action.PerTargetDamages.Count == action.Targets.Count;
        
        if (hasPerTargetDamages)
        {
            // Modify per-target damages
            for (int i = 0; i < action.PerTargetDamages.Count; i++)
            {
                float originalDamage = action.PerTargetDamages[i];
                action.PerTargetDamages[i] = originalDamage * multiplier;
            }
            Debug.Log($"[RestingSystem] CHARGING bonus! {action.Caster.name}'s attack boosted by {multiplier}x (per-target damage)");
        }
        else
        {
            // Modify uniform damage
            float originalDamage = action.Amount;
            action.Amount = originalDamage * multiplier;
            Debug.Log($"[RestingSystem] CHARGING bonus! {action.Caster.name}'s attack boosted from {originalDamage:F1} to {action.Amount:F1} ({multiplier}x)");
        }
        
        // Consume the charging bonus and ensure cleanup
        RemoveChargingState(action.Caster);
    }

    /// <summary>
    /// Checks if a combatant is resting and should skip their turn
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <returns>True if the combatant is resting and should skip their turn</returns>
    public static bool IsResting(CombatantView combatant)
    {
        if (combatant == null) return false;
        return combatant.GetStatusEffectStacks(StatusEffectType.RESTING) > 0;
    }

    /// <summary>
    /// Checks if a combatant is charging
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <returns>True if the combatant is in a charging state</returns>
    public static bool IsCharging(CombatantView combatant)
    {
        if (combatant == null) return false;
        return combatant.GetStatusEffectStacks(StatusEffectType.CHARGING) > 0;
    }

    /// <summary>
    /// Checks if a combatant should skip their turn (resting or charging)
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <returns>True if the combatant should skip their turn</returns>
    public static bool ShouldSkipTurn(CombatantView combatant)
    {
        return IsResting(combatant) || IsCharging(combatant);
    }

    /// <summary>
    /// Reduces RESTING and CHARGING duration by 1 turn for a combatant
    /// Called by StatusEffectTickSystem at the end of each turn
    /// </summary>
    /// <param name="combatant">The combatant whose RESTING/CHARGING should tick</param>
    public static void TickResting(CombatantView combatant)
    {
        if (combatant == null) return;
        
        // Clean up destroyed combatants periodically
        CleanupDestroyedCombatants();
        
        // Tick RESTING duration
        int restingStacks = combatant.GetStatusEffectStacks(StatusEffectType.RESTING);
        if (restingStacks > 0)
        {
            int remainingTurns = restingStacks - 1;
            if (remainingTurns <= 0)
            {
                combatant.RemoveStatusEffect(StatusEffectType.RESTING, restingStacks);
                Debug.Log($"[RestingSystem] {combatant.name}'s RESTING expired");
            }
            else
            {
                // Update stacks to match remaining turns
                combatant.RemoveStatusEffect(StatusEffectType.RESTING, restingStacks);
                combatant.AddStatusEffect(StatusEffectType.RESTING, remainingTurns);
                Debug.Log($"[RestingSystem] {combatant.name} RESTING: {restingStacks} → {remainingTurns} turns remaining");
            }
        }
        
        // Tick CHARGING duration (if not consumed by damage)
        int chargingStacks = combatant.GetStatusEffectStacks(StatusEffectType.CHARGING);
        if (chargingStacks > 0)
        {
            int remainingTurns = chargingStacks - 1;
            if (remainingTurns <= 0)
            {
                // CHARGING expired without being used - clean up
                combatant.RemoveStatusEffect(StatusEffectType.CHARGING, chargingStacks);
                if (chargingMultipliers.ContainsKey(combatant))
                {
                    chargingMultipliers.Remove(combatant);
                }
                Debug.Log($"[RestingSystem] {combatant.name}'s CHARGING expired without being used");
            }
            else
            {
                // Update stacks to match remaining turns
                combatant.RemoveStatusEffect(StatusEffectType.CHARGING, chargingStacks);
                combatant.AddStatusEffect(StatusEffectType.CHARGING, remainingTurns);
                Debug.Log($"[RestingSystem] {combatant.name} CHARGING: {chargingStacks} → {remainingTurns} turns remaining");
            }
        }
    }

    /// <summary>
    /// Decrements RESTING stacks at the end of a skipped turn
    /// Should be called after the turn is skipped
    /// </summary>
    /// <param name="combatant">The combatant whose resting should decrement</param>
    /// <remarks>
    /// DEPRECATED: Use TickResting() instead, which is called by StatusEffectTickSystem
    /// </remarks>
    public static void DecrementResting(CombatantView combatant)
    {
        // Clean up destroyed combatants periodically (once per turn is sufficient)
        CleanupDestroyedCombatants();
        
        if (combatant == null) return;
        
        int restingStacks = combatant.GetStatusEffectStacks(StatusEffectType.RESTING);
        if (restingStacks > 0)
        {
            combatant.RemoveStatusEffect(StatusEffectType.RESTING, 1);
            Debug.Log($"[RestingSystem] {combatant.name} resting reduced to {restingStacks - 1}");
        }
        
        // Note: CHARGING is consumed when damage is dealt, not decremented by turns
        // But if charging status is manually removed, clean up the multiplier too
        if (combatant.GetStatusEffectStacks(StatusEffectType.CHARGING) == 0 && chargingMultipliers.ContainsKey(combatant))
        {
            chargingMultipliers.Remove(combatant);
            Debug.Log($"[RestingSystem] Cleaned up charging multiplier for {combatant.name}");
        }
    }

    /// <summary>
    /// Gets the charging multiplier for a combatant
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <returns>The damage multiplier (1.0 if not charging)</returns>
    public static float GetChargingMultiplier(CombatantView combatant)
    {
        if (combatant == null) return 1f;
        return chargingMultipliers.ContainsKey(combatant) ? chargingMultipliers[combatant] : 1f;
    }

    /// <summary>
    /// Removes charging state from a combatant (both multiplier and status effect)
    /// </summary>
    /// <param name="combatant">The combatant to remove charging from</param>
    private static void RemoveChargingState(CombatantView combatant)
    {
        if (combatant == null) return;
        
        // Remove from dictionary
        chargingMultipliers.Remove(combatant);
        
        // Remove CHARGING status effect
        int stacks = combatant.GetStatusEffectStacks(StatusEffectType.CHARGING);
        if (stacks > 0)
        {
            combatant.RemoveStatusEffect(StatusEffectType.CHARGING, stacks);
        }
    }

    /// <summary>
    /// Cleans up charging multipliers for destroyed or null combatants to prevent memory leaks
    /// </summary>
    private static void CleanupDestroyedCombatants()
    {
        // Create a list of keys to remove (can't modify dictionary while iterating)
        List<CombatantView> toRemove = new List<CombatantView>();
        
        foreach (var kvp in chargingMultipliers)
        {
            // Check if the combatant is null or destroyed (Unity's null check handles both)
            if (kvp.Key == null)
            {
                toRemove.Add(kvp.Key);
            }
        }
        
        // Remove all destroyed combatants
        foreach (var combatant in toRemove)
        {
            chargingMultipliers.Remove(combatant);
            Debug.Log("[RestingSystem] Cleaned up charging multiplier for destroyed combatant");
        }
    }

    /// <summary>
    /// Clears all resting/charging tracking data (for combat reset)
    /// </summary>
    public static void ClearAllRestingState()
    {
        chargingMultipliers.Clear();
    }
}
