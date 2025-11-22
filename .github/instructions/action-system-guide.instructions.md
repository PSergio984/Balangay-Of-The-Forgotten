---
name: Action-System-Guide
description: Comprehensive guide for using the ActionSystem, GameAction, and effect systems in the Balangay turn-based combat game
applyTo: "**/*.cs"
---

# Action System Guide for Balangay Combat

## Overview

This guide explains how to use the **ActionSystem**, **GameAction**, and related effect systems in the Balangay turn-based combat game. These systems form the core of how all game actions (playing cards, dealing damage, applying status effects, etc.) are processed.

---

## Core Concepts

### The Action System Architecture

The game uses a **three-phase action processing pipeline**:

1. **PRE Phase** - Reactions that happen before the main action
2. **PERFORM Phase** - The main action execution
3. **POST Phase** - Reactions that happen after the main action

This creates a chain reaction system where actions can trigger other actions, enabling complex combos and interactions.

---

## Creating Custom Game Actions

### Step 1: Define Your Action Class

All actions inherit from `GameAction`. Create a new class for your specific action:

```csharp
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Action that deals damage to target combatants
/// </summary>
public class DealDamageGA : GameAction
{
    // The combatant who is causing the damage (for tracking purposes)
    public CombatantScript Caster { get; private set; }

    // List of combatants who will receive the damage
    public List<CombatantScript> Targets { get; private set; }

    // Amount of damage to deal
    public int DamageAmount { get; private set; }

    /// <summary>
    /// Constructor to create a deal damage action
    /// </summary>
    /// <param name="caster">Who is dealing the damage</param>
    /// <param name="targets">Who receives the damage</param>
    /// <param name="damageAmount">How much damage to deal</param>
    public DealDamageGA(CombatantScript caster, List<CombatantScript> targets, int damageAmount)
    {
        Caster = caster;
        Targets = targets;
        DamageAmount = damageAmount;
    }
}
```

### Step 2: Register the Action Logic

Create a system that handles your action type by registering a "performer":

```csharp
using System.Collections;
using UnityEngine;

/// <summary>
/// System that processes damage actions
/// </summary>
public class DamageSystem : MonoBehaviour
{
    private void OnEnable()
    {
        // Register this system to handle DealDamageGA actions
        ActionSystem.AttachPerformer<DealDamageGA>(DealDamagePerformer);
    }

    private void OnDisable()
    {
        // Unregister when disabled to prevent memory leaks
        ActionSystem.DetachPerformer<DealDamageGA>();
    }

    /// <summary>
    /// The actual logic that executes when a DealDamageGA action is performed
    /// </summary>
    private IEnumerator DealDamagePerformer(DealDamageGA action)
    {
        // Process each target
        foreach (var target in action.Targets)
        {
            // Apply the damage to the target
            target.TakeDamage(action.DamageAmount);

            // Wait a frame for visual effects
            yield return null;
        }
    }
}
```

### Step 3: Execute Your Action

To trigger your action anywhere in the code:

```csharp
// Create the action
var damageAction = new DealDamageGA(
    caster: playerCombatant,
    targets: new List<CombatantScript> { enemyCombatant },
    damageAmount: 5
);

// Execute the action through the ActionSystem
ActionSystem.Instance.Perform(damageAction);
```

---

## Working with Effects (Cards & Perks)

### Understanding the Effect Pipeline

1. **Effect Definition** - Effects are defined in ScriptableObjects (EffectSO)
2. **Effect Wrapping** - Effects get wrapped in `PerformEffectGA` actions
3. **Effect Processing** - `EffectSystem` converts effects into concrete GameActions
4. **Action Execution** - The GameActions are processed by the ActionSystem

### Creating a Card Effect

```csharp
// In your card or perk code
public class FireballCard : MonoBehaviour
{
    [SerializeField] private EffectSO damageEffect; // Assigned in Inspector

    public void PlayCard()
    {
        // Get targets (e.g., all enemies)
        List<CombatantScript> targets = GetEnemyTargets();

        // Wrap the effect in a PerformEffectGA action
        var effectAction = new PerformEffectGA(
            effect: damageEffect,
            targets: targets
        );

        // Execute through ActionSystem
        ActionSystem.Instance.Perform(effectAction);
    }
}
```

### Creating a Custom Effect Type

Define your effect in a ScriptableObject:

