## What to build

Complete the combat-side leaderboard integration. Verify the `CombatTimer` singleton GameObject exists in the Combat scene. Instantiate the `NameEntryPanel` prefab as a child of the Combat scene Canvas and assign it to the `nameEntryUI` serialized field on `VictoryDefeatUI`. After this ticket, every map victory shows the name entry popup before the continue button appears, and the recorded clear time flows from `CombatTimer` → `LevelTransitionData.ClearTimeSeconds` → `NameEntryUI.Show()` → `LeaderboardManager.SubmitEntry()`. Satisfies spec R5, R1, AC-1, AC-2, AC-10.

## Acceptance criteria

- [ ] `CombatTimer` singleton GameObject is present in the Combat scene
- [ ] `NameEntryPanel` prefab instance is a child of the Combat Canvas
- [ ] `VictoryDefeatUI.nameEntryUI` field references the `NameEntryPanel` instance (no Missing Reference)
- [ ] On victory: name entry popup appears before the Continue button (AC-1)
- [ ] Pressing Skip records "Anonymous" with the correct clear time (AC-2)
- [ ] Pressing Submit with a valid name records that name with the correct clear time (AC-3)
- [ ] All existing `VictoryDefeatIntegrationTests` and `CombatTimerTests` still pass

## Blocked by

- feat(leaderboard): create NameEntryPanel prefab
