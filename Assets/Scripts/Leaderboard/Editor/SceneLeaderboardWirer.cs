#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor utility to wire Leaderboard UI panels and buttons into MainMenu, MapSelection, and Combat scenes.
/// </summary>
public static class SceneLeaderboardWirer
{
    /// <summary>
    /// Wires Leaderboard components into all scenes (MainMenu, MapSelection, and Combat).
    /// </summary>
    /// <returns>True if all scenes were wired successfully; otherwise false.</returns>
    [MenuItem("Tools/Leaderboard/Wire All Scenes")]
    public static bool WireAllScenes()
    {
        bool mainSuccess = WireMainMenuScene();
        bool mapSuccess = WireMapSelectionScene();
        bool combatSuccess = WireCombatScene();

        if (mainSuccess && mapSuccess && combatSuccess)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SceneLeaderboardWirer] All Leaderboard scenes wired successfully.");
            return true;
        }
        else
        {
            Debug.LogError("[SceneLeaderboardWirer] Leaderboard scene wiring incomplete or failed for one or more scenes.");
            return false;
        }
    }

    /// <summary>
    /// Wires the LeaderboardManager singleton and LeaderboardPanel UI into the MainMenu scene.
    /// </summary>
    /// <returns>True if MainMenu scene wiring succeeded; otherwise false.</returns>
    public static bool WireMainMenuScene()
    {
        string scenePath = "Assets/Scenes/MainMenu.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // 1. Find Canvas (validate before scene mutation)
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[SceneLeaderboardWirer] Canvas not found in MainMenu scene!");
            return false;
        }

        // 2. Ensure LeaderboardManager exists
        if (Object.FindFirstObjectByType<LeaderboardManager>() == null)
        {
            GameObject mgrObj = new GameObject("LeaderboardManager");
            mgrObj.AddComponent<LeaderboardManager>();
            Debug.Log("[SceneLeaderboardWirer] Created LeaderboardManager in MainMenu scene.");
        }

        // 3. Instantiate LeaderboardPanel if not present
        LeaderboardUI leaderboardUI = canvas.GetComponentInChildren<LeaderboardUI>(true);
        if (leaderboardUI == null)
        {
            GameObject panelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Leaderboard/LeaderboardPanel.prefab");
            if (panelPrefab != null)
            {
                GameObject panelInstance = (GameObject)PrefabUtility.InstantiatePrefab(panelPrefab, canvas.transform);
                leaderboardUI = panelInstance.GetComponent<LeaderboardUI>();
                Debug.Log("[SceneLeaderboardWirer] Instantiated LeaderboardPanel prefab in MainMenu Canvas.");
            }
            else
            {
                Debug.LogError("[SceneLeaderboardWirer] LeaderboardPanel prefab not found!");
                return false;
            }
        }

        // 4. Ensure Leaderboard Button exists in MainMenu UI
        Button lbButton = null;
        Transform existingBtn = canvas.transform.Find("LeaderboardButton");
        if (existingBtn != null)
        {
            lbButton = existingBtn.GetComponent<Button>();
        }

        if (lbButton == null)
        {
            GameObject btnObj = new GameObject("LeaderboardButton", typeof(RectTransform));
            btnObj.transform.SetParent(canvas.transform, false);
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.85f, 0.05f);
            btnRect.anchorMax = new Vector2(0.98f, 0.12f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            Image btnBg = btnObj.AddComponent<Image>();
            btnBg.color = new Color(0.2f, 0.4f, 0.7f, 1f);
            lbButton = btnObj.AddComponent<Button>();

            GameObject textObj = new GameObject("Text", typeof(RectTransform));
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "LEADERBOARD";
            tmp.fontSize = 14;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            Debug.Log("[SceneLeaderboardWirer] Created LeaderboardButton in MainMenu scene.");
        }

        // 5. Wire onClick listener to LeaderboardUI.Open
        if (lbButton != null && leaderboardUI != null)
        {
            UnityEventTools.RemovePersistentListener(lbButton.onClick, leaderboardUI.Open);
            UnityEventTools.AddPersistentListener(lbButton.onClick, leaderboardUI.Open);
            EditorUtility.SetDirty(lbButton);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[SceneLeaderboardWirer] MainMenu scene saved successfully.");
        return true;
    }

    /// <summary>
    /// Wires the LeaderboardPanel UI into the MapSelection scene.
    /// </summary>
    /// <returns>True if MapSelection scene wiring succeeded; otherwise false.</returns>
    public static bool WireMapSelectionScene()
    {
        string scenePath = "Assets/Scenes/MapSelection.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[SceneLeaderboardWirer] Canvas not found in MapSelection scene!");
            return false;
        }

        // 1.5 Ensure LeaderboardManager exists
        if (Object.FindFirstObjectByType<LeaderboardManager>() == null)
        {
            GameObject mgrObj = new GameObject("LeaderboardManager");
            mgrObj.AddComponent<LeaderboardManager>();
            Debug.Log("[SceneLeaderboardWirer] Created LeaderboardManager in MapSelection scene.");
        }

        LeaderboardUI leaderboardUI = canvas.GetComponentInChildren<LeaderboardUI>(true);
        if (leaderboardUI == null)
        {
            GameObject panelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Leaderboard/LeaderboardPanel.prefab");
            if (panelPrefab != null)
            {
                GameObject panelInstance = (GameObject)PrefabUtility.InstantiatePrefab(panelPrefab, canvas.transform);
                leaderboardUI = panelInstance.GetComponent<LeaderboardUI>();
                Debug.Log("[SceneLeaderboardWirer] Instantiated LeaderboardPanel prefab in MapSelection Canvas.");
            }
            else
            {
                Debug.LogError("[SceneLeaderboardWirer] LeaderboardPanel prefab not found!");
                return false;
            }
        }

        Button lbButton = null;
        Transform existingBtn = canvas.transform.Find("LeaderboardButton");
        if (existingBtn != null)
        {
            lbButton = existingBtn.GetComponent<Button>();
        }

        if (lbButton == null)
        {
            GameObject btnObj = new GameObject("LeaderboardButton", typeof(RectTransform));
            btnObj.transform.SetParent(canvas.transform, false);
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.85f, 0.05f);
            btnRect.anchorMax = new Vector2(0.98f, 0.12f);
            btnRect.offsetMin = Vector2.zero;
            btnRect.offsetMax = Vector2.zero;

            Image btnBg = btnObj.AddComponent<Image>();
            btnBg.color = new Color(0.2f, 0.4f, 0.7f, 1f);
            lbButton = btnObj.AddComponent<Button>();

            GameObject textObj = new GameObject("Text", typeof(RectTransform));
            textObj.transform.SetParent(btnObj.transform, false);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "LEADERBOARD";
            tmp.fontSize = 14;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            Debug.Log("[SceneLeaderboardWirer] Created LeaderboardButton in MapSelection scene.");
        }

        if (lbButton != null && leaderboardUI != null)
        {
            UnityEventTools.RemovePersistentListener(lbButton.onClick, leaderboardUI.Open);
            UnityEventTools.AddPersistentListener(lbButton.onClick, leaderboardUI.Open);
            EditorUtility.SetDirty(lbButton);
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[SceneLeaderboardWirer] MapSelection scene saved successfully.");
        return true;
    }

    /// <summary>
    /// Wires CombatTimer and NameEntryPanel UI into the Combat scene and connects to VictoryDefeatUI.
    /// </summary>
    /// <returns>True if Combat scene wiring succeeded; otherwise false.</returns>
    public static bool WireCombatScene()
    {
        string scenePath = "Assets/Scenes/Combat.unity";
        var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

        // 1. Find Canvas (validate before scene mutation)
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[SceneLeaderboardWirer] Canvas not found in Combat scene!");
            return false;
        }

        // 2. Ensure CombatTimer exists
        if (Object.FindFirstObjectByType<CombatTimer>() == null)
        {
            GameObject timerObj = new GameObject("CombatTimer");
            timerObj.AddComponent<CombatTimer>();
            Debug.Log("[SceneLeaderboardWirer] Created CombatTimer in Combat scene.");
        }

        // 2.5 Ensure LeaderboardManager exists
        if (Object.FindFirstObjectByType<LeaderboardManager>() == null)
        {
            GameObject mgrObj = new GameObject("LeaderboardManager");
            mgrObj.AddComponent<LeaderboardManager>();
            Debug.Log("[SceneLeaderboardWirer] Created LeaderboardManager in Combat scene.");
        }

        // 3. Instantiate NameEntryPanel if not present
        NameEntryUI nameEntryUI = canvas.GetComponentInChildren<NameEntryUI>(true);
        if (nameEntryUI == null)
        {
            GameObject panelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Leaderboard/NameEntryPanel.prefab");
            if (panelPrefab != null)
            {
                GameObject panelInstance = (GameObject)PrefabUtility.InstantiatePrefab(panelPrefab, canvas.transform);
                nameEntryUI = panelInstance.GetComponent<NameEntryUI>();
                Debug.Log("[SceneLeaderboardWirer] Instantiated NameEntryPanel prefab in Combat Canvas.");
            }
            else
            {
                Debug.LogError("[SceneLeaderboardWirer] NameEntryPanel prefab not found!");
                return false;
            }
        }

        // 4. Wire to VictoryDefeatUI and set sorting orders
        VictoryDefeatUI victoryDefeatUI = Object.FindFirstObjectByType<VictoryDefeatUI>();
        if (victoryDefeatUI != null)
        {
            Canvas vCanvas = victoryDefeatUI.GetComponent<Canvas>();
            if (vCanvas != null)
            {
                vCanvas.overrideSorting = true;
                vCanvas.sortingOrder = 1000;
            }
        }

        if (nameEntryUI != null)
        {
            SetUILayer(nameEntryUI.gameObject);
            Canvas nameCanvas = nameEntryUI.GetComponent<Canvas>();
            if (nameCanvas == null)
            {
                nameCanvas = nameEntryUI.gameObject.AddComponent<Canvas>();
            }
            nameCanvas.overrideSorting = true;
            nameCanvas.sortingOrder = 2000;
            if (nameEntryUI.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            {
                nameEntryUI.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            }
        }

        if (victoryDefeatUI != null && nameEntryUI != null)
        {
            SerializedObject vso = new SerializedObject(victoryDefeatUI);
            vso.FindProperty("nameEntryUI").objectReferenceValue = nameEntryUI;
            vso.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(victoryDefeatUI);
            Debug.Log("[SceneLeaderboardWirer] Wired NameEntryUI to VictoryDefeatUI.");
        }
        else
        {
            Debug.LogWarning("[SceneLeaderboardWirer] VictoryDefeatUI or NameEntryUI was null in Combat scene.");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[SceneLeaderboardWirer] Combat scene saved successfully.");
        return true;
    }

    private static void SetUILayer(GameObject go)
    {
        int uiLayer = LayerMask.NameToLayer("UI");
        if (uiLayer < 0) uiLayer = 5;
        go.layer = uiLayer;
        foreach (Transform child in go.transform)
        {
            SetUILayer(child.gameObject);
        }
    }
}
#endif
