/*
 * RAGE STATUS EFFECT SYSTEM DOCUMENTATION
 * 
 * How it works:
 * This system implements the RAGE status effect (Berserk State).
 * RAGE is a powerful offensive buff that combines multiple benefits:
 * - +50% damage increase
 * - Ignores 20% of enemy defense
 * - +20% hit chance
 * 
 * Design reasoning:
 * RAGE is applied as a timed status effect (3 turns) that modifies outgoing damage.
 * We intercept damage actions in PRE phase to apply the damage bonus.
 * Defense ignore and hit chance are handled separately (defense in damage calculation, hit in accuracy checks).
 * 
 * Integration:
 * - Subscribes to DealDamageGA PRE events to modify outgoing damage
 * - Works with existing status effect tracking system
 * - Tracks RAGE duration per combatant
 * 
 * Used by:
 * - Berserk State (Mandirigma): RAGE for 3 turns, requires HP ≤50%, CD 4
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// System that handles RAGE status effect damage modifications
/// </summary>
/// <remarks>
/// RAGE provides a 50% damage boost to all attacks from the raging combatant.
/// It also provides 20% defense ignore and 20% hit chance (handled elsewhere).
/// This system only handles the damage amplification component.
/// </remarks>
public class RageStatusEffectSystem : MonoBehaviour
{
    /// <summary>
    /// Tracks RAGE duration per combatant instance ID (prevents memory leaks)
    /// </summary>
    private static Dictionary<int, int> rageDurations = new Dictionary<int, int>();
    
    /// <summary>
    /// Lookup table to retrieve CombatantView from instance ID (for status effect updates)
    /// </summary>
    private static Dictionary<int, CombatantView> combatantLookup = new Dictionary<int, CombatantView>();

    /// <summary>
    /// Damage multiplier for RAGE (+50% damage = 1.5x multiplier)
    /// </summary>
    private const float RAGE_DAMAGE_MULTIPLIER = 1.5f;
    
    /// <summary>
    /// Defense ignore percentage for RAGE (20% = 0.2)
    /// </summary>
    public const float RAGE_DEFENSE_IGNORE = 0.2f;
    
    /// <summary>
    /// Hit chance bonus for RAGE (+20% = 0.2)
    /// </summary>
    public const float RAGE_HIT_BONUS = 0.2f;

    private void OnEnable()
    {
        // Register the performer for ApplyRageGA
        ActionSystem.AttachPerformer<ApplyRageGA>(ApplyRagePerformer);
        
        // Subscribe to damage PRE events to modify damage from raging combatants
        ActionSystem.SubscribeReaction<DealDamageGA>(OnDamageAboutToBeDealt, ReactionTiming.PRE);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyRageGA>();
        ActionSystem.UnsubscribeReaction<DealDamageGA>(OnDamageAboutToBeDealt, ReactionTiming.PRE);
    }
    
    /// <summary>
    /// Applies RAGE status to all targets
    /// </summary>
    /// <param name="action">The RAGE action to process</param>
    /// <returns>Coroutine for action processing</returns>
    private IEnumerator ApplyRagePerformer(ApplyRageGA action)
    {
        foreach (var target in action.Targets)
        {
            if (target == null) continue;
            
            ApplyRage(target, action.Duration);
            // TODO: Play RAGE VFX
            yield return null;
        }
    }

    /// <summary>
    /// Modifies outgoing damage if the caster has RAGE status
    /// </summary>
    /// <param name="damageAction">The damage action to potentially modify</param>
    /// <remarks>
    /// Checks if the damage caster has RAGE status. If so, increases damage by 50%.
    /// Handles both uniform damage (Amount) and per-target damage (PerTargetDamages).
    /// </remarks>
    private void OnDamageAboutToBeDealt(DealDamageGA damageAction)
    {
        // Check if there's a caster and they have RAGE
        if (damageAction.Caster == null) return;
        
        int rageStacks = damageAction.Caster.GetStatusEffectStacks(StatusEffectType.RAGE);
        if (rageStacks <= 0) return;
        
        // Apply damage multiplier
        bool hasPerTargetDamages = damageAction.PerTargetDamages != null && 
                                   damageAction.PerTargetDamages.Count == damageAction.Targets.Count;
        
        if (hasPerTargetDamages)
        {
            // Modify per-target damages
            for (int i = 0; i < damageAction.PerTargetDamages.Count; i++)
            {
                damageAction.PerTargetDamages[i] *= RAGE_DAMAGE_MULTIPLIER;
            }
            Debug.Log($"[RageSystem] {damageAction.Caster.name}'s RAGE amplified per-target damage by {(RAGE_DAMAGE_MULTIPLIER - 1f) * 100f}%!");
        }
        else
        {
            // Modify uniform damage
            damageAction.Amount *= RAGE_DAMAGE_MULTIPLIER;
            Debug.Log($"[RageSystem] {damageAction.Caster.name}'s RAGE amplified damage from {damageAction.Amount / RAGE_DAMAGE_MULTIPLIER} to {damageAction.Amount}!");
        }
    }

    /// <summary>
    /// Applies RAGE status to a combatant for a specified duration
    /// </summary>
    /// <param name="combatant">The combatant to receive RAGE</param>
    /// <param name="duration">How many turns RAGE lasts</param>
    /// <remarks>
    /// Call this when applying RAGE status effect. Tracks duration separately from stacks.
    /// </remarks>
    public static void ApplyRage(CombatantView combatant, int duration)
    {
        if (combatant == null) return;
        
        int instanceId = combatant.GetInstanceID();
        combatantLookup[instanceId] = combatant;
        rageDurations[instanceId] = duration;
        
        // Add RAGE status indicator (use duration as stacks for UI)
        combatant.AddStatusEffect(StatusEffectType.RAGE, duration);
        Debug.Log($"[RageSystem] {combatant.name} enters RAGE for {duration} turns! (+50% DMG, +20% def ignore, +20% hit)");
    }

    /// <summary>
    /// Decrements RAGE duration at end of turn
    /// Should be called at end of each combatant's turn
    /// </summary>
    /// <param name="combatant">The combatant whose RAGE duration should decrement</param>
    public static void DecrementRageDuration(CombatantView combatant)
    {
        if (combatant == null) return;
        
        int instanceId = combatant.GetInstanceID();
        if (!rageDurations.ContainsKey(instanceId)) return;
        
        rageDurations[instanceId]--;
        
        // Update status effect stacks to match remaining duration
        int currentStacks = combatant.GetStatusEffectStacks(StatusEffectType.RAGE);
        if (currentStacks > 0)
        {
            combatant.RemoveStatusEffect(StatusEffectType.RAGE, 1);
        }
        
        if (rageDurations[instanceId] <= 0)
        {
            // RAGE expired
            combatant.RemoveStatusEffect(StatusEffectType.RAGE, combatant.GetStatusEffectStacks(StatusEffectType.RAGE));
            rageDurations.Remove(instanceId);
            combatantLookup.Remove(instanceId);
            Debug.Log($"[RageSystem] {combatant.name}'s RAGE has ENDED!");
        }
        else
        {
            Debug.Log($"[RageSystem] {combatant.name}'s RAGE: {rageDurations[instanceId]} turn(s) remaining");
        }
    }

    /// <summary>
    /// Checks if a combatant currently has RAGE active
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <returns>True if combatant has RAGE</returns>
    public static bool HasRage(CombatantView combatant)
    {
        return combatant != null && combatant.GetStatusEffectStacks(StatusEffectType.RAGE) > 0;
    }

    /// <summary>
    /// Gets the remaining duration of RAGE for a combatant
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <returns>Remaining turns of RAGE, or 0 if no RAGE</returns>
    public static int GetRageDuration(CombatantView combatant)
    {
        if (combatant == null) return 0;
        int instanceId = combatant.GetInstanceID();
        return rageDurations.ContainsKey(instanceId) ? rageDurations[instanceId] : 0;
    }

    /// <summary>
    /// Clears all RAGE tracking data (for combat reset)
    /// </summary>
    public static void ClearAllRage()
    {
        // Remove status effects before clearing tracking data
        foreach (var kvp in rageDurations)
        {
            if (combatantLookup.TryGetValue(kvp.Key, out var combatant) && combatant != null)
            {
                combatant.RemoveStatusEffect(StatusEffectType.RAGE, combatant.GetStatusEffectStacks(StatusEffectType.RAGE));
            }
        }
        rageDurations.Clear();
        combatantLookup.Clear();
    }
}
