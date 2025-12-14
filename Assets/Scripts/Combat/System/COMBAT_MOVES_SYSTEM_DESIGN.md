# Combat Moves System Design Document

## Balangay Of The Forgotten - Turn-Based Combat System

---

## 1. Architecture Analysis

### 1.1 Existing Patterns Overview

The Balangay combat system uses an **event-driven, action-based architecture** with the following core components:

#### ActionSystem (Singleton)

- **Location:** `Assets/Scripts/Combat/GameActions/ActionSystem.cs`
- **Purpose:** Central brain that processes all game actions in order
- **Key Methods:**
  - `Perform(GameAction action)` - Main entry point for executing actions
  - `AddReaction(GameAction gameAction)` - Queue additional actions during processing
  - `AttachPerformer<T>(Func<T, IEnumerator> performer)` - Register action logic
  - `SubscribeReaction<T>(Action<T> reaction, ReactionTiming timing)` - Global reactions
- **Three-Phase Processing:**
  1. **PRE Phase** - Reactions before main action (e.g., armor reduction)
  2. **PERFORM Phase** - Main action execution
  3. **POST Phase** - Reactions after main action (e.g., apply burn)

#### GameAction (Abstract Base Class)

- **Location:** `Assets/Scripts/Combat/GameActions/GameAction.cs`
- **Purpose:** Blueprint for all game actions
- **Key Properties:**
  - `PreReactions` - Actions before main execution
  - `PerformReactions` - Actions during main execution
  - `PostReactions` - Actions after main execution

#### Effects System

- **Base Class:** `Assets/Scripts/Combat/Models/Effects.cs`
- **EffectSystem:** `Assets/Scripts/Combat/GameActions/EffectSystem.cs`
- **Pattern:** Effects → `GetGameAction()` → GameAction → ActionSystem
- **Caster Tracking:** All effects receive caster for perk system integration

#### Status Effects Pattern

- **Enum:** `Assets/Scripts/Combat/Enums/StatusEffectType.cs`
- **Systems:** Use PRE/POST subscriptions to modify actions
- **Example:** `ArmorStatusEffectSystem` subscribes to `DealDamageGA` PRE events

#### DamageCalculator (Static Utility)

- **Location:** `Assets/Scripts/Combat/System/DamageCalculator.cs`
- **Key Methods:**
  - `CalculateSkillPower(baseDamage, attackAmp, magicAmp, attackPower, magicPower)`
  - `CalculateFinalDamage(skillPower, damageAmp, coefficient, targetDefense, critMultiplier)`
  - `IsHit(accuracy)` - D20 roll for hit chance
  - `IsCriticalHit(critChance)` - D20 roll for crit
  - `ApplyDefenseIgnore(currentDef, ignorePercent)`

---

## 2. Move Categorization

### 2.1 Hero Moves by Category

| Category               | Moves                                                        | Key Mechanics                            |
| ---------------------- | ------------------------------------------------------------ | ---------------------------------------- |
| **Basic Damage**       | Attack, Shield Bash, Mana Surge, Quick Shot                  | Flat + ATK/MAG scaling, 100% hit         |
| **Advanced Damage**    | Heavy Attack, All-in Attack, Piercing Arrow, Explosive Arrow | Variable hit chance, conditional effects |
| **AoE Damage**         | Volley                                                       | Multi-target with hit roll per target    |
| **Status Applicators** | Taunt, Fortify, Last Stand, Guardian's Oath                  | Apply status effects to self/allies      |
| **Debuffs**            | Heavy Attack (Bonecracked), Focus Aim                        | Apply debuffs to enemies                 |
| **Buffs**              | Berserk State, Focus Aim, Blessing                           | Apply buffs to self/allies               |
| **Healing**            | Heal, Sacrifice                                              | Flat + MAG scaling, self-heal component  |
| **Cleanse**            | Rest, Purify                                                 | Remove debuffs from self/allies          |

### 2.2 Boss Moves by Category

