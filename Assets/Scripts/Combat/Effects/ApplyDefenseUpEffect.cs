/*
 * APPLY DEFENSE UP EFFECT DOCUMENTATION
 * 
 * How it works:
 * This effect applies DEFENSE_UP status with configurable percentage and duration.
 * Unlike the basic AddStatusEffectEffect which uses stacks for both strength and duration,
 * this effect separates the two concepts for clearer configuration.
 * 
 * Design reasoning:
 * Separating defense percentage from duration makes cards easier to balance.
 * "+50% defense for 3 turns" is clearer than "50 stacks that tick down".
 * The percentage directly controls damage reduction, duration controls how long it lasts.
 * 
 * Integration:
 * - Used by cards/abilities that grant defense buffs
 * - Creates ApplyDefenseUpGA with percentage and duration
 * - DefenseUpSystem handles the damage reduction
 * - StatusEffectTickSystem handles the duration countdown
 */

using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Effect that applies DEFENSE_UP with configurable percentage and duration
/// </summary>
[System.Serializable]
public class ApplyDefenseUpEffect : Effects
{
    /// <summary>
    /// The percentage of defense increase (e.g., 50 = 50% damage reduction)
    /// </summary>
    [SerializeField]
    [LabelText("Defense Increase (%)")]
    [Range(1, 90)]
    [InfoBox("@\"Targets will take \" + defensePercentage + \"% less damage\"", InfoMessageType.None)]
    private int defensePercentage = 50;
    
    /// <summary>
    /// How many turns the defense buff lasts
    /// </summary>
    [SerializeField]
    [LabelText("Duration (Turns)")]
    [Range(1, 10)]
    [InfoBox("@\"Defense buff lasts for \" + duration + \" turn(s)\"", InfoMessageType.None)]
    private int duration = 3;

    /// <summary>
    /// Creates a game action that applies defense buff to targets
    /// </summary>
    /// <param name="targets">Who should receive the defense buff</param>
    /// <param name="caster">Who is causing this effect (for perk system tracking)</param>
    /// <returns>ApplyDefenseUpGA action ready to be processed</returns>
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning("[ApplyDefenseUpEffect] No targets provided!");
            return null;
        }

        return new ApplyDefenseUpGA(targets, defensePercentage, duration);
    }
}
