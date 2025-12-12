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
    /// Can be assigned in Inspector for pre-set buttons, or set via Setup() method
    /// </summary>
    [Header("Map Configuration")]
    [Tooltip("Assign the MapData ScriptableObject for this map button. Required for pre-set buttons.")]
    [SerializeField] private MapData _mapData;
    
    [Header("Completion Indicator")]
    [Tooltip("Optional checkmark or completion overlay to show when map is completed")]
    [SerializeField] private GameObject completionIndicator;
    
    [Tooltip("Optional color overlay to apply when map is completed")]
    [SerializeField] private Color completedColor = new Color(0.7f, 1f, 0.7f, 1f); // Light green tint
    
    /// <summary>
    /// Tracks whether this map has been completed
    /// </summary>
    public bool IsCompleted { get; private set; }
    
    /// <summary>
    /// Flag to prevent re-enabling completed maps
    /// </summary>
    private bool _isPermanentlyDisabled = false;
    
    /// <summary>
    /// Public property to access MapData (reads from serialized field or runtime-set value)
    /// </summary>
    public MapData MapData 
    { 
        get => _mapData; 
        set => _mapData = value; 
    }
    
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
    /// Tracks if LoadMap listener is already registered to prevent duplicate registrations
    /// </summary>
    private bool _isLoadMapListenerRegistered;

    /// <summary>
    /// The background music to play when entering this map's combat
    /// </summary>
    [SerializeField] private SoundData CombatMusic;
    
    /// <summary>
    /// How long the music takes to fade in (in seconds, default is 2 seconds)
    /// </summary>
    [SerializeField] private float MusicFadeTime = 2f;

    /// <summary>
    /// Reference to the level transition data ScriptableObject for passing data between scenes
    /// </summary>
    /// <remarks>
    /// <para><strong>Why:</strong> Stores selected MapData before scene load so combat can read it</para>
    /// <para><strong>How:</strong> Assign the same LevelTransitionData asset used by MatchSetupSystem</para>
    /// </remarks>
    [Header("Scene Transition")]
    [Tooltip("Assign the LevelTransitionData asset - this passes map data to the combat scene")]
    [SerializeField] public LevelTransitionData levelTransitionData;


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
        
        // Validate serialized text field
        if (_mapNameText == null)
        {
            Debug.LogError($"[MapButton] _mapNameText not assigned on {gameObject.name}. Disabling script.");
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
        Setup(map, isUnlocked, false);
    }
    
    /// <summary>
    /// Sets up this button with map information, lock/unlock state, and completion state
    /// </summary>
    /// <param name="map">The map data (name, enemies, scene, etc.)</param>
    /// <param name="isUnlocked">Can the player click this map? true = yes, false = locked</param>
    /// <param name="isCompleted">Has the player already beaten this map?</param>
    /// <remarks>
    /// <para><strong>When:</strong> Called by MapSelectManager when creating all the map buttons</para>
    /// </remarks>
    public void Setup(MapData map, bool isUnlocked, bool isCompleted)
    {
        if (map == null)
        {
            Debug.LogError($"[MapButton] Setup called with null MapData on {gameObject.name}. Button will not be configured.", this);
            return;
        }
        // Store the map information
        MapData = map;
        IsCompleted = isCompleted;
        
        // Display the map name on the button
        _mapNameText.SetText(map.MapId);
        
        // Make button clickable or not based on unlock state AND completion state
        // If map is completed, it should be disabled (not clickable)
        bool shouldBeInteractable = isUnlocked && !isCompleted;
        
        // CRITICAL: Always disable if completed, regardless of unlock state
        if (isCompleted)
        {
            shouldBeInteractable = false;
            _isPermanentlyDisabled = true; // Mark as permanently disabled
        }
        else
        {
            _isPermanentlyDisabled = false; // Reset flag if not completed
        }
        
        _MapButton.interactable = shouldBeInteractable;

        if (isUnlocked)
        {
            if (isCompleted)
            {
                // Map is completed - DISABLE button and show completed visual
                _MapButton.interactable = false; // Force disable completed maps
                
                // Also disable the Selectable component to prevent keyboard/gamepad navigation
                var selectable = GetComponent<UnityEngine.UI.Selectable>();
                if (selectable != null)
                {
                    selectable.interactable = false;
                }
                
                ReturnColor = completedColor;
                _MapImage.color = ReturnColor;
                
                // Show completion indicator if assigned
                if (completionIndicator != null)
                {
                    completionIndicator.SetActive(true);
                }
                
                Debug.Log($"[MapButton] Map '{map.MapId}' is completed - button disabled and made non-interactable.");
                
                // Double-check: Ensure button stays disabled
                ValidateButtonState();
            }
            else
            {
                // Map is unlocked but not completed - enable button and register listener
                _MapButton.interactable = true;
                RegisterLoadMapListener();
                
                ReturnColor = Color.white;
                _MapImage.color = ReturnColor;
                
                // Hide completion indicator
                if (completionIndicator != null)
                {
                    completionIndicator.SetActive(false);
                }
            }
        }
        else
        {
            // Map is locked - make it gray and not clickable
            ReturnColor = Color.gray;
            _MapImage.color = ReturnColor;
            
            // Hide completion indicator
            if (completionIndicator != null)
            {
                completionIndicator.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Validates and enforces button state - ensures completed maps stay disabled
    /// </summary>
    private void ValidateButtonState()
    {
        if (IsCompleted && _MapButton != null)
        {
            // Force disable if completed
            if (_MapButton.interactable)
            {
                Debug.LogWarning($"[MapButton] Completed map '{MapData?.MapId ?? "unknown"}' was re-enabled! Forcing disable.");
                _MapButton.interactable = false;
            }
            
            // Also disable Selectable component
            var selectable = GetComponent<UnityEngine.UI.Selectable>();
            if (selectable != null && selectable.interactable)
            {
                selectable.interactable = false;
            }
        }
    }
    
    /// <summary>
    /// Called every frame to ensure completed maps stay disabled
    /// </summary>
    private void Update()
    {
        // Only validate if completed (to avoid unnecessary checks)
        if (IsCompleted)
        {
            ValidateButtonState();
        }
    }


    /// <summary>
    /// Unlocks this map button, making it clickable with white color
    /// </summary>
    /// <remarks>
    /// <para><strong>When:</strong> Called when player beats a level and unlocks the next one</para>
    /// <para><strong>Note:</strong> This method respects completion state - won't enable if map is already completed</para>
    /// </remarks>
    public void Unlock()
    {
        // CRITICAL: Don't unlock if map is already completed (completed maps should stay disabled)
        if (IsCompleted || _isPermanentlyDisabled)
        {
            Debug.Log($"[MapButton] Cannot unlock map '{MapData?.MapId ?? "unknown"}' - it's already completed and should remain disabled.");
            // Force disable to be safe
            _MapButton.interactable = false;
            return;
        }
        
        // Make the button clickable
        _MapButton.interactable = true;
        
        // Add the click action (load the map when clicked) - guarded to prevent duplicates
        RegisterLoadMapListener();
        
        // Change color to white (unlocked look)
        ReturnColor = Color.white;
        _MapImage.color = ReturnColor;
    }
    
    
    /// <summary>
    /// Registers the LoadMap listener only once to prevent duplicate click handlers.
    /// <para><strong>Memory Safety:</strong> Always unsubscribe in OnDisable to prevent memory leaks (Unity best practice).</para>
    /// </summary>
    private void RegisterLoadMapListener()
    {
        if (_isLoadMapListenerRegistered)
            return;

        _MapButton.onClick.AddListener(LoadMap);
        _isLoadMapListenerRegistered = true;
    }

    /// <summary>
    /// Unsubscribes the LoadMap listener from the button to prevent memory leaks.
    /// Called automatically by Unity when the object is disabled or destroyed.
    /// </summary>
    private void OnDisable()
    {
        // Unity best practice: Always remove listeners to avoid memory leaks or duplicate calls.
        if (_isLoadMapListenerRegistered && _MapButton != null)
        {
            _MapButton.onClick.RemoveListener(LoadMap);
            _isLoadMapListenerRegistered = false;
        }
    }
    
    /// <summary>
    /// Loads this map's combat scene with transition effects and music
    /// </summary>
    /// <remarks>
    /// <para><strong>When:</strong> Called when player clicks this button</para>
    /// <para><strong>What happens:</strong> Stores MapData → Closes map select → Shows loading overlay → Loads combat → Fades in music</para>
    /// </remarks>
    public void LoadMap()
    {
        // Store the selected map data for the combat scene to read
        // --- Music Selection for Scene Transition ---
        if (levelTransitionData == null)
        {
            Debug.LogError("[MapButton] LevelTransitionData not assigned. Cannot load map.", this);
            return;
        }
        if (MapData == null)
        {
            Debug.LogError("[MapButton] MapData not assigned. Cannot load map.", this);
            return;
        }

        // Store selected map in transition data
        levelTransitionData.SelectedMapData = MapData;

        // Select music: first enemy with CombatMusic, else MapMusic
        SoundData selectedMusic = null;
        if (MapData.EnemyDatas != null)
        {
            foreach (var enemy in MapData.EnemyDatas)
            {
                if (enemy != null && enemy.CombatMusic != null)
                {
                    selectedMusic = enemy.CombatMusic;
                    break;
                }
            }
        }
        if (selectedMusic == null && MapData.MapMusic != null)
        {
            selectedMusic = MapData.MapMusic;
        }


        // Use SceneController to smoothly transition to combat, using selectedMusic
        var transition = SceneController.Instance
            .NewTransition()                                                                      // Start a new scene transition
            .Unload(SceneDatabase.Slots.SessionContent)                                          // Close the current map select screen
            .Load(SceneDatabase.Slots.SessionContent, SceneDatabase.Scenes.Combat, setActive: true); // Open the combat scene

        // Only add music if selectedMusic is not null
        if (selectedMusic != null)
        {
            transition = transition.WithMusic(selectedMusic, MusicFadeTime); // Start playing selected combat music
        }
        
        // Only add loading video if provided - skip if null or empty
        string loadingVideoId = MapData.LoadingVideoId;
        if (!string.IsNullOrEmpty(loadingVideoId))
        {
            transition = transition.WithLoadingVideo(loadingVideoId);  // Show map-specific loading video
        }

        transition = transition.WithPauseMusic(9);
        
        transition.Perform();  // Actually do all the above actions
    }
}