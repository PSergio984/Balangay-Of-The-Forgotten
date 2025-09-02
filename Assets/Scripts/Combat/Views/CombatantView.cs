using DG.Tweening; // Import DOTween library for smooth animations
using TMPro; // Import TextMeshPro for UI text components
using UnityEngine; // Import Unity engine functionality

// Base class for any character that can participate in combat (heroes, enemies, etc.)
// Handles visual representation, health management, and damage effects for all combatants
public class CombatantView : MonoBehaviour
{
    // UI text component that displays the current health points
    [SerializeField] private TMP_Text healthText;
    // UI text component that shows the character's name
    [SerializeField] private TMP_Text NameText;
    // Visual component that displays the character's sprite/image
    [SerializeField] private SpriteRenderer spriteRenderer;
    // The maximum health this combatant can have (starting health)
    public int MaxHealth { get; private set; }
    // The current health points this combatant has remaining
    public int CurrentHealth { get; private set; }

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

    // Updates the health text UI to reflect the current health value
    private void UpdateHealthText()
    {
        // Display current health in format "HP: X"
        healthText.text = "HP: " + CurrentHealth;
    }
    
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
