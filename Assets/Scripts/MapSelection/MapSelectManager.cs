using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;


/* MAP SELECT MANAGER DOCUMENTATION
 * 
 * Purpose: Manages the map selection UI screen - creates buttons, handles unlocks, displays area info
 * 
 * How it works:
 * - Loads area data and creates map buttons for each map
 * - Tracks which maps are unlocked via HashSet
 * - Configures each button with correct unlock state and visual setup
 * 
 * Integration: Central manager for map selection screen, works with AreaData and MapButton components
 */


/// <summary>
/// Manages map selection screen - creates map buttons, handles unlock states, displays area information
/// </summary>
/// <remarks>
/// <para><strong>Why:</strong> Centralizes map selection UI creation and unlock state management</para>
/// <para><strong>How:</strong> Instantiates MapButton prefabs for each map in CurrentArea, configures unlock states from AreaData</para>
/// </remarks>
public class MapSelectManager : MonoBehaviour
{
    /// <summary>
    /// Parent transform where map buttons will be spawned
    /// </summary>
    public Transform MapParent;

    /// <summary>
    /// Prefab template for creating map button instances
    /// </summary>
    public GameObject MapButtonPrefab;

    public LineRenderer linePrefab;

    /// <summary>
    /// UI text displaying the current area name (e.g., "Forest Region")
    /// </summary>
    public TextMeshProUGUI AreaHeaderText;

    /// <summary>
    /// UI text displaying level information (currently unused in Start())
    /// </summary>
    public TextMeshProUGUI LevelHeaderText;

    /// <summary>
    /// ScriptableObject containing all maps and data for the current area
    /// </summary>
    public AreaData CurrentArea;

    [Header("Player References")]
    public GameObject PlayerUIPrefab;
    public RectTransform WorldSpaceCanvasRect;
    public Vector2 PlayerPositionOffsetPerLevel = new Vector2(9.92f, -9.5f);

    private LevelSelectSystemEventHandler _eventSystemHandler;

    /// <summary>
    /// Set of unlocked map IDs for quick lookup (e.g., "Forest_01", "Desert_Boss")
    /// </summary>
    public HashSet<string> UnlockedLevelIDs = new HashSet<string>();

    /// <summary>
    /// Cached reference to main camera (currently unused)
    /// </summary>
    private Camera _camera;

    /// <summary>
    /// List of instantiated map button GameObjects for tracking and cleanup
    /// </summary>
    private List<GameObject> _buttonObjects = new List<GameObject>();

    /// <summary>
    /// Maps button GameObjects to their world positions (currently unused)
    /// </summary>
    private Dictionary<GameObject, Vector3> _buttonLocations = new Dictionary<GameObject, Vector3>();

    public GameObject PlayerObj { get; set; }
    private bool _playerIsFacingRight;
    /// <summary>
    /// Caches main camera reference on initialization
    /// </summary>
    private void Awake()
    {
        _camera = Camera.main;
        _eventSystemHandler = GetComponentInChildren<LevelSelectSystemEventHandler>(true);

        if (_eventSystemHandler == null)
        {
            Debug.LogError("MapSelectManager: LevelSelectEventSystemHandler component not found in children");
        }
    }

    /// <summary>
    /// Initializes map selection screen - validates dependencies, loads area text, processes unlocks, creates buttons
    /// </summary>
    /// <remarks>
    /// <para><strong>How:</strong> Validates required fields → Sets area header → Loads unlocked maps → Creates button instances</para>
    /// </remarks>
    private void Start()
    {

        // Validate all required inspector references before proceeding
        if (CurrentArea == null || AreaHeaderText == null || MapParent == null || MapButtonPrefab == null)
        {
            Debug.LogError("MapSelectManager: Required fields are not assigned in the Inspector");
            return;
        }

        // Additional validation for player prefab and canvas rect
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

        AssignAreaText();
        LoadUnlockedLevels();
        CreateMapButtons();
    }

    /// <summary>
    /// Sets the area header text from CurrentArea data
    /// </summary>
    private void AssignAreaText()
    {
        AreaHeaderText.SetText(CurrentArea.AreaName);
    }

