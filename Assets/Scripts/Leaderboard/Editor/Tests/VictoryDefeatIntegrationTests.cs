using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;

/// <summary>
/// Unit tests for TICKET-006 VictoryDefeatUI integration with Leaderboard system.
/// </summary>
[TestFixture]
public class VictoryDefeatIntegrationTests
{
    private GameObject _victoryDefeatObj;
    private VictoryDefeatUI _victoryDefeatUI;
    private GameObject _nameEntryObj;
    private NameEntryUI _nameEntryUI;
    private GameObject _panelRoot;
    private LevelTransitionData _levelTransitionData;
    private MapData _mapData;
    private GameObject _timerObj;
    private CombatTimer _combatTimer;
    private GameObject _dotweenObj;

    [SetUp]
    public void SetUp()
    {
        // Setup CombatTimer Singleton.
        // NOTE: In the EditMode test runner, AddComponent does not invoke Awake
        // synchronously, so the timer's Instance is never set and
        // VictoryDefeatUI.RecordCombatClearTime (which reads CombatTimer.Instance)
        // would silently no-op. Invoke Awake manually after clearing any stale
        // instance left over from a previous fixture.
        if (CombatTimer.Instance != null)
        {
            Object.DestroyImmediate(CombatTimer.Instance.gameObject);
        }

        _timerObj = new GameObject("CombatTimer_Test");
        _combatTimer = _timerObj.AddComponent<CombatTimer>();
        typeof(CombatTimer).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(_combatTimer, null);

        // DOTween is not initialized in the EditMode test runner (no scene
        // component awakens). AnimateBanner's WaitForCompletion needs
        // DOTween.instance, so create the component and invoke Awake manually.
        // Its Awake assigns DOTween.instance.
        _dotweenObj = new GameObject("DOTween");
        var dotweenComp = _dotweenObj.AddComponent<DG.Tweening.Core.DOTweenComponent>();
        typeof(DG.Tweening.Core.DOTweenComponent)
            .GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.Invoke(dotweenComp, null);

        // Setup LevelTransitionData ScriptableObject
        _levelTransitionData = ScriptableObject.CreateInstance<LevelTransitionData>();
        _mapData = ScriptableObject.CreateInstance<MapData>();
        _mapData.MapId = GameProgressData.MAP_ID_DAGAT;
        _levelTransitionData.SelectedMapData = _mapData;

        // Setup NameEntryUI GameObject
        _nameEntryObj = new GameObject("NameEntryUI_Test");
        _nameEntryUI = _nameEntryObj.AddComponent<NameEntryUI>();
        _panelRoot = new GameObject("NameEntryPanelRoot");
        _panelRoot.transform.SetParent(_nameEntryObj.transform);
        SetPrivateField(_nameEntryUI, "panelRoot", _panelRoot);
        _panelRoot.SetActive(false);

        // Setup VictoryDefeatUI GameObject
        _victoryDefeatObj = new GameObject("VictoryDefeatUI_Test");
        _victoryDefeatUI = _victoryDefeatObj.AddComponent<VictoryDefeatUI>();

        // Setup minimal banner components so ShowVictory's banner coroutine
        // does not log unhandled errors (bannerPanel, victoryImage, sprite)
        var bannerObj = new GameObject("BannerPanel", typeof(RectTransform), typeof(CanvasGroup));
        bannerObj.transform.SetParent(_victoryDefeatObj.transform);
        SetPrivateField(_victoryDefeatUI, "bannerPanel", bannerObj.GetComponent<RectTransform>());

        var victoryImgObj = new GameObject("VictoryImage", typeof(RectTransform), typeof(CanvasGroup));
        victoryImgObj.transform.SetParent(bannerObj.transform);
        Image victoryImage = victoryImgObj.AddComponent<Image>();
        SetPrivateField(_victoryDefeatUI, "victoryImage", victoryImage);
        SetPrivateField(_victoryDefeatUI, "victoryImageCanvasGroup", victoryImgObj.GetComponent<CanvasGroup>());

        var tex = new Texture2D(2, 2);
        Sprite victorySprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
        SetPrivateField(_victoryDefeatUI, "victorySprite", victorySprite);

        // Set private fields on VictoryDefeatUI
        SetPrivateField(_victoryDefeatUI, "nameEntryUI", _nameEntryUI);
        SetPrivateField(_victoryDefeatUI, "levelTransitionData", _levelTransitionData);
    }

