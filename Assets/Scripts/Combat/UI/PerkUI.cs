/*
 * PERK UI DOCUMENTATION
 * 
 * How it works:
 * This represents a single perk icon on the screen. Each perk the player has gets
 * one of these UI elements. It shows the perk's image and keeps track of which
 * perk it represents. Simple visual representation of one perk.
 * 
 * Design reasoning:
 * We keep this simple - just an image and a reference to the perk. This makes it
 * easy to display perks without complex UI logic. The image gives players visual
 * feedback about what perks they have active. Keeping the perk reference lets
 * other systems find and remove this UI when needed.
 * 
 * Integration:
 * - Created by PerksUI when a new perk is added
 * - Setup method configures it with a specific perk's information
 * - PerksUI uses the Perk property to find and remove this UI later
 */

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI element that displays a single perk as an icon on screen
/// </summary>
public class PerkUI : MonoBehaviour
{
    /// <summary>
    /// The image component that displays the perk's icon
    /// </summary>
    /// <remarks>
    /// This UI Image will show the perk's sprite to the player.
    /// Should be set up in the prefab to look good on screen.
    /// </remarks>
    [SerializeField] private Image image;
    
    /// <summary>
    /// Which perk this UI element represents
    /// </summary>
    /// <remarks>
    /// This lets other systems identify which perk this UI belongs to.
    /// Used by PerksUI when it needs to remove this specific perk's display.
    /// </remarks>
    public Perk Perk { get; private set; }

    /// <summary>
    /// Sets up this UI element to display a specific perk
    /// </summary>
    /// <param name="perk">The perk this UI should represent and display</param>
    /// <remarks>
    /// Called by PerksUI when creating a new perk display.
    /// Takes the perk's image and shows it on screen, and remembers which perk this is for.
    /// After this, the player will see this perk's icon in their perk display area.
    /// </remarks>
    public void Setup(Perk perk)
    {
        // Remember which perk this UI represents
        Perk = perk;
        // Show the perk's icon image to the player
        image.sprite = perk.Image;
    }
}
