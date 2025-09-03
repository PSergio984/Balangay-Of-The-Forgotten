/*
 * PERKS UI DOCUMENTATION
 * 
 * How it works:
 * This manages the visual display of all the player's perks on screen. When the player
 * gets a new perk, this creates a UI element to show it. When a perk is removed, this
 * cleans up the visual element. It's like a container that holds all the perk icons.
 * 
 * Design reasoning:
 * We separate the UI from the perk logic to keep things clean. The PerkSystem handles
 * the game mechanics, while this handles just the visual display. This makes it easy
 * to change how perks look without breaking how they work. Each perk gets its own
 * UI element that can show the perk's icon and info.
 * 
 * Integration:
 * - PerkSystem calls AddPerkUI/RemovePerkUI when perks change
 * - Creates PerkUI objects for each individual perk
 * - Automatically handles visual cleanup when perks are removed
 */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages the visual display of all player perks on the UI
/// </summary>
public class PerksUI : MonoBehaviour
{
    /// <summary>
    /// The template used to create new perk UI elements
    /// </summary>
    /// <remarks>
    /// This prefab gets copied every time we need to show a new perk.
    /// It should contain all the visual elements needed to display a perk (icon, etc).
    /// </remarks>
    [SerializeField] private PerkUI perkUIPrefab;
    
    /// <summary>
    /// List of all currently displayed perk UI elements
    /// </summary>
    /// <remarks>
    /// We keep track of these so we can find and remove them later when perks are lost.
    /// Each PerkUI corresponds to one active perk that the player has.
    /// </remarks>
    private readonly List<PerkUI> perkUIs = new();

    /// <summary>
    /// Creates and displays a new perk icon on the UI
    /// </summary>
    /// <param name="perk">The perk to create a visual display for</param>
    /// <remarks>
    /// This gets called by PerkSystem when the player gains a new perk.
    /// Creates a new PerkUI element, sets it up with the perk's info, and adds it to our list.
    /// The new perk icon will automatically appear on screen for the player to see.
    /// </remarks>
    public void AddPerkUI(Perk perk)
    {
        // Create a new perk UI element from our template
        PerkUI perkUI = Instantiate(perkUIPrefab, transform);
        // Set up the visual element with the perk's information (icon, etc)
        perkUI.Setup(perk);
        // Add it to our list so we can find it later if we need to remove it
        perkUIs.Add(perkUI);
    }

    /// <summary>
    /// Finds and removes a perk's visual display from the UI
    /// </summary>
    /// <param name="perk">The perk whose visual display should be removed</param>
    /// <remarks>
    /// This gets called by PerkSystem when the player loses a perk.
    /// Finds the matching PerkUI element, removes it from our list, and destroys
    /// the visual element so it disappears from the screen.
    /// </remarks>
    public void RemovePerkUI(Perk perk)
    {
        // Find the UI element that matches this perk
        PerkUI perkUI = perkUIs.Where(pui => pui.Perk == perk).FirstOrDefault();
        if (perkUI != null)
        {
            // Remove it from our tracking list
            perkUIs.Remove(perkUI);
            // Destroy the visual element so it disappears from screen
            Destroy(perkUI.gameObject);
        }
    }
}
