/*
 * APPLY STUN GAME ACTION DOCUMENTATION
 * 
 * How it works:
 * This action applies stun to target combatants for a specified duration.
 * Stunned combatants skip their turn and cannot perform any actions.
 * The StunStatusEffectSystem processes this action to apply the stun effect.
 * 
 * Design reasoning:
 * Stun is a control effect that prevents actions. We use stacks to track duration,
 * where each stack represents one turn of being stunned. This allows multiple stuns
 * to stack duration or be partially cleansed.
 * 
 * Integration:
 * - Created by StunEffect when moves with stun chance succeed
 * - Processed by StunStatusEffectSystem to apply stun status
 * - Turn system checks for STUN status to skip turns
 * 
 * Used by:
 * - Skyhammer (Bathala): 70% chance to stun 1 player for 1 turn
 * - Thunderous Decree (Bathala): 50% chance to stun 2 players for 1 turn
 * - Radiant Charge (Apolaki): 30% chance to stun 1 enemy
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game action that applies stun status effect to target combatants
/// </summary>
/// <remarks>
/// Stun prevents the target from taking any actions during their turn.
/// Duration is tracked as stacks - each stack = 1 turn of stun.
/// </remarks>
public class ApplyStunGA : GameAction, IHaveCaster
{
    /// <summary>
    /// List of combatants who will be stunned
    /// </summary>
    public List<CombatantView> Targets { get; private set; }
    
    /// <summary>
    /// How many turns the stun lasts (stored as stacks)
    /// </summary>
    public int Duration { get; private set; }
    
    /// <summary>
    /// Who caused this stun (for perk system tracking)
    /// </summary>
    public CombatantView Caster { get; set; }

    /// <summary>
    /// Creates a new stun action
    /// </summary>
    /// <param name="targets">Who should be stunned</param>
    /// <param name="duration">How many turns they're stunned</param>
    /// <param name="caster">Who is causing the stun</param>
    public ApplyStunGA(List<CombatantView> targets, int duration, CombatantView caster)
    {
        if (targets == null)
            throw new System.ArgumentNullException(nameof(targets), "Targets list cannot be null.");
        if (targets.Count == 0)
            throw new System.ArgumentException("Targets list cannot be empty.", nameof(targets));
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] == null)
                throw new System.ArgumentException($"Target at index {i} is null.", nameof(targets));
        }
        if (duration <= 0)
            throw new System.ArgumentOutOfRangeException(nameof(duration), "Stun duration must be greater than zero.");
        if (caster == null)
            throw new System.ArgumentNullException(nameof(caster), "Caster cannot be null.");
        Targets = new List<CombatantView>(targets);
        Duration = duration;
        Caster = caster;
    }
    
    /// <summary>
    /// Creates a stun action for a single target
    /// </summary>
    /// <param name="target">Who should be stunned</param>
    /// <param name="duration">How many turns they're stunned</param>
    /// <param name="caster">Who is causing the stun</param>
    public ApplyStunGA(CombatantView target, int duration, CombatantView caster)
    {
        if (target == null)
            throw new System.ArgumentNullException(nameof(target));
        if (duration <= 0)
            throw new System.ArgumentOutOfRangeException(nameof(duration), "Duration must be positive");
        if (caster == null)
            throw new System.ArgumentNullException(nameof(caster));

        Targets = new List<CombatantView> { target };
        Duration = duration;
        Caster = caster;
    }
}