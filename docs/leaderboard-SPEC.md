# Leaderboard Feature — SPEC.md

**Status:** Ready for implementation  
**Source:** Derived from /grill-me session on 2026-07-25  
**Codebase:** Balangay of the Forgotten — Unity C#

---

## What this delivers

A local-only, modular leaderboard system that records per-map clear times, prompts for a player name on every Victory screen, and displays ranked times via a tabbed overlay panel accessible from both the Main Menu and Map Selection screens.

Designed with a clean persistence seam so the local JSON store can be swapped for a database layer later without touching the UI or game-flow code.

---

## Maps in scope

| Map ID (matches `MapData.MapId` / `GameProgressData` constants) | Display Name |
|---|---|
| `"Dagat Ng Kabisayaan"` | Dagat Ng Kabisayaan |
| `"Daragang Magayon"` | Daragang Magayon |
| `"Bundok Pulag"` | Bundok Pulag |
| `"Kaluwalhatian"` | Kaluwalhatian |

---

## Requirements

### R1 — Name entry (Victory screen)
- **R1.1** After every map victory, before navigating away, the game shows a name-entry popup.
- **R1.2** The popup always appears — every run, every player — supporting multiple players on the same machine.
- **R1.3** The popup has a single text input field (TMP_InputField), a **Submit** button, and a **Skip** button.
- **R1.4** Pressing **Skip** records the run with name `"Anonymous"` and does not block progress.
- **R1.5** Name is trimmed of leading/trailing whitespace. Empty input after trimming defaults to `"Anonymous"`.
- **R1.6** Max name length: 20 characters.

### R2 — Leaderboard entry data
- **R2.1** Each entry stores: `PlayerName (string)`, `MapId (string)`, `ClearTime (float, seconds)`, `DateTimeUtc (string, ISO-8601)`.
- **R2.2** No other fields are stored in this version.

### R3 — Per-map tabs (Top 10)
- **R3.1** The leaderboard panel has 5 tabs: one per map + one **Overall** tab.
- **R3.2** Each per-map tab shows the **top 10 fastest clears** for that map, sorted ascending by `ClearTime`.
- **R3.3** Entries beyond rank 10 are stored in the JSON file but not displayed.
- **R3.4** Each row displays: **Rank** (`#1`–`#10`), **Player Name**, **Clear Time** (`m:ss` or `mm:ss`).
- **R3.5** If fewer than 10 entries exist, empty rows are not shown.

### R4 — Overall tab
- **R4.1** The Overall tab shows players who have cleared **all 4 maps** at least once.
- **R4.2** Each player's Overall score = sum of their **personal best clear time** per map (one best time per map per player name, across all their recorded runs).
- **R4.3** Sorted ascending by Overall score. Top 10 shown.
- **R4.4** Each row displays: **Rank**, **Player Name**, **Overall Time** (sum, formatted as `m:ss` or `mm:ss`).

### R5 — Combat timer
- **R5.1** A new `CombatTimer` component starts counting elapsed time when combat begins.
- **R5.2** The timer stops when the victory condition is reached.
- **R5.3** Elapsed time (float, seconds) is written into `LevelTransitionData` (a new field `ClearTimeSeconds`) before any scene transition occurs.
- **R5.4** `VictoryDefeatUI` reads `ClearTimeSeconds` from `LevelTransitionData` when showing victory.

### R6 — Persistence
- **R6.1** Data is stored as JSON at `Application.persistentDataPath/leaderboard.json`.
- **R6.2** The data model is a single wrapper: `LeaderboardSaveData { List<LeaderboardEntry> Entries }`.
- **R6.3** All reads and writes go through `LeaderboardRepository` — nothing else touches the file directly.
- **R6.4** If the file is missing or corrupt, the system starts with an empty entry list and logs a warning. It does not throw.
- **R6.5** The file is written synchronously on `LeaderboardRepository.Save()`.