    /// <summary>
    /// Populates UnlockedLevelIDs with all maps that are unlocked by default
    /// </summary>
    /// <remarks>
    /// <para><strong>How:</strong> Iterates through CurrentArea.Maps and adds MapIds where IsUnlockedByDefault is true</para>
    /// </remarks>
    private void LoadUnlockedLevels()
    {
        // Safety check for null Maps array
        if (CurrentArea.Maps == null) Debug.LogError("[MapSelectManager] CurrentArea.Maps is null!");

        // Add all default-unlocked maps to the unlock set
        foreach (var map in CurrentArea.Maps)
        {
            if (map.IsUnlockedByDefault)
            {
                UnlockedLevelIDs.Add(map.MapId);
            }
        }
    }

    /// <summary>
    /// Instantiates and configures a MapButton for each map in CurrentArea
    /// </summary>
    /// <remarks>
    /// <para><strong>How:</strong> Loops through CurrentArea.Maps, instantiates prefab, configures MapButton component with unlock state</para>
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

            // Instantiate button prefab as child of MapParent
            GameObject buttonGO = Instantiate(MapButtonPrefab, MapParent);
            _buttonObjects.Add(buttonGO);

            // Cache RectTransform for potential positioning (currently unused)
            RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();

            // Set button name to MapId for easier debugging in hierarchy
            buttonGO.name = mapData.MapId;

            // Configure MapButton component with map data and unlock state
            MapButton mapButton = buttonGO.GetComponent<MapButton>();
            if (mapButton == null)
            {
                Debug.LogWarning($"[MapSelectManager] MapButton component missing on instantiated button '{buttonGO.name}' (Prefab: {MapButtonPrefab?.name ?? "null"}). Skipping Setup and MapButtonObj assignment.");
                continue;
            }

            // Only set MapButtonObj if both buttonGO and levelButton are valid
            mapData.MapButtonObj = buttonGO;
            mapButton.Setup(mapData, UnlockedLevelIDs.Contains(mapData.MapId));

            //populate the selectables for the event system
            Selectable sel = mapButton.GetComponent<Selectable>();
            if (sel != null)
            {
                _eventSystemHandler.AddSelectable(sel);
            }
            else
            {
                Debug.LogWarning($"[MapSelectManager] MapButton '{buttonGO.name}' is missing a Selectable component. Skipping AddSelectable.");
            }

