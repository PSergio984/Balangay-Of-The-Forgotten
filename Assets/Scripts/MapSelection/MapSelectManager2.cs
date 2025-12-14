using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


/* MAP SELECT MANAGER 2 DOCUMENTATION
 * 
 * Purpose: Simplified map selection manager - works with pre-set buttons (no dynamic creation)
 * 
 * How it works:
 * - Uses pre-set MapButton instances assigned in the Inspector
 * - Manages unlock state via HashSet for O(1) lookup performance
 * - Configures explicit navigation graph between unlocked buttons
 * - No LineRenderers, no player marker, no dynamic instantiation
 * 
 * Integration: Coordinates MapButton, AreaData, LevelSelectSystemEventHandler2
 */


/// <summary>
/// Simplified map selection manager - works with pre-set buttons, no animations or dynamic creation
/// </summary>
/// <remarks>
/// <para><strong>Why:</strong> Simplified UX without player marker, line renderers, or dynamic button creation</para>
/// <para><strong>How:</strong> Uses pre-set buttons from Inspector, configures unlock states and navigation</para>
/// </remarks>
public class MapSelectManager2 : MonoBehaviour
{
    [Header("Pre-Set Buttons")]
    /// <summary>
    /// List of MapButton instances that are pre-set in the Inspector (not created dynamically)
    /// </summary>
    [Tooltip("Assign MapButton GameObjects here - they should already exist in the scene")]
    [SerializeField] private List<MapButton> _preSetMapButtons = new List<MapButton>();

    [Header("UI Text")]
    /// <summary>
    /// Displays current area name (e.g., "Forest Region")
    /// </summary>
    [SerializeField] private TextMeshProUGUI _areaHeaderText;

    /// <summary>
    /// Displays currently selected level name (updated by LevelSelectSystemEventHandler2)
    /// </summary>
    public TextMeshProUGUI LevelHeaderText;

    /// <summary>
    /// Image component showing the portrait of the currently selected boss in the level select UI
    /// Updated by LevelSelectSystemEventHandler2 when a map is selected
    /// </summary>
    public Image CurrentBossImage;

    /// <summary>
    /// Text component displaying the name or label of the currently selected boss in the level select UI
    /// Updated by LevelSelectSystemEventHandler2 when a map is selected
    /// </summary>
    public TextMeshProUGUI CurrentBossImageText;

    [Header("Data")]
    /// <summary>
    /// ScriptableObject defining all maps in current area (used for unlock state lookup)
    /// </summary>
    [Tooltip("Optional: Assign AreaData to automatically load unlock states. If null, all buttons start unlocked.")]
    [SerializeField] private AreaData _currentArea;
    
    /// <summary>
    /// ScriptableObject for tracking game progress (map completion, Kaluwalhatian unlock)
    /// </summary>
    [Tooltip("Assign GameProgressData to track map completion and unlock Kaluwalhatian after 3 maps")]
    [SerializeField] private GameProgressData _gameProgress;

    /// <summary>
    /// Reference to event system handler for selection management
    /// </summary>
    private LevelSelectSystemEventHandler2 _eventSystemHandler;

    /// <summary>
    /// HashSet of unlocked map IDs for O(1) lookup (e.g., "Forest_01")
    /// </summary>
    public HashSet<string> UnlockedLevelIDs = new HashSet<string>();

    /// <summary>
    /// List of all MapButton GameObjects for iteration and reference
    /// </summary>
    private List<GameObject> _buttonObjects = new List<GameObject>();


    /// <summary>
    /// Caches event handler reference on initialization
    /// </summary>
    private void Awake()
    {
        _eventSystemHandler = GetComponentInChildren<LevelSelectSystemEventHandler2>(true);
        if (_eventSystemHandler == null)
        {
            Debug.LogError("[MapSelectManager2] LevelSelectSystemEventHandler2 component not found in children", this);
        }
    }


    /// <summary>
    /// Flag to track if Start() has been called (to avoid OnEnable running before initialization)
    /// </summary>
    private bool _hasStarted = false;
    