| Boss         | Damage Moves                                                                                | Support Moves                                                        | Ultimate            |
| ------------ | ------------------------------------------------------------------------------------------- | -------------------------------------------------------------------- | ------------------- |
| **Bathala**  | Skyhammer (175% MAG + stun), Thunderous Decree (AoE + stun)                                 | Heaven's Mandate (DEF buff + cleanse)                                | Celestial Judgement |
| **Mayari**   | Lunar Strike (115% ATK + 20% MAG), Moonfall Spear (105% ATK + DEF debuff)                   | Moonlight Grace (heal), Tide of Night (invulnerable + cleanse + AoE) | -                   |
| **Apolaki**  | Solar Flare Slash (175% ATK + crit), Radiant Charge (AoE + stun), Sunburst Nova (AoE burst) | Daybreak Fury (self buff + HP cost)                                  | -                   |
| **Bakunawa** | Eclipse Fang (lifesteal), Serpent's Coil (damage + DEF debuff), Lunar Devour (AoE + DoT)    | Shadow Dive (counter/charge)                                         | -                   |

### 2.3 New Mechanics Required

1. **Stun Status** - Skip turn, prevent actions
2. **Shield Status** - Temporary HP that expires after turns
3. **Resting Status** - Skip turn (boss rest after ultimate)
4. **Charging Status** - Preparing for powerful attack (Bakunawa Shadow Dive)
5. **Defense Down** - Already exists (DEFENSE_DOWN)
6. **Defense Up** - Already exists (DEFENSE_UP)
7. **Hit Chance Up** - Increase accuracy for future attacks
8. **Devoured Status** - DoT debuff (fixed damage per turn)
9. **Bonecracked Status** - DEF reduction for 1 turn
10. **Moonfall Status** - DEF reduction for 2 turns

---

## 3. GameAction Class Designs

#### CompositeGA

```csharp
/// <summary>
/// Composite GameAction that executes a sequence of child GameActions in order.
/// Used for moves/effects that require multiple actions (e.g., self-damage + shield allies).
/// </summary>
public class CompositeGA : GameAction
{
    /// <summary>
    /// Ordered list of child GameActions to execute.
    /// </summary>
    public List<GameAction> Actions { get; private set; } = new List<GameAction>();

    /// <summary>
    /// Add a child GameAction to the composite.
    /// </summary>
    public void AddAction(GameAction action)
    {
        if (action != null) Actions.Add(action);
    }

    /// <summary>
    /// Executes all child actions in sequence. Context (targets, caster) is propagated as needed.
    /// If any child action fails, subsequent actions may be skipped or rolled back (implementation-dependent).
    /// </summary>
    public override IEnumerator Perform()
    {
        foreach (var action in Actions)
        {
            // Propagate context if needed
            yield return ActionSystem.Instance.Perform(action);
        }
    }

    /// <summary>
    /// Constructor: optionally accepts initial actions.
    /// </summary>
    public CompositeGA(params GameAction[] initialActions)
    {
        if (initialActions != null)
            Actions.AddRange(initialActions);
    }

    // Lifetime/serialization: CompositeGA should serialize its child actions for persistence.
    // Thread/async: Actions are performed in order via coroutine; parallel execution is not standard but can be extended.
}
```

### 3.1a IHaveCaster Interface Contract

#### IHaveCaster

```csharp
/// <summary>
/// Interface for GameActions that require knowledge of the action's caster.
/// Implementing classes must expose a CombatantView Caster { get; set; } property.
/// The setter should be public to allow assignment during construction or deserialization.
/// Caster may be null if the action is system-generated or casterless, but is typically non-null for player/enemy actions.
/// Ownership: The system that creates the GameAction (effect, move, or event) is responsible for setting Caster.
/// </summary>
public interface IHaveCaster
{
    CombatantView Caster { get; set; }
}
```

**Implementation Pattern:**

