using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


/* MAP SELECT MANAGER DOCUMENTATION
 * 
 * Purpose: Central controller for map selection screen UI and progression
 * 
 * How it works:
 * - Instantiates MapButton instances from AreaData configuration
 * - Manages unlock state via HashSet for O(1) lookup performance
 * - Configures explicit navigation graph between unlocked buttons
 * - Spawns and animates player marker on world-space canvas
 * - Generates LineRenderer connectors between sequential map buttons
 * 
 * Integration: Coordinates MapButton, AreaData, LevelSelectSystemEventHandler, and LineRendererConnector
 */


/// <summary>
/// Manages map selection screen instantiation, unlock state, navigation setup, and player marker
/// </summary>
/// <remarks>
/// <para><strong>Why:</strong> Centralizes map selection UI lifecycle and state management</para>
/// <para><strong>How:</strong> Instantiates buttons from AreaData, configures explicit navigation graph, tracks unlocks via HashSet</para>
/// </remarks>
public class MapSelectManager : MonoBehaviour
{
    [Header("Button Setup")]
    /// <summary>
    /// Parent transform for button instantiation (typically LayoutGroup)
    /// </summary>
    public Transform MapParent;

    /// <summary>
    /// Prefab template for MapButton instances
    /// </summary>
    public GameObject MapButtonPrefab;

    /// <summary>
    /// Prefab for LineRenderer connectors between sequential buttons
    /// </summary>
    public LineRenderer linePrefab;

    [Header("UI Text")]
    /// <summary>
    /// Displays current area name (e.g., "Forest Region")
    /// </summary>
    public TextMeshProUGUI AreaHeaderText;

    /// <summary>
    /// Displays currently selected level name (updated by LevelSelectSystemEventHandler)
    /// </summary>
    public TextMeshProUGUI LevelHeaderText;

    [Header("Data")]
    /// <summary>
    /// ScriptableObject defining all maps in current area
    /// </summary>
    public AreaData CurrentArea;

    [Header("Player Marker")]
    /// <summary>
    /// Prefab for player UI marker on world-space canvas
    /// </summary>
    public GameObject PlayerUIPrefab;
    
    /// <summary>
    /// World-space canvas RectTransform for player marker parenting
    /// </summary>
    public RectTransform WorldSpaceCanvasRect;
    
    /// <summary>
    /// Positional offset applied to player marker relative to button position
    /// </summary>
    public Vector2 PlayerPositionOffsetPerLevel = new Vector2(9.92f, -9.5f);

    /// <summary>
    /// Reference to event system handler for selection management
    /// </summary>
    private LevelSelectSystemEventHandler _eventSystemHandler;

    /// <summary>
    /// HashSet of unlocked map IDs for O(1) lookup (e.g., "Forest_01")
    /// </summary>
    public HashSet<string> UnlockedLevelIDs = new HashSet<string>();

    /// <summary>
    /// Cached main camera reference for coordinate space conversions
    /// </summary>
    private Camera _camera;

    /// <summary>
    /// List of instantiated button GameObjects for iteration and reference
    /// </summary>
    private List<GameObject> _buttonObjects = new List<GameObject>();

    /// <summary>
    /// Maps button GameObjects to world-space positions for navigation calculation
    /// </summary>
    private Dictionary<GameObject, Vector3> _buttonLocations = new Dictionary<GameObject, Vector3>();

    /// <summary>
    /// Runtime reference to instantiated player marker GameObject
    /// </summary>
    public GameObject PlayerObj { get; set; }
    
    /// <summary>
    /// Tracks player marker facing direction for flip logic
    /// </summary>
    private bool _playerIsFacingRight;


