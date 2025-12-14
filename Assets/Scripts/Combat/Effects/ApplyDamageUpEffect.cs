/*
 * APPLY DAMAGE UP EFFECT DOCUMENTATION
 * 
 * How it works:
 * This effect applies DMG_UP status with configurable percentage and duration.
 * Unlike the basic AddStatusEffectEffect which uses stacks for both strength and duration,
 * this effect separates the two concepts for clearer configuration.
 * 
 * Design reasoning:
 * Separating damage percentage from duration makes cards easier to balance.
 * "+20% damage for 2 turns" is clearer than "20 stacks that tick down".
 * The percentage directly controls damage multiplier, duration controls how long it lasts.
 * 
 * Integration:
 * - Used by cards/abilities that grant damage buffs
 * - Creates ApplyDamageUpGA with percentage and duration
 * - DamageUpSystem handles the damage amplification
 * - StatusEffectTickSystem handles the duration countdown
 */

using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Effect that applies DMG_UP with configurable percentage and duration
/// </summary>
[System.Serializable]
public class ApplyDamageUpEffect : Effects
{
    /// <summary>
    /// The percentage of damage increase (e.g., 20 = +20% damage)
    /// </summary>
    [SerializeField]
    [LabelText("Damage Increase (%)")]
    [Range(1, 100)]
    [InfoBox("@\"Caster will deal \" + damagePercentage + \"% more damage\"", InfoMessageType.None)]
    private int damagePercentage = 20;
    
    /// <summary>
    /// How many turns the damage buff lasts
    /// </summary>
    [SerializeField]
    [LabelText("Duration (Turns)")]
    [Range(1, 10)]
    [InfoBox("@\"Damage buff lasts for \" + duration + \" turn(s)\"", InfoMessageType.None)]
    private int duration = 2;

    /// <summary>
    /// Creates a game action that applies damage buff to targets
    /// </summary>
    /// <param name="targets">Who should receive the damage buff</param>
    /// <param name="caster">Who is causing this effect (for perk system tracking)</param>
    /// <returns>ApplyDamageUpGA action ready to be processed</returns>
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning("[ApplyDamageUpEffect] No targets provided!");
            return null;
        }

        return new ApplyDamageUpGA(targets, damagePercentage, duration);
    }
}
