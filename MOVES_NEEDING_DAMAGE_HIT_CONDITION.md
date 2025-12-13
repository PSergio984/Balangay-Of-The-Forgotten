# Moves That Need DAMAGE_HIT Condition & TargetsHitByPreviousEffectTM

This document lists all moves with **< 100% hit chance** that have status effects/debuffs that should **only apply if the primary damage effect hits**.

**Important**: Moves with 100% hit chance don't need `DAMAGE_HIT` condition since they always hit.

---

## 📋 Quick Reference

### Single Target Moves (< 100% hit)

- Need: `ConditionalEffect` with `DAMAGE_HIT` condition
- Don't need: `TargetsHitByPreviousEffectTM` (only one target)

### AoE Moves (< 100% hit)

- Need: `ConditionalEffect` with `DAMAGE_HIT` condition
- Need: `TargetsHitByPreviousEffectTM` target mode (to filter to only hit targets)

---

## ✅ Single Target Moves - Need DAMAGE_HIT Only

### 1. **Heavy Attack** (Mandirigma)

- **Line 4**: Deals 334% ATK, **80% hit**, CD 2, inflict **Bonecracked** on enemy
- **Status Effect**: Bonecracked (Reduce target DEF by 10% for next turn)
- **Hit Chance**: 80% (not 100%)
- **Target Type**: Single target
- **Fix Required**:
  - Wrap Bonecracked effect in `ConditionalEffect` with `DAMAGE_HIT` condition
  - Use regular target mode (not `TargetsHitByPreviousEffectTM`)
- **Priority**: ⭐⭐⭐ HIGH

### 2. **Explosive Arrow** (Archer)

- **Line 30**: Deals 300 (+ 500% ATK), **50% hit**, **30% chance** to inflict **Overexplosion**
- **Status Effect**: Overexplosion (Deal 10% of damage dealt to all enemies, immediate)
- **Hit Chance**: 50% (not 100%)
- **Target Type**: Single target
- **Fix Required**:
  - Wrap Overexplosion effect in `ConditionalEffect` with `DAMAGE_HIT` condition
  - Use regular target mode (not `TargetsHitByPreviousEffectTM`)
- **Note**: Has two conditions - first must hit (50%), then 30% chance for Overexplosion
- **Priority**: ⭐⭐⭐ HIGH

### 3. **Skyhammer** (Bathala Boss) ✅ **FIXED**

- **Line 62-63**: Deals 175% MAG on a single target, **70% chance to stun** 1 player for 1 turn
- **Status Effect**: Stun (Affected targets cannot attack)
- **Hit Chance**: Not specified, but stun should only apply if damage hits
- **Target Type**: Single target (random)
- **Fix Required**: ✅ **COMPLETED** - Stun effect wrapped in `ConditionalEffect` with `DAMAGE_HIT` condition
- **Configuration**:
  - Secondary Effect 0: `DealDamageEffect` (175% MAG, Accuracy: 1.0, RandomTargetTM)
  - Secondary Effect 1: `ConditionalEffect` (Condition Type: DAMAGE_HIT, TargetsHitByPreviousEffectTM)
    - Inner Effect: `StunEffect` (Stun Chance: 0.7, Duration: 1, Max Targets: 1)
- **Note**: Uses `TargetsHitByPreviousEffectTM` (works for single target too, but not strictly necessary)
- **Priority**: ⭐⭐⭐ HIGH

### 4. **Moonfall Spear** (Mayari Boss)

- **Line 74-76**: Deals 105% ATK, Inflict **Moonfall**: Reduces **the hit enemies** DEF by 20% for 2 turns
- **Status Effect**: Moonfall (Reduces DEF by 20% for 2 turns)
- **Hit Chance**: Not specified, but description says "hit enemies"
- **Target Type**: Single target
- **Fix Required**:
  - Wrap Moonfall effect in `ConditionalEffect` with `DAMAGE_HIT` condition
  - Use regular target mode (not `TargetsHitByPreviousEffectTM`)
- **Note**: Description explicitly says "hit enemies" - confirms it should only apply to hit targets
- **Priority**: ⭐⭐⭐ HIGH

---

## ✅ AoE Moves - Need DAMAGE_HIT + TargetsHitByPreviousEffectTM

### 5. **Thunderous Decree** (Bathala Boss)

