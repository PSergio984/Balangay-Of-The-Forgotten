/*
 * APPLY DEFENSE DOWN EFFECT DOCUMENTATION
 * 
 * How it works:
 * This effect applies DEFENSE_DOWN status (debuff) with configurable percentage and duration.
 * Reduces target's defense by the specified percentage for the given duration.
 * Targets take more damage while the debuff is active.
 * 
 * Design reasoning:
 * Separating defense reduction from duration makes debuffs easier to balance.
 * "-30% defense for 2 turns" is clearer than "30 stacks that tick down".
 * 
 * Integration:
 * - Used by cards/abilities that apply defense debuffs
 * - Creates ApplyDefenseDownGA with percentage and duration
 * - DefenseDownSystem handles the damage amplification
 * - StatusEffectTickSystem handles the duration countdown
 */

using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Effect that applies DEFENSE_DOWN with configurable percentage and duration
/// </summary>
[System.Serializable]
public class ApplyDefenseDownEffect : Effects
{
    /// <summary>
    /// The percentage of defense decrease (e.g., 30 = -30% defense, takes +30% more damage)
    /// </summary>
    [SerializeField]
    [LabelText("Defense Decrease (%)")]
    [Range(1, 90)]
    [InfoBox("@\"Targets will take \" + defensePercentage + \"% more damage\"", InfoMessageType.None)]
    private int defensePercentage = 30;
    
    /// <summary>
    /// How many turns the defense debuff lasts
    /// </summary>
    [SerializeField]
    [LabelText("Duration (Turns)")]
    [Range(1, 10)]
    [InfoBox("@\"Defense debuff lasts for \" + duration + \" turn(s)\"", InfoMessageType.None)]
    private int duration = 2;
    
    /// <summary>
    /// Optional custom name for consolidated sprite display (e.g., "Bonecracked", "Moonfall", "Bind")
    /// Leave empty to use default DEFENSE_DOWN sprite. Set to a name to use consolidated move sprite.
    /// </summary>
    [SerializeField]
    [LabelText("Custom Effect Name (Optional)")]
    [InfoBox("If set, this name will be used to display a consolidated move sprite instead of the default DEFENSE_DOWN sprite. Examples: 'Bonecracked', 'Moonfall', 'Bind'", InfoMessageType.None)]
    private string customName;

    /// <summary>
    /// Creates a game action that applies defense debuff to targets
    /// </summary>
    /// <param name="targets">Who should receive the defense debuff</param>
    /// <param name="caster">Who is causing this effect (for perk system tracking)</param>
    /// <returns>ApplyDefenseDownGA action ready to be processed</returns>
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning("[ApplyDefenseDownEffect] No targets provided!");
            return null;
        }

        return new ApplyDefenseDownGA(targets, defensePercentage, duration, customName);
    }
}
