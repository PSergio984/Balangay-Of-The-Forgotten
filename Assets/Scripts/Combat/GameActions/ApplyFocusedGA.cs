/*
 * APPLY FOCUSED GAME ACTION DOCUMENTATION
 * 
 * How it works:
 * This action applies FOCUSED status (combo buff) with duration tracking.
 * FOCUSED provides fixed bonuses: +30% hit chance and +20% defense ignore.
 * Unlike percentage-based buffs, the bonuses are fixed.
 * 
 * Design reasoning:
 * FOCUSED represents a concentrated mental state with fixed benefits.
 * Duration is tracked separately to allow multi-turn focus effects.
 * 
 * Integration:
 * - Created by ApplyFocusedEffect
 * - Processed by FocusedSystem
 * - Duration tracked via dictionary and ticked down by StatusEffectTickSystem
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game action that applies FOCUSED with configurable duration
/// </summary>
public class ApplyFocusedGA : GameAction
{
    /// <summary>
    /// List of combatants who will receive the focused buff
    /// </summary>
    public List<CombatantView> Targets { get; private set; }
    
    /// <summary>
    /// How many turns the focused buff lasts before expiring
    /// </summary>
    public int Duration { get; private set; }

    /// <summary>
    /// Creates a new focused action
    /// </summary>
    /// <param name="targets">Who should receive the focused buff</param>
    /// <param name="duration">How many turns it lasts</param>
    public ApplyFocusedGA(List<CombatantView> targets, int duration)
    {
        if (targets == null || targets.Count == 0)
            throw new System.ArgumentException("Targets cannot be null or empty", nameof(targets));
        if (duration <= 0)
            throw new System.ArgumentException("Duration must be positive", nameof(duration));

        Targets = new List<CombatantView>(targets);
        Duration = duration;
    }
}
