using UnityEngine;

/* MANUAL TARGETING SYSTEM DESIGN
 * 
 * Purpose: Handles player target selection for cards that need specific targets
 * 
 * How it works:
 * - Shows visual arrow from card to mouse cursor for targeting feedback
 * - Uses raycasting to detect valid targets under mouse cursor
 * - Activates when player plays cards that require manual target selection
 * - Returns selected target or null if no valid target found
 * 
 * Design reasoning:
 * - Separates targeting logic from card playing logic for cleaner code organization
 * - Visual feedback helps players understand what they're targeting before committing
 * - Raycast approach works with any objects that have the right layer mask
 * - Singleton pattern ensures consistent targeting behavior across all systems
 * 
 * Integration: Used by CardView for manual target cards, works with ArrowView for visuals
 */

/// <summary>
/// System that handles player selection of specific targets for targeted card effects
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Manages the targeting process for cards that need specific targets</para>
/// 
/// <para><strong>What it does:</strong> When players use cards that target specific enemies 
/// (like single-target damage spells), this system handles the targeting process. It shows 
/// a visual arrow pointing from the card to where the mouse is, and uses raycasting to 
/// detect if there's a valid target under the cursor when the player releases the mouse.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Player plays a card that needs a specific target</item>
/// <item>System shows visual arrow from card position to mouse cursor</item>
/// <item>Player moves mouse to choose target while seeing the arrow</item>
/// <item>When player releases mouse, system checks what's under the cursor</item>
/// <item>Returns the target if valid, or null if no valid target found</item>
/// </list>
/// 
/// <para><strong>Why this design:</strong> Keeps targeting separate from card logic so 
/// different card types can use the same targeting system. Visual feedback helps players 
/// understand what they're targeting before committing to the action.</para>
/// 
/// <para><strong>Works with:</strong> CardView for target selection, ArrowView for visual feedback</para>
/// </remarks>
public class ManualTargetingSystem : Singleton<ManualTargetingSystem>
{
    /// <summary>
    /// Visual arrow that shows targeting direction and provides player feedback
    /// </summary>
    /// <remarks>
    /// ArrowView component that draws a line from start position to mouse cursor.
    /// Gets activated during targeting and hidden when targeting ends.
    /// </remarks>
    [SerializeField] private ArrowView arrowView;
    
    /// <summary>
    /// Layer mask that defines which objects can be targeted
    /// </summary>
    /// <remarks>
    /// Controls what objects the raycast will detect as valid targets.
    /// Set in Inspector to match the layers that enemies or targetable objects use.
    /// </remarks>
    [SerializeField] private LayerMask targetLayerMask;

    /// <summary>
    /// Begins the targeting process by showing visual arrow from specified position
    /// </summary>
    /// <param name="startPosition">Where the targeting arrow should start (usually card position)</param>
    /// <remarks>
    /// Called when player starts using a card that needs manual targeting.
    /// Shows the arrow visual and sets up the targeting feedback system.
    /// Start position typically comes from the card's world position.
    /// </remarks>
    public void StartTargeting(Vector3 startPosition)
    {
        // Make the targeting arrow visible to player
        arrowView.gameObject.SetActive(true);
        // Set up arrow to start from the specified position (usually the card)
        arrowView.SetupArrow(startPosition);
    }

    /// <summary>
    /// Ends targeting process and returns the selected target if valid
    /// </summary>
    /// <param name="endPosition">Where the player released the mouse (target location)</param>
    /// <returns>HeroView if valid target found, null if no valid target</returns>
    /// <remarks>
    /// Called when player releases mouse to select target. Uses raycast to check
    /// if there's a valid target at the end position. Hides the arrow visual
    /// regardless of whether a valid target was found.
    /// </remarks>
    public HeroView EndTargeting(Vector3 endPosition)
    {
        // Hide the targeting arrow since targeting is finished
        arrowView.gameObject.SetActive(false);
        
        // Check if there's a valid target where the player clicked
        if (Physics.Raycast(endPosition, Vector3.forward, out RaycastHit hit, 10f, targetLayerMask) && 
            hit.collider != null && 
            hit.transform.TryGetComponent(out HeroView heroView))
        {
            // Valid target found - return it
            return heroView;
        }
        
        // No valid target found
        return null;
    }
    
    /// <summary>
    /// Cancels targeting without selecting a target
    /// </summary>
    /// <remarks>
    /// Called when targeting needs to be aborted (e.g., card went on cooldown, interaction blocked).
    /// Simply hides the arrow visual without performing any target selection.
    /// </remarks>
    public void CancelTargeting()
    {
        // Hide the targeting arrow since targeting is cancelled
        if (arrowView != null)
        {
            arrowView.gameObject.SetActive(false);
        }
    }
}
