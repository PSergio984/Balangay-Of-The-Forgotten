using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Overlay panel controller for displaying top leaderboard rankings across 4 map tabs and an Overall tab.
/// </summary>
public class LeaderboardUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button closeButton;

    [Header("Tabs")]
    [SerializeField] private Button[] tabButtons;
    [SerializeField] private string[] tabMapIds;

    [Header("Row List")]
    [SerializeField] private Transform rowContainer;
    [SerializeField] private LeaderboardEntryRowUI rowPrefab;

    private int _currentTabIndex = 0;
    private bool _isValidConfig = true;

    /// <summary>
    /// Configures the leaderboard UI with its required dependencies.
    /// Call this in tests instead of assigning private fields via reflection.
    /// </summary>
    public void Configure(GameObject panelRoot, Button closeButton, Button[] tabButtons, string[] tabMapIds, Transform rowContainer, LeaderboardEntryRowUI rowPrefab)
    {
        this.panelRoot = panelRoot;
        this.closeButton = closeButton;
        this.tabButtons = tabButtons;
        this.tabMapIds = tabMapIds;
        this.rowContainer = rowContainer;
        this.rowPrefab = rowPrefab;
        _isValidConfig = true;
        Initialize();
    }

    private void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        // Startup validation of tab configuration
        if (!ValidateTabConfiguration())
        {
            _isValidConfig = false;
            Debug.LogWarning("[LeaderboardUI] Serialized tab configuration is invalid. Leaderboard UI disabled.", this);
            return;
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }

        if (tabButtons != null)
        {
            for (int i = 0; i < tabButtons.Length; i++)
            {
                int tabIndex = i;
                if (tabButtons[i] != null)
                {
                    tabButtons[i].onClick.RemoveAllListeners();
                    tabButtons[i].onClick.AddListener(() => ShowTab(tabIndex));
                }
            }
        }
    }

    private bool ValidateTabConfiguration()
    {
        if (tabButtons == null || tabButtons.Length != 5) return false;
        if (tabMapIds == null || tabMapIds.Length != 5) return false;

        for (int i = 0; i < 5; i++)
        {
            if (tabButtons[i] == null) return false;
            if (string.IsNullOrWhiteSpace(tabMapIds[i])) return false;
        }

        return true;
    }

    /// <summary>
    /// Opens the leaderboard overlay panel, populates the default tab (tab 0), and sets UI focus.
    /// </summary>
    public void Open()
    {
        if (!_isValidConfig)
        {
            Debug.LogWarning("[LeaderboardUI] Cannot open LeaderboardUI: configuration is invalid.", this);
            return;
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        ShowTab(0);
        FocusInitialControl();
    }

    private void FocusInitialControl()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null) return;

        // Prefer the currently displayed tab for keyboard focus
        if (tabButtons != null)
        {
            if (_currentTabIndex >= 0 && _currentTabIndex < tabButtons.Length)
            {
                var currentTabBtn = tabButtons[_currentTabIndex];
                if (currentTabBtn != null && currentTabBtn.gameObject.activeInHierarchy && currentTabBtn.interactable)
                {
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(currentTabBtn.gameObject);
                    return;
                }
            }

            for (int i = 0; i < tabButtons.Length; i++)
            {
                if (tabButtons[i] != null && tabButtons[i].gameObject.activeInHierarchy && tabButtons[i].interactable)
                {
                    UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(tabButtons[i].gameObject);
                    return;
                }
            }
        }

        if (closeButton != null && closeButton.gameObject.activeInHierarchy && closeButton.interactable)
        {
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(closeButton.gameObject);
        }
    }

    /// <summary>
    /// Closes the leaderboard overlay panel.
    /// </summary>
    public void Close()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    /// <summary>
    /// Swaps the active tab and populates leaderboard rows for the selected category.
    /// </summary>
    /// <param name="tabIndex">0-3 for per-map tabs, 4 for Overall tab.</param>
    private void ShowTab(int tabIndex)
    {
        _currentTabIndex = tabIndex;

        ClearExistingRows();

        if (LeaderboardManager.Instance == null || rowPrefab == null || rowContainer == null)
        {
            return;
        }

        // Per-map tab (0 to 3)
        if (tabIndex < 4)
        {
            string mapId = (tabMapIds != null && tabIndex < tabMapIds.Length) ? tabMapIds[tabIndex] : string.Empty;
            if (!string.IsNullOrEmpty(mapId))
            {
                var entries = LeaderboardManager.Instance.GetTopEntriesForMap(mapId);
                for (int i = 0; i < entries.Count; i++)
                {
                    var row = Instantiate(rowPrefab, rowContainer);
                    row.Populate(i + 1, entries[i].PlayerName, entries[i].ClearTime);
                }
            }
        }
        // Overall tab (4)
        else if (tabIndex == 4)
        {
            var overallEntries = LeaderboardManager.Instance.GetOverallTopEntries();
            for (int i = 0; i < overallEntries.Count; i++)
            {
                var row = Instantiate(rowPrefab, rowContainer);
                row.Populate(i + 1, overallEntries[i].PlayerName, overallEntries[i].TotalBestTime);
            }
        }

        UpdateTabVisuals(tabIndex);
    }

    private void ClearExistingRows()
    {
        if (rowContainer == null) return;

        // Collect children first to avoid mutation issues during iteration
        var children = new List<GameObject>();
        foreach (Transform child in rowContainer)
        {
            children.Add(child.gameObject);
        }

        foreach (var childObj in children)
        {
            DestroyImmediate(childObj);
        }
    }

    private void UpdateTabVisuals(int activeIndex)
    {
        if (tabButtons == null) return;

        for (int i = 0; i < tabButtons.Length; i++)
        {
            if (tabButtons[i] != null)
            {
                tabButtons[i].interactable = true;
            }
        }
    }
}
