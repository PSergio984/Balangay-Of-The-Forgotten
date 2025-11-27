# Animation Integration Examples

This document shows practical examples of integrating animations into your existing combat systems.

---

## Example 1: Card Effects with Animations

### Basic Attack Card

```csharp
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Example card that plays attack animation before dealing damage
/// </summary>
public class AttackCardExample : MonoBehaviour
{
    [SerializeField] private int damageAmount = 5;

    public void OnCardPlayed(CombatantView caster, List<CombatantView> targets)
    {
        // Play attack animation on the caster
        caster.PlayAnimation(CombatantAnimState.Attack);

        // Create and execute damage action
        // (Hit animations will play automatically on targets)
        DealDamageGA damageAction = new DealDamageGA(
            damageAmount,
            targets,
            caster
        );
        ActionSystem.Instance.Perform(damageAction);
    }
}
```

### Magic/Spell Card

```csharp
/// <summary>
/// Example magic card that uses cast animation
/// </summary>
public class FireballCardExample : MonoBehaviour
{
    [SerializeField] private int magicDamage = 8;
    [SerializeField] private GameObject fireballVFX;

    public void OnCardPlayed(CombatantView caster, List<CombatantView> targets)
    {
        // Play cast animation for magic attacks
        caster.PlayAnimation(CombatantAnimState.Cast);

        // Spawn VFX
        if (fireballVFX != null)
        {
            Instantiate(fireballVFX, caster.transform.position, Quaternion.identity);
        }

        // Deal damage (hit animations automatic)
        DealDamageGA damageAction = new DealDamageGA(
            magicDamage,
            targets,
            caster
        );
        ActionSystem.Instance.Perform(damageAction);
    }
}
```

### Defensive/Support Card

```csharp
/// <summary>
/// Example card that uses defend animation when applying armor
/// </summary>
public class DefendCardExample : MonoBehaviour
{
    [SerializeField] private int armorAmount = 5;

    public void OnCardPlayed(CombatantView caster)
    {
        // Play defend animation
        caster.PlayAnimation(CombatantAnimState.Defend);

        // Apply armor status effect
        AddStatusEffectGA armorAction = new AddStatusEffectGA(
            new List<CombatantView> { caster },
            StatusEffectType.Armor,
            armorAmount
        );
        ActionSystem.Instance.Perform(armorAction);
    }
}
```

---

## Example 2: Timed Animation with Coroutines

### Attack with Precise Timing

```csharp
using System.Collections;

/// <summary>
/// Advanced example: Wait for animation before dealing damage
/// </summary>
public class TimedAttackExample : MonoBehaviour
{
    [SerializeField] private float attackAnimationDuration = 0.5f;
    [SerializeField] private float damageTimingOffset = 0.3f; // When weapon hits

    public void OnCardPlayed(CombatantView caster, List<CombatantView> targets)
    {
        StartCoroutine(ExecuteTimedAttack(caster, targets));
    }

    private IEnumerator ExecuteTimedAttack(CombatantView caster, List<CombatantView> targets)
    {
        // Start attack animation
        caster.PlayAnimation(CombatantAnimState.Attack);

        // Wait for weapon to connect (impact frame)
        yield return new WaitForSeconds(damageTimingOffset);

        // Apply damage at precise moment
        DealDamageGA damageAction = new DealDamageGA(10, targets, caster);
        ActionSystem.Instance.Perform(damageAction);

        // Wait for animation to complete
        yield return new WaitForSeconds(attackAnimationDuration - damageTimingOffset);

        // Return to idle
        caster.PlayIdleAnimation();
    }
}
```

---

## Example 3: Enemy AI Integration

### Enemy Attack Pattern

```csharp
/// <summary>
/// Example enemy AI that uses animations
/// </summary>
public class EnemyAIExample : MonoBehaviour
{
    private EnemyView enemyView;

    private void Awake()
    {
        enemyView = GetComponent<EnemyView>();
    }

    public IEnumerator PerformAttack(CombatantView target)
    {
        // Play attack animation
        enemyView.PlayAnimation(CombatantAnimState.Attack);

        // Wait for visual impact
        yield return new WaitForSeconds(0.4f);

        // Deal damage
        DealDamageGA damageAction = new DealDamageGA(
            enemyView.AttackPower,
            new List<CombatantView> { target },
            enemyView
        );
        ActionSystem.Instance.Perform(damageAction);

        // Wait for animation to finish
        yield return new WaitForSeconds(0.3f);

        // Return to idle
        enemyView.PlayIdleAnimation();
    }
}
```

---