    /// <summary>
    /// Validates camera reference and re-finds if destroyed during scene transitions
    /// </summary>
    /// <returns>True if camera is valid, false otherwise</returns>
    private bool ValidateCamera()
    {
        // Check if camera reference is null or destroyed
        if (_camera == null || !_camera)
        {
            Debug.LogWarning("[MapSelectManager] Camera reference lost, searching for camera in MapSelection scene...", this);
            
            // Strategy 1: Try to find main camera first
            _camera = Camera.main;
            if (_camera == null)
            {
                // Strategy 2: Find any camera in the active scene
                Camera[] allCameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
                foreach (Camera cam in allCameras)
                {
                    // Prefer cameras in this scene (MapSelection)
                    if (cam.gameObject.scene == gameObject.scene)
                    {
                        _camera = cam;
                        Debug.LogWarning($"[MapSelectManager] Found camera '{cam.name}' in MapSelection scene.", this);
                        break;
                    }
                }
                
                // Strategy 3: If still not found, use any camera
                if (_camera == null && allCameras.Length > 0)
                {
                    _camera = allCameras[0];
                    Debug.LogWarning($"[MapSelectManager] Using camera '{_camera.name}' from another scene as fallback.", this);
                }
                
                if (_camera == null)
                {
                    Debug.LogError("[MapSelectManager] No camera found in any scene! MapSelection camera may have been destroyed. Check your scene setup.", this);
                    return false;
                }
            }
        }
        return true;
    }


    /// <summary>
    /// Caches camera and event handler references on initialization
    /// </summary>
    private void Awake()
    {
        _camera = Camera.main;
        if (_camera == null)
        {
            // Try to find any camera in the scene
            _camera = FindFirstObjectByType<Camera>();
            if (_camera != null)
            {
                Debug.LogWarning("[MapSelectManager] No camera tagged as MainCamera found. Using first available camera: " + _camera.name, this);
            }
            else
            {
                Debug.LogError("[MapSelectManager] No camera found in the scene! UI/world position calculations will fail.", this);
                // Optionally, create a new camera at runtime:
                // GameObject camObj = new GameObject("AutoCreatedCamera");
                // _camera = camObj.AddComponent<Camera>();
                // camObj.tag = "MainCamera";
                // Debug.LogWarning("[MapSelectManager] Created a new camera at runtime.", this);
            }
        }

        _eventSystemHandler = GetComponentInChildren<LevelSelectSystemEventHandler>(true);
        if (_eventSystemHandler == null)
        {
            Debug.LogError("MapSelectManager: LevelSelectEventSystemHandler component not found in children");
        }
    }


    /// <summary>
    /// Validates dependencies and initializes map selection screen
    /// </summary>
    /// <remarks>
    /// <para><strong>Execution order:</strong> Validation → AssignAreaText → LoadUnlockedLevels → CreateMapButtons</para>
    /// </remarks>
    private void Start()
    {
        // Validate required inspector references
        if (CurrentArea == null || AreaHeaderText == null || MapParent == null || MapButtonPrefab == null)
        {
            Debug.LogError("MapSelectManager: Required fields are not assigned in the Inspector");
            return;
        }

        // Validate player marker dependencies
        if (PlayerUIPrefab == null)
        {
            Debug.LogError("MapSelectManager: PlayerUIPrefab is not assigned in the Inspector. Disabling component.", this);
            enabled = false;
            return;
        }
        if (WorldSpaceCanvasRect == null)
        {
            Debug.LogError("MapSelectManager: WorldSpaceCanvasRect is not assigned in the Inspector. Disabling component.", this);
            enabled = false;
            return;
        }

        // Validate camera before proceeding
        if (!ValidateCamera())
        {
            Debug.LogError("[MapSelectManager] Cannot initialize - no camera available. Make sure MapSelection scene has a camera.", this);
            enabled = false;
            return;
        }

        AssignAreaText();
        LoadUnlockedLevels();
        CreateMapButtons();
    }


    /// <summary>
    /// Sets area header text from CurrentArea.AreaName
    /// </summary>
    private void AssignAreaText()
    {
        AreaHeaderText.SetText(CurrentArea.AreaName);
    }


