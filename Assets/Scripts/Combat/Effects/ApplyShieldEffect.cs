/*
 * APPLY SHIELD EFFECT DOCUMENTATION
 * 
 * How it works:
 * This effect applies a temporary shield (extra HP) to targets.
 * Shield amount can be flat, percentage of caster's HP, or percentage of target's HP.
 * Shields have a duration and expire after the specified number of turns.
 * 
 * Design reasoning:
 * Shields are a defensive mechanic that's different from armor:
 * - Armor is consumed 1:1 with damage
 * - Shields have a total HP pool that expires over time
 * This allows for strategic timing of defensive abilities.
 * 
 * Integration:
 * - Used by defensive abilities like Fortify and Guardian's Oath
 * - Creates ApplyShieldGA for processing by ShieldStatusEffectSystem
 * - Shield tracking managed by individual CombatantView
 * 
 * Used by:
 * - Fortify (Bagani): Gain shield equal to +30% max HP for 2 turns
 * - Guardian's Oath (Bagani): Shield allies for 25% of caster's current HP
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Effect that applies temporary shield HP to targets
/// </summary>
[System.Serializable]
public class ApplyShieldEffect : Effects
{
    /// <summary>
    /// Flat shield amount added to all targets
    /// </summary>
    [SerializeField]
    [Tooltip("Flat shield amount added to the total")]
    private float flatShieldAmount = 0f;
    
    /// <summary>
    /// Shield amount based on percentage of target's max HP
    /// </summary>
    [SerializeField]
    [Tooltip("Shield as percentage of target's max HP (0.3 = 30%)")]
    [Range(0f, 2f)]
    private float percentOfTargetMaxHP = 0f;
    
    /// <summary>
    /// Shield amount based on percentage of caster's current HP
    /// </summary>
    [SerializeField]
    [Tooltip("Shield as percentage of caster's current HP (0.25 = 25%)")]
    [Range(0f, 2f)]
    private float percentOfCasterCurrentHP = 0f;
    
    /// <summary>
    /// Shield amount based on percentage of caster's max HP
    /// </summary>
    [SerializeField]
    [Tooltip("Shield as percentage of caster's max HP (0.3 = 30%)")]
    [Range(0f, 2f)]
    private float percentOfCasterMaxHP = 0f;
    
    /// <summary>
    /// How many turns the shield lasts before expiring
    /// </summary>
    [SerializeField]
    [Tooltip("How many turns the shield lasts")]
    [Min(1)]
    private int duration = 2;
    
    /// <summary>
    /// Whether this shield can stack with existing shields
    /// </summary>
    [SerializeField]
    [Tooltip("Can this shield stack with existing shields?")]
    private bool canStack = true;
    
    /// <summary>
    /// Whether to exclude the caster from receiving the shield
    /// </summary>
    [SerializeField]
    [Tooltip("Exclude caster from receiving shield (for Guardian's Oath)")]
    private bool excludeCaster = false;

    /// <summary>
    /// Creates a shield action for the specified targets
    /// </summary>
    /// <param name="targets">Who should receive shields</param>
    /// <param name="caster">Who is creating the shields</param>
    /// <returns>ApplyShieldGA with calculated shield amounts</returns>
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        List<ShieldTarget> shieldTargets = new List<ShieldTarget>();
        
        // Calculate caster-based shield components once
        float casterBasedShield = 0f;
        if (caster != null)
        {
            casterBasedShield += caster.CurrentHealth * percentOfCasterCurrentHP;
            casterBasedShield += caster.MaxHealth * percentOfCasterMaxHP;
        }
        
        foreach (var target in targets)
        {
            if (target == null) continue;
            if (excludeCaster && target == caster) continue;
            
            // Calculate total shield for this target
            float shieldAmount = flatShieldAmount;
            shieldAmount += casterBasedShield;
            shieldAmount += target.MaxHealth * percentOfTargetMaxHP;
            
            int finalShield = Mathf.RoundToInt(shieldAmount);
            if (finalShield > 0)
            {
                shieldTargets.Add(new ShieldTarget(target, finalShield));
                Debug.Log($"[ApplyShieldEffect] {target.name} receives {finalShield} shield for {duration} turns");
            }
        }
        
        if (shieldTargets.Count == 0)
        {
            Debug.Log("[ApplyShieldEffect] No valid shield targets.");
            return null;
        }
        
        return new ApplyShieldGA(shieldTargets, duration, caster, canStack);
    }
}
