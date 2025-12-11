using UnityEngine;
using System.Collections.Generic;

/* GAME PROGRESS DATA
 * 
 * Purpose: Persists game progression data across scenes and sessions
 * 
 * How it works:
 * - Tracks which maps have been completed
 * - Unlocks Kaluwalhatian after 3 maps are completed
 * - Triggers credits scene after final boss defeat
 * - Uses PlayerPrefs for persistence across game sessions
 * 
 * Integration: Used by VictoryDefeatUI, MapSelectManager2, and SceneController
 */

/// <summary>
/// ScriptableObject for tracking overall game progression.
/// Persists completion state of maps, unlocks, and collected items.
/// <br/><br/>
/// <b>IMPORTANT:</b>
/// <para>
/// This ScriptableObject uses PlayerPrefs for persistence across sessions.
/// Call <see cref="Save"/> after any state change to persist data.
/// Call <see cref="Load"/> on game startup to restore previous progress.
/// </para>
/// </summary>
[CreateAssetMenu(fileName = "GameProgressData", menuName = "Data/Game Progress Data")]
public class GameProgressData : ScriptableObject
{
    #region Constants
    
    // Map IDs - these should match MapData.MapId values
    public const string MAP_ID_DAGAT = "Dagat_ng_Kabisayaan";
    public const string MAP_ID_DARAGANG = "Daragang_Magayon";
    public const string MAP_ID_BUNDOK = "Bundok_Pulag";
    public const string MAP_ID_KALUWALHATIAN = "Kaluwalhatian";
    
    // PlayerPrefs keys for persistence
    private const string PREFS_PREFIX = "GameProgress_";
    private const string PREFS_MAP_COMPLETE_PREFIX = "MapComplete_";
    private const string PREFS_KALUWALHATIAN_UNLOCKED = "KaluwalhatianUnlocked";
    private const string PREFS_GAME_COMPLETED = "GameCompleted";
    private const string PREFS_INTRO_SEEN = "IntroDialogueSeen";
    
    // Number of maps required to unlock Kaluwalhatian
    private const int MAPS_REQUIRED_FOR_FINAL = 3;
    
    #endregion
    
    #region Runtime Data
    
    [Header("Completion Tracking (Runtime Data)")]
    [Tooltip("Set of completed map IDs (persisted via PlayerPrefs)")]
    [SerializeField] private List<string> completedMapIds = new List<string>();
    
    [Tooltip("Whether Kaluwalhatian is unlocked")]
    [SerializeField] private bool isKaluwalhatianUnlocked = false;
    
    [Tooltip("Whether the game has been fully completed")]
    [SerializeField] private bool isGameCompleted = false;
    
    [Tooltip("Whether the intro dialogue has been shown")]
    [SerializeField] private bool hasSeenIntroDialogue = false;
    
    #endregion
    
    #region Public Properties
    
    /// <summary>
    /// Total number of maps completed
    /// </summary>
    public int CompletedMapCount => completedMapIds.Count;
    
    /// <summary>
    /// Whether Kaluwalhatian is unlocked (requires 3 map completions)
    /// </summary>
    public bool IsKaluwalhatianUnlocked => isKaluwalhatianUnlocked;
    
    /// <summary>
    /// Whether the entire game has been completed (Kaluwalhatian defeated)
    /// </summary>
    public bool IsGameCompleted => isGameCompleted;
    
    /// <summary>
    /// Whether the intro dialogue has been shown to the player
    /// </summary>
    public bool HasSeenIntroDialogue => hasSeenIntroDialogue;
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Marks a map as completed and checks for unlocks
    /// </summary>
    /// <param name="mapId">The ID of the completed map (e.g., "Dagat_ng_Kabisayaan")</param>
    /// <returns>True if this was a new completion, false if already completed</returns>
    public bool MarkMapComplete(string mapId)
    {
        if (string.IsNullOrEmpty(mapId))
        {
            Debug.LogWarning("[GameProgressData] Cannot mark null or empty mapId as complete.");
            return false;
        }
        
        // Check if already completed
        if (IsMapComplete(mapId))
        {
            Debug.Log($"[GameProgressData] Map '{mapId}' was already completed.");
            return false;
        }
        
        // Add to completed list
        completedMapIds.Add(mapId);
        Debug.Log($"[GameProgressData] Map '{mapId}' marked as complete. Total: {CompletedMapCount}/{MAPS_REQUIRED_FOR_FINAL}");
        
        // Check if this was the final boss (Kaluwalhatian)
        if (mapId == MAP_ID_KALUWALHATIAN)
        {
            isGameCompleted = true;
            Debug.Log("[GameProgressData] GAME COMPLETED! Final boss defeated.");
        }
        
        // Check if we should unlock Kaluwalhatian
        CheckKaluwalhatianUnlock();
        
        // Save immediately after state change
        Save();
        
        return true;
    }
    
