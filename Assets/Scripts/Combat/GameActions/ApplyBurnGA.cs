/*
 * APPLY BURN GAME ACTION DOCUMENTATION
 * 
 * How it works:
 * This action applies burn damage to a target combatant. Burn is damage over time -
 * when someone has burn stacks, they take damage each turn. This action handles
 * applying that damage and reducing the burn stacks by 1 after damaging.
 * 
 * Design reasoning:
 * We use the GameAction pattern to keep burn damage consistent with other damage
 * sources. This makes it easy to queue burn damage at the right time in the turn
 * cycle. The action contains just what's needed: damage amount and target.
 * 
 * Integration:
 * - Created by HeroSystem when checking for burn at end of enemy turn
 * - Processed by BurnSystem to apply damage and visual effects
 * - Part of the status effect system for damage over time mechanics
 */

using UnityEngine;

/// <summary>
/// Game action that applies burn damage to a target combatant
/// </summary>
public class ApplyBurnGA : GameAction
{
    /// <summary>
    /// How much damage this burn application will deal
    /// </summary>
    /// <remarks>
    /// This equals the number of burn stacks the target has.
    /// More burn stacks = more damage per turn.
    /// </remarks>
    public int BurnDamage { get; private set; }
    
    /// <summary>
    /// Which combatant will take the burn damage
    /// </summary>
    public CombatantView Target { get; private set; }

    /// <summary>
    /// Creates a new burn damage action
    /// </summary>
    /// <param name="burnDamage">How much damage to deal (usually = burn stacks)</param>
    /// <param name="target">Who should take the damage</param>
    public ApplyBurnGA(int burnDamage, CombatantView target)
    {
        BurnDamage = burnDamage;
        Target = target;
    }
}