- Classes inheriting from `GameAction` and requiring caster context should implement `IHaveCaster`.
- The `Caster` property should be `[public]` for both getter and setter.
- The property is typically set in the GameAction constructor, but may also be set via deserialization or by the ActionSystem when reconstructing actions.
- Nullability: `Caster` may be null for system actions, but is expected to be non-null for most combat moves and effects.

**Example Implementation:**

```csharp
public class ApplyShieldGA : GameAction, IHaveCaster
{
    /// <summary>
    /// List of HealTarget, each containing a target and its shield amount.
    /// </summary>
    public List<HealTarget> ShieldTargets { get; private set; }
    public int Duration { get; private set; }
    public CombatantView Caster { get; set; } // Set in constructor or by ActionSystem

    public ApplyShieldGA(List<HealTarget> shieldTargets, int duration, CombatantView caster)
    {
        ShieldTargets = shieldTargets;
        Duration = duration;
        Caster = caster;
    }
}

public class ApplyBuffGA : GameAction, IHaveCaster
{
    public List<CombatantView> Targets { get; private set; }
    public BuffType BuffType { get; private set; }
    public float Value { get; private set; }
    public int Duration { get; private set; }
    public CombatantView Caster { get; set; }

    public ApplyBuffGA(List<CombatantView> targets, BuffType buffType, float value, int duration, CombatantView caster)
    {
        Targets = targets;
        BuffType = buffType;
        Value = value;
        Duration = duration;
        Caster = caster;
    }
}
```

### 3.1 New GameAction Classes Required

#### ApplyStunGA

```csharp
// Ensure IHaveCaster is imported if needed
public class ApplyStunGA : GameAction, IHaveCaster
{
    public List<CombatantView> Targets { get; private set; }
    public int Duration { get; private set; }  // Turns to skip
    public CombatantView Caster { get; set; } // Public get/set for caster context
}
```

#### ApplyShieldGA

```csharp
public class ApplyShieldGA : GameAction, IHaveCaster
{
    /// <summary>
    /// List of HealTarget, each containing a target and its shield amount.
    /// </summary>
    public List<HealTarget> ShieldTargets { get; private set; }
    public int Duration { get; private set; }
    public CombatantView Caster { get; set; }

    public ApplyShieldGA(List<HealTarget> shieldTargets, int duration, CombatantView caster)
    {
        ShieldTargets = shieldTargets;
        Duration = duration;
        Caster = caster;
    }
}
```

#### SelfDamageGA

```csharp
public class SelfDamageGA : GameAction
{
    public CombatantView Target { get; private set; }
    public float Amount { get; private set; }
    public bool IsPercentOfCurrent { get; private set; }  // true = % of current HP
    public bool IsPercentOfMax { get; private set; }      // true = % of max HP
}
```

#### ApplyBuffGA

```csharp
public class ApplyBuffGA : GameAction, IHaveCaster
{
    public List<CombatantView> Targets { get; private set; }
    public BuffType BuffType { get; private set; }
    public float Value { get; private set; }  // Percentage or flat value
    public int Duration { get; private set; }
    public CombatantView Caster { get; set; }
}
```

#### RemoveDebuffsGA

```csharp
public class RemoveDebuffsGA : GameAction
{
    public List<CombatantView> Targets { get; private set; }
    public bool RemoveAll { get; private set; }  // true = all debuffs, false = specific
    public StatusEffectType? SpecificType { get; private set; }
}
```

#### SkipTurnGA

```csharp
public class SkipTurnGA : GameAction
{
    public CombatantView Target { get; private set; }
    public int TurnsToSkip { get; private set; }
}
```

#### LifestealDamageGA

```csharp
public class LifestealDamageGA : GameAction, IHaveCaster
{
    public float DamageAmount { get; private set; }
    public CombatantView Target { get; private set; }
    public float LifestealPercent { get; private set; }  // e.g., 1.0 = 100% of damage
    public CombatantView Caster { get; set; }
}
```

### 3.2 Existing GameActions to Reuse

