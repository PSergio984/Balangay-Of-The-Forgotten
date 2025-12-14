/*
 * NO COOLDOWN STATUS EFFECT SYSTEM DOCUMENTATION
 * 
 * How it works:
 * This system handles NO_COOLDOWN status effect for the Bundok Pulag special card.
 * While active, all skills for affected combatants skip cooldown increments.
 * Duration is tracked per combatant and ticked down each turn.
 * 
 * Design reasoning:
 * Bundok Pulag grants "no cooldown for all skills for 4 rounds".
 * This is implemented by intercepting cooldown increment actions.
 * When a combatant has NO_COOLDOWN status, their skills don't gain cooldown.
 * 
 * Integration:
 * - Performs ApplyNoCooldownGA actions to grant the buff
 * - CooldownSystem checks HasNoCooldown() before incrementing cooldowns
 * - StatusEffectTickSystem calls TickNoCooldown() each turn for duration countdown
 * 
 * Used by:
 * - Bundok Pulag Mini-Boss Buff (4 rounds for all players)
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// System that handles NO_COOLDOWN status effect
/// </summary>
public class NoCooldownSystem : MonoBehaviour
{
    /// <summary>
    /// Singleton instance for easy access
    /// </summary>
    public static NoCooldownSystem Instance { get; private set; }
    
    /// <summary>
    /// Tracks remaining duration for each combatant (key = instance ID)
    /// </summary>
    private Dictionary<int, int> noCooldownDurations = new Dictionary<int, int>();
    
    /// <summary>
    /// Lookup to get combatant reference from instance ID
    /// </summary>
    private Dictionary<int, CombatantView> combatantLookup = new Dictionary<int, CombatantView>();

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        // Register performer for ApplyNoCooldownGA
        ActionSystem.AttachPerformer<ApplyNoCooldownGA>(ApplyNoCooldownPerformer);
    }

    private void OnDisable()
    {
        // Unregister performer
        ActionSystem.DetachPerformer<ApplyNoCooldownGA>();
    }

    /// <summary>
    /// Performs ApplyNoCooldownGA action to grant no cooldown buff
    /// </summary>
    private IEnumerator ApplyNoCooldownPerformer(ApplyNoCooldownGA action)
    {
        foreach (var target in action.Targets)
        {
            if (target == null) continue;
            
            int instanceId = target.GetInstanceID();
            
            // Store or update duration data
            noCooldownDurations[instanceId] = action.Duration;
            combatantLookup[instanceId] = target;
            
            // Apply status effect with duration as stacks (for UI display)
            target.AddStatusEffect(StatusEffectType.NO_COOLDOWN, action.Duration);
            
            Debug.Log($"[NoCooldownSystem] {target.name} gains NO COOLDOWN for {action.Duration} rounds");
        }
        
        yield return null;
    }

    /// <summary>
    /// Checks if a combatant has NO_COOLDOWN status active
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <returns>True if the combatant has no cooldown buff</returns>
    public bool HasNoCooldown(CombatantView combatant)
    {
        if (combatant == null) return false;
        
        int instanceId = combatant.GetInstanceID();
        
        if (noCooldownDurations.TryGetValue(instanceId, out int duration))
        {
            return duration > 0;
        }
        
        return false;
    }

    /// <summary>
    /// Checks if ANY combatant has NO_COOLDOWN status active
    /// </summary>
    /// <returns>True if any combatant has no cooldown buff</returns>
    /// <remarks>
    /// Used by CooldownSystem to check if the Bundok Pulag buff is active.
    /// Since this buff applies to all heroes, we just check if anyone has it.
    /// </remarks>
    public bool HasAnyNoCooldown()
    {
        foreach (var kvp in noCooldownDurations)
        {
            if (kvp.Value > 0)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Ticks down NO_COOLDOWN duration for all affected combatants
    /// Called by StatusEffectTickSystem at the end of each round
    /// </summary>
    public void TickNoCooldown()
    {
        List<int> toRemove = new List<int>();
        
        foreach (var kvp in noCooldownDurations)
        {
            int instanceId = kvp.Key;
            int duration = kvp.Value;
            
            if (duration > 0)
            {
                // Decrement duration
                noCooldownDurations[instanceId] = duration - 1;
                
                // Get combatant reference
                if (combatantLookup.TryGetValue(instanceId, out CombatantView combatant) && combatant != null)
                {
                    // Update status effect stacks to show remaining duration
                    combatant.RemoveStatusEffect(StatusEffectType.NO_COOLDOWN, 1);
                    
                    Debug.Log($"[NoCooldownSystem] {combatant.name} NO COOLDOWN duration: {noCooldownDurations[instanceId]} rounds remaining");
                    
                    // If duration hit 0, remove completely
                    if (noCooldownDurations[instanceId] <= 0)
                    {
                        int remainingStacks = combatant.GetStatusEffectStacks(StatusEffectType.NO_COOLDOWN);
                        combatant.RemoveStatusEffect(StatusEffectType.NO_COOLDOWN, remainingStacks);
                        toRemove.Add(instanceId);
                        Debug.Log($"[NoCooldownSystem] {combatant.name} NO COOLDOWN expired");
                    }
                }
                else
                {
                    // Combatant destroyed, mark for removal
                    toRemove.Add(instanceId);
                }
            }
            else
            {
                toRemove.Add(instanceId);
            }
        }
        
        // Clean up expired entries
        foreach (int id in toRemove)
        {
            noCooldownDurations.Remove(id);
            combatantLookup.Remove(id);
        }
    }

    /// <summary>
    /// Clears all NO_COOLDOWN effects (called when combat ends)
    /// </summary>
    public void ClearAll()
    {
        noCooldownDurations.Clear();
        combatantLookup.Clear();
        Debug.Log("[NoCooldownSystem] All NO_COOLDOWN effects cleared");
    }

    /// <summary>
    /// Gets remaining duration for a specific combatant
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <returns>Remaining rounds, or 0 if not affected</returns>
    public int GetRemainingDuration(CombatantView combatant)
    {
        if (combatant == null) return 0;
        
        int instanceId = combatant.GetInstanceID();
        
        if (noCooldownDurations.TryGetValue(instanceId, out int duration))
        {
            return duration;
        }
        
        return 0;
    }
}
