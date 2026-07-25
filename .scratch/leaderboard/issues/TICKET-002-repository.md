# TICKET-002 — Repository (Persistence Layer)

**Status:** ready-for-agent  
**Blocks:** TICKET-004 (LeaderboardManager)  
**Blocked by:** TICKET-001 (data models must exist first)

---

## Goal

Build the `ILeaderboardRepository` interface and its JSON-file implementation. This is the **only** place that touches `leaderboard.json`. Everything above it talks to the interface.

---

## Files to create

### `Assets/Scripts/Leaderboard/LeaderboardRepository.cs`

Implement:

```
public interface ILeaderboardRepository
{
    LeaderboardSaveData Load();
    void Save(LeaderboardSaveData data);
}

public class LeaderboardRepository : ILeaderboardRepository
{
    // File path: Application.persistentDataPath + "/leaderboard.json"
    // Load():
    //   - If file does not exist → return new LeaderboardSaveData()
    //   - If file is corrupt/unparseable → log warning, return new LeaderboardSaveData()
    //   - Otherwise → return parsed LeaderboardSaveData
    // Save(data):
    //   - Serialise to JSON (JsonUtility.ToJson, prettyPrint: false)
    //   - Write synchronously with File.WriteAllText
}
```

---

## Constraints

- Use `UnityEngine.JsonUtility` for serialisation — no third-party JSON library.
- Use `System.IO.File` for file I/O — no coroutines, no async.
- Load must never throw — all exceptions caught, warning logged with `Debug.LogWarning`.
- Save may throw if the disk is full; that is acceptable (log the exception).
- The file path must be derived at runtime (`Application.persistentDataPath`) — not baked in as a constant.

---

## Done when

- [ ] `LeaderboardRepository` compiles.
- [ ] `Load()` called when file is absent → returns `LeaderboardSaveData` with empty `Entries`.
- [ ] `Load()` called when file contains invalid JSON → logs a warning, returns empty data, no exception propagates.
- [ ] `Save()` then `Load()` round-trips a list of entries correctly.
