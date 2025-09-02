using UnityEngine;

/* HERO VIEW DOCUMENTATION
 * 
 * Purpose: Visual representation of the player's hero character
 * 
 * How it works:
 * - Extends CombatantView to get basic health and damage functionality
 * - Represents the player's main character in combat
 * - Shows hero stats and appearance to the player
 * - Inherits all combat functionality like taking damage and health display
 * 
 * Integration: Created by HeroSystem, inherits from CombatantView for combat functionality
 */

/// <summary>
/// Visual representation of the player's hero character in combat
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Shows the player's main character on screen during combat</para>
/// 
/// <para><strong>What it does:</strong> This represents the player's hero character that 
/// fights against enemies. It shows the hero's health, appearance, and name. It inherits 
/// all the basic combat functionality from CombatantView like taking damage, health 
/// tracking, and visual effects when hurt. This is the character the player controls 
/// and tries to keep alive.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>HeroSystem creates this when combat starts</item>
/// <item>Gets set up with hero data (health, image, name)</item>
/// <item>Displays hero information so player can see their character</item>
/// <item>Inherits damage handling, health display, and visual effects from CombatantView</item>
/// <item>Represents the player's avatar in the combat system</item>
/// </list>
/// 
/// <para><strong>Key Features:</strong></para>
/// <list type="bullet">
/// <item>Player's main character representation</item>
/// <item>Health tracking and display</item>
/// <item>Damage effects when hurt by enemies</item>
/// <item>Visual appearance based on hero data</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> HeroSystem for creation, CombatantView for base functionality, HeroData for stats</para>
/// 
/// <para><strong>How to use:</strong> HeroSystem creates this automatically, assign UI components in prefab</para>
/// </remarks>
public class HeroView : CombatantView
{
   /// <summary>
   /// Sets up this hero view with data from a HeroData asset
   /// </summary>
   /// <param name="heroData">Contains all the stats and appearance info for the hero</param>
   /// <remarks>
   /// This initializes the hero with all its starting values from the HeroData.
   /// Calls the base SetupBase method to handle health, image, and name setup.
   /// Simple setup since heroes don't need additional UI like enemies (no attack display).
   /// </remarks>
   public void Setup(HeroData heroData)
   {
       // Set up the base combatant properties (health, image, name) using parent class method
       SetupBase(heroData.Health, heroData.Image, heroData.HeroName);
   }
}