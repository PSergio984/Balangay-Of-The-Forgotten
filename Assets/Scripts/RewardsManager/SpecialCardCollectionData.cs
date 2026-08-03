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
    
    [Header("Capacity & Settings")]
    [Tooltip("Maximum number of special cards allowed in total (customizable 2-4)")]
    [Range(2, 4)]
    [SerializeField] private int maxCapacity = 3;
    public int MaxCapacity => maxCapacity;
    
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
    
    /// <summary>
    /// Dictionary mapping HeroName to assigned SpecialCardData
    /// </summary>
    private Dictionary<string, SpecialCardData> heroCardAssignments = new Dictionary<string, SpecialCardData>();
    
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
    /// Gets the special card currently assigned to a hero by name, or null if none
    /// </summary>
    public SpecialCardData GetSpecialCardForHero(string heroName)
    {
        if (string.IsNullOrEmpty(heroName)) return null;
        if (heroCardAssignments.TryGetValue(heroName, out var card))
        {
            return card;
        }
        return null;
    }
    
    /// <summary>
    /// Randomly assigns a special card to an eligible hero in activeHeroNames who does not yet have a special card.
    /// Capped by maxCapacity and 1 card per hero.
    /// </summary>
    public bool AddSpecialCardToRandomHero(SpecialCardData card, List<string> activeHeroNames)
    {
        if (card == null || string.IsNullOrEmpty(card.CardId))
        {
            Debug.LogWarning("[SpecialCardCollectionData] Cannot add null or empty ID card.");
            return false;
        }
        
        if (activeHeroNames == null || activeHeroNames.Count == 0)
        {
            Debug.LogWarning("[SpecialCardCollectionData] No active heroes provided for assignment.");
            return false;
        }
        
        if (heroCardAssignments.Count >= maxCapacity)
        {
            Debug.LogWarning($"[SpecialCardCollectionData] Cannot assign special card '{card.CardName}': max capacity ({maxCapacity}) reached.");
            return false;
        }
        
        // Find eligible heroes without a special card assigned
        List<string> eligibleHeroes = new List<string>();
        foreach (var name in activeHeroNames)
        {
            if (!string.IsNullOrEmpty(name) && !heroCardAssignments.ContainsKey(name))
            {
                eligibleHeroes.Add(name);
            }
        }
        
        if (eligibleHeroes.Count == 0)
        {
            Debug.Log($"[SpecialCardCollectionData] All active heroes already hold a special card. Cannot assign '{card.CardName}'.");
            return false;
        }
        
        // Pick a random hero
        int randomIndex = Random.Range(0, eligibleHeroes.Count);
        string selectedHero = eligibleHeroes[randomIndex];
        
        heroCardAssignments[selectedHero] = card;
        if (!collectedCards.Contains(card))
        {
            collectedCards.Add(card);
            collectedCardIds.Add(card.CardId);
        }
        
        Debug.Log($"[SpecialCardCollectionData] SPECIAL CARD ASSIGNED: '{card.CardName}' -> Hero '{selectedHero}'. Total assigned: {heroCardAssignments.Count}/{maxCapacity}");
        
        Save();
        return true;
    }
    
    /// <summary>
    /// Consumes (removes) a special card assigned to a specific hero when played in combat.
    /// </summary>
    public bool ConsumeSpecialCardForHero(string heroName, SpecialCardData card)
    {
        if (string.IsNullOrEmpty(heroName) || card == null) return false;
        
        if (heroCardAssignments.TryGetValue(heroName, out var assignedCard) && assignedCard != null && assignedCard.CardId == card.CardId)
        {
            heroCardAssignments.Remove(heroName);
            collectedCards.Remove(card);
            collectedCardIds.Remove(card.CardId);
            
            Debug.Log($"[SpecialCardCollectionData] SPECIAL CARD CONSUMED: '{card.CardName}' from Hero '{heroName}'. Remaining: {heroCardAssignments.Count}");
            Save();
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Legacy fallback method for adding special cards
    /// </summary>
    public bool AddSpecialCard(SpecialCardData card)
    {
        if (card == null || string.IsNullOrEmpty(card.CardId)) return false;
        
        // Check if already collected
        if (HasCard(card.CardId)) return false;
        
        collectedCards.Add(card);
        collectedCardIds.Add(card.CardId);
        Save();
        return true;
    }
    
    /// <summary>
    /// Uses (consumes) a special card from collection
    /// </summary>
    public bool UseSpecialCard(SpecialCardData card)
    {
        if (card == null) return false;
        
        // Try removing from any hero assignment holding this card
        string assignedHeroKey = null;
        foreach (var kvp in heroCardAssignments)
        {
            if (kvp.Value != null && kvp.Value.CardId == card.CardId)
            {
                assignedHeroKey = kvp.Key;
                break;
            }
        }
        
        if (assignedHeroKey != null)
        {
            heroCardAssignments.Remove(assignedHeroKey);
        }
        
        collectedCards.Remove(card);
        collectedCardIds.Remove(card.CardId);
        
        Save();
        return true;
    }
    
    /// <summary>
    /// Checks if a special card is in the collection
    /// </summary>
    public bool HasCard(string cardId)
    {
        if (string.IsNullOrEmpty(cardId)) return false;
        return collectedCardIds.Contains(cardId);
    }
    
    /// <summary>
    /// Gets all available special cards for current map
    /// </summary>
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
        heroCardAssignments.Clear();
        
        int assignCount = PlayerPrefs.GetInt(PREFS_PREFIX + "AssignCount", 0);
        for (int i = 0; i < assignCount; i++)
        {
            PlayerPrefs.DeleteKey(PREFS_PREFIX + "AssignHero_" + i);
            PlayerPrefs.DeleteKey(PREFS_PREFIX + "AssignCard_" + i);
        }
        PlayerPrefs.DeleteKey(PREFS_PREFIX + "AssignCount");
        
        int count = PlayerPrefs.GetInt(PREFS_PREFIX + PREFS_CARD_COUNT, 0);
        for (int i = 0; i < count; i++)
        {
            PlayerPrefs.DeleteKey(PREFS_PREFIX + PREFS_CARD_ID_PREFIX + i);
        }
        PlayerPrefs.DeleteKey(PREFS_PREFIX + PREFS_CARD_COUNT);
        PlayerPrefs.Save();
        
        Debug.Log("[SpecialCardCollectionData] Collection and Hero Assignments cleared.");
    }
    
    #endregion
    
    #region Persistence
    
    /// <summary>
    /// Saves collected card IDs and Hero Assignments to PlayerPrefs
    /// </summary>
    public void Save()
    {
        int index = 0;
        foreach (var kvp in heroCardAssignments)
        {
            if (!string.IsNullOrEmpty(kvp.Key) && kvp.Value != null)
            {
                PlayerPrefs.SetString(PREFS_PREFIX + "AssignHero_" + index, kvp.Key);
                PlayerPrefs.SetString(PREFS_PREFIX + "AssignCard_" + index, kvp.Value.CardId);
                index++;
            }
        }
        PlayerPrefs.SetInt(PREFS_PREFIX + "AssignCount", index);
        
        // Save simple collected cards list
        PlayerPrefs.SetInt(PREFS_PREFIX + PREFS_CARD_COUNT, collectedCards.Count);
        for (int i = 0; i < collectedCards.Count; i++)
        {
            if (collectedCards[i] != null && !string.IsNullOrEmpty(collectedCards[i].CardId))
            {
                PlayerPrefs.SetString(PREFS_PREFIX + PREFS_CARD_ID_PREFIX + i, collectedCards[i].CardId);
            }
        }
        
        PlayerPrefs.Save();
        Debug.Log($"[SpecialCardCollectionData] Saved {heroCardAssignments.Count} assignments & {collectedCards.Count} special cards to PlayerPrefs.");
    }
    
    /// <summary>
    /// Loads collected cards and Hero Assignments from PlayerPrefs
    /// </summary>
    public void Load()
    {
        collectedCards.Clear();
        collectedCardIds.Clear();
        heroCardAssignments.Clear();
        
        // Load hero assignments
        int assignCount = PlayerPrefs.GetInt(PREFS_PREFIX + "AssignCount", 0);
        for (int i = 0; i < assignCount; i++)
        {
            string heroName = PlayerPrefs.GetString(PREFS_PREFIX + "AssignHero_" + i, "");
            string cardId = PlayerPrefs.GetString(PREFS_PREFIX + "AssignCard_" + i, "");
            
            if (!string.IsNullOrEmpty(heroName) && !string.IsNullOrEmpty(cardId))
            {
                SpecialCardData card = FindCardById(cardId);
                if (card != null)
                {
                    heroCardAssignments[heroName] = card;
                    if (!collectedCards.Contains(card))
                    {
                        collectedCards.Add(card);
                        collectedCardIds.Add(cardId);
                    }
                }
            }
        }
        
        // Fallback load simple cards list
        int count = PlayerPrefs.GetInt(PREFS_PREFIX + PREFS_CARD_COUNT, 0);
        for (int i = 0; i < count; i++)
        {
            string cardId = PlayerPrefs.GetString(PREFS_PREFIX + PREFS_CARD_ID_PREFIX + i, "");
            if (!string.IsNullOrEmpty(cardId) && !collectedCardIds.Contains(cardId))
            {
                SpecialCardData card = FindCardById(cardId);
                if (card != null)
                {
                    collectedCards.Add(card);
                    collectedCardIds.Add(cardId);
                }
            }
        }
        
        Debug.Log($"[SpecialCardCollectionData] Loaded {heroCardAssignments.Count} hero assignments and {collectedCards.Count} total special cards.");
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
        var allCards = Resources.LoadAll<SpecialCardData>("");
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
