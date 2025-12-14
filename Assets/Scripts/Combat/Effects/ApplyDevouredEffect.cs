using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/* APPLY DEVOURED EFFECT DOCUMENTATION
 * 
 * How it works:
 * This effect applies DEVOURED status with configurable MAG percentage and duration.
 * Unlike the old system which used stacks for both damage and duration,
 * this effect separates the two concepts for clearer configuration.
 * 
 * Design reasoning:
 * Separating damage percentage from duration makes moves easier to balance.
 * "20% MAG damage for 2 turns" is clearer than "60 stacks that tick down".
 * The MAG percentage determines DoT damage, duration controls how long it lasts.
 * 
 * Integration:
 * - Used by moves/abilities that apply Devoured debuff (Lunar Devour)
 * - Creates ApplyDevouredGA with MAG percentage and duration
 * - DevouredSystem handles the DoT damage calculation
 * - StatusEffectTickSystem handles the duration countdown
 * 
 * Used by:
 * - Lunar Devour (Bakunawa): 20% MAG damage (fixed at 60HP) for 2 turns
 */

/// <summary>
/// Effect that applies DEVOURED with configurable MAG percentage and duration
/// </summary>
[System.Serializable]
public class ApplyDevouredEffect : Effects
{
    /// <summary>
    /// Percentage of MAG to deal as damage per turn (e.g., 20 = 20% of MAG)
    /// </summary>
    [SerializeField]
    [LabelText("MAG Damage (%)")]
    [Range(1, 100)]
    [InfoBox("@\"Targets take \" + magDamagePercentage + \"% of MAG as damage per turn\"", InfoMessageType.None)]
    private int magDamagePercentage = 20;
    
    /// <summary>
    /// Fixed damage amount (overrides MAG percentage if set to > 0)
    /// </summary>
    [SerializeField]
    [LabelText("Fixed Damage (0 = use MAG%)")]
    [Range(0, 1000)]
    [InfoBox("@\"If set to > 0, deals this fixed damage instead of MAG%\"", InfoMessageType.None)]
    private int fixedDamage = 0;
    
    /// <summary>
    /// How many turns the Devoured debuff lasts
    /// </summary>
    [SerializeField]
    [LabelText("Duration (Turns)")]
    [Range(1, 10)]
    [InfoBox("@\"Devoured lasts for \" + duration + \" turn(s)\"", InfoMessageType.None)]
    private int duration = 2;

    /// <summary>
    /// Creates a game action that applies Devoured debuff to targets
    /// </summary>
    /// <param name="targets">Who should receive the Devoured debuff</param>
    /// <param name="caster">Who is causing this effect (for MAG calculation)</param>
    /// <returns>ApplyDevouredGA action ready to be processed</returns>
    /// <remarks>
    /// Returns a valid action even when targets are empty/null to prevent null reference exceptions.
    /// The action will simply have no effect when processed with empty targets.
    /// </remarks>
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning("[ApplyDevouredEffect] No targets provided! Returning action with empty targets list.");
            return new ApplyDevouredGA(new List<CombatantView>(), magDamagePercentage, fixedDamage, duration, caster);
        }

        return new ApplyDevouredGA(targets, magDamagePercentage, fixedDamage, duration, caster);
    }
}
