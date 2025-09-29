using TMPro;
using UnityEngine;
using DG.Tweening;

/* CARD VIEW DESIGN
 * 
 * How it works:
 * - Displays card info like description, information artwork, and role icon
 * - Uses DOTween for smooth hover animations (scale, position, rotation)
 * - Handles both drag-to-play and manual targeting interactions
 * - For manual target cards: shows targeting arrow instead of dragging
 * - For regular cards: uses drag-and-drop to play them
 * 
 * 
 * Design reasoning:
 * - Separates manual targeting from drag behavior to provide clear feedback
 * - Manual target cards feel more precise and intentional than drag-and-drop
 * - Both interaction styles use the same  for consistency
 * - DOTween provides smooth, optimized animations for better UX
 * - Visual feedback helps players understand different card interaction modes
 * 
 * Integration: Works with DOTween hover system, drag system, and ManualTargetingSystem
 */

/// <summary>
/// Visual display of a card that players can see and interact with
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Shows card information and handles player interactions</para>
/// 
/// <para><strong>What it does:</strong> This component makes cards visible to players 
/// and lets them interact with the cards. It shows the card's description, information, 
///  artwork, and role icon. Players can hover over cards to see smooth 
/// animations, and drag cards to play them if they have enough stamina.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Card gets created and displays its information</item>
/// <item>Player hovers mouse over card to see smooth scale/position/rotation animations</item>
/// <item>Player clicks and drags card to play it</item>
/// <item>Game checks if player has enough to play the card</item>
/// <item>Card either gets played or returns to hand</item>
/// </list>
/// 
/// <para><strong>Needs:</strong> Card data to display, DOTween for animations</para>
/// 
/// <para><strong>Works with:</strong> DOTween for animations, , ActionSystem for playing</para>
/// 
/// <para><strong>How to use:</strong> Attach to card prefab and assign UI text components in Inspector</para>
/// </remarks>
// Represents the visual components and data display of a single card
public class CardView : MonoBehaviour
{
    // TEXT COMPONENTS IN THE CARD
    /// <summary>
    /// Text that shows the card's name to the player
    /// </summary>
    /// <remarks>
    /// This text component displays what the card is called.
    /// Assign a TextMeshPro component in the Inspector.
    /// </remarks>
    // Text component to display the card's title/name
    [SerializeField] private TMP_Text title;
    
    /// <summary>
    /// Text that explains what the card does when played
    /// </summary>
    /// <remarks>
    /// This text component shows the card's effects and abilities.
    /// Assign a TextMeshPro component in the Inspector.
    /// </remarks>
    // Text component to display the card's description or effect text
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text information;

    
    
    /// <summary>
    /// Image component that shows the card's artwork
    /// </summary>
    /// <remarks>
    /// This component displays the visual art on the card.
    /// Assign a SpriteRenderer component in the Inspector.
    /// </remarks>
    // Sprite renderer component to display the card's artwork/image
    [SerializeField] private SpriteRenderer CardArt;

    /// <summary>
    /// Image component that shows the card's background art
    /// </summary>
    /// <remarks>
    /// This component displays the background art on the card.
    /// Assign a SpriteRenderer component in the Inspector.
    /// </remarks>
    // Sprite renderer component to display the card's background art
    [SerializeField] private SpriteRenderer BackgroundArt;


    /// <summary>
    /// Image component that shows the card's role icon
    /// </summary>

    [SerializeField] private SpriteRenderer RoleCircle;

    [SerializeField] private SpriteRenderer GlassTop;
    [SerializeField] private SpriteRenderer GlassBottom;

     // ADD RENDERERS FOR THE NEW PARTS
    [Header("Role-Based Sprites")]

    /// <summary>
    /// Main border for the card (role-based)
    /// </summary>
    [SerializeField] private SpriteRenderer MainBorder;
    /// <summary>
    /// Inner/dark border for the card (role-based)
    /// </summary>
    [SerializeField] private SpriteRenderer DarkBorder;
    /// <summary>
    /// Lower border for the card (role-based)
    /// </summary>
    [SerializeField] private SpriteRenderer LowerBorder;
    /// <summary>
    /// Role icon for the card (role-based)
    /// </summary>
    [SerializeField] private SpriteRenderer RoleIcon;

    /// <summary>
    /// Container that holds all the visual parts of the card
    /// </summary>
    /// <remarks>
    /// This GameObject groups all the card's visual elements together.
    /// Gets hidden when showing the big hover version of the card.
    /// </remarks>
    // GameObject that wraps/contains all the card's visual elements
    [SerializeField] private GameObject wrapper;
    