| GameAction          | Used For                                                 |
| ------------------- | -------------------------------------------------------- |
| `DealDamageGA`      | All damage moves (already has per-target damage support) |
| `HealGA`            | All healing moves (already has multi-target + self-heal) |
| `ApplyBuffGA`       | Apply buffs to self/allies                               |
| `AddStatusEffectGA` | Apply status effects (armor, burn, taunt, etc.)          |
| `ApplyBurnGA`       | Burn damage over time                                    |

---

## 4. Effect Class Designs

### 4.1 New Effect Classes Required

#### ApplyShieldEffect

```csharp
[System.Serializable]
public class ApplyShieldEffect : Effects
{
    [SerializeField] private float shieldPercentOfMaxHP = 0.3f;  // 30% max HP
    [SerializeField] private float flatShieldAmount = 0f;
    [SerializeField] private int duration = 2;  // turns

    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        // Calculate shield amount for each target
        List<HealTarget> shieldTargets = new();
        foreach (var target in targets)
        {
            float amount = flatShieldAmount + (target.MaxHealth * shieldPercentOfMaxHP);
            shieldTargets.Add(new HealTarget(target, amount));
        }
        return new ApplyShieldGA(shieldTargets, duration, caster);
    }
}
```

#### StunEffect

```csharp
[System.Serializable]
public class StunEffect : Effects
{
    [SerializeField] private float stunChance = 0.7f;  // 70% chance
    [SerializeField] private int stunDuration = 1;
    [SerializeField] private int maxTargets = 1;  // How many targets can be stunned

    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        // Roll for stun on each target up to maxTargets
        // Returns ApplyStunGA with stunned targets
    }
}
```

#### SelfDamageEffect

```csharp
[System.Serializable]
public class SelfDamageEffect : Effects
{
    [SerializeField] private float flatDamage = 0f;
    [SerializeField] private float percentOfCurrentHP = 0f;
    [SerializeField] private float percentOfMaxHP = 0f;

    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        return new SelfDamageGA(caster, flatDamage, percentOfCurrentHP, percentOfMaxHP);
    }
}
```

#### ApplyBuffEffect

```csharp
[System.Serializable]
public class ApplyBuffEffect : Effects
{
    [SerializeField] private StatusEffectType buffType;  // ATTACK_UP, DEFENSE_UP, etc.
    [SerializeField] private float value = 0.2f;  // 20%
    [SerializeField] private int duration = 2;
    [SerializeField] private int stacks = 1;

    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        // Use ApplyBuffGA for buff application
        return new ApplyBuffGA(targets, buffType, value, duration, caster);
    }
}
```

#### RemoveDebuffsEffect

```csharp
[System.Serializable]
public class RemoveDebuffsEffect : Effects
{
    [SerializeField] private bool removeAll = true;
    [SerializeField] private StatusEffectType specificDebuff;

    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        return new RemoveDebuffsGA(targets, removeAll, specificDebuff);
    }
}
```

#### ConditionalDamageEffect (for conditional effects like Explosive Arrow's Overexplosion)

