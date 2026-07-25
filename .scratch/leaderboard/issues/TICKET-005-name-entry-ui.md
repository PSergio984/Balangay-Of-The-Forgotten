# TICKET-005 — NameEntryUI (Victory Screen Popup)

**Status:** completed  
**Blocks:** TICKET-006 (VictoryDefeatUI integration)  
**Blocked by:** TICKET-004 (LeaderboardManager must exist to call Submit)

---

## Goal

Build the name-entry popup that appears on the Victory screen. It is a self-contained UI component: it receives the clear time, prompts the player for a name, then calls `LeaderboardManager.Instance.SubmitEntry()` and fires a callback so `VictoryDefeatUI` can continue.

---

## Files to create

### `Assets/Scripts/Leaderboard/NameEntryUI.cs`

```
public class NameEntryUI : MonoBehaviour
{
    [SerializeField] private GameObject        panelRoot;      // root panel to show/hide
    [SerializeField] private TMP_InputField    nameInputField; // player name input
    [SerializeField] private Button            submitButton;
    [SerializeField] private Button            skipButton;

    // Show the popup.
    // mapId: the map just cleared (from LevelTransitionData.SelectedMapData.MapId)
    // clearTimeSeconds: from LevelTransitionData.ClearTimeSeconds
    // onComplete: called after entry is saved (whether submitted or skipped)
    public void Show(string mapId, float clearTimeSeconds, System.Action onComplete)

    // Hides the panel
    public void Hide()
}
```

---

## Behaviour

### `Show(mapId, clearTimeSeconds, onComplete)`
1. Set `panelRoot.SetActive(true)`.
2. Clear the `nameInputField` text.
3. Enforce max character limit: `nameInputField.characterLimit = 20`.
4. Register `submitButton.onClick` → `OnSubmit()` and `skipButton.onClick` → `OnSkip()` once during `Awake()` (or clear existing listeners before wiring in `Show()`).

### `OnSubmit()`
1. Take `nameInputField.text.Trim()`. If empty → use `"Anonymous"`.
2. Call `LeaderboardManager.Instance.SubmitEntry(name, _mapId, _clearTime)`.
3. `Hide()`, then invoke `_onComplete`.

### `OnSkip()`
1. Call `LeaderboardManager.Instance.SubmitEntry("Anonymous", _mapId, _clearTime)`.
2. `Hide()`, then invoke `_onComplete`.

---

## Constraints

- The panel must be **invisible by default** (`panelRoot.SetActive(false)` in Awake or via Inspector).
- Do not use `DontDestroyOnLoad` — this lives in the Combat scene only.
- Use `TMP_InputField` (TextMesh Pro) consistent with the rest of the project.
- Do not navigate scenes — just call `onComplete` and let `VictoryDefeatUI` handle what comes next.

---

## Done when

- [x] `NameEntryUI` compiles.
- [x] Panel hidden on scene load.
- [x] `Show()` makes panel visible, clears prior input.
- [x] Submit with non-empty name → `LeaderboardManager.Instance.SubmitEntry` called with trimmed name.
- [x] Submit with empty name → `SubmitEntry` called with `"Anonymous"`.
- [x] Skip → `SubmitEntry` called with `"Anonymous"`.
- [x] `onComplete` is called in both Submit and Skip paths.
