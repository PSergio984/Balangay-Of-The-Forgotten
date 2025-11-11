using TMPro;
using UnityEngine.UI;
using UnityEngine;
using AudioSystem;


/* 
 * Purpose: UI button for selecting maps in the map selection screen
 * 
 * How it works:
 * - Displays map name and lock/unlock state visually
 * - Triggers scene transition to combat when clicked
 * - Integrates with SceneController for smooth loading
 * 
 * Integration: Used by map selection manager to create clickable map choices
 */
/// <summary>
/// UI button representing a selectable map. Handles display, unlock state, and scene loading.
/// </summary>
/// <remarks>
/// <para><strong>Why:</strong> Provides visual, clickable map selection that loads the correct combat scene</para>
/// <para><strong>How:</strong> Manages button state (locked/unlocked), color feedback, and calls SceneController for transitions</para>
/// <para><strong>What:</strong> Like level select buttons in Mario - each represents one playable map</para>
/// </remarks>
[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class MapButton : MonoBehaviour
{
    /// <summary>
    /// Text component displaying the map name (set from MapData.MapId)
    /// </summary>
    [SerializeField] private TextMeshProUGUI _mapNameText;
    
    /// <summary>
    /// Data container for this map (ID, scene reference, metadata)
    /// </summary>
    public MapData MapData { get; set; }
    
    /// <summary>
    /// Cached Button component - controls interactivity and click handling
    /// </summary>
    private Button _MapButton;

    /// <summary>
    /// Cached Image component - provides visual feedback via color changes
    /// </summary>
    private Image _MapImage;

    /// <summary>
    /// Current button color (white = unlocked, gray = locked)
    /// </summary>
    public Color ReturnColor { get; set; }

    /// <summary>
    /// Music track to play when loading this map's combat scene
    /// </summary>
    [SerializeField] private SoundData CombatMusic;
    
    /// <summary>
    /// Duration in seconds for music fade-in transition (default: 2s)
    /// </summary>
    [SerializeField] private float MusicFadeTime = 2f;


    /// <summary>
    /// Caches required components and validates dependencies. Disables script if components missing.
    /// </summary>
    /// <remarks>
    /// <para><strong>Why:</strong> Fail gracefully to prevent null reference errors during gameplay</para>
    /// <para><strong>How:</strong> GetComponent for Button and Image, disable script if either is null</para>
    /// </remarks>
    private void Awake()
    {
        _MapButton = GetComponent<Button>();
        if (_MapButton == null)
        {
            Debug.LogError($"[MapButton] Missing Button component on {gameObject.name}. Disabling script.");
            enabled = false;
            return;
        }
        _MapImage = GetComponent<Image>();
        if (_MapImage == null)
        {
            Debug.LogError($"[MapButton] Missing Image component on {gameObject.name}. Disabling script.");
            enabled = false;
            return;
        }
        ReturnColor = Color.gray;
    }


    /// <summary>
    /// Configures button with map data and unlock state
    /// </summary>
    /// <param name="map">Map data containing ID and scene info</param>
    /// <param name="isUnlocked">True if player can select this map</param>
    /// <remarks>
    /// <para><strong>Why:</strong> Initialize button with correct map and visual state</para>
    /// <para><strong>How:</strong> Sets text, adds click listener if unlocked, updates color (white=unlocked, gray=locked)</para>
    /// <para><strong>When:</strong> Called once during map selection screen initialization</para>
    /// </remarks>
    public void Setup(MapData map, bool isUnlocked)
    {
        MapData = map;
        _mapNameText.SetText(map.MapId);
        _MapButton.interactable = isUnlocked;

        if (isUnlocked)
        {
            _MapButton.onClick.AddListener(LoadMap);
            ReturnColor = Color.white;
            _MapImage.color = ReturnColor;
        }
        else
        {
            ReturnColor = Color.gray;
            _MapImage.color = ReturnColor;
        }
    }


    /// <summary>
    /// Unlocks the button, making it clickable and updating visuals
    /// </summary>
    /// <remarks>
    /// <para><strong>Why:</strong> Allow player progression - make new maps accessible after completing previous ones</para>
    /// <para><strong>How:</strong> Enable button interaction, add click listener, change color to white</para>
    /// <para><strong>When:</strong> Called when player unlocks a new map (e.g., after beating previous map)</para>
    /// </remarks>
    public void Unlock()
    {
        _MapButton.interactable = true;
        _MapButton.onClick.AddListener(LoadMap);
        ReturnColor = Color.white;
        _MapImage.color = ReturnColor;
    }
    
    /// <summary>
    /// Loads the combat scene for this map with music and transition overlay
    /// </summary>
    public void LoadMap()
    {
        SceneController.Instance
            .NewTransition()
            .Unload(SceneDatabase.Slots.SessionContent)
            .Load(SceneDatabase.Slots.SessionContent, SceneDatabase.Scenes.Combat, setActive: true)
            .WithMusic(CombatMusic, MusicFadeTime)
            .WithOverlay()
            .Perform();
    }
}
