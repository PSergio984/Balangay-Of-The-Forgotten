using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Unit tests for LeaderboardEntry and LeaderboardSaveData data models.
/// </summary>
[TestFixture]
public class LeaderboardDataModelsTests
{
    /// <summary>
    /// Verifies that instantiating LeaderboardSaveData initializes an empty non-null Entries list.
    /// </summary>
    [Test]
    public void LeaderboardSaveData_DefaultConstructor_InitializesEmptyEntriesList()
    {
        var saveData = new LeaderboardSaveData();

        Assert.IsNotNull(saveData.Entries);
        Assert.AreEqual(0, saveData.Entries.Count);
    }

    /// <summary>
    /// Verifies that fields in LeaderboardEntry can be properly set and read.
    /// </summary>
    [Test]
    public void LeaderboardEntry_FieldsSetAndGet_StoresValuesAccurately()
    {
        var entry = new LeaderboardEntry
        {
            PlayerName = "LapuLapu",
            MapId = "Mactan_01",
            ClearTime = 125.5f,
            DateTimeUtc = "2026-07-25T07:30:00Z"
        };

        Assert.AreEqual("LapuLapu", entry.PlayerName);
        Assert.AreEqual("Mactan_01", entry.MapId);
        Assert.AreEqual(125.5f, entry.ClearTime);
        Assert.AreEqual("2026-07-25T07:30:00Z", entry.DateTimeUtc);
    }

    /// <summary>
    /// Verifies that LeaderboardSaveData can be serialized to JSON and deserialized back.
    /// </summary>
    [Test]
    public void LeaderboardSaveData_JsonUtilitySerialization_RoundTripsCorrectly()
    {
        var saveData = new LeaderboardSaveData();
        saveData.Entries.Add(new LeaderboardEntry
        {
            PlayerName = "Datu",
            MapId = "Balangay_01",
            ClearTime = 42.0f,
            DateTimeUtc = "2026-07-25T12:00:00Z"
        });

        string json = JsonUtility.ToJson(saveData);
        var deserialized = JsonUtility.FromJson<LeaderboardSaveData>(json);

        Assert.IsNotNull(deserialized);
        Assert.IsNotNull(deserialized.Entries);
        Assert.AreEqual(1, deserialized.Entries.Count);
        Assert.AreEqual("Datu", deserialized.Entries[0].PlayerName);
        Assert.AreEqual("Balangay_01", deserialized.Entries[0].MapId);
        Assert.AreEqual(42.0f, deserialized.Entries[0].ClearTime);
        Assert.AreEqual("2026-07-25T12:00:00Z", deserialized.Entries[0].DateTimeUtc);
    }
}