### R7 — Access points
- **R7.1** A **Leaderboard** button exists on the Main Menu scene.
- **R7.2** A **Leaderboard** button exists on the Map Selection scene.
- **R7.3** Both buttons open the same leaderboard overlay panel in-scene (no scene transition).
- **R7.4** The panel has a **Close** button that hides it.

### R8 — Modularity boundary
- **R8.1** All leaderboard code lives under `Assets/Scripts/Leaderboard/`.
- **R8.2** The rest of the game depends on `LeaderboardManager` for leaderboard queries/submissions. The Main Menu and Map Selection scenes both call `LeaderboardUI.Open()` to show the overlay panel. `VictoryDefeatUI` has a direct serialized dependency on `NameEntryUI` as a component for the name-entry popup flow. `CombatTimer` and `LevelTransitionData` belong to combat/scene infrastructure.
- **R8.3** `LeaderboardManager` creates a `LeaderboardRepository` by default during initialization while exposing an `Initialize(ILeaderboardRepository)` seam to allow injection for testing or alternate storage without touching `LeaderboardManager`.


---

## Explicit out-of-scope (this version)

- No online/database storage.
- No character-used tracking per entry.
- No difficulty tracking per entry.
- No "delete my scores" UI.
- No per-session aggregate timer (all 4 maps in one sitting).
- No sound effects on name entry or leaderboard panel open.

---

## File layout

```
Assets/Scripts/Leaderboard/
  LeaderboardEntry.cs          - [Serializable] data model
  LeaderboardSaveData.cs       - [Serializable] JSON root wrapper
  LeaderboardRepository.cs     - read/write JSON; interface ILeaderboardRepository
  LeaderboardManager.cs        - singleton; submit(), query per-map, query overall
  LeaderboardUI.cs             - overlay panel controller; tabs, row population
  LeaderboardEntryRowUI.cs     - single row prefab controller (rank, name, time)
  NameEntryUI.cs               - Victory screen name popup
```

Modified files:
- `LevelTransitionData.cs` - add `float ClearTimeSeconds` field
- `VictoryDefeatUI.cs` - trigger name entry popup on victory; read `ClearTimeSeconds`
- Main Menu scene - add Leaderboard button wired to `LeaderboardUI.Open()`
- Map Selection scene - add Leaderboard button wired to `LeaderboardUI.Open()`
- Combat scene - add `CombatTimer` component

---

## Acceptance criteria

| # | Criterion | Pass condition |
|---|---|---|
| AC-1 | Name prompt appears every victory | Win a map -> name popup shown before returning to menu |
| AC-2 | Skip records "Anonymous" | Press Skip -> entry saved with name "Anonymous" |
| AC-3 | Entry saved to JSON | After name submit, leaderboard.json contains the new entry |
| AC-4 | Per-map tab shows top 10 | Submit 11+ times on one map -> only 10 rows visible, fastest first |
| AC-5 | Overall tab requires all 4 | Player with only 3 maps cleared does not appear on Overall tab |
| AC-6 | Overall score = sum of personal bests | Player has 2 runs on Dagat (60s, 45s) - Overall uses 45s for Dagat |
| AC-7 | Corrupt file handled gracefully | Delete/corrupt leaderboard.json -> game loads without exception, empty leaderboard shown |
| AC-8 | Both access points work | Leaderboard button functional from Main Menu AND Map Selection |
| AC-9 | Overlay, no scene transition | Leaderboard opens as panel, no loading screen |
| AC-10 | Timer accuracy | Clear time recorded matches visible elapsed seconds within +-1s |

---

## Open questions (Resolved)

- **Tie-breaking for equal clear times**: Entries with identical clear times are ranked in the order they were submitted (first-submitted-first-ranked).
- **Case sensitivity of player names during Overall aggregation**: Player names are compared case-sensitively. `"PlayerOne"` and `"playerone"` are treated as different players.
- **Repeated calls to `Initialize(ILeaderboardRepository)`**: Calling `Initialize` when already initialized will replace the repository and reload data from the new source.