```csharp
/// <summary>
/// ConditionalDamageEffect applies bonus damage as a POST reaction after a main damage action.
/// Rolls against triggerChance; if successful, applies damagePercentOfDealt * originalDamage to target(s).
/// </summary>
[System.Serializable]
public class ConditionalDamageEffect : Effects
{
    [SerializeField] private float triggerChance = 0.3f;  // 30%
    [SerializeField] private float damagePercentOfDealt = 0.1f;  // 10% of damage dealt
    [SerializeField] private bool targetAllies = true;

    /// <summary>
    /// Called as a POST reaction, reads context from base ReactionContext property.
    /// </summary>
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        // ReactionContext is set by the system before calling this method
        if (ReactionContext == null)
        {
            UnityEngine.Debug.Log("ConditionalDamageEffect: No reaction context provided.");
            return null;
        }
        float originalDamage = ReactionContext.OriginalDamage;
        CombatantView originalTarget = ReactionContext.OriginalTarget;

        // Edge-case checks
        if (originalDamage <= 0)
        {
            UnityEngine.Debug.Log("ConditionalDamageEffect: No original damage to apply bonus.");
            return null;
        }
        if (targets == null || targets.Count == 0)
        {
            UnityEngine.Debug.Log("ConditionalDamageEffect: No valid targets for bonus damage.");
            return null;
        }

        // Roll for trigger
        float roll = UnityEngine.Random.value;
        if (roll > triggerChance)
        {
            UnityEngine.Debug.Log($"ConditionalDamageEffect: Trigger failed (roll={roll}, chance={triggerChance})");
            return null;
        }

        float bonusDamage = originalDamage * damagePercentOfDealt;
        if (bonusDamage <= 0)
        {
            UnityEngine.Debug.Log("ConditionalDamageEffect: Calculated bonus damage is zero.");
            return null;
        }

        // Determine targets: allies or enemies
        List<CombatantView> bonusTargets = new List<CombatantView>();
        if (targetAllies)
        {
            // Apply to all allies except caster
            bonusTargets.AddRange(targets.FindAll(t => t != caster));
        }
        else
        {
            // Apply to original target or all targets
            if (originalTarget != null)
                bonusTargets.Add(originalTarget);
            else
                bonusTargets.AddRange(targets);
        }

        UnityEngine.Debug.Log($"ConditionalDamageEffect: Applying bonus damage {bonusDamage} to {bonusTargets.Count} target(s).");
        return new DealDamageGA(bonusDamage, bonusTargets, caster);
    }
// ---
// ReactionContext for Effects (used for POST-reaction context passing)
public class ReactionContext
{
    public float OriginalDamage { get; set; }
    public CombatantView OriginalTarget { get; set; }
    // Add more fields as needed for other reaction types
}

// In Effects base class:
// protected/internal property for context
public abstract class Effects : ScriptableObject
{
    // ...existing code...
    public ReactionContext ReactionContext { get; set; }
    // ...existing code...
}
}
```

### 4.2 Existing Effects to Reuse

| Effect                  | Used For                                                                                      |
| ----------------------- | --------------------------------------------------------------------------------------------- |
| `DealDamageEffect`      | All damage (already has baseDamage, AttackAmp, MagicAmp, accuracy, critChance, defenseIgnore) |
| `HealEffect`            | All healing (already has baseHeal, MagicAmp, percentHeal, selfHealPercent)                    |
| `AddStatusEffectEffect` | Apply any status effect                                                                       |

---

## 5. System Class Designs

### 5.1 StunStatusEffectSystem

```csharp
// PSEUDOCODE — NOT IMPLEMENTED
// TODO: Implement full turn-start subscription and turn-skip logic for STUN
public class StunStatusEffectSystem : MonoBehaviour
{
    // OnEnable: subscribe to turn start events and attach performer
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ApplyStunGA>(ApplyStunPerformer);
        // TODO: Subscribe to turn start event (e.g., TurnManager.OnTurnStart)
    }

    // Performer for ApplyStunGA
    private IEnumerator ApplyStunPerformer(ApplyStunGA action)
    {
        foreach (var target in action.Targets)
        {
            // TODO: Add STUN stacks and handle skip logic
            target.AddStatusEffect(StatusEffectType.STUN, action.Duration);
        }
        yield return null;
    }
    // TODO: OnTurnStart, if combatant has STUN > 0, skip turn and decrement stack
}
```

### 5.2 ShieldStatusEffectSystem

```csharp
// PSEUDOCODE — NOT IMPLEMENTED
// TODO: Implement shield absorption and duration tracking
public class ShieldStatusEffectSystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ApplyShieldGA>(ApplyShieldPerformer);
        ActionSystem.SubscribeReaction<DealDamageGA>(OnDamageAboutToBeDealt, ReactionTiming.PRE);
    }

    // TODO: Performer for ApplyShieldGA
    private IEnumerator ApplyShieldPerformer(ApplyShieldGA action)
    {
        // TODO: Apply shield stacks to targets
        yield return null;
    }

    // TODO: PRE-reaction to absorb damage into shield before HP
    private void OnDamageAboutToBeDealt(DealDamageGA action)
    {
        // TODO: Absorb damage into shield, remove expired shields
    }
}
```

