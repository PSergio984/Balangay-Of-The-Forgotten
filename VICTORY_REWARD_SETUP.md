# Victory/Defeat Rewards System Setup Guide

This guide explains how to set up the reward system that displays chest rewards after each enemy defeat.

---

## Overview

The reward system works as follows:

1. **First Enemy (Miniboss) Defeated** → Victory banner → Continue → Reward chest → Next enemy spawns
2. **Second Enemy (Main Boss) Defeated** → Victory banner → Continue → Reward chest → Final victory → Map selection

Each map can have different rewards for the miniboss and main boss.

---

## Step 1: Create Reward Data Assets

### 1.1 Create Miniboss Reward Data

1. In Unity Project window, navigate to your ScriptableObjects folder (or create one)
2. Right-click → **Create** → **Map Selection** → **Reward Data**
3. Name it something like `RewardData_Miniboss_Map01`
4. In the Inspector, assign:
   - **Closed Chest Sprite**: The sprite for the closed chest (shown initially)
   - **Open Chest Sprite**: The sprite for the open chest (shown after clicking)
   - **Glow Sprite**: The glow effect sprite displayed behind the chest
   - **Reward Item Sprite**: The reward item sprite displayed after the chest opens

### 1.2 Create Main Boss Reward Data

1. Right-click → **Create** → **Map Selection** → **Reward Data**
2. Name it something like `RewardData_MainBoss_Map01`
3. Assign the same sprite fields (these should be different from miniboss rewards)

**Note**: You can create different reward data assets for each map, or reuse the same ones across multiple maps.

---

## Step 2: Assign Rewards to Map Data

1. Select your **MapData** asset (e.g., `MapData_Map01`)
2. In the Inspector, scroll to the **Rewards** section
3. Assign:
   - **Miniboss Reward**: Drag your miniboss reward data asset here
   - **Main Boss Reward**: Drag your main boss reward data asset here

**Repeat for each map** that needs rewards.

---

## Step 3: Set Up VictoryDefeatUI GameObject

### 3.1 Locate VictoryDefeatUI

1. In your combat scene hierarchy, find the **VictoryDefeatUI** GameObject
2. Select it to view its Inspector

### 3.2 Assign Reward Chest UI Reference

1. In the **Reward System** section, find the **Reward Chest UI** field
2. You need to create or locate the RewardChestUI component:

   **Option A: If RewardChestUI already exists:**

   - Drag the GameObject with the RewardChestUI component into this field

   **Option B: If RewardChestUI doesn't exist yet:**

   - Create a new GameObject as a child of VictoryDefeatUI (or in the UI hierarchy)
   - Name it `RewardChestUI`
   - Add the `RewardChestUI` component to it
   - Set up the UI elements (see Step 4)

---

## Step 4: Set Up RewardChestUI GameObject

### 4.1 Create UI Structure

Create the following UI hierarchy under your RewardChestUI GameObject:

```
RewardChestUI (GameObject with RewardChestUI component)
├── RewardPanel (RectTransform - full screen, for click detection)
│   ├── GlowImage (Image component)
│   ├── ChestImage (Image component)
│   └── RewardItemImage (Image component)
```

### 4.2 Configure RewardChestUI Component

1. Select the **RewardChestUI** GameObject
2. In the Inspector, assign references:

   **Chest References:**

   - **Chest Image**: Drag the `ChestImage` GameObject here
   - **Glow Image**: Drag the `GlowImage` GameObject here
   - **Reward Item Image**: Drag the `RewardItemImage` GameObject here
   - **Reward Panel**: Drag the `RewardPanel` GameObject here

   **Animation Settings (Optional - defaults are fine):**

   - Adjust animation durations if needed
   - **Chest Fade In Duration**: 0.5s (default)
   - **Glow Pulse Duration**: 1.0s (default)
   - **Shake Intensity**: 10 (default)
   - **Shake Duration**: 0.3s (default)
   - **Chest Transition Duration**: 0.5s (default)
   - **Chest Disappear Duration**: 0.5s (default)
   - **Reward Fade In Duration**: 0.5s (default)
   - **Disappear Scale**: 0.3 (30% size when disappearing)

