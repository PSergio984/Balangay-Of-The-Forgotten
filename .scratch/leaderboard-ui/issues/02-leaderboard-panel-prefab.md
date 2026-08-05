# 02 — `LeaderboardPanel` prefab

## What to build

Create the `LeaderboardPanel` Unity prefab — a full-screen overlay panel with 5 tab buttons (one per map + Overall), a scrollable row list, and a Close button. Attach the `LeaderboardUI` component and wire all serialized fields: `panelRoot`, `closeButton`, `tabButtons[0..4]`, `tabMapIds[0..4]`, `rowContainer` (the scroll Content transform), and `rowPrefab` (the `LeaderboardEntryRow` prefab). The panel starts hidden. Save to `Assets/Prefabs/Leaderboard/LeaderboardPanel.prefab`.

`tabMapIds` values (in order): `"Dagat Ng Kabisayaan"`, `"Daragang Magayon"`, `"Bundok Pulag"`, `"Kaluwalhatian"`, `"OVERALL"`.

## Acceptance criteria

- [ ] Prefab exists at `Assets/Prefabs/Leaderboard/LeaderboardPanel.prefab`
- [ ] `LeaderboardUI` component attached; all 6 field groups wired (no Missing References)
- [ ] `tabButtons` array has exactly 5 elements; `tabMapIds` array has exactly 5 matching string values
- [ ] `rowContainer` points to the `Content` transform inside a `ScrollRect`
- [ ] `rowPrefab` is set to the `LeaderboardEntryRow` prefab from Ticket 01
- [ ] Calling `LeaderboardUI.Open()` shows the panel and populates Tab 0 rows
- [ ] Calling `LeaderboardUI.Close()` hides the panel
- [ ] Clicking a tab button populates that tab's rows and clears the previous rows
- [ ] All existing `LeaderboardUITests` still pass

## Blocked by

- 01 — `LeaderboardEntryRow` prefab

**Status:** ready-for-agent
