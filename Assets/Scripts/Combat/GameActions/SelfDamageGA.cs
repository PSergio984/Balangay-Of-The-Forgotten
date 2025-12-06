/*
 * SELF DAMAGE GAME ACTION DOCUMENTATION
 * 
 * How it works:
 * This action deals damage to the caster (self-damage) for abilities that
 * have an HP cost. Can be flat damage, percentage of current HP, or percentage
 * of max HP. Bypasses armor and most defensive effects.
 * 
 * Design reasoning:
 * Many abilities have an HP cost as a balancing mechanic. This action handles
 * that cleanly without triggering damage reactions that would apply to enemy damage.
 * Self-damage is intentional and shouldn't be blocked by shields.
 * 
 * Integration:
 * - Created by SelfDamageEffect from abilities with HP costs
 * - Processed by SelfDamageSystem to apply damage directly
 * - Does NOT trigger on-damage perks (it's a cost, not an attack)
 * 
 * Used by:
 * - Guardian's Oath (Bagani): Sacrifice 25% current HP
 * - Sacrifice (Support): Lose 200 HP
 * - Daybreak Fury (Apolaki): Cost 30% of current HP
 */

using UnityEngine;

/// <summary>
/// Game action that deals damage to the caster as an ability cost
/// </summary>
/// <remarks>
/// This is different from DealDamageGA - self-damage bypasses shields
/// and defensive reactions since it's an intentional cost, not an attack.
/// </remarks>
public class SelfDamageGA : GameAction
{
    /// <summary>
    /// The combatant who takes the self-damage
    /// </summary>
    public CombatantView Target { get; private set; }
    
    /// <summary>
    /// Flat damage amount (added to percentage-based damage)
    /// </summary>
    public float FlatDamage { get; private set; }
    
    /// <summary>
    /// Percentage of current HP to lose (0.25 = 25%)
    /// </summary>
    public float PercentOfCurrentHP { get; private set; }
    
    /// <summary>
    /// Percentage of max HP to lose (0.1 = 10%)
    /// </summary>
    public float PercentOfMaxHP { get; private set; }

    /// <summary>
    /// Creates a self-damage action with all damage types
    /// </summary>
    /// <param name="target">Who takes the self-damage (usually the caster)</param>
    /// <param name="flatDamage">Flat HP loss</param>
    /// <param name="percentOfCurrentHP">Percentage of current HP to lose</param>
    /// <param name="percentOfMaxHP">Percentage of max HP to lose</param>
    public SelfDamageGA(CombatantView target, float flatDamage, float percentOfCurrentHP, float percentOfMaxHP)
    {
        if (target == null)
            throw new System.ArgumentNullException(nameof(target), "Target cannot be null.");
        if (flatDamage < 0f)
            throw new System.ArgumentOutOfRangeException(nameof(flatDamage), "Flat damage cannot be negative.");
        if (percentOfCurrentHP < 0f || percentOfCurrentHP > 1f)
            throw new System.ArgumentOutOfRangeException(nameof(percentOfCurrentHP), "Percent of current HP must be between 0 and 1.");
        if (percentOfMaxHP < 0f || percentOfMaxHP > 1f)
            throw new System.ArgumentOutOfRangeException(nameof(percentOfMaxHP), "Percent of max HP must be between 0 and 1.");
        Target = target;
        FlatDamage = flatDamage;
        PercentOfCurrentHP = percentOfCurrentHP;
        PercentOfMaxHP = percentOfMaxHP;
    }    
    /// <summary>
    /// Creates a self-damage action with just flat damage
    /// </summary>
    /// <param name="target">Who takes the self-damage</param>
    /// <param name="flatDamage">Flat HP loss</param>
    public SelfDamageGA(CombatantView target, float flatDamage)
    {
        if (target == null)
            throw new System.ArgumentNullException(nameof(target), "Target cannot be null.");
        if (flatDamage < 0f)
            throw new System.ArgumentOutOfRangeException(nameof(flatDamage), "Flat damage cannot be negative.");
        Target = target;
        FlatDamage = flatDamage;
        PercentOfCurrentHP = 0f;
        PercentOfMaxHP = 0f;
    }
    
    /// <summary>
    /// Calculates the total damage to deal based on target's current stats
    /// </summary>
    /// <returns>Total damage amount as integer</returns>
    public int CalculateTotalDamage()
    {
        float total = FlatDamage;
        total += Target.CurrentHealth * PercentOfCurrentHP;
        total += Target.MaxHealth * PercentOfMaxHP;
        return Mathf.RoundToInt(total);
    }
}
