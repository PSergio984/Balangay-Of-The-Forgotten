using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using TMPro;


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
            if (i > 0)
            {
                LineRenderer line = Instantiate(linePrefab, MapParent);

                line.transform.SetSiblingIndex(0);
                LineRendererConnector lineConnector = line.GetComponent<LineRendererConnector>();

                lineConnector.StartRectTrans = CurrentArea.Maps[i - 1].MapButtonObj.GetComponent<RectTransform>();
                lineConnector.EndRectTrans = mapData.MapButtonObj.GetComponent<RectTransform>();
                StartCoroutine(DelayedLineSetup(lineConnector));
            }
        }

        MapParent.gameObject.SetActive(true);
        _eventSystemHandler.InitSelectables();
        _eventSystemHandler.SetFirstSelected();
    }
    
    private IEnumerator DelayedLineSetup(LineRendererConnector lineConnector)
    {
        yield return null;
        lineConnector.UpdateLinePosition();
    }
    
}
