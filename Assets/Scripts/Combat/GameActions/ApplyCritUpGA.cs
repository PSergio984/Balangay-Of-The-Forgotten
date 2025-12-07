/*
 * APPLY CRIT UP GAME ACTION DOCUMENTATION
 * 
 * How it works:
 * This action applies CRIT_UP status with separate percentage and duration tracking.
 * The percentage determines crit chance increase, duration determines how long it lasts.
 * 
 * Design reasoning:
 * Separates crit chance from duration for clarity and balance.
 * This allows "+55% crit for 1 turn" to work as expected.
 * 
 * Integration:
 * - Created by ApplyCritUpEffect
 * - Processed by CritUpSystem
 * - Duration tracked via dictionary and ticked down by StatusEffectTickSystem
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game action that applies CRIT_UP with configurable percentage and duration
/// </summary>
public class ApplyCritUpGA : GameAction
{
    /// <summary>
    /// List of combatants who will receive the crit buff
    /// </summary>
    public List<CombatantView> Targets { get; private set; }
    
    /// <summary>
    /// The percentage of crit chance increase (e.g., 55 = +55% crit chance)
    /// </summary>
    public int CritPercentage { get; private set; }
    
    /// <summary>
    /// How many turns the crit buff lasts before expiring
    /// </summary>
    public int Duration { get; private set; }

    /// <summary>
    /// Creates a new crit up action
    /// </summary>
    /// <param name="targets">Who should receive the crit buff</param>
    /// <param name="critPercentage">Percentage of crit increase (1-100)</param>
    /// <param name="duration">How many turns it lasts</param>
    public ApplyCritUpGA(List<CombatantView> targets, int critPercentage, int duration)
    {
        if (targets == null || targets.Count == 0)
            throw new System.ArgumentException("Targets cannot be null or empty", nameof(targets));
        if (critPercentage <= 0 || critPercentage > 100)
            throw new System.ArgumentException("Crit percentage must be between 1 and 100", nameof(critPercentage));
        if (duration <= 0)
            throw new System.ArgumentException("Duration must be positive", nameof(duration));

        Targets = new List<CombatantView>(targets);
        CritPercentage = critPercentage;
        Duration = duration;
    }
}
