using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Result data container for the Overall leaderboard tab.
/// Represents a player who cleared all 4 maps, scored by the sum of their personal best clear times.
/// </summary>
public class OverallLeaderboardEntry
{
    public string PlayerName;
    public float TotalBestTime;
}

/// <summary>
/// Singleton manager that owns all leaderboard business logic:
/// submitting new entries, querying per-map top 10, and computing overall top 10 rankings.
/// </summary>
public class LeaderboardManager : Singleton<LeaderboardManager>
{
    private static readonly string[] RequiredOverallMapIds = new[]
    {
        GameProgressData.MAP_ID_DAGAT,
        GameProgressData.MAP_ID_DARAGANG,
        GameProgressData.MAP_ID_BUNDOK,
        GameProgressData.MAP_ID_KALUWALHATIAN
    };

    private ILeaderboardRepository _repository;
    private LeaderboardSaveData _data;

    protected override void Awake()
    {
        base.Awake();

        if (_repository == null)
        {
            Initialize(new LeaderboardRepository());
        }
    }

    /// <summary>
    /// Initializes the manager with a given repository instance.
    /// Useful for dependency injection in unit tests or replacing storage implementation.
    /// </summary>
    public void Initialize(ILeaderboardRepository repository)
    {
        _repository = repository ?? new LeaderboardRepository();
        _data = _repository.Load() ?? new LeaderboardSaveData();
        if (_data.Entries == null)
        {
            _data.Entries = new List<LeaderboardEntry>();
        }
    }

    /// <summary>
    /// Submits a completed run entry.
    /// Normalizes player name (trimmed, max 20 chars, "Anonymous" default if empty).
    /// Returns true if entry was valid and saved successfully; false otherwise.
    /// </summary>
    /// <param name="playerName">Player name provided by user.</param>
    /// <param name="mapId">Target map ID constant.</param>
    /// <param name="clearTimeSeconds">Elapsed clear duration in seconds.</param>
    public bool SubmitEntry(string playerName, string mapId, float clearTimeSeconds)
    {
        if (float.IsNaN(clearTimeSeconds) || float.IsInfinity(clearTimeSeconds) || clearTimeSeconds < 0f)
        {
            Debug.LogWarning($"[LeaderboardManager] Rejecting entry with invalid clear time: {clearTimeSeconds}");
            return false;
        }

        if (string.IsNullOrEmpty(mapId) || !RequiredOverallMapIds.Contains(mapId))
        {
            Debug.LogWarning($"[LeaderboardManager] Rejecting entry with unrecognized map ID: '{mapId}'");
            return false;
        }

        if (_data == null)
        {
            _data = new LeaderboardSaveData();
        }
        if (_data.Entries == null)
        {
            _data.Entries = new List<LeaderboardEntry>();
        }

        string normalizedName = string.IsNullOrWhiteSpace(playerName) ? "Anonymous" : playerName.Trim();
        if (normalizedName.Length > 20)
        {
            normalizedName = normalizedName.Substring(0, 20);
        }
        if (string.IsNullOrWhiteSpace(normalizedName))
        {
            normalizedName = "Anonymous";
        }

        var entry = new LeaderboardEntry
        {
            PlayerName = normalizedName,
            MapId = mapId,
            ClearTime = clearTimeSeconds,
            DateTimeUtc = System.DateTime.UtcNow.ToString("o")
        };

        _data.Entries.Add(entry);
        PruneToTopPerMap(10);

        if (_repository != null)
        {
            bool saveSuccess = _repository.Save(_data);
            if (!saveSuccess)
            {
                _data.Entries.Remove(entry);
            }
            return saveSuccess;
        }

        return true;
    }

