/*
 * REMOVE DEBUFFS GAME ACTION DOCUMENTATION
 * 
 * How it works:
 * This action removes debuff status effects from target combatants.
 * Can remove all debuffs or specific types. Used for cleanse abilities
 * like Rest, Purify, Heaven's Mandate, and Tide of Night.
 * 
 * Design reasoning:
 * Cleansing is important in turn-based combat to counter debuff strategies.
 * We support both "remove all debuffs" and "remove specific debuff" patterns.
 * The system identifies debuffs by checking a predefined list of debuff types.
 * 
 * Integration:
 * - Created by RemoveDebuffsEffect from cleanse abilities
 * - Processed by BuffDebuffSystem to remove status effects
 * - Works with existing status effect tracking on CombatantView
 * 
 * Used by:
 * - Rest (Mandirigma): Removes negative status effects for self
 * - Purify (Support): Removes all debuffs from all players
 * - Heaven's Mandate (Bathala): Removes debuffs
 * - Tide of Night (Mayari): Removes all debuffs
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game action that removes debuff status effects from target combatants
/// </summary>
public class RemoveDebuffsGA : GameAction
{
    /// <summary>
    /// List of combatants whose debuffs will be removed
    /// </summary>
    public List<CombatantView> Targets { get; private set; }
    
    /// <summary>
    /// Whether to remove all debuffs or just a specific one
    /// </summary>
    public bool RemoveAll { get; private set; }
    
    /// <summary>
    /// Specific debuff type to remove (only used if RemoveAll is false)
    /// </summary>
    public StatusEffectType? SpecificDebuff { get; private set; }

    /// <summary>
    /// Static list of status effect types that are considered debuffs
    /// </summary>
    public static readonly StatusEffectType[] DebuffTypes = new[]
    {
        StatusEffectType.BURN,
        StatusEffectType.DEFENSE_DOWN,
        StatusEffectType.BONECRACKED,
        StatusEffectType.MOONFALL,
        StatusEffectType.DEVOURED,
        StatusEffectType.STUN,
    };

    public RemoveDebuffsGA(List<CombatantView> targets)
    {
        if (targets == null)
            throw new System.ArgumentNullException(nameof(targets));
        Targets = new List<CombatantView>(targets);
        RemoveAll = true;
        SpecificDebuff = null;
    }    
    /// <summary>
    /// Creates an action to remove a specific debuff from targets
    /// </summary>
    /// <param name="targets">Who should have the debuff removed</param>
    /// <param name="specificDebuff">Which debuff type to remove</param>
    public RemoveDebuffsGA(List<CombatantView> targets, StatusEffectType specificDebuff)
    {
        if (targets == null)
            throw new System.ArgumentNullException(nameof(targets));
        Targets = new List<CombatantView>(targets);
        RemoveAll = false;
        SpecificDebuff = specificDebuff;
    }    
    /// <summary>
    /// Creates an action for a single target
    /// </summary>
    /// <param name="target">Who should have debuffs removed</param>
    /// <param name="removeAll">True to remove all, false for specific</param>
    /// <param name="specificDebuff">Which debuff if not removing all</param>
    public RemoveDebuffsGA(CombatantView target, bool removeAll = true, StatusEffectType? specificDebuff = null)
    {
        if (target == null)
            throw new System.ArgumentNullException(nameof(target));
        if (!removeAll && specificDebuff == null)
            throw new System.ArgumentException("specificDebuff must be provided when removeAll is false");
        Targets = new List<CombatantView> { target };
        RemoveAll = removeAll;
        SpecificDebuff = specificDebuff;
    }    
    /// <summary>
    /// Checks if a status effect type is considered a debuff
    /// </summary>
    /// <param name="type">The status effect type to check</param>
    /// <returns>True if it's a debuff</returns>
    public static bool IsDebuff(StatusEffectType type)
    {
        foreach (var debuff in DebuffTypes)
        {
            if (debuff == type) return true;
        }
        return false;
    }
}

