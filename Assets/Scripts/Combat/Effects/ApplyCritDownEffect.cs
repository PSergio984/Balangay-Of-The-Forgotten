/*
 * APPLY CRIT DOWN EFFECT DOCUMENTATION
 * 
 * How it works:
 * This effect applies CRIT_DOWN status (debuff) with configurable percentage and duration.
 * Reduces target's critical hit chance by the specified percentage for the given duration.
 * 
 * Design reasoning:
 * Separating crit reduction from duration makes debuffs easier to balance.
 * "-30% crit chance for 2 turns" is clearer than "30 stacks that tick down".
 * 
 * Integration:
 * - Used by cards/abilities that apply crit debuffs
 * - Creates ApplyCritDownGA with percentage and duration
 * - CritDownSystem handles the crit chance reduction
 * - StatusEffectTickSystem handles the duration countdown
 */

using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Effect that applies CRIT_DOWN with configurable percentage and duration
/// </summary>
[System.Serializable]
public class ApplyCritDownEffect : Effects
{
    /// <summary>
    /// The percentage of crit chance decrease (e.g., 30 = -30% crit chance)
    /// </summary>
    [SerializeField]
    [LabelText("Crit Chance Decrease (%)")]
    [Range(1, 100)]
    [InfoBox("@\"Targets lose \" + critPercentage + \"% critical hit chance\"", InfoMessageType.None)]
    private int critPercentage = 30;
    
    /// <summary>
    /// How many turns the crit debuff lasts
    /// </summary>
    [SerializeField]
    [LabelText("Duration (Turns)")]
    [Range(1, 10)]
    [InfoBox("@\"Crit debuff lasts for \" + duration + \" turn(s)\"", InfoMessageType.None)]
    private int duration = 2;

    /// <summary>
    /// Creates a game action that applies crit debuff to targets
    /// </summary>
    /// <param name="targets">Who should receive the crit debuff</param>
    /// <param name="caster">Who is causing this effect (for perk system tracking)</param>
    /// <returns>ApplyCritDownGA action ready to be processed</returns>
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning("[ApplyCritDownEffect] No targets provided!");
            return null;
        }

        return new ApplyCritDownGA(targets, critPercentage, duration);
    }
}
