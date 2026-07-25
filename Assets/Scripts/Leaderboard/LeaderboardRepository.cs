using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Interface for reading and writing leaderboard data to persistent storage.
/// </summary>
public interface ILeaderboardRepository
{
    /// <summary>
    /// Loads the stored LeaderboardSaveData.
    /// Returns a new empty LeaderboardSaveData if missing or corrupted.
    /// </summary>
    LeaderboardSaveData Load();

    /// <summary>
    /// Saves the given LeaderboardSaveData synchronously to persistent storage.
    /// Returns true if successful; false if a write/serialization error occurred.
    /// </summary>
    bool Save(LeaderboardSaveData data);
}

/// <summary>
/// JSON file implementation of ILeaderboardRepository using UnityEngine.JsonUtility and System.IO.File.
/// </summary>
public class LeaderboardRepository : ILeaderboardRepository
{
    private readonly string _filePath;

    /// <summary>
    /// Initializes a new instance of the LeaderboardRepository.
    /// </summary>
    /// <param name="customFilePath">Optional custom path for testing or override. Defaults to Application.persistentDataPath/leaderboard.json</param>
    public LeaderboardRepository(string customFilePath = null)
    {
        _filePath = !string.IsNullOrEmpty(customFilePath)
            ? customFilePath
            : Path.Combine(Application.persistentDataPath, "leaderboard.json");
    }

    /// <inheritdoc />
    public LeaderboardSaveData Load()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return new LeaderboardSaveData();
            }

            string json = File.ReadAllText(_filePath);
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("[LeaderboardRepository] Leaderboard file is empty. Returning new LeaderboardSaveData.");
                return new LeaderboardSaveData();
            }

            LeaderboardSaveData saveData = JsonUtility.FromJson<LeaderboardSaveData>(json);
            if (saveData == null)
            {
                Debug.LogWarning("[LeaderboardRepository] Failed to parse JSON into LeaderboardSaveData. Returning new LeaderboardSaveData.");
                return new LeaderboardSaveData();
            }

            if (saveData.Entries == null)
            {
                saveData.Entries = new System.Collections.Generic.List<LeaderboardEntry>();
            }

            return saveData;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[LeaderboardRepository] Exception occurred while loading leaderboard data: {ex.Message}. Returning new LeaderboardSaveData.");
            return new LeaderboardSaveData();
        }
    }

    /// <inheritdoc />
    public bool Save(LeaderboardSaveData data)
    {
        if (data == null)
        {
            data = new LeaderboardSaveData();
        }

        string tempPath = _filePath + ".tmp";
        try
        {
            string directoryPath = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string json = JsonUtility.ToJson(data, false);
            File.WriteAllText(tempPath, json);

            if (File.Exists(_filePath))
            {
                File.Replace(tempPath, _filePath, null);
            }
            else
            {
                File.Move(tempPath, _filePath);
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[LeaderboardRepository] Exception occurred while saving leaderboard data: {ex.Message}");
            return false;
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                try
                {
                    File.Delete(tempPath);
                }
                catch
                {
                    // Ignore temp file cleanup failure
                }
            }
        }
    }
}