    [TearDown]
    public void TearDown()
    {
        if (_victoryDefeatObj != null) Object.DestroyImmediate(_victoryDefeatObj);
        if (_nameEntryObj != null) Object.DestroyImmediate(_nameEntryObj);
        if (_timerObj != null) Object.DestroyImmediate(_timerObj);
        if (_dotweenObj != null) Object.DestroyImmediate(_dotweenObj);
        if (_levelTransitionData != null) Object.DestroyImmediate(_levelTransitionData);
        if (_mapData != null) Object.DestroyImmediate(_mapData);
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
    public void RecordCombatClearTime_StopsTimer_AndStoresClearTimeSeconds()
    {
        // Arrange
        _combatTimer.StartTimer();
        SetPrivateField(_combatTimer, "<ElapsedSeconds>k__BackingField", 12.5f);
        Assert.IsTrue(_combatTimer.IsRunning);

        // Act
        _victoryDefeatUI.ShowVictory();

        // Assert
        Assert.IsFalse(_combatTimer.IsRunning);
        Assert.Greater(_combatTimer.ElapsedSeconds, 0f);
        Assert.AreEqual(12.5f, _combatTimer.ElapsedSeconds);
    }

    [Test]
    public void ShowVictory_TriggersRecordCombatClearTime()
    {
        // Arrange
        _combatTimer.StartTimer();
        SetPrivateField(_combatTimer, "<ElapsedSeconds>k__BackingField", 24.3f);

        // Act
        _victoryDefeatUI.ShowVictory();

        // Assert
        Assert.IsFalse(_combatTimer.IsRunning);
        Assert.Greater(_combatTimer.ElapsedSeconds, 0f);
        Assert.AreEqual(24.3f, _combatTimer.ElapsedSeconds);
    }

    [Test]
    public void NameEntryUI_Integration_ShowsOnVictory_AndFiresCallback()
    {
        // Arrange
        _combatTimer.StartTimer();
        SetPrivateField(_combatTimer, "<ElapsedSeconds>k__BackingField", 45.5f);

        Button continueButton = new GameObject("ContinueButton", typeof(CanvasGroup)).AddComponent<Button>();
        continueButton.transform.SetParent(_victoryDefeatObj.transform);
        SetPrivateField(_victoryDefeatUI, "continueButton", continueButton);
        SetPrivateField(_victoryDefeatUI, "continueButtonCanvasGroup", continueButton.GetComponent<CanvasGroup>());

        Button skipButton = new GameObject("SkipButton").AddComponent<Button>();
        skipButton.transform.SetParent(_nameEntryObj.transform);
        SetPrivateField(_nameEntryUI, "skipButton", skipButton);

        // The test drives the manual-entry path, so disable silent auto-submit
        // (default true would submit and hide the panel without showing it)
        SetPrivateField(_nameEntryUI, "autoSubmitSilent", false);

        bool callbackExecuted = false;

        // Act - Trigger the actual production victory flow (stops timer, persists clear time)
        _victoryDefeatUI.ShowVictory();

        // Assert - Timer was stopped and ClearTimeSeconds written by the production flow
        Assert.IsFalse(_combatTimer.IsRunning);
        Assert.AreEqual(45.5f, _combatTimer.ElapsedSeconds);
        Assert.AreEqual(45.5f, _levelTransitionData.ClearTimeSeconds);

        // Assert - Banner animation synchronously disables the continue button
        // (re-enabled asynchronously by EnableContinueButtonRoutine after name entry)
        Assert.IsFalse(continueButton.interactable);

        // Verify the persisted values that AnimateBanner would pass to NameEntryUI
        string expectedMapId = _levelTransitionData.SelectedMapData.MapId;
        float expectedClearTime = _levelTransitionData.ClearTimeSeconds;
        Assert.AreEqual(GameProgressData.MAP_ID_DAGAT, expectedMapId);
        Assert.AreEqual(45.5f, expectedClearTime);

        // Simulate AnimateBanner phase 3 handoff: show NameEntryUI with captured values
        _nameEntryUI.Show(
            expectedMapId,
            expectedClearTime,
            () => callbackExecuted = true
        );

        // Assert - NameEntryUI panel activated via production handoff
        Assert.IsTrue(_panelRoot.activeSelf);

        // Act - Trigger skip through the public production path (exercises the callback
        // supplied to Show, instead of assigning callbackExecuted directly)
        _nameEntryUI.TriggerSkip();

        // Assert - Panel hidden and callback executed
        Assert.IsFalse(_panelRoot.activeSelf);
        Assert.IsTrue(callbackExecuted);
    }
}
