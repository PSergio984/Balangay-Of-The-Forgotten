using System.Collections.Generic;
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
    /// UI component that manages and displays all status effects for this combatant
    /// </summary>
    /// <remarks>
    /// Shows visual icons and stack counts for all active status effects.
    /// Automatically updates when effects are added, removed, or change stacks.
    /// </remarks>
    [SerializeField] private StatusEffectsUI statusEffectsUI;

    /// <summary>
    /// Dictionary tracking all active status effects and their stack counts
    /// </summary>
    /// <remarks>
    /// Maps each status effect type to its current stack count.
    /// Used for quick lookup during damage calculation and effect management.
    /// </remarks>
    private Dictionary<StatusEffectType, int> statusEffects = new();

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
    /// 
    /// NEW STATUS EFFECT INTEGRATION:
    /// Now checks for armor status effects before applying damage. Armor absorbs damage
    /// first before health is reduced. This creates tactical depth where players can
    /// use armor effects to protect themselves.
    /// 
    /// NOTE: This armor checking logic could be improved and moved to a dedicated
    /// damage calculation system. Currently mixing UI logic with damage logic here.
    /// Consider creating a separate DamageCalculator class to handle armor interactions
    /// and keep this class focused on visual representation.
    /// </remarks>
    // Applies damage to this combatant, reducing health and playing damage effects
    public void Damage(int damageAmount)
    {
        // Calculate how much damage actually gets through after armor
        int remainingDamage = damageAmount;
        int currentArmor = GetStatusEffectStacks(StatusEffectType.ARMOR);

        // If this combatant has armor, use it to absorb damage first
        if (currentArmor > 0)
        {
            // Armor completely absorbs the damage
            if (currentArmor >= damageAmount)
            {
                RemoveStatusEffect(StatusEffectType.ARMOR, remainingDamage);
                remainingDamage = 0;
            }
            // Armor partially absorbs damage, some gets through
            else if (currentArmor < damageAmount)
            {
                RemoveStatusEffect(StatusEffectType.ARMOR, currentArmor);
                remainingDamage -= currentArmor;
            }
        }

        // Apply any remaining damage to health after armor absorption
        if (remainingDamage > 0)
        {
            CurrentHealth -= remainingDamage;
            if (CurrentHealth < 0)
            {
                CurrentHealth = 0;
            }
        }
            // Play a screen shake animation when taking damage (0.2 seconds, 0.5 intensity)
            transform.DOShakePosition(0.2f, 0.5f);
            // Update the health display to show the new health value
            UpdateHealthText();
    }

    /// <summary>
    /// Adds stacks of a status effect to this combatant
    /// </summary>
    /// <param name="type">The type of status effect to add</param>
    /// <param name="stackCount">How many stacks to add</param>
    /// <remarks>
    /// Called by status effect systems to apply effects like armor or burn.
    /// If the effect already exists, adds to the existing stacks.
    /// Updates the UI automatically to show the new effect.
    /// </remarks>
    public void AddStatusEffect(StatusEffectType type, int stackCount)
    {
        // Add to existing stacks or create new entry
        if (statusEffects.ContainsKey(type))
        {
            statusEffects[type] += stackCount;
        }
        else
        {
            statusEffects.Add(type, stackCount);
        }
        // Update the visual display to show the new stack count
        statusEffectsUI.UpdateStatusEffectUI(type, GetStatusEffectStacks(type));
    }
    
    /// <summary>
    /// Removes stacks of a status effect from this combatant
    /// </summary>
    /// <param name="type">The type of status effect to remove</param>
    /// <param name="stackCount">How many stacks to remove</param>
    /// <remarks>
    /// Called when effects wear off or get consumed (like armor blocking damage).
    /// Removes the effect completely if stacks reach zero or below.
    /// Updates the UI automatically to reflect the change.
    /// </remarks>
    public void RemoveStatusEffect(StatusEffectType type, int stackCount)
    {
        // Reduce stacks if the effect exists
        if (statusEffects.ContainsKey(type))
        {
            statusEffects[type] -= stackCount;
            // Remove completely if no stacks remain
            if (statusEffects[type] <= 0)
            {
                statusEffects.Remove(type);
            }
        }
        // Update the visual display (will hide if no stacks remain)
        statusEffectsUI.UpdateStatusEffectUI(type, GetStatusEffectStacks(type));
    }

    /// <summary>
    /// Gets the current number of stacks for a specific status effect
    /// </summary>
    /// <param name="type">The type of status effect to check</param>
    /// <returns>Number of stacks currently active (0 if effect not present)</returns>
    /// <remarks>
    /// Used by damage calculation and other systems to check effect strength.
    /// Returns 0 for effects that aren't currently active on this combatant.
    /// </remarks>
    public int GetStatusEffectStacks(StatusEffectType type)
    {
        // Return current stacks or 0 if effect not present
        if (statusEffects.ContainsKey(type)) return statusEffects[type];
        else return 0;
    }
}
