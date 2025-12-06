/*
 * SELF DAMAGE EFFECT DOCUMENTATION
 * 
 * How it works:
 * This effect causes the caster to take self-damage as a cost for using an ability.
 * Damage can be flat, percentage of current HP, or percentage of max HP.
 * Self-damage bypasses shields and defensive reactions.
 * 
 * Design reasoning:
 * HP costs are a common balancing mechanic for powerful abilities.
 * Self-damage is intentional, so it shouldn't trigger damage reactions or be blocked.
 * This effect is typically added to the PreReactions or PerformReactions of an ability.
 * 
 * Integration:
 * - Used by abilities with HP costs (Sacrifice, Guardian's Oath, Daybreak Fury)
 * - Creates SelfDamageGA for processing by SelfDamageSystem
 * - Does not trigger on-damage perks or reactions
 * 
 * Used by:
 * - Sacrifice (Support): Lose 200 HP
 * - Guardian's Oath (Bagani): Sacrifice 25% current HP
 * - Daybreak Fury (Apolaki): Cost 30% of current HP
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Effect that causes the caster to take self-damage as an ability cost
/// </summary>
[System.Serializable]
public class SelfDamageEffect : Effects
{
    /// <summary>
    /// Flat damage amount to take
    /// </summary>
    [SerializeField]
    [Tooltip("Flat HP cost")]
    private float flatDamage = 0f;
    
    /// <summary>
    /// Percentage of current HP to lose
    /// </summary>
    [SerializeField]
    [Tooltip("Percentage of current HP to lose (0.25 = 25%)")]
    [Range(0f, 1f)]
    private float percentOfCurrentHP = 0f;
    
    /// <summary>
    /// Percentage of max HP to lose
    /// </summary>
    [SerializeField]
    [Tooltip("Percentage of max HP to lose (0.1 = 10%)")]
    [Range(0f, 1f)]
    private float percentOfMaxHP = 0f;

    /// <summary>
    /// Creates a self-damage action for the caster
    /// </summary>
    /// <param name="targets">Ignored - self-damage always affects caster</param>
    /// <param name="caster">Who takes the self-damage</param>
    /// <returns>SelfDamageGA targeting the caster</returns>
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        if (caster == null)
        {
            Debug.LogWarning("[SelfDamageEffect] Caster is null, cannot apply self-damage.");
            return null;
        }
        
        // Validate that at least some damage is configured
        if (flatDamage <= 0 && percentOfCurrentHP <= 0 && percentOfMaxHP <= 0)
        {
            Debug.LogWarning("[SelfDamageEffect] No damage configured for self-damage effect.");
            return null;
        }
        
        var action = new SelfDamageGA(caster, flatDamage, percentOfCurrentHP, percentOfMaxHP);
        Debug.Log($"[SelfDamageEffect] {caster.name} will take {action.CalculateTotalDamage()} self-damage");
        
        return action;
    }
}
