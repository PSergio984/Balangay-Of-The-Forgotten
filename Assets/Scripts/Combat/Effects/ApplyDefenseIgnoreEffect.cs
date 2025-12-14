using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/* APPLY DEFENSE IGNORE EFFECT DOCUMENTATION
 * 
 * How it works:
 * This effect applies a defense ignore buff to targets for a specified duration.
 * When the target deals damage, they ignore a percentage of the enemy's defense.
 * 
 * Design reasoning:
 * Some abilities grant defense penetration (like Daybreak Fury: ignores 20% DEF).
 * This is separate from RAGE which also provides defense ignore but with other bonuses.
 * Defense ignore is applied during damage calculation in DamageSystem.
 * 
 * Integration:
 * - Used by abilities that grant defense ignore (Daybreak Fury, Focus Aim)
 * - Creates ApplyDefenseIgnoreGA for processing by DefenseIgnoreSystem
 * - DefenseIgnoreSystem modifies DealDamageGA PRE events
 * 
 * Used by:
 * - Daybreak Fury (Apolaki): Ignores 20% DEF for 1 turn
 * - Any future abilities that grant defense penetration
 */

/// <summary>
/// Effect that applies defense ignore buff to targets
/// </summary>
[System.Serializable]
public class ApplyDefenseIgnoreEffect : Effects
{
    /// <summary>
    /// Percentage of defense to ignore (e.g., 20 = ignore 20% of enemy defense)
    /// </summary>
    [SerializeField]
    [LabelText("Defense Ignore (%)")]
    [Range(1, 100)]
    [InfoBox("@\"Attacks will ignore \" + defenseIgnorePercentage + \"% of enemy defense\"", InfoMessageType.None)]
    private int defenseIgnorePercentage = 20;
    
    /// <summary>
    /// How many turns the defense ignore buff lasts
    /// </summary>
    [SerializeField]
    [LabelText("Duration (Turns)")]
    [Range(1, 10)]
    [InfoBox("@\"Defense ignore lasts for \" + duration + \" turn(s)\"", InfoMessageType.None)]
    private int duration = 1;

    /// <summary>
    /// Creates a game action that applies defense ignore buff to targets
    /// </summary>
    /// <param name="targets">Who should receive the defense ignore buff</param>
    /// <param name="caster">Who is causing this effect</param>
    /// <returns>ApplyDefenseIgnoreGA action ready to be processed</returns>
    /// <remarks>
    /// Returns a valid action even when targets are empty/null to prevent null reference exceptions.
    /// The action will simply have no effect when processed with empty targets.
    /// </remarks>
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning("[ApplyDefenseIgnoreEffect] No targets provided! Returning action with empty targets list.");
            return new ApplyDefenseIgnoreGA(new List<CombatantView>(), defenseIgnorePercentage, duration);
        }

        return new ApplyDefenseIgnoreGA(targets, defenseIgnorePercentage, duration);
    }
}
