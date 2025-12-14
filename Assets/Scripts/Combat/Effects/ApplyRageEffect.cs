/*
 * APPLY RAGE EFFECT DOCUMENTATION
 * 
 * How it works:
 * This effect applies the RAGE status effect to targets when processed.
 * RAGE is the Berserk State buff that provides:
 * - +50% damage increase
 * - Ignores 20% of enemy defense
 * - +20% hit chance
 * 
 * Design reasoning:
 * We create a dedicated effect for RAGE since it has special duration tracking
 * beyond simple stack counts. This effect bridges between card/perk systems
 * and the RageStatusEffectSystem that handles the actual buff logic.
 * 
 * Integration:
 * - Used by cards/perks that want to apply RAGE (Berserk State)
 * - Creates actions processed by RageStatusEffectSystem
 * - Works with any targeting system
 * 
 * Used by:
 * - Berserk State (Mandirigma): Apply RAGE for 3 turns
 * - Any future cards/abilities that grant RAGE
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Effect that applies RAGE status to targets when processed
/// </summary>
[CreateAssetMenu(fileName = "ApplyRageEffect", menuName = "Effects/Apply Rage Effect")]
public class ApplyRageEffect : Effects
{
    /// <summary>
    /// How many turns the RAGE buff lasts
    /// </summary>
    [SerializeField] 
    [Tooltip("Duration of RAGE in turns (e.g., 3 for Berserk State)")]
    private int duration = 3;

    /// <summary>
    /// Creates a game action that will apply RAGE to the targets
    /// </summary>
    /// <param name="targets">Who should receive RAGE</param>
    /// <param name="caster">Who is causing this effect</param>
    /// <returns>ApplyRageGA action ready to be processed</returns>
    /// <remarks>
    /// Called by the EffectSystem when this effect needs to be executed.
    /// Creates an action with the configured duration.
    /// </remarks>
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        // Create action to apply RAGE to all targets
        return new ApplyRageGA(targets, duration);
    }
}
