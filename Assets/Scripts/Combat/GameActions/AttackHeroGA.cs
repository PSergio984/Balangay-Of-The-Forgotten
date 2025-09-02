using UnityEngine;

/* ATTACK HERO GA DOCUMENTATION
 * 
 * Purpose: Action representing an enemy attacking the player's hero
 * 
 * How it works:
 * - Contains reference to which enemy is doing the attacking
 * - Gets processed during enemy turns to hurt the player
 * - Usually creates a DealDamageGA action to actually apply damage
 * - Part of the enemy AI behavior system
 * 
 * Integration: Created by EnemySystem during enemy turns, processed by ActionSystem
 */

/// <summary>
/// Game action that represents an enemy attacking the hero player
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Represents when an enemy character attacks the player</para>
/// 
/// <para><strong>What it does:</strong> This action represents an enemy's attack on the player's 
/// hero character. It contains information about which enemy is attacking, and when processed, 
/// it typically creates a damage action to actually hurt the hero. This is a key part of 
/// enemy AI behavior and turn-based combat flow.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Enemy turn starts and EnemySystem decides this enemy should attack</item>
/// <item>EnemySystem creates this action with the attacking enemy</item>
/// <item>ActionSystem processes the attack action</item>
/// <item>Attack handler creates a DealDamageGA to actually hurt the hero</item>
/// <item>Hero takes damage and combat continues</item>
/// </list>
/// 
/// <para><strong>Examples:</strong></para>
/// <list type="bullet">
/// <item>"Goblin attacks hero for 3 damage"</item>
/// <item>"Dragon attacks hero for 12 damage"</item>
/// <item>"Skeleton archer shoots hero for 5 damage"</item>
/// <item>Any enemy's basic attack on the player</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> EnemySystem creates these, ActionSystem processes them, usually creates DealDamageGA</para>
/// 
/// <para><strong>How to use:</strong> EnemySystem creates these during enemy turns with the attacking enemy</para>
/// </remarks>
// Game Action that represents an enemy attacking the hero player
// This is like a "combat move" that gets processed by the action system
public class AttackHeroGA : GameAction
{
    /// <summary>
    /// The enemy that is performing the attack
    /// </summary>
    /// <remarks>
    /// Reference to the specific enemy character that is attacking the hero.
    /// Used to get the enemy's attack power and for any attack-related effects.
    /// </remarks>
    // The enemy that is performing the attack - stored as reference
    public EnemyView Attacker { get; private set; }
    
    /// <summary>
    /// Creates a new attack action with the specified enemy as attacker
    /// </summary>
    /// <param name="attacker">The enemy that will attack the hero</param>
    /// <remarks>
    /// Constructor that creates a new attack action with the specified enemy as attacker.
    /// Stores which enemy is doing the attacking so the system knows whose attack 
    /// power to use and which enemy gets credit for the attack.
    /// </remarks>
    // Constructor - creates a new attack action with the specified enemy as attacker
    public AttackHeroGA(EnemyView attacker)
    {
        // Store which enemy is doing the attacking
        Attacker = attacker;
    }
}
