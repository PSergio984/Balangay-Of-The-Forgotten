# TICKET-008 — Access Points (Leaderboard Buttons in Scenes)

**Status:** ready-for-agent  
**Blocks:** nothing — final ticket  
**Blocked by:** TICKET-007 (LeaderboardUI must exist and be functional)

---

## Goal

Wire a **Leaderboard** button in both the **Main Menu scene** and the **Map Selection scene** so players can open the leaderboard overlay from either screen.

---

## Scenes to modify

### Main Menu (`Assets/Scenes/MainMenu.unity`)

1. Add a `Button` (UI) to the Main Menu canvas. Label it "Leaderboard".
2. Add `LeaderboardUI` prefab (or component) to the scene if it isn't already present.
3. Wire the button's `onClick` → `LeaderboardUI.Open()`.
4. The Leaderboard panel overlay must sit in front of all other UI (high Canvas `sortingOrder` or a sibling at the bottom of the hierarchy so it renders on top).

### Map Selection (`Assets/Scenes/MapSelection.unity`)

1. Same as above — add a "Leaderboard" button to the Map Selection canvas.
2. Wire to the same `LeaderboardUI` component in that scene.
3. The leaderboard overlay must NOT interfere with the map selection UI when closed.

---

## Approach options (pick one, note which you chose)

**Option A — Shared prefab:** Create `Assets/Prefabs/Leaderboard/LeaderboardOverlay.prefab` containing the full `LeaderboardUI` hierarchy. Drop the prefab into both scenes. Both scenes get independent instances backed by the same `LeaderboardManager` singleton (which persists across scenes via `DontDestroyOnLoad` or lives in the Core scene).

**Option B — Per-scene duplicate:** Build the panel hierarchy once in Main Menu, duplicate into Map Selection. Easier to tweak per scene but two copies to maintain.

> Recommendation: **Option A** — prefab is cleaner for this project's pattern.

---

## Constraints

- Do NOT add the Leaderboard button inside any existing panel that has its own show/hide logic — add it as a sibling at the top level of the canvas.
- The button must remain accessible at all times while the scene is active (not gated behind a progress check).
- No scene transition when clicking the button.

---

## Done when

- [ ] Main Menu scene has a Leaderboard button.
- [ ] Map Selection scene has a Leaderboard button.
- [ ] Clicking the button in either scene opens the `LeaderboardUI` overlay panel.
- [ ] Closing the panel returns to the normal scene UI (no scene reload).
- [ ] Both buttons work after returning from a combat run (i.e. panel state is clean on re-open).
