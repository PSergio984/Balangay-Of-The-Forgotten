# Balangay Progress & Rewards Systems Implementation Guide

This guide explains how to set up and integrate the new systems for map progress, relics, special cards, status effects, and enhanced dialogue in the Balangay Of The Forgotten project. Follow these steps to ensure correct asset creation, scene wiring, and game hierarchy integration.

---

## 1. Game Progress Manager (Map Completion & Kaluwalhatian Unlock)

### Purpose

Tracks which maps are completed, unlocks Kaluwalhatian after 3 maps, and persists progress across sessions.

### Key Script

- `GameProgressData.cs` (ScriptableObject, in `Assets/Scripts/SceneController/`)

### Setup Steps

1. **Create ScriptableObject Asset:**
   - In Unity Editor: Right-click in `Assets/Scriptables/` → Create → SceneController → Game Progress Data
   - Name: `GameProgressData`
2. **Assign to Managers:**
   - Reference this asset in `MapSelectManager2` and `VictoryDefeatUI` via Inspector.
3. **Usage:**
   - Call `MarkMapComplete(mapId)` after a map is finished.
   - Use `IsMapComplete(mapId)` and `IsKaluwalhatianUnlocked` for UI logic.

### Game Hierarchy

- No runtime GameObject needed; asset is referenced by managers.

---

## 2. Relics System (Decorative Trophies)

### Purpose

Collects and displays relics earned from main boss victories. Relics are persistent, decorative rewards.

### Key Scripts

- `RelicData.cs` (ScriptableObject, defines a relic)
- `RelicCollectionData.cs` (ScriptableObject, tracks collected relics)
- `RelicDisplayUI.cs` (MonoBehaviour, displays relics in combat UI)

### Setup Steps

1. **Create RelicData Assets:**
   - In `Assets/Scriptables/`, right-click → Create → RewardsManager → Relic Data
   - Create one asset per relic (e.g., `Relic_Dagat`, `Relic_Daragang`, ...)
2. **Create RelicCollectionData Asset:**
   - Right-click → Create → RewardsManager → Relic Collection Data
   - Name: `RelicCollectionData`
   - Add all `RelicData` assets to its list in Inspector.
3. **Assign to UI:**
   - Add `RelicDisplayUI` to your combat UI Canvas.
   - Assign `RelicCollectionData` in Inspector.
4. **Integration:**
   - `VictoryDefeatUI` calls `AddRelic()` on collection after boss victory.

### Game Hierarchy

- `RelicDisplayUI` under Combat UI Canvas
- ScriptableObjects in `Assets/Scriptables/`

---

## 3. Special Cards System (Mini-Boss Buff Rewards)

### Purpose

Awards special cards from mini-bosses. Cards grant team-wide buffs (DamageUp, DefenseUpTwoTargets, NoCooldown) and persist if unused.

### Key Scripts

- `SpecialCardData.cs` (ScriptableObject, defines a card)
- `SpecialCardCollectionData.cs` (ScriptableObject, tracks owned/used cards)
- `SpecialCardPanelUI.cs` (MonoBehaviour, panel for using cards)
- `SpecialCardButtonUI.cs` (MonoBehaviour, button for each card)

### Setup Steps

1. **Create SpecialCardData Assets:**
   - In `Assets/Scriptables/`, right-click → Create → RewardsManager → Special Card Data
   - Create one asset per card (e.g., `Card_DamageUp`, `Card_DefenseUp`, `Card_NoCooldown`)
   - Set effect type and icon in Inspector.
2. **Create SpecialCardCollectionData Asset:**
   - Right-click → Create → RewardsManager → Special Card Collection Data
   - Name: `SpecialCardCollectionData`
   - Add all `SpecialCardData` assets to its list in Inspector.
3. **Assign to UI:**
   - Add `SpecialCardPanelUI` to your combat UI Canvas.
   - Assign `SpecialCardCollectionData` in Inspector.
