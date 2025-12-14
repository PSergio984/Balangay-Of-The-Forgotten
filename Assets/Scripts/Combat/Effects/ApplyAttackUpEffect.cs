/*
 * APPLY ATTACK UP EFFECT DOCUMENTATION
 * 
 * How it works:
 * This effect applies ATTACK_UP status with configurable percentage and duration.
 * Unlike the basic AddStatusEffectEffect which uses stacks for both strength and duration,
 * this effect separates the two concepts for clearer configuration.
 * 
 * Design reasoning:
 * Separating attack percentage from duration makes cards easier to balance.
 * "+40% attack for 2 turns" is clearer than "40 stacks that tick down".
 * The percentage directly controls damage increase, duration controls how long it lasts.
 * 
 * Integration:
 * - Used by cards/abilities that grant attack buffs
 * - Creates ApplyAttackUpGA with percentage and duration
 * - AttackUpSystem handles the damage amplification
 * - StatusEffectTickSystem handles the duration countdown
 */

using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Effect that applies ATTACK_UP with configurable percentage and duration
/// </summary>
[System.Serializable]
public class ApplyAttackUpEffect : Effects
{
    /// <summary>
    /// The percentage of attack increase (e.g., 40 = +40% attack damage)
    /// </summary>
    [SerializeField]
    [LabelText("Attack Increase (%)")]
    [Range(1, 100)]
    [InfoBox("@\"Targets will deal \" + attackPercentage + \"% more attack damage\"", InfoMessageType.None)]
    private int attackPercentage = 40;
    
    /// <summary>
    /// How many turns the attack buff lasts
    /// </summary>
    [SerializeField]
    [LabelText("Duration (Turns)")]
    [Range(1, 10)]
    [InfoBox("@\"Attack buff lasts for \" + duration + \" turn(s)\"", InfoMessageType.None)]
    private int duration = 2;

    /// <summary>
    /// Creates a game action that applies attack buff to targets
    /// </summary>
    /// <param name="targets">Who should receive the attack buff</param>
    /// <param name="caster">Who is causing this effect (for perk system tracking)</param>
    /// <returns>ApplyAttackUpGA action ready to be processed</returns>
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning("[ApplyAttackUpEffect] No targets provided!");
            return null;
        }

        return new ApplyAttackUpGA(targets, attackPercentage, duration);
    }
}