### 5.3 BuffDebuffSystem

```csharp
// PSEUDOCODE — NOT IMPLEMENTED
// TODO: Implement buff/debuff duration tracking and stat modification
public class BuffDebuffSystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<ApplyBuffGA>(ApplyBuffPerformer);
        ActionSystem.AttachPerformer<RemoveDebuffsGA>(RemoveDebuffsPerformer);
    }

    // TODO: Performer for ApplyBuffGA
    private IEnumerator ApplyBuffPerformer(ApplyBuffGA action)
    {
        // TODO: Apply buff to targets, track duration
        yield return null;
    }

    // TODO: Performer for RemoveDebuffsGA
    private IEnumerator RemoveDebuffsPerformer(RemoveDebuffsGA action)
    {
        // TODO: Remove debuffs from targets
        yield return null;
    }

    // TODO: OnTurnEnd, decrement buff durations and remove expired buffs
    // TODO: Modify stats when buffs are active
}
```

### 5.4 LifestealSystem

```csharp
public class LifestealSystem : MonoBehaviour
{
    // Handle lifesteal damage + healing

    private void OnEnable()
    {
        ActionSystem.AttachPerformer<LifestealDamageGA>(LifestealPerformer);
    }

    private IEnumerator LifestealPerformer(LifestealDamageGA action)
    {
        // Deal damage first
        var damageAction = new DealDamageGA(action.DamageAmount,
            new List<CombatantView> { action.Target }, action.Caster);
        ActionSystem.Instance.AddReaction(damageAction);

        // Then heal caster
        float healAmount = action.DamageAmount * action.LifestealPercent;
        var healAction = new HealGA(
            new List<HealTarget> { new HealTarget(action.Caster, healAmount) },
            action.Caster);
        ActionSystem.Instance.AddReaction(healAction);

        yield return null;
    }
}
```

### 5.5 SelfDamageSystem

```csharp
// PSEUDOCODE — NOT IMPLEMENTED
// TODO: Implement CalculateSelfDamage and full performer logic
public class SelfDamageSystem : MonoBehaviour
{
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<SelfDamageGA>(SelfDamagePerformer);
    }

    private IEnumerator SelfDamagePerformer(SelfDamageGA action)
    {
        // TODO: Implement or call CalculateSelfDamage
        int damage = CalculateSelfDamage(action); // Placeholder
        action.Target.Damage(damage);
        yield return null;
    }

    // TODO: Implement CalculateSelfDamage based on action parameters
}
```

---

## 6. New StatusEffectType Additions

```csharp
public enum StatusEffectType
{
    // Existing
    ARMOR,
    ATTACK_UP,
    BURN,
    CRIT_UP,
    DEFENSE_DOWN,
    DEFENSE_UP,
    DMG_UP,
    IGNORE_DEFENSE,
    INVULNERABLE,
    TAUNT,
    TEMP_HP,

    // New - Required for AllMoves.md
    STUN,           // Skip turn
    SHIELD,         // Temporary HP with duration
    RESTING,        // Cannot act (boss post-ultimate)
    CHARGING,       // Preparing attack (Bakunawa Shadow Dive)
    HIT_UP,         // Increased accuracy
    DEVOURED,       // DoT debuff (Bakunawa)
    BONECRACKED,    // DEF reduction 10% for 1 turn
    MOONFALL,       // DEF reduction 20% for 2 turns
    RAGE,           // +50% DMG, +20% def ignore, +20% hit (Berserk State)
    FOCUSED,        // +30% hit, +20% def ignore (Focus Aim)
    BLESSED,        // +20% DMG buff (Blessing)
}
```

