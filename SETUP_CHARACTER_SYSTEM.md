**Status: Ready-to-implement. This guide matches the current implementation branch.**

# Character Preset System - Setup & Testing Guide

## ⚠️ Critical: Fixed Stats Architecture

**IMPORTANT:** Build presets use **FIXED stats** that completely **override** hero base stats (NOT additive modifiers).

- When you create a preset, you set the exact final stats (e.g., HP=650, ATK=85)
- Hero base stats are only used as templates for creating presets
- In combat, only the preset's stats are used - hero base stats are ignored

## ✅ Implementation Complete

The dynamic character preset selection system has been successfully implemented! Here's what was created:

### Created Files

1. **CharacterTransitionData.cs** - ScriptableObject for passing selected heroes between scenes
2. **CharacterSelectionManager.cs** - Main manager for card generation, selection logic, and UI interaction
   - **Responsibilities:**
     - Dynamically generates character cards from available HeroData
     - Maintains the current selection state and enforces selection limits
     - Handles user interactions (card clicks, confirm/cancel, etc.)
   - **Key Public Methods:**
     - `void Initialize(IEnumerable<HeroData> heroes)` — Populate cards from hero data
     - `void ConfirmSelection()` — Finalize selection and write to CharacterTransitionData
     - `void CancelSelection()` — Reset or cancel the current selection
     - `IEnumerable<HeroData> GetSelectedHeroes()` — Get the currently selected heroes
     - `event Action OnSelectionChanged` (optional) — Subscribe to selection changes
   - **Usage:**
     - Add CharacterSelectionManager to a GameObject in your scene
     - Assign required references in the Inspector (see Step 4 below)
     - Wire the Confirm button's OnClick to `ConfirmSelection()`
     - See [CORRECTED_ARCHITECTURE.md](CORRECTED_ARCHITECTURE.md) for full implementation details and code examples
3. **Modified CharacterCard.cs** - Added `BoundHeroData` field and `Initialize(HeroData)` method
4. **Modified CharacterCardVisual.cs** - Added character data display fields and `UpdateCharacterData()` method
5. **Modified HorizontalCharacterCardHolder.cs** - Added `autoSpawnOnStart` toggle for compatibility

---

## 🔧 Manual Setup Steps (In Unity Editor)

### Step 1: Create CharacterTransitionData Asset

1. In Unity, right-click in `Assets/Data/` folder
2. Select **Create → Data → Character Transition Data**
3. Name it `CharacterTransitionData`
4. This asset will be used to pass selected characters to the combat scene

### Step 2: Create Sample Hero Data Assets (if you don't have them)

1. Right-click in `Assets/Data/` folder → **Create → Data → Hero**
2. Create 3-5 hero assets (e.g., `Warrior`, `Mage`, `Archer`, `Rogue`, `Cleric`)
3. For each hero, fill in:
   - **Hero Name**: (e.g., "Warrior")
   - **Image**: Assign a character portrait sprite
   - **Health**: Set to 100-200
   - **Attack Power**: Set to 10-30
   - **Magic Power**: Set to 0-30
   - **Defense**: Set to 5-15
   - **Deck**: Assign at least 5 CardData assets

### Step 3: Update Character Card Prefab

Your character card prefab needs UI elements to display character data:

1. Open the **CharacterCard** prefab (likely in `Assets/Prefabs/CharacterSelection/`)
2. Find the **CharacterCardVisual** component hierarchy
3. Add these UI elements as children (if not already present):

   **A. Character Portrait Image:**

   - Add a new **Image** GameObject
   - Name it "CharacterPortrait"
   - Position it where you want the hero portrait to appear
   - Set the Image component to use a default portrait sprite

   **B. Character Name Text:**

   - Add a **TextMeshPro - Text** GameObject
   - Name it "CharacterNameText"
   - Position it below or above the portrait
   - Set default text: "Character Name"

   **C. Health Text (Optional):**

   - Add a **TextMeshPro - Text** GameObject
   - Name it "HealthText"
   - Position it at the bottom of the card
   - Set default text: "HP: 100"

4. Select the **CharacterCardVisual** component in the prefab
5. In the Inspector, find the **Character Data Display** section
6. Assign the UI elements you just created:

   - **Character Portrait** → Drag the CharacterPortrait Image
   - **Character Name Text** → Drag the CharacterNameText TMP_Text
   - **Health Text** → Drag the HealthText TMP_Text