    /// <summary>
    /// Called when the GameObject is enabled - refreshes button states when returning from combat
    /// </summary>
    private void OnEnable()
    {
        Debug.Log($"[MapSelectManager2] OnEnable called - _hasStarted: {_hasStarted}, _shouldTriggerPostCombatDialogue: {_shouldTriggerPostCombatDialogue}");
        
        // Only refresh if Start() has already been called (buttons are initialized)
        // This prevents OnEnable from running before Start() on first load
        if (_hasStarted && _preSetMapButtons != null && _preSetMapButtons.Count > 0)
        {
            // Refresh button states when scene becomes active (e.g., returning from combat)
            // This ensures completed maps are properly disabled
            RefreshMapButtonStates();
            
            // Trigger appropriate dialogue based on game progress after returning from combat
            // Use a small delay to ensure scene is fully loaded
            if (_shouldTriggerPostCombatDialogue)
            {
                Debug.Log("[MapSelectManager2] Starting coroutine to trigger post-combat dialogue");
                StartCoroutine(TriggerPostCombatDialogueDelayed());
            }
            else
            {
                Debug.Log("[MapSelectManager2] Not triggering post-combat dialogue - flag not set");
            }
        }
        else
        {
            Debug.Log($"[MapSelectManager2] OnEnable: Skipping (hasStarted: {_hasStarted}, buttons: {(_preSetMapButtons != null && _preSetMapButtons.Count > 0)})");
        }
    }
    
    /// <summary>
    /// Coroutine to trigger post-combat dialogue after a short delay
    /// </summary>
    private System.Collections.IEnumerator TriggerPostCombatDialogueDelayed()
    {
        Debug.Log("[MapSelectManager2] TriggerPostCombatDialogueDelayed: Waiting one frame...");
        // Wait a frame to ensure scene is fully loaded
        yield return null;
        Debug.Log("[MapSelectManager2] TriggerPostCombatDialogueDelayed: Frame complete, calling TriggerPostCombatDialogue");
        TriggerPostCombatDialogue();
    }
    
    /// <summary>
    /// Extended delay version for Start() - waits longer to ensure all systems are initialized
    /// </summary>
    private System.Collections.IEnumerator TriggerPostCombatDialogueDelayedExtended()
    {
        Debug.Log("[MapSelectManager2] TriggerPostCombatDialogueDelayedExtended: Waiting for initialization...");
        // Wait a few frames to ensure everything is initialized
        yield return null;
        yield return null;
        yield return null;
        Debug.Log($"[MapSelectManager2] TriggerPostCombatDialogueDelayedExtended: After wait, flag: {_shouldTriggerPostCombatDialogue}");
        TriggerPostCombatDialogue();
    }

    /// <summary>
    /// Validates dependencies and initializes map selection screen
    /// </summary>
    private void Start()
    {
        _hasStarted = true; // Mark that Start() has been called
        
        // Check for post-combat dialogue FIRST (before intro dialogue)
        // This handles the case where OnEnable ran before Start() and couldn't trigger
        // IMPORTANT: This handles the case where scene loads fresh and OnEnable hasn't run yet
        if (_shouldTriggerPostCombatDialogue)
        {
            Debug.Log("[MapSelectManager2] Start() detected post-combat dialogue flag. Triggering after initialization...");
            // Use a longer delay to ensure everything is initialized
            StartCoroutine(TriggerPostCombatDialogueDelayedExtended());
        }
        // Trigger intro dialogue if not seen yet (after lore transition)
        // Only trigger intro if we're NOT returning from combat (to avoid dialogue conflicts)
        else if (!_shouldTriggerPostCombatDialogue)
        {
            TriggerIntroDialogueIfNeeded();
        }
        
        // Validate required inspector references
        if (_preSetMapButtons == null || _preSetMapButtons.Count == 0)
        {
            Debug.LogWarning("[MapSelectManager2] No pre-set MapButtons assigned. Map selection will not work.", this);
            return;
        }

        // Validate Canvas is active and visible
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            if (!canvas.gameObject.activeInHierarchy)
            {
                Debug.LogError($"[MapSelectManager2] Canvas '{canvas.gameObject.name}' is not active! UI will not be visible in Game view.", this);
            }
            
            // Check camera assignment for ScreenSpaceCamera mode
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                if (canvas.worldCamera == null)
                {
                    Debug.LogError($"[MapSelectManager2] Canvas '{canvas.gameObject.name}' is set to ScreenSpaceCamera but has no camera assigned! UI will not be visible in Game view.", this);
                }
                else if (!canvas.worldCamera.gameObject.activeInHierarchy)
                {
                    Debug.LogError($"[MapSelectManager2] Canvas camera '{canvas.worldCamera.gameObject.name}' is not active! UI will not be visible in Game view.", this);
                }
                else
                {
                    Debug.Log($"[MapSelectManager2] Canvas camera '{canvas.worldCamera.gameObject.name}' is active and assigned.", this);
                }
            }
            