---

## 7. Implementation Priority

### Phase 1: Foundation (Core Systems)

1. ✅ Update `StatusEffectType.cs` with new types
2. Create `ApplyStunGA.cs`
3. Create `StunStatusEffectSystem.cs`
4. Create `RemoveDebuffsGA.cs` (fix existing placeholder)
5. Create `RemoveDebuffsEffect.cs` (fix existing placeholder)

### Phase 2: Shield & Self-Damage

1. Create `ApplyShieldGA.cs`
2. Create `ApplyShieldEffect.cs`
3. Create `ShieldStatusEffectSystem.cs`
4. Create `SelfDamageGA.cs`
5. Create `SelfDamageEffect.cs`
6. Create `SelfDamageSystem.cs`

### Phase 3: Buff/Debuff System

1. Create `ApplyBuffGA.cs`
2. Create `ApplyBuffEffect.cs`
3. Create `BuffDebuffSystem.cs`
4. Create duration tracking for timed effects

### Phase 4: Lifesteal & Advanced

1. Create `LifestealDamageGA.cs`
2. Create `LifestealEffect.cs`
3. Create `LifestealSystem.cs`
4. Create `StunEffect.cs`
5. Create `ConditionalEffectWrapper.cs`

### Phase 5: Boss Mechanics

1. Create `SkipTurnGA.cs`
2. Create `RestingStatusEffectSystem.cs`
3. Create `ChargingStatusEffectSystem.cs`
4. Create boss-specific targeting (highest HP)

---

## 8. File Structure

```
Assets/Scripts/Combat/
├── Effects/
│   ├── [EXISTING] DealDamageEffect.cs
│   ├── [EXISTING] HealEffect.cs
│   ├── [EXISTING] AddStatusEffectEffect.cs
│   ├── [NEW] ApplyShieldEffect.cs
│   ├── [NEW] StunEffect.cs
│   ├── [NEW] SelfDamageEffect.cs
│   ├── [NEW] ApplyBuffEffect.cs
│   ├── [FIX] RemoveDebuffsEffect.cs
│   └── [NEW] ConditionalDamageEffect.cs
├── GameActions/
│   ├── [EXISTING] DealDamageGA.cs
│   ├── [EXISTING] HealGA.cs
│   ├── [EXISTING] AddStatusEffectGA.cs
│   ├── [NEW] ApplyStunGA.cs
│   ├── [NEW] ApplyShieldGA.cs
│   ├── [NEW] SelfDamageGA.cs
│   ├── [NEW] ApplyBuffGA.cs
│   ├── [FIX] RemoveDebuffsGA.cs
│   ├── [NEW] LifestealDamageGA.cs
│   └── [NEW] SkipTurnGA.cs
├── StatusEffects/
│   ├── [EXISTING] ArmorStatusEffectSystem.cs
│   ├── [EXISTING] BurnSystem.cs
│   ├── [EXISTING] TauntStatusEffectSystem.cs
│   ├── [EXISTING] InvulnerableStatusEffectSystem.cs
│   ├── [NEW] StunStatusEffectSystem.cs
│   ├── [NEW] ShieldStatusEffectSystem.cs
│   ├── [NEW] BuffDebuffSystem.cs
│   ├── [NEW] LifestealSystem.cs
│   ├── [NEW] SelfDamageSystem.cs
│   ├── [NEW] RestingStatusEffectSystem.cs
│   └── [NEW] ChargingStatusEffectSystem.cs
└── Enums/
    └── [UPDATE] StatusEffectType.cs
```

---

## 9. Reference Implementations

### Example 1: Simple Move - Attack (Mandirigma)

**Move:** "Deals 50 (+150% ATK), 100% hit, CD 1"

Uses existing `DealDamageEffect`:

