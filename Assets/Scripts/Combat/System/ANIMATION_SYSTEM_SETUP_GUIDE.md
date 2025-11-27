# Combatant Animation System - Setup Guide

## Overview

This guide explains how to set up and use the animation system for your turn-based combat game. The system is now fully integrated and ready to use with your existing combatants.

## System Components

### 1. **CombatantAnimState Enum**

Defines all available animation states:

- `Idle` - Default standing/waiting
- `Attack` - Physical attack animation
- `Hit` - Taking damage/hurt animation
- `Dead` - Death/defeat animation
- `Victory` - Winning/celebration animation
- `Cast` - Magic/spell casting animation
- `Defend` - Blocking/defensive animation

### 2. **CombatantAnimationController**

The component that manages animation state transitions. Features:

- Cached animator parameter hashes for performance
- Automatic animator component detection
- Safe null-checking (combat works even if animator missing)
- Animation event callback support
- Both specific methods (`PlayAttack()`) and generic method (`SetState()`)

### 3. **CombatantView Integration**

Your base combatant class now includes:

- Animation controller reference
- Automatic component finding in `Awake()`
- `PlayAnimation(CombatantAnimState)` public method
- `PlayIdleAnimation()` convenience method
- Automatic hit animation on damage
- Automatic death animation on defeat

## Quick Start Setup

### Step 1: Add Components to Your Combatant Prefab

1. Open your combatant prefab (Hero or Enemy)
2. Add `CombatantAnimationController` component if not already present
3. The animator should already be on the GameObject (visible in your screenshot)
4. The `CombatantAnimationController` will automatically find the Animator in `Awake()`

### Step 2: Configure Your Animator Controller

Your current controller is at: `Assets/Animations/Combat/Heroes/BaganiAnimation_0.controller`

#### Required Animator Parameters:

Add these **Triggers** to your Animator Controller:

- `Idle` (Trigger)
- `Attack` (Trigger)
- `Hit` (Trigger)
- `Dead` (Trigger)
- `Victory` (Trigger) - Optional
- `Cast` (Trigger) - Optional
- `Defend` (Trigger) - Optional

#### Recommended State Machine Structure (Best Practice):

```
       [Entry]
         ↓
      [Idle State]
        /   |   \   \   \
 [Attack] [Hit] [Cast] [Defend]
   |        |      |      |
   ↓        ↓      ↓      ↓
 [Idle]  [Idle]  [Idle]  [Idle]

// Any State (global transition):
[Any State] ──[Dead trigger]──→ [Dead State] (final, no exit)
```

**Key Principles:**

- Only Idle can transition to Attack, Hit, Cast, or Defend.
- Attack/Hit/Cast/Defend always return to Idle after finishing.
- Any State can only transition to Dead (for instant interruption on death).
- No transitions out of Dead (final state).

**Why?**
This structure prevents bugs like attacking while dead or hit, and ensures only valid transitions occur. It is robust for turn-based combat and easy to extend.

### Step 3: Create Animation States

For each state in your Animator Controller:

1. **Idle State**

   - Animation: `bagani_idle.anim` (already created)
   - Loop: Yes
   - Default state: Yes

2. **Attack State**

   - Animation: Create `bagani_attack.anim`
   - Loop: No
   - Transition: Exit time → return to Idle
   - Duration: ~0.5-1.0 seconds

3. **Hit State**

   - Animation: Create `bagani_hit.anim`
   - Loop: No
   - Transition: Exit time → return to Idle
   - Duration: ~0.3-0.5 seconds

4. **Dead State**

   - Animation: Create `bagani_dead.anim`
   - Loop: No
   - Final state: Yes (no transition back)
   - Duration: ~1.0-2.0 seconds

5. **Cast State** (Optional)

   - Animation: Create `bagani_cast.anim`
   - Loop: No
   - Transition: Exit time → return to Idle

6. **Defend State** (Optional)
   - Animation: Create `bagani_defend.anim`
   - Loop: No
   - Transition: Exit time → return to Idle

### Step 4: Set Up Transitions

#### From Idle State:

- Add transitions from Idle to: Attack, Hit, Cast, Defend
- Condition: Respective trigger (Attack, Hit, Cast, Defend)
- Has Exit Time: No
- Transition Duration: 0.1 seconds

#### From Attack, Hit, Cast, Defend States:

- Add transition back to Idle
- Has Exit Time: Yes
- Exit Time: 0.95 (95% through animation)
- Transition Duration: 0.1-0.2 seconds

#### From Any State:

- Add transition to Dead only
- Condition: Dead trigger
- Has Exit Time: No
- Transition Duration: 0.1 seconds

#### Dead State:

- No transitions out (final state)

**Do NOT** add Any State transitions to Attack, Hit, Cast, or Defend. Only Dead should be globally interruptible.

## Usage Examples

### Automatic Animations (Already Implemented)

The following animations trigger automatically:

```csharp
// Hit animation plays automatically when damage is taken
// (already integrated in CombatantView.Damage())

// Death animation plays automatically when health reaches zero
// (already integrated in DamageSystem)
```

### Manual Animation Triggers

Trigger animations from your card effects, abilities, or AI:

