using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class MapSelectManager : MonoBehaviour
{
    public Transform MapParent;
    public GameObject MapButtonPrefab;
    public TextMeshProUGUI AreaHeaderText;
    public TextMeshProUGUI LevelHeaderText;
    public AreaData CurrentArea;

    public HashSet<string> UnlockedLevelIDs = new HashSet<string>();
    private Camera _camera;

    private List<GameObject> _buttonObjects = new List<GameObject>();
    private Dictionary<GameObject, Vector3> _buttonLocations = new Dictionary<GameObject, Vector3>();

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Start()
    {
        if (CurrentArea == null || AreaHeaderText == null || MapParent == null || MapButtonPrefab == null)
        {
            Debug.LogError("MapSelectManager: Required fields are not assigned in the Inspector");
            return;
        }

        AssignAreaText();
        LoadUnlockedLevels();
        CreateMapButtons();
    }
    private void AssignAreaText()
    {
        AreaHeaderText.SetText(CurrentArea.AreaName);
    }
    private void LoadUnlockedLevels()
    {
        if (CurrentArea.Maps == null) Debug.LogError("[MapSelectManager] CurrentArea.Maps is null!");

        foreach (var map in CurrentArea.Maps)
        {
            if (map.IsUnlockedByDefault)
            {
                UnlockedLevelIDs.Add(map.MapId);
            }
        }
    }
    private void CreateMapButtons()
    {
        for (int i = 0; i < CurrentArea.Maps.Count; i++)
        {
            GameObject buttonGO = Instantiate(MapButtonPrefab, MapParent);
            _buttonObjects.Add(buttonGO);

            RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();

            buttonGO.name = CurrentArea.Maps[i].MapId;
            CurrentArea.Maps[i].MapButtonObj = buttonGO;

            MapButton levelButton = buttonGO.GetComponent<MapButton>();
            levelButton.Setup(CurrentArea.Maps[i], UnlockedLevelIDs.Contains(CurrentArea.Maps[i].MapId));
        }
        

    }



}