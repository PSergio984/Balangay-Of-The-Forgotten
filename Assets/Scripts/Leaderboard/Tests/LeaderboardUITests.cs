using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Unit tests for LeaderboardEntryRowUI and LeaderboardUI panel controllers.
/// </summary>
[TestFixture]
public class LeaderboardUITests
{
    private GameObject _managerObject;
    private LeaderboardManager _manager;
    private MockLeaderboardRepository _mockRepo;

    private GameObject _uiObject;
    private LeaderboardUI _leaderboardUI;
    private GameObject _panelRoot;
    private Button _closeButton;
    private Button[] _tabButtons;
    private string[] _tabMapIds;
    private GameObject _rowContainerObj;
    private LeaderboardEntryRowUI _rowPrefab;

    [SetUp]
    public void SetUp()
    {
        // Setup LeaderboardManager Singleton
        _managerObject = new GameObject("LeaderboardManager_Test");
        _manager = _managerObject.AddComponent<LeaderboardManager>();
        _mockRepo = new MockLeaderboardRepository();
        _manager.Initialize(_mockRepo);

        // Setup LeaderboardUI GameObject structure
        _uiObject = new GameObject("LeaderboardUI_Test");
        _leaderboardUI = _uiObject.AddComponent<LeaderboardUI>();

        _panelRoot = new GameObject("PanelRoot");
        _panelRoot.transform.SetParent(_uiObject.transform);

        var closeBtnObj = new GameObject("CloseButton");
        closeBtnObj.transform.SetParent(_panelRoot.transform);
        _closeButton = closeBtnObj.AddComponent<Button>();

        _tabButtons = new Button[5];
        _tabMapIds = new string[]
        {
            GameProgressData.MAP_ID_DAGAT,
            GameProgressData.MAP_ID_DARAGANG,
            GameProgressData.MAP_ID_BUNDOK,
            GameProgressData.MAP_ID_KALUWALHATIAN,
            "Overall"
        };

        for (int i = 0; i < 5; i++)
        {
            var tabBtnObj = new GameObject($"TabButton_{i}");
            tabBtnObj.transform.SetParent(_panelRoot.transform);
            _tabButtons[i] = tabBtnObj.AddComponent<Button>();
        }

        _rowContainerObj = new GameObject("RowContainer");
        _rowContainerObj.transform.SetParent(_panelRoot.transform);

        var rowPrefabObj = new GameObject("RowPrefab");
        _rowPrefab = rowPrefabObj.AddComponent<LeaderboardEntryRowUI>();

        var rankTextObj = new GameObject("RankText");
        rankTextObj.transform.SetParent(rowPrefabObj.transform);
        SetPrivateField(_rowPrefab, "rankText", rankTextObj.AddComponent<TextMeshProUGUI>());

        var nameTextObj = new GameObject("PlayerNameText");
        nameTextObj.transform.SetParent(rowPrefabObj.transform);
        SetPrivateField(_rowPrefab, "playerNameText", nameTextObj.AddComponent<TextMeshProUGUI>());

        var timeTextObj = new GameObject("ClearTimeText");
        timeTextObj.transform.SetParent(rowPrefabObj.transform);
        SetPrivateField(_rowPrefab, "clearTimeText", timeTextObj.AddComponent<TextMeshProUGUI>());

        // Assign fields to LeaderboardUI
        SetPrivateField(_leaderboardUI, "panelRoot", _panelRoot);
        SetPrivateField(_leaderboardUI, "closeButton", _closeButton);
        SetPrivateField(_leaderboardUI, "tabButtons", _tabButtons);
        SetPrivateField(_leaderboardUI, "tabMapIds", _tabMapIds);
        SetPrivateField(_leaderboardUI, "rowContainer", _rowContainerObj.transform);
        SetPrivateField(_leaderboardUI, "rowPrefab", _rowPrefab);

        // Re-invoke Awake after all fields are assigned so tab-button listeners are wired
        var awakeMethod = typeof(LeaderboardUI).GetMethod("Awake",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        awakeMethod?.Invoke(_leaderboardUI, null);
    }

    [TearDown]
    public void TearDown()
    {
        if (_uiObject != null) Object.DestroyImmediate(_uiObject);
        if (_managerObject != null) Object.DestroyImmediate(_managerObject);
        if (_rowPrefab != null && _rowPrefab.gameObject != null) Object.DestroyImmediate(_rowPrefab.gameObject);
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(target, value);
        }
    }