```csharp
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Effect that deals damage to targets
/// </summary>
[CreateAssetMenu(fileName = "DamageEffect", menuName = "Effects/Damage Effect")]
public class DamageEffectSO : EffectSO
{
    [SerializeField] private int damageAmount = 5;

    /// <summary>
    /// Convert this effect into a concrete GameAction
    /// </summary>
    public override GameAction GetGameAction(List<CombatantScript> targets, CombatantScript caster)
    {
        // Return the appropriate GameAction for this effect
        return new DealDamageGA(caster, targets, damageAmount);
    }
}
```

---

## Working with Status Effects

Status effects (like Armor, Burn, Poison) use a dedicated system.

### Applying Status Effects

```csharp
// Create a status effect action
var statusAction = new AddStatusEffectGA(
    targets: new List<CombatantScript> { targetCombatant },
    statusEffectType: StatusEffectType.Armor,
    stackCount: 5
);

// Execute through ActionSystem
ActionSystem.Instance.Perform(statusAction);
```

The `StatusEffectSystem` will automatically process this action and apply the status effect to the targets.

---

## Adding Reactions to Actions

Reactions allow you to create complex interactions like "whenever you play a card, gain 1 energy".

### PRE Reactions (Before Main Action)

```csharp
public class EnergyCard : MonoBehaviour
{
    public void PlayCard()
    {
        // Create the main card action
        var playCardAction = new PlayCardGA(this);

        // Add a PRE reaction: gain energy before the card effect happens
        var gainEnergyAction = new GainEnergyGA(amount: 1);
        playCardAction.PreReactions.Add(gainEnergyAction);

        // Execute the action (energy will be gained first, then card plays)
        ActionSystem.Instance.Perform(playCardAction);
    }
}
```

### POST Reactions (After Main Action)

```csharp
public class DrawCardOnPlay : MonoBehaviour
{
    public void PlayCard()
    {
        // Create the main card action
        var playCardAction = new PlayCardGA(this);

        // Add a POST reaction: draw a card after the card effect happens
        var drawAction = new DrawCardGA(count: 1);
        playCardAction.PostReactions.Add(drawAction);

        // Execute the action (card plays, then you draw)
        ActionSystem.Instance.Perform(playCardAction);
    }
}
```

---

## Creating Global Reactions (Passive Abilities)

Global reactions trigger whenever a specific action type occurs anywhere in the game.

### PRE Subscription (Before Any Action)

```csharp
public class PassiveEnergyPerk : MonoBehaviour
{
    private void OnEnable()
    {
        // Whenever ANY card is played, gain 1 energy first
        ActionSystem.AttachPreReaction<PlayCardGA>(OnAnyCardPlayed);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPreReaction<PlayCardGA>();
    }

    private void OnAnyCardPlayed(GameAction action)
    {
        // Cast to the specific action type
        var playCardAction = (PlayCardGA)action;

        // Add a reaction to gain energy
        var gainEnergyAction = new GainEnergyGA(1);
        ActionSystem.Instance.AddReaction(gainEnergyAction);
    }
}
```

### POST Subscription (After Any Action)

```csharp
public class OnDamageDealtPerk : MonoBehaviour
{
    private void OnEnable()
    {
        // Whenever ANY damage is dealt, apply burn
        ActionSystem.AttachPostReaction<DealDamageGA>(OnDamageDealt);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPostReaction<DealDamageGA>();
    }

    private void OnDamageDealt(GameAction action)
    {
        var damageAction = (DealDamageGA)action;

        // Add burn to all targets that took damage
        var burnAction = new AddStatusEffectGA(
            targets: damageAction.Targets,
            statusEffectType: StatusEffectType.Burn,
            stackCount: 1
        );

        ActionSystem.Instance.AddReaction(burnAction);
    }
}
```

---

## Best Practices

### 1. Always Use Coroutines for Performers

```csharp
// CORRECT: Returns IEnumerator
private IEnumerator MyPerformer(MyActionGA action)
{
    // Do work
    yield return null;
}

// WRONG: Regular method won't work with ActionSystem
private void MyPerformer(MyActionGA action)
{
    // This won't work!
}
```

### 2. Clean Up Subscriptions

Always detach performers and reactions in `OnDisable()` to prevent memory leaks:

```csharp
private void OnEnable()
{
    ActionSystem.AttachPerformer<MyActionGA>(MyPerformer);
}

private void OnDisable()
{
    ActionSystem.DetachPerformer<MyActionGA>();
}
```