    /// <summary>
    /// Populates UnlockedLevelIDs from CurrentArea.Maps where IsUnlockedByDefault is true
    /// </summary>
    private void LoadUnlockedLevels()
    {
        if (CurrentArea.Maps == null) 
            Debug.LogError("[MapSelectManager] CurrentArea.Maps is null!");

        foreach (var map in CurrentArea.Maps)
        {
            if (map.IsUnlockedByDefault)
            {
                UnlockedLevelIDs.Add(map.MapId);
            }
        }
    }


    /// <summary>
    /// Instantiates MapButton prefabs, configures unlock states, generates LineRenderers, spawns player marker
    /// </summary>
    /// <remarks>
    /// <para><strong>Process:</strong> For each map → Instantiate button → Configure MapButton → Add to event handler → 
    /// Create LineRenderer (if not first) → Cache world position → Setup navigation → Spawn player (on first button)</para>
    /// </remarks>
    private void CreateMapButtons()
    {
        for (int i = 0; i < CurrentArea.Maps.Count; i++)
        {
            var mapData = CurrentArea.Maps[i];
            if (mapData == null)
            {
                Debug.LogWarning($"[MapSelectManager] MapData at index {i} is null. Skipping button creation.");
                continue;
            }

            // Instantiate button prefab under MapParent
            GameObject buttonGO = Instantiate(MapButtonPrefab, MapParent);
            _buttonObjects.Add(buttonGO);

            // Set the button's sprite to the map's thumbnail
            var image = buttonGO.GetComponent<UnityEngine.UI.Image>();
            if (image != null && mapData.MapThumbnail != null)
            {
                image.sprite = mapData.MapThumbnail;
            }

            RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();

            // Set GameObject name to MapId for hierarchy readability
            buttonGO.name = mapData.MapId;

            // Configure MapButton component
            MapButton mapButton = buttonGO.GetComponent<MapButton>();
            if (mapButton == null)
            {
                Debug.LogWarning($"[MapSelectManager] MapButton component missing on instantiated button '{buttonGO.name}' (Prefab: {MapButtonPrefab?.name ?? "null"}). Skipping Setup and MapButtonObj assignment.");
                continue;
            }

            mapData.MapButtonObj = buttonGO;
            mapButton.Setup(mapData, UnlockedLevelIDs.Contains(mapData.MapId));

            // Register button with event system handler
            Selectable sel = mapButton.GetComponent<Selectable>();
            if (sel != null)
            {
                _eventSystemHandler.AddSelectable(sel);
            }
            else
            {
                Debug.LogWarning($"[MapSelectManager] MapButton '{buttonGO.name}' is missing a Selectable component. Skipping AddSelectable.");
            }

            // Cache world position for navigation calculation
            StartCoroutine(AddLocationAfterDelay(buttonGO, buttonRect));
            
            // Create LineRenderer between sequential buttons
            if (i > 0)
            {
                LineRenderer line = Instantiate(linePrefab, MapParent);
                line.transform.SetSiblingIndex(0); // Move to back of hierarchy (render behind buttons)
                
                LineRendererConnector lineConnector = line.GetComponent<LineRendererConnector>();
                lineConnector.StartRectTrans = CurrentArea.Maps[i - 1].MapButtonObj.GetComponent<RectTransform>();
                lineConnector.EndRectTrans = mapData.MapButtonObj.GetComponent<RectTransform>();
                
                StartCoroutine(DelayedLineSetup(lineConnector));
            }
            else
            {
                // Spawn player marker at first button
                StartCoroutine(SpawnPlayerAfterDelay(buttonRect, WorldSpaceCanvasRect));
            }
        }

        StartCoroutine(SetupButtonNavigation());

        MapParent.gameObject.SetActive(true);
        _eventSystemHandler.InitSelectables();
        _eventSystemHandler.SetFirstSelected();
    }


    /// <summary>
    /// Updates LineRenderer positions after layout pass (requires RectTransform positions to be finalized)
    /// </summary>
    private IEnumerator DelayedLineSetup(LineRendererConnector lineConnector)
    {
        yield return null; // Wait for layout calculation
        lineConnector.UpdateLinePosition();
    }


