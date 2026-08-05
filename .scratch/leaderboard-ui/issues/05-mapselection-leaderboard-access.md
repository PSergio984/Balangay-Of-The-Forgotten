# 05 — MapSelection scene — Leaderboard access point

## What to build

Wire the leaderboard into the MapSelection scene. Instantiate the `LeaderboardPanel` prefab as a child of the existing root Canvas. Add a "Leaderboard" Button to the MapSelection UI whose `onClick` calls `LeaderboardUI.Open()`. The `LeaderboardManager` singleton will already exist from the MainMenu scene (or be created fresh if MapSelection is the entry point — `Singleton<T>` handles this). Satisfies spec R7.2 and AC-8 (MapSelection access point).

## Acceptance criteria

- [ ] `LeaderboardPanel` prefab instance is a child of the MapSelection Canvas
- [ ] A "Leaderboard" Button is visible in the MapSelection UI
- [ ] Clicking the button opens the leaderboard panel in-scene (AC-8)
- [ ] The Close button hides the panel
- [ ] No Missing Reference warnings in the Inspector for any leaderboard GameObjects
- [ ] All existing Leaderboard tests still pass

## Blocked by

- 02 — `LeaderboardPanel` prefab

**Status:** ready-for-agent
