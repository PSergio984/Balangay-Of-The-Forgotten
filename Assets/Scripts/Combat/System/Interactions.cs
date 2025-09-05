using UnityEngine;

/* INTERACTIONS SYSTEM DOCUMENTATION
 * 
 * Purpose: Controls when players can interact with cards and game elements
 * 
 * How it works:
 * - Tracks if player is currently dragging a card
 * - Checks if game actions are happening that block interactions
 * - Prevents conflicts between different types of interactions
 * - Ensures smooth gameplay by managing interaction timing
 * 
 * Integration: Used by cards, UI elements, and other interactive components
 */

/// <summary>
/// System that manages when players can interact with cards and game elements
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Controls player interaction timing to prevent conflicts</para>
/// 
/// <para><strong>What it does:</strong> This system keeps track of what the player is doing 
/// and decides when they're allowed to interact with cards or other game elements. 
/// It prevents problems like hovering over cards while dragging another card, or 
/// interacting with things while game actions are happening.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Tracks if player is currently dragging a card</item>
/// <item>Checks if the action system is busy with game actions</item>
/// <item>Other components ask this system before allowing interactions</item>
/// <item>Prevents conflicting interactions from happening at the same time</item>
/// </list>
/// 
/// <para><strong>Needs:</strong> ActionSystem to check if game actions are running</para>
/// 
/// <para><strong>Works with:</strong> CardView for interactions, ActionSystem for timing</para>
/// 
/// <para><strong>How to use:</strong> Other components call the check methods before allowing interactions</para>
/// </remarks>
public class Interactions : Singleton<Interactions>
{
    /// <summary>
    /// Tracks whether the player is currently dragging a card
    /// </summary>
    /// <remarks>
    /// This property remembers if the player is in the middle of dragging a card.
    /// Used to prevent other interactions while dragging is happening.
    /// </remarks>
    public bool PlayerIsDragging { get; set; } = false;

    /// <summary>
    /// Tracks whether the player is currently targeting with a manual targeting card
    /// </summary>
    /// <remarks>
    /// This property remembers if the player is in the middle of manual targeting.
    /// Used to prevent other card interactions while targeting is happening.
    /// </remarks>
    public bool PlayerIsTargeting { get; set; } = false;

    /// <summary>
    /// Checks if the player is allowed to interact with cards right now
    /// </summary>
    /// <returns>True if player can interact, false if they should wait</returns>
    /// <remarks>
    /// This method checks if the game is currently busy with actions.
    /// If the action system is performing actions, players have to wait.
    /// Used before allowing card playing, dragging, and other major interactions.
    /// </remarks>
    public bool PlayerCanInteract()
    {
        // Check if the action system is currently performing actions
        if (!ActionSystem.Instance.isPerforming)
        {
            // Action system is free, player can interact
            return true;
        }
        else
        {
            // Action system is busy, player needs to wait
            return false;
        }
    }

    /// <summary>
    /// Checks if the player is allowed to hover over cards right now
    /// </summary>
    /// <returns>True if player can hover, false if hovering should be blocked</returns>
    /// <remarks>
    /// This method checks if hovering should be allowed based on current player state.
    /// Hovering is blocked when the player is dragging a card or targeting to avoid conflicts.
    /// Used by cards before showing hover effects.
    /// </remarks>
    public bool PlayerCanHover()
    {
        // Don't allow hovering if player is dragging a card
        if (PlayerIsDragging) return false;
        // Don't allow hovering if player is targeting with another card
        if (PlayerIsTargeting) return false;
        // Otherwise hovering is allowed
        return true;
    }
}
