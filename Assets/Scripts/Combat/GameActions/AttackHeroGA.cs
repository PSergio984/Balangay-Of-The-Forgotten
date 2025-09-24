
using System.Collections.Generic;
using UnityEngine;

/* ATTACK HERO GA DOCUMENTATION
 * 
 * How it works:
 * - Contains reference to which enemy is doing the attacking
 * - Gets processed during enemy turns to hurt the player
 * - Implements IHaveCaster so perks can target the attacking enemy
 * - Usually creates a DealDamageGA action to actually apply damage
 * 
 * Design reasoning:
 * - IHaveCaster implementation enables reactive perks that target attackers
 * - Stores both Attacker (for game logic) and Caster (for perk system)
 * - Same enemy stored in two places for different system needs
 * - Allows perks like "counter-attack" or "damage reflection" to work
 * 
 * Integration: Created by EnemySystem, processed by ActionSystem, supports perk reactive targeting
 */

/// <summary>
/// Game action that represents an enemy attacking the hero player
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Represents when an enemy character attacks the player</para>
/// 
/// <para><strong>What it does:</strong> This action represents an enemy's attack on the player's 
/// hero character. It contains information about which enemy is attacking, and when processed, 
/// it typically creates a damage action to actually hurt the hero. The key new feature is 
/// that it implements IHaveCaster, which lets perks know who attacked so they can target 
/// that enemy with reactive effects.</para>
/// 
/// <para><strong>Perk system integration:</strong> When this action happens, perks with 
/// "UseActionCasterAsTarget" can automatically target the attacking enemy. This enables 
/// defensive perks like counter-attacks, damage reflection, or debuffs that affect 
/// whoever attacked the player.</para>
/// 
/// <para><strong>Examples with perks:</strong></para>
/// <list type="bullet">
/// <item>Goblin attacks, counter-attack perk damages the goblin back</item>
/// <item>Dragon attacks, shield perk reduces damage and stuns the dragon</item>
/// <item>Archer attacks, reflection perk bounces damage back to the archer</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> EnemySystem creates these, perk system can target the Caster</para>
/// </remarks>
// Game Action that represents an enemy attacking the hero player
// This is like a "combat move" that gets processed by the action system
// Now supports perk system through IHaveCaster interface
public class AttackHeroGA : GameAction,IHaveCaster
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
    /// The hero targets for this attack action. Can be overridden by status effects (e.g., taunt).
    /// </summary>
    /// <remarks>
    /// By default, this is null and targeting is handled by the EnemySystem. If set, this list overrides default targeting logic.
    /// </remarks>
    public List<HeroView> Targets { get; set; }

    /// <summary>
    /// The character who caused this action (same as Attacker, but for perk system)
    /// </summary>
    /// <remarks>
    /// Required by IHaveCaster interface. Lets perks know who to target when using 
    /// "target the action caster" mode. Always points to the same enemy as Attacker.
    /// </remarks>
    public CombatantView Caster { get; set; }
    
    /// <summary>
    /// Creates a new attack action with the specified enemy as attacker
    /// </summary>
    /// <param name="attacker">The enemy that will attack the hero</param>
    /// <remarks>
    /// Constructor that creates a new attack action with the specified enemy as attacker.
    /// Sets both Attacker (for game logic) and Caster (for perk system) to the same enemy.
    /// This dual setup enables both normal combat and reactive perk targeting.
    /// </remarks>
    // Constructor - creates a new attack action with the specified enemy as attacker
    public AttackHeroGA(EnemyView attacker)
    {
    // Store which enemy is doing the attacking
    Attacker = attacker;
    // Also store as Caster for perk system reactive targeting
    Caster = attacker;
    // Targets is null by default; can be set by status effect systems
    Targets = null;
    }
}
