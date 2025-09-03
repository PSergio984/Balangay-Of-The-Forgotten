using TMPro;
using UnityEngine;

/* CARD VIEW DESIGN
 * 
 * How it works:
 * - Displays card info like name, description, and cost on the visual card
 * - Lets players hover over cards to see a bigger version
 * - Handles both drag-to-play and manual targeting interactions
 * - For manual target cards: shows targeting arrow instead of dragging
 * - For regular cards: uses drag-and-drop to play them
 * - Checks if player has enough stamina before playing any card
 * 
 * Design reasoning:
 * - Separates manual targeting from drag behavior to provide clear feedback
 * - Manual target cards feel more precise and intentional than drag-and-drop
 * - Both interaction styles use the same stamina validation for consistency
 * - Visual feedback helps players understand different card interaction modes
 * 
 * Integration: Works with hover system, drag system, stamina system, and ManualTargetingSystem
 */

/// <summary>
/// Visual display of a card that players can see and interact with
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Shows card information and handles player interactions</para>
/// 
/// <para><strong>What it does:</strong> This component makes cards visible to players 
/// and lets them interact with the cards. It shows the card's name, description, 
/// cost, and artwork. Players can hover over cards to see them bigger, and drag 
/// cards to play them if they have enough stamina.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Card gets created and displays its information</item>
/// <item>Player hovers mouse over card to see bigger version</item>
/// <item>Player clicks and drags card to play it</item>
/// <item>Game checks if player has enough stamina to play the card</item>
/// <item>Card either gets played or returns to hand</item>
/// </list>
/// 
/// <para><strong>Needs:</strong> Card data to display, hover system for big view, stamina system for costs</para>
/// 
/// <para><strong>Works with:</strong> CardViewHoverSystem for big card view, StaminaSystem for costs, ActionSystem for playing</para>
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
    
    /// <summary>
    /// Text that shows how much stamina the card costs to play
    /// </summary>
    /// <remarks>
    /// This text component displays the stamina cost.
    /// Players need this much stamina to use the card.
    /// </remarks>
    // Text component to display the stamina cost required to play the card
    [SerializeField] private TMP_Text stamina;
    
    /// <summary>
    /// Text that shows how much damage the card deals
    /// </summary>
    /// <remarks>
    /// This text component displays the damage value if the card attacks.
    /// Not all cards have damage, so this might be empty.
    /// </remarks>
    // Text component to display the damage value the card deals
    [SerializeField] private TMP_Text damage;
    
    /// <summary>
    /// Image component that shows the card's artwork
    /// </summary>
    /// <remarks>
    /// This component displays the visual art on the card.
    /// Assign a SpriteRenderer component in the Inspector.
    /// </remarks>
    // Sprite renderer component to display the card's artwork/image
    [SerializeField] private SpriteRenderer imagesSR;
    
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
        // Show how much stamina the card costs
        stamina.text = card.Stamina.ToString();
        // Show the card's artwork
        imagesSR.sprite = card.image;
    }
    
    /// <summary>
    /// Called when player moves mouse over the card
    /// </summary>
    /// <remarks>
    /// Unity calls this automatically when the mouse enters the card area.
    /// Shows a bigger version of the card for easier reading.
    /// </remarks>
    private void OnMouseEnter()
    {
        // Check if player is allowed to hover (not dragging another card)
        if (!Interactions.Instance.PlayerCanHover()) return;
        // Hide the small card version
        wrapper.SetActive(false);
        // Calculate where to show the big card version
        Vector3 pos = new(transform.position.x, -2, 0);
        // Show the big card version at that position
        CardViewHoverSystem.Instance.Show(Card, pos);
    }

    /// <summary>
    /// Called when player moves mouse away from the card
    /// </summary>
    /// <remarks>
    /// Unity calls this automatically when the mouse leaves the card area.
    /// Hides the big card version and shows the normal card again.
    /// </remarks>
    void OnMouseExit()
    {
        // Check if player is allowed to hover
        if (!Interactions.Instance.PlayerCanHover()) return;
        // Hide the big card version
        CardViewHoverSystem.Instance.Hide();
        // Show the normal card version again
        wrapper.SetActive(true);
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
        if (Card.ManualTargetEffects != null)
        {
            // Start targeting mode - shows arrow from card to mouse cursor
            ManualTargetingSystem.Instance.StartTargeting(transform.position);
        }
        else
        {
            // Tell the game that player is now dragging a card
            Interactions.Instance.PlayerIsDragging = true;
            // Make sure the normal card is visible during dragging
            wrapper.SetActive(true);
            // Hide the big hover version since we're dragging now
            CardViewHoverSystem.Instance.Hide();
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
        if(Card.ManualTargetEffects != null)
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
        if (Card.ManualTargetEffects != null)
        {
            // End targeting and get the selected target from mouse position
            EnemyView target = ManualTargetingSystem.Instance.EndTargeting(MouseUtil.GetMousePositionInWorldSpace(-1));
            
            // Play the card if valid target found and player has enough stamina
            if(target!= null && StaminaSystem.Instance.HasEnoughStamina(Card.Stamina))
            {
                // Create play action with the selected target
                PlayCardsGA playCardGA = new(Card, target);
                ActionSystem.Instance.Perform(playCardGA);
            }
            else
            {
                // No valid target or not enough stamina - return card to original position
                transform.position = dragStartPosition;
                transform.rotation = dragStartRotation;
            }
        }
        else
        {
            // Check if player has enough stamina AND the card is over a valid drop area
            if (StaminaSystem.Instance.HasEnoughStamina(Card.Stamina) && Physics.Raycast(transform.position, Vector3.forward, out RaycastHit hit, 10f, dropLayer))
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
            }

            // Player is no longer dragging a card
            Interactions.Instance.PlayerIsDragging = false;
        }
    }

}
