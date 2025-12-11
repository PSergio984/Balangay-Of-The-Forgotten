# Implementation Summary: Progress, Rewards, and Dialogue Systems

This document summarizes the implementation of the five major systems for the Balangay Of The Forgotten turn-based combat game.

## Overview

All systems have been implemented following the existing codebase patterns (ActionSystem/GameAction, ScriptableObject data, Singleton managers).

---

## System 1: Character Selection Persistence & Order Preservation

**Status:** Already implemented in the existing `CharacterTransitionData.cs` ScriptableObject.

**Files Used:**

- [CharacterTransitionData.cs](Assets/Scripts/SceneController/CharacterTransitionData.cs) - Already persists selected characters across scenes

---

## System 2: Progress Manager for Map Completion Tracking

**Files Created/Modified:**

### [GameProgressData.cs](Assets/Scripts/SceneController/GameProgressData.cs) (NEW)

ScriptableObject that tracks:

- Completed map IDs (HashSet for O(1) lookup)
- Kaluwalhatian unlock status (requires 3 maps completed)
- Game completion status (Kaluwalhatian beaten)
- PlayerPrefs persistence with Save()/Load() methods

**Key Constants:**

```csharp
public const string MAP_ID_DAGAT = "Dagat_ng_Kabisayaan";
public const string MAP_ID_DARAGANG = "Daragang_Magayon";
public const string MAP_ID_BUNDOK = "Bundok_Pulag";
public const string MAP_ID_KALUWALHATIAN = "Kaluwalhatian";
public const int MAPS_REQUIRED_FOR_FINAL = 3;
```

**Key Methods:**

- `MarkMapComplete(string mapId)` - Marks a map as complete
- `IsMapComplete(string mapId)` - Checks if a map is completed
- `CheckKaluwalhatianUnlock()` - Checks if 3 maps are done → unlocks Kaluwalhatian

### [MapSelectManager2.cs](Assets/Scripts/MapSelection/MapSelectManager2.cs) (MODIFIED)

- Added `GameProgressData _gameProgress` field
- Modified `LoadUnlockedLevels()` to:
  - Load progress data at startup
  - Unlock Kaluwalhatian when 3 maps are complete
- Modified `SetupMapButtons()` to pass completion state to buttons
- Added `IsMapCompleted(string mapId)` helper method

### [MapButton.cs](Assets/Scripts/MapSelection/MapButton.cs) (MODIFIED)

- Added `IsCompleted` property
- Added `completionIndicator` GameObject field (optional checkmark)
- Added `completedColor` field (default green tint)
- New overload: `Setup(MapData map, bool isUnlocked, bool isCompleted)`

---

## System 3: Relics System (Decorative Trophies)

**Files Created:**

### [RelicData.cs](Assets/Scripts/RewardsManager/RelicData.cs) (NEW)

ScriptableObject defining a single relic:

- `relicId` - Unique identifier
- `relicName` - Display name
- `relicSprite` - Icon sprite
- `description` - Flavor text
- `sourceMapId` - Which map drops this relic

### [RelicCollectionData.cs](Assets/Scripts/RewardsManager/RelicCollectionData.cs) (NEW)

ScriptableObject tracking all collected relics:

- PlayerPrefs persistence
- `AddRelic(RelicData relic)` - Adds to collection
- `HasRelic(string relicId)` - Check if owned
- `GetAllRelics()` - Returns all collected relics

### [RelicDisplayUI.cs](Assets/Scripts/Combat/UI/RelicDisplayUI.cs) (NEW)

MonoBehaviour for combat scene:

- Displays collected relics as decorative icons
- Auto-hides if no relics collected
- Refreshes on enable

### [RewardData.cs](Assets/Scripts/MapSelection/RewardData.cs) (MODIFIED)

- Added `AssociatedRelic` field for main boss rewards

---

## System 4: Special Cards System (Mini-Boss Buff Rewards)

**Files Created:**

### [SpecialCardData.cs](Assets/Scripts/RewardsManager/SpecialCardData.cs) (NEW)

ScriptableObject defining a special card:

- `cardId`, `cardName`, `cardSprite`, `description`
- `effectType` - Enum: DamageUp, DefenseUpTwoTargets, NoCooldown
- `effectPercentage` - Bonus amount (e.g., 15 for +15%)
- `duration` - How many turns/rounds the effect lasts
- `targetCount` - For multi-target effects

