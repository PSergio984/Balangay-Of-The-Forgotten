# TICKET-004 — LeaderboardManager (Business Logic)

**Status:** ready-for-agent  
**Blocks:** TICKET-005 (NameEntryUI), TICKET-007 (LeaderboardUI)  
**Blocked by:** TICKET-001 (data models), TICKET-002 (repository)

---

## Goal

Build the singleton manager that owns all leaderboard business logic: submitting a new entry, querying per-map top 10, and computing the Overall top 10.

---

## Files to create

### `Assets/Scripts/Leaderboard/LeaderboardManager.cs`

```
public class LeaderboardManager : Singleton<LeaderboardManager>
{
    // Injected at Awake() — creates a new LeaderboardRepository() by default
    // (allows replacement in tests or future remote version)
    private ILeaderboardRepository _repository;
    private LeaderboardSaveData    _data;

    protected override void Awake()
    {
        base.Awake();
        if (_repository == null)
        {
            Initialize(new LeaderboardRepository());
        }
    }

    public void Initialize(ILeaderboardRepository repository)
    {
        _repository = repository ?? new LeaderboardRepository();
        _data = _repository.Load() ?? new LeaderboardSaveData();
    }

    // --- Public API ---

    // Submit a completed run.
    // playerName: trimmed, capped to 20 chars, "Anonymous" if empty
    // mapId: one of the GameProgressData.MAP_ID_* constants
    // clearTimeSeconds: from LevelTransitionData.ClearTimeSeconds
    public void SubmitEntry(string playerName, string mapId, float clearTimeSeconds)

    // Returns top 10 entries for a single map, sorted ascending by ClearTime
    public List<LeaderboardEntry> GetTopEntriesForMap(string mapId, int count = 10)

    // Returns top 10 overall entries (players with all 4 maps cleared),
    // scored by sum of personal bests, sorted ascending
    public List<OverallLeaderboardEntry> GetOverallTopEntries(int count = 10)
}

// Lightweight result type for the Overall tab (not persisted)
public class OverallLeaderboardEntry
{
    public string PlayerName;
    public float  TotalBestTime; // sum of per-map personal bests
}
```

---

## Business logic details

### `SubmitEntry`
1. Normalise name: `playerName.Trim()`, cap at 20 chars, default `"Anonymous"` if empty after trim.
2. Build a `LeaderboardEntry` with current UTC time: `System.DateTime.UtcNow.ToString("o")`.
3. Append to `_data.Entries`.
4. Call `_repository.Save(_data)`.

### `GetTopEntriesForMap`
1. Filter `_data.Entries` where `entry.MapId == mapId`.
2. Sort ascending by `ClearTime`.
3. Return first `count` entries.

### `GetOverallTopEntries`
All 4 map IDs: use `GameProgressData.MAP_ID_DAGAT`, `MAP_ID_DARAGANG`, `MAP_ID_BUNDOK`, `MAP_ID_KALUWALHATIAN`.

1. Collect all distinct player names in `_data.Entries`.
2. For each player name, check if they have at least one entry for **each of the 4 maps**.
3. If yes: compute their personal best per map (min `ClearTime` per map), sum the 4 bests → `TotalBestTime`.
4. Build a list of `OverallLeaderboardEntry` for qualifying players.
5. Sort ascending by `TotalBestTime`.
6. Return first `count`.

---

## Constraints

- `LeaderboardManager` extends `Singleton<LeaderboardManager>` (same base as `VictoryDefeatUI`).
- Place a `LeaderboardManager` GameObject in a scene that loads early (e.g. the **Core** scene, or use `DontDestroyOnLoad`).
- `_data` is always non-null — initialised to `new LeaderboardSaveData()` if load fails.
- Player name comparison for Overall is **case-sensitive** (matches exactly as submitted).

---

## Done when

- [ ] `LeaderboardManager` compiles.
- [ ] `SubmitEntry("Alice", MAP_ID_DAGAT, 90f)` → `GetTopEntriesForMap(MAP_ID_DAGAT)` returns one entry with name "Alice", ClearTime 90f.
- [ ] `GetOverallTopEntries()` returns empty list when no player has all 4 maps.
- [ ] `GetOverallTopEntries()` uses personal-best-per-map, not average.
