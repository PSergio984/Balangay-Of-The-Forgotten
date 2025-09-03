using UnityEngine;

/* IHAVE CASTER DOCUMENTATION
 * 
 * How it works:
 * - Simple interface that marks actions as having someone who caused them
 * - Lets perk system know who to target when using "target the attacker" mode
 * - Any action that implements this can be used with action caster targeting
 * - Keeps track of who did what for reactive effects
 * 
 * Design reasoning:
 * - Interface design means any action can choose to track its caster
 * - Enables reactive perks that target whoever caused the triggering action
 * - Simple one-property design keeps it easy to implement
 * - Makes perk targeting flexible without complex code
 * 
 * Integration: Used by perk system, implemented by actions like AttackHeroGA and DealDamageGA
 */

/// <summary>
/// Interface for actions that track who caused them to happen
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Lets perks know who to target when using reactive targeting</para>
/// 
/// <para><strong>What it does:</strong> This simple interface marks actions as having 
/// someone who caused them. When perks use "target the action caster" mode, they 
/// look for this interface to find out who to affect. It's like a label that says 
/// "this action has someone responsible for it".</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Actions that want to track their caster implement this interface</item>
/// <item>They store who caused the action in the Caster property</item>
/// <item>Perk system checks if actions have this interface</item>
/// <item>If yes, perk can target the caster for reactive effects</item>
/// </list>
/// 
/// <para><strong>Examples:</strong></para>
/// <list type="bullet">
/// <item>AttackHeroGA tracks which enemy is attacking</item>
/// <item>DealDamageGA tracks who is dealing the damage</item>
/// <item>Counter-attack perk can target the original attacker</item>
/// <item>Damage reflection perk can hurt whoever damaged the player</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> Perk system for reactive targeting, any action that wants to track its source</para>
/// </remarks>
public interface IHaveCaster
{   
    /// <summary>
    /// The character who caused this action to happen
    /// </summary>
    /// <remarks>
    /// Stores who is responsible for this action occurring.
    /// Used by perks that want to target "whoever did this action".
    /// For example, if an enemy attacks, the Caster would be that enemy.
    /// </remarks>
    CombatantView Caster { get; set; }
}
