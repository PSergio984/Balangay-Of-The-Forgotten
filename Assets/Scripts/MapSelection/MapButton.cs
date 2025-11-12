using TMPro;
using UnityEngine.UI;
using UnityEngine;
using AudioSystem;


/* MAP BUTTON
 * 
 * Purpose: A clickable button that represents one map/level in the map selection screen
 * 
 * How it works:
 * - Shows the map name on the button
 * - Displays if the map is locked (gray) or unlocked (white)
 * - When clicked, loads the combat scene with music and transition effects
 * 
 * Integration: Created by MapSelectManager for each available map
 */


/// <summary>
/// A clickable button for selecting a map. Shows map name and handles loading the combat scene.
/// </summary>
/// <remarks>
/// <para><strong>Why:</strong> Makes maps selectable with visual feedback (like level select in Mario games)</para>
/// <para><strong>How:</strong> Changes color based on lock state, plays music, and loads combat scene when clicked</para>
/// </remarks>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class MapButton : MonoBehaviour
{
    /// <summary>
    /// The text that shows the map's name on the button
    /// </summary>
    [SerializeField] private TextMeshProUGUI _mapNameText;
    
    /// <summary>
    /// Information about this map (name, enemies, scene to load, etc.)
    /// </summary>
    public MapData MapData { get; set; }
    
    /// <summary>
    /// The button component that makes this clickable
    /// </summary>
    private Button _MapButton;

    /// <summary>
    /// The image/background of this button (used to change colors)
    /// </summary>
    private Image _MapImage;

    /// <summary>
    /// What color this button should be (white if unlocked, gray if locked)
    /// </summary>
    public Color ReturnColor { get; set; }

    /// <summary>
    /// The background music to play when entering this map's combat
    /// </summary>
    [SerializeField] private SoundData CombatMusic;
    
    /// <summary>
    /// How long the music takes to fade in (in seconds, default is 2 seconds)
    /// </summary>
    [SerializeField] private float MusicFadeTime = 2f;


    /// <summary>
    /// Runs when this button is first created - finds the Button and Image components
    /// </summary>
    /// <remarks>
    /// <para><strong>Why:</strong> Catches missing components early to prevent crashes later</para>
    /// </remarks>
    private void Awake()
    {
        // Try to find the Button component
        _MapButton = GetComponent<Button>();
        if (_MapButton == null)
        {
            Debug.LogError($"[MapButton] Missing Button component on {gameObject.name}. Disabling script.");
            enabled = false;
            return;
        }
        
        // Try to find the Image component
        _MapImage = GetComponent<Image>();
        if (_MapImage == null)
        {
            Debug.LogError($"[MapButton] Missing Image component on {gameObject.name}. Disabling script.");
            enabled = false;
            return;
        }
        
        // Start with gray color (locked state)
        ReturnColor = Color.gray;
    }


    /// <summary>
    /// Sets up this button with map information and lock/unlock state
    /// </summary>
    /// <param name="map">The map data (name, enemies, scene, etc.)</param>
    /// <param name="isUnlocked">Can the player click this map? true = yes, false = locked</param>
    /// <remarks>
    /// <para><strong>When:</strong> Called by MapSelectManager when creating all the map buttons</para>
    /// </remarks>
    public void Setup(MapData map, bool isUnlocked)
    {
        // Store the map information
        MapData = map;
        
        // Display the map name on the button
        _mapNameText.SetText(map.MapId);
        
        // Make button clickable or not based on unlock state
        _MapButton.interactable = isUnlocked;

        if (isUnlocked)
        {
            // Map is unlocked - make it white and clickable
            _MapButton.onClick.AddListener(LoadMap);
            ReturnColor = Color.white;
            _MapImage.color = ReturnColor;
        }
        else
        {
            // Map is locked - make it gray and not clickable
            ReturnColor = Color.gray;
            _MapImage.color = ReturnColor;
        }
    }


    /// <summary>
    /// Unlocks this map button, making it clickable with white color
    /// </summary>
    /// <remarks>
    /// <para><strong>When:</strong> Called when player beats a level and unlocks the next one</para>
    /// </remarks>
    public void Unlock()
    {
        // Make the button clickable
        _MapButton.interactable = true;
        
        // Add the click action (load the map when clicked)
        _MapButton.onClick.AddListener(LoadMap);
        
        // Change color to white (unlocked look)
        ReturnColor = Color.white;
        _MapImage.color = ReturnColor;
    }
    
    /// <summary>
    /// Loads this map's combat scene with transition effects and music
    /// </summary>
    /// <remarks>
    /// <para><strong>When:</strong> Called when player clicks this button</para>
    /// <para><strong>What happens:</strong> Closes map select screen → Shows loading overlay → Loads combat → Fades in music</para>
    /// </remarks>
    public void LoadMap()
    {
        // Use SceneController to smoothly transition to combat
        SceneController.Instance
            .NewTransition()                                                                      // Start a new scene transition
            .Unload(SceneDatabase.Slots.SessionContent)                                          // Close the current map select screen
            .Load(SceneDatabase.Slots.SessionContent, SceneDatabase.Scenes.Combat, setActive: true) // Open the combat scene
            .WithMusic(CombatMusic, MusicFadeTime)                                               // Start playing combat music (fades in over 2 seconds)
            .WithOverlay()                                                                        // Show a loading screen overlay
            .Perform();                                                                           // Actually do all the above actions
    }
}