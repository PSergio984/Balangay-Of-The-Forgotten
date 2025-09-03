/*
 * ADD STATUS EFFECT EFFECT DOCUMENTATION
 * 
 * How it works:
 * This effect can be attached to cards or perks to give status effects to targets.
 * When the effect is processed, it creates an AddStatusEffectGA action that the
 * StatusEffectSystem will handle. Simple bridge between card/perk effects and
 * the actual status effect application.
 * 
 * Design reasoning:
 * We keep the effect data simple - just the type and stack count. The targeting
 * is handled by the card/perk system that uses this effect. This makes it flexible
 * for different targeting scenarios while keeping the effect logic focused.
 * 
 * Integration:
 * - Used by cards and perks that want to apply status effects
 * - Creates AddStatusEffectGA actions when processed by EffectSystem
 * - Works with any targeting system to apply effects to chosen targets
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Effect that applies a status effect to targets when processed
/// </summary>
public class AddStatusEffectEffect : Effects
{
    /// <summary>
    /// The type of status effect this effect will apply
    /// </summary>
    /// <remarks>
    /// Set in the inspector to define what kind of effect this creates.
    /// Could be armor for protection, burn for damage over time, etc.
    /// </remarks>
    [SerializeField] private StatusEffectType statusEffectType;
    
    /// <summary>
    /// How many stacks of the effect to apply
    /// </summary>
    /// <remarks>
    /// Set in the inspector to control effect strength.
    /// Higher numbers mean stronger effects (more armor, more burn damage, etc).
    /// </remarks>
    [SerializeField] private int stackCount;

    /// <summary>
    /// Creates a game action that will apply this status effect to the targets
    /// </summary>
    /// <param name="targets">Who should receive the status effect</param>
    /// <param name="caster">Who is causing this effect (for perk system tracking)</param>
    /// <returns>AddStatusEffectGA action ready to be processed</returns>
    /// <remarks>
    /// Called by the EffectSystem when this effect needs to be executed.
    /// Creates an action with the configured effect type and stack count.
    /// The caster parameter supports perk system tracking but isn't used by
    /// the status effect itself - it's for reactive perks that might care.
    /// </remarks>
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        // Create action to apply the configured status effect to all targets
        return new AddStatusEffectGA(statusEffectType, stackCount, targets);
    }
}
