/*
 * LIFESTEAL DAMAGE GAME ACTION DOCUMENTATION
 * 
 * How it works:
 * This action deals damage to a target and heals the caster for a percentage
 * of the damage dealt. Combines damage and healing in one action for lifesteal
 * abilities like Eclipse Fang.
 * 
 * Design reasoning:
 * Lifesteal is a common mechanic that needs to calculate heal AFTER damage.
 * This ensures the heal is based on actual damage dealt (after armor/shields).
 * The action chains DealDamageGA and HealGA for proper processing.
 * 
 * Integration:
 * - Created by LifestealEffect from lifesteal abilities
 * - Processed by LifestealSystem to deal damage then heal
 * - Heal amount is based on final damage dealt, not raw damage
 * 
 * Used by:
 * - Eclipse Fang (Bakunawa): 110% MAG damage, heals for 50 (+100% MAG)
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game action that deals damage and heals the caster based on damage dealt
/// </summary>
public class LifestealDamageGA : GameAction, IHaveCaster
{
    /// <summary>
    /// The base damage amount before calculations
    /// </summary>
    public float DamageAmount { get; private set; }
    
    /// <summary>
    /// The target who receives the damage
    /// </summary>
    public CombatantView Target { get; private set; }
    
    /// <summary>
    /// Who is dealing the damage and receiving the heal
    /// </summary>
    public CombatantView Caster { get; set; }
    
    /// <summary>
    /// Flat heal amount that doesn't depend on damage
    /// </summary>
    public float FlatHeal { get; private set; }
    
    /// <summary>
    /// Multiplier for heal based on caster's magic power
    /// </summary>
    public float HealMagicAmp { get; private set; }
    
    /// <summary>
    /// Percentage of damage dealt to heal (1.0 = 100% lifesteal)
    /// </summary>
    public float LifestealPercent { get; private set; }

    /// <summary>
    /// Creates a lifesteal damage action
    /// </summary>
    /// <param name="damageAmount">Base damage to deal</param>
    /// <param name="target">Who receives the damage</param>
    /// <param name="caster">Who deals damage and receives heal</param>
    /// <param name="flatHeal">Flat heal amount</param>
    /// <param name="healMagicAmp">Magic scaling for heal</param>
    /// <param name="lifestealPercent">Percentage of damage to heal</param>
    public LifestealDamageGA(float damageAmount, CombatantView target, CombatantView caster, 
        float flatHeal = 0f, float healMagicAmp = 0f, float lifestealPercent = 0f)
    {
        if (caster == null)
        {
            Debug.LogError("LifestealDamageGA: Caster cannot be null. Lifesteal actions require a valid caster.");
            throw new System.ArgumentNullException(nameof(caster), "Caster cannot be null for LifestealDamageGA.");
        }
        DamageAmount = damageAmount;
        Target = target;
        Caster = caster;
        FlatHeal = flatHeal;
        HealMagicAmp = healMagicAmp;
        LifestealPercent = lifestealPercent;
    }
    
    /// <summary>
    /// Calculates the total heal amount based on caster stats and damage dealt
    /// </summary>
    /// <param name="actualDamageDealt">The final damage that was dealt after reductions</param>
    /// <returns>Total heal amount</returns>
    public float CalculateHealAmount(float actualDamageDealt)
    {
        // Caster is enforced non-null in constructor
        float heal = FlatHeal;
        heal += HealMagicAmp * Caster.MagicPower;
        heal += actualDamageDealt * LifestealPercent;
        return heal;
    }
}