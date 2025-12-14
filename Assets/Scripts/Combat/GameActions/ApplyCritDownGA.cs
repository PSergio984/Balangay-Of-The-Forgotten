/*
 * APPLY CRIT DOWN GAME ACTION DOCUMENTATION
 * 
 * How it works:
 * This action applies CRIT_DOWN status (debuff) with separate percentage and duration tracking.
 * The percentage determines crit chance reduction, duration determines how long it lasts.
 * 
 * Design reasoning:
 * Separates crit reduction from duration for clarity and balance.
 * This allows "-30% crit for 2 turns" to work as expected.
 * 
 * Integration:
 * - Created by ApplyCritDownEffect
 * - Processed by CritDownSystem
 * - Duration tracked via dictionary and ticked down by StatusEffectTickSystem
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game action that applies CRIT_DOWN with configurable percentage and duration
/// </summary>
public class ApplyCritDownGA : GameAction
{
    /// <summary>
    /// List of combatants who will receive the crit debuff
    /// </summary>
    public List<CombatantView> Targets { get; private set; }
    
    /// <summary>
    /// The percentage of crit chance decrease (e.g., 30 = -30% crit chance)
    /// </summary>
    public int CritPercentage { get; private set; }
    
    /// <summary>
    /// How many turns the crit debuff lasts before expiring
    /// </summary>
    public int Duration { get; private set; }

    /// <summary>
    /// Creates a new crit down action
    /// </summary>
    /// <param name="targets">Who should receive the crit debuff</param>
    /// <param name="critPercentage">Percentage of crit decrease (1-100)</param>
    /// <param name="duration">How many turns it lasts</param>
    public ApplyCritDownGA(List<CombatantView> targets, int critPercentage, int duration)
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
