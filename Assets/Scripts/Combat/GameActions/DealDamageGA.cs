using System.Collections.Generic; 
using UnityEngine;

/* DEAL DAMAGE GA DOCUMENTATION
 * 
 * Purpose: Action that hurts one or more characters in combat
 * 
 * How it works:
 * - Contains damage amount and list of targets to hurt
 * - Gets processed by DamageSystem to actually apply the damage
 * - Can target multiple characters at once (like area of effect spells)
 * - Used by attacks, spells, abilities, and any damage-dealing effects
 * 
 * Integration: Created by various systems, processed by DamageSystem through ActionSystem
 */

/// <summary>
/// Game action that represents dealing damage to one or more targets
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> The action used whenever something needs to hurt characters</para>
/// 
/// <para><strong>What it does:</strong> This action represents any kind of damage being dealt 
/// in the game. Whether it's an enemy attacking the hero, a spell hitting multiple targets, 
/// or a card effect dealing damage, this action carries all the information needed: how much 
/// damage and who gets hurt. The DamageSystem processes this to actually reduce health and 
/// show visual effects.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Something creates this action with damage amount and target list</item>
/// <item>ActionSystem receives and processes the action</item>
/// <item>DamageSystem handles the action and applies damage to each target</item>
/// <item>Each target loses health and visual effects play</item>
/// <item>Any "on damage" reactions get triggered</item>
/// </list>
/// 
/// <para><strong>Examples:</strong></para>
/// <list type="bullet">
/// <item>Enemy attack: 5 damage to hero</item>
/// <item>Fireball spell: 8 damage to all enemies</item>
/// <item>Poison effect: 2 damage to poisoned character each turn</item>
/// <item>Area spell: 4 damage to multiple selected targets</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> DamageSystem processes this, ActionSystem handles it, any system that deals damage creates it</para>
/// 
/// <para><strong>How to use:</strong> Create with damage amount and target list, send to ActionSystem</para>
/// </remarks>
// Game Action that represents dealing damage to one or more targets
// This is used when attacks, spells, or abilities need to hurt characters
public class DealDamageGA : GameAction
{
    /// <summary>
    /// The amount of damage this action will deal to targets
    /// </summary>
    /// <remarks>
    /// This is how much health each target will lose when this action is processed.
    /// All targets in the list will take this same amount of damage.
    /// </remarks>
    // The amount of damage this action will deal to targets
    public int Amount { get; set; }
    
    /// <summary>
    /// List of all combatants (heroes/enemies) that will receive this damage
    /// </summary>
    /// <remarks>
    /// These are the characters that will get hurt when this action is processed.
    /// Can be a single target or multiple targets for area effects.
    /// </remarks>
    // List of all combatants (heroes/enemies) that will receive this damage
    public List<CombatantView> Targets { get; set; }
    
    /// <summary>
    /// Creates a new damage action with specified amount and targets
    /// </summary>
    /// <param name="amount">How much damage to deal</param>
    /// <param name="targets">List of characters to hurt</param>
    /// <remarks>
    /// Constructor that creates a new damage action with specified amount and targets.
    /// Makes a copy of the targets list to avoid reference issues where the original 
    /// list might get modified after creating this action.
    /// </remarks>
    // Constructor that creates a new damage action with specified amount and targets
    public DealDamageGA(int amount, List<CombatantView> targets)
    {
        // Store the damage amount to be dealt
        Amount = amount;
        // Create a new list copy of the targets to avoid reference issues
        Targets = new(targets);
    }
}
