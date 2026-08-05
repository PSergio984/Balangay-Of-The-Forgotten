## What to build

Wire the leaderboard into the MapSelection scene. Instantiate the `LeaderboardPanel` prefab as a child of the existing root Canvas. Add a "Leaderboard" Button to the MapSelection UI whose `onClick` calls `LeaderboardUI.Open()`. The `LeaderboardManager` singleton handles its own creation if missing. Satisfies spec R7.2 and AC-8 (MapSelection access point).

## Acceptance criteria

- [ ] `LeaderboardPanel` prefab instance is a child of the MapSelection Canvas
- [ ] A "Leaderboard" Button is visible in the MapSelection UI
- [ ] Clicking the button opens the leaderboard panel in-scene (AC-8)
- [ ] The Close button hides the panel
- [ ] No Missing Reference warnings in the Inspector for any leaderboard GameObjects
- [ ] All existing Leaderboard tests still pass

## Blocked by

- feat(leaderboard): create LeaderboardPanel prefab
