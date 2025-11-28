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
    /// The EnemyData asset containing stats, moveset, and other info for this enemy
    /// </summary>
    public EnemyData Data { get; private set; }

    /// <summary>
    /// Sets up this enemy view with data from an EnemyData asset
    /// </summary>
    /// <param name="enemyData">Contains all the stats and appearance info for this enemy</param>
    /// <remarks>
    /// This initializes the enemy with all its starting values. Sets attack power,
    /// updates the attack display, then calls the base setup for health, image, and name.
    /// Also stores the EnemyData reference for moveset and other logic.
    /// </remarks>
    public void Setup(EnemyData enemyData)
    {
        Data = enemyData;
        // Set up the base combatant properties without UI (UI assigned later in AssignHealthBar)
        SetupBaseWithoutUI(enemyData.Health, enemyData.Image, enemyData.EnemyName, enemyData.MagicPower, enemyData.AttackPower, enemyData.Defense);
    }

    /// <summary>
    /// Assigns the health bar UI components from the scene to this enemy.
    /// </summary>
    /// <param name="healthBarSlider">The Slider UI element to use for this enemy's health bar.</param>
    /// <param name="healthBarFill">The Image component for health bar fill color.</param>
    /// <param name="healthBarText">The Text component displaying health values.</param>
    /// <param name="nameText">The Text component displaying the enemy name.</param>
    /// <remarks>
    /// <para><strong>Why separate from prefab:</strong> Enemies use static scene-based health bars</para>
    /// <para><strong>When called:</strong> After Setup() in EnemyBoardView.AddEnemy()</para>
    /// <para><strong>What it does:</strong> Links all health bar UI to this enemy and updates values</para>
    /// </remarks>
    public void AssignHealthBar(UnityEngine.UI.Slider healthBarSlider, UnityEngine.UI.Image healthBarFill, TMPro.TMP_Text healthBarText, TMPro.TMP_Text nameText)
    {
        // Assign all health bar components
        this.sliderHealth = healthBarSlider;
        this.fillHealth = healthBarFill;
        this.healthText = healthBarText;
        this.NameText = nameText;
        
        // Update all components immediately with current values
        if (sliderHealth != null)
        {
            sliderHealth.maxValue = MaxHealth;
            sliderHealth.value = CurrentHealth;
        }
        
        if (fillHealth != null && gradientHealth != null)
        {
            fillHealth.color = gradientHealth.Evaluate(1f);
        }
        
        if (healthText != null)
        {
            healthText.text = CurrentHealth + "/" + MaxHealth;
        }
        
        if (NameText != null && Data != null)
        {
            NameText.text = Data.EnemyName;
        }
    }
}
