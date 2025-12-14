# Animator Controller Setup - Visual Guide

## Quick Setup for BaganiAnimation_0 Controller

This guide shows exactly how to configure your existing Animator Controller for the animation system.

---

## Part 1: Add Animator Parameters

### Step-by-Step:

1. **Open Animator Window**

   - Select your controller: `Assets/Animations/Combat/Heroes/BaganiAnimation_0.controller`
   - Window → Animation → Animator

2. **Add Parameters** (Left panel in Animator window)

   Click the `+` button and add these **Triggers**:

   | Parameter Name | Type    | Required |
   | -------------- | ------- | -------- |
   | Idle           | Trigger | Yes      |
   | Attack         | Trigger | Yes      |
   | Hit            | Trigger | Yes      |
   | Dead           | Trigger | Yes      |
   | Victory        | Trigger | Optional |
   | Cast           | Trigger | Optional |
   | Defend         | Trigger | Optional |

   **Important:**

   - Parameter names are case-sensitive!
   - Must be exact matches
   - All should be **Triggers**, not Bools or Floats

---

## Part 2: Create Animation States

### Current State:

You already have:

- ✅ `bagani_idle` state with animation

### States to Add:

Right-click in the grid area → Create State → Empty

#### State 1: Attack

- Name: `bagani_attack`
- Motion: Create new animation clip `bagani_attack.anim`
- Speed: 1.0
- Loop Time: Unchecked

#### State 2: Hit

- Name: `bagani_hit`
- Motion: Create new animation clip `bagani_hit.anim`
- Speed: 1.5 (faster = snappier)
- Loop Time: Unchecked

#### State 3: Dead

- Name: `bagani_dead`
- Motion: Create new animation clip `bagani_dead.anim`
- Speed: 1.0
- Loop Time: Unchecked

#### State 4: Cast (Optional)

- Name: `bagani_cast`
- Motion: Create new animation clip `bagani_cast.anim`
- Speed: 1.0
- Loop Time: Unchecked

---

## Part 3: Set Up Transitions

### A. From "Any State" to Action States

For each action state (Attack, Hit, Dead, Cast):

1. **Right-click "Any State"** → Make Transition → Select target state

2. **Configure transition** (Inspector):

   ```
   Has Exit Time: ☐ Unchecked
   Fixed Duration: ☑ Checked
   Transition Duration: 0.1
   Interruption Source: Current State

   Conditions:
   + Add Condition → [StateName] (e.g., Attack)
   ```

### B. From Action States Back to Idle

For each action state (except Dead):

1. **Right-click action state** → Make Transition → Select `bagani_idle`

2. **Configure transition** (Inspector):

   ```
   Has Exit Time: ☑ Checked
   Exit Time: 0.95
   Fixed Duration: ☑ Checked
   Transition Duration: 0.15

   Conditions: (leave empty)
   ```

### C. Dead State (Special Case)

**Do NOT add transition from Dead state back to Idle**

- Dead is a final state
- Character should stay in dead animation

---

## Part 4: Create Animation Clips

### If you haven't created the animation clips yet:

1. **Create new Animation Clip:**

   - Right-click in Project → Create → Animation
   - Name it (e.g., `bagani_attack.anim`)
   - Place in same folder as `bagani_idle.anim`

2. **Edit the Animation:**
   - Select the animation clip
   - Window → Animation → Animation
   - Add sprite frames from your sprite sheet
   - Adjust timing/speed

### Example Sprite Animation Setup:

```
bagani_attack.anim:
Frame 0 (0:00): attack_frame_1
Frame 2 (0:08): attack_frame_2
Frame 4 (0:16): attack_frame_3 (impact frame)
Frame 6 (0:25): attack_frame_4
Frame 8 (0:33): return to idle pose

Total duration: ~0.5 seconds
Sample rate: 12 FPS
```

---

## Part 5: Verification Checklist

### ✅ Parameters Added

- [ ] All 7 parameters added as Triggers
- [ ] Names match exactly (case-sensitive)
- [ ] No typos

### ✅ States Created

- [ ] bagani_idle (exists)
- [ ] bagani_attack (new)
- [ ] bagani_hit (new)
- [ ] bagani_dead (new)
- [ ] Each has animation clip assigned

### ✅ Transitions Set Up

- [ ] Any State → Attack (trigger: Attack)
- [ ] Any State → Hit (trigger: Hit)
- [ ] Any State → Dead (trigger: Dead)
- [ ] Any State → Cast (trigger: Cast, optional)
- [ ] Attack → Idle (exit time: 0.95)
- [ ] Hit → Idle (exit time: 0.95)
- [ ] Cast → Idle (exit time: 0.95)
- [ ] Dead has NO transition back

