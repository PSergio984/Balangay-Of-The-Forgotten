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
    
    // Map IDs - these should match MapData.MapId values (with spaces as they appear in MapData assets)
    public const string MAP_ID_DAGAT = "Dagat Ng Kabisayaan";
    public const string MAP_ID_DARAGANG = "Daragang Magayon";
    public const string MAP_ID_BUNDOK = "Bundok Pulag";
    public const string MAP_ID_KALUWALHATIAN = "Kaluwalhatian";
    
    // PlayerPrefs keys for persistence
    private const string PREFS_PREFIX = "GameProgress_";
    private const string PREFS_MAP_COMPLETE_PREFIX = "MapComplete_";
    private const string PREFS_KALUWALHATIAN_UNLOCKED = "KaluwalhatianUnlocked";
    private const string PREFS_GAME_COMPLETED = "GameCompleted";
    private const string PREFS_INTRO_SEEN = "IntroDialogueSeen";
    private const string PREFS_PLAYER_NAME = "PlayerName";
    
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
    
    [Tooltip("Active session player name")]
    [SerializeField] private string playerName = string.Empty;
    
    #endregion
    
    #region Public Properties
    
    /// <summary>
    /// Raised whenever the session player name state changes (set, normalized, or reset),
    /// so UI that depends on <see cref="HasPlayerName"/> (e.g. the main menu Start button gate)
    /// can re-evaluate without polling.
    /// </summary>
    public static event System.Action PlayerNameStateChanged;

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
    
    /// <summary>
    /// Active session player name (defaults to "Anonymous" if empty).
    /// </summary>
    public string PlayerName => string.IsNullOrWhiteSpace(playerName) ? "Anonymous" : playerName;
    
    /// <summary>
    /// Whether a non-empty player name has been set for this session.
    /// </summary>
    public bool HasPlayerName => !string.IsNullOrWhiteSpace(playerName);
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Sets and normalizes the active session player name.
    /// </summary>
    /// <param name="name">Player name to set.</param>
    public void SetPlayerName(string name)
    {
        string normalized = name != null ? name.Trim() : string.Empty;
        if (normalized.Length > 20)
        {
            normalized = normalized.Substring(0, 20);
        }
        playerName = normalized;
        Save();
        Debug.Log($"[GameProgressData] Session player name set to '{playerName}'.");
        PlayerNameStateChanged?.Invoke();
    }
    
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
        // Save each map's completion status based on what's actually in completedMapIds
        int dagatComplete = completedMapIds.Contains(MAP_ID_DAGAT) ? 1 : 0;
        int daragangComplete = completedMapIds.Contains(MAP_ID_DARAGANG) ? 1 : 0;
        int bundokComplete = completedMapIds.Contains(MAP_ID_BUNDOK) ? 1 : 0;
        int kaluwalhatianComplete = completedMapIds.Contains(MAP_ID_KALUWALHATIAN) ? 1 : 0;
        
        PlayerPrefs.SetInt(GetMapPrefsKey(MAP_ID_DAGAT), dagatComplete);
        PlayerPrefs.SetInt(GetMapPrefsKey(MAP_ID_DARAGANG), daragangComplete);
        PlayerPrefs.SetInt(GetMapPrefsKey(MAP_ID_BUNDOK), bundokComplete);
        PlayerPrefs.SetInt(GetMapPrefsKey(MAP_ID_KALUWALHATIAN), kaluwalhatianComplete);
        
        // Save unlock and completion states
        PlayerPrefs.SetInt(PREFS_PREFIX + PREFS_KALUWALHATIAN_UNLOCKED, isKaluwalhatianUnlocked ? 1 : 0);
        PlayerPrefs.SetInt(PREFS_PREFIX + PREFS_GAME_COMPLETED, isGameCompleted ? 1 : 0);
        PlayerPrefs.SetInt(PREFS_PREFIX + PREFS_INTRO_SEEN, hasSeenIntroDialogue ? 1 : 0);
        PlayerPrefs.SetString(PREFS_PREFIX + PREFS_PLAYER_NAME, playerName ?? string.Empty);
        
        PlayerPrefs.Save();
        
        // Enhanced debug logging to verify save
        Debug.Log($"[GameProgressData] Progress saved. Completed maps: {CompletedMapCount}, PlayerName: '{PlayerName}'");
        Debug.Log($"[GameProgressData] Map completion status - Dagat: {dagatComplete}, Daragang: {daragangComplete}, Bundok: {bundokComplete}, Kaluwalhatian: {kaluwalhatianComplete}");
        Debug.Log($"[GameProgressData] Kaluwalhatian unlocked: {isKaluwalhatianUnlocked}, Game completed: {isGameCompleted}");
        Debug.Log($"[GameProgressData] Completed map IDs: {string.Join(", ", completedMapIds)}");
    }
    
    /// <summary>
    /// Loads progress from PlayerPrefs
    /// </summary>
    public void Load()
    {
        completedMapIds.Clear();
        
        // Load each map's completion status
        int dagatValue = PlayerPrefs.GetInt(GetMapPrefsKey(MAP_ID_DAGAT), 0);
        int daragangValue = PlayerPrefs.GetInt(GetMapPrefsKey(MAP_ID_DARAGANG), 0);
        int bundokValue = PlayerPrefs.GetInt(GetMapPrefsKey(MAP_ID_BUNDOK), 0);
        int kaluwalhatianValue = PlayerPrefs.GetInt(GetMapPrefsKey(MAP_ID_KALUWALHATIAN), 0);
        
        if (dagatValue == 1)
            completedMapIds.Add(MAP_ID_DAGAT);
        if (daragangValue == 1)
            completedMapIds.Add(MAP_ID_DARAGANG);
        if (bundokValue == 1)
            completedMapIds.Add(MAP_ID_BUNDOK);
        if (kaluwalhatianValue == 1)
            completedMapIds.Add(MAP_ID_KALUWALHATIAN);
        
        // Load unlock and completion states
        isKaluwalhatianUnlocked = PlayerPrefs.GetInt(PREFS_PREFIX + PREFS_KALUWALHATIAN_UNLOCKED, 0) == 1;
        isGameCompleted = PlayerPrefs.GetInt(PREFS_PREFIX + PREFS_GAME_COMPLETED, 0) == 1;
        hasSeenIntroDialogue = PlayerPrefs.GetInt(PREFS_PREFIX + PREFS_INTRO_SEEN, 0) == 1;
        playerName = PlayerPrefs.GetString(PREFS_PREFIX + PREFS_PLAYER_NAME, string.Empty);
        
        // Enhanced debug logging to verify load
        Debug.Log($"[GameProgressData] Progress loaded. Completed maps: {CompletedMapCount}, Kaluwalhatian unlocked: {isKaluwalhatianUnlocked}, PlayerName: '{PlayerName}'");
        Debug.Log($"[GameProgressData] Loaded from PlayerPrefs - Dagat: {dagatValue}, Daragang: {daragangValue}, Bundok: {bundokValue}, Kaluwalhatian: {kaluwalhatianValue}");
        Debug.Log($"[GameProgressData] Loaded completed map IDs: {string.Join(", ", completedMapIds)}");
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
        playerName = string.Empty;
        
        // Clear all PlayerPrefs for this system
        PlayerPrefs.DeleteKey(GetMapPrefsKey(MAP_ID_DAGAT));
        PlayerPrefs.DeleteKey(GetMapPrefsKey(MAP_ID_DARAGANG));
        PlayerPrefs.DeleteKey(GetMapPrefsKey(MAP_ID_BUNDOK));
        PlayerPrefs.DeleteKey(GetMapPrefsKey(MAP_ID_KALUWALHATIAN));
        PlayerPrefs.DeleteKey(PREFS_PREFIX + PREFS_KALUWALHATIAN_UNLOCKED);
        PlayerPrefs.DeleteKey(PREFS_PREFIX + PREFS_GAME_COMPLETED);
        PlayerPrefs.DeleteKey(PREFS_PREFIX + PREFS_INTRO_SEEN);
        PlayerPrefs.DeleteKey(PREFS_PREFIX + PREFS_PLAYER_NAME);
        
        PlayerPrefs.Save();
        Debug.Log("[GameProgressData] All progress has been reset.");
        PlayerNameStateChanged?.Invoke();
    }
    
    private string GetMapPrefsKey(string mapId)
    {
        return PREFS_PREFIX + PREFS_MAP_COMPLETE_PREFIX + mapId;
    }
    
    #endregion
    
    #region Unity Lifecycle

    /// <summary>
    /// Runtime singleton reference. Resolved on first <see cref="OnEnable"/>.
    /// Multiple components (e.g. <c>NewPlayerButtonUI</c>, <c>SessionNameEntryUI</c>)
    /// should prefer this over a separately-asset-wired serialized field, because a
    /// ScriptableObject loaded through two different asset references would be two
    /// separate runtime instances and a reset on one would not be visible to the other.
    /// </summary>
    public static GameProgressData Instance { get; private set; }

    private void OnEnable()
    {
        if (Instance != null && Instance != this)
        {
            return;
        }
        Instance = this;
        // Automatically load saved progress when ScriptableObject is enabled
        Load();
    }

    private void OnDisable()
    {
        // Clear the singleton reference so a later instance can take over after a
        // domain reload or scene teardown instead of pointing at a stale object.
        if (Instance == this)
        {
            Instance = null;
        }
    }

    #endregion
}
