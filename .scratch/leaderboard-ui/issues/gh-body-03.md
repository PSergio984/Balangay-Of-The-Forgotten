## What to build

Create the `NameEntryPanel` Unity prefab — a modal popup that appears after every map victory, letting the player type their name (max 20 chars) and press Submit or Skip. Attach the `NameEntryUI` component and wire `panelRoot`, `nameInputField`, `submitButton`, and `skipButton`. The panel root is hidden (`SetActive(false)`) by default. Save to `Assets/Prefabs/Leaderboard/NameEntryPanel.prefab`. This prefab is placed in the Combat scene Canvas and referenced by `VictoryDefeatUI.nameEntryUI`.

## Acceptance criteria

- [ ] Prefab exists at `Assets/Prefabs/Leaderboard/NameEntryPanel.prefab`
- [ ] Root `NameEntryPanel` GameObject is inactive by default
- [ ] Inner modal has: title `TMP_Text`, a `TMP_InputField` with `characterLimit = 20`, a Submit `Button`, a Skip `Button`
- [ ] `NameEntryUI` component attached; all four serialized fields wired (no Missing References)
- [ ] Calling `Show(mapId, clearTime, callback)` makes the panel visible and focuses the input field
- [ ] Pressing Submit calls `LeaderboardManager.Instance.SubmitEntry` then invokes callback
- [ ] Pressing Skip submits with name `"Anonymous"` then invokes callback
- [ ] All existing `NameEntryUITests` still pass

## Blocked by

None — can start immediately.
