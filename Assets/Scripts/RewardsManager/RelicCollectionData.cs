using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/* RELIC COLLECTION DATA
 * 
 * Purpose: Persists collected relics across scenes and game sessions
 * 
 * How it works:
 * - Tracks all relics the player has obtained from defeating main bosses
 * - Uses PlayerPrefs to persist relic IDs across game sessions
 * - Provides methods to add relics, check if owned, and get all collected relics
 * 
 * Integration: 
 * - VictoryDefeatUI calls AddRelic() when main boss chest is opened
 * - RelicPanelUI reads GetAllRelics() to display collected relics in combat
 * - Save/Load called automatically via OnEnable
 */

/// <summary>
/// ScriptableObject that tracks all collected relics.
/// Persists across scenes and game sessions via PlayerPrefs.
/// </summary>
[CreateAssetMenu(fileName = "RelicCollectionData", menuName = "Data/Relic Collection Data")]
public class RelicCollectionData : ScriptableObject
{
    #region Constants
    
    private const string PREFS_PREFIX = "RelicCollection_";
    private const string PREFS_RELIC_COUNT = "RelicCount";
    private const string PREFS_RELIC_ID_PREFIX = "RelicId_";
    
    #endregion
    
    #region Configuration
    
    [Header("Available Relics (Assign All Possible Relics)")]
    [Tooltip("All relic assets that can be collected - used for loading by ID")]
    [SerializeField] private List<RelicData> allRelicAssets = new List<RelicData>();
    
    #endregion
    
    #region Runtime Data
    
    [Header("Runtime Data (Collected Relics)")]
    [Tooltip("List of relics the player has collected")]
    [SerializeField] private List<RelicData> collectedRelics = new List<RelicData>();
    
    /// <summary>
    /// Set of collected relic IDs for O(1) lookup
    /// </summary>
    private HashSet<string> collectedRelicIds = new HashSet<string>();
    
    #endregion
    
    #region Public Properties
    
    /// <summary>
    /// Number of relics collected
    /// </summary>
    public int RelicCount => collectedRelics.Count;
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Adds a relic to the collection if not already owned
    /// </summary>
    /// <param name="relic">The relic to add</param>
    /// <returns>True if relic was added, false if already owned or invalid</returns>
    public bool AddRelic(RelicData relic)
    {
        if (relic == null)
        {
            Debug.LogWarning("[RelicCollectionData] Cannot add null relic.");
            return false;
        }
        
        if (string.IsNullOrEmpty(relic.RelicId))
        {
            Debug.LogWarning($"[RelicCollectionData] Cannot add relic with empty ID: {relic.name}");
            return false;
        }
        
        // Check if already collected
        if (HasRelic(relic.RelicId))
        {
            Debug.Log($"[RelicCollectionData] Relic '{relic.RelicName}' already in collection.");
            return false;
        }
        
        // Add to collection
        collectedRelics.Add(relic);
        collectedRelicIds.Add(relic.RelicId);
        
        Debug.Log($"[RelicCollectionData] NEW RELIC OBTAINED: '{relic.RelicName}' ({relic.RelicId}). Total relics: {RelicCount}");
        
        // Save immediately
        Save();
        
        return true;
    }
    
    /// <summary>
    /// Checks if a relic is already in the collection
    /// </summary>
    /// <param name="relicId">The relic ID to check</param>
    /// <returns>True if the relic is collected</returns>
    public bool HasRelic(string relicId)
    {
        if (string.IsNullOrEmpty(relicId)) return false;
        return collectedRelicIds.Contains(relicId);
    }
    
    /// <summary>
    /// Gets all collected relics
    /// </summary>
    /// <returns>Read-only list of collected relics</returns>
    public IReadOnlyList<RelicData> GetAllRelics()
    {
        return collectedRelics.AsReadOnly();
    }
    
    /// <summary>
    /// Clears the collection (for new game or debug)
    /// </summary>
    public void Clear()
    {
        collectedRelics.Clear();
        collectedRelicIds.Clear();
        
        // Clear PlayerPrefs
        int count = PlayerPrefs.GetInt(PREFS_PREFIX + PREFS_RELIC_COUNT, 0);
        for (int i = 0; i < count; i++)
        {
            PlayerPrefs.DeleteKey(PREFS_PREFIX + PREFS_RELIC_ID_PREFIX + i);
        }
        PlayerPrefs.DeleteKey(PREFS_PREFIX + PREFS_RELIC_COUNT);
        PlayerPrefs.Save();
        
        Debug.Log("[RelicCollectionData] Collection cleared.");
    }
    
    #endregion
    
    #region Persistence
    
    /// <summary>
    /// Saves collected relic IDs to PlayerPrefs
    /// </summary>
    public void Save()
    {
        // Save count
        PlayerPrefs.SetInt(PREFS_PREFIX + PREFS_RELIC_COUNT, collectedRelics.Count);
        
        // Save each relic ID
        for (int i = 0; i < collectedRelics.Count; i++)
        {
            if (collectedRelics[i] != null && !string.IsNullOrEmpty(collectedRelics[i].RelicId))
            {
                PlayerPrefs.SetString(PREFS_PREFIX + PREFS_RELIC_ID_PREFIX + i, collectedRelics[i].RelicId);
            }
        }
        
        PlayerPrefs.Save();
        Debug.Log($"[RelicCollectionData] Saved {collectedRelics.Count} relics to PlayerPrefs.");
    }
    
    /// <summary>
    /// Loads collected relics from PlayerPrefs
    /// </summary>
    public void Load()
    {
        collectedRelics.Clear();
        collectedRelicIds.Clear();
        
        int count = PlayerPrefs.GetInt(PREFS_PREFIX + PREFS_RELIC_COUNT, 0);
        
        for (int i = 0; i < count; i++)
        {
            string relicId = PlayerPrefs.GetString(PREFS_PREFIX + PREFS_RELIC_ID_PREFIX + i, "");
            
            if (!string.IsNullOrEmpty(relicId))
            {
                // Find the relic asset by ID
                RelicData relic = FindRelicById(relicId);
                
                if (relic != null)
                {
                    collectedRelics.Add(relic);
                    collectedRelicIds.Add(relicId);
                }
                else
                {
                    Debug.LogWarning($"[RelicCollectionData] Could not find relic asset for ID: {relicId}");
                }
            }
        }
        
        Debug.Log($"[RelicCollectionData] Loaded {collectedRelics.Count} relics from PlayerPrefs.");
    }
    
    /// <summary>
    /// Finds a relic asset by its ID
    /// </summary>
    private RelicData FindRelicById(string relicId)
    {
        if (string.IsNullOrEmpty(relicId)) return null;
        
        // Search in pre-configured relic assets
        foreach (var relic in allRelicAssets)
        {
            if (relic != null && relic.RelicId == relicId)
            {
                return relic;
            }
        }
        
        // Fallback: Try to find in Resources
        var allRelics = Resources.LoadAll<RelicData>("Relics");
        return allRelics.FirstOrDefault(r => r.RelicId == relicId);
    }
    
    #endregion
    
    #region Unity Lifecycle
    
    private void OnEnable()
    {
        // Automatically load saved relics when ScriptableObject is enabled
        Load();
    }
    
    #endregion
}
