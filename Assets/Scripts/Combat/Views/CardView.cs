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
 * - Displays cooldown overlay and blocks interaction when card is on cooldown
 * 
 * 
 * Design reasoning:
 * - Separates manual targeting from drag behavior to provide clear feedback
 * - Manual target cards feel more precise and intentional than drag-and-drop
 * - Both interaction styles use the same  for consistency
 * - DOTween provides smooth, optimized animations for better UX
 * - Visual feedback helps players understand different card interaction modes
 * - Cooldown overlay clearly shows when cards cannot be played
 * 
 * Integration: Works with DOTween hover system, drag system, ManualTargetingSystem, and CooldownSystem
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
    [SerializeField] private TMP_Text Title;
    
    /// <summary>
    /// Text that shows the card's target mode to the player
    /// </summary>
    /// <remarks>
    /// This text component displays what the card targets.
    /// Assign a TextMeshPro component in the Inspector.
    /// </remarks>
    // Text component to display the card's target mode
    [SerializeField] private TMP_Text Target;
    /// <summary>
    /// Text that explains what the card does when played
    /// </summary>
    /// <remarks>
    /// This text component shows the card's effects and abilities.
    /// Assign a TextMeshPro component in the Inspector.
    /// </remarks>
    // Text component to display the card's description or effect text
    [SerializeField] private TMP_Text Description;

    
    
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
    /// Role Background Icon for the card (role-based)
    /// </summary>
    [SerializeField] private SpriteRenderer RoleCircleIcon;

    [Header("Cooldown UI")]
    /// <summary>
    /// The cooldown overlay sprite that appears when the card is on cooldown
    /// </summary>
    [SerializeField] private SpriteRenderer cooldownOverlay;
    
    /// <summary>
    /// The sprite for the cooldown badge/icon background
    /// </summary>
    [SerializeField] private SpriteRenderer cooldownBadge;
    
    /// <summary>
    /// Text component that shows the remaining cooldown number
    /// </summary>
    [SerializeField] private TMP_Text cooldownText;
    
    /// <summary>
    /// The color to tint the card when on cooldown (greyed out)
    /// </summary>
    [SerializeField] private Color cooldownTintColor = new Color(0.5f, 0.5f, 0.5f, 1f);

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

    // Track if original values have been initialized
    private bool originalsInitialized = false;
    
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
    
    // ADD these static fields for single-card hover management
    private static CardView currentlyHoveredCard;
    [SerializeField]  private float lastHoverEventTime = 0f;
    [SerializeField] private const float HOVER_DEBOUNCE_TIME = 0.05f;
    
    /// <summary>
    /// Stores all sprite renderers for tinting during cooldown
    /// </summary>
    private SpriteRenderer[] allSpriteRenderers;
    
    /// <summary>
    /// Original colors of sprite renderers before cooldown tint
    /// </summary>
    private Color[] originalColors;

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
        // Unsubscribe first to prevent duplicate event handlers
        CooldownSystem.OnCooldownChanged -= OnCooldownChanged;

        // Validate card data
        if (card == null)
        {
            Debug.LogError("[CardView] Setup called with null card!");
            return;
        }
        
        // Check if card has valid data
        if (!card.IsValid)
        {
            Debug.LogError($"[CardView] Setup called with invalid card (CardData is null)!");
            return;
        }

        // Remember which card this visual represents
        Card = card;
        
        // Show the card's name in the title text (with null check)
        if (Title != null) Title.text = card.Title;
        else Debug.LogWarning($"[CardView] Title is null for card '{card.Title}'");
        
        // Show what the card does in the description text (with null check)
        if (Description != null) Description.text = card.Description;
        else Debug.LogWarning($"[CardView] Description is null for card '{card.Title}'");
        
        if (Target != null) Target.text = card.Target;
        else Debug.LogWarning($"[CardView] Target is null for card '{card.Title}'");
        
        // Show the card's artwork (with null checks)
        if (CardArt != null) CardArt.sprite = card.CardArt;
        else Debug.LogWarning($"[CardView] CardArt is null for card '{card.Title}'");
        
        if (BackgroundArt != null) BackgroundArt.sprite = card.CardBackground;
        else Debug.LogWarning($"[CardView] BackgroundArt is null for card '{card.Title}'");
        
        // NOW, SET ALL THE ROLE-BASED SPRITES (with null checks)
        if (RoleIcon != null) RoleIcon.sprite = card.RoleIcon;
        else Debug.LogWarning($"[CardView] RoleIcon is null for card '{card.Title}'");
        
        if (RoleCircleIcon != null) RoleCircleIcon.sprite = card.RoleCircleIcon;
        else Debug.LogWarning($"[CardView] RoleCircleIcon is null for card '{card.Title}'");
        
        if (MainBorder != null) MainBorder.sprite = card.MainBorder;
        else Debug.LogWarning($"[CardView] MainBorder is null for card '{card.Title}'");
        
        if (DarkBorder != null) DarkBorder.sprite = card.DarkBorder;
        else Debug.LogWarning($"[CardView] DarkBorder is null for card '{card.Title}'");
        
        if (LowerBorder != null) LowerBorder.sprite = card.LowerBorder;
        else Debug.LogWarning($"[CardView] LowerBorder is null for card '{card.Title}'");
        
        // Store original scale for optimized hover animations
        originalScale = transform.localScale;
        originalRotation = transform.rotation;
        // Position will be updated after hand positioning
        originalsInitialized = true;

        // Cache sprite renderers for cooldown tinting
        CacheSpriteRenderers();

        // Initialize cooldown display
        UpdateCooldownDisplay();

        // Subscribe to cooldown changes
        CooldownSystem.OnCooldownChanged += OnCooldownChanged;
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
    originalsInitialized = true;
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
            ForceExitHover();
        }
    }
    private void OnMouseEnter()
    {
        // Prevent rapid firing
        if (Time.time - lastHoverEventTime < HOVER_DEBOUNCE_TIME) return;
        
            // Safety checks
        if (this == null || transform == null) return;
        if (isPositioning || isHovering) return;
        if (Interactions.Instance == null || !Interactions.Instance.PlayerCanHover()) return;
        
        // CRITICAL: Only allow ONE card to hover at a time
        if (currentlyHoveredCard != null && currentlyHoveredCard != this)
        {
            // Force exit the previous card
            currentlyHoveredCard.ForceExitHover();
        }
        
        // Set this as the current hovered card
        currentlyHoveredCard = this;
        lastHoverEventTime = Time.time;
        
        // Ensure original values are set (fallback for edge cases)
        if (!originalsInitialized)
        {
            originalScale = transform.localScale;
            originalPosition = transform.position;
            originalRotation = transform.rotation;
            originalsInitialized = true;
        }
        
        isHovering = true;
        transform.DOKill();
        BringCardToFront();
            
        // Perfect card game hover with rotation
        Vector3 hoverPosition = originalPosition + Vector3.up * 0.2f;
        Vector3 hoverScale = originalScale * 1.1f;
        Quaternion straightRotation = Quaternion.identity; // 0 degrees = straight
        // Animate to hover state
        transform.DOMove(hoverPosition, 0.25f).SetEase(Ease.OutQuint);
        transform.DOScale(hoverScale, 0.25f).SetEase(Ease.OutQuint);
        transform.DORotate(straightRotation.eulerAngles, 0.25f).SetEase(Ease.OutQuint);
    }

    void OnMouseExit()
    {
        // Prevent rapid firing
        if (Time.time - lastHoverEventTime < HOVER_DEBOUNCE_TIME) return;
        
        // Safety checks
        if (this == null || transform == null) return;
        if (Interactions.Instance == null || !Interactions.Instance.PlayerCanHover()) return;
        if (!isHovering) return;
        
        // Clear static reference if this was the hovered card
        if (currentlyHoveredCard == this)
        {
            currentlyHoveredCard = null;
        }
        
        lastHoverEventTime = Time.time;
        isHovering = false;
        
        transform.DOKill();
        ResetCardSortingOrder();
        
        // Return to original hand position and rotation
        transform.DOMove(originalPosition, 0.2f).SetEase(Ease.OutQuart);
        transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutQuart);
        transform.DORotate(originalRotation.eulerAngles, 0.2f).SetEase(Ease.OutQuart);
    }
    
    // ADD: Force exit method for clearing other cards
    private void ForceExitHover()
    {
        if (!isHovering) return;

        // Clear static reference if this is the hovered card
        if (currentlyHoveredCard == this)
        {
            currentlyHoveredCard = null;
        }

        isHovering = false;
        transform.DOKill();
        ResetCardSortingOrder();

        // Instant return to original state
        transform.DOMove(originalPosition, 0.15f).SetEase(Ease.OutQuart);
        transform.DOScale(originalScale, 0.15f).SetEase(Ease.OutQuart);
        transform.DORotate(originalRotation.eulerAngles, 0.15f).SetEase(Ease.OutQuart);
    }
    
   

    /// <summary>
    /// Called when player clicks down on the card
    /// </summary>
    /// <remarks>
    /// Unity calls this automatically when the mouse button is pressed on the card.
    /// Branches between manual targeting and drag behavior based on card type.
    /// Blocks interaction if card is on cooldown.
    /// </remarks>
    void OnMouseDown()
    {
        // CRITICAL: Check cooldown FIRST before any other checks or state changes
        // This prevents any visual feedback or state changes for cards on cooldown
        if (!CanInteract())
        {
            Debug.Log($"[CardView] Card '{Card?.Title ?? "Unknown"}' is on cooldown ({Card?.CurrentCooldown ?? 0} rounds remaining) - blocking interaction");
            return;
        }
        
        // Check if player is allowed to interact with cards
        if (Interactions.Instance == null || !Interactions.Instance.PlayerCanInteract()) return;
        
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
    /// Does nothing if card is on cooldown.
    /// </remarks>
    void OnMouseDrag()
    {
        // CRITICAL: Check cooldown FIRST - if card became on cooldown during drag, stop immediately
        if (!CanInteract()) return;
        
        // Check if player is allowed to interact with cards
        if (Interactions.Instance == null || !Interactions.Instance.PlayerCanInteract()) return;
        
        // Manual target cards don't drag - they use targeting arrows instead
        if (Card.ManualTargetEffect != null)
        {
            return;
        }
        
        // Only move if player is actually dragging this card
        if (!Interactions.Instance.PlayerIsDragging) return;
        
        // Make the card follow the mouse position
        transform.position = MouseUtil.GetMousePositionInWorldSpace(-1);
    }

    /// <summary>
    /// Called when player releases the mouse button
    /// </summary>
    /// <remarks>
    /// Unity calls this automatically when the mouse button is released.
    /// Handles both manual targeting completion and drag-to-play validation.
    /// Returns card to hand if on cooldown or if play fails.
    /// </remarks>
    void OnMouseUp()
    {
        // CRITICAL: Check cooldown FIRST - if card is on cooldown, return it to hand position
        if (!CanInteract())
        {
            ReturnCardToHand();
            return;
        }
        
        // Check if player is allowed to interact with cards
        if (Interactions.Instance == null || !Interactions.Instance.PlayerCanInteract())
        {
            ReturnCardToHand();
            return;
        }

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
    /// Returns the card to its original hand position with smooth animation
    /// </summary>
    /// <remarks>
    /// Used when card interaction fails or is blocked (cooldown, invalid target, etc.)
    /// Resets all interaction states and smoothly animates the card back to hand.
    /// </remarks>
    private void ReturnCardToHand()
    {
        // Clear any interaction states
        if (Interactions.Instance != null)
        {
            Interactions.Instance.PlayerIsDragging = false;
            Interactions.Instance.PlayerIsTargeting = false;
        }
        
        // Cancel any targeting in progress
        if (ManualTargetingSystem.Instance != null)
        {
            ManualTargetingSystem.Instance.CancelTargeting();
        }
        
        // Kill any existing animations
        transform.DOKill();
        
        // Reset sorting order
        ResetCardSortingOrder();
        
        // Smoothly animate back to original hand position
        transform.DOMove(originalPosition, 0.3f).SetEase(Ease.OutQuart);
        transform.DORotate(originalRotation.eulerAngles, 0.3f).SetEase(Ease.OutQuart);
        transform.DOScale(originalScale, 0.3f).SetEase(Ease.OutQuart);
        
        Debug.Log($"[CardView] Card '{Card?.Title ?? "Unknown"}' returned to hand");
    }
    
    #region Cooldown Methods
    /// <summary>
    /// Caches all sprite renderers for efficient cooldown tinting
    /// </summary>
    private void CacheSpriteRenderers()
    {
        allSpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        originalColors = new Color[allSpriteRenderers.Length];
        for (int i = 0; i < allSpriteRenderers.Length; i++)
        {
            originalColors[i] = allSpriteRenderers[i].color;
        }
    }
    
    /// <summary>
    /// Handles cooldown change events from CooldownSystem
    /// </summary>
    /// <param name="card">The card that changed</param>
    private void OnCooldownChanged(Card card)
    {
        // Only update if this is our card
        if (card == Card)
        {
            UpdateCooldownDisplay();
        }
    }
    
    /// <summary>
    /// Updates the cooldown visual display based on current card state
    /// </summary>
    public void UpdateCooldownDisplay()
    {
        if (Card == null) return;
        
        bool isOnCooldown = Card.IsOnCooldown;
        
        // Show/hide cooldown overlay
        if (cooldownOverlay != null)
        {
            cooldownOverlay.gameObject.SetActive(isOnCooldown);
        }
        
        // Show/hide cooldown badge
        if (cooldownBadge != null)
        {
            cooldownBadge.gameObject.SetActive(isOnCooldown);
        }
        
        // Update cooldown text
        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(isOnCooldown);
            if (isOnCooldown)
            {
                cooldownText.text = Card.CurrentCooldown.ToString();
            }
        }
        
        // Apply or remove tint
        ApplyCooldownTint(isOnCooldown);
    }
    
    /// <summary>
    /// Applies or removes the cooldown tint from all sprite renderers
    /// </summary>
    /// <param name="applyTint">Whether to apply the tint (true) or restore original colors (false)</param>
    private void ApplyCooldownTint(bool applyTint)
    {
        if (allSpriteRenderers == null || originalColors == null) return;
        
        for (int i = 0; i < allSpriteRenderers.Length; i++)
        {
            if (allSpriteRenderers[i] != null)
            {
                // Skip cooldown UI elements from tinting
                if (allSpriteRenderers[i] == cooldownOverlay || 
                    allSpriteRenderers[i] == cooldownBadge)
                {
                    continue;
                }
                
                if (applyTint)
                {
                    // Apply grey tint
                    allSpriteRenderers[i].color = originalColors[i] * cooldownTintColor;
                }
                else
                {
                    // Restore original color
                    allSpriteRenderers[i].color = originalColors[i];
                }
            }
        }
    }
    
    /// <summary>
    /// Checks if the card can be interacted with (not on cooldown)
    /// </summary>
    /// <returns>True if the card can be played, false if on cooldown</returns>
    private bool CanInteract()
    {
        return Card != null && !Card.IsOnCooldown;
    }
    #endregion

    /// <summary>
    /// Clean up DOTween animations when the card is destroyed to prevent errors
    /// </summary>
    private void OnDestroy()
    {
        // Unsubscribe from cooldown events
        CooldownSystem.OnCooldownChanged -= OnCooldownChanged;
        
        // Clear static reference if this card was hovered
        if (currentlyHoveredCard == this)
        {
            currentlyHoveredCard = null;
        }
        
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
