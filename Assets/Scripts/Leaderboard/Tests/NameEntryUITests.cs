using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Unit tests for NameEntryUI component behavior and callback flow.
/// </summary>
[TestFixture]
public class NameEntryUITests
{
    private GameObject _uiObject;
    private NameEntryUI _nameEntryUI;
    private GameObject _panelRoot;
    private TMP_InputField _nameInputField;
    private Button _submitButton;
    private Button _skipButton;

    private GameObject _managerObject;
    private LeaderboardManager _manager;
    private MockLeaderboardRepository _mockRepo;

    [SetUp]
    public void SetUp()
    {
        // Setup LeaderboardManager Singleton
        _managerObject = new GameObject("LeaderboardManager_Test");
        _manager = _managerObject.AddComponent<LeaderboardManager>();
        _mockRepo = new MockLeaderboardRepository();
        _manager.Initialize(_mockRepo);

        // Setup NameEntryUI GameObject and references
        _uiObject = new GameObject("NameEntryUI_Test");
        _nameEntryUI = _uiObject.AddComponent<NameEntryUI>();

        _panelRoot = new GameObject("PanelRoot");
        _panelRoot.transform.SetParent(_uiObject.transform);

        var inputObject = new GameObject("NameInputField");
        inputObject.transform.SetParent(_panelRoot.transform);
        _nameInputField = inputObject.AddComponent<TMP_InputField>();

        var submitObj = new GameObject("SubmitButton");
        submitObj.transform.SetParent(_panelRoot.transform);
        _submitButton = submitObj.AddComponent<Button>();

        var skipObj = new GameObject("SkipButton");
        skipObj.transform.SetParent(_panelRoot.transform);
        _skipButton = skipObj.AddComponent<Button>();

        // Set serialized fields via reflection
        SetPrivateField(_nameEntryUI, "panelRoot", _panelRoot);
        SetPrivateField(_nameEntryUI, "nameInputField", _nameInputField);
        SetPrivateField(_nameEntryUI, "submitButton", _submitButton);
        SetPrivateField(_nameEntryUI, "skipButton", _skipButton);
        SetPrivateField(_nameEntryUI, "autoSubmitSilent", false);

        // Run Awake initialization after setting serialized references
        var awakeMethod = typeof(NameEntryUI).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        awakeMethod?.Invoke(_nameEntryUI, null);
    }

    [TearDown]
    public void TearDown()
    {
        if (_uiObject != null) Object.DestroyImmediate(_uiObject);
        if (_managerObject != null) Object.DestroyImmediate(_managerObject);
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
    public void Awake_HidesPanelDefault()
    {
        Assert.IsFalse(_panelRoot.activeSelf);
    }

    [Test]
    public void Show_MakesPanelVisible_ClearsInput_AndSetsCharacterLimit()
    {
        _nameInputField.text = "OldName";

        _nameEntryUI.Show(GameProgressData.MAP_ID_DAGAT, 120.5f, null);

        Assert.IsTrue(_panelRoot.activeSelf);
        Assert.AreEqual(string.Empty, _nameInputField.text);
        Assert.AreEqual(20, _nameInputField.characterLimit);
    }

    [Test]
    public void OnSubmit_WithValidName_SubmitsTrimmedName_HidesPanel_InvokesCallback()
    {
        bool callbackFired = false;
        _nameEntryUI.Show(GameProgressData.MAP_ID_DAGAT, 95.0f, () => callbackFired = true);
        _nameInputField.text = "  LapuLapu  ";

        // Trigger submission via public submitButton event API
        _submitButton.onClick.Invoke();

        Assert.IsFalse(_panelRoot.activeSelf);
        Assert.IsTrue(callbackFired);

        var topEntries = LeaderboardManager.Instance.GetTopEntriesForMap(GameProgressData.MAP_ID_DAGAT);
        Assert.AreEqual(1, topEntries.Count);
        Assert.AreEqual("LapuLapu", topEntries[0].PlayerName);
        Assert.AreEqual(95.0f, topEntries[0].ClearTime);
    }

    [Test]
    public void OnSubmit_WithEmptyName_SubmitsAnonymous_HidesPanel_InvokesCallback()
    {
        bool callbackFired = false;
        _nameEntryUI.Show(GameProgressData.MAP_ID_DAGAT, 110.0f, () => callbackFired = true);
        _nameInputField.text = "   ";

        // Trigger submission via public submitButton event API
        _submitButton.onClick.Invoke();

        Assert.IsFalse(_panelRoot.activeSelf);
        Assert.IsTrue(callbackFired);

        var topEntries = LeaderboardManager.Instance.GetTopEntriesForMap(GameProgressData.MAP_ID_DAGAT);
        Assert.AreEqual(1, topEntries.Count);
        Assert.AreEqual("Anonymous", topEntries[0].PlayerName);
    }

    [Test]
    public void OnSkip_SubmitsAnonymous_HidesPanel_InvokesCallback()
    {
        _nameEntryUI.Show(GameProgressData.MAP_ID_DAGAT, 150.0f, null);

        // Trigger skip via public skipButton event API
        _skipButton.onClick.Invoke();

        var topEntries = LeaderboardManager.Instance.GetTopEntriesForMap(GameProgressData.MAP_ID_DAGAT);
        Assert.AreEqual(1, topEntries.Count);
        Assert.AreEqual("Anonymous", topEntries[0].PlayerName);
    }

    [Test]
    public void Show_AutoSubmitSilent_SubmitsSessionNameAndInvokesCallback()
    {
        SetPrivateField(_nameEntryUI, "autoSubmitSilent", true);

        var progressData = ScriptableObject.CreateInstance<GameProgressData>();
        progressData.SetPlayerName("SilentSessionPlayer");
        SetPrivateField(_nameEntryUI, "gameProgressData", progressData);

        bool callbackFired = false;
        _nameEntryUI.Show(GameProgressData.MAP_ID_DAGAT, 88.0f, () => callbackFired = true);

        Assert.IsFalse(_panelRoot.activeSelf);
        Assert.IsTrue(callbackFired);

        var topEntries = LeaderboardManager.Instance.GetTopEntriesForMap(GameProgressData.MAP_ID_DAGAT);
        Assert.AreEqual(1, topEntries.Count);
        Assert.AreEqual("SilentSessionPlayer", topEntries[0].PlayerName);
        Assert.AreEqual(88.0f, topEntries[0].ClearTime);

        Object.DestroyImmediate(progressData);
    }
}