    /// <summary>
    /// Layer mask that defines where cards can be dropped to play them
    /// </summary>
    /// <remarks>
    /// This setting controls what areas of the game accept dropped cards.
    /// Set in Inspector to define valid drop zones.
    /// </remarks>
    [SerializeField] private LayerMask dropLayer;

    /// <summary>
    /// The actual card data that this visual represents
    /// </summary>
    /// <remarks>
    /// This property holds the card's game data like effects, cost, and name.
    /// Gets set when the visual card is created.
    /// </remarks>
    public Card Card { get; private set; }

    
    /// <summary>
    /// Remembers where the card was before dragging started
    /// </summary>
    /// <remarks>
    /// This field stores the card's original position.
    /// Used to put the card back if it can't be played.
    /// </remarks>
    private Vector3 dragStartPosition;
    
    /// <summary>
    /// Remembers how the card was rotated before dragging started
    /// </summary>
    /// <remarks>
    /// This field stores the card's original rotation.
    /// Used to put the card back to its original angle if it can't be played.
    /// </remarks>
    private Quaternion dragStartRotation;
    
    /// <summary>
    /// Stores the original scale for optimized hover animations
    /// </summary>
    /// <remarks>
    /// Remembers the card's normal size for returning after hover.
    /// Used by DOTween for performance-optimized hover effects.
    /// </remarks>
    private Vector3 originalScale;
    
    /// <summary>
    /// Stores the original position for optimized hover animations
    /// </summary>
    /// <remarks>
    /// Remembers where the card should return to after hover animation ends.
    /// Updated after hand positioning to ensure correct return position.
    /// </remarks>
    private Vector3 originalPosition;
    
    /// <summary>
    /// Stores the original rotation for optimized hover animations
    /// </summary>
    /// <remarks>
    /// Remembers the card's original rotation for returning after hover.
    /// Updated after hand positioning to ensure correct return rotation.
    /// </remarks>
    private Quaternion originalRotation;
    
    /// <summary>
    /// Tracks if the card is currently being hovered for layer management
    /// </summary>
    /// <remarks>
    /// Prevents layer conflicts and ensures proper visual stacking order.
    /// </remarks>
    private bool isHovering = false;
    
    /// <summary>
    /// Flag to prevent hover during hand positioning animations
    /// </summary>
    /// <remarks>
    /// Prevents hover issues while cards are animating to hand positions.
    /// </remarks>
    private bool isPositioning = false;

    /// <summary>
    /// Sets the card's sorting order using SortingGroup component
    /// </summary>
    /// <param name="sortingOrder">The sorting order value to set</param>
    private void SetCardSortingOrder(int sortingOrder)
    {
        UnityEngine.Rendering.SortingGroup sortingGroup = GetComponent<UnityEngine.Rendering.SortingGroup>();
        if (sortingGroup != null)
        {
            sortingGroup.sortingOrder = sortingOrder;
        }
    }

    /// <summary>
    /// Brings the card to the front (above other cards)
    /// </summary>
    private void BringCardToFront() => SetCardSortingOrder(100);

    /// <summary>
    /// Returns the card to normal sorting order
    /// </summary>
    private void ResetCardSortingOrder() => SetCardSortingOrder(1);

    /// <summary>
    /// Sets up this card visual with the card's information
    /// </summary>
    /// <param name="card">The card data to display on this visual</param>
    /// <remarks>
    /// This method fills in all the text and image components with the card's information.
    /// Call this when creating a new card visual to make it show the right data.
    /// </remarks>
    public void Setup(Card card)
    {
        // Remember which card this visual represents
        Card = card;
        // Show the card's name in the title text
        title.text = card.Title;
        // Show what the card does in the description text
        description.text = card.Description;
        information.text = card.Information;
        // Show the card's artwork
        CardArt.sprite = card.CardArt;
        BackgroundArt.sprite = card.CardBackground;
        // NOW, SET ALL THE ROLE-BASED SPRITES
        RoleIcon.sprite = card.RoleIcon;
        MainBorder.sprite = card.MainBorder;
        DarkBorder.sprite = card.DarkBorder;
        LowerBorder.sprite = card.LowerBorder;
        // Store original scale for optimized hover animations
        originalScale = transform.localScale;
        // Store original rotation for optimized hover animations
        originalRotation = transform.rotation;
        // Position will be updated after hand positioning
        
    }
    
