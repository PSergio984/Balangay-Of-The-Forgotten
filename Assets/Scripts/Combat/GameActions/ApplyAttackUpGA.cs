/*
 * APPLY ATTACK UP GAME ACTION DOCUMENTATION
 * 
 * How it works:
 * This action applies ATTACK_UP status with separate percentage and duration tracking.
 * The percentage determines attack amplification, duration determines how long it lasts.
 * Stores duration metadata so StatusEffectTickSystem can count down properly.
 * 
 * Design reasoning:
 * Separates attack strength from duration for clarity and balance.
 * Uses metadata to track duration since status effect stacks represent percentage.
 * This allows "+40% attack for 2 turns" to work as expected.
 * 
 * Integration:
 * - Created by ApplyAttackUpEffect
 * - Processed by AttackUpSystem
 * - Duration tracked via dictionary and ticked down by StatusEffectTickSystem
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game action that applies ATTACK_UP with configurable percentage and duration
/// </summary>
public class ApplyAttackUpGA : GameAction
{
    /// <summary>
    /// List of combatants who will receive the attack buff
    /// </summary>
    public List<CombatantView> Targets { get; private set; }
    
    /// <summary>
    /// The percentage of attack increase (e.g., 40 = +40% attack damage)
    /// This becomes the stack count in the status effect system
    /// </summary>
    public int AttackPercentage { get; private set; }
    
    /// <summary>
    /// How many turns the attack buff lasts before expiring
    /// </summary>
    public int Duration { get; private set; }

    /// <summary>
    /// Creates a new attack up action
    /// </summary>
    /// <param name="targets">Who should receive the attack buff</param>
    /// <param name="attackPercentage">Percentage of attack increase (1-100)</param>
    /// <param name="duration">How many turns it lasts</param>
    public ApplyAttackUpGA(List<CombatantView> targets, int attackPercentage, int duration)
    {
        if (targets == null || targets.Count == 0)
            throw new System.ArgumentException("Targets cannot be null or empty", nameof(targets));
        if (attackPercentage <= 0 || attackPercentage > 100)
            throw new System.ArgumentException("Attack percentage must be between 1 and 100", nameof(attackPercentage));
        if (duration <= 0)
            throw new System.ArgumentException("Duration must be positive", nameof(duration));

        Targets = new List<CombatantView>(targets);
        AttackPercentage = attackPercentage;
        Duration = duration;
    }
}
