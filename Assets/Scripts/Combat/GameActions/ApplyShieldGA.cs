/*
 * APPLY SHIELD GAME ACTION DOCUMENTATION
 * 
 * How it works:
 * This action applies a shield (temporary HP) to target combatants.
 * Shields absorb damage before actual HP is reduced.
 * Unlike armor, shields have a duration and expire after set turns.
 * 
 * Design reasoning:
 * Shields are different from armor - they're temporary HP with duration.
 * This allows for strategic defensive abilities that expire over time.
 * The ShieldStatusEffectSystem handles damage absorption.
 * 
 * Integration:
 * - Created by ApplyShieldEffect from defensive abilities
 * - Processed by ShieldStatusEffectSystem to apply shield
 * - Damage system checks shield before applying HP damage
 * 
 * Used by:
 * - Fortify (Bagani): +30% max HP shield for 2 turns
 * - Guardian's Oath (Bagani): Shield allies for 25% of caster's current HP
 */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// Represents a shield target with specific shield amount
/// </summary>
public struct ShieldTarget
{
    /// <summary>
    /// The combatant who will receive the shield
    /// </summary>
    public CombatantView Target;
    
    /// <summary>
    /// The amount of shield HP to apply
    /// </summary>
    public int Amount;
    
    /// <summary>
    /// Creates a new shield target
    /// </summary>
    public ShieldTarget(CombatantView target, int amount)
    {
        if (target == null)
            throw new System.ArgumentNullException(nameof(target), "Shield target cannot be null");
        if (amount <= 0)
            throw new System.ArgumentException("Shield amount must be positive", nameof(amount));
        
        Target = target;
        Amount = amount;
    }
}
/// <summary>
/// Game action that applies shield (temporary HP with duration) to targets
/// </summary>
public class ApplyShieldGA : GameAction, IHaveCaster
{
    /// <summary>
    /// List of targets and their shield amounts
    /// </summary>
    public List<ShieldTarget> ShieldTargets { get; private set; }
    
    /// <summary>
    /// How many turns the shield lasts before expiring
    /// </summary>
    public int Duration { get; private set; }
    
    /// <summary>
    /// Who created this shield (for perk system tracking)
    /// </summary>
    public CombatantView Caster { get; set; }
    
    /// <summary>
    /// Whether this shield can stack with existing shields
    /// </summary>
    public bool CanStack { get; private set; }

    /// <summary>
    /// Creates a shield action with multiple targets and amounts
    /// </summary>
    /// <param name="shieldTargets">List of targets with their shield amounts</param>
    /// <param name="duration">How many turns the shield lasts</param>
    /// <param name="caster">Who is creating the shield</param>
    /// <param name="canStack">Whether shields can stack (default: true)</param>
    public ApplyShieldGA(List<ShieldTarget> shieldTargets, int duration, CombatantView caster, bool canStack = true)
    {
        if (shieldTargets == null)
            throw new System.ArgumentNullException(nameof(shieldTargets));
        if (shieldTargets.Count == 0)
            throw new System.ArgumentException("Shield targets list cannot be empty", nameof(shieldTargets));
        if (duration <= 0)
            throw new System.ArgumentException("Duration must be positive", nameof(duration));
        if (caster == null)
            throw new System.ArgumentNullException(nameof(caster));

        for (int i = 0; i < shieldTargets.Count; i++)
        {
            var entry = shieldTargets[i];
            if (entry.Target == null)
                throw new System.ArgumentException($"ShieldTargets[{i}].Target is null", nameof(shieldTargets));
            if (entry.Amount <= 0)
                throw new System.ArgumentException($"ShieldTargets[{i}].Amount must be positive (got {entry.Amount})", nameof(shieldTargets));
        }

        ShieldTargets = new List<ShieldTarget>(shieldTargets);
        Duration = duration;
        Caster = caster;
        CanStack = canStack;
    }
    /// <summary>
    /// Creates a shield action with uniform shield amount for all targets
    /// </summary>
    /// <param name="targets">Who should receive shields</param>
    /// <param name="shieldAmount">How much shield each target gets</param>
    /// <param name="duration">How many turns the shield lasts</param>
    /// <param name="caster">Who is creating the shield</param>
    /// <param name="canStack">Whether shields can stack (default: true)</param>
    public ApplyShieldGA(List<CombatantView> targets, int shieldAmount, int duration, CombatantView caster, bool canStack = true)
    {
        if (targets == null)
            throw new System.ArgumentNullException(nameof(targets));
        if (targets.Count == 0)
            throw new System.ArgumentException("Targets list cannot be empty", nameof(targets));
        if (targets.Any(t => t == null))
            throw new System.ArgumentException("Targets list contains null element(s)", nameof(targets));
        if (shieldAmount <= 0)
            throw new System.ArgumentOutOfRangeException(nameof(shieldAmount), "Shield amount must be greater than zero");
        if (duration <= 0)
            throw new System.ArgumentOutOfRangeException(nameof(duration), "Duration must be greater than zero");
        if (caster == null)
            throw new System.ArgumentNullException(nameof(caster));

        ShieldTargets = new List<ShieldTarget>();
        foreach (var target in targets)
        {
            ShieldTargets.Add(new ShieldTarget(target, shieldAmount));
        }
        Duration = duration;
        Caster = caster;
        CanStack = canStack;
    }
}