**Effect Types:**

```csharp
public enum SpecialCardEffectType
{
    DamageUp,              // +15% DMG to all players for 2 turns
    DefenseUpTwoTargets,   // +25% DEF to 2 allies for 1 turn
    NoCooldown             // No cooldowns for 4 rounds
}
```

### [SpecialCardCollectionData.cs](Assets/Scripts/RewardsManager/SpecialCardCollectionData.cs) (NEW)

ScriptableObject tracking collected special cards:

- Persist if unused, consumed when played
- `AddSpecialCard(SpecialCardData card)` - Adds to collection
- `UseSpecialCard(SpecialCardData card)` - Consumes (removes) card
- `GetAvailableCards()` - Returns unused cards

### [SpecialCardPanelUI.cs](Assets/Scripts/Combat/UI/SpecialCardPanelUI.cs) (NEW)

MonoBehaviour for combat scene:

- Displays available special cards as clickable buttons
- On click: applies effect via ActionSystem, consumes card
- Uses `ApplyDamageUpGA`, `ApplyDefenseUpGA`, `ApplyNoCooldownGA`

### [SpecialCardButtonUI.cs](Assets/Scripts/Combat/UI/SpecialCardButtonUI.cs) (NEW)

Individual card button with:

- Card icon, name, description
- Click handling
- Use animation (scale up, flash, fade out)

### [ApplyNoCooldownGA.cs](Assets/Scripts/Combat/GameActions/ApplyNoCooldownGA.cs) (NEW)

GameAction for applying NO_COOLDOWN status effect.

### [NoCooldownSystem.cs](Assets/Scripts/Combat/StatusEffects/NoCooldownSystem.cs) (NEW)

Singleton system that:

- Attaches performer for `ApplyNoCooldownGA`
- Tracks duration per combatant
- Provides `HasNoCooldown(instanceId)` and `HasAnyNoCooldown()` checks
- Ticks duration at turn end

### [StatusEffectType.cs](Assets/Scripts/Combat/Enums/StatusEffectType.cs) (MODIFIED)

- Added `NO_COOLDOWN` enum value

### [CooldownSystem.cs](Assets/Scripts/Combat/System/CooldownSystem.cs) (MODIFIED)

- Modified `OnCardPlayed()` to check `NoCooldownSystem.Instance.HasAnyNoCooldown()`
- If active, skips applying cooldown to played cards

### [RewardData.cs](Assets/Scripts/MapSelection/RewardData.cs) (MODIFIED)

- Added `SpecialCardReward` field for mini-boss rewards

---

## System 5: Enhanced Dialogue System with Image Overlays

**Files Modified:**

### [DialogueTrigger.cs](Assets/Scripts/MapSelection/DialogueTrigger.cs) (MODIFIED)

**DialogueLine additions:**

- `overlayImage` - Optional sprite to display as fullscreen overlay
- `overlayDuration` - How long to show (0 = until next line)
- `overlayFadeDuration` - Fade in/out time

**Dialogue additions:**

- `onCompleteEventName` - Optional callback name
- `dialogueType` - Enum for special handling

**New enum:**

```csharp
public enum DialogueType
{
    Normal,
    IntroScene,
    PostVictory,
    PostFinalBoss
}
```

### [DialogueManager.cs](Assets/Scripts/MapSelection/DialogueManager.cs) (MODIFIED)

- Added overlay image UI references
- Added `ShowOverlay()` / `HideOverlay()` with DOTween fade
- Handles overlay per dialogue line
- New overload: `StartDialogue(Dialogue dialogue, System.Action onComplete)`
- `HandleDialogueTypeCompletion()` for type-specific behaviors

---

## Integration: VictoryDefeatUI

### [VictoryDefeatUI.cs](Assets/Scripts/Combat/UI/VictoryDefeatUI.cs) (MODIFIED)

**New fields:**

- `RelicCollectionData relicCollection`
- `SpecialCardCollectionData specialCardCollection`
- `GameProgressData gameProgress`
- `LevelTransitionData levelTransitionData`

**New methods:**

