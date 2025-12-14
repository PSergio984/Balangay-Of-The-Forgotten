/*
 * FOCUSED STATUS EFFECT SYSTEM DOCUMENTATION
 * 
 * How it works:
 * This system handles FOCUSED status effect with duration tracking.
 * FOCUSED is a combo buff that provides fixed bonuses:
 * - +30% hit chance (increases accuracy)
 * - +20% defense ignore (bypasses portion of target's defense)
 * Duration determines how long the buff lasts.
 * 
 * Design reasoning:
 * FOCUSED represents a concentrated combat state with multiple synergistic effects.
 * The bonuses are fixed values (not percentage-based) for balance.
 * Duration is tracked in a separate dictionary and counted down each turn.
 * 
 * Integration:
 * - Performs ApplyFocusedGA actions to grant focused buff
 * - Provides GetHitChanceBonus() for accuracy calculations
 * - Provides GetDefenseIgnoreBonus() for defense penetration
 * - StatusEffectTickSystem calls TickFocused() each turn for duration countdown
 * 
 * Used by:
 * - Focus Aim (2 turns)
 * - Precision Strike (1 turn)
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// System that handles FOCUSED status effect with duration tracking
/// </summary>
public class FocusedSystem : MonoBehaviour
{
    // Fixed bonus values for FOCUSED status effect
    private const float HIT_CHANCE_BONUS = 0.30f;     // +30% hit chance
    private const float DEFENSE_IGNORE_BONUS = 0.20f; // +20% defense ignore

    /// <summary>
    /// Tracks remaining duration for each combatant (key = instance ID)
    /// </summary>
    private Dictionary<int, int> focusedDurations = new Dictionary<int, int>();
    
    /// <summary>
    /// Lookup to get combatant reference from instance ID
    /// </summary>
    private Dictionary<int, CombatantView> combatantLookup = new Dictionary<int, CombatantView>();

    private void OnEnable()
    {
        // Register the performer for ApplyFocusedGA
        ActionSystem.AttachPerformer<ApplyFocusedGA>(ApplyFocusedPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<ApplyFocusedGA>();
    }

    /// <summary>
    /// Applies FOCUSED status to all targets
    /// </summary>
    private IEnumerator ApplyFocusedPerformer(ApplyFocusedGA action)
    {
        foreach (var target in action.Targets)
        {
            if (target == null) continue;
            
            int instanceId = target.GetInstanceID();
            
            // Check if already has focused - refresh duration
            if (focusedDurations.TryGetValue(instanceId, out int existingDuration))
            {
                focusedDurations[instanceId] = Mathf.Max(existingDuration, action.Duration);
                Debug.Log($"[FocusedSystem] {target.name}'s FOCUSED duration refreshed to {focusedDurations[instanceId]} turns");
            }
            else
            {
                // New focused buff
                focusedDurations[instanceId] = action.Duration;
                combatantLookup[instanceId] = target;
                
                // Apply status effect with 1 stack (for UI display - shows the icon)
                target.AddStatusEffect(StatusEffectType.FOCUSED, 1);
                
                Debug.Log($"[FocusedSystem] {target.name} gains FOCUSED (+30% hit, +20% def ignore) for {action.Duration} turns");
            }
        }
        
        yield return null;
    }

    /// <summary>
    /// Ticks down the focused buff duration for a combatant
    /// Called by StatusEffectTickSystem at the end of each turn
    /// </summary>
    /// <param name="combatant">The combatant to tick</param>
    public void TickFocused(CombatantView combatant)
    {
        if (combatant == null) return;
        
        int instanceId = combatant.GetInstanceID();
        if (!focusedDurations.ContainsKey(instanceId)) return;
        
        // Decrease duration
        focusedDurations[instanceId]--;
        
        if (focusedDurations[instanceId] <= 0)
        {
            // Focused buff expired
            focusedDurations.Remove(instanceId);
            combatantLookup.Remove(instanceId);
            
            // Remove the status effect
            combatant.RemoveStatusEffect(StatusEffectType.FOCUSED, 1);
            
            Debug.Log($"[FocusedSystem] {combatant.name}'s FOCUSED buff expired");
        }
        else
        {
            Debug.Log($"[FocusedSystem] {combatant.name}'s FOCUSED duration: {focusedDurations[instanceId]} turns remaining");
        }
    }

    /// <summary>
    /// Gets the hit chance bonus for a combatant with FOCUSED
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <returns>Hit chance bonus as decimal (0.30 = +30%)</returns>
    public float GetHitChanceBonus(CombatantView combatant)
    {
        if (combatant == null) return 0f;
        
        int instanceId = combatant.GetInstanceID();
        if (focusedDurations.ContainsKey(instanceId))
        {
            return HIT_CHANCE_BONUS;
        }
        
        return 0f;
    }

    /// <summary>
    /// Gets the defense ignore bonus for a combatant with FOCUSED
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <returns>Defense ignore percentage as decimal (0.20 = 20%)</returns>
    public float GetDefenseIgnoreBonus(CombatantView combatant)
    {
        if (combatant == null) return 0f;
        
        int instanceId = combatant.GetInstanceID();
        if (focusedDurations.ContainsKey(instanceId))
        {
            return DEFENSE_IGNORE_BONUS;
        }
        
        return 0f;
    }

    /// <summary>
    /// Calculates the effective defense after FOCUSED defense ignore is applied
    /// </summary>
    /// <param name="attacker">The attacking combatant</param>
    /// <param name="targetDefense">The target's base defense value</param>
    /// <returns>Effective defense after penetration</returns>
    public int CalculateEffectiveDefense(CombatantView attacker, int targetDefense)
    {
        float defenseIgnore = GetDefenseIgnoreBonus(attacker);
        
        if (defenseIgnore > 0)
        {
            int ignoredDefense = Mathf.RoundToInt(targetDefense * defenseIgnore);
            int effectiveDefense = targetDefense - ignoredDefense;
            
            Debug.Log($"[FocusedSystem] {attacker.name} ignores {ignoredDefense} defense ({targetDefense} → {effectiveDefense})");
            
            return Mathf.Max(0, effectiveDefense); // Defense can't go below 0
        }
        
        return targetDefense;
    }

    /// <summary>
    /// Checks if an attack hits based on attacker's hit chance
    /// </summary>
    /// <param name="attacker">The attacking combatant</param>
    /// <param name="baseHitChance">Base hit chance (0.0 to 1.0)</param>
    /// <returns>True if the attack hits</returns>
    public bool RollForHit(CombatantView attacker, float baseHitChance = 1.0f)
    {
        float totalHitChance = baseHitChance + GetHitChanceBonus(attacker);
        
        // Cap hit chance at 100%
        totalHitChance = Mathf.Min(totalHitChance, 1f);
        
        float roll = Random.value;
        bool isHit = roll <= totalHitChance;
        
        if (!isHit)
        {
            Debug.Log($"[FocusedSystem] {attacker.name} MISSED! (roll: {roll:F2}, chance: {totalHitChance:F2})");
        }
        
        return isHit;
    }

    /// <summary>
    /// Gets whether the combatant has the FOCUSED buff active
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <returns>True if FOCUSED is active</returns>
    public bool HasFocused(CombatantView combatant)
    {
        if (combatant == null) return false;
        return focusedDurations.ContainsKey(combatant.GetInstanceID());
    }
}
