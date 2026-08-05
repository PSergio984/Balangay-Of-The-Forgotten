using UnityEngine;
using UnityEngine.UI;
using AudioSystem;
using System.Runtime.InteropServices;

#if UNITY_EDITOR
using UnityEditor;
#endif

/* MAIN MENU DOCUMENTATION
 * 
 * Purpose: Handles the main menu screen with start and quit functionality
 * 
 * How it works:
 * - Shows start and quit buttons to the player
 * - Start button loads the game scene to begin playing
 * - Quit button exits the application (or stops play mode in editor)
 * - Simple menu system for game entry point
 * 
 * Integration: Entry point for the game, uses Unity's scene management system
 */

/// <summary>
/// Manages the main menu screen with start and quit functionality
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> The first screen players see when launching the game</para>
/// 
/// <para><strong>What it does:</strong> This is the main menu that players see when they 
/// start the game. It provides two basic options: start playing the game or quit the 
/// application. When players click start, it loads the main game scene. When they click 
/// quit, it closes the game (or stops play mode if running in the Unity editor).</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Unity calls Start() when the menu scene loads</item>
/// <item>System sets up button click listeners for start and quit</item>
/// <item>Player clicks Start → loads scene 1 (main game scene)</item>
/// <item>Player clicks Quit → closes application or stops editor playmode</item>
/// <item>Simple and straightforward menu navigation</item>
/// </list>
/// 
/// <para><strong>Features:</strong></para>
/// <list type="bullet">
/// <item>Start button to begin the game</item>
/// <item>Quit button to exit the application</item>
/// <item>Editor-safe quitting (stops play mode instead of crashing)</item>
/// <item>Scene management for transitioning to gameplay</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> Unity SceneManager for scene loading, UI buttons for interaction</para>
/// 
/// <para><strong>How to use:</strong> Assign start and quit buttons in Inspector, place on main menu scene</para>
/// </remarks>
public class MainMenu : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    /// <summary>
    /// JavaScript function to refresh the page in WebGL builds.
    /// This calls location.reload() in the browser to properly refresh.
    /// </summary>
    [DllImport("__Internal")]
    private static extern void RefreshPage();
