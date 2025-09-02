using DG.Tweening; // Import DOTween library for smooth animations
using TMPro; // Import TextMeshPro for UI text components
using UnityEngine; // Import Unity engine functionality

/* COMBATANT VIEW DOCUMENTATION
 * 
 * Purpose: Base class for all characters that can fight (heroes, enemies, etc.)
 * 
 * How it works:
 * - Manages health tracking and display for any fighting character
 * - Handles damage effects like screen shake when hurt
 * - Provides visual setup for character appearance and name
 * - Updates UI automatically when health changes
 * 
 * Integration: Base class for HeroView and EnemyView, works with damage system
 */

/// <summary>
/// Base class for any character that can participate in combat
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Foundation for all fighting characters in the game</para>
/// 
/// <para><strong>What it does:</strong> This is the base class that both heroes and enemies 
/// inherit from. It handles all the common stuff that any fighting character needs: 
/// health tracking, taking damage, showing their appearance, and displaying their name. 
/// When any character gets hurt, this handles the visual effects and health updates.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Sets up character with health, image, and name</item>
/// <item>Tracks current and maximum health</item>
/// <item>Handles damage with visual feedback (screen shake)</item>
/// <item>Updates health display automatically</item>
/// <item>Prevents health from going below zero</item>
/// </list>
/// 
/// <para><strong>Features:</strong></para>
/// <list type="bullet">
/// <item>Health management (current and max)</item>
/// <item>Visual damage feedback with DOTween shake</item>
/// <item>Automatic UI updates</item>
/// <item>Character appearance setup</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> DamageSystem for taking damage, DOTween for effects, UI components</para>
/// 
/// <para><strong>How to use:</strong> Inherit from this class for specific character types (HeroView, EnemyView)</para>
/// </remarks>
// Base class for any character that can participate in combat (heroes, enemies, etc.)
// Handles visual representation, health management, and damage effects for all combatants
public class CombatantView : MonoBehaviour
{
    /// <summary>
    /// UI text component that displays the current health points
    /// </summary>
    /// <remarks>
    /// Shows the character's health in format "HP: X" so players can see 
    /// how much health each character has remaining.
    /// </remarks>
    // UI text component that displays the current health points
    [SerializeField] private TMP_Text healthText;
    
    /// <summary>
    /// UI text component that shows the character's name
    /// </summary>
    /// <remarks>
    /// Displays the character's name so players can identify who is who.
    /// Helps distinguish between different enemies or characters.
    /// </remarks>
    // UI text component that shows the character's name
    [SerializeField] private TMP_Text NameText;
    
    /// <summary>
    /// Visual component that displays the character's sprite/image
    /// </summary>
    /// <remarks>
    /// Shows the character's visual appearance - their portrait or sprite.
    /// This is what players see to identify the character visually.
    /// </remarks>
    // Visual component that displays the character's sprite/image
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    /// <summary>
    /// The maximum health this combatant can have (starting health)
    /// </summary>
    /// <remarks>
    /// This is the full health the character starts with and can't exceed.
    /// Used for healing limits and displaying health as a fraction.
    /// </remarks>
    // The maximum health this combatant can have (starting health)
    public int MaxHealth { get; private set; }
    
    /// <summary>
    /// The current health points this combatant has remaining
    /// </summary>
    /// <remarks>
    /// This goes down when taking damage and up when healing.
    /// When it reaches zero, the character is defeated.
    /// </remarks>
    // The current health points this combatant has remaining
    public int CurrentHealth { get; private set; }

    /// <summary>
    /// Sets up the basic properties of this combatant
    /// </summary>
    /// <param name="health">Starting health value (becomes both current and max health)</param>
    /// <param name="image">Sprite image to display for this character</param>
    /// <param name="name">Name to display for this character</param>
    /// <remarks>
    /// Called by child classes (HeroView, EnemyView) to initialize the character.
    /// Sets up health, appearance, and name, then updates the health display.
    /// </remarks>
    // Sets up the basic properties of this combatant (health, appearance, name)
    protected void SetupBase(int health, Sprite image, string name)
    {
        // Set both max health and current health to the starting health value
        MaxHealth = CurrentHealth = health;
        // Set the visual sprite image for this combatant
        spriteRenderer.sprite = image;
        // Display the combatant's name in the UI text
        NameText.text = name;
        // Update the health display to show current health
        UpdateHealthText();
    }

    /// <summary>
    /// Updates the health text UI to reflect the current health value
    /// </summary>
    /// <remarks>
    /// Called whenever health changes to keep the display accurate.
    /// Shows health in format "HP: X" for clear readability.
    /// </remarks>
    // Updates the health text UI to reflect the current health value
    private void UpdateHealthText()
    {
        // Display current health in format "HP: X"
        healthText.text = "HP: " + CurrentHealth;
    }
    
    /// <summary>
    /// Applies damage to this combatant, reducing health and playing damage effects
    /// </summary>
    /// <param name="damageAmount">How much damage to deal to this character</param>
    /// <remarks>
    /// This is called by the damage system when this character gets hurt.
    /// Reduces health, prevents going below zero, plays screen shake effect,
    /// and updates the health display. Makes combat feel impactful with visual feedback.
    /// </remarks>
    // Applies damage to this combatant, reducing health and playing damage effects
    public void Damage(int damageAmount)
    {
        // Subtract the damage amount from current health
        CurrentHealth -= damageAmount;
        // Prevent health from going below zero (dead but not negative)
        if (CurrentHealth < 0)
        {
            // Set health to exactly zero if it went negative
            CurrentHealth = 0;
        }
        // Play a screen shake animation when taking damage (0.2 seconds, 0.5 intensity)
        transform.DOShakePosition(0.2f, 0.5f);
        // Update the health display to show the new health value
        UpdateHealthText();
    }
}
