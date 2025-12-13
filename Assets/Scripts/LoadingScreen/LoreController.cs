using UnityEngine;
using AudioSystem;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Video controller for lore cutscenes
/// Inherits all video playback functionality from VideoControllerBase
/// </summary>
/// <remarks>
/// <para><strong>Inherits:</strong> VideoControllerBase - All video playback logic is in base class</para>
/// <para><strong>Override:</strong> LoadNextScene() - Transitions to MainMenu after lore videos finish</para>
/// </remarks>
public class LoreController : VideoControllerBase
{
    [Header("Music Settings")]
    [Tooltip("Music to play when transitioning to map selection")]
    [SerializeField] private SoundData mapSelectionMusic;
    
    [Tooltip("Fade time for music transition")]
    [SerializeField] private float MusicFadeTime = 2f;
    
    protected override void Awake()
    {
        base.Awake();
        // Lore videos should not loop the last video
        loopLastVideo = false;
        
        Debug.Log($"[LoreController] Awake() called on {gameObject.name}. mapSelectionMusic is {(mapSelectionMusic != null ? $"assigned: {mapSelectionMusic.name}" : "NULL")}");
        
        // Try to find mapSelectionMusic if not assigned
        if (mapSelectionMusic == null)
        {
            Debug.LogWarning("[LoreController] mapSelectionMusic not assigned in Inspector. Attempting to find asset automatically...");
            
            // Try to load from Resources first (common location for SoundData assets)
            mapSelectionMusic = Resources.Load<SoundData>("MapMusic");
            
            // If not in Resources, try to find it in the project (Editor only)
            if (mapSelectionMusic == null)
            {
                #if UNITY_EDITOR
                // Use UnityEditor API to find the asset
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:SoundData mapMusic");
                if (guids.Length == 0)
                {
                    // Try without the "mapMusic" filter - search for any SoundData with "map" in the name
                    guids = UnityEditor.AssetDatabase.FindAssets("t:SoundData");
                    foreach (string guid in guids)
                    {
                        string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                        string assetName = System.IO.Path.GetFileNameWithoutExtension(path).ToLower();
                        if (assetName.Contains("map") && assetName.Contains("music"))
                        {
                            mapSelectionMusic = UnityEditor.AssetDatabase.LoadAssetAtPath<SoundData>(path);
                            Debug.Log($"[LoreController] Found mapSelectionMusic at: {path}");
                            break;
                        }
                    }
                }
                else
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    mapSelectionMusic = UnityEditor.AssetDatabase.LoadAssetAtPath<SoundData>(path);
                    Debug.Log($"[LoreController] Found mapSelectionMusic at: {path}");
                }
                #endif
            }
            
            if (mapSelectionMusic == null)
            {
                Debug.LogError("[LoreController] mapSelectionMusic could not be found automatically! Please assign it in the Inspector or create it in Resources/MapMusic.asset", this);
            }
            else
            {
                Debug.Log("[LoreController] mapSelectionMusic found and assigned automatically.");
            }
        }
    }
    
    /// <summary>
    /// Load the next scene - transitions to MainMenu after lore videos finish
    /// </summary>
    protected override void LoadNextScene()
    {
        Debug.Log($"[LoreController] LoadNextScene() called on {gameObject.name}. mapSelectionMusic is {(mapSelectionMusic != null ? $"assigned: {mapSelectionMusic.name}" : "NULL")}");
        
        // Try to find mapSelectionMusic again if it's null (in case it was cleared between Awake and LoadNextScene)
        if (mapSelectionMusic == null)
        {
            Debug.LogWarning("[LoreController] mapSelectionMusic is NULL in LoadNextScene! Attempting to find again...");
            
            // Try to load from Resources first
            mapSelectionMusic = Resources.Load<SoundData>("MapMusic");
            
            // If not in Resources, try to find it in the project (Editor only)
            if (mapSelectionMusic == null)
            {
                #if UNITY_EDITOR
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:SoundData");
                foreach (string guid in guids)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                    string assetName = System.IO.Path.GetFileNameWithoutExtension(path).ToLower();
                    if (assetName.Contains("map") && assetName.Contains("music"))
                    {
                        mapSelectionMusic = UnityEditor.AssetDatabase.LoadAssetAtPath<SoundData>(path);
                        Debug.Log($"[LoreController] Found mapSelectionMusic at: {path}");
                        break;
                    }
                }
                #endif
            }
        }
        
        // Validate mapSelectionMusic is assigned (double-check in case it was cleared)
        if (mapSelectionMusic == null)
        {
            Debug.LogError($"[LoreController] mapSelectionMusic is NULL on GameObject '{gameObject.name}'! Music will not play. " +
                          $"Please check:\n" +
                          $"1. Is the assignment on the correct GameObject instance (not just the prefab)?\n" +
                          $"2. Is the GameObject in the scene (not just in a prefab)?\n" +
                          $"3. Assign a SoundData asset in the Inspector.", this);
        }
        else if (mapSelectionMusic.clip == null)
        {
            Debug.LogError($"[LoreController] mapSelectionMusic ({mapSelectionMusic.name}) has no clip assigned! Music will not play. Assign an AudioClip to the SoundData asset.", this);
        }
        else
        {
            Debug.Log($"[LoreController] mapSelectionMusic is assigned: {mapSelectionMusic.name}, Clip: {mapSelectionMusic.clip.name}, Volume: {mapSelectionMusic.volume}");
        }
        
        // Check MusicManager
        if (MusicManager.Instance == null)
        {
            Debug.LogError("[LoreController] MusicManager.Instance is NULL! Music cannot play. Ensure MusicManager exists in the scene.", this);
        }
        else
        {
            Debug.Log("[LoreController] MusicManager.Instance is available");
        }
        
        // Check SceneController before using it
        if (SceneController.Instance == null)
        {
            Debug.LogError("[LoreController] SceneController.Instance is NULL! Cannot transition to next scene. Ensure SceneController exists in the scene.", this);
            return; // Exit early to prevent NullReferenceException
        }
        
        // Transition with smooth music crossfade
        var transition = SceneController.Instance
            .NewTransition()
            .Unload(SceneDatabase.Slots.SessionContent)
            .Load(SceneDatabase.Slots.SessionContent, SceneDatabase.Scenes.MapSelection, setActive: true);
        
        if (mapSelectionMusic != null)
        {
            transition = transition.WithMusic(mapSelectionMusic, MusicFadeTime);
        }
        
        Debug.Log($"[LoreController] Created transition with Music: {(mapSelectionMusic != null ? mapSelectionMusic.name : "NULL")}, FadeTime: {MusicFadeTime}");
        transition.Perform();
    }
}