### ✅ Default State

- [ ] Entry arrow points to `bagani_idle`
- [ ] Idle state is orange (default)

---

## Part 6: Test in Play Mode

### Testing Script (Optional):

Create a test script to manually trigger animations:

```csharp
using UnityEngine;

public class AnimationTester : MonoBehaviour
{
    [SerializeField] private CombatantView combatantView;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            combatantView.PlayAnimation(CombatantAnimState.Idle);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            combatantView.PlayAnimation(CombatantAnimState.Attack);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            combatantView.PlayAnimation(CombatantAnimState.Hit);

        if (Input.GetKeyDown(KeyCode.Alpha4))
            combatantView.PlayAnimation(CombatantAnimState.Dead);

        if (Input.GetKeyDown(KeyCode.Alpha5))
            combatantView.PlayAnimation(CombatantAnimState.Cast);
    }
}
```

**Test Keys:**

- 1 = Idle
- 2 = Attack
- 3 = Hit
- 4 = Dead
- 5 = Cast

### What to Check:

- [ ] Idle plays on start
- [ ] Attack plays and returns to idle
- [ ] Hit plays and returns to idle
- [ ] Dead plays and stays in final pose
- [ ] No console errors
- [ ] Smooth transitions

---

## Part 7: Apply to Other Heroes/Enemies

### Option A: Duplicate & Modify

1. Duplicate `BaganiAnimation_0.controller`
2. Rename to character name
3. Replace animation clips with character-specific clips
4. Parameters and structure stay the same

### Option B: Use Animator Override Controller (Recommended)

1. Right-click → Create → Animator Override Controller
2. Name it (e.g., `HeroX_Animations`)
3. Select it and set Controller to `BaganiAnimation_0`
4. Replace clips in the list:
   - bagani_idle → heroX_idle
   - bagani_attack → heroX_attack
   - etc.
5. Assign override controller to character's Animator

**Benefits:**

- All characters share same state machine logic
- Easy to maintain (fix once, applies to all)
- Only animation clips differ

---

## Common Issues & Fixes

### Issue: Animation doesn't play

**Fix:**

- Check parameter name spelling
- Verify transition exists
- Check Animator component has controller assigned

### Issue: Animation plays but gets stuck

**Fix:**

- Add exit time transition back to Idle
- Check exit time value (should be 0.90-0.95)
- Ensure transition duration is reasonable (0.1-0.2)

### Issue: Animation plays twice

**Fix:**

- Check for duplicate transitions
- Verify "Has Exit Time" is unchecked on Any State transitions

### Issue: Wrong animation plays

**Fix:**

- Check parameter names match exactly
- Verify you're triggering correct state
- Check transition conditions

---

## Visual Reference: Animator Window Layout

```
┌─────────────────────────────────────────────────────────┐
│  Animator: BaganiAnimation_0                            │
├──────────────┬──────────────────────────────────────────┤
│ Parameters   │  State Machine Grid                      │
│              │                                           │
│ + Idle       │    [Entry] ──→ [bagani_idle] ←──┐       │
│ + Attack     │                     │            │       │
│ + Hit        │              ┌──────┴────────┐   │       │
│ + Dead       │              ↓      ↓        ↓   │       │
│ + Cast       │        [Attack] [Hit]   [Dead]   │       │
│ + Victory    │              │      │             │       │
│ + Defend     │              └──────┴─────────────┘       │
│              │                                           │
│              │    [Any State]                            │
│              │         │                                 │
│              │    (transitions to all action states)     │
└──────────────┴──────────────────────────────────────────┘
```

---

## Summary: What You're Setting Up

The Animator Controller acts as a **state machine** for your character animations:

1. **Idle** is the default/waiting state
2. **Triggers** cause transitions to action states
3. **Action states** play their animation
4. **Exit time** returns to Idle when animation finishes
5. **Dead** is a final state with no return

This setup works for ALL characters - just swap the animation clips!

---

## Next Steps After Setup

1. ✅ Configure animator parameters (Part 1)
2. ✅ Create animation states (Part 2)
3. ✅ Set up transitions (Part 3)
4. ✅ Create/assign animation clips (Part 4)
5. ✅ Test in play mode (Part 6)
6. 🔲 Apply to all characters (Part 7)
7. 🔲 Polish animations
8. 🔲 Add animation events for precise VFX/SFX timing

You're done when all checklist items are complete and animations play correctly in combat!
