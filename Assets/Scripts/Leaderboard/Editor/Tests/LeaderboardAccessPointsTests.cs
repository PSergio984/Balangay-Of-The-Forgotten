using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Unit tests for TICKET-008 Leaderboard Access Points in MainMenu and MapSelectManager2.
/// </summary>
[TestFixture]
public class LeaderboardAccessPointsTests
{
    private GameObject _mainMenuObj;
    private MainMenu _mainMenu;

    private GameObject _mapSelectObj;
    private MapSelectManager2 _mapSelectManager;

    private GameObject _leaderboardUIObj;
    private LeaderboardUI _leaderboardUI;
    private GameObject _panelRoot;

    [SetUp]
    public void SetUp()
    {
        // Setup LeaderboardUI via public Configure seam
        _leaderboardUIObj = new GameObject("LeaderboardUI_Test");
        _leaderboardUI = _leaderboardUIObj.AddComponent<LeaderboardUI>();
        _panelRoot = new GameObject("PanelRoot");
        _panelRoot.transform.SetParent(_leaderboardUIObj.transform);

        var closeBtnObj = new GameObject("CloseButton");
        closeBtnObj.transform.SetParent(_panelRoot.transform);
        Button closeButton = closeBtnObj.AddComponent<Button>();

        Button[] tabButtons = new Button[5];
        for (int i = 0; i < 5; i++)
        {
            var btnObj = new GameObject($"TabButton_{i}");
            btnObj.transform.SetParent(_leaderboardUIObj.transform);
            tabButtons[i] = btnObj.AddComponent<Button>();
        }
        string[] tabMapIds = new string[] { "Map1", "Map2", "Map3", "Map4", "Overall" };

        var rowContainerObj = new GameObject("RowContainer");
        rowContainerObj.transform.SetParent(_leaderboardUIObj.transform);

        _leaderboardUI.Configure(_panelRoot, closeButton, tabButtons, tabMapIds, rowContainerObj.transform, null);

        // Setup MainMenu
        _mainMenuObj = new GameObject("MainMenu_Test");
        _mainMenu = _mainMenuObj.AddComponent<MainMenu>();
        SetPrivateField(_mainMenu, "leaderboardUI", _leaderboardUI);

        // Setup MapSelectManager2 with child handler. Both components are added
        // while inactive so their Awake calls fire only after the hierarchy is
        // complete (each finds the other in its Awake).
        _mapSelectObj = new GameObject("MapSelectManager2_Test");
        _mapSelectObj.SetActive(false);
        var handlerObj = new GameObject("EventHandler");
        handlerObj.transform.SetParent(_mapSelectObj.transform);
        handlerObj.AddComponent<LevelSelectSystemEventHandler2>();
        _mapSelectManager = _mapSelectObj.AddComponent<MapSelectManager2>();
        _mapSelectObj.SetActive(true);
        SetPrivateField(_mapSelectManager, "leaderboardUI", _leaderboardUI);
    }

    [TearDown]
    public void TearDown()
    {
        if (_mainMenuObj != null) Object.DestroyImmediate(_mainMenuObj);
        if (_mapSelectObj != null) Object.DestroyImmediate(_mapSelectObj);
        if (_leaderboardUIObj != null) Object.DestroyImmediate(_leaderboardUIObj);
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field == null)
        {
            Assert.Fail($"Field '{fieldName}' not found on target {target?.GetType().Name}");
        }
        field.SetValue(target, value);
    }

    [Test]
    public void MainMenu_OpenLeaderboard_OpensLeaderboardUI()
    {
        _leaderboardUI.Close();
        Assert.IsFalse(_panelRoot.activeSelf);

        _mainMenu.OpenLeaderboard();

        Assert.IsTrue(_panelRoot.activeSelf);
    }

    [Test]
    public void MapSelectManager2_OpenLeaderboard_OpensLeaderboardUI()
    {
        _leaderboardUI.Close();
        Assert.IsFalse(_panelRoot.activeSelf);

        _mapSelectManager.OpenLeaderboard();

        Assert.IsTrue(_panelRoot.activeSelf);
    }

    [Test]
    public void MainMenu_OpenLeaderboard_WhenUIIsNull_LogsWarningGracefully()
    {
        SetPrivateField(_mainMenu, "leaderboardUI", null);

        // Production code logs: Debug.LogWarning("[MainMenu] leaderboardUI is null! Cannot open leaderboard panel.")
        Assert.DoesNotThrow(() => _mainMenu.OpenLeaderboard());
    }

    [Test]
    public void MapSelectManager2_OpenLeaderboard_WhenUIIsNull_LogsWarningGracefully()
    {
        SetPrivateField(_mapSelectManager, "leaderboardUI", null);

        // Production code logs: Debug.LogWarning("[MapSelectManager2] leaderboardUI is null! Cannot open leaderboard panel.")
        Assert.DoesNotThrow(() => _mapSelectManager.OpenLeaderboard());
    }
}