    /// <summary>
    /// Updates the original position, rotation, and scale after the card has been positioned in the hand
    /// </summary>
    /// <remarks>
    /// Call this method after the card has been moved to its final position in the hand.
    /// This ensures hover animations return to the correct hand position, rotation, and scale.
    /// Essential for proper DOTween hover optimization.
    /// </remarks>
    public void UpdateOriginalPosition()
    {
        // Safety check: ensure object is not destroyed
        if (this == null || transform == null) return;
        
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
        isPositioning = false; // Mark positioning as complete
    }
    
    /// <summary>
    /// Marks the card as being repositioned to prevent hover during animation
    /// </summary>
    /// <remarks>
    /// Call this before starting hand positioning animations to prevent hover conflicts.
    /// </remarks>
    public void SetPositioning(bool positioning)
    {
        isPositioning = positioning;
        if (positioning && isHovering)
        {
            // Force exit hover if we start positioning while hovering
            OnMouseExit();
        }
    }
    
    /// <summary>
    /// Called when player moves mouse over the card
    /// </summary>
    /// <remarks>
    /// Unity calls this automatically when the mouse enters the card area.
    /// Uses optimized DOTween animations instead of CardViewHoverSystem for better performance.
    /// </remarks>
    private void OnMouseEnter()
    {
        // Safety check: ensure object is not destroyed
        if (this == null || transform == null) return;
        
        // Prevent hover during positioning or if already hovering
        if (isPositioning || isHovering) return;
        
        // Check if player is allowed to hover (not dragging another card)
        if (!Interactions.Instance.PlayerCanHover()) return;
        
        // Ensure we have valid original values for animation
        if (originalScale == Vector3.zero) originalScale = transform.localScale;
        if (originalPosition == Vector3.zero) originalPosition = transform.position;
        
        // Mark as hovering to prevent conflicts
        isHovering = true;
        
        // Kill any existing animations to prevent conflicts
        transform.DOKill();
        
        // Bring card to front using SortingGroup
        BringCardToFront();
        
        // Calculate hover transformations (move up, scale up, rotate to straight)
        Vector3 hoverPosition = originalPosition + Vector3.up * 2.5f;
        Vector3 hoverScale = originalScale * 1.2f;
        Quaternion hoverRotation = Quaternion.identity; // Straight rotation (0, 0, 0)
        
        // Optimized simultaneous animations
        transform.DOMove(hoverPosition, 0.3f).SetEase(Ease.OutBack);
        transform.DOScale(hoverScale, 0.3f).SetEase(Ease.OutBack);
        transform.DORotate(hoverRotation.eulerAngles, 0.3f).SetEase(Ease.OutBack);
    }