### 4.3 Configure UI Elements

**GlowImage:**

- Set Image component to display the glow sprite (will be set at runtime)
- Position it behind where the chest will appear
- Set initial alpha to 0 (will be animated)

**ChestImage:**

- Set Image component to display chest sprite (will be set at runtime)
- Position it in the center of the screen
- Set initial alpha to 0 (will be animated)

**RewardItemImage:**

- Set Image component to display reward item sprite (will be set at runtime)
- Position it where you want the reward to appear (usually same position as chest)
- Set initial alpha to 0 (will be animated)

**RewardPanel:**

- Add an `EventTrigger` component (automatically added by RewardChestUI)
- Make sure it covers the full screen for click detection
- Set `Raycast Target` to true on the Image component (if it has one)

---

## Step 5: Test the System

### 5.1 Quick Test Checklist

1. ✅ MapData has reward data assigned (miniboss and main boss)
2. ✅ VictoryDefeatUI has RewardChestUI reference assigned
3. ✅ RewardChestUI has all UI element references assigned
4. ✅ Reward sprites are assigned in RewardData assets

### 5.2 Testing Flow

1. Start combat in a map with rewards configured
2. Defeat the first enemy (miniboss)
3. **Expected**: Victory banner appears → Continue button → Click continue → Reward chest appears with glow
4. Click the chest (or anywhere on screen)
5. **Expected**: Chest shakes → Transitions to open sprite → Disappears → Reward item appears
6. After reward animation, next enemy should spawn
7. Defeat the second enemy (main boss)
8. **Expected**: Same reward flow, then final victory → Map selection

---

## Troubleshooting

### Issue: Reward chest doesn't appear

**Check:**

- Is RewardChestUI reference assigned in VictoryDefeatUI?
- Does the MapData have reward data assigned?
- Check console for errors

### Issue: Chest doesn't respond to clicks

**Check:**

- Is RewardPanel set up with EventTrigger?
- Does RewardPanel cover the screen area?
- Is `Raycast Target` enabled on the panel?

### Issue: Sprites don't appear

**Check:**

- Are sprites assigned in RewardData assets?
- Are Image components assigned in RewardChestUI?
- Check that Image components have `Raycast Target` enabled if needed

### Issue: Animation doesn't play

**Check:**

- Is DOTween imported and set up in the project?
- Check console for DOTween errors
- Verify animation duration settings are reasonable (> 0)

---

## Advanced Configuration

### Custom Animation Timing

You can adjust animation timings in the RewardChestUI Inspector:

- Faster animations: Reduce duration values
- Slower animations: Increase duration values
- More dramatic shake: Increase shake intensity

### Different Chest Types Per Map

Create multiple RewardData assets with different chest sprites:

- `RewardData_Miniboss_Forest` (forest-themed chest)
- `RewardData_Miniboss_Desert` (desert-themed chest)
- Assign appropriate rewards to each map's MapData

### Reward Item Positioning

Adjust the RewardItemImage position in the scene to control where the reward appears after the chest opens.

---

## File Structure Reference

```
Assets/
├── ScriptableObjects/
│   ├── Maps/
│   │   ├── MapData_Map01.asset
│   │   └── MapData_Map02.asset
│   └── Rewards/
│       ├── RewardData_Miniboss_Map01.asset
│       ├── RewardData_MainBoss_Map01.asset
│       ├── RewardData_Miniboss_Map02.asset
│       └── RewardData_MainBoss_Map02.asset
└── Scenes/
    └── Combat/
        └── VictoryDefeatUI (GameObject)
            └── RewardChestUI (GameObject)
```

---

## Summary

The reward system is now set up! The flow is:

1. **Enemy Defeated** → EnemySystem triggers victory
2. **Victory Banner** → VictoryDefeatUI shows banner with continue button
3. **Continue Clicked** → Reward chest appears (if reward data exists)
4. **Chest Clicked** → Animation plays (shake → open → disappear → reward)
5. **Reward Collected** → Next enemy spawns OR final victory → map selection

Each map can have unique rewards by creating different RewardData assets and assigning them to the MapData.
