/*
 * APPLY CRIT UP EFFECT DOCUMENTATION
 * 
 * How it works:
 * This effect applies CRIT_UP status with configurable percentage and duration.
 * Increases target's critical hit chance by the specified percentage for the given duration.
 * 
 * Design reasoning:
 * Separating crit chance from duration makes cards easier to balance.
 * "+55% crit chance for 1 turn" is clearer than "55 stacks that tick down".
 * 
 * Integration:
 * - Used by cards/abilities that grant crit buffs
 * - Creates ApplyCritUpGA with percentage and duration
 * - CritUpSystem handles the crit chance calculation
 * - StatusEffectTickSystem handles the duration countdown
 */

using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Effect that applies CRIT_UP with configurable percentage and duration
/// </summary>
[System.Serializable]
public class ApplyCritUpEffect : Effects
{
    /// <summary>
    /// The percentage of crit chance increase (e.g., 55 = +55% crit chance)
    /// </summary>
    [SerializeField]
    [LabelText("Crit Chance Increase (%)")]
    [Range(1, 100)]
    [InfoBox("@\"Targets gain +\" + critPercentage + \"% critical hit chance\"", InfoMessageType.None)]
    private int critPercentage = 55;
    
    /// <summary>
    /// How many turns the crit buff lasts
    /// </summary>
    [SerializeField]
    [LabelText("Duration (Turns)")]
    [Range(1, 10)]
    [InfoBox("@\"Crit buff lasts for \" + duration + \" turn(s)\"", InfoMessageType.None)]
    private int duration = 1;

    /// <summary>
    /// Creates a game action that applies crit buff to targets
    /// </summary>
    /// <param name="targets">Who should receive the crit buff</param>
    /// <param name="caster">Who is causing this effect (for perk system tracking)</param>
    /// <returns>ApplyCritUpGA action ready to be processed</returns>
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning("[ApplyCritUpEffect] No targets provided!");
            return null;
        }

        return new ApplyCritUpGA(targets, critPercentage, duration);
    }
}
