using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/* SPECIAL CARD COLLECTION DATA
 * 
 * Purpose: Persists collected special cards across scenes and game sessions
 * 
 * How it works:
 * - Tracks special cards obtained from mini-boss defeats
 * - Cards persist across maps if NOT played
 * - Cards are consumed (removed permanently) when played
 * - Uses PlayerPrefs to persist card IDs across game sessions
 * 
 * Key behaviors:
 * - AddSpecialCard(): Called when mini-boss is defeated
 * - UseSpecialCard(): Called when player plays the card (removes it permanently)
 * - GetAvailableCards(): Returns cards ready to use this map
 * - Cards obtained are added to collection
 * - Cards played are removed from collection
 * 
 * Integration:
 * - VictoryDefeatUI calls AddSpecialCard() when mini-boss chest is opened
 * - SpecialCardPanelUI reads GetAvailableCards() to display cards
 * - SpecialCardPanelUI calls UseSpecialCard() when card is played
 */

/// <summary>
/// ScriptableObject that tracks collected special cards.
/// Cards persist across maps if unused, but are consumed when played.
/// </summary>
[CreateAssetMenu(fileName = "SpecialCardCollectionData", menuName = "Data/Special Card Collection Data")]
public class SpecialCardCollectionData : ScriptableObject
{
    #region Constants
    
    private const string PREFS_PREFIX = "SpecialCards_";
    private const string PREFS_CARD_COUNT = "CardCount";
    private const string PREFS_CARD_ID_PREFIX = "CardId_";
    
    #endregion
    
    #region Configuration
    
    [Header("Available Special Cards (Assign All Possible Cards)")]
    [Tooltip("All special card assets that can be collected - used for loading by ID")]
    [SerializeField] private List<SpecialCardData> allSpecialCardAssets = new List<SpecialCardData>();
    
    #endregion
    
    #region Runtime Data
    
    [Header("Runtime Data (Collected Cards)")]
    [Tooltip("List of special cards the player has collected and not yet used")]
    [SerializeField] private List<SpecialCardData> collectedCards = new List<SpecialCardData>();
    
    /// <summary>
    /// Set of collected card IDs for O(1) lookup
    /// </summary>
    private HashSet<string> collectedCardIds = new HashSet<string>();
    
    #endregion
    
    #region Public Properties
    
    /// <summary>
    /// Number of special cards in collection
    /// </summary>
    public int CardCount => collectedCards.Count;
    
    /// <summary>
    /// Whether there are any available special cards
    /// </summary>
    public bool HasAvailableCards => collectedCards.Count > 0;
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Adds a special card to the collection (called when mini-boss is defeated)
    /// </summary>
    /// <param name="card">The special card to add</param>
    /// <returns>True if card was added, false if already owned or invalid</returns>
    public bool AddSpecialCard(SpecialCardData card)
    {
        if (card == null)
        {
            Debug.LogWarning("[SpecialCardCollectionData] Cannot add null special card.");
            return false;
        }
        
        if (string.IsNullOrEmpty(card.CardId))
        {
            Debug.LogWarning($"[SpecialCardCollectionData] Cannot add card with empty ID: {card.name}");
            return false;
        }
        
        // Check if already collected
        if (HasCard(card.CardId))
        {
            Debug.Log($"[SpecialCardCollectionData] Special card '{card.CardName}' already in collection.");
            return false;
        }
        
        // Add to collection
        collectedCards.Add(card);
        collectedCardIds.Add(card.CardId);
        
        Debug.Log($"[SpecialCardCollectionData] NEW SPECIAL CARD OBTAINED: '{card.CardName}' ({card.CardId}). Total cards: {CardCount}");
        
        // Save immediately
        Save();
        
        return true;
    }
    
    /// <summary>
    /// Uses (consumes) a special card, removing it from the collection permanently
    /// </summary>
    /// <param name="card">The special card to use</param>
    /// <returns>True if card was used and removed, false if not in collection</returns>
    public bool UseSpecialCard(SpecialCardData card)
    {
        if (card == null)
        {
            Debug.LogWarning("[SpecialCardCollectionData] Cannot use null special card.");
            return false;
        }
        
        if (!HasCard(card.CardId))
        {
            Debug.LogWarning($"[SpecialCardCollectionData] Special card '{card.CardName}' not in collection!");
            return false;
        }
        
        // Remove from collection
        collectedCards.Remove(card);
        collectedCardIds.Remove(card.CardId);
        
        Debug.Log($"[SpecialCardCollectionData] SPECIAL CARD USED: '{card.CardName}' - Removed from collection. Remaining: {CardCount}");
        
        // Save immediately
        Save();
        
        return true;
    }
    