4. **Integration:**
   - `VictoryDefeatUI` calls `AddSpecialCard()` after mini-boss victory.
   - `SpecialCardPanelUI` displays and lets player use available cards.

### Game Hierarchy

- `SpecialCardPanelUI` under Combat UI Canvas
- ScriptableObjects in `Assets/Scriptables/`

---

## 4. NoCooldown Status Effect (Special Card Effect)

### Purpose

Implements the NO_COOLDOWN status effect, which disables cooldowns for a duration (from Bundok Pulag special card).

### Key Scripts

- `ApplyNoCooldownGA.cs` (GameAction, applies effect)
- `NoCooldownSystem.cs` (handles effect logic)
- `StatusEffectType.cs` (enum, includes NO_COOLDOWN)
- `CooldownSystem.cs` (checks for NO_COOLDOWN)

### Setup Steps

1. **Ensure Scripts Exist:**
   - All scripts are in `Assets/Scripts/Combat/` subfolders.
2. **GameAction Integration:**
   - `SpecialCardPanelUI` triggers `ApplyNoCooldownGA` when card is used.
3. **Effect Logic:**
   - `NoCooldownSystem` manages duration and removal.
   - `CooldownSystem` checks `HasAnyNoCooldown()` before applying cooldowns.

### Game Hierarchy

- No special GameObject needed; logic is handled by combatant systems.

---

## 5. Dialogue System Enhancement (Image Overlays)

### Purpose

Adds support for image overlays in dialogue (intro, post-victory, post-final-boss scenes).

### Key Scripts

- `DialogueManager.cs` (handles overlays, fade animations)
- `DialogueTrigger.cs` (sets up dialogue type and overlay images)
- `DialogueLine` (data structure, now includes overlay image/duration)

### Setup Steps

1. **Update Dialogue Assets:**
   - Add overlay image, duration, and fade settings to dialogue lines as needed.
2. **Scene UI:**
   - Ensure Dialogue UI Canvas has an Image overlay object referenced by `DialogueManager`.
3. **Integration:**
   - `DialogueTrigger` sets dialogue type and overlay image.
   - `DialogueManager` handles fade-in/out and display.

### Game Hierarchy

- Dialogue UI Canvas with overlay Image
- `DialogueManager` and `DialogueTrigger` on relevant scene objects

---

## 6. UI Integration (VictoryDefeatUI, Relic/SpecialCard Panels)

### Purpose

Connects all systems to the main combat and map selection UI.

### Key Scripts

- `VictoryDefeatUI.cs` (handles reward collection, progress marking)
- `MapSelectManager2.cs` (shows map completion, unlocks Kaluwalhatian)
- `MapButton.cs` (shows completion indicator)

### Setup Steps

1. **VictoryDefeatUI:**
   - Assign `GameProgressData`, `RelicCollectionData`, and `SpecialCardCollectionData` in Inspector.
   - Ensure reward chests call `ProcessRewardCollection()`.
2. **MapSelectManager2:**
   - Assign `GameProgressData` in Inspector.
   - Use `LoadUnlockedLevels()` and `SetupMapButtons()` for progress display.
3. **MapButton:**
   - Use new `Setup()` overload to show completion indicator.

### Game Hierarchy

- `VictoryDefeatUI` on combat UI Canvas
- `MapSelectManager2` on map selection scene root
- `MapButton` on each map button prefab

---

## Testing & Validation

- Play through a map, defeat bosses/mini-bosses, and verify relics/special cards are awarded and persist.
- Complete 3 maps and check Kaluwalhatian unlocks.
- Use special cards in combat and verify effects.
- Trigger dialogue scenes and confirm overlay images display/fade correctly.
- Check all UI panels update as expected.

---

## Notes

- All ScriptableObject assets should be created in `Assets/Scriptables/`.
- Assign all references via Inspector to avoid null errors.
- For further details, see `PROGRESS_REWARDS_IMPLEMENTATION.md` and code comments in each script.

---

**For questions or troubleshooting, consult the code comments or reach out to the project maintainers.**