## Example 4: Victory/Defeat Sequences

### Victory Screen

```csharp
/// <summary>
/// Play victory animations when combat ends
/// </summary>
public class VictorySequence : MonoBehaviour
{
    public void OnCombatVictory()
    {
        // Get all surviving heroes
        foreach (var hero in HeroSystem.Instance.HeroViews)
        {
            if (hero.CurrentHealth > 0)
            {
                // Play victory animation
                hero.PlayAnimation(CombatantAnimState.Victory);
            }
        }
    }
}
```

### Enemy Death Sequence

```csharp
/// <summary>
/// Example: Enemies already play death animation automatically
/// But you can add extra effects
/// </summary>
public class EnemyDeathHandler : MonoBehaviour
{
    [SerializeField] private GameObject deathVFX;
    [SerializeField] private AudioClip deathSound;

    // Subscribe to enemy death events
    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<KillEnemyGA>(OnEnemyDeath, ReactionTiming.PRE);
    }

    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<KillEnemyGA>(OnEnemyDeath, ReactionTiming.PRE);
    }

    private void OnEnemyDeath(KillEnemyGA killAction)
    {
        EnemyView enemy = killAction.Enemy;

        // Death animation already playing automatically
        // Add extra VFX/SFX
        if (deathVFX != null)
        {
            Instantiate(deathVFX, enemy.transform.position, Quaternion.identity);
        }

        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, enemy.transform.position);
        }
    }
}
```

---

## Example 5: Multi-Target Abilities

### Area of Effect with Sequential Animations

```csharp
/// <summary>
/// AoE ability that processes each target with animation delays
/// </summary>
public class AoEAbilityExample : MonoBehaviour
{
    [SerializeField] private int aoeDamage = 3;
    [SerializeField] private float delayBetweenHits = 0.2f;

    public void OnCardPlayed(CombatantView caster, List<CombatantView> targets)
    {
        StartCoroutine(ExecuteAoESequence(caster, targets));
    }

    private IEnumerator ExecuteAoESequence(CombatantView caster, List<CombatantView> targets)
    {
        // Caster plays cast animation for AoE
        caster.PlayAnimation(CombatantAnimState.Cast);

        // Wait a bit for cast animation
        yield return new WaitForSeconds(0.3f);

        // Deal damage to each target with visual delay
        foreach (var target in targets)
        {
            if (target != null)
            {
                // Damage action (triggers hit animation automatically)
                DealDamageGA damageAction = new DealDamageGA(
                    aoeDamage,
                    new List<CombatantView> { target },
                    caster
                );
                ActionSystem.Instance.Perform(damageAction);

                // Brief delay before next target
                yield return new WaitForSeconds(delayBetweenHits);
            }
        }

        // Caster returns to idle
        caster.PlayIdleAnimation();
    }
}
```

---

## Example 6: Using Animation Events (Advanced)

### Setting Up Animation Events

In Unity Animation window:

1. Select your animation clip (e.g., `bagani_attack.anim`)
2. Scrub to the frame where impact occurs
3. Click the Event button (or right-click timeline)
4. Add Event
5. Function: `OnAttackImpact`

### Extended Animation Controller with Events

```csharp
/// <summary>
/// Extended version of CombatantAnimationController with custom event handling
/// </summary>
public class ExtendedAnimationController : CombatantAnimationController
{
    // Store pending action to execute on animation event
    private System.Action pendingImpactAction;

    /// <summary>
    /// Play attack and execute action at precise impact frame
    /// </summary>
    public void PlayAttackWithCallback(System.Action onImpact)
    {
        pendingImpactAction = onImpact;
        PlayAttack();
    }

    /// <summary>
    /// Called by Animation Event at impact frame
    /// </summary>
    public new void OnAttackImpact()
    {
        // Execute pending action at precise moment
        pendingImpactAction?.Invoke();
        pendingImpactAction = null;
    }
}
```

### Using the Extended Controller

```csharp
/// <summary>
/// Card that uses precise animation timing
/// </summary>
public class PrecisionAttackCard : MonoBehaviour
{
    public void OnCardPlayed(CombatantView caster, List<CombatantView> targets)
    {
        var extendedController = caster.GetComponent<ExtendedAnimationController>();

        if (extendedController != null)
        {
            // Damage will execute exactly at animation impact frame
            extendedController.PlayAttackWithCallback(() =>
            {
                DealDamageGA damageAction = new DealDamageGA(10, targets, caster);
                ActionSystem.Instance.Perform(damageAction);
            });
        }
        else
        {
            // Fallback to normal timing
            caster.PlayAnimation(CombatantAnimState.Attack);
            DealDamageGA damageAction = new DealDamageGA(10, targets, caster);
            ActionSystem.Instance.Perform(damageAction);
        }
    }
}
```