```csharp
// In EnemyMoveData ScriptableObject or CardData
[SerializeField] private DealDamageEffect attackEffect;
// Configured in Inspector:
// - baseDamage: 50
// - AttackAmp: 1.5 (150%)
// - MagicAmp: 0
// - accuracy: 1.0 (100%)
// - critChance: 0
// - defenseIgnore: 0
```

### Example 2: Medium Complexity - Heavy Attack (Bonecracked)

**Move:** "Deals 334% ATK, 80% hit, CD 2, 50% chance to inflict Bonecracked"

Requires chained effects:

1. DealDamageEffect (334% ATK, 80% hit)
2. ConditionalStatusEffect (50% chance → BONECRACKED)

```csharp
// In CardData or EnemyMoveData
public List<AutoTargetEffect> OtherEffects;
// Effect 1: Damage
// Effect 2: Conditional debuff application
```

### Example 3: Complex Move - Guardian's Oath

**Move:** "Sacrifice 25% current HP, shield all ally (except itself) for 25% current HP for 2 turns"

```csharp
// GuardiansOathEffect.cs
[System.Serializable]
public class GuardiansOathEffect : Effects
{
    [SerializeField] private float sacrificePercent = 0.25f;
    [SerializeField] private int shieldDuration = 2;

    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        // Calculate sacrifice amount (float), then round to int for damage and shield
        float sacrificeAmount = caster.CurrentHealth * sacrificePercent;
        int damageAmount = Mathf.RoundToInt(sacrificeAmount);

        // Create composite action
        var compositeAction = new CompositeGA();

        // 1. Self damage (explicit named parameters for clarity)
        compositeAction.AddAction(new SelfDamageGA(
            target: caster,
            amount: damageAmount,
            isPercentOfCurrent: false,
            isPercentOfMax: false
        ));

        // 2. Shield allies (excluding caster)
        var allies = targets.Where(t => t != caster).ToList();
        foreach (var ally in allies)
        {
            var shieldTargets = new List<HealTarget> { new HealTarget(ally, damageAmount) };
            compositeAction.AddAction(new ApplyShieldGA(
                shieldTargets: shieldTargets,
                duration: shieldDuration,
                caster: caster
            ));
        }

        return compositeAction;
    }
}
```

---

## 10. Integration Checklist

### For Each New GameAction:

- [ ] Create class inheriting from `GameAction`
- [ ] Add all required properties with documentation
- [ ] Implement `IHaveCaster` if caster tracking needed
- [ ] Create corresponding System with `AttachPerformer<T>`
- [ ] Register/unregister in `OnEnable`/`OnDisable`
- [ ] Performer method returns `IEnumerator`
- [ ] Add VFX/animation triggers where appropriate

### For Each New Effect:

- [ ] Create class inheriting from `Effects`
- [ ] Add `[System.Serializable]` attribute
- [ ] Add `[SerializeField]` for Inspector configuration
- [ ] Implement `GetGameAction()` method
- [ ] Handle null targets gracefully
- [ ] Include caster in returned GameAction

### For Each New StatusEffectType:

- [ ] Add to `StatusEffectType.cs` enum
- [ ] Create corresponding status effect system
- [ ] Subscribe to relevant GameAction events
- [ ] Handle duration tracking if timed
- [ ] Update StatusEffectsUI if visual needed
- [ ] Add sprite to status effect icon set

---

## 11. Notes & Considerations

### Performance

- Cache references in `Awake()`/`Start()`
- Use object pooling for VFX
- Avoid allocations in Update loops
- Pre-allocate lists where possible

### Edge Cases

- What if target dies mid-action chain?
- What if combatant has multiple buffs of same type?
- What if shield expires during damage calculation?
- Stun should prevent ALL actions including passives?

### Testing Checkpoints

1. Stun prevents turn correctly
2. Shield absorbs damage before HP
3. Debuffs removed by cleanse
4. Duration tracking decrements properly
5. Buff effects apply to stat calculations
6. Self-damage doesn't trigger on-damage reactions

---

_Document Version: 1.0_
_Last Updated: December 6, 2025_
_Status: Ready for Implementation_
