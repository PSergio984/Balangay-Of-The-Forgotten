#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor utility to generate and configure Session UI prefabs automatically
/// (SessionNameEntryPanel and NewPlayerResetModal) without modifying LeaderboardPanel or EntryRow.
/// </summary>
public static class SessionUIPrefabBuilder
{
    [MenuItem("Tools/Leaderboard/Build Session UI Prefabs Only")]
    public static void BuildSessionPrefabs()
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

        BuildSessionNameEntryPanelPrefab(dirPath + "/SessionNameEntryPanel.prefab");
        BuildNewPlayerResetModalPrefab(dirPath + "/NewPlayerResetModal.prefab");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[SessionUIPrefabBuilder] Session UI prefabs created successfully.");
    }

    [MenuItem("Tools/Leaderboard/Instantiate Session UI in MainMenu Scene")]
    public static void InstantiateSessionUIInMainMenu()
    {
        string scenePath = "Assets/Scenes/MainMenu.unity";
        if (!System.IO.File.Exists(scenePath))
        {
            Debug.LogError($"[SessionUIPrefabBuilder] MainMenu scene file not found at {scenePath}");
            return;
        }

        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Single);

        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[SessionUIPrefabBuilder] No Canvas found in MainMenu scene!");
            return;
        }

        string dirPath = "Assets/Prefabs/Leaderboard";
        GameObject sessionNamePanelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(dirPath + "/SessionNameEntryPanel.prefab");
        if (sessionNamePanelPrefab == null)
        {
            sessionNamePanelPrefab = BuildSessionNameEntryPanelPrefab(dirPath + "/SessionNameEntryPanel.prefab");
        }

        GameObject newPlayerResetModalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(dirPath + "/NewPlayerResetModal.prefab");
        if (newPlayerResetModalPrefab == null)
        {
            newPlayerResetModalPrefab = BuildNewPlayerResetModalPrefab(dirPath + "/NewPlayerResetModal.prefab");
        }

        Transform existingSessionNamePanel = canvas.transform.Find("SessionNameEntryPanel");
        if (existingSessionNamePanel == null && sessionNamePanelPrefab != null)
        {
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(sessionNamePanelPrefab, canvas.transform);
            inst.name = "SessionNameEntryPanel";
            Undo.RegisterCreatedObjectUndo(inst, "Instantiate SessionNameEntryPanel");
            Debug.Log("[SessionUIPrefabBuilder] Instantiated SessionNameEntryPanel into MainMenu Canvas.");
        }

        Transform existingResetModal = canvas.transform.Find("NewPlayerResetModal");
        if (existingResetModal == null && newPlayerResetModalPrefab != null)
        {
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(newPlayerResetModalPrefab, canvas.transform);
            inst.name = "NewPlayerResetModal";
            Undo.RegisterCreatedObjectUndo(inst, "Instantiate NewPlayerResetModal");
            Debug.Log("[SessionUIPrefabBuilder] Instantiated NewPlayerResetModal into MainMenu Canvas.");
        }

        SessionNameEntryUI sessionUI = canvas.GetComponentInChildren<SessionNameEntryUI>(true);
        NewPlayerButtonUI newPlayerUI = canvas.GetComponentInChildren<NewPlayerButtonUI>(true);

        if (sessionUI != null && newPlayerUI != null)
        {
            SerializedObject newPlayerSo = new SerializedObject(newPlayerUI);
            SerializedProperty sessionProp = newPlayerSo.FindProperty("sessionNameEntryUI");
            if (sessionProp != null)
            {
                sessionProp.objectReferenceValue = sessionUI;
                newPlayerSo.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("[SessionUIPrefabBuilder] Successfully cross-wired NewPlayerButtonUI to SessionNameEntryUI!");
            }
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[SessionUIPrefabBuilder] MainMenu scene updated and saved successfully!");
    }

    public static GameObject BuildSessionNameEntryPanelPrefab(string savePath)
    {
        // Root panel backdrop
        GameObject root = new GameObject("SessionNameEntryPanel", typeof(RectTransform));
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
        modalRect.sizeDelta = new Vector2(480, 280);

        Image modalBg = modalObj.AddComponent<Image>();
        Sprite panelBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Character Selection/CharacterSelectionBG.png");
        if (panelBgSprite != null)
        {
            modalBg.sprite = panelBgSprite;
            modalBg.color = Color.white;
        }
        else
        {
            modalBg.color = new Color(0.12f, 0.12f, 0.15f, 0.95f);
        }

        VerticalLayoutGroup vl = modalObj.AddComponent<VerticalLayoutGroup>();
        vl.padding = new RectOffset(25, 25, 25, 25);
        vl.spacing = 15;
        vl.childControlWidth = true;
        vl.childControlHeight = false;
        vl.childForceExpandWidth = true;
        vl.childForceExpandHeight = false;

        // TitleText
        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform));
        titleObj.transform.SetParent(modalObj.transform, false);
        TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
        titleTMP.text = "WELCOME! ENTER YOUR NAME";
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
        Sprite inputBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Character Selection/CurrentBuildBg.png");
        if (inputBgSprite != null)
        {
            inputBg.sprite = inputBgSprite;
            inputBg.color = Color.white;
        }
        else
        {
            inputBg.color = new Color(0.08f, 0.08f, 0.1f, 1f);
        }

        TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();
        inputField.characterLimit = 20;

        LayoutElement inputLayout = inputObj.AddComponent<LayoutElement>();
        inputLayout.preferredHeight = 45;

        // Text Area inside inputField
        GameObject textViewportObj = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
        textViewportObj.transform.SetParent(inputObj.transform, false);
        RectTransform tvRect = textViewportObj.GetComponent<RectTransform>();
        tvRect.anchorMin = Vector2.zero;
        tvRect.anchorMax = Vector2.one;
        tvRect.offsetMin = new Vector2(12, 5);
        tvRect.offsetMax = new Vector2(-12, -5);

        // Placeholder Text
        GameObject placeholderObj = new GameObject("Placeholder", typeof(RectTransform));
        placeholderObj.transform.SetParent(textViewportObj.transform, false);
        RectTransform phRect = placeholderObj.GetComponent<RectTransform>();
        phRect.anchorMin = Vector2.zero;
        phRect.anchorMax = Vector2.one;
        phRect.offsetMin = Vector2.zero;
        phRect.offsetMax = Vector2.zero;

        TextMeshProUGUI placeholderTMP = placeholderObj.AddComponent<TextMeshProUGUI>();
        placeholderTMP.text = "Enter Player Name...";
        placeholderTMP.fontSize = 16;
        placeholderTMP.fontStyle = FontStyles.Italic;
        placeholderTMP.color = new Color(0.7f, 0.7f, 0.7f, 0.6f);

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

        // Play Button
        GameObject playObj = new GameObject("PlayButton", typeof(RectTransform));
        playObj.transform.SetParent(modalObj.transform, false);
        Image playBg = playObj.AddComponent<Image>();
        Sprite playBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Character Selection/ableSave.png");
        if (playBtnSprite != null)
        {
            playBg.sprite = playBtnSprite;
            playBg.color = Color.white;
        }
        else
        {
            playBg.color = new Color(0.2f, 0.6f, 0.3f, 1f);
        }
        Button playBtn = playObj.AddComponent<Button>();
        LayoutElement playLayout = playObj.AddComponent<LayoutElement>();
        playLayout.preferredHeight = 50;

        GameObject playTextObj = new GameObject("Text", typeof(RectTransform));
        playTextObj.transform.SetParent(playObj.transform, false);
        RectTransform ptRect = playTextObj.GetComponent<RectTransform>();
        ptRect.anchorMin = Vector2.zero;
        ptRect.anchorMax = Vector2.one;
        ptRect.offsetMin = Vector2.zero;
        ptRect.offsetMax = Vector2.zero;
        TextMeshProUGUI playTMP = playTextObj.AddComponent<TextMeshProUGUI>();
        playTMP.text = "START SESSION";
        playTMP.fontSize = 18;
        playTMP.fontStyle = FontStyles.Bold;
        playTMP.alignment = TextAlignmentOptions.Center;
        playTMP.color = Color.white;

        // SessionNameEntryUI component
        SessionNameEntryUI sessionUI = root.AddComponent<SessionNameEntryUI>();
        GameProgressData gameProgressAsset = AssetDatabase.LoadAssetAtPath<GameProgressData>("Assets/Data/Data Persistence/Game Progress.asset");

        SerializedObject uiSo = new SerializedObject(sessionUI);
        SetObjectReferenceProperty(uiSo, "panelRoot", root);
        SetObjectReferenceProperty(uiSo, "nameInputField", inputField);
        SetObjectReferenceProperty(uiSo, "playButton", playBtn);
        if (gameProgressAsset != null)
        {
            SetObjectReferenceProperty(uiSo, "gameProgressData", gameProgressAsset);
        }
        uiSo.ApplyModifiedPropertiesWithoutUndo();

        SetUILayer(root);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, savePath);
        Object.DestroyImmediate(root);
        Debug.Log($"[SessionUIPrefabBuilder] Saved {savePath}");
        return prefab;
    }

    public static GameObject BuildNewPlayerResetModalPrefab(string savePath)
    {
        // Root panel backdrop
        GameObject root = new GameObject("NewPlayerResetModal", typeof(RectTransform));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        Image bgImage = root.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.75f);

        Canvas rootCanvas = root.AddComponent<Canvas>();
        rootCanvas.overrideSorting = true;
        rootCanvas.sortingOrder = 2100;

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
        modalRect.sizeDelta = new Vector2(460, 260);

        Image modalBg = modalObj.AddComponent<Image>();
        Sprite panelBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Character Selection/CharacterSelectionBG.png");
        if (panelBgSprite != null)
        {
            modalBg.sprite = panelBgSprite;
            modalBg.color = Color.white;
        }
        else
        {
            modalBg.color = new Color(0.12f, 0.12f, 0.15f, 0.95f);
        }

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
        titleTMP.text = "START NEW PLAYER SESSION?";
        titleTMP.fontSize = 18;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;
        titleTMP.color = Color.white;
        LayoutElement titleLayout = titleObj.AddComponent<LayoutElement>();
        titleLayout.preferredHeight = 30;

        // Description Text
        GameObject descObj = new GameObject("DescText", typeof(RectTransform));
        descObj.transform.SetParent(modalObj.transform, false);
        TextMeshProUGUI descTMP = descObj.AddComponent<TextMeshProUGUI>();
        descTMP.text = "This will clear active progress for the next player. High scores on the Leaderboard will remain saved.";
        descTMP.fontSize = 14;
        descTMP.alignment = TextAlignmentOptions.Center;
        descTMP.color = new Color(0.85f, 0.85f, 0.85f, 1f);
        LayoutElement descLayout = descObj.AddComponent<LayoutElement>();
        descLayout.preferredHeight = 50;

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

        // Confirm Yes Button
        GameObject yesObj = new GameObject("ConfirmYesButton", typeof(RectTransform));
        yesObj.transform.SetParent(btnRowObj.transform, false);
        Image yesBg = yesObj.AddComponent<Image>();
        Sprite yesSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Character Selection/ableSave.png");
        if (yesSprite != null)
        {
            yesBg.sprite = yesSprite;
            yesBg.color = Color.white;
        }
        else
        {
            yesBg.color = new Color(0.2f, 0.6f, 0.3f, 1f);
        }
        Button yesBtn = yesObj.AddComponent<Button>();

        GameObject yesTextObj = new GameObject("Text", typeof(RectTransform));
        yesTextObj.transform.SetParent(yesObj.transform, false);
        RectTransform ytRect = yesTextObj.GetComponent<RectTransform>();
        ytRect.anchorMin = Vector2.zero;
        ytRect.anchorMax = Vector2.one;
        ytRect.offsetMin = Vector2.zero;
        ytRect.offsetMax = Vector2.zero;
        TextMeshProUGUI yesTMP = yesTextObj.AddComponent<TextMeshProUGUI>();
        yesTMP.text = "YES, RESET";
        yesTMP.fontSize = 15;
        yesTMP.fontStyle = FontStyles.Bold;
        yesTMP.alignment = TextAlignmentOptions.Center;
        yesTMP.color = Color.white;

        // Cancel No Button
        GameObject noObj = new GameObject("ConfirmNoButton", typeof(RectTransform));
        noObj.transform.SetParent(btnRowObj.transform, false);
        Image noBg = noObj.AddComponent<Image>();
        Sprite noSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Character Selection/disabledSave.png");
        if (noSprite != null)
        {
            noBg.sprite = noSprite;
            noBg.color = Color.white;
        }
        else
        {
            noBg.color = new Color(0.4f, 0.4f, 0.4f, 1f);
        }
        Button noBtn = noObj.AddComponent<Button>();

        GameObject noTextObj = new GameObject("Text", typeof(RectTransform));
        noTextObj.transform.SetParent(noObj.transform, false);
        RectTransform ntRect = noTextObj.GetComponent<RectTransform>();
        ntRect.anchorMin = Vector2.zero;
        ntRect.anchorMax = Vector2.one;
        ntRect.offsetMin = Vector2.zero;
        ntRect.offsetMax = Vector2.zero;
        TextMeshProUGUI noTMP = noTextObj.AddComponent<TextMeshProUGUI>();
        noTMP.text = "CANCEL";
        noTMP.fontSize = 15;
        noTMP.fontStyle = FontStyles.Bold;
        noTMP.alignment = TextAlignmentOptions.Center;
        noTMP.color = Color.white;

        // Trigger New Player Button (standalone button component attached to container or header)
        GameObject newPlayerBtnObj = new GameObject("NewPlayerTriggerButton", typeof(RectTransform));
        newPlayerBtnObj.transform.SetParent(root.transform, false);
        Image triggerBg = newPlayerBtnObj.AddComponent<Image>();
        Sprite triggerSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Character Selection/CurrentBuildBg.png");
        if (triggerSprite != null)
        {
            triggerBg.sprite = triggerSprite;
            triggerBg.color = Color.white;
        }
        Button triggerBtn = newPlayerBtnObj.AddComponent<Button>();

        GameObject triggerTextObj = new GameObject("Text", typeof(RectTransform));
        triggerTextObj.transform.SetParent(newPlayerBtnObj.transform, false);
        RectTransform ttRect = triggerTextObj.GetComponent<RectTransform>();
        ttRect.anchorMin = Vector2.zero;
        ttRect.anchorMax = Vector2.one;
        ttRect.offsetMin = Vector2.zero;
        ttRect.offsetMax = Vector2.zero;
        TextMeshProUGUI triggerTMP = triggerTextObj.AddComponent<TextMeshProUGUI>();
        triggerTMP.text = "NEW PLAYER";
        triggerTMP.fontSize = 14;
        triggerTMP.fontStyle = FontStyles.Bold;
        triggerTMP.alignment = TextAlignmentOptions.Center;
        triggerTMP.color = Color.white;

        // NewPlayerButtonUI component
        NewPlayerButtonUI newPlayerUI = root.AddComponent<NewPlayerButtonUI>();
        GameProgressData gameProgressAsset = AssetDatabase.LoadAssetAtPath<GameProgressData>("Assets/Data/Data Persistence/Game Progress.asset");

        SerializedObject uiSo = new SerializedObject(newPlayerUI);
        SetObjectReferenceProperty(uiSo, "newPlayerButton", triggerBtn);
        SetObjectReferenceProperty(uiSo, "confirmationPanelRoot", modalObj);
        SetObjectReferenceProperty(uiSo, "confirmYesButton", yesBtn);
        SetObjectReferenceProperty(uiSo, "confirmNoButton", noBtn);
        if (gameProgressAsset != null)
        {
            SetObjectReferenceProperty(uiSo, "gameProgressData", gameProgressAsset);
        }
        uiSo.ApplyModifiedPropertiesWithoutUndo();

        SetUILayer(root);
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, savePath);
        Object.DestroyImmediate(root);
        Debug.Log($"[SessionUIPrefabBuilder] Saved {savePath}");
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
            Debug.LogError($"[SessionUIPrefabBuilder] Property '{propertyName}' missing on {serializedObject.targetObject.GetType().Name}. Skipping assignment.");
        }
    }
}
#endif