    /// <summary>
    /// Caches button world position after RectTransform layout is finalized
    /// </summary>
    private IEnumerator AddLocationAfterDelay(GameObject buttonGo, RectTransform buttonRect)
    {
        yield return null; // Wait for layout calculation

        if (!ValidateCamera())
        {
            Debug.LogError("[MapSelectManager] Cannot cache button location - camera is invalid.", this);
            yield break;
        }

        Vector2 buttonScreenPoint = RectTransformUtility.WorldToScreenPoint(_camera, buttonRect.position);
        Vector3 buttonWorldPos = _camera.ScreenToWorldPoint(new Vector3(buttonScreenPoint.x, buttonScreenPoint.y, _camera.nearClipPlane));
        _buttonLocations.Add(buttonGo, buttonWorldPos);
    }


    #region Navigation

    /// <summary>
    /// Configures explicit navigation graph between unlocked buttons based on spatial positions
    /// </summary>
    /// <remarks>
    /// <para><strong>How:</strong> Calculates direction vectors between sequential buttons, assigns to Navigation.selectOnRight/Left/Up/Down</para>
    /// <para><strong>Why:</strong> Unity's automatic navigation fails with non-grid layouts, explicit navigation required</para>
    /// </remarks>
    private IEnumerator SetupButtonNavigation()
    {
        yield return null; // Wait for button positions to be cached

        for (int i = 0; i < _buttonObjects.Count; i++)
        {
            GameObject currentButton = _buttonObjects[i];
            Vector3 currentPos = _buttonLocations[currentButton];
            Selectable currentSelectable = currentButton.GetComponent<Selectable>();
            Navigation nav = new Navigation { mode = Navigation.Mode.Explicit };

            // Link to previous button if both current and previous are unlocked
            if (i > 0 && UnlockedLevelIDs.Contains(CurrentArea.Maps[i].MapId) && UnlockedLevelIDs.Contains(CurrentArea.Maps[i - 1].MapId))
            {
                GameObject prevButton = _buttonObjects[i - 1];
                Vector3 prevPos = _buttonLocations[prevButton];
                Vector3 dirToPrev = (prevPos - currentPos).normalized;
                Selectable prevSelectable = prevButton.GetComponent<Selectable>();
                
                if (prevSelectable != null)
                {
                    SetNavigationForDirection(ref nav, dirToPrev, prevSelectable);
                }
                else
                {
                    Debug.LogWarning($"[MapSelectManager] Previous button at index {i - 1} is missing a Selectable component. Navigation not set.");
                }
            }

            // Link to next button if both current and next are unlocked
            if (i < _buttonObjects.Count - 1 && UnlockedLevelIDs.Contains(CurrentArea.Maps[i].MapId) && UnlockedLevelIDs.Contains(CurrentArea.Maps[i + 1].MapId))
            {
                GameObject nextButton = _buttonObjects[i + 1];
                Vector3 nextPos = _buttonLocations[nextButton];
                Vector3 dirToNext = (nextPos - currentPos).normalized;
                Selectable nextSelectable = nextButton.GetComponent<Selectable>();
                
                if (nextSelectable != null)
                {
                    SetNavigationForDirection(ref nav, dirToNext, nextSelectable);
                }
                else
                {
                    Debug.LogWarning($"[MapSelectManager] Next button at index {i + 1} is missing a Selectable component. Navigation not set.");
                }
            }

            currentSelectable.navigation = nav;
        }
    }

    #endregion


    /// <summary>
    /// Maps direction vector to Navigation property via dot product threshold
    /// </summary>
    /// <param name="nav">Navigation struct (passed by reference for modification)</param>
    /// <param name="direction">Normalized direction vector from current to target button</param>
    /// <param name="target">Selectable to assign to appropriate navigation property</param>
    /// <remarks>
    /// <para><strong>Threshold:</strong> 0.7 dot product (approximately 45° cone)</para>
    /// </remarks>
    private void SetNavigationForDirection(ref Navigation nav, Vector3 direction, Selectable target)
    {
        if (Vector3.Dot(direction, Vector3.right) > 0.7f)
            nav.selectOnRight = target;
        else if (Vector3.Dot(direction, Vector3.left) > 0.7f)
            nav.selectOnLeft = target;
        else if (Vector3.Dot(direction, Vector3.up) > 0.7f)
            nav.selectOnUp = target;
        else if (Vector3.Dot(direction, Vector3.down) > 0.7f)
            nav.selectOnDown = target;
    } 