    [Test]
    public void FormatTime_FormatsSecondsToMinutesAndSecondsCorrectly()
    {
        Assert.AreEqual("1:34", LeaderboardEntryRowUI.FormatTime(94.3f));
        Assert.AreEqual("12:05", LeaderboardEntryRowUI.FormatTime(725.0f));
        Assert.AreEqual("0:00", LeaderboardEntryRowUI.FormatTime(0f));
        Assert.AreEqual("0:59", LeaderboardEntryRowUI.FormatTime(59.9f));
    }

    [Test]
    public void LeaderboardEntryRowUI_Populate_SetsTextsCorrectly()
    {
        var rowObj = new GameObject("TestRow");
        var row = rowObj.AddComponent<LeaderboardEntryRowUI>();

        var rankTextObj = new GameObject("RankText");
        rankTextObj.transform.SetParent(rowObj.transform);
        var rankTMP = rankTextObj.AddComponent<TextMeshProUGUI>();
        SetPrivateField(row, "rankText", rankTMP);

        var nameTextObj = new GameObject("PlayerNameText");
        nameTextObj.transform.SetParent(rowObj.transform);
        var nameTMP = nameTextObj.AddComponent<TextMeshProUGUI>();
        SetPrivateField(row, "playerNameText", nameTMP);

        var timeTextObj = new GameObject("ClearTimeText");
        timeTextObj.transform.SetParent(rowObj.transform);
        var timeTMP = timeTextObj.AddComponent<TextMeshProUGUI>();
        SetPrivateField(row, "clearTimeText", timeTMP);

        row.Populate(1, "Datu", 94.3f);

        Assert.AreEqual("#1", rankTMP.text);
        Assert.AreEqual("Datu", nameTMP.text);
        Assert.AreEqual("1:34", timeTMP.text);

        Object.DestroyImmediate(rowObj);
    }

    [Test]
    public void Open_MakesPanelVisible_AndPopulatesTab0Rows()
    {
        _manager.SubmitEntry("Player1", GameProgressData.MAP_ID_DAGAT, 80f);
        _manager.SubmitEntry("Player2", GameProgressData.MAP_ID_DAGAT, 100f);

        _leaderboardUI.Open();

        Assert.IsTrue(_panelRoot.activeSelf);
        Assert.AreEqual(2, _rowContainerObj.transform.childCount);
    }

    [Test]
    public void Close_HidesPanel()
    {
        _leaderboardUI.Open();
        Assert.IsTrue(_panelRoot.activeSelf);

        _leaderboardUI.Close();
        Assert.IsFalse(_panelRoot.activeSelf);
    }

    [Test]
    public void ShowTab_SwitchesTabsAndPopulatesCorrectEntries()
    {
        _manager.SubmitEntry("DagatPlayer", GameProgressData.MAP_ID_DAGAT, 60f);
        _manager.SubmitEntry("DaragangPlayer", GameProgressData.MAP_ID_DARAGANG, 70f);

        _leaderboardUI.Open(); // Tab 0 (Dagat)
        Assert.AreEqual(1, _rowContainerObj.transform.childCount);

        // Switch to Tab 1 (Daragang) via public tab button click event
        _tabButtons[1].onClick.Invoke();

        Assert.AreEqual(1, _rowContainerObj.transform.childCount);
        var nameText = _rowContainerObj.transform.GetChild(0).Find("PlayerNameText").GetComponent<TextMeshProUGUI>().text;
        Assert.AreEqual("DaragangPlayer", nameText);
    }
}
