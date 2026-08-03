using System.Collections.Generic;
using UnityEngine;
using AudioSystem;

/* CARD MODEL DOCUMENTATION
 * 
 * Purpose: Represents a single card with all its properties and data
 * 
 * How it works:
 * - Wraps CardData (ScriptableObject) to provide runtime card functionality
 * - Stores current card state like stamina cost, effects, and cooldown
 * - Provides easy access to card properties for game systems
 * - Tracks runtime cooldown state for each card instance
 * 
 * Integration: Used by CardSystem, CardView, CooldownSystem, and effect systems for card handling
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
/// modifications to the card including cooldown tracking.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Gets created from CardData when cards are loaded</item>
/// <item>Stores references to card properties like title, image, effects</item>
/// <item>Provides easy access to card data for game systems</item>
/// <item>Tracks current cooldown state for playability checks</item>
/// <item>Can be modified during gameplay (like stamina changes, cooldown)</item>
/// </list>
/// 
/// <para><strong>Needs:</strong> CardData asset to initialize the card properties</para>
/// 
/// <para><strong>Works with:</strong> CardSystem for deck management, CardView for display, CooldownSystem for cooldown, EffectSystem for abilities</para>
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
    public string Title => data?.Title ?? "Unknown Card";

    /// <summary>
    /// The display target of this card
    /// </summary>
    /// <remarks>
    /// Returns the card's target information from the underlying CardData.
    /// This indicates what type of target the card can be played on (e.g., enemy, ally, self).
    /// </remarks>
    public string Target => data?.Target.ToDisplayString() ?? "Unknown";    
    /// <summary>
    /// The description text explaining what this card does
    /// </summary>
    /// <remarks>
    /// Returns the card's description from the underlying CardData.
    /// This explains the card in general way.
    /// </remarks>
    public string Description => data?.Description ?? "";
    

    /// <summary>
    /// The visual artwork/image displayed on this card
    /// </summary>
    /// <remarks>
    /// Returns the card's sprite image from the underlying CardData.
    /// This is the artwork that appears on the card in the game.
    /// </remarks>
    public Sprite CardArt => data?.Art;
    /// <summary>
    /// The visual artwork/image displayed on this card
    /// </summary>
    /// <remarks>
    /// Returns the card's sprite image from the underlying CardData.
    /// This is the artwork that appears on the card in the game.
    /// </remarks>
    public Sprite CardBackground => data?.BackgroundArt;

    /// Role-based visual properties (from CardRoleData asset)
    /// <summary>
    /// The icon representing the card's role (from CardRoleData)
    /// </summary>
    public Sprite RoleIcon => data.RoleData?.RoleIcon;
    /// <summary>
    /// The icon representing the role's background (from CardRoleData)
    /// </summary>
    public Sprite RoleCircleIcon => data.RoleData?.RoleCircleIcon;
    /// <summary>
    /// The main border sprite for the card's role (from CardRoleData)
    /// </summary>
    public Sprite MainBorder => data.RoleData?.MainBorderSprite;
    /// <summary>
    /// The dark border sprite for the card's role (from CardRoleData)
    /// </summary>
    public Sprite DarkBorder => data.RoleData?.DarkBorderSprite;
    /// <summary>
    /// The lower border sprite for the card's role (from CardRoleData)
    /// </summary>
    public Sprite LowerBorder => data?.RoleData?.LowerBorderSprite;


    /// <summary>
    /// Sound effect played when this card is played
    /// </summary>
    /// <remarks>
    /// Returns the card's sound effect from the underlying CardData.
    /// Plays when the card is cast/played during gameplay.
    /// </remarks>
    public SoundData SoundData => data?.SoundData;
    
    /// <summary>
    /// The main effect that requires manual target selection (if any)
    /// </summary>
    public Effects ManualTargetEffect => data?.ManualTargetEffect;
    /// <summary>
    /// List of secondary effects that use automatic targeting
    /// </summary>
    public List<AutoTargetEffect> OtherEffects => data?.OtherEffects; 
    
    /// <summary>
    /// The base cooldown duration from CardData (how many rounds to wait after playing)
    /// </summary>
    public int BaseCooldown => data?.Cooldown ?? 0;
    
    /// <summary>
    /// The current remaining cooldown for this card instance
    /// </summary>
    /// <remarks>
    /// When greater than 0, the card cannot be played.
    /// Decreases by 1 at the start of each player turn.
    /// Set to BaseCooldown when the card is played.
    /// </remarks>
    public int CurrentCooldown { get; private set; } = 0;
    
    /// <summary>
    /// Whether this card is currently on cooldown and cannot be played
    /// </summary>
    public bool IsOnCooldown => CurrentCooldown > 0;
    
    /// <summary>
    /// Whether this card was just played this turn (used to skip first cooldown reduction)
    /// </summary>
    /// <remarks>
    /// This flag prevents cooldown reduction on the same turn the card was played.
    /// Set to true when StartCooldown() is called, reset to false after the first ReduceCooldown() call.
    /// This ensures "1-turn cooldown" means unavailable for 1 FULL turn after playing:
    /// - Turn 1: Play card → cooldown = 1, justPlayedThisTurn = true
    /// - Turn 1 End: ReduceCooldown() sees flag, resets flag but SKIPS reduction → cooldown stays 1
    /// - Turn 2: Card unavailable (cooldown = 1) → End Turn → ReduceCooldown() reduces to 0
    /// - Turn 3: Card available (cooldown = 0)
    /// </remarks>
    public bool JustPlayedThisTurn { get; private set; } = false;
    
    /// <summary>
    /// Reference to the original CardData that defines this card
    /// </summary>
    /// <remarks>
    /// This field holds the ScriptableObject data that defines the card's properties.
    /// Used to access the original card design data.
    /// </remarks>
    private readonly CardData data;
    public CardData Data => data;
    
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
        
        // Validate card data
        if (data == null)
        {
            Debug.LogError("[Card] Card created with null CardData!");
        }
        
        // Initialize cooldown to 0 (card starts ready to play)
        CurrentCooldown = 0;
        // Copy stamina value so it can be modified during gameplay
       // Stamina = cardData.Stamina; // Disabled for testing purposes
    }
    
    /// <summary>
    /// Whether this card has valid data
    /// </summary>
    public bool IsValid => data != null;
    
    /// <summary>
    /// Starts the cooldown for this card (called when card is played)
    /// </summary>
    /// <remarks>
    /// Sets CurrentCooldown to BaseCooldown from CardData.
    /// If BaseCooldown is 0, the card has no cooldown.
    /// Sets JustPlayedThisTurn to true to prevent cooldown reduction on the same turn.
    /// </remarks>
    public void StartCooldown()
    {
        CurrentCooldown = BaseCooldown;
        JustPlayedThisTurn = true;
    }
    
    /// <summary>
    /// Reduces the cooldown by 1 (called at the end of each player turn)
    /// </summary>
    /// <remarks>
    /// If JustPlayedThisTurn is true, resets the flag but does NOT reduce cooldown.
    /// This ensures cooldowns count full turns - a card played on Turn 1 is unavailable on Turn 2.
    /// Otherwise, decrements CurrentCooldown by 1, but never goes below 0.
    /// When cooldown reaches 0, the card becomes playable again.
    /// </remarks>
    /// <returns>True if cooldown was actually reduced, false if skipped due to just-played flag</returns>
    public bool ReduceCooldown()
    {
        // If this card was just played this turn, don't reduce cooldown yet
        // Just clear the flag so it will reduce next turn
        if (JustPlayedThisTurn)
        {
            JustPlayedThisTurn = false;
            return false;
        }
        
        // Normal cooldown reduction
        if (CurrentCooldown > 0)
        {
            CurrentCooldown--;
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Resets the cooldown to 0 (for special effects that remove cooldown)
    /// </summary>
    public void ResetCooldown()
    {
        CurrentCooldown = 0;
    }
}