    #region Helper Methods

    /// <summary>
    /// Unlocks level, updates MapButton visual state, optionally refreshes navigation graph
    /// </summary>
    /// <param name="levelID">Map identifier to add to UnlockedLevelIDs</param>
    /// <param name="mapButton">MapButton instance to unlock</param>
    /// <param name="updateNavigation">If true, recalculates navigation graph (default: true). Set false for batch unlocks.</param>
    /// <remarks>
    /// <para><strong>Usage:</strong> Call with updateNavigation=false for batch unlocks, then manually call SetupButtonNavigation() once</para>
    /// </remarks>
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
    /// [Debug Method] Unlocks second map button via context menu
    /// </summary>
    [ContextMenu("Unlock Level Two Example")]
    public void UnlockLevelTwoExample()
    {
        if (_buttonObjects == null || _buttonObjects.Count <= 1)
        {
            Debug.LogWarning("[MapSelectManager] _buttonObjects is null or does not have enough elements for UnlockLevelTwoExample.");
            return;
        }
        
        GameObject buttonObj = _buttonObjects[1];
        if (buttonObj == null)
        {
            Debug.LogWarning("[MapSelectManager] _buttonObjects[1] is null in UnlockLevelTwoExample.");
            return;
        }
        
        MapButton mapButton = buttonObj.GetComponent<MapButton>();
        if (mapButton == null)
        {
            Debug.LogWarning("[MapSelectManager] MapButton component missing on _buttonObjects[1] in UnlockLevelTwoExample.");
            return;
        }
        
        string levelToUnlock = mapButton.MapData.MapId;
        UnlockLevel(levelToUnlock, mapButton);
    }


    /// <summary>
    /// [Debug Method] Unlocks all map buttons via context menu (optimized batch unlock)
    /// </summary>
    [ContextMenu("Unlock All Levels Example")]
    public void UnlockAllLevelsExample()
    {
        if (_buttonObjects == null)
        {
            Debug.LogWarning("[MapSelectManager] _buttonObjects is null in UnlockAllLevelsExample.");
            return;
        }
        
        for (int i = 0; i < _buttonObjects.Count; i++)
        {
            GameObject buttonObj = _buttonObjects[i];
            if (buttonObj == null)
            {
                Debug.LogWarning($"[MapSelectManager] _buttonObjects[{i}] is null in UnlockAllLevelsExample. Skipping.");
                continue;
            }
            
            MapButton mapButton = buttonObj.GetComponent<MapButton>();
            if (mapButton == null)
            {
                Debug.LogWarning($"[MapSelectManager] MapButton component missing on _buttonObjects[{i}] in UnlockAllLevelsExample. Skipping.");
                continue;
            }
            
            string levelToUnlock = mapButton.MapData.MapId;
            UnlockLevel(levelToUnlock, mapButton, false); // Batch unlock without navigation update
        }
        
        StartCoroutine(SetupButtonNavigation()); // Single navigation update after all unlocks
    }

    #endregion


    #region Player Marker

    /// <summary>
    /// Spawns player marker at first button position after layout pass
    /// </summary>
    private IEnumerator SpawnPlayerAfterDelay(RectTransform screenSpaceButton, RectTransform worldSpaceCanvas)
    {
        yield return null; // Wait for layout
        SpawnInPlayerRectTransform(screenSpaceButton, worldSpaceCanvas);
    }


