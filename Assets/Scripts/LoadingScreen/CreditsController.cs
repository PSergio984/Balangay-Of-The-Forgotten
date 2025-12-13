using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Video controller for credits sequence
/// Inherits all video playback functionality from VideoControllerBase
/// </summary>
/// <remarks>
/// <para><strong>Inherits:</strong> VideoControllerBase - All video playback logic is in base class</para>
/// <para><strong>Override:</strong> LoadNextScene() - Transitions to MainMenu after credits finish and clears all game saves</para>
/// </remarks>
public class CreditsController : VideoControllerBase
{
    protected override void Awake()
    {
        base.Awake();
        // Credits videos should not loop the last video
        loopLastVideo = false;
    }
    
    /// <summary>
    /// Load the next scene - transitions to MainMenu after credits finish
    /// Also clears all player prefs and game saves to reset the game for a fresh start
    /// </summary>
    protected override void LoadNextScene()
    {
        // Clear all game saves and player prefs before returning to main menu
        ClearAllGameData();
        
        // Transition to MainMenu
        SceneController.Instance
            .NewTransition()
            .Unload(SceneDatabase.Slots.LoadingScreen)
            .Load(SceneDatabase.Slots.MainMenu, SceneDatabase.Scenes.MainMenu, setActive: true) 
            .Perform();
    }
    
    /// <summary>
    /// Clears all game data including progress, relics, special cards, and all PlayerPrefs
    /// This ensures a completely fresh start when returning to main menu after credits
    /// </summary>
    private void ClearAllGameData()
    {
        Debug.Log("[CreditsController] Clearing all game data and player prefs...");
        
        // Clear GameProgressData (map completions, unlocks, etc.)
        GameProgressData gameProgress = Resources.Load<GameProgressData>("Game Progress");
        if (gameProgress != null)
        {
            gameProgress.ResetProgress();
            Debug.Log("[CreditsController] Cleared GameProgressData");
        }
        else
        {
            Debug.LogWarning("[CreditsController] GameProgressData not found in Resources. Trying alternative path...");
            #if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:GameProgressData");
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                gameProgress = UnityEditor.AssetDatabase.LoadAssetAtPath<GameProgressData>(path);
                if (gameProgress != null)
                {
                    gameProgress.ResetProgress();
                    Debug.Log("[CreditsController] Cleared GameProgressData (found via AssetDatabase)");
                }
            }
            #endif
        }
        
        // Clear RelicCollectionData
        RelicCollectionData relicCollection = Resources.Load<RelicCollectionData>("RelicCollectionData");
        if (relicCollection != null)
        {
            relicCollection.Clear();
            Debug.Log("[CreditsController] Cleared RelicCollectionData");
        }
        else
        {
            Debug.LogWarning("[CreditsController] RelicCollectionData not found in Resources. Trying alternative path...");
            #if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:RelicCollectionData");
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                relicCollection = UnityEditor.AssetDatabase.LoadAssetAtPath<RelicCollectionData>(path);
                if (relicCollection != null)
                {
                    relicCollection.Clear();
                    Debug.Log("[CreditsController] Cleared RelicCollectionData (found via AssetDatabase)");
                }
            }
            #endif
        }
        
        // Clear SpecialCardCollectionData
        SpecialCardCollectionData specialCardCollection = Resources.Load<SpecialCardCollectionData>("SpecialCardCollectionData");
        if (specialCardCollection != null)
        {
            specialCardCollection.Clear();
            Debug.Log("[CreditsController] Cleared SpecialCardCollectionData");
        }
        else
        {
            Debug.LogWarning("[CreditsController] SpecialCardCollectionData not found in Resources. Trying alternative path...");
            #if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:SpecialCardCollectionData");
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                specialCardCollection = UnityEditor.AssetDatabase.LoadAssetAtPath<SpecialCardCollectionData>(path);
                if (specialCardCollection != null)
                {
                    specialCardCollection.Clear();
                    Debug.Log("[CreditsController] Cleared SpecialCardCollectionData (found via AssetDatabase)");
                }
            }
            #endif
        }
        
        // Clear CharacterTransitionData if it exists
        CharacterTransitionData characterData = Resources.Load<CharacterTransitionData>("CharacterTransitionData");
        if (characterData != null)
        {
            characterData.Clear();
            Debug.Log("[CreditsController] Cleared CharacterTransitionData");
        }
        else
        {
            #if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:CharacterTransitionData");
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                characterData = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterTransitionData>(path);
                if (characterData != null)
                {
                    characterData.Clear();
                    Debug.Log("[CreditsController] Cleared CharacterTransitionData (found via AssetDatabase)");
                }
            }
            #endif
        }
        
        // Finally, clear ALL PlayerPrefs as a catch-all to ensure nothing is left behind
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        
        Debug.Log("[CreditsController] All game data and PlayerPrefs cleared. Game reset to fresh state.");
    }
}
