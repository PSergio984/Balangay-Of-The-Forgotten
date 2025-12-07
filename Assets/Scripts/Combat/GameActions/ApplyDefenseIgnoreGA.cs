using System.Collections.Generic;
using UnityEngine;

/* APPLY DEFENSE IGNORE GAME ACTION DOCUMENTATION
 * 
 * How it works:
 * This action applies a defense ignore buff to target combatants.
 * When these combatants deal damage, they ignore a percentage of the target's defense.
 * 
 * Design reasoning:
 * Defense ignore is a powerful offensive buff that makes attacks more effective.
 * It's applied during damage calculation by modifying the target's effective defense.
 * This action is processed by DefenseIgnoreSystem.
 * 
 * Integration:
 * - Created by ApplyDefenseIgnoreEffect
 * - Processed by DefenseIgnoreSystem
 * - DefenseIgnoreSystem subscribes to DealDamageGA PRE events to apply the ignore
 * 
 * Used by:
 * - Daybreak Fury (Apolaki): Ignores 20% DEF for 1 turn
 */

/// <summary>
/// Game action that applies defense ignore buff to target combatants
/// </summary>
public class ApplyDefenseIgnoreGA : GameAction
{
    /// <summary>
    /// List of combatants who will receive the defense ignore buff
    /// </summary>
    public List<CombatantView> Targets { get; private set; }
    
    /// <summary>
    /// Percentage of defense to ignore (e.g., 20 = ignore 20% of enemy defense)
    /// </summary>
    public int DefenseIgnorePercentage { get; private set; }
    
    /// <summary>
    /// How many turns the buff lasts
    /// </summary>
    public int Duration { get; private set; }

    /// <summary>
    /// Creates an action to apply defense ignore buff
    /// </summary>
    /// <param name="targets">Who receives the buff</param>
    /// <param name="defenseIgnorePercentage">Percentage of defense to ignore (1-100)</param>
    /// <param name="duration">How many turns the buff lasts</param>
    public ApplyDefenseIgnoreGA(List<CombatantView> targets, int defenseIgnorePercentage, int duration)
    {
        Targets = targets ?? new List<CombatantView>();
        DefenseIgnorePercentage = defenseIgnorePercentage;
        Duration = duration;
    }
}