    /// <summary>
    /// Checks if a specific map has been completed
    /// </summary>
    /// <param name="mapId">The map ID to check</param>
    /// <returns>True if the map has been completed</returns>
    public bool IsMapComplete(string mapId)
    {
        if (string.IsNullOrEmpty(mapId)) return false;
        return completedMapIds.Contains(mapId);
    }
    
    /// <summary>
    /// Checks and updates Kaluwalhatian unlock status
    /// </summary>
    /// <returns>True if Kaluwalhatian is now unlocked</returns>
    public bool CheckKaluwalhatianUnlock()
    {
        // Count completions of the first 3 maps (not Kaluwalhatian itself)
        int prerequisiteCount = 0;
        
        if (IsMapComplete(MAP_ID_DAGAT)) prerequisiteCount++;
        if (IsMapComplete(MAP_ID_DARAGANG)) prerequisiteCount++;
        if (IsMapComplete(MAP_ID_BUNDOK)) prerequisiteCount++;
        
        bool shouldUnlock = prerequisiteCount >= MAPS_REQUIRED_FOR_FINAL;
        
        if (shouldUnlock && !isKaluwalhatianUnlocked)
        {
            isKaluwalhatianUnlocked = true;
            Debug.Log("[GameProgressData] KALUWALHATIAN UNLOCKED! All prerequisite maps completed.");
            Save();
        }
        
        return isKaluwalhatianUnlocked;
    }
    
    /// <summary>
    /// Checks if all maps are complete (including Kaluwalhatian)
    /// </summary>
    /// <returns>True if all 4 maps are complete</returns>
    public bool CheckAllMapsComplete()
    {
        return IsMapComplete(MAP_ID_DAGAT) &&
               IsMapComplete(MAP_ID_DARAGANG) &&
               IsMapComplete(MAP_ID_BUNDOK) &&
               IsMapComplete(MAP_ID_KALUWALHATIAN);
    }
    
    /// <summary>
    /// Marks the intro dialogue as seen
    /// </summary>
    public void MarkIntroDialogueSeen()
    {
        if (!hasSeenIntroDialogue)
        {
            hasSeenIntroDialogue = true;
            Save();
            Debug.Log("[GameProgressData] Intro dialogue marked as seen.");
        }
    }
    
    /// <summary>
    /// Gets all completed map IDs
    /// </summary>
    /// <returns>Read-only list of completed map IDs</returns>
    public IReadOnlyList<string> GetCompletedMaps()
    {
        return completedMapIds.AsReadOnly();
    }
    
    #endregion
    
    #region Persistence
    
