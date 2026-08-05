using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Mock repository for isolated testing of LeaderboardManager business logic.
/// </summary>
public class MockLeaderboardRepository : ILeaderboardRepository
{
    public LeaderboardSaveData StoredData = new LeaderboardSaveData();
    public bool SaveCalled = false;
    public bool ShouldFailSave = false;

    public LeaderboardSaveData Load()
    {
        return StoredData;
    }

    public bool Save(LeaderboardSaveData data)
    {
        if (ShouldFailSave) return false;
        StoredData = data;
        SaveCalled = true;
        return true;
    }
}

/// <summary>
/// Unit tests for LeaderboardManager business logic.
/// </summary>
[TestFixture]
public class LeaderboardManagerTests
{
    private GameObject _managerObject;
    private LeaderboardManager _manager;
    private MockLeaderboardRepository _mockRepo;

    [SetUp]
    public void SetUp()
    {
        _managerObject = new GameObject("LeaderboardManager_Test");
        _manager = _managerObject.AddComponent<LeaderboardManager>();
        _mockRepo = new MockLeaderboardRepository();
        _manager.Initialize(_mockRepo);
    }

    [TearDown]
    public void TearDown()
    {
        if (_managerObject != null)
        {
            Object.DestroyImmediate(_managerObject);
        }
    }

    /// <summary>
    /// Verifies name trimming, length capping, and default fallback to "Anonymous".
    /// </summary>
    [Test]
    public void SubmitEntry_NormalizesPlayerNameCorrectly()
    {
        // Case 1: Empty name -> Anonymous
        _manager.SubmitEntry("", GameProgressData.MAP_ID_DAGAT, 100f);
        var entries = _manager.GetTopEntriesForMap(GameProgressData.MAP_ID_DAGAT);
        Assert.AreEqual(1, entries.Count);
        Assert.AreEqual("Anonymous", entries[0].PlayerName);

        // Case 2: Whitespace and trimming
        _manager.SubmitEntry("  Maria  ", GameProgressData.MAP_ID_DAGAT, 80f);
        entries = _manager.GetTopEntriesForMap(GameProgressData.MAP_ID_DAGAT);
        Assert.AreEqual(2, entries.Count);
        Assert.AreEqual("Maria", entries[0].PlayerName); // Sorted faster first (80s < 100s)

        // Case 3: > 20 chars capped at 20
        string longName = "123456789012345678901234567890";
        _manager.SubmitEntry(longName, GameProgressData.MAP_ID_DARAGANG, 50f);
        var daragangEntries = _manager.GetTopEntriesForMap(GameProgressData.MAP_ID_DARAGANG);
        Assert.AreEqual(1, daragangEntries.Count);
        Assert.AreEqual(20, daragangEntries[0].PlayerName.Length);
        Assert.AreEqual("12345678901234567890", daragangEntries[0].PlayerName);
    }

    /// <summary>
    /// Verifies per-map query filtering, ascending sort order, and limit count.
    /// </summary>
    [Test]
    public void GetTopEntriesForMap_FiltersSortsAndLimitsCount()
    {
        for (int i = 1; i <= 15; i++)
        {
            _manager.SubmitEntry($"Player_{i}", GameProgressData.MAP_ID_DAGAT, 200f - i * 5f);
        }

        var top10 = _manager.GetTopEntriesForMap(GameProgressData.MAP_ID_DAGAT, 10);

        Assert.AreEqual(10, top10.Count);
        Assert.AreEqual("Player_15", top10[0].PlayerName);
        Assert.AreEqual(125f, top10[0].ClearTime); // 200 - 15*5 = 125
        Assert.IsTrue(top10[0].ClearTime <= top10[1].ClearTime);
    }

    /// <summary>
    /// Verifies overall tab returns empty list when no player has cleared all 4 maps.
    /// </summary>
    [Test]
    public void GetOverallTopEntries_ReturnsEmptyWhenNoPlayerHasAllFourMaps()
    {
        _manager.SubmitEntry("Player1", GameProgressData.MAP_ID_DAGAT, 60f);
        _manager.SubmitEntry("Player1", GameProgressData.MAP_ID_DARAGANG, 60f);
        _manager.SubmitEntry("Player1", GameProgressData.MAP_ID_BUNDOK, 60f);
        // Missing Kaluwalhatian

        var overall = _manager.GetOverallTopEntries();

        Assert.AreEqual(0, overall.Count);
    }

    /// <summary>
    /// Verifies overall score uses personal best per map for players with all 4 maps.
    /// </summary>
    [Test]
    public void GetOverallTopEntries_CalculatesSumOfPersonalBestsCorrectly()
    {
        // Player1 clears all 4 maps (with 2 runs on Dagat: 90s and 60s)
        _manager.SubmitEntry("Player1", GameProgressData.MAP_ID_DAGAT, 90f);
        _manager.SubmitEntry("Player1", GameProgressData.MAP_ID_DAGAT, 60f); // PB: 60s
        _manager.SubmitEntry("Player1", GameProgressData.MAP_ID_DARAGANG, 50f);
        _manager.SubmitEntry("Player1", GameProgressData.MAP_ID_BUNDOK, 40f);
        _manager.SubmitEntry("Player1", GameProgressData.MAP_ID_KALUWALHATIAN, 30f);

        // Player2 clears all 4 maps (Total: 200s)
        _manager.SubmitEntry("Player2", GameProgressData.MAP_ID_DAGAT, 50f);
        _manager.SubmitEntry("Player2", GameProgressData.MAP_ID_DARAGANG, 50f);
        _manager.SubmitEntry("Player2", GameProgressData.MAP_ID_BUNDOK, 50f);
        _manager.SubmitEntry("Player2", GameProgressData.MAP_ID_KALUWALHATIAN, 50f);

        var overall = _manager.GetOverallTopEntries();

        Assert.AreEqual(2, overall.Count);
        // Player1 sum of PBs: 60 + 50 + 40 + 30 = 180s
        // Player2 sum: 50 + 50 + 50 + 50 = 200s
        Assert.AreEqual("Player1", overall[0].PlayerName);
        Assert.AreEqual(180f, overall[0].TotalBestTime);

        Assert.AreEqual("Player2", overall[1].PlayerName);
        Assert.AreEqual(200f, overall[1].TotalBestTime);
    }

    /// <summary>
    /// Verifies ClearLeaderboard empties in-memory entries and persists the empty state.
    /// </summary>
    [Test]
    public void ClearLeaderboard_ClearsEntriesAndPersists()
    {
        _manager.SubmitEntry("Player1", GameProgressData.MAP_ID_DAGAT, 60f);
        _manager.SubmitEntry("Player2", GameProgressData.MAP_ID_BUNDOK, 45f);
        Assert.AreEqual(2, _manager.GetTopEntriesForMap(GameProgressData.MAP_ID_DAGAT, 100).Count
            + _manager.GetTopEntriesForMap(GameProgressData.MAP_ID_BUNDOK, 100).Count);

        bool cleared = _manager.ClearLeaderboard();

        Assert.IsTrue(cleared);
        Assert.IsTrue(_mockRepo.SaveCalled);
        Assert.AreEqual(0, _mockRepo.StoredData.Entries.Count);
        Assert.AreEqual(0, _manager.GetTopEntriesForMap(GameProgressData.MAP_ID_DAGAT, 100).Count);
        Assert.AreEqual(0, _manager.GetOverallTopEntries(100).Count);
    }
}