- **Line 64-66**: Deals 125% MAG on all players, **50% chance to stun 2 players** for 1 turn
- **Status Effect**: Stun (Affected targets cannot attack)
- **Hit Chance**: Not specified, but likely < 100% (AoE with conditional stun)
- **Target Type**: **AoE (all players)** - some may miss
- **Fix Required**:
  - Wrap Stun effect in `ConditionalEffect` with `DAMAGE_HIT` condition
  - **Use `TargetsHitByPreviousEffectTM` target mode** to only target players that were hit
- **Note**: Should only stun players that were actually hit by the damage (not all players)
- **Priority**: ⭐⭐⭐ HIGH

### 6. **Apolaki Move** (Apolaki Boss)

- **Line 86-87**: Deals 80% ATK + 100% MAG to **the hit enemies**, **30% chance to stun 1 of those hit enemies**
- **Status Effect**: Stun (Affected targets cannot attack)
- **Hit Chance**: Not specified, but description says "hit enemies"
- **Target Type**: **AoE (all enemies)** - some may miss
- **Fix Required**:
  - Wrap Stun effect in `ConditionalEffect` with `DAMAGE_HIT` condition
  - **Use `TargetsHitByPreviousEffectTM` target mode** to only target enemies that were hit
- **Note**: Description explicitly says "hit enemies" and "those hit enemies" - confirms requirement
- **Priority**: ⭐⭐⭐ HIGH

### 7. **Lunar Devour** (Bakunawa Boss)

- **Line 101-103**: Deal 80% MAG to all enemies, **Each target that are hit by this** gets inflicted by **Devoured**
- **Status Effect**: Devoured (for 2 turns, takes DMG equal to 20% of MAG, fixed at 60HP)
- **Hit Chance**: Not specified, but description says "hit by this"
- **Target Type**: **AoE (all enemies)** - some may miss
- **Fix Required**:
  - Wrap Devoured effect in `ConditionalEffect` with `DAMAGE_HIT` condition
  - **Use `TargetsHitByPreviousEffectTM` target mode** to only target enemies that were hit
- **Note**: Description explicitly says "target that are hit by this" - confirms requirement
- **Priority**: ⭐⭐⭐ HIGH

---

## ⚠️ Needs Review - Ambiguous

### 8. **Serpent's Coil** (Bakunawa Boss)

- **Line 98-100**: **Binds enemy**, reducing DEF by 15% for 2 turns, Deal 50 (+150%) ATK damage
- **Status Effect**: Bind (reduce target DEF by 15% for 2 turns)
- **Hit Chance**: Not specified
- **Target Type**: Single target (likely)
- **Issue**: Description order suggests bind happens first, but logically should only bind if damage hits
- **Fix Required**: Review design intent - if bind should only apply on hit, wrap in `ConditionalEffect` with `DAMAGE_HIT`
- **Priority**: ⭐⭐ MEDIUM (Needs design clarification)

---

## ✅ Safe - No Fix Needed

These moves either have 100% hit chance or don't have conditional status effects:

### 100% Hit Moves (No DAMAGE_HIT needed):

- **Attack** (Mandirigma) - 100% hit, no status effect
- **Shield Bash** (Bagani) - 100% hit, no status effect
- **Quick Shot** (Archer) - 100% hit, no status effect
- **Mana Surge** (Babaylan) - 100% hit, no status effect

### Moves with < 100% hit but no status effects:

- **All-in Attack** (Mandirigma) - 40% hit, but no status effect mentioned
- **Piercing Arrow** (Archer) - 80% hit, but no status effect mentioned
- **Volley** (Archer) - 80% hit, but no status effect mentioned

### Non-damage moves:

- **Rest** (Mandirigma) - Removes debuffs, no damage
- **Berserk State** (Mandirigma) - Self-buff with HP requirement, no damage
- **Taunt** (Bagani) - No damage, just status effect
- **Fortify** (Bagani) - Self-buff, no damage
- **Last Stand** (Bagani) - Self-buff with HP requirement, no damage
- **Guardian's Oath** (Bagani) - Self-sacrifice + shield, no damage to enemies
- **Heal** (Babaylan) - Healing, no damage
- **Blessing** (Babaylan) - Buff, no damage
- **Purify** (Babaylan) - Debuff removal, no damage
- **Sacrifice** (Babaylan) - Self-damage + healing, no enemy damage
- **Focus Aim** (Archer) - Self-buff, no damage

---

## Implementation Guide

### For Single Target Moves (< 100% hit):