            Debug.Log($"[MapSelectManager2] Canvas found: '{canvas.gameObject.name}', Active: {canvas.gameObject.activeInHierarchy}, RenderMode: {canvas.renderMode}", this);
        }
        else
        {
            Debug.LogWarning("[MapSelectManager2] No Canvas found in parent hierarchy! UI may not render correctly.", this);
        }

        // Validate EventSystem exists and has input module
        if (EventSystem.current == null)
        {
            Debug.LogError("[MapSelectManager2] No EventSystem found in scene! Keyboard navigation will not work.", this);
        }
        else
        {
            Debug.Log($"[MapSelectManager2] EventSystem found: '{EventSystem.current.gameObject.name}'", this);
            
            // Check for input modules (required for keyboard navigation)
            var standaloneModule = EventSystem.current.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            
            // Check for InputSystemUIInputModule using reflection (in case Input System package is installed)
            UnityEngine.Component inputSystemModule = null;
            var inputSystemModuleType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule");
            if (inputSystemModuleType != null)
            {
                inputSystemModule = EventSystem.current.GetComponent(inputSystemModuleType);
            }
            
            // Prefer StandaloneInputModule for reliable keyboard navigation
            // If InputSystemUIInputModule exists, we'll disable it and use StandaloneInputModule instead
            if (standaloneModule == null)
            {
                // Add StandaloneInputModule if it doesn't exist
                Debug.Log($"[MapSelectManager2] Adding StandaloneInputModule to '{EventSystem.current.gameObject.name}' for keyboard navigation...", this);
                standaloneModule = EventSystem.current.gameObject.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
            
            // Enable StandaloneInputModule
            if (!standaloneModule.enabled)
            {
                Debug.Log($"[MapSelectManager2] Enabling StandaloneInputModule on '{EventSystem.current.gameObject.name}'...", this);
                standaloneModule.enabled = true;
            }
            
            // Disable InputSystemUIInputModule if both exist (to avoid conflicts)
            if (inputSystemModule != null)
            {
                var enabledProperty = inputSystemModuleType.GetProperty("enabled");
                if (enabledProperty != null)
                {
                    bool isEnabled = (bool)enabledProperty.GetValue(inputSystemModule);
                    if (isEnabled)
                    {
                        Debug.Log($"[MapSelectManager2] Disabling InputSystemUIInputModule to use StandaloneInputModule for keyboard navigation...", this);
                        enabledProperty.SetValue(inputSystemModule, false);
                    }
                }
            }
            
            Debug.Log($"[MapSelectManager2] StandaloneInputModule is active. Keyboard navigation should work.", this);
        }

        // Set area header text if available
        if (_currentArea != null && _areaHeaderText != null)
        {
            _areaHeaderText.SetText(_currentArea.AreaName);
        }

        // Load unlock states from AreaData if available
        LoadUnlockedLevels();

        // Setup pre-set buttons
        SetupMapButtons();
    }


    /// <summary>
    /// Populates UnlockedLevelIDs from CurrentArea.Maps where IsUnlockedByDefault is true
    /// Also checks GameProgressData for Kaluwalhatian unlock status
    /// If no AreaData is assigned, all buttons start unlocked
    /// </summary>
    private void LoadUnlockedLevels()
    {
        // Load progress data if available
        if (_gameProgress != null)
        {
            _gameProgress.Load();
            Debug.Log($"[MapSelectManager2] Loaded game progress: {_gameProgress.CompletedMapCount} maps completed, Kaluwalhatian unlocked: {_gameProgress.IsKaluwalhatianUnlocked}");
        }
        
        if (_currentArea == null)
        {
            Debug.LogWarning("[MapSelectManager2] No AreaData assigned. All buttons will start unlocked.", this);
            // If no AreaData, unlock all buttons by default
            foreach (var button in _preSetMapButtons)
            {
                if (button != null && button.MapData != null)
                {
                    UnlockedLevelIDs.Add(button.MapData.MapId);
                }
            }
            return;
        }

        if (_currentArea.Maps == null)
        {
            Debug.LogError("[MapSelectManager2] CurrentArea.Maps is null!", this);
            return;
        }

        foreach (var map in _currentArea.Maps)
        {
            // Check if map is unlocked by default
            if (map.IsUnlockedByDefault)
            {
                UnlockedLevelIDs.Add(map.MapId);
            }
            // Special case: Kaluwalhatian is unlocked via progress, not by default
            else if (map.MapId == GameProgressData.MAP_ID_KALUWALHATIAN && 
                     _gameProgress != null && 
                     _gameProgress.IsKaluwalhatianUnlocked)
            {
                UnlockedLevelIDs.Add(map.MapId);
                Debug.Log("[MapSelectManager2] Kaluwalhatian unlocked via game progress!");
            }
        }
    }
    
    /// <summary>
    /// Checks if a specific map has been completed using GameProgressData
    /// </summary>
    /// <param name="mapId">The map ID to check</param>
    /// <returns>True if the map has been completed</returns>
    private bool IsMapCompleted(string mapId)
    {
        if (_gameProgress == null || string.IsNullOrEmpty(mapId))
        {
            return false;
        }
        return _gameProgress.IsMapComplete(mapId);
    }


    /// <summary>
    /// Refreshes map button states based on current game progress (called when returning from combat)
    /// </summary>
    private void RefreshMapButtonStates()
    {
        if (_gameProgress == null)
        {
            Debug.LogWarning("[MapSelectManager2] GameProgressData not assigned, cannot refresh button states.");
            return;
        }
        
        // Reload progress to ensure we have latest data
        _gameProgress.Load();
        
        foreach (var mapButton in _preSetMapButtons)
        {
            if (mapButton == null || mapButton.MapData == null)
                continue;
            
            MapData mapData = mapButton.MapData;
            bool isUnlocked = UnlockedLevelIDs.Contains(mapData.MapId);
            bool isCompleted = IsMapCompleted(mapData.MapId);
            
            // Re-setup button with updated completion state
            mapButton.Setup(mapData, isUnlocked, isCompleted);
            
            Debug.Log($"[MapSelectManager2] Refreshed button '{mapData.MapId}': Unlocked={isUnlocked}, Completed={isCompleted}, Interactable={!isCompleted && isUnlocked}");
        }
    }
    
    /// <summary>
    /// Configures pre-set MapButton instances with unlock states and registers them with event handler
    /// </summary>
    /// <remarks>
    /// <para><strong>Process:</strong> For each pre-set button → Configure MapButton → Add to event handler → Setup navigation</para>
    /// </remarks>
    private void SetupMapButtons()
    {
        foreach (var mapButton in _preSetMapButtons)
        {
            if (mapButton == null)
            {
                Debug.LogWarning("[MapSelectManager2] Found null MapButton in _preSetMapButtons list. Skipping.", this);
                continue;
            }

            // Get MapData from button
            MapData mapData = mapButton.MapData;
            if (mapData == null)
            {
                Debug.LogWarning($"[MapSelectManager2] MapButton '{mapButton.gameObject.name}' has null MapData. Skipping setup.", this);
                continue;
            }

            // Note: Button sprite is set manually in Inspector for UI design flexibility
            // MapData is only used for game logic (unlock state, scene loading, boss info, etc.)

            // Determine if this map is unlocked
            bool isUnlocked = UnlockedLevelIDs.Contains(mapData.MapId);
            
            // Determine if this map has been completed
            bool isCompleted = IsMapCompleted(mapData.MapId);
            
            Debug.Log($"[MapSelectManager2] Setting up button '{mapData.MapId}': Unlocked={isUnlocked}, Completed={isCompleted}");
            
            // Setup the MapButton with unlock and completion state
            mapButton.Setup(mapData, isUnlocked, isCompleted);

            // Cache button GameObject for navigation
            _buttonObjects.Add(mapButton.gameObject);

            // Register button with event system handler
            Selectable sel = mapButton.GetComponent<Selectable>();
            if (sel != null)
            {
                if (_eventSystemHandler != null)
                {
                    _eventSystemHandler.AddSelectable(sel);
                    Debug.Log($"[MapSelectManager2] Registered button '{mapButton.gameObject.name}' with event handler", this);
                }
                else
                {
                    Debug.LogError($"[MapSelectManager2] EventSystemHandler is null! Cannot register button '{mapButton.gameObject.name}'.", this);
                }
            }
            else
            {
                Debug.LogWarning($"[MapSelectManager2] MapButton '{mapButton.gameObject.name}' is missing a Selectable component. Skipping AddSelectable.", this);
            }
        }

        // Setup button navigation after all buttons are configured
        StartCoroutine(SetupButtonNavigation());

        // Initialize event system handler and select first button
        // Use coroutine to ensure EventSystem is ready
        StartCoroutine(InitializeEventSystem());
    }


    /// <summary>
    /// Initializes event system handler and selects first button after a frame delay
    /// </summary>
    /// <remarks>
    /// <para><strong>Why:</strong> Ensures EventSystem is fully initialized before selecting first button</para>
    /// </remarks>
    private IEnumerator InitializeEventSystem()
    {
        // Wait for EventSystem to be ready
        yield return null;
        
        if (_eventSystemHandler != null)
        {
            _eventSystemHandler.InitSelectables();
            
            // Wait another frame to ensure selectables are registered
            yield return null;
            
            _eventSystemHandler.SetFirstSelected();
            
            Debug.Log("[MapSelectManager2] Event system initialized and first button selected");
        }
        else
        {
            Debug.LogError("[MapSelectManager2] EventSystemHandler is null! Cannot initialize navigation.", this);
        }
    }

    #region Navigation

    /// <summary>
    /// Configures explicit navigation graph between unlocked buttons based on spatial positions
    /// </summary>
    /// <remarks>
    /// <para><strong>How:</strong> Calculates direction vectors between buttons and links based on position (supports up/down/left/right)</para>
    /// <para><strong>Why:</strong> Unity's automatic navigation may not work well with custom layouts</para>
    /// </remarks>
    private IEnumerator SetupButtonNavigation()
    {
        yield return null; // Wait for button positions to be finalized

        // Build list of unlocked buttons with their positions
        List<(GameObject button, Selectable selectable, MapButton mapButton, Vector2 position)> unlockedButtons = new List<(GameObject, Selectable, MapButton, Vector2)>();
        
        foreach (GameObject buttonObj in _buttonObjects)
        {
            MapButton mapButton = buttonObj.GetComponent<MapButton>();
            if (mapButton == null || mapButton.MapData == null)
                continue;
            
            if (!UnlockedLevelIDs.Contains(mapButton.MapData.MapId))
                continue;
            
            Selectable selectable = buttonObj.GetComponent<Selectable>();
            if (selectable == null)
                continue;
            
            RectTransform rectTransform = buttonObj.GetComponent<RectTransform>();
            if (rectTransform == null)
                continue;
            
            unlockedButtons.Add((buttonObj, selectable, mapButton, rectTransform.position));
        }

        // Configure navigation for each unlocked button
        for (int i = 0; i < unlockedButtons.Count; i++)
        {
            var current = unlockedButtons[i];
            Navigation nav = new Navigation { mode = Navigation.Mode.Explicit };
            
            Vector2 currentPos = current.position;
            float closestLeftDist = float.MaxValue;
            float closestRightDist = float.MaxValue;
            float closestUpDist = float.MaxValue;
            float closestDownDist = float.MaxValue;
            Selectable closestLeft = null;
            Selectable closestRight = null;
            Selectable closestUp = null;
            Selectable closestDown = null;

            // Find closest buttons in each direction
            for (int j = 0; j < unlockedButtons.Count; j++)
            {
                if (i == j) continue; // Skip self
                
                var other = unlockedButtons[j];
                Vector2 otherPos = other.position;
                Vector2 direction = otherPos - currentPos;
                
                // Check horizontal direction (left/right)
                if (Mathf.Abs(direction.y) < Mathf.Abs(direction.x) * 0.5f) // More horizontal than vertical
                {
                    if (direction.x < 0) // Left
                    {
                        float dist = direction.magnitude;
                        if (dist < closestLeftDist)
                        {
                            closestLeftDist = dist;
                            closestLeft = other.selectable;
                        }
                    }
                    else if (direction.x > 0) // Right
                    {
                        float dist = direction.magnitude;
                        if (dist < closestRightDist)
                        {
                            closestRightDist = dist;
                            closestRight = other.selectable;
                        }
                    }
                }
                
                // Check vertical direction (up/down)
                if (Mathf.Abs(direction.x) < Mathf.Abs(direction.y) * 0.5f) // More vertical than horizontal
                {
                    if (direction.y > 0) // Up
                    {
                        float dist = direction.magnitude;
                        if (dist < closestUpDist)
                        {
                            closestUpDist = dist;
                            closestUp = other.selectable;
                        }
                    }
                    else if (direction.y < 0) // Down
                    {
                        float dist = direction.magnitude;
                        if (dist < closestDownDist)
                        {
                            closestDownDist = dist;
                            closestDown = other.selectable;
                        }
                    }
                }
            }

            // Assign navigation
            nav.selectOnLeft = closestLeft;
            nav.selectOnRight = closestRight;
            nav.selectOnUp = closestUp;
            nav.selectOnDown = closestDown;
            
            current.selectable.navigation = nav;
        }
        
        Debug.Log($"[MapSelectManager2] Navigation configured for {unlockedButtons.Count} unlocked buttons (supports up/down/left/right)", this);
    }

    #endregion


    #region Helper Methods

    /// <summary>
    /// Unlocks level, updates MapButton visual state, optionally refreshes navigation graph
    /// </summary>
    /// <param name="levelID">Map identifier to add to UnlockedLevelIDs</param>
    /// <param name="mapButton">MapButton instance to unlock</param>
    /// <param name="updateNavigation">If true, recalculates navigation graph (default: true)</param>
    public void UnlockLevel(string levelID, MapButton mapButton, bool updateNavigation = true)
    {
        UnlockedLevelIDs.Add(levelID);
        mapButton.Unlock();
        
        if (updateNavigation)
        {
            StartCoroutine(SetupButtonNavigation());
        }
    }

    /// <summary>
    /// [Debug Method] Unlocks all map buttons via context menu
    /// </summary>
    [ContextMenu("Unlock All Levels Example")]
    public void UnlockAllLevelsExample()
    {
        if (_buttonObjects == null)
        {
            Debug.LogWarning("[MapSelectManager2] _buttonObjects is null in UnlockAllLevelsExample.", this);
            return;
        }
        
        foreach (var buttonObj in _buttonObjects)
        {
            if (buttonObj == null)
                continue;
            
            MapButton mapButton = buttonObj.GetComponent<MapButton>();
            if (mapButton == null || mapButton.MapData == null)
                continue;
            
            string levelToUnlock = mapButton.MapData.MapId;
            UnlockLevel(levelToUnlock, mapButton, false); // Batch unlock without navigation update
        }
        
        StartCoroutine(SetupButtonNavigation()); // Single navigation update after all unlocks
    }
    
    /// <summary>
    /// Triggers intro dialogue if it hasn't been seen yet (after lore transition)
    /// </summary>
    private void TriggerIntroDialogueIfNeeded()
    {
        // Check if GameProgressData is assigned
        if (_gameProgress == null)
        {
            Debug.LogWarning("[MapSelectManager2] GameProgressData not assigned, cannot check intro dialogue status.");
            return;
        }
        
        // Check if intro dialogue has already been seen
        if (_gameProgress.HasSeenIntroDialogue)
        {
            Debug.Log("[MapSelectManager2] Intro dialogue already seen, skipping.");
            return;
        }
        
        // Find DialogueTrigger with IntroScene type in the scene
        DialogueTrigger[] dialogueTriggers = FindObjectsOfType<DialogueTrigger>();
        
        foreach (var dialogueTrigger in dialogueTriggers)
        {
            if (dialogueTrigger != null && dialogueTrigger.dialogue != null)
            {
                // Check if dialogue is marked as IntroScene type
                if (dialogueTrigger.dialogue.dialogueType == DialogueType.IntroScene)
                {
                    Debug.Log("[MapSelectManager2] Triggering intro dialogue after lore transition");
                    dialogueTrigger.TriggerDialogue();
                    
                    // Mark intro dialogue as seen (will be saved when dialogue completes)
                    // Note: DialogueManager should call MarkIntroDialogueSeen when dialogue completes
                    return; // Only trigger the first one found
                }
            }
        }
        
        Debug.Log("[MapSelectManager2] No DialogueTrigger with IntroScene dialogue found in scene. Skipping intro dialogue.");
    }
    
    /// <summary>
    /// Flag to track if we should trigger post-combat dialogue (set when returning from combat)
    /// </summary>
    private static bool _shouldTriggerPostCombatDialogue = false;
    
    /// <summary>
    /// Static method to mark that post-combat dialogue should be triggered
    /// Called from VictoryDefeatUI when transitioning back to MapSelection
    /// </summary>
    public static void MarkShouldTriggerPostCombatDialogue()
    {
        _shouldTriggerPostCombatDialogue = true;
        Debug.Log("[MapSelectManager2] Marked to trigger post-combat dialogue on next MapSelection load");
    }
    
    /// <summary>
    /// Triggers post-combat dialogue (PostVictory or PostFinalBoss) when returning from combat
    /// </summary>
    private void TriggerPostCombatDialogue()
    {
        Debug.Log($"[MapSelectManager2] TriggerPostCombatDialogue called - flag: {_shouldTriggerPostCombatDialogue}");
        
        // Only trigger if we marked that we should (i.e., we just returned from combat)
        if (!_shouldTriggerPostCombatDialogue)
        {
            Debug.LogWarning("[MapSelectManager2] Not triggering post-combat dialogue - not marked as returning from combat");
            return;
        }
        
        Debug.Log("[MapSelectManager2] Flag is set! Proceeding with dialogue trigger...");
        
        // Clear the flag so it doesn't trigger again
        _shouldTriggerPostCombatDialogue = false;
        
        if (_gameProgress == null)
        {
            Debug.LogWarning("[MapSelectManager2] GameProgressData not assigned, cannot determine dialogue type.");
            return;
        }
        
        // Reload progress to get latest state
        _gameProgress.Load();
        
        // Determine which dialogue to trigger
        bool shouldTriggerPostFinalBoss = _gameProgress.IsGameCompleted;
        bool shouldTriggerPostVictory = !shouldTriggerPostFinalBoss && _gameProgress.CompletedMapCount > 0;
        
        Debug.Log($"[MapSelectManager2] Determining dialogue type - PostFinalBoss: {shouldTriggerPostFinalBoss}, PostVictory: {shouldTriggerPostVictory}, Completed maps: {_gameProgress.CompletedMapCount}, IsGameCompleted: {_gameProgress.IsGameCompleted}");
        
        // Find DialogueTriggers in the scene
        DialogueTrigger[] dialogueTriggers = FindObjectsOfType<DialogueTrigger>(true); // Include inactive objects
        Debug.Log($"[MapSelectManager2] Found {dialogueTriggers.Length} DialogueTrigger(s) in scene (including inactive)");
        
        if (shouldTriggerPostFinalBoss)
        {
            // Final map completed - trigger PostFinalBoss dialogue
            bool foundPostFinalBoss = false;
            foreach (var dialogueTrigger in dialogueTriggers)
            {
                if (dialogueTrigger != null)
                {
                    Debug.Log($"[MapSelectManager2] Checking DialogueTrigger '{dialogueTrigger.gameObject.name}' - Active: {dialogueTrigger.gameObject.activeInHierarchy}, Has Dialogue: {dialogueTrigger.dialogue != null}");
                    
                    if (dialogueTrigger.dialogue != null)
                    {
                        Debug.Log($"[MapSelectManager2] DialogueTrigger '{dialogueTrigger.gameObject.name}' - Type: {dialogueTrigger.dialogue.dialogueType} (expected: {DialogueType.PostFinalBoss})");
                        
                        // Check both the enum value and the integer value to be safe
                        if (dialogueTrigger.dialogue.dialogueType == DialogueType.PostFinalBoss || 
                            (int)dialogueTrigger.dialogue.dialogueType == 3) // PostFinalBoss = 3
                        {
                            Debug.Log($"[MapSelectManager2] Found PostFinalBoss dialogue! Triggering on '{dialogueTrigger.gameObject.name}'");
                            
                            // Ensure the GameObject is active
                            if (!dialogueTrigger.gameObject.activeInHierarchy)
                            {
                                Debug.LogWarning($"[MapSelectManager2] DialogueTrigger GameObject '{dialogueTrigger.gameObject.name}' is inactive. Activating it.");
                                dialogueTrigger.gameObject.SetActive(true);
                            }
                            
                            dialogueTrigger.TriggerDialogue();
                            foundPostFinalBoss = true;
                            return;
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[MapSelectManager2] DialogueTrigger '{dialogueTrigger.gameObject.name}' has null dialogue field!");
                    }
                }
            }
            
            if (!foundPostFinalBoss)
            {
                Debug.LogError("[MapSelectManager2] No DialogueTrigger with PostFinalBoss dialogue found! Check that a DialogueTrigger in the MapSelection scene has dialogueType set to PostFinalBoss (value 3).");
            }
        }
        else if (shouldTriggerPostVictory)
        {
            // Regular map completed - trigger PostVictory dialogue
            bool foundPostVictory = false;
            foreach (var dialogueTrigger in dialogueTriggers)
            {
                if (dialogueTrigger != null && dialogueTrigger.dialogue != null)
                {
                    Debug.Log($"[MapSelectManager2] Checking DialogueTrigger '{dialogueTrigger.gameObject.name}' - Type: {dialogueTrigger.dialogue.dialogueType}");
                    
                    // Check both the enum value and the integer value to be safe
                    if (dialogueTrigger.dialogue.dialogueType == DialogueType.PostVictory || 
                        (int)dialogueTrigger.dialogue.dialogueType == 2) // PostVictory = 2
                    {
                        Debug.Log($"[MapSelectManager2] Found PostVictory dialogue! Triggering on '{dialogueTrigger.gameObject.name}'");
                        
                        // Ensure the GameObject is active
                        if (!dialogueTrigger.gameObject.activeInHierarchy)
                        {
                            Debug.LogWarning($"[MapSelectManager2] DialogueTrigger GameObject '{dialogueTrigger.gameObject.name}' is inactive. Activating it.");
                            dialogueTrigger.gameObject.SetActive(true);
                        }
                        
                        dialogueTrigger.TriggerDialogue();
                        foundPostVictory = true;
                        return;
                    }
                }
            }
            
            if (!foundPostVictory)
            {
                Debug.LogWarning("[MapSelectManager2] No DialogueTrigger with PostVictory dialogue found. Skipping dialogue.");
            }
        }
    }

    #endregion
}

