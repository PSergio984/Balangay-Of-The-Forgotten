# Card-Based Preset Selection - Quick Integration Guide

## ✅ What Was Added

Your existing card system (`CharacterCard`, `CharacterCardVisual`, `HorizontalCharacterCardHolder`) now integrates with build preset selection!

### New Scripts Created:

1. **PresetSelectionUI.cs** - Overlay UI that appears when clicking a card
2. **CardPresetManager.cs** - Connects card clicks to preset selection
3. **CardSelectionSceneManager.cs** - Example scene manager for combat transitions

### Modified Scripts:

1. **CharacterCardVisual.cs** - Added `UpdatePresetData()` method and `buildPresetText` field
2. **CharacterCard.cs** - Added `SelectedPreset` field to track the selected build preset for each card

---

## 🎯 How It Works

```
1. Player clicks CharacterCard
   ↓
2. CardPresetManager catches SelectEvent
   ↓
3. Opens PresetSelectionUI overlay showing hero's 3 presets
   ↓
4. Player clicks a preset
   ↓
5. Card updates to show "Build Name" and preset stats (HP, etc.)
   ↓
6. Player clicks "Start Combat" button
   ↓
7. CardPresetManager.PrepareTransitionData() collects all card-preset pairs
   ↓
8. Scene transitions to Combat with CharacterTransitionData populated
```

---

## 🔧 Unity Setup (5 Minutes)

### Step 1: Update CharacterCardVisual Prefab

1. Open your **CharacterCardVisual** prefab
2. Add **UI → Text - TextMeshPro** below HealthText
3. Name it: `BuildPresetText`
4. Set text: "No Build"
5. Select CharacterCardVisual component
6. Assign `BuildPresetText` to **Build Preset Text** field
7. Save prefab

### Step 2: Create Preset Selection Overlay

In your character selection scene:

1. Under Canvas, add **UI → Panel**
2. Name: `PresetSelectionPanel`
3. Stretch to full screen (anchors 0,0 to 1,1)
4. Set color: Semi-transparent black (#000000AA)
5. **Disable** the GameObject
6. Add **PresetSelectionUI** component

Inside `PresetSelectionPanel`:

- Add **TitleText** (TMP) - "Select Build for: Hero"
- Add 3 **Buttons** (Preset1Button, Preset2Button, Preset3Button)
  - Each with **Image** child (PresetImage)
- Add **CurrentBuildText** (TMP) - "Current: None"
- Add **BackButton** (Button) - "Back"

### Step 3: Configure PresetSelectionUI Component

Select `PresetSelectionPanel`, assign references:

- Panel Root: PresetSelectionPanel itself
- Title Text: TitleText
- Preset Images[3]: The 3 PresetImage components
- Preset Buttons[3]: The 3 Preset buttons
- Back Button: BackButton
- Current Build Text: CurrentBuildText

### Step 4: Add CardPresetManager

1. Create empty GameObject: `CardPresetManager`
2. Add **CardPresetManager** component
3. Assign:
   - Card Holder: Your HorizontalCharacterCardHolder
   - Preset Selection UI: PresetSelectionPanel
   - Transition Data: Your CharacterTransitionData asset
   - Auto Wire Cards: ✓
   - Enable Card Click To Open Presets: ✓

**Public Method:**

`public void ShowPresetSelectionForCard(CharacterCard card)`

- **Parameter:** `card` (the `CharacterCard` to open the preset selection UI for)
- **Effect:** Opens the preset selection overlay for the given card, allowing the user to pick a build preset.
- **Usage Example:**
  ```csharp
  CardPresetManager manager = FindObjectOfType<CardPresetManager>();
  manager.ShowPresetSelectionForCard(someCard);
  ```

### Step 5: (Optional) Add Scene Manager

For easy testing:

1. Create GameObject: `SceneManager`
2. Add **CardSelectionSceneManager** component
3. Assign:
   - Card Preset Manager: CardPresetManager
   - Start Combat Button: Your "Start" button
   - Feedback Text: A TMP_Text for messages
   - Combat Scene Name: "Combat"

---

## 🎮 Testing

1. **Press Play**
2. Cards should appear (via your existing spawn system)
3. **Click a card** → Preset selection overlay appears
4. **Click a preset** → Card updates with build name
5. **Click Back** → Overlay closes
6. **Repeat** for other cards
7. **Click Start Combat** → Transitions to combat scene

---

## 📝 Code Examples

### Getting Selected Preset for a Card

> **Note:** The `SelectedPreset` field is automatically added to each `CharacterCard` and tracks the currently selected preset for that card instance.

The selected preset is stored on the `CharacterCard` instance:

```csharp
CharacterBuildPreset preset = card.SelectedPreset;
if (preset != null)
{
   Debug.Log($"Card has preset: {preset.PresetName}");
}
```

> **Note:** There is no static lookup method; each card tracks its own selected preset via the `SelectedPreset` field. Access this field directly to get the current selection for any card.

### Manual Scene Transition

```csharp
// Collect selections
CardPresetManager manager = FindObjectOfType<CardPresetManager>();
int count = manager.PrepareTransitionData();

if (count > 0)
{
    SceneManager.LoadScene("Combat");
}
```

### Opening Preset UI Manually

```csharp
CardPresetManager manager = FindObjectOfType<CardPresetManager>();
manager.ShowPresetSelectionForCard(someCard);
```

---

## 🔍 Data Flow

### Before Combat:

```
HeroData → CharacterCard → Player clicks → PresetSelectionUI
                                              ↓
                                    CharacterBuildPreset selected
                                              ↓
                          CardPresetManager.PrepareTransitionData()
                                              ↓
                          CharacterTransitionData (4 slots with Hero+Preset)
```

### In Combat:

```
MatchSetupSystem reads CharacterTransitionData.CharacterSlots
    ↓
CharacterSlotData[i].GetFinalStats() returns preset's FIXED stats
    ↓
Combat uses preset stats (hero base stats ignored)
```

---

## 🐛 Troubleshooting

**Cards not responding:**

- Check CardPresetManager has Card Holder assigned
- Verify cards exist in cardHolder.characterCards list

**Preset UI not showing:**

- Verify hero has BuildPresets assigned (3 recommended)
- Check PresetSelectionPanel starts disabled
- Ensure all PresetSelectionUI references assigned

**Card not updating:**

- Check BuildPresetText is assigned in CharacterCardVisual
- Verify preset has PresetName and PresetSprite

**Stats wrong in combat:**

- Ensure MatchSetupSystem uses CharacterSlotData.GetFinalStats()
- Verify presets use FIXED stats (not modifiers)

---

## ✅ Checklist

- [ ] BuildPresetText added to CharacterCardVisual prefab
- [ ] PresetSelectionPanel created with all UI elements
- [ ] PresetSelectionUI component fully configured
- [ ] CardPresetManager in scene with references assigned
- [ ] Heroes have BuildPresets assigned (3 per hero)
- [ ] Tested: Click card opens preset UI
- [ ] Tested: Select preset updates card
- [ ] Tested: Can transition to combat with selections

---

## 📚 Related Files

- **UI_SETUP_GUIDE.md** - Detailed UI setup instructions
- **CORRECTED_ARCHITECTURE.md** - System architecture and design decisions
- **IMPLEMENTATION_SUMMARY.md** - Complete system overview
- **QUICK_START.md** - Minimal setup steps

---

**You're ready to go! Your card system now supports build preset selection. 🎉**