- `ProcessRewardCollection(RewardData, bool isMiniBossReward)`:
  - If mini-boss reward → add to SpecialCardCollectionData
  - If main boss reward → add to RelicCollectionData
- `MarkCurrentMapComplete()`:
  - Gets current map from LevelTransitionData
  - Calls GameProgressData.MarkMapComplete()
  - Checks for Kaluwalhatian unlock

**Modified `OnRewardChestComplete()`:**

- Captures reward data before clearing
- Calls `ProcessRewardCollection()`
- Calls `MarkCurrentMapComplete()` when no more enemies

---

## Setup Instructions

### 1. Create ScriptableObject Assets

In Unity, create the following assets via right-click → Create:

1. **GameProgressData** (Data/Progress/GameProgressData)
2. **RelicCollectionData** (Data/Collections/RelicCollectionData)
3. **SpecialCardCollectionData** (Data/Collections/SpecialCardCollectionData)
4. **RelicData** for each main boss (Data/Relics/\*)
5. **SpecialCardData** for each mini-boss (Data/SpecialCards/\*)

### 2. Configure RewardData Assets

For each map's RewardData:

- **Mini-boss reward**: Assign `SpecialCardReward` field
- **Main boss reward**: Assign `AssociatedRelic` field

### 3. Wire Up VictoryDefeatUI

In the Combat scene's VictoryDefeatUI component:

- Assign `RelicCollectionData`
- Assign `SpecialCardCollectionData`
- Assign `GameProgressData`
- Assign `LevelTransitionData`

### 4. Wire Up MapSelectManager2

In the MapSelection scene:

- Assign `GameProgressData` to MapSelectManager2
- Optionally add `completionIndicator` GameObjects to MapButtons

### 5. Add UI Panels to Combat Scene

Add these components:

- `RelicDisplayUI` - Shows collected relics as trophies
- `SpecialCardPanelUI` - Shows available special cards

### 6. Configure Dialogue Overlays

In DialogueManager:

- Assign `overlayImage` (Image component)
- Assign `overlayCanvasGroup` (CanvasGroup for fading)
- Assign `overlayPanel` (container GameObject)

---

## File Summary

| File                         | Status   | Description                                    |
| ---------------------------- | -------- | ---------------------------------------------- |
| GameProgressData.cs          | NEW      | Map completion & Kaluwalhatian unlock tracking |
| RelicData.cs                 | NEW      | Relic definition SO                            |
| RelicCollectionData.cs       | NEW      | Collected relics with persistence              |
| RelicDisplayUI.cs            | NEW      | Combat scene relic display                     |
| SpecialCardData.cs           | NEW      | Special card definition with effect types      |
| SpecialCardCollectionData.cs | NEW      | Collected special cards with consume-on-use    |
| SpecialCardPanelUI.cs        | NEW      | Combat scene special card panel                |
| SpecialCardButtonUI.cs       | NEW      | Individual card button component               |
| ApplyNoCooldownGA.cs         | NEW      | GameAction for NO_COOLDOWN effect              |
| NoCooldownSystem.cs          | NEW      | Handles NO_COOLDOWN status with duration       |
| RewardData.cs                | MODIFIED | Added relic & special card reward fields       |
| StatusEffectType.cs          | MODIFIED | Added NO_COOLDOWN enum                         |
| CooldownSystem.cs            | MODIFIED | Checks NO_COOLDOWN before applying cooldown    |
| VictoryDefeatUI.cs           | MODIFIED | Integrated all collection systems              |
| MapSelectManager2.cs         | MODIFIED | Progress-based unlock & completion display     |
| MapButton.cs                 | MODIFIED | Completion indicator support                   |
| DialogueTrigger.cs           | MODIFIED | Image overlay & dialogue type support          |
| DialogueManager.cs           | MODIFIED | Overlay display with fade animations           |

---

## Testing Checklist

- [ ] Beat a mini-boss → Verify special card added to collection
- [ ] Beat a main boss → Verify relic added to collection
- [ ] Complete 3 maps → Verify Kaluwalhatian unlocks
- [ ] Use a special card → Verify effect applies and card consumed
- [ ] Return to map select → Verify completed maps show completion indicator
- [ ] Restart game → Verify progress persists via PlayerPrefs
- [ ] Test dialogue with overlay image → Verify image fades in/out
