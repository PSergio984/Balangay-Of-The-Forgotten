/*
 * APPLY RAGE GAME ACTION DOCUMENTATION
 * 
 * How it works:
 * This action applies RAGE status effect to target combatants.
 * RAGE is a powerful offensive buff that combines:
 * - +50% damage increase
 * - Ignores 20% of enemy defense  
 * - +20% hit chance
 * 
 * Design reasoning:
 * We use the GameAction pattern to keep RAGE application consistent
 * with other game actions. RAGE has special duration tracking handled
 * by RageStatusEffectSystem, so it gets its own action type instead
 * of using the generic AddStatusEffectGA.
 * 
 * Integration:
 * - Created by ApplyRageEffect when cards/perks want to apply RAGE
 * - Processed by RageStatusEffectSystem to actually apply RAGE to combatants
 * - Fits into the standard GameAction workflow
 */

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game action that applies RAGE status effect to target combatants
/// </summary>
public class ApplyRageGA : GameAction
{
    /// <summary>
    /// List of combatants who will receive RAGE
    /// </summary>
    public List<CombatantView> Targets { get; private set; }
    
    /// <summary>
    /// How many turns RAGE lasts
    /// </summary>
    public int Duration { get; private set; }

    /// <summary>
    /// Creates a new RAGE application action
    /// </summary>
    /// <param name="targets">Who should receive RAGE</param>
    /// <param name="duration">How many turns RAGE lasts</param>
    /// <exception cref="ArgumentNullException">Thrown if targets is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if duration is less than 1</exception>
    public ApplyRageGA(List<CombatantView> targets, int duration)
    {
        if (targets == null)
            throw new ArgumentNullException(nameof(targets), "targets cannot be null (ApplyRageGA constructor)");
        if (duration < 1)
            throw new ArgumentOutOfRangeException(nameof(duration), duration, "duration must be at least 1 turn (ApplyRageGA constructor)");
        
        Targets = new List<CombatantView>(targets);
        Duration = duration;
    }
}
