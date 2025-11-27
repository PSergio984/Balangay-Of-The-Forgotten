# Animation System Quick Reference Card

## 🎯 Quick Start (3 Steps)

### 1️⃣ Add Animator Parameters

Open your Animator Controller and add these **Triggers**:

```
Idle, Attack, Hit, Dead, Victory, Cast, Defend
```

### 2️⃣ Create Animation States

Create states in Animator with your animation clips:

```
bagani_idle, bagani_attack, bagani_hit, bagani_dead
```

### 3️⃣ Set Up Transitions

- Any State → action states (on trigger, no exit time)
- Action states → Idle (with exit time 0.95)

**Done!** Animations will play automatically during combat.

---

## 📖 Code Reference

### Enum Values

```csharp
CombatantAnimState.Idle
CombatantAnimState.Attack
CombatantAnimState.Hit      // Automatic
CombatantAnimState.Dead     // Automatic
CombatantAnimState.Victory
CombatantAnimState.Cast
CombatantAnimState.Defend
```

### Trigger Animation

```csharp
// Generic method (any state)
combatantView.PlayAnimation(CombatantAnimState.Attack);

// Convenience method
combatantView.PlayIdleAnimation();

// Direct access to controller
animController.PlayAttack();
animController.PlayHit();
animController.PlayDead();
```

### In Card/Ability

```csharp
public void OnCardPlayed()
{
    // Play animation
    caster.PlayAnimation(CombatantAnimState.Attack);

    // Deal damage (hit animation automatic)
    var damage = new DealDamageGA(5, targets, caster);
    ActionSystem.Instance.Perform(damage);
}
```

### With Timing

```csharp
private IEnumerator AttackSequence()
{
    caster.PlayAnimation(CombatantAnimState.Attack);
    yield return new WaitForSeconds(0.3f);

    // Deal damage
    var damage = new DealDamageGA(5, targets, caster);
    ActionSystem.Instance.Perform(damage);

    yield return new WaitForSeconds(0.3f);
    caster.PlayIdleAnimation();
}
```

---

## 🔧 Animator Setup Quick Guide

### Parameters (All Triggers):

| Name    | Type    | Required |
| ------- | ------- | -------- |
| Idle    | Trigger | ✅       |
| Attack  | Trigger | ✅       |
| Hit     | Trigger | ✅       |
| Dead    | Trigger | ✅       |
| Victory | Trigger | Optional |
| Cast    | Trigger | Optional |
| Defend  | Trigger | Optional |

### Transitions:

```
Any State → Attack (trigger: Attack, no exit time)
Any State → Hit (trigger: Hit, no exit time)
Any State → Dead (trigger: Dead, no exit time)
Any State → Cast (trigger: Cast, no exit time)

Attack → Idle (exit time: 0.95)
Hit → Idle (exit time: 0.95)
Cast → Idle (exit time: 0.95)

Dead → (none, final state)
```

### State Settings:

- **Idle**: Loop ✅, Default ✅
- **Attack**: Loop ❌, Duration ~0.5s
- **Hit**: Loop ❌, Duration ~0.3s
- **Dead**: Loop ❌, No exit transition
- **Cast**: Loop ❌, Duration ~0.6s

---

## ✅ What's Already Working

### Automatic (No Code Needed):

- ✅ Hit animation plays when taking damage
- ✅ Death animation plays at 0 HP
- ✅ Component auto-finds animator
- ✅ Safe if animator missing

### Manual (Your Code):

- 🎮 Attack/Cast animations for abilities
- 🎮 Victory animations for win screen
- 🎮 Idle returns after actions
- 🎮 Timed sequences with coroutines

---

## 🎨 Animation Timing Guide

### Recommended Durations:

- **Idle**: Loop (1-2s per cycle)
- **Attack**: 0.4-0.6s
- **Hit**: 0.2-0.4s (brief!)
- **Dead**: 0.8-1.5s
- **Cast**: 0.5-0.8s
- **Defend**: 0.3-0.5s
- **Victory**: 1.0-2.0s

### Transition Timing:

- Exit time: **0.90-0.95** (return to idle)
- Duration: **0.1-0.2s** (smooth blend)
- Impact frame: **40-60%** of animation

---

## 🚨 Common Issues

### Animation doesn't play

✓ Check parameter spelling (case-sensitive!)
✓ Verify transition exists
✓ Confirm animator assigned

### Gets stuck in animation

✓ Add exit time transition to Idle
✓ Check exit time value (0.95)
✓ Call PlayIdleAnimation() after action

### Plays wrong animation

✓ Check trigger name matches
✓ Verify transition conditions
✓ Check Any State priority

---

## 📊 System Status

**Implementation**: ✅ Complete  
**Integration**: ✅ Done  
**Documentation**: ✅ Comprehensive  
**Testing**: ⏳ Your Turn!

---

## 📁 File Locations

**Code**:

- `Combat/System/CombatantAnimationController.cs`
- `Combat/Views/CombatantView.cs`
- `Combat/System/DamageSystem.cs`

**Docs**:

- `Combat/System/ANIMATION_SYSTEM_SETUP_GUIDE.md`
- `Combat/System/ANIMATOR_CONTROLLER_VISUAL_GUIDE.md`
- `Combat/System/ANIMATION_INTEGRATION_EXAMPLES.md`
- `Combat/System/ANIMATION_SYSTEM_SUMMARY.md`

**Your Assets**:

- `Animations/Combat/Heroes/BaganiAnimation_0.controller`
- `Animations/Combat/Heroes/Bagani/bagani_idle.anim`

---

## 🎓 Learning Path

1. **5 min**: Add parameters to Animator
2. **15 min**: Read ANIMATOR_CONTROLLER_VISUAL_GUIDE.md
3. **30 min**: Create animation clips
4. **15 min**: Set up transitions
5. **10 min**: Test in play mode
6. **30 min**: Read ANIMATION_INTEGRATION_EXAMPLES.md
7. **1 hour**: Add animations to your cards/abilities

**Total**: ~2-3 hours to full implementation

---

## 💡 Pro Tips

1. Start with Idle + Hit, test thoroughly
2. Keep animations SHORT for responsive combat
3. Use AnimatorOverrideController for multiple characters
4. Test one state at a time
5. Polish animations last

---

## 🆘 Help

**Questions?** Check comprehensive guides:

- Setup: `ANIMATION_SYSTEM_SETUP_GUIDE.md`
- Unity: `ANIMATOR_CONTROLLER_VISUAL_GUIDE.md`
- Code: `ANIMATION_INTEGRATION_EXAMPLES.md`

**Still stuck?** Check console for warnings from CombatantAnimationController - it tells you what's missing!

---

**Quick Start → 3 steps → Working animations!** 🎮✨

Print this card and keep it handy while setting up your animations!
