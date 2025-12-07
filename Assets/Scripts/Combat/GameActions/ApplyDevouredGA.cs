using System.Collections.Generic;
using UnityEngine;

/* APPLY DEVOURED GAME ACTION DOCUMENTATION
 * 
 * How it works:
 * This action applies DEVOURED status with separate MAG percentage and duration tracking.
 * The MAG percentage determines DoT damage, duration determines how long it lasts.
 * Stores damage metadata so DevouredSystem can calculate DoT properly.
 * 
 * Design reasoning:
 * Separates damage strength from duration for clarity and balance.
 * Uses metadata to track MAG percentage and fixed damage since status effect stacks represent duration.
 * This allows "20% MAG damage for 2 turns" to work as expected.
 * 
 * Integration:
 * - Created by ApplyDevouredEffect
 * - Processed by DevouredSystem
 * - Duration tracked via metadata and ticked down by StatusEffectTickSystem
 * - Damage calculated based on MAG percentage or fixed damage each turn
 */

/// <summary>
/// Game action that applies DEVOURED with configurable MAG percentage and duration
/// </summary>
public class ApplyDevouredGA : GameAction
{
    /// <summary>
    /// List of combatants who will receive the Devoured debuff
    /// </summary>
    public List<CombatantView> Targets { get; private set; }
    
    /// <summary>
    /// The percentage of MAG to deal as damage per turn (e.g., 20 = 20% of MAG)
    /// </summary>
    public int MagDamagePercentage { get; private set; }
    
    /// <summary>
    /// Fixed damage amount (overrides MAG percentage if > 0)
    /// </summary>
    public int FixedDamage { get; private set; }
    
    /// <summary>
    /// How many turns the Devoured debuff lasts before expiring
    /// </summary>
    public int Duration { get; private set; }
    
    /// <summary>
    /// The caster who applied this debuff (for MAG calculation)
    /// </summary>
    public CombatantView Caster { get; private set; }

    /// <summary>
    /// Creates a new Devoured action
    /// </summary>
    /// <param name="targets">Who should receive the Devoured debuff</param>
    /// <param name="magDamagePercentage">Percentage of MAG to deal as damage (1-100)</param>
    /// <param name="fixedDamage">Fixed damage amount (0 = use MAG%, >0 = use fixed)</param>
    /// <param name="duration">How many turns it lasts</param>
    /// <param name="caster">The caster who applied this (for MAG calculation)</param>
    public ApplyDevouredGA(List<CombatantView> targets, int magDamagePercentage, int fixedDamage, int duration, CombatantView caster)
    {
        Targets = targets ?? new List<CombatantView>();
        MagDamagePercentage = magDamagePercentage;
        FixedDamage = fixedDamage;
        Duration = duration;
        Caster = caster;
    }
}