            StartCoroutine(AddLocationAfterDelay(buttonGO, buttonRect));
            if (i > 0)
            {
                LineRenderer line = Instantiate(linePrefab, MapParent);

                line.transform.SetSiblingIndex(0);
                LineRendererConnector lineConnector = line.GetComponent<LineRendererConnector>();

                lineConnector.StartRectTrans = CurrentArea.Maps[i - 1].MapButtonObj.GetComponent<RectTransform>();
                lineConnector.EndRectTrans = mapData.MapButtonObj.GetComponent<RectTransform>();
                StartCoroutine(DelayedLineSetup(lineConnector));
            }
            else
            {
                StartCoroutine(SpawnPlayerAfterDelay(buttonRect, WorldSpaceCanvasRect));
            }
        }

        StartCoroutine(SetupButtonNavigation());

        MapParent.gameObject.SetActive(true);
        _eventSystemHandler.InitSelectables();
        _eventSystemHandler.SetFirstSelected();
    }

    private IEnumerator DelayedLineSetup(LineRendererConnector lineConnector)
    {
        yield return null;
        lineConnector.UpdateLinePosition();
    }

    private IEnumerator AddLocationAfterDelay(GameObject buttonGo, RectTransform buttonRect)
    {
        yield return null;

        Vector2 buttonScreenPoint = RectTransformUtility.WorldToScreenPoint(_camera, buttonRect.position);
        Vector3 buttonWorldPos = _camera.ScreenToWorldPoint(new Vector3(buttonScreenPoint.x, buttonScreenPoint.y, _camera.nearClipPlane));
        _buttonLocations.Add(buttonGo, buttonWorldPos);
    }

    #region Navigation

    private IEnumerator SetupButtonNavigation()
    {
        yield return null;

        for (int i = 0; i < _buttonObjects.Count; i++)
        {
            GameObject currentButton = _buttonObjects[i];
            Vector3 currentPos = _buttonLocations[currentButton];
            Selectable currentSelectable = currentButton.GetComponent<Selectable>();
            Navigation nav = new Navigation { mode = Navigation.Mode.Explicit };

            // Check if previous button exists and both current and previous maps are unlocked
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

            // Check if future button exists and both current and next maps are unlocked
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
    /// Sets the appropriate navigation property (right/left/up/down) on the Navigation struct based on the direction vector.
    /// </summary>
    /// <param name="nav">Navigation struct to modify (by ref).</param>
    /// <param name="direction">Normalized direction vector from current to target.</param>
    /// <param name="target">Selectable to assign to the correct direction.</param>
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
    /// Unlocks a level and optionally updates button navigation.
    /// </summary>
    /// <param name="levelID">The level ID to unlock.</param>
    /// <param name="mapButton">The MapButton to unlock.</param>
    /// <param name="updateNavigation">If true, updates navigation after unlocking. Default is true.</param>
    public void UnlockLevel(string levelID, MapButton mapButton, bool updateNavigation = true)
    {
        UnlockedLevelIDs.Add(levelID);
        mapButton.Unlock();
        if (updateNavigation)
        {
            StartCoroutine(SetupButtonNavigation());
        }
    }

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
        UnlockLevel(levelToUnlock, mapButton); // Single unlock, keep default updateNavigation = true
    }

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
            UnlockLevel(levelToUnlock, mapButton, false); // Batch unlock, don't update navigation yet
        }
        StartCoroutine(SetupButtonNavigation()); // Update navigation once after all unlocks
    }

    #endregion

    #region Player

    private IEnumerator SpawnPlayerAfterDelay(RectTransform screenSpaceButton, RectTransform worldSpaceCanvas)
    {
        yield return null;
        SpawnInPlayerRectTransform(screenSpaceButton, worldSpaceCanvas);
    }

    private void SpawnInPlayerRectTransform(RectTransform screenSpaceUIObject, RectTransform worldSpaceUIObject)
    {
        _playerIsFacingRight = true;
        PlayerObj = Instantiate(PlayerUIPrefab, worldSpaceUIObject);

        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(_camera, screenSpaceUIObject.position);
        Vector3 worldPosition = _camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, _camera.nearClipPlane));
        worldPosition.z = worldSpaceUIObject.position.z;

        Vector3 offsetPosition = worldPosition + (Vector3)PlayerPositionOffsetPerLevel;

        PlayerObj.transform.position = offsetPosition;

        if (_buttonObjects.Count > 1)
        {
            Vector2 secondScreenPoint = RectTransformUtility.WorldToScreenPoint(_camera, _buttonObjects[1].GetComponent<RectTransform>().position);
            Vector3 secondWorldPoint = _camera.ScreenToWorldPoint(new Vector3(secondScreenPoint.x, secondScreenPoint.y, _camera.nearClipPlane));
            secondWorldPoint.z = worldSpaceUIObject.position.z;

            CheckForRightOrLeftTurn(PlayerObj, ref _playerIsFacingRight, secondWorldPoint);
        }

    }

    private void CheckForRightOrLeftTurn(GameObject player, ref bool isFacingRight, Vector3 targetWorldPosition)
    {
        if (isFacingRight)
        {
            if (targetWorldPosition.x < player.transform.position.x)
            {
                player.transform.Rotate(0f, 180f, 0f);
                isFacingRight = false;
            }
        }
        else
        {
            if (targetWorldPosition.x > player.transform.position.x)
            {
                player.transform.Rotate(0f, -180f, 0f);
                isFacingRight = true;
            }
        }
    }

    public void MovePlayerToButton(GameObject playerUI, RectTransform targetButton, RectTransform worldSpaceUIObject)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(_camera, targetButton.position);
        Vector3 worldPosition = _camera.ScreenToWorldPoint(new Vector3(screenPoint.x, screenPoint.y, _camera.nearClipPlane));
        worldPosition.z = worldSpaceUIObject.position.z;

        Vector3 endPosition = worldPosition + (Vector3)PlayerPositionOffsetPerLevel;

        CheckForRightOrLeftTurn(playerUI, ref _playerIsFacingRight, worldPosition);

        playerUI.transform.DOMove(endPosition, 0.11f);
    }

    #endregion


}
