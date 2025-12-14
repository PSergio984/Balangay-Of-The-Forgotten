# Lunar Devour Configuration Guide

## Move Description

**Lunar Devour (AoE, Debuff)**

- Deal 80% MAG to all enemies.
- Each target that are hit by this gets inflicted by Devoured: for 2 turns, takes DMG equal to 20% of MAG (fixed at 60HP).

## Configuration Steps

### 1. Effect 0: Damage Effect (80% MAG to all heroes)

**Target Selection:**

- Set to `AllHeroesTM (TargetMode)`

**Effect Type:**

- Set to `DealDamageEffect (Effects)`

**Parameters:**

- `Base Damage`: 0
- `Damage Amplification`: 1
- `Accuracy`: 1
- `Crit Chance`: 0
- `Defense Ignore`: 0
- `Attack Amp`: 0
- `Magic Amp`: **0.8** (this is 80% MAG)

### 2. Effect 1: Devoured Status Effect (only on hit targets)

**Target Selection:**

- Set to `TargetsHitByPreviousEffectTM (TargetMode)` ⚠️ **NEW TARGET MODE**
  - This target mode will automatically select only heroes that were successfully hit (not missed) by Effect 0
  - The caster is automatically set by EnemySystem when processing the move

**Effect Type:**

- Set to `ApplyDevouredEffect (Effects)` ⚠️ **NEW EFFECT TYPE**

**Parameters:**

- `MAG Damage (%)`: **20** (20% of MAG as damage per turn)
- `Fixed Damage (0 = use MAG%)`: **60** (overrides MAG% if > 0)
  - For Lunar Devour: Set to 60 to use fixed damage instead of MAG%
- `Duration (Turns)`: **2** (how many turns Devoured lasts)
  - Note: Stacks now represent duration only, not damage
  - The system will deal 60 fixed damage per turn for 2 turns

## How It Works

1. **Effect 0 (Damage)** executes first:

   - Targets all heroes using `AllHeroesTM`
   - Deals 80% MAG damage to each
   - `HitTargetTracker` records which heroes were successfully hit (damage > 0, not missed)

2. **Effect 1 (Devoured)** executes second:
   - Uses `TargetsHitByPreviousEffectTM` to query `HitTargetTracker`
   - Only targets heroes that were hit by Effect 0
   - Applies DEVOURED status effect with configurable MAG% or fixed damage
   - Duration is tracked separately (stacks = duration, not damage)
   - Heroes that missed the attack do NOT get Devoured

## Important Notes

- **Target Mode Order Matters**: Effect 0 must come before Effect 1 in the `OtherEffects` list
- **Hit Tracking**: The system automatically tracks hits within the same move sequence
- **Miss Handling**: Heroes that dodge/miss the damage will NOT receive Devoured
- **DEVOURED Damage**: The current implementation deals stack count as damage per turn, then reduces by 1. For exactly 60 damage per turn for 2 turns, you may need to adjust the system, but 60 stacks gives close to the desired effect.

## Troubleshooting

**Problem**: Devoured is applied to all heroes, even those that missed

- **Solution**: Make sure Effect 1 uses `TargetsHitByPreviousEffectTM`, not `AllHeroesTM`

**Problem**: Devoured is not applied to anyone

- **Solution**: Check that Effect 0 comes before Effect 1 in the effects list
- **Solution**: Verify that Effect 0 actually deals damage (Magic Amp = 0.8, not 0)

**Problem**: Wrong damage amount from Devoured

- **Solution**: Check `MAG Damage (%)` and `Fixed Damage` values in `ApplyDevouredEffect`
- **Solution**: If `Fixed Damage` > 0, it overrides MAG%. Set to 0 to use MAG% instead.
