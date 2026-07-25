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

**In victory trigger (`ShowVictory` / `ShowVictoryWithReward`):**
Immediately stop and record the timer via `RecordCombatClearTime()` before banner animations or delays begin:
```csharp
if (CombatTimer.Instance != null && CombatTimer.Instance.IsRunning)
{
    CombatTimer.Instance.StopTimer();
    if (levelTransitionData != null)
    {
        levelTransitionData.ClearTimeSeconds = CombatTimer.Instance.ElapsedSeconds;
    }
}
```

**In banner completion path (`AnimateBanner`):**
If `nameEntryUI != null`, display `nameEntryUI.Show(...)` with `EnableContinueButtonRoutine()` as completion callback; otherwise log `Debug.LogWarning` and enable Continue button directly.

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
