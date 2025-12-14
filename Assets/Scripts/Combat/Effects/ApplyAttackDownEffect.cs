/*
 * APPLY ATTACK DOWN EFFECT DOCUMENTATION
 * 
 * How it works:
 * This effect applies ATTACK_DOWN status (debuff) with configurable percentage and duration.
 * Reduces target's attack damage by the specified percentage for the given duration.
 * 
 * Design reasoning:
 * Separating attack reduction from duration makes debuffs easier to balance.
 * "-30% attack for 2 turns" is clearer than "30 stacks that tick down".
 * 
 * Integration:
 * - Used by cards/abilities that apply attack debuffs
 * - Creates ApplyAttackDownGA with percentage and duration
 * - AttackDownSystem handles the damage reduction
 * - StatusEffectTickSystem handles the duration countdown
 */

using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Effect that applies ATTACK_DOWN with configurable percentage and duration
/// </summary>
[System.Serializable]
public class ApplyAttackDownEffect : Effects
{
    /// <summary>
    /// The percentage of attack decrease (e.g., 30 = -30% attack damage)
    /// </summary>
    [SerializeField]
    [LabelText("Attack Decrease (%)")]
    [Range(1, 90)]
    [InfoBox("@\"Targets will deal \" + attackPercentage + \"% less attack damage\"", InfoMessageType.None)]
    private int attackPercentage = 30;
    
    /// <summary>
    /// How many turns the attack debuff lasts
    /// </summary>
    [SerializeField]
    [LabelText("Duration (Turns)")]
    [Range(1, 10)]
    [InfoBox("@\"Attack debuff lasts for \" + duration + \" turn(s)\"", InfoMessageType.None)]
    private int duration = 2;

    /// <summary>
    /// Creates a game action that applies attack debuff to targets
    /// </summary>
    /// <param name="targets">Who should receive the attack debuff</param>
    /// <param name="caster">Who is causing this effect (for perk system tracking)</param>
    /// <returns>ApplyAttackDownGA action ready to be processed</returns>
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning("[ApplyAttackDownEffect] No targets provided!");
            return null;
        }

        return new ApplyAttackDownGA(targets, attackPercentage, duration);
    }
}
