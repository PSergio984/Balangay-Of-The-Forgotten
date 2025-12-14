# Card Cooldown System Setup Guide

## Overview

The card cooldown system has been implemented and is ready to use. This guide explains how to configure the visual UI elements for the cooldown display.

## System Components

### 1. CardData.cs (ScriptableObject)

- Added `Cooldown` property (0-10 range)
- 0 = No cooldown (card can be played every turn)
- 1+ = Number of turns to wait after playing before the card can be used again

### 2. Card.cs (Runtime Model)

- `BaseCooldown` - The cooldown value from CardData
- `CurrentCooldown` - Runtime countdown (decreases each turn)
- `IsOnCooldown` - True if CurrentCooldown > 0
- `StartCooldown()` - Called when card is played
- `ReduceCooldown()` - Called at start of each player turn

### 3. CooldownSystem.cs (Singleton)

- Automatically triggers cooldowns when cards are played
- Automatically reduces cooldowns at the start of each player turn (after enemy turn ends)
- Fires events for UI updates

### 4. CardView.cs (Visual Component)

- Shows/hides cooldown overlay when card is on cooldown
- Displays remaining cooldown number
- Applies grey tint to cooldown cards
- Blocks interaction (drag/click) when on cooldown

---

## Setting Up Cooldown UI in CardView Prefab

### Step 1: Open the CardView Prefab

1. Navigate to `Assets/Prefabs/Combat/`
2. Double-click `CardView.prefab` to open it in Prefab Edit mode

### Step 2: Create Cooldown Overlay

1. Right-click on `Wrapper` in the hierarchy
2. Select `2D Object > Sprite`
3. Rename it to `CooldownOverlay`
4. Position it to cover the card (Position: 0, 0, 0)
5. Scale it to match card size (approximately Scale: 0.3, 0.3, 0.3 to match other elements)
6. Set the sprite to a semi-transparent dark sprite (or create one)
7. Set the Sorting Order to 10 (to appear above other card elements)
8. Set the Color alpha to about 0.5 for semi-transparency

### Step 3: Create Cooldown Badge

1. Right-click on `Wrapper` in the hierarchy
2. Select `2D Object > Sprite`
3. Rename it to `CooldownBadge`
4. Position it in the center of the card (Position: 0, 0, 0)
5. Scale appropriately (Scale: 0.2, 0.2, 0.2)
6. Set sprite to a circular badge or icon
7. Set Sorting Order to 11 (above overlay)

### Step 4: Create Cooldown Text

1. Right-click on `Wrapper` in the hierarchy
2. Select `UI > Text - TextMeshPro` or `3D Object > Text - TextMeshPro`
3. Rename it to `CooldownText`
4. Position it in the center of the badge (Position: 0, 0, -0.1)
5. Set Font Size to match card text
6. Set text alignment to Center/Middle
7. Set Sorting Order to 12 (above badge)

### Step 5: Assign References in CardView Component

1. Select the root `CardView` GameObject
2. In the Inspector, find the `CardView` component
3. Under the "Cooldown UI" header:
   - Drag `CooldownOverlay` to the `Cooldown Overlay` field
   - Drag `CooldownBadge` to the `Cooldown Badge` field
   - Drag `CooldownText` to the `Cooldown Text` field
4. Optionally adjust `Cooldown Tint Color` (default is grey: 0.5, 0.5, 0.5, 1)

### Step 6: Save the Prefab

1. Press Ctrl+S or click `File > Save`
2. Exit Prefab Edit mode

---

## Testing the Cooldown System

### Quick Test Setup

Two cards have been configured with cooldowns for testing:

- `ArmorTest.asset` - Cooldown: 2 turns
- `Heavy Attack.asset` (Mandirigma) - Cooldown: 3 turns

### Test Steps

1. Start Play mode
2. Play the Heavy Attack card
3. Observe:
   - Card should be greyed out
   - Cooldown overlay should appear
   - Card should not be draggable/clickable
   - Cooldown number should display
4. End your turn
5. Observe cooldown number decrease
6. After enough turns, card should become playable again

---

## Adding Cooldown to Other Cards

### In Unity Editor

1. Select a CardData asset (e.g., `Assets/Data/Cards/Mandirigma/Attack.asset`)
2. In the Inspector, find the "Cooldown (Rounds)" field under "Basic Info"
3. Set the value (0-10)
4. Save the asset

### In YAML (for batch editing)

Add this line after the Description field:

```yaml
<Cooldown>k__BackingField: 2
```

---

## How the System Works

### When a Card is Played

1. `PlayCardsGA` is performed
2. `CooldownSystem` receives POST reaction
3. If card has `BaseCooldown > 0`:
   - `Card.StartCooldown()` is called
   - `CurrentCooldown` is set to `BaseCooldown`
   - `CooldownSystem.OnCooldownChanged` event fires
   - `CardView.UpdateCooldownDisplay()` shows the overlay

### When Player Turn Starts (After Enemy Turn)

1. `EnemyTurnGA` completes
2. `CooldownSystem` receives POST reaction
3. Creates `ReduceCooldownGA` action
4. All cards have `ReduceCooldown()` called
5. `CurrentCooldown` decreases by 1
6. When `CurrentCooldown` reaches 0:
   - `IsOnCooldown` returns false
   - Card becomes playable again
   - Visual overlay hides

---

## Troubleshooting

### Card doesn't show cooldown UI

- Verify UI elements are assigned in CardView component
- Check that the cooldown sprites/text are set to appropriate sorting orders
- Ensure the card's CardData has Cooldown > 0

### Cards with cooldown are still playable

- Verify CooldownSystem GameObject exists in the scene (under `--- SYSTEMS ---`)
- Check that CardView's `CanPlay()` method is being called
- Look for errors in console related to CooldownSystem

### Cooldown doesn't decrease

- Verify ReduceCooldownGA performer is registered (check CooldownSystem.OnEnable)
- Ensure EnemyTurnGA is completing properly
- Check console for "[CooldownSystem] Reduced cooldown" messages

---

## Files Modified/Created

### New Files

- `Assets/Scripts/Combat/GameActions/ReduceCooldownGA.cs`
- `Assets/Scripts/Combat/System/CooldownSystem.cs`

### Modified Files

- `Assets/Scripts/Combat/Data/CardData.cs` - Added Cooldown property
- `Assets/Scripts/Combat/Models/Card.cs` - Added cooldown tracking
- `Assets/Scripts/Combat/Views/CardView.cs` - Added cooldown UI and interaction blocking

### Scene Changes

- `Assets/Scenes/Combat.unity` - Added CooldownSystem GameObject
