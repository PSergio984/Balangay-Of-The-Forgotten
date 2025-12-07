/*
 * APPLY DAMAGE UP GAME ACTION DOCUMENTATION
 * 
 * How it works:
 * This action applies DMG_UP status with separate percentage and duration tracking.
 * The percentage determines damage amplification, duration determines how long it lasts.
 * Stores duration metadata so StatusEffectTickSystem can count down properly.
 * 
 * Design reasoning:
 * Separates damage strength from duration for clarity and balance.
 * Uses metadata to track duration since status effect stacks represent percentage.
 * This allows "+20% damage for 2 turns" to work as expected.
 * 
 * Integration:
 * - Created by ApplyDamageUpEffect
 * - Processed by StatusEffectSystem/DamageUpSystem
 * - Duration tracked via metadata and ticked down by StatusEffectTickSystem
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game action that applies DMG_UP with configurable percentage and duration
/// </summary>
public class ApplyDamageUpGA : GameAction
{
    /// <summary>
    /// List of combatants who will receive the damage buff
    /// </summary>
    public List<CombatantView> Targets { get; private set; }
    
    /// <summary>
    /// The percentage of damage increase (e.g., 20 = +20% damage)
    /// This becomes the stack count in the status effect system
    /// </summary>
    public int DamagePercentage { get; private set; }
    
    /// <summary>
    /// How many turns the damage buff lasts before expiring
    /// </summary>
    public int Duration { get; private set; }

    /// <summary>
    /// Creates a new damage up action
    /// </summary>
    /// <param name="targets">Who should receive the damage buff</param>
    /// <param name="damagePercentage">Percentage of damage increase (1-100)</param>
    /// <param name="duration">How many turns it lasts</param>
    public ApplyDamageUpGA(List<CombatantView> targets, int damagePercentage, int duration)
    {
        if (targets == null || targets.Count == 0)
            throw new System.ArgumentException("Targets cannot be null or empty", nameof(targets));
        if (damagePercentage <= 0 || damagePercentage > 100)
            throw new System.ArgumentException("Damage percentage must be between 1 and 100", nameof(damagePercentage));
        if (duration <= 0)
            throw new System.ArgumentException("Duration must be positive", nameof(duration));

        Targets = new List<CombatantView>(targets);
        DamagePercentage = damagePercentage;
        Duration = duration;
    }
}
