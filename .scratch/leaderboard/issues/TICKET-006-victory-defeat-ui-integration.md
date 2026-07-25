# TICKET-006 — VictoryDefeatUI Integration

**Status:** ready-for-agent  
**Blocks:** nothing (last combat-side ticket)  
**Blocked by:** TICKET-003 (ClearTimeSeconds in LevelTransitionData), TICKET-005 (NameEntryUI component)

---

## Goal

Wire the Victory flow in `VictoryDefeatUI` so that:
1. When victory is triggered, the combat timer stops and its value is stored.
2. The `NameEntryUI` popup is shown before the Continue button appears.
3. The Continue button only becomes available after the player submits or skips the name entry.

---

## Files to modify

### `Assets/Scripts/Combat/UI/VictoryDefeatUI.cs`

**Add serialised field:**
```csharp
[Header("Leaderboard")]
[Tooltip("Name entry popup — shown after victory, before continue button")]
[SerializeField] private NameEntryUI nameEntryUI;
```

**In `ShowVictory()` (or wherever the victory banner coroutine completes and the Continue button would be enabled):**

Replace the line that enables the Continue button with:

```csharp
// 1. Stop and record the timer
levelTransitionData.ClearTimeSeconds = CombatTimer.Instance.ElapsedSeconds;
CombatTimer.Instance.StopTimer();

// 2. Show name entry; only show Continue button in the callback
nameEntryUI.Show(
    mapId: levelTransitionData.SelectedMapData.MapId,
    clearTimeSeconds: levelTransitionData.ClearTimeSeconds,
    onComplete: () => EnableContinueButton()
);
```

Where `EnableContinueButton()` is the existing logic (or a new private method) that makes the Continue button interactable and visible.

---

## Read first

Before making changes, read `VictoryDefeatUI.cs` fully to:
- Find the exact method and line where the Continue button is currently enabled after victory animation.
- Confirm `levelTransitionData` is already a serialised field (it is — line 78).
- Understand the existing coroutine flow so the name entry popup slots in correctly.

---

## Constraints

- Do **not** refactor unrelated parts of `VictoryDefeatUI`.
- The defeat path (`ShowDefeat`) must not be touched — name entry is victory-only.
- If `nameEntryUI` is null in the Inspector, log a warning and enable Continue button directly (safe fallback).
- The Continue button must NOT be interactable before `onComplete` fires.

---

## Done when

- [ ] Winning a combat → `NameEntryUI` popup appears before Continue button is shown.
- [ ] After Submit or Skip → Continue button becomes available.
- [ ] `LevelTransitionData.ClearTimeSeconds` holds the correct elapsed time at the moment of victory.
- [ ] Defeat path is unchanged.
