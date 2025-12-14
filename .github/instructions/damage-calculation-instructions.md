# Balangay Combat Calculation System

## Overview
This document defines the core calculation logic for the Balangay turn-based combat game. All formulas are system-agnostic and can be implemented in Unity C#, servers, or other platforms.

---

## Table of Contents
1. [Damage Calculation](#damage-calculation)
2. [Healing Calculation](#healing-calculation)
3. [Defense Calculation](#defense-calculation)
4. [Critical Hit System](#critical-hit-system)
5. [Accuracy System](#accuracy-system)
6. [Shield System](#shield-system)
7. [Status Effect Modifiers](#status-effect-modifiers)

---

## 1. Damage Calculation

### Core Damage Formula

```
DMG = [DMG_skill * DMG_AMP / (C * (1 + DEF_final * 0.01))] * CRIT_MULTIPLIER
```

**Where:**
- `DMG_skill` = Base damage of the skill/ability
- `DMG_AMP` = Damage amplification multiplier (1.0 = 100%, 1.2 = +20% damage)
- `C` = Coefficient based on attacker type:
  - `1.0` for Player → Enemy attacks
  - `1.5` for Enemy → Player attacks (enemies hit harder)
- `DEF_final` = Target's final defense value after all modifiers
- `CRIT_MULTIPLIER` = Critical hit multiplier (default 1.0, crit = 1.5 or 1.2)

---

### Skill Damage Calculation

**Formula:**
```
DMG_skill = baseDamage + (AttackAmp × caster.AttackPower) + (MagicAmp × caster.MagicPower)
```

**Parameters:**
- `baseDamage`: Flat skill damage (e.g., 50)
- `AttackAmp`: Physical damage multiplier (e.g., 1.5 = 150% ATK scaling)
- `MagicAmp`: Magical damage multiplier (e.g., 1.0 = 100% MAG scaling)
- `caster.AttackPower`: Attacker's ATK stat
- `caster.MagicPower`: Attacker's MAG stat

**Example Usage:**
```csharp
// Physical Attack: 50 base + (1.5 × ATK)
float skillPower = 50 + (1.5f * caster.AttackPower);

// Magic Attack: 0 base + (2.0 × MAG)
float skillPower = 0 + (2.0f * caster.MagicPower);

// Hybrid Attack: 100 base + (0.8 × ATK) + (1.0 × MAG)
float skillPower = 100 + (0.8f * caster.AttackPower) + (1.0f * caster.MagicPower);
```

---

### Implementation Example (C#)

```csharp
/// <summary>
/// Calculates final damage using the core formula
/// </summary>
/// <param name="skillPower">Calculated skill power (baseDamage + scaling)</param>
/// <param name="damageAmplification">Damage amplification from buffs (1.0 = 100%)</param>
/// <param name="coefficient">Attacker coefficient (1.0 for player, 1.5 for enemy)</param>
/// <param name="targetDefense">Target's final defense value</param>
/// <param name="critMultiplier">Critical multiplier (1.0 = no crit, 1.5 = player crit, 1.2 = enemy crit)</param>
/// <returns>Final damage as integer (minimum 1)</returns>
public static int CalculateFinalDamage(
    float skillPower, 
    float damageAmplification, 
    float coefficient, 
    float targetDefense, 
    float critMultiplier)
{
    float denominator = coefficient * (1f + targetDefense * 0.01f);
    float rawDamage = (skillPower * damageAmplification) / denominator;
    float finalDamage = rawDamage * critMultiplier;
    
    return Mathf.Max(1, Mathf.RoundToInt(finalDamage)); // Minimum 1 damage
}
```

**Usage Example:**
```csharp
// Player attacks enemy with fireball (100 base + 50% ATK + 150% MAG)
float skillPower = 100 + (0.5f * playerATK) + (1.5f * playerMAG);
float damageAmp = 1.2f; // +20% damage from buff
float coefficient = 1.0f; // Player attacking
float enemyDef = 50f; // Enemy has 50 defense
float critMult = 1.5f; // Critical hit!

int finalDamage = CalculateFinalDamage(skillPower, damageAmp, coefficient, enemyDef, critMult);
```

---

## 2. Healing Calculation

### Basic Healing Formula

```
HEAL_AMOUNT = heal_flat + (magPercent_heal × caster.MagicPower)
```

**Parameters:**
- `heal_flat`: Base flat healing amount
- `magPercent_heal`: Percentage of caster's MAG to add (0.5 = 50% MAG scaling)
- `caster.MagicPower`: Caster's MAG stat

---

### Self-Heal Component

```
SELF_HEAL = HEAL_AMOUNT × self_heal_percent
```

**Example:**
```csharp
/// <summary>
/// Calculates healing amount based on flat heal and magic power
/// </summary>
/// <param name="flatHeal">Base flat healing value</param>
/// <param name="magicPercent">Percentage of caster's MAG to add (0-1)</param>
/// <param name="casterMagic">Caster's MAG stat</param>
/// <returns>Total healing amount</returns>
public static int CalculateHealAmount(float flatHeal, float magicPercent, float casterMagic)
{
    return Mathf.RoundToInt(flatHeal + (magicPercent * casterMagic));
}

/// <summary>
/// Calculates self-heal amount from main heal
/// </summary>
public static int CalculateSelfHeal(int mainHealAmount, float selfHealPercent)
{
    return Mathf.RoundToInt(mainHealAmount * selfHealPercent);
}
```

**Usage Example:**
```csharp
// Heal spell: 100 base + 50% MAG
int healAmount = CalculateHealAmount(100f, 0.5f, casterMAG);

// Caster also heals self for 30% of the heal amount
int selfHeal = CalculateSelfHeal(healAmount, 0.3f);
```

---

### Healing with HP Sacrifice

```
SACRIFICE_AMOUNT = caster.currentHP × sacrifice_percent
HEAL_TO_ALLIES = heal_flat + (magPercent_heal × caster.MagicPower)
```

**Example:**
```csharp
/// <summary>
/// Calculates HP sacrifice and resulting heal
/// </summary>
public static (int sacrificeAmount, int healAmount) CalculateSacrificeHeal(
    int casterCurrentHP, 
    float sacrificePercent, 
    float flatHeal, 
    float magicPercent, 
    float casterMagic)
{
    int sacrifice = Mathf.RoundToInt(casterCurrentHP * sacrificePercent);
    int heal = Mathf.RoundToInt(flatHeal + (magicPercent * casterMagic));
    
    return (sacrifice, heal);
}
```

---

## 3. Defense Calculation

### Defense Modification Order

Defense is calculated in this specific order:
1. **Apply DEF Buffs** (multiplicative increases)
2. **Apply DEF Reductions** (debuffs that lower DEF)
3. **Apply DEF Ignore** (attacker ignores percentage of remaining DEF)

---

### Defense Buffs

**Formula:**
```
DEF_buffed = base_DEF × (1 + buff_percent)
```

**Example Buffs:**
- Heaven's Mandate: `DEF × 1.3` (+30% DEF)
- Last Stand: `DEF × 1.5` (+50% DEF)
- Dungeon Buff: `DEF × 1.25` (+25% DEF)

```csharp
/// <summary>
/// Applies defense buff to base defense
/// </summary>
public static float ApplyDefenseBuff(float baseDef, float buffPercent)
{
    return baseDef * (1f + buffPercent);
}
```

---

### Defense Reductions (Debuffs)

**Formula:**
```
DEF_reduced = current_DEF × (1 - debuff_percent)
```

**Example Debuffs:**
- Bone Cracked: `DEF × 0.9` (-10% DEF)
- Bind: `DEF × 0.85` (-15% DEF)
- Moonfall: `DEF × 0.8` (-20% DEF)

```csharp
/// <summary>
/// Applies defense reduction debuff
/// </summary>
public static float ApplyDefenseReduction(float currentDef, float reductionPercent)
{
    return currentDef * (1f - reductionPercent);
}
```

---

### Defense Ignore

**Formula:**
```
DEF_final = current_DEF × (1 - ignore_percent)
```

**Example Ignore Sources:**
- Piercing Arrow: Ignores 100% DEF (`ignore = 1.0`)
- Focus Aim: Ignores 20% DEF (`ignore = 0.2`)
- Rage: Ignores 20% DEF (`ignore = 0.2`)

```csharp
/// <summary>
/// Applies defense ignore from attacker
/// </summary>
public static float ApplyDefenseIgnore(float currentDef, float ignorePercent)
{
    return currentDef * (1f - ignorePercent);
}
```

---

### Complete Defense Calculation Example

```csharp
/// <summary>
/// Calculates final defense after all modifiers
/// </summary>
public static float CalculateFinalDefense(
    float baseDef,
    float[] buffPercents,
    float[] reductionPercents,
    float[] ignorePercents)
{
    float def = baseDef;
    
    // Step 1: Apply all buffs (multiplicative)
    foreach (float buff in buffPercents)
    {
        def *= (1f + buff);
    }
    
    // Step 2: Apply all reductions (multiplicative)
    foreach (float reduction in reductionPercents)
    {
        def *= (1f - reduction);
    }
    
    // Step 3: Apply all ignore effects (multiplicative)
    foreach (float ignore in ignorePercents)
    {
        def *= (1f - ignore);
    }
    
    return Mathf.Max(0f, def); // Defense cannot go below 0
}
```

**Usage Example:**
```csharp
float baseDef = 100f;
float[] buffs = new float[] { 0.3f }; // +30% from Heaven's Mandate
float[] debuffs = new float[] { 0.1f, 0.15f }; // -10% Bone Cracked, -15% Bind
float[] ignores = new float[] { 0.2f, 0.2f }; // 20% from Rage, 20% from Focus Aim

float finalDef = CalculateFinalDefense(baseDef, buffs, debuffs, ignores);
// Result: 100 × 1.3 × 0.9 × 0.85 × 0.8 × 0.8 ≈ 63.6
```

---

## 4. Critical Hit System

### Critical Chance Calculation

**Formula (D20 System):**
```
Roll d20 (1-20)
If roll == 20: Automatic critical hit
Else if roll <= (critChance × 20): Critical hit
Else: Normal hit
```

**Critical Multipliers:**
- **Player Critical**: `1.5×` damage
- **Enemy Critical**: `1.2×` damage

```csharp
/// <summary>
/// Determines if an attack is a critical hit
/// </summary>
/// <param name="critChance">Critical chance (0-1, e.g., 0.25 = 25%)</param>
/// <returns>True if critical hit</returns>
public static bool IsCriticalHit(float critChance)
{
    int roll = UnityEngine.Random.Range(1, 21); // Roll d20 (1-20)
    
    if (roll == 20) return true; // Automatic crit
    
    float threshold = critChance * 20f;
    return roll <= threshold;
}

/// <summary>
/// Gets the critical multiplier based on attacker type
/// </summary>
public static float GetCriticalMultiplier(bool isPlayer)
{
    return isPlayer ? 1.5f : 1.2f;
}
```

**Usage Example:**
```csharp
float critChance = 0.25f; // 25% crit chance
bool isCrit = IsCriticalHit(critChance);

if (isCrit)
{
    float critMultiplier = GetCriticalMultiplier(isPlayerAttack);
    // Apply critMultiplier to damage calculation
}
```

---

## 5. Accuracy System

### Accuracy Calculation (D20 System)

**Formula:**
```
Roll d20 (1-20)
If roll == 1: Automatic miss
Else if roll == 20: Automatic hit
Else if roll <= (accuracy × 20): Hit
Else: Miss
```

**Default Accuracy:** `1.0` (100% hit chance, excluding natural 1s)

```csharp
/// <summary>
/// Determines if an attack hits the target
/// </summary>
/// <param name="accuracy">Accuracy (0-1, e.g., 0.75 = 75% hit chance)</param>
/// <returns>True if attack hits</returns>
public static bool IsHit(float accuracy)
{
    int roll = UnityEngine.Random.Range(1, 21); // Roll d20 (1-20)
    
    if (roll == 1) return false; // Automatic miss
    if (roll == 20) return true;  // Automatic hit
    
    float threshold = accuracy * 20f;
    return roll <= threshold;
}
```

**Usage Example:**
```csharp
float accuracy = 0.8f; // 80% accuracy
bool didHit = IsHit(accuracy);

if (!didHit)
{
    // Attack missed, deal 0 damage
    return 0;
}

// Continue with damage calculation
```

---

## 6. Shield System

### Shield Stacking

**Shield Types:**
- **Stackable**: Multiple shields stack additively
- **Unstackable**: Only the highest shield value is kept, duration refreshes

**Shield Structure:**
```csharp
public struct ShieldStack
{
    public float amount;      // Shield HP value
    public int turnsLeft;     // Remaining duration
}
```

---

### Shield Calculation

**Percentage-Based Shield:**
```
SHIELD_AMOUNT = maxHP × shield_percent
```

**Current HP-Based Shield:**
```
SHIELD_AMOUNT = currentHP × shield_percent
```

```csharp
/// <summary>
/// Calculates shield amount from max HP
/// </summary>
public static int CalculateShieldFromMaxHP(int maxHP, float shieldPercent)
{
    return Mathf.RoundToInt(maxHP * shieldPercent);
}

/// <summary>
/// Calculates shield amount from current HP
/// </summary>
public static int CalculateShieldFromCurrentHP(int currentHP, float shieldPercent)
{
    return Mathf.RoundToInt(currentHP * shieldPercent);
}
```

**Example:**
```csharp
// Fortify: Gain shield = 30% of max HP
int maxHP = 1000;
int shieldAmount = CalculateShieldFromMaxHP(maxHP, 0.3f); // 300 shield

// Guardian's Oath: Grant shield = 25% of caster's current HP
int currentHP = 800;
int shieldAmount = CalculateShieldFromCurrentHP(currentHP, 0.25f); // 200 shield
```

---

### Shield Damage Absorption

**Order of Damage Application:**
1. Damage hits shield first
2. Excess damage spills to HP
3. Shield cannot block damage marked as "ignore shield"

```csharp
/// <summary>
/// Applies damage to entity with shield absorption
/// </summary>
/// <returns>Tuple of (shieldDamage, hpDamage)</returns>
public static (int shieldDamage, int hpDamage) ApplyDamageWithShield(
    int totalDamage, 
    int currentShield, 
    int currentHP)
{
    int shieldDamage = Mathf.Min(totalDamage, currentShield);
    int remainingDamage = totalDamage - shieldDamage;
    int hpDamage = Mathf.Min(remainingDamage, currentHP);
    
    return (shieldDamage, hpDamage);
}
```

**Example:**
```csharp
int damage = 500;
int shield = 200;
int hp = 1000;

var (shieldLoss, hpLoss) = ApplyDamageWithShield(damage, shield, hp);
// shieldLoss = 200 (shield fully consumed)
// hpLoss = 300 (remaining damage to HP)
// New shield = 0, New HP = 700
```

---

## 7. Status Effect Modifiers

### Damage Amplification Effects

**Multiplicative Damage Buffs:**
- Blessing: `damage × 1.2` (+20%)
- Rage: `damage × 1.5` (+50%)
- Dungeon Buff: `damage × 1.15` (+15%)

**Combined Multiplicative Formula:**
```
FINAL_DAMAGE = base_damage × buff1 × buff2 × buff3 × ...
```

```csharp
/// <summary>
/// Applies all damage amplification buffs
/// </summary>
public static float ApplyDamageAmplification(float baseDamage, params float[] multipliers)
{
    float result = baseDamage;
    foreach (float mult in multipliers)
    {
        result *= mult;
    }
    return result;
}
```

**Example:**
```csharp
float baseDamage = 100f;
float blessingBuff = 1.2f;  // +20%
float rageBuff = 1.5f;      // +50%
float dungeonBuff = 1.15f;  // +15%

float amplifiedDamage = ApplyDamageAmplification(baseDamage, blessingBuff, rageBuff, dungeonBuff);
// Result: 100 × 1.2 × 1.5 × 1.15 = 207
```

---

### DoT (Damage Over Time) Calculation

**DoT Formula:**
```
DOT_DAMAGE = caster.MAG × dot_percent
```

**Example: Devoured Effect**
```
DOT = 20% of caster's MAG per turn
```

```csharp
/// <summary>
/// Calculates damage over time from magic power
/// </summary>
public static int CalculateDoTDamage(float casterMagic, float dotPercent)
{
    return Mathf.RoundToInt(casterMagic * dotPercent);
}
```

**Example:**
```csharp
float enemyMAG = 300f;
float devouredPercent = 0.2f; // 20% MAG per turn

int dotDamage = CalculateDoTDamage(enemyMAG, devouredPercent);
// Result: 60 damage per turn
```

---

### Current HP Percentage Damage

**Formula:**
```
DAMAGE = target.currentHP × damage_percent
```

**Example: Tide of Night (30% current HP damage)**

```csharp
/// <summary>
/// Calculates damage based on target's current HP
/// </summary>
public static int CalculateCurrentHPDamage(int targetCurrentHP, float damagePercent)
{
    return Mathf.Max(1, Mathf.RoundToInt(targetCurrentHP * damagePercent));
}
```

**Example:**
```csharp
int targetHP = 500;
float tidePercent = 0.3f; // 30% current HP

int damage = CalculateCurrentHPDamage(targetHP, tidePercent);
// Result: 150 damage (minimum 1)
```

---

## Implementation Notes

### Unity C# Integration

All formulas in this document can be directly implemented in Unity C# by:
1. Creating a static `CombatCalculations` class
2. Using `Mathf.RoundToInt()` for integer results
3. Using `Mathf.Max()` to enforce minimum values (e.g., minimum 1 damage)
4. Using `UnityEngine.Random.Range()` for d20 rolls

### Calculation Order

**For Damage:**
1. Calculate skill power (base + scaling)
2. Calculate final defense (buffs → reductions → ignore)
3. Check accuracy (hit or miss)
4. Check critical hit
5. Apply damage amplification buffs
6. Calculate final damage
7. Apply to shield first, then HP

**For Healing:**
1. Calculate base heal amount (flat + MAG scaling)
2. Apply to target HP (cannot exceed max HP)
3. Calculate self-heal if applicable
4. Apply to caster HP

---

## Quick Reference

### Core Formulas Summary

| Calculation Type | Formula |
|-----------------|---------|
| **Damage** | `[skillPower × amp / (C × (1 + DEF × 0.01))] × crit` |
| **Skill Power** | `base + (atkAmp × ATK) + (magAmp × MAG)` |
| **Healing** | `flat + (magPercent × MAG)` |
| **Defense Buff** | `DEF × (1 + buff%)` |
| **Defense Reduction** | `DEF × (1 - reduction%)` |
| **Defense Ignore** | `DEF × (1 - ignore%)` |
| **Shield (Max HP)** | `maxHP × shield%` |
| **Shield (Current HP)** | `currentHP × shield%` |
| **DoT** | `MAG × dot%` |
| **Current HP Damage** | `currentHP × damage%` |

### Coefficient Values

| Attacker Type | Coefficient (C) |
|---------------|----------------|
| Player → Enemy | 1.0 |
| Enemy → Player | 1.5 |

### Critical Multipliers

| Attacker Type | Critical Multiplier |
|---------------|-------------------|
| Player | 1.5× |
| Enemy | 1.2× |

---

## End of Document

This calculation system provides the foundation for all combat-related math in Balangay. Implement these functions in your combat systems (Unity, server, etc.) to ensure consistent gameplay across all platforms.
