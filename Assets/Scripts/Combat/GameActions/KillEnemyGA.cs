using UnityEngine;

/* KILL ENEMY GA DOCUMENTATION
 * 
 * Purpose: Action that removes a defeated enemy from the battlefield
 * 
 * How it works:
 * - Contains reference to which enemy should be killed/removed
 * - Triggered when an enemy's health reaches zero
 * - Handles the death sequence and cleanup for defeated enemies
 * - Part of the combat death system
 * 
 * Integration: Created by DamageSystem when enemies die, processed by EnemySystem
 */

/// <summary>
/// Game action that represents killing/removing a defeated enemy from the battlefield
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> The action used when an enemy dies and needs to be removed</para>
/// 
/// <para><strong>What it does:</strong> This action represents an enemy being defeated and 
/// removed from the battlefield. When an enemy's health reaches zero from damage, this 
/// action is created to handle the death sequence. It contains a reference to which 
/// specific enemy should be killed and removed from the game.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Enemy takes damage and health reaches zero or below</item>
/// <item>DamageSystem detects the death and creates this action</item>
/// <item>ActionSystem processes the kill enemy action</item>
/// <item>EnemySystem handles the death sequence with animations</item>
/// <item>Enemy gets removed from battlefield and destroyed</item>
/// </list>
/// 
/// <para><strong>Examples:</strong></para>
/// <list type="bullet">
/// <item>Enemy takes lethal damage from player's attack card</item>
/// <item>Enemy dies from poison or damage over time effect</item>
/// <item>Area spell kills multiple enemies at once</item>
/// <item>Any situation where enemy health reaches zero</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> DamageSystem creates these, EnemySystem processes them, EnemyBoardView for removal</para>
/// 
/// <para><strong>How to use:</strong> Created automatically when enemies die, specify which enemy to kill</para>
/// </remarks>
public class KillEnemyGA : GameAction
{
    /// <summary>
    /// The enemy that should be killed and removed from the battlefield
    /// </summary>
    /// <remarks>
    /// Reference to the specific enemy that has been defeated and needs to be removed.
    /// Used to identify which enemy to kill and clean up from the game.
    /// </remarks>
    public EnemyView EnemyView { get; set; }
    
    /// <summary>
    /// Creates a new kill enemy action for the specified enemy
    /// </summary>
    /// <param name="enemyView">The enemy that should be killed</param>
    /// <remarks>
    /// Constructor that creates a new enemy death action. Stores which enemy 
    /// has been defeated so the system knows which one to remove.
    /// </remarks>
    public KillEnemyGA(EnemyView enemyView)
    {
        // Store which enemy should be killed
        EnemyView = enemyView;
    }
}