    /// <summary>
    /// Saves current progress to PlayerPrefs
    /// </summary>
    public void Save()
    {
        // Save each map's completion status
        PlayerPrefs.SetInt(GetMapPrefsKey(MAP_ID_DAGAT), IsMapComplete(MAP_ID_DAGAT) ? 1 : 0);
        PlayerPrefs.SetInt(GetMapPrefsKey(MAP_ID_DARAGANG), IsMapComplete(MAP_ID_DARAGANG) ? 1 : 0);
        PlayerPrefs.SetInt(GetMapPrefsKey(MAP_ID_BUNDOK), IsMapComplete(MAP_ID_BUNDOK) ? 1 : 0);
        PlayerPrefs.SetInt(GetMapPrefsKey(MAP_ID_KALUWALHATIAN), IsMapComplete(MAP_ID_KALUWALHATIAN) ? 1 : 0);
        
        // Save unlock and completion states
        PlayerPrefs.SetInt(PREFS_PREFIX + PREFS_KALUWALHATIAN_UNLOCKED, isKaluwalhatianUnlocked ? 1 : 0);
        PlayerPrefs.SetInt(PREFS_PREFIX + PREFS_GAME_COMPLETED, isGameCompleted ? 1 : 0);
        PlayerPrefs.SetInt(PREFS_PREFIX + PREFS_INTRO_SEEN, hasSeenIntroDialogue ? 1 : 0);
        
        PlayerPrefs.Save();
        Debug.Log($"[GameProgressData] Progress saved. Completed maps: {CompletedMapCount}, Kaluwalhatian unlocked: {isKaluwalhatianUnlocked}");
    }
    
    /// <summary>
    /// Loads progress from PlayerPrefs
    /// </summary>
    public void Load()
    {
        completedMapIds.Clear();
        
        // Load each map's completion status
        if (PlayerPrefs.GetInt(GetMapPrefsKey(MAP_ID_DAGAT), 0) == 1)
            completedMapIds.Add(MAP_ID_DAGAT);
        if (PlayerPrefs.GetInt(GetMapPrefsKey(MAP_ID_DARAGANG), 0) == 1)
            completedMapIds.Add(MAP_ID_DARAGANG);
        if (PlayerPrefs.GetInt(GetMapPrefsKey(MAP_ID_BUNDOK), 0) == 1)
            completedMapIds.Add(MAP_ID_BUNDOK);
        if (PlayerPrefs.GetInt(GetMapPrefsKey(MAP_ID_KALUWALHATIAN), 0) == 1)
            completedMapIds.Add(MAP_ID_KALUWALHATIAN);
        
        // Load unlock and completion states
        isKaluwalhatianUnlocked = PlayerPrefs.GetInt(PREFS_PREFIX + PREFS_KALUWALHATIAN_UNLOCKED, 0) == 1;
        isGameCompleted = PlayerPrefs.GetInt(PREFS_PREFIX + PREFS_GAME_COMPLETED, 0) == 1;
        hasSeenIntroDialogue = PlayerPrefs.GetInt(PREFS_PREFIX + PREFS_INTRO_SEEN, 0) == 1;
        
        Debug.Log($"[GameProgressData] Progress loaded. Completed maps: {CompletedMapCount}, Kaluwalhatian unlocked: {isKaluwalhatianUnlocked}");
    }
    
    /// <summary>
    /// Resets all progress (for new game or debug)
    /// </summary>
    public void ResetProgress()
    {
        completedMapIds.Clear();
        isKaluwalhatianUnlocked = false;
        isGameCompleted = false;
        hasSeenIntroDialogue = false;
        
        // Clear all PlayerPrefs for this system
        PlayerPrefs.DeleteKey(GetMapPrefsKey(MAP_ID_DAGAT));
        PlayerPrefs.DeleteKey(GetMapPrefsKey(MAP_ID_DARAGANG));
        PlayerPrefs.DeleteKey(GetMapPrefsKey(MAP_ID_BUNDOK));
        PlayerPrefs.DeleteKey(GetMapPrefsKey(MAP_ID_KALUWALHATIAN));
        PlayerPrefs.DeleteKey(PREFS_PREFIX + PREFS_KALUWALHATIAN_UNLOCKED);
        PlayerPrefs.DeleteKey(PREFS_PREFIX + PREFS_GAME_COMPLETED);
        PlayerPrefs.DeleteKey(PREFS_PREFIX + PREFS_INTRO_SEEN);
        
        PlayerPrefs.Save();
        Debug.Log("[GameProgressData] All progress has been reset.");
    }
    
    private string GetMapPrefsKey(string mapId)
    {
        return PREFS_PREFIX + PREFS_MAP_COMPLETE_PREFIX + mapId;
    }
    
    #endregion
    
    #region Unity Lifecycle
    
    private void OnEnable()
    {
        // Automatically load saved progress when ScriptableObject is enabled
        Load();
    }
    
    #endregion
}