1. **Open the move/card asset** in Unity Inspector
2. **Find the status effect** in `OtherEffects` or `ManualTargetEffect`
3. **Wrap it in a ConditionalEffect**:
   - Create a new `ConditionalEffect` instance
   - Set **Condition Type** to `DAMAGE_HIT`
   - Set **Inner Effect** to the status effect (e.g., `ApplyDefenseDownEffect` for Bonecracked)
   - Use **regular target mode** (not `TargetsHitByPreviousEffectTM`)
4. **Replace the original status effect** with the wrapped `ConditionalEffect`

### For AoE Moves (< 100% hit):

1. **Open the move/card asset** in Unity Inspector
2. **Find the status effect** in `OtherEffects`
3. **Wrap it in a ConditionalEffect**:
   - Create a new `ConditionalEffect` instance
   - Set **Condition Type** to `DAMAGE_HIT`
   - Set **Inner Effect** to the status effect
4. **Set target mode to `TargetsHitByPreviousEffectTM`**:
   - In the `AutoTargetEffect` wrapper, set **Target Mode** to `TargetsHitByPreviousEffectTM`
   - This ensures only targets hit by the previous damage effect are considered
5. **Replace the original status effect** with the wrapped `ConditionalEffect`

### Example: Heavy Attack Fix (Single Target)

```
Before:
- ManualTargetEffect: DealDamageEffect (334% ATK, 80% hit)
- OtherEffects[0]: ApplyDefenseDownEffect (Bonecracked, 10% DEF reduction)
  - Target Mode: SingleTargetTM (or appropriate)

After:
- ManualTargetEffect: DealDamageEffect (334% ATK, 80% hit)
- OtherEffects[0]: ConditionalEffect
  - Condition Type: DAMAGE_HIT
  - Inner Effect: ApplyDefenseDownEffect (Bonecracked, 10% DEF reduction)
  - Target Mode: SingleTargetTM (same as before)
```

### Example: Thunderous Decree Fix (AoE)

```
Before:
- OtherEffects[0]: DealDamageEffect (125% MAG, AllHeroesTM)
- OtherEffects[1]: StunEffect (50% chance, 2 targets)
  - Target Mode: AllHeroesTM

After:
- OtherEffects[0]: DealDamageEffect (125% MAG, AllHeroesTM)
- OtherEffects[1]: ConditionalEffect
  - Condition Type: DAMAGE_HIT
  - Inner Effect: StunEffect (50% chance, 2 targets)
  - Target Mode: TargetsHitByPreviousEffectTM ⭐ IMPORTANT
```

---

## Testing Checklist

After implementing fixes, test each move:

### Single Target Moves:

- [ ] Heavy Attack: Miss damage → No Bonecracked applied
- [ ] Heavy Attack: Hit damage → Bonecracked applied
- [ ] Explosive Arrow: Miss damage → No Overexplosion
- [ ] Explosive Arrow: Hit damage → 30% chance for Overexplosion
- [ ] Skyhammer: Miss damage → No Stun
- [ ] Skyhammer: Hit damage → 70% chance for Stun
- [ ] Moonfall Spear: Miss damage → No Moonfall
- [ ] Moonfall Spear: Hit damage → Moonfall applied

### AoE Moves (Critical - Test with partial hits):

- [ ] Thunderous Decree: Hit 2/4 players → Only those 2 can be stunned
- [ ] Thunderous Decree: Miss all players → No stuns possible
- [ ] Apolaki Move: Hit 2/3 enemies → Only those 2 can be stunned
- [ ] Apolaki Move: Miss all enemies → No stuns possible
- [ ] Lunar Devour: Hit 2/3 enemies → Only those 2 get Devoured
- [ ] Lunar Devour: Miss all enemies → No Devoured applied

### Ambiguous:

- [ ] Serpent's Coil: Test both scenarios (bind before/after damage) based on design decision

---

## Summary

**Total Moves Needing Fix**: 7 confirmed + 1 needs review

**Breakdown by Type**:

- **Single Target Moves** (< 100% hit): 4 moves
  - Need: `ConditionalEffect` with `DAMAGE_HIT` only
- **AoE Moves** (< 100% hit): 3 moves
  - Need: `ConditionalEffect` with `DAMAGE_HIT` + `TargetsHitByPreviousEffectTM`
- **Needs Review**: 1 move (Serpent's Coil)

**Priority Breakdown**:

- ⭐⭐⭐ HIGH Priority: 7 moves (all confirmed)
- ⭐⭐ MEDIUM Priority: 1 move (needs design clarification)

**Key Point**: AoE moves with < 100% hit chance **must** use `TargetsHitByPreviousEffectTM` to ensure status effects only apply to targets that were actually hit, not all potential targets.
