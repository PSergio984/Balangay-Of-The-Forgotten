# 04 — MainMenu scene — Leaderboard access point

## What to build

Wire the leaderboard into the MainMenu scene. Add a `LeaderboardManager` singleton GameObject. Instantiate the `LeaderboardPanel` prefab as a child of the existing root Canvas. Add a "Leaderboard" Button to the MainMenu UI whose `onClick` calls `LeaderboardUI.Open()`. Satisfies spec R7.1 and AC-8 (MainMenu access point).

## Acceptance criteria

- [ ] `LeaderboardManager` GameObject exists in MainMenu scene and initialises without errors on Play
- [ ] `LeaderboardPanel` prefab instance is a child of the MainMenu Canvas
- [ ] A "Leaderboard" Button is visible in the MainMenu UI
- [ ] Clicking the button opens the leaderboard panel (no scene transition — AC-9)
- [ ] The Close button hides the panel
- [ ] No Missing Reference warnings in the Inspector for any leaderboard GameObjects
- [ ] All existing Leaderboard tests still pass

## Blocked by

- 02 — `LeaderboardPanel` prefab

**Status:** ready-for-agent
