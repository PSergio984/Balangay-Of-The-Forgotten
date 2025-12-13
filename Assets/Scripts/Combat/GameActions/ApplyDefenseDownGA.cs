/*
 * APPLY DEFENSE DOWN GAME ACTION DOCUMENTATION
 * 
 * How it works:
 * This action applies DEFENSE_DOWN status (debuff) with separate percentage and duration tracking.
 * The percentage determines defense reduction, duration determines how long it lasts.
 * 
 * Design reasoning:
 * Separates defense reduction from duration for clarity and balance.
 * This allows "-30% defense for 2 turns" to work as expected.
 * 
 * Integration:
 * - Created by ApplyDefenseDownEffect
 * - Processed by DefenseDownSystem
 * - Duration tracked via dictionary and ticked down by StatusEffectTickSystem
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game action that applies DEFENSE_DOWN with configurable percentage and duration
/// </summary>
public class ApplyDefenseDownGA : GameAction
{
    /// <summary>
    /// List of combatants who will receive the defense debuff
    /// </summary>
    public List<CombatantView> Targets { get; private set; }
    
    /// <summary>
    /// The percentage of defense decrease (e.g., 30 = -30% defense)
    /// </summary>
    public int DefensePercentage { get; private set; }
    
    /// <summary>
    /// How many turns the defense debuff lasts before expiring
    /// </summary>
    public int Duration { get; private set; }
    
    /// <summary>
    /// Optional custom name for this effect (e.g., "Bonecracked", "Moonfall", "Bind")
    /// Used to display consolidated move sprites instead of default effect sprites
    /// </summary>
    public string CustomName { get; private set; }

    /// <summary>
    /// Creates a new defense down action
    /// </summary>
    /// <param name="targets">Who should receive the defense debuff</param>
    /// <param name="defensePercentage">Percentage of defense decrease (1-90)</param>
    /// <param name="duration">How many turns it lasts</param>
    /// <param name="customName">Optional custom name for consolidated sprite display (e.g., "Bonecracked", "Moonfall")</param>
    public ApplyDefenseDownGA(List<CombatantView> targets, int defensePercentage, int duration, string customName = null)
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
        CustomName = customName;
    }
}
