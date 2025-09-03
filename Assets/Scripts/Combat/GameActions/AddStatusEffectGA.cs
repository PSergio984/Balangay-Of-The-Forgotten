/*
 * ADD STATUS EFFECT GAME ACTION DOCUMENTATION
 * 
 * How it works:
 * This action adds status effects (like armor or burn) to target combatants.
 * Contains all the info needed: what type of effect, how many stacks, and who
 * gets affected. The StatusEffectSystem processes this action to actually apply
 * the effects to the targets.
 * 
 * Design reasoning:
 * We use the GameAction pattern to keep status effect application consistent
 * with other game actions. This makes it easy to queue, delay, or modify status
 * effects through the action system. All the data is bundled together cleanly.
 * 
 * Integration:
 * - Created by AddStatusEffectEffect when cards/perks want to apply effects
 * - Processed by StatusEffectSystem to actually apply effects to combatants
 * - Fits into the standard GameAction workflow for consistent execution
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game action that applies status effects to target combatants
/// </summary>
public class AddStatusEffectGA : GameAction
{
    /// <summary>
    /// The type of status effect to apply (armor, burn, etc.)
    /// </summary>
    public StatusEffectType StatusEffectType { get; private set; }
    
    /// <summary>
    /// How many stacks of the effect to add
    /// </summary>
    public int StackCount { get; private set; }
    
    /// <summary>
    /// List of combatants who will receive this status effect
    /// </summary>
    public List<CombatantView> Targets { get; private set; }

    /// <summary>
    /// Creates a new status effect action
    /// </summary>
    /// <param name="statusEffectType">What type of effect to apply</param>
    /// <param name="stackCount">How many stacks to add</param>
    /// <param name="targets">Who should receive the effect</param>
    public AddStatusEffectGA(StatusEffectType statusEffectType, int stackCount, List<CombatantView> targets)
    {
        StatusEffectType = statusEffectType;
        StackCount = stackCount;
        Targets = targets;
    }
}