7. Save the prefab

### Step 4: Setup Character Selection Scene

1. Open your **Character Selection Scene** (or create a new scene)

2. Create or find the **HorizontalCharacterCardHolder** GameObject

   - In the Inspector, set **Auto Spawn On Start** to `false`
   - This prevents it from spawning placeholder cards

3. Create a new **Empty GameObject** and name it `CharacterSelectionManager`
4. Add the **CharacterSelectionManager** component to it
5. Configure the CharacterSelectionManager in Inspector:

   - **Data Section:**
     - **Available Heroes**: Click the `+` button and assign your HeroData assets (Warrior, Mage, etc.)
     - **Transition Data**: Assign the `CharacterTransitionData` asset you created
   - **UI References:**
     - **Card Holder**: Assign the `HorizontalCharacterCardHolder` GameObject
     - **Character Card Prefab**: Assign your CharacterCard prefab
     - **Card Spawn Parent**: Assign the Transform where slots should spawn (usually the HorizontalCharacterCardHolder itself)
     - **Slot Prefab**: Assign the slot prefab (the one that contains CharacterCard as child)
   - **Selection Settings:**
     - **Min Selections**: 1 (minimum characters to select)
     - **Max Selections**: 3 (maximum characters to select)
     - **Combat Scene Name**: "Combat" (or your combat scene name)
   - **UI Feedback (Optional):**
     - **Confirm Button**: Assign a UI Button for confirming selection
     - **Selection Count Text**: Assign a TextMeshPro Text to show "Selected: X/3"

6. If you have a **Confirm Button**, add an OnClick event:
   - Click the `+` in the OnClick list
   - Drag the `CharacterSelectionManager` GameObject
   - Select function: `CharacterSelectionManager.ConfirmSelection()`

### Step 5: Update Combat Scene (MatchSetupSystem)

## 🛠️ Combat Integration (MatchSetupSystem)

Update your combat initialization script to use `CharacterSlotData[]` and apply preset modifiers:

```csharp
[SerializeField] private CharacterTransitionData characterTransitionData;

private void Start()
{
   if (!characterTransitionData.HasValidData())
   {
      Debug.LogError("[MatchSetupSystem] No character data!");
      return;
   }

   CharacterSlotData[] slots = characterTransitionData.GetCompleteSlots();

   for (int i = 0; i < slots.Length; i++)
   {
      SpawnHeroFromSlot(slots[i], i);
   }

#if UNITY_EDITOR
   characterTransitionData.Clear();
#endif
}

private void SpawnHeroFromSlot(CharacterSlotData slot, int spawnIndex)
{
   // CRITICAL: GetFinalStats returns preset's FIXED stats (overrides hero base stats)
   var (health, attack, magic, defense) = slot.GetFinalStats();
   Debug.Log($"[MatchSetupSystem] Spawning {slot.Hero.HeroName} with {slot.SelectedPreset.PresetName}");
   Debug.Log($"  Final Stats (from preset): HP={health}, ATK={attack}, MAG={magic}, DEF={defense}");
   // TODO: Instantiate hero prefab, apply preset stats (health, attack, magic, defense)
   // DO NOT use slot.Hero.Health, slot.Hero.AttackPower, etc. - those are ignored!
   // Assign deck from slot.Hero.Deck, set position by spawnIndex
}
```

> **Backward Compatibility:**
> If you must support the old API (`SelectedHeroes`), mark it as deprecated. You can map from slots to heroes like this:
>
> ```csharp
> // Deprecated: Only use if you have legacy code
> List<HeroData> selectedHeroes = characterTransitionData.GetCompleteSlots()
>     .Where(slot => slot != null && slot.Hero != null)
>     .Select(slot => slot.Hero)
>     .ToList();
> ```
>
> Prefer using `CharacterSlotData[]` for full preset/build support.

---

## 🧪 Testing the System

### Test 1: Card Generation

1. Play the Character Selection scene
2. **Expected:** Character cards should spawn for each available hero, displaying:
   - Hero portraits
   - Hero names
   - Health and other stats (if configured)
   - Build preset options (if present)