    /// <summary>
    /// Checks if a special card is in the collection
    /// </summary>
    /// <param name="cardId">The card ID to check</param>
    /// <returns>True if the card is in the collection</returns>
    public bool HasCard(string cardId)
    {
        if (string.IsNullOrEmpty(cardId)) return false;
        return collectedCardIds.Contains(cardId);
    }
    
    /// <summary>
    /// Gets all available special cards for current map
    /// </summary>
    /// <returns>Read-only list of available special cards</returns>
    public IReadOnlyList<SpecialCardData> GetAvailableCards()
    {
        return collectedCards.AsReadOnly();
    }
    
    /// <summary>
    /// Clears the collection (for new game or debug)
    /// </summary>
    public void Clear()
    {
        collectedCards.Clear();
        collectedCardIds.Clear();
        
        // Clear PlayerPrefs
        int count = PlayerPrefs.GetInt(PREFS_PREFIX + PREFS_CARD_COUNT, 0);
        for (int i = 0; i < count; i++)
        {
            PlayerPrefs.DeleteKey(PREFS_PREFIX + PREFS_CARD_ID_PREFIX + i);
        }
        PlayerPrefs.DeleteKey(PREFS_PREFIX + PREFS_CARD_COUNT);
        PlayerPrefs.Save();
        
        Debug.Log("[SpecialCardCollectionData] Collection cleared.");
    }
    
    #endregion
    
    #region Persistence
    
    /// <summary>
    /// Saves collected card IDs to PlayerPrefs
    /// </summary>
    public void Save()
    {
        // Save count
        PlayerPrefs.SetInt(PREFS_PREFIX + PREFS_CARD_COUNT, collectedCards.Count);
        
        // Save each card ID
        for (int i = 0; i < collectedCards.Count; i++)
        {
            if (collectedCards[i] != null && !string.IsNullOrEmpty(collectedCards[i].CardId))
            {
                PlayerPrefs.SetString(PREFS_PREFIX + PREFS_CARD_ID_PREFIX + i, collectedCards[i].CardId);
            }
        }
        
        PlayerPrefs.Save();
        Debug.Log($"[SpecialCardCollectionData] Saved {collectedCards.Count} special cards to PlayerPrefs.");
    }
    
    /// <summary>
    /// Loads collected cards from PlayerPrefs
    /// </summary>
    public void Load()
    {
        collectedCards.Clear();
        collectedCardIds.Clear();
        
        int count = PlayerPrefs.GetInt(PREFS_PREFIX + PREFS_CARD_COUNT, 0);
        
        for (int i = 0; i < count; i++)
        {
            string cardId = PlayerPrefs.GetString(PREFS_PREFIX + PREFS_CARD_ID_PREFIX + i, "");
            
            if (!string.IsNullOrEmpty(cardId))
            {
                // Find the card asset by ID
                SpecialCardData card = FindCardById(cardId);
                
                if (card != null)
                {
                    collectedCards.Add(card);
                    collectedCardIds.Add(cardId);
                }
                else
                {
                    Debug.LogWarning($"[SpecialCardCollectionData] Could not find special card asset for ID: {cardId}");
                }
            }
        }
        
        Debug.Log($"[SpecialCardCollectionData] Loaded {collectedCards.Count} special cards from PlayerPrefs.");
    }
    
    /// <summary>
    /// Finds a special card asset by its ID
    /// </summary>
    private SpecialCardData FindCardById(string cardId)
    {
        if (string.IsNullOrEmpty(cardId)) return null;
        
        // Search in pre-configured card assets
        foreach (var card in allSpecialCardAssets)
        {
            if (card != null && card.CardId == cardId)
            {
                return card;
            }
        }
        
        // Fallback: Try to find in Resources
        var allCards = Resources.LoadAll<SpecialCardData>("SpecialCards");
        return allCards.FirstOrDefault(c => c.CardId == cardId);
    }
    
    #endregion
    
    #region Unity Lifecycle
    
    private void OnEnable()
    {
        // Automatically load saved cards when ScriptableObject is enabled
        Load();
    }
    
    #endregion
}
