/*
 * APPLY DEFENSE UP GAME ACTION DOCUMENTATION
 * 
 * How it works:
 * This action applies DEFENSE_UP status with separate percentage and duration tracking.
 * The percentage determines damage reduction, duration determines how long it lasts.
 * Stores duration metadata so StatusEffectTickSystem can count down properly.
 * 
 * Design reasoning:
 * Separates defense strength from duration for clarity and balance.
 * Uses metadata to track duration since status effect stacks represent percentage.
 * This allows "+50% defense for 3 turns" to work as expected.
 * 
 * Integration:
 * - Created by ApplyDefenseUpEffect
 * - Processed by StatusEffectSystem/DefenseUpSystem
 * - Duration tracked via metadata and ticked down by StatusEffectTickSystem
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game action that applies DEFENSE_UP with configurable percentage and duration
/// </summary>
public class ApplyDefenseUpGA : GameAction
{
    /// <summary>
    /// List of combatants who will receive the defense buff
    /// </summary>
    public List<CombatantView> Targets { get; private set; }
    
    /// <summary>
    /// The percentage of defense increase (e.g., 50 = 50% damage reduction)
    /// This becomes the stack count in the status effect system
    /// </summary>
    public int DefensePercentage { get; private set; }
    
    /// <summary>
    /// How many turns the defense buff lasts before expiring
    /// </summary>
    public int Duration { get; private set; }

    /// <summary>
    /// Creates a new defense up action
    /// </summary>
    /// <param name="targets">Who should receive the defense buff</param>
    /// <param name="defensePercentage">Percentage of defense increase (1-90)</param>
    /// <param name="duration">How many turns it lasts</param>
    public ApplyDefenseUpGA(List<CombatantView> targets, int defensePercentage, int duration)
    {
        if (targets == null || targets.Count == 0)
            throw new System.ArgumentException("Targets cannot be null or empty", nameof(targets));
        if (defensePercentage <= 0 || defensePercentage > 90)
            throw new System.ArgumentException("Defense percentage must be between 1 and 90", nameof(defensePercentage));
        if (duration <= 0)
            throw new System.ArgumentException("Duration must be positive", nameof(duration));

        Targets = new List<CombatantView>(targets);
        DefensePercentage = defensePercentage;
        Duration = duration;
    }
}
