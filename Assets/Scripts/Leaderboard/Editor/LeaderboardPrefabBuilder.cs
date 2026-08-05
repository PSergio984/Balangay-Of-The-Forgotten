#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor utility to generate and configure Leaderboard prefabs automatically.
/// </summary>
public static class LeaderboardPrefabBuilder
{
    [MenuItem("Tools/Leaderboard/Build All Prefabs")]
    public static void BuildAllPrefabs()
    {
        string dirPath = "Assets/Prefabs/Leaderboard";
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        if (!AssetDatabase.IsValidFolder(dirPath))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "Leaderboard");
        }

        BuildLeaderboardEntryRowPrefab(dirPath + "/LeaderboardEntryRow.prefab");
        BuildNameEntryPanelPrefab(dirPath + "/NameEntryPanel.prefab");
        GameObject panelPrefab = BuildLeaderboardPanelPrefab(dirPath + "/LeaderboardPanel.prefab", dirPath + "/LeaderboardEntryRow.prefab");

        if (panelPrefab == null)
        {
            Debug.LogError("[LeaderboardPrefabBuilder] Prefab build failed.");
            return;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[LeaderboardPrefabBuilder] All Leaderboard prefabs created successfully.");
    }

    public static GameObject BuildLeaderboardEntryRowPrefab(string savePath)
    {
        GameObject root = new GameObject("LeaderboardEntryRow", typeof(RectTransform));
        RectTransform rect = root.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(600, 40);

        HorizontalLayoutGroup hl = root.AddComponent<HorizontalLayoutGroup>();
        hl.childControlWidth = true;
        hl.childControlHeight = true;
        hl.childForceExpandWidth = false;
        hl.childForceExpandHeight = true;
        hl.spacing = 10;
        hl.padding = new RectOffset(15, 15, 5, 5);

        // RankText
        GameObject rankObj = new GameObject("RankText", typeof(RectTransform));
        rankObj.transform.SetParent(root.transform, false);
        TextMeshProUGUI rankTMP = rankObj.AddComponent<TextMeshProUGUI>();
        rankTMP.text = "#1";
        rankTMP.fontSize = 18;
        rankTMP.alignment = TextAlignmentOptions.Right;
        LayoutElement rankLayout = rankObj.AddComponent<LayoutElement>();
        rankLayout.preferredWidth = 60;
        rankLayout.flexibleWidth = 0;

        // PlayerNameText
        GameObject nameObj = new GameObject("PlayerNameText", typeof(RectTransform));
        nameObj.transform.SetParent(root.transform, false);
        TextMeshProUGUI nameTMP = nameObj.AddComponent<TextMeshProUGUI>();
        nameTMP.text = "Player Name";
        nameTMP.fontSize = 18;
        nameTMP.alignment = TextAlignmentOptions.Left;
        LayoutElement nameLayout = nameObj.AddComponent<LayoutElement>();
        nameLayout.flexibleWidth = 1;

        // ClearTimeText
        GameObject timeObj = new GameObject("ClearTimeText", typeof(RectTransform));
        timeObj.transform.SetParent(root.transform, false);
        TextMeshProUGUI timeTMP = timeObj.AddComponent<TextMeshProUGUI>();
        timeTMP.text = "0:00";
        timeTMP.fontSize = 18;
        timeTMP.alignment = TextAlignmentOptions.Right;
        LayoutElement timeLayout = timeObj.AddComponent<LayoutElement>();
        timeLayout.preferredWidth = 80;
        timeLayout.flexibleWidth = 0;

        // LeaderboardEntryRowUI component
        LeaderboardEntryRowUI rowUI = root.AddComponent<LeaderboardEntryRowUI>();
        SerializedObject so = new SerializedObject(rowUI);
        SetObjectReferenceProperty(so, "rankText", rankTMP);
        SetObjectReferenceProperty(so, "playerNameText", nameTMP);
        SetObjectReferenceProperty(so, "clearTimeText", timeTMP);
        so.ApplyModifiedPropertiesWithoutUndo();

        SetUILayer(root);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, savePath);
        Object.DestroyImmediate(root);
        Debug.Log($"[LeaderboardPrefabBuilder] Saved {savePath}");
        return prefab;
    }

    public static GameObject BuildNameEntryPanelPrefab(string savePath)
    {
        // Root panel backdrop
        GameObject root = new GameObject("NameEntryPanel", typeof(RectTransform));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image bgImage = root.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.75f);

        Canvas rootCanvas = root.AddComponent<Canvas>();
        rootCanvas.overrideSorting = true;
        rootCanvas.sortingOrder = 2000;

        root.AddComponent<GraphicRaycaster>();

        CanvasGroup cg = root.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = true;
        cg.interactable = true;

        // Inner Modal Box
        GameObject modalObj = new GameObject("Panel", typeof(RectTransform));
        modalObj.transform.SetParent(root.transform, false);
        RectTransform modalRect = modalObj.GetComponent<RectTransform>();
        modalRect.anchorMin = new Vector2(0.5f, 0.5f);
        modalRect.anchorMax = new Vector2(0.5f, 0.5f);
        modalRect.sizeDelta = new Vector2(400, 250);

        Image modalBg = modalObj.AddComponent<Image>();
        modalBg.color = new Color(0.12f, 0.12f, 0.15f, 0.95f);

        VerticalLayoutGroup vl = modalObj.AddComponent<VerticalLayoutGroup>();
        vl.padding = new RectOffset(20, 20, 20, 20);
        vl.spacing = 15;
        vl.childControlWidth = true;
        vl.childControlHeight = false;
        vl.childForceExpandWidth = true;
        vl.childForceExpandHeight = false;

        // TitleText
        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform));
        titleObj.transform.SetParent(modalObj.transform, false);
        TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "VICTORY! ENTER YOUR NAME";
        titleTMP.fontSize = 20;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = Color.white;
        LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = 35;

        // TMP_InputField
        GameObject inputObj = new GameObject("NameInputField", typeof(RectTransform));
        inputObj.transform.SetParent(modalObj.transform, false);
        Image inputBg = inputObj.AddComponent<Image>();
        inputBg.color = new Color(0.08f, 0.08f, 0.1f, 1f);

        TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();
        inputField.characterLimit = 20;

        LayoutElement inputLayout = inputObj.AddComponent<LayoutElement>();
        inputLayout.preferredHeight = 45;

        // TextArea inside inputField
        GameObject textViewportObj = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
        textViewportObj.transform.SetParent(inputObj.transform, false);
        RectTransform tvRect = textViewportObj.GetComponent<RectTransform>();
        tvRect.anchorMin = Vector2.zero;
        tvRect.anchorMax = Vector2.one;
        tvRect.offsetMin = new Vector2(10, 5);
        tvRect.offsetMax = new Vector2(-10, -5);

        // Placeholder Text
        GameObject placeholderObj = new GameObject("Placeholder", typeof(RectTransform));
        placeholderObj.transform.SetParent(textViewportObj.transform, false);
        RectTransform phRect = placeholderObj.GetComponent<RectTransform>();
        phRect.anchorMin = Vector2.zero;
        phRect.anchorMax = Vector2.one;
        phRect.offsetMin = Vector2.zero;
        phRect.offsetMax = Vector2.zero;

        TextMeshProUGUI placeholderTMP = placeholderObj.AddComponent<TextMeshProUGUI>();
        placeholderTMP.text = "Enter Name...";
        placeholderTMP.fontSize = 16;
        placeholderTMP.fontStyle = FontStyles.Italic;
        placeholderTMP.color = new Color(0.6f, 0.6f, 0.6f, 0.6f);

        // Text Component
        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(textViewportObj.transform, false);
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TextMeshProUGUI textTMP = textObj.AddComponent<TextMeshProUGUI>();
        textTMP.text = "";
        textTMP.fontSize = 16;
        textTMP.color = Color.white;

        inputField.textViewport = tvRect;
        inputField.textComponent = textTMP;
        inputField.placeholder = placeholderTMP;

        // Button Row
        GameObject btnRowObj = new GameObject("ButtonRow", typeof(RectTransform));
        btnRowObj.transform.SetParent(modalObj.transform, false);
        HorizontalLayoutGroup btnHl = btnRowObj.AddComponent<HorizontalLayoutGroup>();
        btnHl.spacing = 15;
        btnHl.childControlWidth = true;
        btnHl.childControlHeight = true;
        btnHl.childForceExpandWidth = true;
        btnHl.childForceExpandHeight = true;
        LayoutElement btnRowLayout = btnRowObj.AddComponent<LayoutElement>();
        btnRowLayout.preferredHeight = 45;

        // Submit Button
        GameObject submitObj = new GameObject("SubmitButton", typeof(RectTransform));
        submitObj.transform.SetParent(btnRowObj.transform, false);
        Image submitBg = submitObj.AddComponent<Image>();
        submitBg.color = new Color(0.2f, 0.6f, 0.3f, 1f);
        Button submitBtn = submitObj.AddComponent<Button>();

        GameObject submitTextObj = new GameObject("Text", typeof(RectTransform));
        submitTextObj.transform.SetParent(submitObj.transform, false);
        RectTransform stRect = submitTextObj.GetComponent<RectTransform>();
        stRect.anchorMin = Vector2.zero;
        stRect.anchorMax = Vector2.one;
        stRect.offsetMin = Vector2.zero;
        stRect.offsetMax = Vector2.zero;
        TextMeshProUGUI submitTMP = submitTextObj.AddComponent<TextMeshProUGUI>();
        submitTMP.text = "SUBMIT";
        submitTMP.fontSize = 16;
        submitTMP.fontStyle = FontStyles.Bold;
        submitTMP.alignment = TextAlignmentOptions.Center;
        submitTMP.color = Color.white;

        // Skip Button
        GameObject skipObj = new GameObject("SkipButton", typeof(RectTransform));
        skipObj.transform.SetParent(btnRowObj.transform, false);
        Image skipBg = skipObj.AddComponent<Image>();
        skipBg.color = new Color(0.35f, 0.35f, 0.4f, 1f);
        Button skipBtn = skipObj.AddComponent<Button>();

        GameObject skipTextObj = new GameObject("Text", typeof(RectTransform));
        skipTextObj.transform.SetParent(skipObj.transform, false);
        RectTransform skRect = skipTextObj.GetComponent<RectTransform>();
        skRect.anchorMin = Vector2.zero;
        skRect.anchorMax = Vector2.one;
        skRect.offsetMin = Vector2.zero;
        skRect.offsetMax = Vector2.zero;
        TextMeshProUGUI skipTMP = skipTextObj.AddComponent<TextMeshProUGUI>();
        skipTMP.text = "SKIP";
        skipTMP.fontSize = 16;
        skipTMP.fontStyle = FontStyles.Bold;
        skipTMP.alignment = TextAlignmentOptions.Center;
        skipTMP.color = Color.white;

        // NameEntryUI component
        NameEntryUI nameUI = root.AddComponent<NameEntryUI>();
        SerializedObject nameSo = new SerializedObject(nameUI);
        SetObjectReferenceProperty(nameSo, "panelRoot", root);
        SetObjectReferenceProperty(nameSo, "nameInputField", inputField);
        SetObjectReferenceProperty(nameSo, "submitButton", submitBtn);
        SetObjectReferenceProperty(nameSo, "skipButton", skipBtn);
        nameSo.ApplyModifiedPropertiesWithoutUndo();

        SetUILayer(root);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, savePath);
        Object.DestroyImmediate(root);
        Debug.Log($"[LeaderboardPrefabBuilder] Saved {savePath}");
        return prefab;
    }

    public static GameObject BuildLeaderboardPanelPrefab(string savePath, string rowPrefabPath)
    {
        LeaderboardEntryRowUI rowPrefab = AssetDatabase.LoadAssetAtPath<LeaderboardEntryRowUI>(rowPrefabPath);
        if (rowPrefab == null)
        {
            GameObject rowObj = AssetDatabase.LoadAssetAtPath<GameObject>(rowPrefabPath);
            if (rowObj != null)
            {
                rowPrefab = rowObj.GetComponent<LeaderboardEntryRowUI>();
            }
        }

        if (rowPrefab == null)
        {
            Debug.LogError($"[LeaderboardPrefabBuilder] Failed to load LeaderboardEntryRowUI component from rowPrefabPath: {rowPrefabPath}. Aborting panel build.");
            return null;
        }

        GameObject root = new GameObject("LeaderboardPanel", typeof(RectTransform));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image bgImage = root.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.75f);

        CanvasGroup cg = root.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = true;
        cg.interactable = true;

        // Main Panel Box
        GameObject panelObj = new GameObject("Panel", typeof(RectTransform));
        panelObj.transform.SetParent(root.transform, false);
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(700, 500);

        Image panelBg = panelObj.AddComponent<Image>();
        panelBg.color = new Color(0.12f, 0.12f, 0.15f, 0.95f);

        VerticalLayoutGroup mainVl = panelObj.AddComponent<VerticalLayoutGroup>();
        mainVl.padding = new RectOffset(20, 20, 15, 15);
        mainVl.spacing = 15;
        mainVl.childControlWidth = true;
        mainVl.childControlHeight = false;
        mainVl.childForceExpandWidth = true;
        mainVl.childForceExpandHeight = false;

        // Header Row
        GameObject headerObj = new GameObject("Header", typeof(RectTransform));
        headerObj.transform.SetParent(panelObj.transform, false);
        HorizontalLayoutGroup headerHl = headerObj.AddComponent<HorizontalLayoutGroup>();
        headerHl.childControlWidth = true;
        headerHl.childControlHeight = true;
        headerHl.childForceExpandWidth = false;
        headerHl.childForceExpandHeight = true;
        LayoutElement headerLayout = headerObj.AddComponent<LayoutElement>();
        headerLayout.preferredHeight = 40;

        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform));
        titleObj.transform.SetParent(headerObj.transform, false);
        TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "LEADERBOARD";
        titleTMP.fontSize = 24;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Left;
        titleTMP.color = Color.white;
        LayoutElement titleL = titleObj.AddComponent<LayoutElement>();
        titleL.flexibleWidth = 1;

        // Close Button (Direct child of panelObj, top-right anchored square outside layout groups)
        GameObject closeObj = new GameObject("CloseButton", typeof(RectTransform));
        closeObj.transform.SetParent(panelObj.transform, false);
        RectTransform closeRect = closeObj.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.anchoredPosition = new Vector2(-15f, -15f);
        closeRect.sizeDelta = new Vector2(32f, 32f);

        LayoutElement closeLayout = closeObj.AddComponent<LayoutElement>();
        closeLayout.ignoreLayout = true;

        Image closeBg = closeObj.AddComponent<Image>();
        closeBg.color = new Color(0.75f, 0.15f, 0.15f, 1f);
        Button closeBtn = closeObj.AddComponent<Button>();

        GameObject closeTextObj = new GameObject("Text", typeof(RectTransform));
        closeTextObj.transform.SetParent(closeObj.transform, false);
        RectTransform ctRect = closeTextObj.GetComponent<RectTransform>();
        ctRect.anchorMin = Vector2.zero;
        ctRect.anchorMax = Vector2.one;
        ctRect.offsetMin = Vector2.zero;
        ctRect.offsetMax = Vector2.zero;
        TextMeshProUGUI closeTMP = closeTextObj.AddComponent<TextMeshProUGUI>();
        closeTMP.text = "X";
        closeTMP.fontSize = 18;
        closeTMP.fontStyle = FontStyles.Bold;
        closeTMP.alignment = TextAlignmentOptions.Center;
        closeTMP.color = Color.white;

        // Tab Bar
        GameObject tabBarObj = new GameObject("TabBar", typeof(RectTransform));
        tabBarObj.transform.SetParent(panelObj.transform, false);
        HorizontalLayoutGroup tabHl = tabBarObj.AddComponent<HorizontalLayoutGroup>();
        tabHl.spacing = 8;
        tabHl.childControlWidth = true;
        tabHl.childControlHeight = true;
        tabHl.childForceExpandWidth = true;
        tabHl.childForceExpandHeight = true;
        LayoutElement tabBarLayout = tabBarObj.AddComponent<LayoutElement>();
        tabBarLayout.preferredHeight = 40;

        string[] tabNames = new string[] { "Dagat", "Daragang", "Bundok", "Kaluwalhatian", "Overall" };
        string[] tabMapIds = new string[]
        {
            GameProgressData.MAP_ID_DAGAT,
            GameProgressData.MAP_ID_DARAGANG,
            GameProgressData.MAP_ID_BUNDOK,
            GameProgressData.MAP_ID_KALUWALHATIAN,
            "OVERALL"
        };

        Sprite tabBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Character Selection/CurrentBuildBg.png");
        Button[] tabButtons = new Button[5];
        for (int i = 0; i < 5; i++)
        {
            GameObject tabBtnObj = new GameObject($"TabButton_{i}", typeof(RectTransform));
            tabBtnObj.transform.SetParent(tabBarObj.transform, false);
            Image tabBg = tabBtnObj.AddComponent<Image>();
            if (tabBtnSprite != null)
            {
                tabBg.sprite = tabBtnSprite;
                tabBg.color = Color.white;
            }
            else
            {
                tabBg.color = new Color(0.2f, 0.25f, 0.3f, 1f);
            }
            Button tabBtn = tabBtnObj.AddComponent<Button>();
            tabButtons[i] = tabBtn;

            GameObject tabTextObj = new GameObject("Text", typeof(RectTransform));
            tabTextObj.transform.SetParent(tabBtnObj.transform, false);
            RectTransform ttRect = tabTextObj.GetComponent<RectTransform>();
            ttRect.anchorMin = Vector2.zero;
            ttRect.anchorMax = Vector2.one;
            ttRect.offsetMin = Vector2.zero;
            ttRect.offsetMax = Vector2.zero;
            TextMeshProUGUI tabTMP = tabTextObj.AddComponent<TextMeshProUGUI>();
            tabTMP.text = tabNames[i];
            tabTMP.fontSize = 14;
            tabTMP.fontStyle = FontStyles.Bold;
            tabTMP.alignment = TextAlignmentOptions.Center;
            tabTMP.color = Color.white;
        }

        // Scroll View Container
        GameObject scrollViewObj = new GameObject("ScrollView", typeof(RectTransform));
        scrollViewObj.transform.SetParent(panelObj.transform, false);
        Image svBg = scrollViewObj.AddComponent<Image>();
        svBg.color = new Color(0.08f, 0.08f, 0.1f, 0.8f);
        ScrollRect sr = scrollViewObj.AddComponent<ScrollRect>();
        LayoutElement svLayout = scrollViewObj.AddComponent<LayoutElement>();
        svLayout.preferredHeight = 350;

        // Viewport
        GameObject viewportObj = new GameObject("Viewport", typeof(RectTransform));
        viewportObj.transform.SetParent(scrollViewObj.transform, false);
        RectTransform vpRect = viewportObj.GetComponent<RectTransform>();
        vpRect.anchorMin = Vector2.zero;
        vpRect.anchorMax = Vector2.one;
        vpRect.offsetMin = Vector2.zero;
        vpRect.offsetMax = Vector2.zero;
        viewportObj.AddComponent<RectMask2D>();

        // Content
        GameObject contentObj = new GameObject("Content", typeof(RectTransform));
        contentObj.transform.SetParent(viewportObj.transform, false);
        RectTransform contentRect = contentObj.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = new Vector2(0, 0);

        VerticalLayoutGroup contentVl = contentObj.AddComponent<VerticalLayoutGroup>();
        contentVl.childControlWidth = true;
        contentVl.childControlHeight = false;
        contentVl.childForceExpandWidth = true;
        contentVl.childForceExpandHeight = false;
        contentVl.spacing = 5;

        ContentSizeFitter csf = contentObj.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        sr.viewport = vpRect;
        sr.content = contentRect;
        sr.horizontal = false;
        sr.vertical = true;

        // LeaderboardUI component
        LeaderboardUI leaderboardUI = root.AddComponent<LeaderboardUI>();
        SerializedObject uiSo = new SerializedObject(leaderboardUI);
        SetObjectReferenceProperty(uiSo, "panelRoot", root);
        SetObjectReferenceProperty(uiSo, "closeButton", closeBtn);

        SerializedProperty tabButtonsProp = FindPropertyGuarded(uiSo, "tabButtons");
        if (tabButtonsProp != null)
        {
            tabButtonsProp.arraySize = 5;
            for (int i = 0; i < 5; i++)
            {
                tabButtonsProp.GetArrayElementAtIndex(i).objectReferenceValue = tabButtons[i];
            }
        }

        SerializedProperty tabMapIdsProp = FindPropertyGuarded(uiSo, "tabMapIds");
        if (tabMapIdsProp != null)
        {
            tabMapIdsProp.arraySize = 5;
            for (int i = 0; i < 5; i++)
            {
                tabMapIdsProp.GetArrayElementAtIndex(i).stringValue = tabMapIds[i];
            }
        }

        SetObjectReferenceProperty(uiSo, "rowContainer", contentObj.transform);
        SetObjectReferenceProperty(uiSo, "rowPrefab", rowPrefab);
        uiSo.ApplyModifiedPropertiesWithoutUndo();

        SetUILayer(root);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, savePath);
        Object.DestroyImmediate(root);
        Debug.Log($"[LeaderboardPrefabBuilder] Saved {savePath}");
        return prefab;
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

    private static void SetObjectReferenceProperty(SerializedObject serializedObject, string propertyName, Object value)
    {
        SerializedProperty prop = serializedObject.FindProperty(propertyName);
        if (prop != null)
        {
            prop.objectReferenceValue = value;
        }
        else
        {
            Debug.LogError($"[LeaderboardPrefabBuilder] Property '{propertyName}' missing on {serializedObject.targetObject.GetType().Name}. Skipping assignment.");
        }
    }

    private static SerializedProperty FindPropertyGuarded(SerializedObject serializedObject, string propertyName)
    {
        SerializedProperty prop = serializedObject.FindProperty(propertyName);
        if (prop == null)
        {
            Debug.LogError($"[LeaderboardPrefabBuilder] Property '{propertyName}' missing on {serializedObject.targetObject.GetType().Name}.");
        }
        return prop;
    }
}
#endif
