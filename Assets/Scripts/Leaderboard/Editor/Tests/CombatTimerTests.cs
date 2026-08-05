using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Unit tests for CombatTimer state management and LevelTransitionData clear time payload.
/// </summary>
[TestFixture]
public class CombatTimerTests
{
    private GameObject _timerObject;
    private CombatTimer _timer;

    [SetUp]
    public void SetUp()
    {
        _timerObject = new GameObject("CombatTimer_Test");
        _timer = _timerObject.AddComponent<CombatTimer>();
    }

    [TearDown]
    public void TearDown()
    {
        if (_timerObject != null)
        {
            Object.DestroyImmediate(_timerObject);
        }
    }

    /// <summary>
    /// Verifies initial state of CombatTimer.
    /// </summary>
    [Test]
    public void CombatTimer_InitialState_IsNotRunningAndZeroElapsed()
    {
        Assert.IsFalse(_timer.IsRunning);
        Assert.AreEqual(0f, _timer.ElapsedSeconds);
    }

    /// <summary>
    /// Verifies state changes on StartTimer, StopTimer, and ResetTimer.
    /// </summary>
    [Test]
    public void CombatTimer_StartStopReset_UpdatesStateCorrectly()
    {
        _timer.StartTimer();
        Assert.IsTrue(_timer.IsRunning);

        _timer.StopTimer();
        Assert.IsFalse(_timer.IsRunning);

        _timer.ResetTimer();
        Assert.IsFalse(_timer.IsRunning);
        Assert.AreEqual(0f, _timer.ElapsedSeconds);
    }

    /// <summary>
    /// Verifies that LevelTransitionData.Clear() resets SelectedMapData.
    /// </summary>
    [Test]
    public void LevelTransitionData_Clear_ResetsSelectedMapData()
    {
        var data = ScriptableObject.CreateInstance<LevelTransitionData>();
        var mapData = ScriptableObject.CreateInstance<MapData>();
        data.SelectedMapData = mapData;

        data.Clear();

        Assert.IsNull(data.SelectedMapData);

        Object.DestroyImmediate(mapData);
        Object.DestroyImmediate(data);
    }
}