3. Click a slot button to open the preset selection UI for that slot.
4. Select a build preset for the hero in that slot.
5. The slot image and build name should update to reflect your selection.
6. Repeat for all 4 slots.
7. Click Save to confirm your selections.

### Test 2: Data Transfer to Combat Scene

1. Enter Play Mode and select builds for all 4 slots.
2. Click Save/Confirm to transition to the combat scene.
3. In the combat scene, verify that the correct heroes and builds are loaded from the CharacterTransitionData asset.
4. Each player combatant should match the selected hero and build preset from the selection scene.

### Troubleshooting

- If cards do not appear, check that your CharacterSelectionManager is configured with the correct HeroData assets and UI references.
- If build presets are missing, ensure each HeroData asset has at least one CharacterBuildPreset assigned.
- If the combat scene loads default heroes, verify that CharacterTransitionData is being written to and read from correctly.

---

5.  Set PresetName (e.g., "Glass Cannon Set")
6.  Set stat modifiers (can leave at 0 for testing)

Repeat for at least 1 hero with 1-3 presets.

### Step 2: Create HeroData Assets

1.  Right-click `Assets/Data/` → Create → Data → Hero
2.  Name: e.g., `TestWarrior`
3.  Fill in:
    - Hero Name: "Test Warrior"
    - Image: Any sprite
    - Health, Attack Power, Magic Power, Defense
    - Deck: Add at least 5 CardData assets
    - BuildPresets: Add your CharacterBuildPreset(s) from above

### Step 3: Create CharacterTransitionData Asset

1.  Right-click `Assets/Data/`
2.  Create → Data → Character Transition Data
3.  Name: `CharacterTransitionData`

### Step 4: Setup Main Selection UI

1.  Create a Canvas (if not present)
2.  Add 4 Buttons for slots (Slot1Button, Slot2Button, Slot3Button, Slot4Button)
3.  Each slot button should have an Image child (SlotImage) to display the selected preset's sprite
4.  Add a TMP_Text below slots for "Current Build: ..."
5.  Add a Save Button
6.  Add a GameObject for PresetSelectionUI (see CORRECTED_ARCHITECTURE.md for structure)

### Step 5: Link UI to Scripts

1.  Add MainSelectionUI script to a GameObject
2.  Assign:
    - transitionData: CharacterTransitionData asset
    - slotImages: The 4 SlotImage components
    - currentBuildText: The TMP_Text for build name
    - saveButton: The Save Button
    - presetSelectionUI: Reference to your PresetSelectionUI script
3.  Add PresetSelectionUI script to its GameObject
4.  Assign:
    - presetImages: 3 Image components for preset options
    - presetButtons: 3 Buttons for selecting presets
    - titleText: TMP_Text for the title

---

---

## 📊 Data Flow Summary

```
[HeroData + BuildPresets]
   ↓
[MainSelectionUI: 4 slots]
   ↓
[PresetSelectionUI: 3 presets per hero]
   ↓
[CharacterTransitionData.CharacterSlots[4]]
   ↓
[MatchSetupSystem: reads slots, spawns heroes in order]
```

---

## Alternative Setup: 4-Slot UI Method

## 🚀 Next Steps

1.  Create 3-4 more heroes, each with 3 presets and sprites
2.  Polish UI (add animations, tooltips, etc.)
3.  Integrate with combat (see CORRECTED_ARCHITECTURE.md)

## 💡 Future Enhancements

- **Character Tooltips**: Show detailed stats on hover
- **Deck Preview**: Display sample cards from hero's deck
- **Character Classes**: Add class icons (Warrior, Mage, etc.)
- **Rarity/Tiers**: Different card backgrounds for rare heroes
- **Filters/Search**: Search bar for large hero rosters
- **Random Selection**: Button to randomly select heroes
- **Save Last Selection**: Remember player's last team composition

---

## ✅ System Benefits

✨ **Data-Driven**: Add new heroes by creating HeroData assets (no code changes)
✨ **Maintainable**: Clear separation of concerns (Manager → Cards → Visuals)
✨ **Scalable**: Supports unlimited heroes with minimal performance impact
✨ **Type-Safe**: Uses Unity's ScriptableObject pattern for safe data transfer
✨ **Editor-Friendly**: All configuration via Inspector, no hardcoding

---

**Implementation Status**: ✅ Complete - Ready for testing and integration!