    /// <summary>
    /// Instantiates player marker on world-space canvas with positional offset
    /// </summary>
    /// <param name="screenSpaceUIObject">Screen-space button RectTransform for position reference</param>
    /// <param name="worldSpaceUIObject">World-space canvas RectTransform for parenting</param>
    private void SpawnInPlayerRectTransform(RectTransform screenSpaceUIObject, RectTransform worldSpaceUIObject)
    {
        if (!ValidateCamera())
        {
            Debug.LogError("[MapSelectManager] Cannot spawn player marker - camera is invalid.", this);
            return;
        }

        _playerIsFacingRight = true;

        PlayerObj = Instantiate(PlayerUIPrefab, worldSpaceUIObject);
        // Ensure player marker is rendered on top
        PlayerObj.transform.SetAsLastSibling();

        // Convert screen-space UI position to world-space
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(_camera, screenSpaceUIObject.position);
        Vector3 worldPosition = _camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, _camera.nearClipPlane));
        worldPosition.z = worldSpaceUIObject.position.z;

        // Apply configured offset
        Vector3 offsetPosition = worldPosition + (Vector3)PlayerPositionOffsetPerLevel;
        PlayerObj.transform.position = offsetPosition;

        // Pre-orient player towards second button if it exists
        if (_buttonObjects.Count > 1)
        {
            Vector2 secondScreenPoint = RectTransformUtility.WorldToScreenPoint(_camera, _buttonObjects[1].GetComponent<RectTransform>().position);
            Vector3 secondWorldPoint = _camera.ScreenToWorldPoint(new Vector3(secondScreenPoint.x, secondScreenPoint.y, _camera.nearClipPlane));
            secondWorldPoint.z = worldSpaceUIObject.position.z;

            CheckForRightOrLeftTurn(PlayerObj, ref _playerIsFacingRight, secondWorldPoint);
        }
    }


    /// <summary>
    /// Flips player marker sprite based on target position relative to current position
    /// </summary>
    /// <param name="player">Player marker GameObject to flip</param>
    /// <param name="isFacingRight">Current facing direction (modified by reference)</param>
    /// <param name="targetWorldPosition">Target position to face towards</param>
    private void CheckForRightOrLeftTurn(GameObject player, ref bool isFacingRight, Vector3 targetWorldPosition)
    {
        if (isFacingRight)
        {
            // Target is to the left, flip to face left
            if (targetWorldPosition.x < player.transform.position.x)
            {
                player.transform.Rotate(0f, 180f, 0f);
                isFacingRight = false;
            }
        }
        else
        {
            // Target is to the right, flip to face right
            if (targetWorldPosition.x > player.transform.position.x)
            {
                player.transform.Rotate(0f, -180f, 0f);
                isFacingRight = true;
            }
        }
    }


    /// <summary>
    /// Animates player marker to target button position with DOTween
    /// </summary>
    /// <param name="playerUI">Player marker GameObject to move</param>
    /// <param name="targetButton">Target button RectTransform</param>
    /// <param name="worldSpaceUIObject">World-space canvas RectTransform for Z-axis reference</param>
    /// <remarks>
    /// <para><strong>Called by:</strong> LevelSelectSystemEventHandler.OnSelect when button selection changes</para>
    /// </remarks>
    public void MovePlayerToButton(GameObject playerUI, RectTransform targetButton, RectTransform worldSpaceUIObject)
    {
        if (!ValidateCamera())
        {
            Debug.LogError("[MapSelectManager] Cannot move player marker - camera is invalid.", this);
            return;
        }

        // Convert target button position to world-space
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(_camera, targetButton.position);
        Vector3 worldPosition = _camera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, _camera.nearClipPlane));
        worldPosition.z = worldSpaceUIObject.position.z;

        Vector3 endPosition = worldPosition + (Vector3)PlayerPositionOffsetPerLevel;

        // Orient player towards target before moving
        CheckForRightOrLeftTurn(playerUI, ref _playerIsFacingRight, worldPosition);

        // Animate movement (0.11s duration)
        playerUI.transform.DOMove(endPosition, 0.11f);
    }

    #endregion
}