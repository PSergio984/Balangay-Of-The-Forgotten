using System.IO;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Unit tests for LeaderboardRepository persistence logic.
/// </summary>
[TestFixture]
public class LeaderboardRepositoryTests
{
    private string _testFilePath;

    [SetUp]
    public void SetUp()
    {
        _testFilePath = Path.Combine(Application.temporaryCachePath, $"test_leaderboard_{System.Guid.NewGuid()}.json");
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_testFilePath))
        {
            File.Delete(_testFilePath);
        }
    }

    /// <summary>
    /// Verifies that Load() returns an empty LeaderboardSaveData when file does not exist.
    /// </summary>
    [Test]
    public void Load_WhenFileDoesNotExist_ReturnsEmptyLeaderboardSaveData()
    {
        var repo = new LeaderboardRepository(_testFilePath);

        var data = repo.Load();

        Assert.IsNotNull(data);
        Assert.IsNotNull(data.Entries);
        Assert.AreEqual(0, data.Entries.Count);
    }

    /// <summary>
    /// Verifies that Load() catches corrupt JSON, logs warning, and returns an empty data object without throwing.
    /// </summary>
    [Test]
    public void Load_WhenFileContainsInvalidJson_ReturnsEmptySaveDataAndDoesNotThrow()
    {


        File.WriteAllText(_testFilePath, "{ invalid json data }");
        var repo = new LeaderboardRepository(_testFilePath);

        LeaderboardSaveData data = null;
        Assert.DoesNotThrow(() =>
        {
            data = repo.Load();
        });

        Assert.IsNotNull(data);
        Assert.IsNotNull(data.Entries);
    }

    /// <summary>
    /// Verifies that Save() followed by Load() round-trips entries accurately.
    /// </summary>
    [Test]
    public void SaveAndLoad_RoundTripsEntriesCorrectly()
    {
        var repo = new LeaderboardRepository(_testFilePath);
        var originalData = new LeaderboardSaveData();
        originalData.Entries.Add(new LeaderboardEntry
        {
            PlayerName = "Juan",
            MapId = GameProgressData.MAP_ID_DAGAT,
            ClearTime = 120.5f,
            DateTimeUtc = "2026-07-25T15:00:00Z"
        });

        repo.Save(originalData);

        var loadedData = repo.Load();

        Assert.IsNotNull(loadedData);
        Assert.AreEqual(1, loadedData.Entries.Count);
        Assert.AreEqual("Juan", loadedData.Entries[0].PlayerName);
        Assert.AreEqual(GameProgressData.MAP_ID_DAGAT, loadedData.Entries[0].MapId);
        Assert.AreEqual(120.5f, loadedData.Entries[0].ClearTime);
        Assert.AreEqual("2026-07-25T15:00:00Z", loadedData.Entries[0].DateTimeUtc);
    }
}
