using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[TestFixture]
public class SessionIdentityTests
{
    private GameProgressData _progressData;

    [SetUp]
    public void SetUp()
    {
        _progressData = ScriptableObject.CreateInstance<GameProgressData>();
        _progressData.ResetProgress();
    }

    [TearDown]
    public void TearDown()
    {
        if (_progressData != null)
        {
            _progressData.ResetProgress();
            Object.DestroyImmediate(_progressData);
        }
    }

    [Test]
    public void GameProgressData_DefaultPlayerName_ReturnsAnonymous()
    {
        Assert.IsFalse(_progressData.HasPlayerName);
        Assert.AreEqual("Anonymous", _progressData.PlayerName);
    }

    [Test]
    public void GameProgressData_SetPlayerName_NormalizesAndPersists()
    {
        _progressData.SetPlayerName("  HeroOfDagat  ");

        Assert.IsTrue(_progressData.HasPlayerName);
        Assert.AreEqual("HeroOfDagat", _progressData.PlayerName);

        // Verify persistence via fresh Load
        var freshInstance = ScriptableObject.CreateInstance<GameProgressData>();
        freshInstance.Load();
        Assert.AreEqual("HeroOfDagat", freshInstance.PlayerName);
        Object.DestroyImmediate(freshInstance);
    }

    [Test]
    public void GameProgressData_SetPlayerName_TruncatesExceedingLength()
    {
        string longName = "A Very Long Player Name That Exceeds Twenty Chars";
        _progressData.SetPlayerName(longName);

        Assert.AreEqual(20, _progressData.PlayerName.Length);
        Assert.AreEqual("A Very Long Player N", _progressData.PlayerName);
    }

    [Test]
    public void GameProgressData_ResetProgress_ClearsPlayerName()
    {
        _progressData.SetPlayerName("ActivePlayer");
        _progressData.MarkMapComplete(GameProgressData.MAP_ID_DAGAT);

        _progressData.ResetProgress();

        Assert.IsFalse(_progressData.HasPlayerName);
        Assert.AreEqual("Anonymous", _progressData.PlayerName);
        Assert.AreEqual(0, _progressData.CompletedMapCount);
    }

    [Test]
    public void SessionNameEntryUI_SetPlayerNameOnPlay()
    {
        var uiObj = new GameObject("SessionNameEntryUI_Test");
        var sessionUI = uiObj.AddComponent<SessionNameEntryUI>();

        var panelRoot = new GameObject("PanelRoot");
        panelRoot.transform.SetParent(uiObj.transform);

        var inputObj = new GameObject("NameInput");
        inputObj.transform.SetParent(panelRoot.transform);
        var inputField = inputObj.AddComponent<TMP_InputField>();

        var playBtnObj = new GameObject("PlayButton");
        playBtnObj.transform.SetParent(panelRoot.transform);
        var playButton = playBtnObj.AddComponent<Button>();

        SetPrivateField(sessionUI, "panelRoot", panelRoot);
        SetPrivateField(sessionUI, "nameInputField", inputField);
        SetPrivateField(sessionUI, "playButton", playButton);
        SetPrivateField(sessionUI, "gameProgressData", _progressData);

        var awakeMethod = typeof(SessionNameEntryUI).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        awakeMethod?.Invoke(sessionUI, null);

        sessionUI.Show();
        Assert.IsTrue(panelRoot.activeSelf);

        inputField.text = "NewSessionUser";
        playButton.onClick.Invoke();

        Assert.IsFalse(uiObj.activeSelf);
        Assert.AreEqual("NewSessionUser", _progressData.PlayerName);

        Object.DestroyImmediate(uiObj);
    }

    [Test]
    public void GameProgressData_PlayerNameStateChanged_RaisedOnSetAndReset()
    {
        int raised = 0;
        System.Action handler = () => raised++;
        GameProgressData.PlayerNameStateChanged += handler;
        try
        {
            _progressData.SetPlayerName("TestName");
            Assert.AreEqual(1, raised);

            _progressData.ResetProgress();
            Assert.AreEqual(2, raised);
        }
        finally
        {
            GameProgressData.PlayerNameStateChanged -= handler;
        }
    }

    [Test]
    public void MainMenu_StartButton_StaysEnabledAndStartSessionOpensNamePanelWithoutName()
    {
        var menuObj = new GameObject("MainMenu_Test");
        var menu = menuObj.AddComponent<MainMenu>();

        var startBtnObj = new GameObject("Start");
        startBtnObj.transform.SetParent(menuObj.transform);
        var startButton = startBtnObj.AddComponent<Button>();
        SetPrivateField(menu, "startButton", startButton);
        SetPrivateField(menu, "gameProgressData", _progressData);

        // Wire a SessionNameEntryUI like the MainMenu scene does.
        var sessionUiObj = new GameObject("SessionNameEntryUI_Test");
        sessionUiObj.transform.SetParent(menuObj.transform);
        var sessionUI = sessionUiObj.AddComponent<SessionNameEntryUI>();
        var panelRoot = new GameObject("PanelRoot");
        panelRoot.transform.SetParent(sessionUiObj.transform);
        SetPrivateField(sessionUI, "panelRoot", panelRoot);
        SetPrivateField(sessionUI, "gameProgressData", _progressData);
        SetPrivateField(menu, "sessionNameEntryUI", sessionUI);

        var startMethod = typeof(MainMenu).GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        startMethod?.Invoke(menu, null);

        // The Start button is never locked; without a name it opens the name panel instead.
        Assert.IsTrue(startButton.interactable);

        menu.StartSession();

        Assert.IsTrue(sessionUiObj.activeSelf);
        Assert.IsFalse(_progressData.HasPlayerName);

        Object.DestroyImmediate(menuObj);
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(target, value);
    }
}