---

## Example 7: Status Effect Animations

### Burn Effect with Animation

```csharp
/// <summary>
/// Show hit animation when burn damage is applied
/// </summary>
public class BurnAnimationHandler : MonoBehaviour
{
    private void OnEnable()
    {
        // Subscribe to burn damage events
        ActionSystem.SubscribeReaction<ApplyBurnGA>(OnBurnDamage, ReactionTiming.POST);
    }

    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<ApplyBurnGA>(OnBurnDamage, ReactionTiming.POST);
    }

    private void OnBurnDamage(ApplyBurnGA burnAction)
    {
        // Play subtle hit animation for burn damage
        foreach (var target in burnAction.Targets)
        {
            if (target != null && target.CurrentHealth > 0)
            {
                target.PlayAnimation(CombatantAnimState.Hit);
            }
        }
    }
}
```

---

## Example 8: Turn Start/End Animations

### Idle Animations During Turns

```csharp
/// <summary>
/// Ensure characters return to idle at appropriate times
/// </summary>
public class TurnAnimationManager : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.SubscribeReaction<PlayerTurnGA>(OnPlayerTurnStart, ReactionTiming.POST);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(OnEnemyTurnStart, ReactionTiming.POST);
    }

    private void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<PlayerTurnGA>(OnPlayerTurnStart, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(OnEnemyTurnStart, ReactionTiming.POST);
    }

    private void OnPlayerTurnStart(PlayerTurnGA turnAction)
    {
        // Ensure all heroes are in idle state at turn start
        foreach (var hero in HeroSystem.Instance.HeroViews)
        {
            if (hero != null && hero.CurrentHealth > 0)
            {
                hero.PlayIdleAnimation();
            }
        }
    }

    private void OnEnemyTurnStart(EnemyTurnGA turnAction)
    {
        // Enemies return to idle before their turn
        foreach (var enemy in EnemySystem.Instance.Enemies)
        {
            if (enemy != null && enemy.CurrentHealth > 0)
            {
                enemy.PlayIdleAnimation();
            }
        }
    }
}
```

---

## Best Practices Summary

### DO:

✅ Play attack/cast animations BEFORE dealing damage
✅ Let hit animations play automatically (already integrated)
✅ Let death animations play automatically (already integrated)
✅ Return to idle after action sequences complete
✅ Use coroutines for precise timing
✅ Keep animation durations short (0.3-0.8s) for responsive combat

### DON'T:

❌ Play animations during Update() loops (performance)
❌ Forget to return to idle after actions
❌ Make animations too long (slows combat flow)
❌ Trigger multiple animations simultaneously on same character
❌ Rely on animation events unless you need precise frame timing

### Performance Tips:

- Animation triggers are lightweight (cached hashes)
- Coroutines are fine for turn-based combat
- Avoid complex animation blending
- Keep sprite animations simple
- Pool VFX objects if spawning many

---

## Troubleshooting Animation Issues

### Problem: Animation plays but damage happens instantly

**Solution:** Use coroutine with WaitForSeconds to delay damage

### Problem: Character stuck in attack pose

**Solution:** Ensure you call `PlayIdleAnimation()` after action completes

### Problem: Animations look choppy

**Solution:**

- Check animation sample rate (12 FPS is typical for pixel art)
- Ensure transitions are smooth (0.1-0.2s duration)
- Verify exit times are set correctly (0.90-0.95)

### Problem: Multiple animations overlap

**Solution:**

- Check transition interruption settings
- Ensure only one animation triggers at a time
- Use coroutines to sequence actions

---

## Quick Reference: When to Use Each State

| State   | When to Use                         | Returns to Idle?   |
| ------- | ----------------------------------- | ------------------ |
| Idle    | Default state, waiting, no action   | N/A (is idle)      |
| Attack  | Physical attacks, weapon swings     | Yes (automatic)    |
| Cast    | Magic spells, special abilities     | Yes (automatic)    |
| Hit     | Taking damage (automatic)           | Yes (automatic)    |
| Defend  | Using defensive abilities, blocking | Yes (automatic)    |
| Dead    | Health reaches 0 (automatic)        | No (final state)   |
| Victory | Combat won (optional polish)        | No (end of combat) |

---

Your animation system is fully integrated and ready to use! Just add the animator parameters and create your animation clips, then use these examples as templates for your cards and abilities.
