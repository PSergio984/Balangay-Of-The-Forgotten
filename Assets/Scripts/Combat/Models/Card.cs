using System.Collections.Generic;
using UnityEngine;

/* CARD MODEL DOCUMENTATION
 * 
 * Purpose: Represents a single card with all its properties and data
 * 
 * How it works:
 * - Wraps CardData (ScriptableObject) to provide runtime card functionality
 * - Stores current card state like stamina cost and effects
 * - Provides easy access to card properties for game systems
 * 
 * Integration: Used by CardSystem, CardView, and effect systems for card handling
 */

/// <summary>
/// Runtime representation of a card with all its properties and game data
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Represents a card during gameplay with all its data and state</para>
/// 
/// <para><strong>What it does:</strong> This class wraps CardData (the design-time 
/// card definition) and provides runtime functionality. It holds the card's 
/// current state, makes properties easily accessible, and handles any runtime 
/// modifications to the card.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Gets created from CardData when cards are loaded</item>
/// <item>Stores references to card properties like title, image, effects</item>
/// <item>Provides easy access to card data for game systems</item>
/// <item>Can be modified during gameplay (like stamina changes)</item>
/// </list>
/// 
/// <para><strong>Needs:</strong> CardData asset to initialize the card properties</para>
/// 
/// <para><strong>Works with:</strong> CardSystem for deck management, CardView for display, EffectSystem for abilities</para>
/// 
/// <para><strong>How to use:</strong> Create with CardData, then use properties to access card information</para>
/// </remarks>
public class Card
{
    /// <summary>
    /// The display name/title of this card
    /// </summary>
    /// <remarks>
    /// Returns the card's title from the underlying CardData.
    /// This is what players see as the card's name in the game.
    /// </remarks>
    public string Title => data.Title;

    /// <summary>
    /// The display target of this card
    /// </summary>
    /// <remarks>
    /// Returns the card's title from the underlying CardData.
    /// This is what players see as the card's name in the game.
    /// </remarks>
    public string Target => data.Target.ToDisplayString();
    
    /// <summary>
    /// The description text explaining what this card does
    /// </summary>
    /// <remarks>
    /// Returns the card's description from the underlying CardData.
    /// This explains the card in general way.
    /// </remarks>
    public string Description => data.Description;
    

    /// <summary>
    /// The visual artwork/image displayed on this card
    /// </summary>
    /// <remarks>
    /// Returns the card's sprite image from the underlying CardData.
    /// This is the artwork that appears on the card in the game.
    /// </remarks>
    public Sprite CardArt => data.Art;
    /// <summary>
    /// The visual artwork/image displayed on this card
    /// </summary>
    /// <remarks>
    /// Returns the card's sprite image from the underlying CardData.
    /// This is the artwork that appears on the card in the game.
    /// </remarks>
    public Sprite CardBackground => data.BackgroundArt;

    /// Role-based visual properties (from CardRoleData asset)
    /// <summary>
    /// The icon representing the card's role (from CardRoleData)
    /// </summary>
    public Sprite RoleIcon => data.RoleData.RoleIcon;
    /// <summary>
    /// The icon representing the role's background (from CardRoleData)
    /// </summary>
    public Sprite RoleCircleIcon => data.RoleData.RoleCircleIcon;
    /// <summary>
    /// The main border sprite for the card's role (from CardRoleData)
    /// </summary>
    public Sprite MainBorder => data.RoleData.MainBorderSprite;
    /// <summary>
    /// The dark border sprite for the card's role (from CardRoleData)
    /// </summary>
    public Sprite DarkBorder => data.RoleData.DarkBorderSprite;
    /// <summary>
    /// The lower border sprite for the card's role (from CardRoleData)
    /// </summary>
    public Sprite LowerBorder => data.RoleData.LowerBorderSprite;


    /// <summary>
    /// The main effect that requires manual target selection (if any)
    /// </summary>
    public Effects ManualTargetEffect => data.ManualTargetEffect;
    /// <summary>
    /// List of secondary effects that use automatic targeting
    /// </summary>
    public List<AutoTargetEffect> OtherEffects => data.OtherEffects; 
    
    /// <summary>
    /// Reference to the original CardData that defines this card
    /// </summary>
    /// <remarks>
    /// This field holds the ScriptableObject data that defines the card's properties.
    /// Used to access the original card design data.
    /// </remarks>
    private readonly CardData data;
    
    /// <summary>
    /// Creates a new card instance from card data
    /// </summary>
    /// <param name="cardData">The CardData asset that defines this card's properties</param>
    /// <remarks>
    /// Creates a runtime card from the design-time CardData.
    /// Copies the stamina value so it can be modified during gameplay if needed.
    /// </remarks>
    public Card(CardData cardData)
    {
        // Store reference to the original card data
        data = cardData;
        // Copy stamina value so it can be modified during gameplay
       // Stamina = cardData.Stamina; // Disabled for testing purposes
    }
}
