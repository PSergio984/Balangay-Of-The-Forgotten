#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Editor utility to apply real UI styling (CharacterSelectionBG sprite and 04B_03__ SDF font)
/// to the existing Leaderboard prefabs.
/// </summary>
public static class ApplyLeaderboardUIStyling
{
    private const string BG_SPRITE_PATH = "Assets/Art/UI/Character Selection/CharacterSelectionBG.png";
    private const string FONT_ASSET_PATH = "Assets/Fonts/04B_03__ SDF.asset";
    private const string BACK_BUTTON_SPRITE_PATH = "Assets/Art/UI/Character Selection/BackButton.png";
    private const string TAB_BG_SPRITE_PATH = "Assets/Art/UI/Character Selection/CurrentBuildBg.png";

    private const string ROW_PREFAB_PATH = "Assets/Prefabs/Leaderboard/LeaderboardEntryRow.prefab";
    private const string NAME_ENTRY_PREFAB_PATH = "Assets/Prefabs/Leaderboard/NameEntryPanel.prefab";
    private const string PANEL_PREFAB_PATH = "Assets/Prefabs/Leaderboard/LeaderboardPanel.prefab";

    [MenuItem("Tools/Leaderboard/Apply UI Styling")]
    public static void ApplyStyling()
    {
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BG_SPRITE_PATH);
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FONT_ASSET_PATH);
        Sprite backBtnSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BACK_BUTTON_SPRITE_PATH);
        Sprite tabBgSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TAB_BG_SPRITE_PATH);

        if (bgSprite == null)
        {
            Debug.LogError($"[ApplyLeaderboardUIStyling] Failed to load background sprite at {BG_SPRITE_PATH}");
            return;
        }

        if (fontAsset == null)
        {
            Debug.LogError($"[ApplyLeaderboardUIStyling] Failed to load font asset at {FONT_ASSET_PATH}");
            return;
        }

        Debug.Log("[ApplyLeaderboardUIStyling] Successfully loaded UI assets. Applying to prefabs...");

        StyleEntryRowPrefab(fontAsset);
        StyleNameEntryPanelPrefab(bgSprite, fontAsset);
        StyleLeaderboardPanelPrefab(bgSprite, fontAsset, backBtnSprite, tabBgSprite);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[ApplyLeaderboardUIStyling] Leaderboard prefabs updated successfully! Wiring scenes...");
        
        // Re-wire scenes so scene instances match updated prefabs
        SceneLeaderboardWirer.WireAllScenes();

        Debug.Log("[ApplyLeaderboardUIStyling] All Leaderboard UI styling applied and scenes re-wired cleanly!");
    }

    private static void StyleEntryRowPrefab(TMP_FontAsset fontAsset)
    {
        GameObject prefabObj = PrefabUtility.LoadPrefabContents(ROW_PREFAB_PATH);
        if (prefabObj == null)
        {
            Debug.LogError($"[ApplyLeaderboardUIStyling] Could not load prefab contents for {ROW_PREFAB_PATH}");
            return;
        }

        TextMeshProUGUI[] texts = prefabObj.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var t in texts)
        {
            t.font = fontAsset;
        }

        PrefabUtility.SaveAsPrefabAsset(prefabObj, ROW_PREFAB_PATH);
        PrefabUtility.UnloadPrefabContents(prefabObj);
        Debug.Log($"[ApplyLeaderboardUIStyling] Applied styling to {ROW_PREFAB_PATH}");
    }

    private static void StyleNameEntryPanelPrefab(Sprite bgSprite, TMP_FontAsset fontAsset)
    {
        GameObject prefabObj = PrefabUtility.LoadPrefabContents(NAME_ENTRY_PREFAB_PATH);
        if (prefabObj == null)
        {
            Debug.LogError($"[ApplyLeaderboardUIStyling] Could not load prefab contents for {NAME_ENTRY_PREFAB_PATH}");
            return;
        }

        // 1. Style background image on the modal 'Panel'
        Transform modalTransform = prefabObj.transform.Find("Panel");
        if (modalTransform != null)
        {
            Image modalImg = modalTransform.GetComponent<Image>();
            if (modalImg != null)
            {
                modalImg.sprite = bgSprite;
                modalImg.type = bgSprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                modalImg.color = Color.white;
            }
        }

        // 2. Apply font to all TMP texts
        TextMeshProUGUI[] texts = prefabObj.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var t in texts)
        {
            t.font = fontAsset;
        }

        // 3. Highlight Title Text with Gold color (#FFD700)
        Transform titleTransform = prefabObj.transform.Find("Panel/TitleText");
        if (titleTransform != null)
        {
            TextMeshProUGUI titleTmp = titleTransform.GetComponent<TextMeshProUGUI>();
            if (titleTmp != null)
            {
                ColorUtility.TryParseHtmlString("#FFD700", out Color goldColor);
                titleTmp.color = goldColor;
                titleTmp.fontSize = 20;
            }
        }

        PrefabUtility.SaveAsPrefabAsset(prefabObj, NAME_ENTRY_PREFAB_PATH);
        PrefabUtility.UnloadPrefabContents(prefabObj);
        Debug.Log($"[ApplyLeaderboardUIStyling] Applied styling to {NAME_ENTRY_PREFAB_PATH}");
    }

    private static void StyleLeaderboardPanelPrefab(Sprite bgSprite, TMP_FontAsset fontAsset, Sprite backBtnSprite, Sprite tabBgSprite)
    {
        GameObject prefabObj = PrefabUtility.LoadPrefabContents(PANEL_PREFAB_PATH);
        if (prefabObj == null)
        {
            Debug.LogError($"[ApplyLeaderboardUIStyling] Could not load prefab contents for {PANEL_PREFAB_PATH}");
            return;
        }

        // 1. Style background image on the inner modal 'Panel'
        Transform modalTransform = prefabObj.transform.Find("Panel");
        if (modalTransform != null)
        {
            Image modalImg = modalTransform.GetComponent<Image>();
            if (modalImg != null)
            {
                modalImg.sprite = bgSprite;
                modalImg.type = bgSprite.border.sqrMagnitude > 0 ? Image.Type.Sliced : Image.Type.Simple;
                modalImg.color = Color.white;
            }
        }

        // 2. Apply font to all TMP texts
        TextMeshProUGUI[] texts = prefabObj.GetComponentsInChildren<TextMeshProUGUI>(true);
        foreach (var t in texts)
        {
            t.font = fontAsset;
        }

        // 3. Highlight Header Title with Gold color (#FFD700)
        Transform titleTransform = prefabObj.transform.Find("Panel/Header/TitleText");
        if (titleTransform != null)
        {
            TextMeshProUGUI titleTmp = titleTransform.GetComponent<TextMeshProUGUI>();
            if (titleTmp != null)
            {
                ColorUtility.TryParseHtmlString("#FFD700", out Color goldColor);
                titleTmp.color = goldColor;
                titleTmp.fontSize = 24;
            }
        }

        // 4. Style TabBar Buttons
        if (tabBgSprite != null)
        {
            Transform tabBarTransform = prefabObj.transform.Find("Panel/TabBar");
            if (tabBarTransform != null)
            {
                Image[] tabImgs = tabBarTransform.GetComponentsInChildren<Image>(true);
                foreach (var img in tabImgs)
                {
                    img.sprite = tabBgSprite;
                    img.color = Color.white;
                }
            }
        }

        // 4. Style & Reparent Close Button (Fixed 32x32 square anchored at top-right corner of Panel)
        Transform closeBtnTransform = prefabObj.transform.Find("Panel/Header/CloseButton");
        if (closeBtnTransform == null)
        {
            closeBtnTransform = prefabObj.transform.Find("Panel/CloseButton");
        }

        if (closeBtnTransform != null)
        {
            // Ensure direct child of Panel (outside layout groups)
            if (modalTransform != null && closeBtnTransform.parent != modalTransform)
            {
                closeBtnTransform.SetParent(modalTransform, false);
            }

            // Ensure LayoutElement has ignoreLayout = true so VerticalLayoutGroup ignores it
            LayoutElement layoutElem = closeBtnTransform.GetComponent<LayoutElement>();
            if (layoutElem == null)
            {
                layoutElem = closeBtnTransform.gameObject.AddComponent<LayoutElement>();
            }
            layoutElem.ignoreLayout = true;

            RectTransform closeRect = closeBtnTransform.GetComponent<RectTransform>();
            if (closeRect != null)
            {
                closeRect.anchorMin = new Vector2(1f, 1f);
                closeRect.anchorMax = new Vector2(1f, 1f);
                closeRect.pivot = new Vector2(1f, 1f);
                closeRect.anchoredPosition = new Vector2(-15f, -15f);
                closeRect.sizeDelta = new Vector2(32f, 32f);
            }

            Image btnImg = closeBtnTransform.GetComponent<Image>();
            if (btnImg != null)
            {
                btnImg.sprite = null; // Use clean flat square image to prevent wide button distortion
                btnImg.color = new Color(0.75f, 0.15f, 0.15f, 1f);
            }
        }

        PrefabUtility.SaveAsPrefabAsset(prefabObj, PANEL_PREFAB_PATH);
        PrefabUtility.UnloadPrefabContents(prefabObj);
        Debug.Log($"[ApplyLeaderboardUIStyling] Applied styling to {PANEL_PREFAB_PATH}");
    }
}
#endif
