/*
 * APPLY ATTACK DOWN GAME ACTION DOCUMENTATION
 * 
 * How it works:
 * This action applies ATTACK_DOWN status (debuff) with separate percentage and duration tracking.
 * The percentage determines attack reduction, duration determines how long it lasts.
 * 
 * Design reasoning:
 * Separates attack reduction from duration for clarity and balance.
 * This allows "-30% attack for 2 turns" to work as expected.
 * 
 * Integration:
 * - Created by ApplyAttackDownEffect
 * - Processed by AttackDownSystem
 * - Duration tracked via dictionary and ticked down by StatusEffectTickSystem
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game action that applies ATTACK_DOWN with configurable percentage and duration
/// </summary>
public class ApplyAttackDownGA : GameAction
{
    /// <summary>
    /// List of combatants who will receive the attack debuff
    /// </summary>
    public List<CombatantView> Targets { get; private set; }
    
    /// <summary>
    /// The percentage of attack decrease (e.g., 30 = -30% attack damage)
    /// </summary>
    public int AttackPercentage { get; private set; }
    
    /// <summary>
    /// How many turns the attack debuff lasts before expiring
    /// </summary>
    public int Duration { get; private set; }

    /// <summary>
    /// Creates a new attack down action
    /// </summary>
    /// <param name="targets">Who should receive the attack debuff</param>
    /// <param name="attackPercentage">Percentage of attack decrease (1-90)</param>
    /// <param name="duration">How many turns it lasts</param>
    public ApplyAttackDownGA(List<CombatantView> targets, int attackPercentage, int duration)
    {
        if (targets == null || targets.Count == 0)
            throw new System.ArgumentException("Targets cannot be null or empty", nameof(targets));
        if (attackPercentage <= 0 || attackPercentage > 90)
            throw new System.ArgumentException("Attack percentage must be between 1 and 90", nameof(attackPercentage));
        if (duration <= 0)
            throw new System.ArgumentException("Duration must be positive", nameof(duration));

        Targets = new List<CombatantView>(targets);
        AttackPercentage = attackPercentage;
        Duration = duration;
    }
}