    /// <summary>
    /// Keeps at most <paramref name="maxPerMap"/> entries per map (best clear times),
    /// dropping the rest so the stored leaderboard never grows unbounded.
    /// </summary>
    private void PruneToTopPerMap(int maxPerMap)
    {
        if (_data == null || _data.Entries == null || _data.Entries.Count <= maxPerMap)
        {
            return;
        }

        var keep = new HashSet<LeaderboardEntry>();
        foreach (var group in _data.Entries.GroupBy(e => e.MapId))
        {
            foreach (var entry in group
                .Where(e => e != null)
                .OrderBy(e => e.ClearTime)
                .ThenBy(e => e.DateTimeUtc)
                .Take(maxPerMap))
            {
                keep.Add(entry);
            }
        }

        _data.Entries.RemoveAll(e => !keep.Contains(e));
    }

    /// <summary>
    /// Returns the top entries for a specific map ID, sorted ascending by ClearTime.
    /// Filters out legacy or corrupt entries with negative/non-finite clear times or invalid map IDs.
    /// </summary>
    /// <param name="mapId">Map ID constant.</param>
    /// <param name="count">Maximum entries to return (default 10).</param>
    public List<LeaderboardEntry> GetTopEntriesForMap(string mapId, int count = 10)
    {
        if (_data == null || _data.Entries == null)
        {
            return new List<LeaderboardEntry>();
        }

        return _data.Entries
            .Where(e => e != null && e.MapId == mapId && !float.IsNaN(e.ClearTime) && !float.IsInfinity(e.ClearTime) && e.ClearTime >= 0f && RequiredOverallMapIds.Contains(e.MapId))
            .OrderBy(e => e.ClearTime)
            .Take(count)
            .ToList();
    }

    /// <summary>
    /// Clears all leaderboard entries and persists the empty state.
    /// </summary>
    /// <returns>True if the cleared state was saved successfully (or no repository exists); false on save failure.</returns>
    public bool ClearLeaderboard()
    {
        if (_data == null)
        {
            _data = new LeaderboardSaveData();
        }
        if (_data.Entries == null)
        {
            _data.Entries = new List<LeaderboardEntry>();
        }
        _data.Entries.Clear();

        if (_repository == null)
        {
            return true;
        }

        bool saveSuccess = _repository.Save(_data);
        Debug.Log(saveSuccess
            ? "[LeaderboardManager] Leaderboard cleared and saved."
            : "[LeaderboardManager] Leaderboard cleared in memory but failed to save.");
        return saveSuccess;
    }

    /// <summary>
    /// Returns top overall entries for players who have cleared ALL 4 maps.
    /// Scored by the sum of personal best clear times per map, sorted ascending.
    /// </summary>
    /// <param name="count">Maximum entries to return (default 10).</param>
    public List<OverallLeaderboardEntry> GetOverallTopEntries(int count = 10)
    {
        if (_data == null || _data.Entries == null || _data.Entries.Count == 0)
        {
            return new List<OverallLeaderboardEntry>();
        }

        var overallEntries = new List<OverallLeaderboardEntry>();

        // Group by case-sensitive PlayerName
        var playerGroups = _data.Entries
            .Where(e => e != null && !string.IsNullOrEmpty(e.PlayerName) && !string.IsNullOrEmpty(e.MapId) && RequiredOverallMapIds.Contains(e.MapId) && !float.IsNaN(e.ClearTime) && !float.IsInfinity(e.ClearTime) && e.ClearTime >= 0f)
            .GroupBy(e => e.PlayerName);

        foreach (var group in playerGroups)
        {
            var mapClears = group
                .GroupBy(e => e.MapId)
                .ToDictionary(g => g.Key, g => g.Min(e => e.ClearTime));

            // Check if player has cleared all 4 required maps
            bool clearedAllMaps = RequiredOverallMapIds.All(mapId => mapClears.ContainsKey(mapId));
            if (clearedAllMaps)
            {
                float totalBest = RequiredOverallMapIds.Sum(mapId => mapClears[mapId]);
                overallEntries.Add(new OverallLeaderboardEntry
                {
                    PlayerName = group.Key,
                    TotalBestTime = totalBest
                });
            }
        }

        return overallEntries
            .OrderBy(e => e.TotalBestTime)
            .Take(count)
            .ToList();
    }
}