### 3. Use AddReaction for Dynamic Actions

When creating actions inside performers or reactions, use `AddReaction()`:

```csharp
private IEnumerator MyPerformer(MyActionGA action)
{
    // Create a new action dynamically
    var newAction = new AnotherActionGA();

    // Add it to the action queue
    ActionSystem.Instance.AddReaction(newAction);

    yield return null;
}
```

### 4. Keep Actions Simple and Focused

Each action should do ONE thing well:

```csharp
// GOOD: Focused action
public class DealDamageGA : GameAction
{
    // Just deals damage
}

// BAD: Does too many things
public class DealDamageAndHealAndDrawGA : GameAction
{
    // Too complex - split into separate actions!
}
```

### 5. Use Callbacks for Completion

When you need to know when an action chain is complete:

```csharp
ActionSystem.Instance.Perform(myAction, OnActionComplete);

private void OnActionComplete()
{
    Debug.Log("Action finished!");
    // Continue with game logic
}
```

---

## Common Patterns

### Pattern 1: Card Effect with Multiple Effects

```csharp
public void PlayMultiEffectCard()
{
    var action = new PlayCardGA(this);

    // Add damage effect
    action.PerformReactions.Add(new DealDamageGA(caster, targets, 5));

    // Add status effect
    action.PerformReactions.Add(new AddStatusEffectGA(targets, StatusEffectType.Burn, 2));

    ActionSystem.Instance.Perform(action);
}
```

### Pattern 2: Conditional Effect

```csharp
private IEnumerator ConditionalPerformer(MyActionGA action)
{
    if (action.Caster.CurrentHealth < 10)
    {
        // Low health bonus
        var bonusAction = new HealGA(action.Caster, 5);
        ActionSystem.Instance.AddReaction(bonusAction);
    }

    yield return null;
}
```

### Pattern 3: Chain Reaction Combo

```csharp
private IEnumerator ComboPerformer(ComboActionGA action)
{
    // First action
    var damage = new DealDamageGA(action.Caster, action.Targets, 5);
    ActionSystem.Instance.AddReaction(damage);

    yield return new WaitForSeconds(0.5f);

    // Second action (triggers after damage)
    var status = new AddStatusEffectGA(action.Targets, StatusEffectType.Stun, 1);
    ActionSystem.Instance.AddReaction(status);

    yield return null;
}
```

---

## Debugging Tips

### 1. Add Debug Logs

```csharp
private IEnumerator MyPerformer(MyActionGA action)
{
    Debug.Log($"[MySystem] Processing action for {action.Targets.Count} targets");

    // Your logic here

    Debug.Log($"[MySystem] Action complete");
    yield return null;
}
```

### 2. Check isPerforming Flag

```csharp
if (ActionSystem.Instance.isPerforming)
{
    Debug.LogWarning("Action system is busy!");
}
```

### 3. Verify Performer Registration

```csharp
private void Start()
{
    if (!ActionSystem.HasPerformer<MyActionGA>())
    {
        Debug.LogError("MyActionGA performer not registered!");
    }
}
```

---

## Summary

The ActionSystem provides a robust, flexible framework for handling all game actions:

- **Create actions** by inheriting from `GameAction`
- **Register logic** with `AttachPerformer<T>()`
- **Execute actions** with `ActionSystem.Instance.Perform()`
- **Add reactions** to create complex interactions
- **Use effects** for card and perk functionality
- **Follow best practices** for clean, maintainable code

This system enables you to build complex card interactions, perk abilities, and combo systems with clean, modular code.

---

## Quick Reference

```csharp
// Create and perform an action
var action = new MyActionGA();
ActionSystem.Instance.Perform(action);

// Register a performer
ActionSystem.AttachPerformer<MyActionGA>(MyPerformer);

// Add reactions
action.PreReactions.Add(new AnotherActionGA());
action.PostReactions.Add(new YetAnotherActionGA());

// Global subscriptions
ActionSystem.AttachPreReaction<MyActionGA>(OnMyAction);
ActionSystem.AttachPostReaction<MyActionGA>(OnMyActionComplete);

// Dynamic reactions
ActionSystem.Instance.AddReaction(new DynamicActionGA());

// Clean up
ActionSystem.DetachPerformer<MyActionGA>();
ActionSystem.DetachPreReaction<MyActionGA>();
ActionSystem.DetachPostReaction<MyActionGA>();
```
