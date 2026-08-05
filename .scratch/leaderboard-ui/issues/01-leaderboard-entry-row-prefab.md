# 01 — `LeaderboardEntryRow` prefab

## What to build

Create the `LeaderboardEntryRow` Unity prefab — a single horizontal UI row displaying rank, player name, and clear time. Attach the `LeaderboardEntryRowUI` component and wire all three `TMP_Text` fields (`rankText`, `playerNameText`, `clearTimeText`). Save to `Assets/Prefabs/Leaderboard/LeaderboardEntryRow.prefab`. This prefab is instantiated at runtime by `LeaderboardUI.ShowTab()`.

## Acceptance criteria

- [ ] Prefab exists at `Assets/Prefabs/Leaderboard/LeaderboardEntryRow.prefab`
- [ ] Root has a `HorizontalLayoutGroup`
- [ ] Three `TMP_Text` children: RankText (fixed ~60px), PlayerNameText (flexible), ClearTimeText (fixed ~80px)
- [ ] `LeaderboardEntryRowUI` component is attached to the root
- [ ] `rankText`, `playerNameText`, `clearTimeText` serialized fields are wired (no Missing Reference warnings in Inspector)
- [ ] Calling `row.Populate(1, "TestPlayer", 94.3f)` sets texts to `#1`, `TestPlayer`, `1:34` without errors
- [ ] All existing Leaderboard tests still pass

## Blocked by

None — can start immediately.

**Status:** ready-for-agent
