#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Automatically clears relic PlayerPrefs when exiting Play Mode in Unity Editor.
/// Only active in Editor - does nothing in actual game builds.
/// Useful for testing to ensure clean state each time you enter Play Mode.
/// </summary>
[InitializeOnLoad]
public static class AutoClearPlayerPrefsOnPlayModeExit
{
    private static bool wasPlaying = false;
    
    static AutoClearPlayerPrefsOnPlayModeExit()
    {
        // Subscribe to play mode state changes
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }
    
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        // When exiting Play Mode
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            //ClearRelicPlayerPrefs();
        }
    }
    
    /// <summary>
    /// Clears all relic-related PlayerPrefs
    /// </summary>
    private static void ClearRelicPlayerPrefs()
    {
        string prefix = "RelicCollection_";
        int count = PlayerPrefs.GetInt(prefix + "RelicCount", 0);
        
        if (count > 0)
        {
            // Delete all relic ID keys
            for (int i = 0; i < count; i++)
            {
                PlayerPrefs.DeleteKey(prefix + "RelicId_" + i);
            }
            
            // Delete the count key
            PlayerPrefs.DeleteKey(prefix + "RelicCount");
            PlayerPrefs.Save();
            
            Debug.Log($"[AutoClearPlayerPrefs] Cleared {count} relic(s) from PlayerPrefs on Play Mode exit (Editor only).");
        }
    }
}
#endif
