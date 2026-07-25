# TICKET-003 — Combat Timer + LevelTransitionData field

**Status:** ready-for-agent  
**Blocks:** TICKET-006 (VictoryDefeatUI integration)  
**Blocked by:** nothing — can be worked in parallel with TICKET-001

---

## Goal

Track elapsed combat time and carry it to the Victory screen via the existing `LevelTransitionData` ScriptableObject mailbox.

---

## Files to create / modify

### [CREATE] `Assets/Scripts/Combat/System/CombatTimer.cs`

```
public class CombatTimer : MonoBehaviour
{
    // Public read-only elapsed seconds
    public float ElapsedSeconds { get; private set; }
    public bool  IsRunning      { get; private set; }

    public void StartTimer()  // call when combat begins
    public void StopTimer()   // call when victory condition is met
    public void ResetTimer()  // optional: call on scene unload / new match

    // Update() increments ElapsedSeconds while IsRunning == true
}
```

Attach this to a persistent GameObject in the **Combat scene** (same one that hosts `MatchSetupSystem` or similar).

### [MODIFY] `Assets/Scripts/SceneController/LevelTransitionData.cs`

Add one field alongside the existing `SelectedMapData`:

```csharp
[Header("Runtime Data (Set by Victory)")]
[Tooltip("Elapsed seconds of the winning combat run — set by CombatTimer on victory")]
public float ClearTimeSeconds;
```

Also extend the existing `Clear()` method to reset `ClearTimeSeconds = 0f`.

---

## Integration point

Whoever currently calls the victory flow (e.g. `VictoryDefeatUI.ShowVictory()`, or a game-end event) must call:

```csharp
levelTransitionData.ClearTimeSeconds = CombatTimer.Instance.ElapsedSeconds;
CombatTimer.Instance.StopTimer();
```

> Note: Identifying the exact call site is part of this ticket. Read `VictoryDefeatUI.cs` and `EnemySystem.cs` to find where the final enemy death triggers victory, then insert the two lines above at that point.

---

## Constraints

- `CombatTimer` must be a `Singleton<CombatTimer>` (the project already has a `Singleton<T>` base class at `Assets/Scripts/Combat/General/Singleton.cs`).
- Timer uses `Time.deltaTime` in `Update()` — no coroutines.
- The timer must survive scene reloads within the Combat scene but be reset on a fresh match start.

---

## Done when

- [ ] `CombatTimer` compiles and attaches to the Combat scene.
- [ ] `LevelTransitionData` has `ClearTimeSeconds` field and `Clear()` resets it.
- [ ] After winning a combat, `LevelTransitionData.ClearTimeSeconds` contains a non-zero value matching elapsed time within ±1s.