#endif

    [SerializeField] private SoundData mapSelectionMusic;
    [SerializeField] private float MusicFadeTime = 2f;
    public float MusicFadeDuration => MusicFadeTime;
    
    [Header("Character Data")]
    [Tooltip("Reference to CharacterTransitionData - will be cleared when starting a new game session")]
    [SerializeField] private CharacterTransitionData characterTransitionData;

    [Header("Leaderboard")]
    [Tooltip("Leaderboard overlay UI component")]
    [SerializeField] private LeaderboardUI leaderboardUI;

    [Tooltip("Button to open leaderboard overlay panel")]
    [SerializeField] private Button leaderboardButton;

    [Header("Session Name Gate")]
    [Tooltip("Main menu Start button. Leave null to auto-find a Button named 'Start'.")]
    [SerializeField] private Button startButton;

    [Tooltip("Pre-session name entry panel. Shown when Start is clicked without a session player name.")]
    [SerializeField] private SessionNameEntryUI sessionNameEntryUI;

    [Tooltip("GameProgressData used for the name gate. Leave null to use the runtime singleton.")]
    [SerializeField] private GameProgressData gameProgressData;

    private void Start()
    {
        if (leaderboardButton != null)
        {
            leaderboardButton.onClick.AddListener(OpenLeaderboard);
        }

        if (startButton == null)
        {
            startButton = FindStartButton();
        }
        if (sessionNameEntryUI == null)
        {
            sessionNameEntryUI = FindObjectOfType<SessionNameEntryUI>(true);
        }
    }

    /// <summary>
    /// Auto-finds the Start button by GameObject name when it is not serialized.
    /// The MainMenu scene names the button GameObject "Start".
    /// </summary>
    private Button FindStartButton()
    {
        foreach (var button in GetComponentsInChildren<Button>(true))
        {
            if (button != null && button.gameObject.name == "Start")
            {
                return button;
            }
        }
        return null;
    }

    /// <summary>
    /// Resolves GameProgressData at runtime, preferring the serialized field and
    /// falling back to the singleton (mirrors SessionNameEntryUI.ResolveData).
    /// </summary>
    private GameProgressData ResolveProgressData()
    {
        return gameProgressData != null ? gameProgressData : GameProgressData.Instance;
    }

    /// <summary>
    /// Opens the Leaderboard overlay panel.
    /// </summary>
    public void OpenLeaderboard()
    {
        if (leaderboardUI != null)
        {
            leaderboardUI.Open();
        }
        else
        {
            Debug.LogWarning("[MainMenu] leaderboardUI is null! Cannot open leaderboard panel.", this);
        }
    }


    /// <summary>
    /// Initiates a new game session by transitioning from the menu to the session scenes
    /// </summary>
    /// <remarks>
    /// Performs a complex scene transition that loads the session infrastructure and map selection,
    /// unloads the menu, and applies transition effects. This should be called when the player
    /// clicks the start button.
    /// 
    /// Also clears CharacterTransitionData to ensure a fresh start for character selection.
    /// </remarks>
    public void StartSession()
    {
        var progressData = ResolveProgressData();
        if (progressData != null && !progressData.HasPlayerName)
        {
            Debug.Log("[MainMenu] No session player name. Opening name entry panel instead of starting.");
            if (sessionNameEntryUI != null)
            {
                sessionNameEntryUI.Show();
            }
            else
            {
                Debug.LogWarning("[MainMenu] sessionNameEntryUI is null! Cannot open name entry panel.", this);
            }
            return;
        }

        if (SceneController.Instance == null)
        {
            Debug.LogError("SceneController.Instance is null. Cannot start session.");
            return;
        }
        
        // Clear character transition data when starting a new game session
        // This ensures old character selections don't persist into a new game
        if (characterTransitionData == null)
        {
            // Try to find it automatically
            #if UNITY_EDITOR
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:CharacterTransitionData");
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                characterTransitionData = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterTransitionData>(path);
            }
            #endif
            
            if (characterTransitionData == null)
            {
                characterTransitionData = Resources.Load<CharacterTransitionData>("CharacterTransitionData");
            }
        }
        
        if (characterTransitionData != null)
        {
            characterTransitionData.Clear();
            Debug.Log("[MainMenu] Cleared CharacterTransitionData for new game session");
        }
        else
        {
            Debug.LogWarning("[MainMenu] CharacterTransitionData not found - cannot clear old data. This is okay if starting first game.");
        }
        
        SceneController.Instance
            .NewTransition()
            .Unload(SceneDatabase.Slots.MainMenu)
            .Load(SceneDatabase.Slots.SessionContent, SceneDatabase.Scenes.CharacterSelection, setActive: true)
            .WithLoadingVideo("loading")
            .WithPauseMusic()
            .Perform();
    }

    /// <summary>
    /// Quits the application or stops editor play mode
    /// </summary>
    /// <remarks>
    /// Called when the quit button is clicked. Behavior depends on platform:
    /// - Editor: Stops play mode
    /// - WebGL: Refreshes the page using JavaScript (since Application.Quit doesn't work)
    /// - Standalone: Quits the application
    /// </remarks>
    public void QuitGame()
    {
        #if UNITY_EDITOR
        // Stop playing the scene in the editor
        EditorApplication.isPlaying = false;
        #elif UNITY_WEBGL
        // On WebGL, use JavaScript to refresh the page properly
        RefreshPage();
        #else
        // Quit the application on standalone builds
        Application.Quit();
        #endif
    }

    /// <summary>
    /// Initiates a scene transition back to the loading screen.
    /// </summary>
    /// <remarks>
    /// Triggers a transition that unloads the main menu, loads the loading scene, sets it active,
    /// plays the loading video, and performs the transition. This is typically used to return to the
    /// loading screen from the main menu, ensuring a consistent transition effect and video playback.
    /// </remarks>
    public void GoBackLoadingScreen()
    {
        SceneController.Instance
            .NewTransition()
            .Unload(SceneDatabase.Slots.MainMenu)
            .Load(SceneDatabase.Slots.LoadingScreen, SceneDatabase.Scenes.LoadingScreen, setActive: true)
            .WithLoadingVideo("loading")
            .Perform();
    }
}
