# TICKET-007 — LeaderboardUI Panel + Row Prefab

**Status:** ready-for-agent  
**Blocks:** TICKET-008 (Access points / wiring buttons)  
**Blocked by:** TICKET-004 (LeaderboardManager must exist to query data)

---

## Goal

Build the leaderboard overlay panel with 5 tabs (4 maps + Overall) and a scrollable list of ranked rows. This is a UI-only ticket — no game-flow changes.

---

## Files to create

### `Assets/Scripts/Leaderboard/LeaderboardEntryRowUI.cs`

Single-row prefab controller. Displays rank, name, and formatted clear time.

```
public class LeaderboardEntryRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;      // "#1", "#2", ...
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text clearTimeText; // "1:34", "12:05"

    public void Populate(int rank, string playerName, float clearTimeSeconds)
    // Format: rank → "#N", clearTimeSeconds → "m:ss"
}
```

**Time format helper** (private static):
```
// 94.3s  → "1:34"
// 725.0s → "12:05"
static string FormatTime(float seconds)
{
    int m = (int)(seconds / 60);
    int s = (int)(seconds % 60);
    return $"{m}:{s:D2}";
}
```

---

### `Assets/Scripts/Leaderboard/LeaderboardUI.cs`

Overlay panel controller. Manages tabs, clears and repopulates rows on tab switch.

```
public class LeaderboardUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button     closeButton;

    [Header("Tabs")]
    // One button per tab, in order: Dagat, Daragang, Bundok, Kaluwalhatian, Overall
    [SerializeField] private Button[] tabButtons;   // length 5
    [SerializeField] private string[] tabMapIds;    // length 5; last entry = "Overall"

    [Header("Row List")]
    [SerializeField] private Transform         rowContainer;   // parent for row instances
    [SerializeField] private LeaderboardEntryRowUI rowPrefab;

    // --- Public API ---
    public void Open()                    // called by Main Menu / Map Select buttons
    public void Close()                   // called by close button
    private void ShowTab(int tabIndex)    // clears rows, repopulates from LeaderboardManager
}
```

**`ShowTab(int tabIndex)` logic:**
1. Destroy all existing children of `rowContainer`.
2. If `tabIndex < 4` (per-map tab):
   - Call `LeaderboardManager.Instance.GetTopEntriesForMap(tabMapIds[tabIndex])`.
   - For each entry (index 0–9): instantiate `rowPrefab`, call `Populate(rank, name, time)`.
3. If `tabIndex == 4` (Overall):
   - Call `LeaderboardManager.Instance.GetOverallTopEntries()`.
   - For each `OverallLeaderboardEntry`: instantiate `rowPrefab`, call `Populate(rank, name, totalTime)`.
4. Highlight the active tab button (e.g. slightly different color via `ColorBlock`).

**`Open()` logic:**
1. `panelRoot.SetActive(true)`.
2. Reload data (call `LeaderboardManager.Instance` — it already has latest data).
3. Default to tab 0 (first map).

**`Close()`:**
1. `panelRoot.SetActive(false)`.

---

## Scene setup guidance (for implementer)

- Create a UI Canvas child panel as the leaderboard overlay in both Main Menu and Map Selection scenes, OR make `LeaderboardUI` a prefab that both scenes share.
- `panelRoot` is `SetActive(false)` by default.
- The 5 tab buttons and `tabMapIds` string array must be assigned in the Inspector in the same order:
  `"Dagat Ng Kabisayaan"`, `"Daragang Magayon"`, `"Bundok Pulag"`, `"Kaluwalhatian"`, `"Overall"`

---

## Constraints

- No scrollview required if ≤10 rows fit comfortably; use a simple `VerticalLayoutGroup` with `Content Size Fitter`.
- Use `TMP_Text` (TextMesh Pro) for all text — consistent with the rest of the project.
- Do not hard-code any map names as string literals in `LeaderboardUI.cs` — they come from the `tabMapIds` Inspector array.

---

## Done when

- [ ] `LeaderboardUI` and `LeaderboardEntryRowUI` compile.
- [ ] Panel hidden on scene load.
- [ ] `Open()` → panel visible, tab 0 selected, rows populated from `LeaderboardManager`.
- [ ] Switching tabs → rows update correctly.
- [ ] Overall tab: shows only players with all 4 maps; displays summed personal best.
- [ ] `Close()` → panel hidden.
- [ ] Time format: 94.3s displays as "1:34", 725.0s displays as "12:05".
