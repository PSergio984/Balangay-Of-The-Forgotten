using TMPro;
using UnityEngine;

/* ENEMY VIEW DOCUMENTATION
 * 
 * Purpose: Visual representation of enemy characters in combat
 * 
 * How it works:
 * - Extends CombatantView to get basic health and damage functionality
 * - Adds enemy-specific features like attack power display
 * - Shows enemy stats (health, attack) to the player
 * - Updates UI when enemy stats change
 * 
 * Integration: Created by EnemySystem, inherits from CombatantView for combat functionality
 */

/// <summary>
/// Visual representation of an enemy character in combat
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Shows enemy characters on screen with their stats and health</para>
/// 
/// <para><strong>What it does:</strong> This represents an enemy that the player can see and 
/// fight against. It shows the enemy's health, attack power, and visual appearance. 
/// It inherits basic combat functionality like taking damage from CombatantView, but 
/// adds enemy-specific features like displaying attack power.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>EnemySystem creates this when spawning enemies</item>
/// <item>Gets set up with enemy data (health, attack, image, name)</item>
/// <item>Displays attack power so player knows how dangerous the enemy is</item>
/// <item>Inherits damage handling, health display, and visual effects from CombatantView</item>
/// <item>Updates attack display when attack power changes</item>
/// </list>
/// 
/// <para><strong>Needs:</strong> EnemyData to set up stats, UI text components for display</para>
/// 
/// <para><strong>Works with:</strong> EnemySystem for creation, CombatantView for base functionality</para>
/// 
/// <para><strong>How to use:</strong> EnemySystem creates these automatically, assign UI components in prefab</para>
/// </remarks>
public class EnemyView : CombatantView
{

    
    /// <summary>
    /// Sets up this enemy view with data from an EnemyData asset
    /// </summary>
    /// <param name="enemyData">Contains all the stats and appearance info for this enemy</param>
    /// <remarks>
    /// This initializes the enemy with all its starting values. Sets attack power,
    /// updates the attack display, then calls the base setup for health, image, and name.
    /// </remarks>
    /// 
    public void Setup(EnemyData enemyData)
    {
        // Set up the base combatant properties using explicit values
        SetupBase(enemyData.Health, enemyData.Image, enemyData.EnemyName, enemyData.MagicPower, enemyData.AttackPower, enemyData.Defense);
    }   
    
   
}