    /// <summary>
    /// Called when player moves mouse away from the card
    /// </summary>
    /// <remarks>
    /// Unity calls this automatically when the mouse leaves the card area.
    /// Uses optimized DOTween animations to return card to normal state.
    /// </remarks>
    void OnMouseExit()
    {
        // Safety check: ensure object is not destroyed
        if (this == null || transform == null) return;
        
        // Check if player is allowed to hover
        if (!Interactions.Instance.PlayerCanHover()) return;
        
        // Ensure we have valid original values for animation
        if (originalScale == Vector3.zero) originalScale = Vector3.one;
        if (originalPosition == Vector3.zero) originalPosition = transform.position - Vector3.up * 0.5f;
        if (originalRotation == Quaternion.identity) originalRotation = transform.rotation;
        
        // Only process if we were actually hovering
        if (!isHovering) return;
        
        // Mark as no longer hovering
        isHovering = false;
        
        // Kill any existing animations to prevent conflicts
        transform.DOKill();
        
        // Restore card to original sorting order using SortingGroup
        ResetCardSortingOrder();

        // Optimized return animations (faster than hover in)
        transform.DOMove(originalPosition, 0.2f).SetEase(Ease.OutQuart);
        transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutQuart);
        transform.DORotate(originalRotation.eulerAngles, 0.2f).SetEase(Ease.OutQuart);
    }

    /// <summary>
    /// Called when player clicks down on the card
    /// </summary>
    /// <remarks>
    /// Unity calls this automatically when the mouse button is pressed on the card.
    /// Branches between manual targeting and drag behavior based on card type.
    /// </remarks>
    void OnMouseDown()
    {
        // Check if player is allowed to interact with cards
        if (!Interactions.Instance.PlayerCanInteract()) return;
        
        // Check if this card needs manual targeting (like single-target spells)
        if (Card.ManualTargetEffect != null)
        {
            // Remember where the card started so we can put it back if targeting fails
            dragStartPosition = transform.position;
            dragStartRotation = transform.rotation;
            
            // Set targeting state to prevent other cards from being hovered
            Interactions.Instance.PlayerIsTargeting = true;
            
            // Keep the card at elevated sorting order during targeting (don't let OnMouseExit reset it)
            // Mark as not hovering to prevent OnMouseExit conflicts, but keep the elevated sorting
            isHovering = false;
            
            // Ensure the card stays above others during targeting
            BringCardToFront();
            
            // Start targeting mode - shows arrow from card to mouse cursor
            ManualTargetingSystem.Instance.StartTargeting(transform.position);
        }
        else
        {
            // Tell the game that player is now dragging a card
            Interactions.Instance.PlayerIsDragging = true;
            // Kill any hover animations since we're now dragging
            transform.DOKill();
            // Reset to normal scale and position for dragging
            transform.localScale = originalScale;
            // Remember where the card started so we can put it back if needed
            dragStartPosition = transform.position;
            dragStartRotation = transform.rotation;
            // Make the card face forward while dragging
            transform.rotation = Quaternion.Euler(0, 0, 0);
            // Move the card to where the mouse is pointing
            transform.position = MouseUtil.GetMousePositionInWorldSpace(-1);
        }
   
    }
    
    /// <summary>
    /// Called while player drags the card around
    /// </summary>
    /// <remarks>
    /// Unity calls this repeatedly while the mouse is held down and moving.
    /// Only works for regular cards - manual target cards don't drag.
    /// </remarks>
    void OnMouseDrag()
    {
        // Check if player is allowed to interact with cards
        if (!Interactions.Instance.PlayerCanInteract()) return;
        
        // Manual target cards don't drag - they use targeting arrows instead
        if(Card.ManualTargetEffect != null)
        {
            return;
        }
        // Make the card follow the mouse position
        transform.position = MouseUtil.GetMousePositionInWorldSpace(-1);
    }

    /// <summary>
    /// Called when player releases the mouse button
    /// </summary>
    /// <remarks>
    /// Unity calls this automatically when the mouse button is released.
    /// Handles both manual targeting completion and drag-to-play validation.
    /// </remarks>
    void OnMouseUp()
    {
        // Check if player is allowed to interact with cards
        if (!Interactions.Instance.PlayerCanInteract()) return;

        // Handle manual target cards (like single-target damage spells)
        if (Card.ManualTargetEffect != null)
        {
            // End targeting and get the selected target from mouse position
            HeroView target = ManualTargetingSystem.Instance.EndTargeting(MouseUtil.GetMousePositionInWorldSpace(-1));
            
            if(target!= null)
            {
                // Clear targeting state since targeting is complete
                Interactions.Instance.PlayerIsTargeting = false;
                // Create play action with the selected target
                PlayCardsGA playCardGA = new(Card, target);
                ActionSystem.Instance.Perform(playCardGA);
            }
            else
            {
                // Clear targeting state since targeting failed
                Interactions.Instance.PlayerIsTargeting = false;
                // No valid target- return card to original position with smooth animation
                // Use originalPosition (hand position) instead of dragStartPosition for manual targeting cards
                transform.DOMove(originalPosition, 0.3f).SetEase(Ease.OutQuart);
                transform.DORotate(dragStartRotation.eulerAngles, 0.3f).SetEase(Ease.OutQuart);
                transform.DOScale(originalScale, 0.3f).SetEase(Ease.OutQuart);
                
                // Reset sorting order back to normal after targeting fails
                ResetCardSortingOrder();
            }
        }
        else
        {
            if (Physics.Raycast(transform.position, Vector3.forward, out RaycastHit hit, 10f, dropLayer))
            {
                // Player can play this card - create a play card action
                PlayCardsGA playCardGA = new(Card);
                // Tell the game to play the card
                ActionSystem.Instance.Perform(playCardGA);
            }
            else
            {
                // Player can't play the card - put it back where it came from
                transform.position = dragStartPosition;
                transform.rotation = dragStartRotation;
                transform.localScale = originalScale;
            }

            // Player is no longer dragging a card
            Interactions.Instance.PlayerIsDragging = false;
        }
    }

    /// <summary>
    /// Clean up DOTween animations when the card is destroyed to prevent errors
    /// </summary>
    private void OnDestroy()
    {
        try
        {
            // Reset hovering state
            isHovering = false;
            
            // Kill all DOTween animations on this transform to prevent errors
            // when the card is destroyed while animations are still running
            if (transform != null)
            {
                transform.DOKill();
            }
            
            // Also kill any animations that might be targeting this object by ID
            DOTween.Kill(this);
        }
        catch (System.Exception e)
        {
            // Log the error but don't let it crash the game
            Debug.LogWarning($"Error cleaning up CardView animations: {e.Message}");
        }
    }

}