```csharp
// Basic usage - trigger any state
heroView.PlayAnimation(CombatantAnimState.Attack);

// Specific methods for common states
heroView.PlayIdleAnimation();

// From card/ability scripts
public class FireballCard : MonoBehaviour
{
    public void OnCardPlayed()
    {
        // Play cast animation before dealing damage
        casterView.PlayAnimation(CombatantAnimState.Cast);

        // Deal damage (hit animation plays automatically on targets)
        // ...damage code...
    }
}

// From enemy AI
public class EnemyAI : MonoBehaviour
{
    private IEnumerator AttackSequence()
    {
        // Play attack animation
        enemyView.PlayAnimation(CombatantAnimState.Attack);

        // Wait for animation to reach impact frame
        yield return new WaitForSeconds(0.3f);

        // Deal damage
        // ...damage code...

        // Return to idle after action
        yield return new WaitForSeconds(0.5f);
        enemyView.PlayIdleAnimation();
    }
}
```

### Using Animation Events (Advanced)

For precise timing of damage or VFX:

1. Open your attack animation
2. Add an Animation Event at the impact frame
3. Function: `OnAttackImpact`
4. This calls back to CombatantAnimationController

```csharp
// Extend CombatantAnimationController to handle events
public void OnAttackImpact()
{
    // Trigger damage exactly when weapon connects
    // Trigger VFX/SFX at precise moment
}
```

## Multi-Hero/Enemy Support

The system is fully multi-character ready:

```csharp
// Each hero/enemy has their own animator and animation controller
foreach (var hero in HeroSystem.Instance.HeroViews)
{
    hero.PlayAnimation(CombatantAnimState.Victory);
}

// Different characters can use different Animator Controllers
// via AnimatorOverrideController for character-specific animations
```

## Testing Your Setup

### Test Checklist:

1. **Idle Animation**

   - [ ] Character plays idle animation when spawned
   - [ ] Idle loops smoothly

2. **Damage Flow**

   - [ ] Character plays hit animation when damaged (only from Idle)
   - [ ] Hit animation is brief and returns to idle
   - [ ] Screen shake works alongside animation

3. **Death Flow**

   - [ ] Character plays death animation at 0 HP (can interrupt any state)
   - [ ] Death animation plays completely before removal
   - [ ] Character doesn't return to idle after death

4. **Manual Triggers**
   - [ ] `PlayAnimation(CombatantAnimState.Attack)` works (only from Idle)
   - [ ] All state transitions work smoothly (no attack/hit while dead)
   - [ ] No animation errors in console

### Debug Tips:

```csharp
// Check if animation controller is assigned
if (combatantView.GetComponent<CombatantAnimationController>() == null)
{
    Debug.LogWarning("CombatantAnimationController missing!");
}

// Check animator parameters in runtime
Animator anim = combatantView.GetComponentInChildren<Animator>();
foreach (var param in anim.parameters)
{
    Debug.Log($"Parameter: {param.name}, Type: {param.type}");
}
```

## Performance Considerations

### What's Already Optimized:

- ✅ Animator parameter hashes are cached (no string allocations)
- ✅ Null checks prevent errors if animator missing
- ✅ Static hash fields shared across all instances
- ✅ Minimal per-frame overhead (only during transitions)

### Best Practices:

- Keep animation clips short for responsive combat
- Use sprite sheet animations (not transform animations)
- Minimize animator layers (1-2 maximum)
- Avoid complex blend trees for turn-based combat

## Extending the System

### Adding New Animation States:

1. Add to enum:

```csharp
public enum CombatantAnimState
{
    // ...existing states...
    Stunned,  // New state
}
```

2. Add cached hash:

```csharp
private static readonly int StunnedHash = Animator.StringToHash("Stunned");
```

3. Add to switch statement:

```csharp
case CombatantAnimState.Stunned:
    animator.SetTrigger(StunnedHash);
    break;
```

4. Add specific method (optional):

```csharp
public void PlayStunned()
{
    if (animator != null)
        animator.SetTrigger(StunnedHash);
}
```

5. Add animator parameter and states in Unity

### Character-Specific Animations:

Use `AnimatorOverrideController` to reuse the same state machine with different animations:

```csharp
// Create override controller in Unity or code
AnimatorOverrideController overrideController = new AnimatorOverrideController(baseController);
overrideController["bagani_idle"] = heroSpecificIdleClip;
overrideController["bagani_attack"] = heroSpecificAttackClip;
animator.runtimeAnimatorController = overrideController;
```

## Troubleshooting

### Animation Not Playing

- Check animator component is present
- Verify parameter names match exactly (case-sensitive)
- Check transitions are set up correctly (see state machine diagram above)
- Look for warnings in console

### Animation Stuck

- Check for missing "exit time" on transitions from Attack/Hit/Cast/Defend to Idle
- Verify transition conditions (only Idle can trigger Attack/Hit/Cast/Defend)
- Check for state machine loops

### Multiple Animations Playing or Animation Bugs

- Ensure only Idle transitions to Attack/Hit/Cast/Defend
- Only Dead is allowed as an Any State transition
- Check "Has Exit Time" settings
- Verify trigger parameters are being reset

## Next Steps

1. ✅ System is implemented and integrated
2. 🔲 Add animator parameters to your controller
3. 🔲 Create animation clips for each state
4. 🔲 Set up state machine transitions
5. 🔲 Test with your existing combatants
6. 🔲 Add character-specific animations via override controllers
7. 🔲 Polish with animation events for precise timing

## Additional Resources

- Unity Animator Documentation: https://docs.unity3d.com/Manual/class-AnimatorController.html
- Animator Parameters: https://docs.unity3d.com/Manual/AnimationParameters.html
- Animation Events: https://docs.unity3d.com/Manual/script-AnimationWindowEvent.html

## Support

Your animation system is now:

- ✅ Fully implemented
- ✅ Integrated with combat flow
- ✅ Multi-character ready
- ✅ Performance optimized
- ✅ Extensible and maintainable

The hit and death animations already trigger automatically through your damage system. You just need to create the animation clips and set up the animator parameters!
