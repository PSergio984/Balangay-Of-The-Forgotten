# TICKET-001 — Data Models

**Status:** ready-for-human  
**Blocks:** TICKET-002 (Repository), TICKET-004 (LeaderboardManager)  
**Blocked by:** nothing — start here

---

## Goal

Create the two serialisable data classes that every other leaderboard script depends on.

---

## Files to create

### `Assets/Scripts/Leaderboard/LeaderboardEntry.cs`

```csharp
[System.Serializable]
public class LeaderboardEntry
{
    public string PlayerName;   // max 20 chars; "Anonymous" if skipped
    public string MapId;        // matches GameProgressData map ID constants
    public float  ClearTime;    // seconds (e.g. 94.3f)
    public string DateTimeUtc;  // ISO-8601 string, e.g. "2026-07-25T07:30:00Z"
}
```

### `Assets/Scripts/Leaderboard/LeaderboardSaveData.cs`

```csharp
[System.Serializable]
public class LeaderboardSaveData
{
    public System.Collections.Generic.List<LeaderboardEntry> Entries
        = new System.Collections.Generic.List<LeaderboardEntry>();
}
```

---

## Constraints

- No Unity dependencies — plain C# only. Both classes must compile in a non-Unity test runner.
- Do NOT add any methods, properties, or logic. Data only.
- `[System.Serializable]` required for `JsonUtility`.

---

## Done when

- [x] Both files compile with zero errors.
- [x] `new LeaderboardSaveData()` produces an object with an empty `Entries` list (no null).
