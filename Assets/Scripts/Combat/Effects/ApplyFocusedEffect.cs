/*
 * APPLY FOCUSED EFFECT DOCUMENTATION
 * 
 * How it works:
 * This effect applies FOCUSED status (combo buff) with configurable duration.
 * FOCUSED provides fixed bonuses: +30% hit chance and +20% defense ignore.
 * Unlike percentage-based buffs, FOCUSED has fixed values.
 * 
 * Design reasoning:
 * FOCUSED is a combo buff that provides multiple synergistic effects.
 * The bonuses are fixed (not scaling) because it represents a focused mental state.
 * Duration controls how many turns the combatant stays focused.
 * 
 * Integration:
 * - Used by cards/abilities that grant precision/focus
 * - Creates ApplyFocusedGA with duration
 * - FocusedSystem handles the hit chance and defense ignore
 * - StatusEffectTickSystem handles the duration countdown
 */

using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Effect that applies FOCUSED with configurable duration
/// </summary>
[System.Serializable]
public class ApplyFocusedEffect : Effects
{
    /// <summary>
    /// How many turns the focused buff lasts
    /// </summary>
    [SerializeField]
    [LabelText("Duration (Turns)")]
    [Range(1, 10)]
    [InfoBox("FOCUSED grants +30% hit chance and +20% defense ignore", InfoMessageType.Info)]
    [InfoBox("@\"Focused state lasts for \" + duration + \" turn(s)\"", InfoMessageType.None)]
    private int duration = 2;

    /// <summary>
    /// Creates a game action that applies focused buff to targets
    /// </summary>
    /// <param name="targets">Who should receive the focused buff</param>
    /// <param name="caster">Who is causing this effect (for perk system tracking)</param>
    /// <returns>ApplyFocusedGA action ready to be processed</returns>
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning("[ApplyFocusedEffect] No targets provided!");
            return null;
        }

        return new ApplyFocusedGA(targets, duration);
    }
}
