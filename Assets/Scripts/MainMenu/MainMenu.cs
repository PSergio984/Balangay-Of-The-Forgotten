using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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
    
    /// <summary>
    /// Button that quits the game when clicked
    /// </summary>
    /// <remarks>
    /// When players click this button, it exits the application. In editor, it stops 
    /// play mode instead of trying to quit (which would cause issues).
    /// Assign a UI Button component in the Inspector.
    /// </remarks>
    [SerializeField] private Button QuitButton;

    /// <summary>
    /// Sets up button listeners when the menu loads
    /// </summary>
    /// <remarks>
    /// Unity calls this automatically when the scene starts. Connects the button 
    /// click events to their respective methods so the menu responds to player input.
    /// </remarks>
    void Start()
    {
        // Add listener for the Quit button to call the QuitGame method when clicked
        QuitButton.onClick.AddListener(QuitGame);
    }
    /// <summary>
    /// Initiates a new game session by transitioning from the menu to the session scenes
    /// </summary>
    /// <remarks>
    /// Performs a complex scene transition that loads the session infrastructure and map selection,
    /// unloads the menu, and applies transition effects. This should be called when the player
    /// clicks the start button.
    /// </remarks>
    public void StartSession()
    {
        if (SceneController.Instance == null)
        {
            Debug.LogError("SceneController.Instance is null. Cannot start session.");
            return;
        }
        
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Session, SceneDatabase.Scenes.Session)
            .Load(SceneDatabase.Slots.SessionContent, SceneDatabase.Scenes.MapSelection, setActive: true)
            .Unload(SceneDatabase.Slots.Menu)
            .WithOverlay()
            .WithClearUnusedAssets()
            .Perform();
    }    

    /// <summary>
    /// Quits the application or stops editor play mode
    /// </summary>
    /// <remarks>
    /// Called when the quit button is clicked. In a built game, this closes the 
    /// application. In the Unity editor, it stops play mode instead of trying 
    /// to quit (which would cause problems in the editor).
    /// </remarks>
    private void QuitGame()
    {
        #if UNITY_EDITOR
                if (Application.isEditor)
                {
                    // Stop playing the scene in the editor
                    EditorApplication.isPlaying = false;
                }
                else
        #endif  
            {
            // Quit the application
            Application.Quit();
        }
    }
}
