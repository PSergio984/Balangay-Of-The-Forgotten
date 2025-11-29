# 🚀 Quick Start - Slot-Based Character Preset System

This is the absolute minimum to get the new 4-slot character preset selection system working.

---

## ⏱️ Step 1: Create Core Assets (2 minutes)

### A. Create CharacterBuildPreset Assets

For each hero you want to test:

1. Right-click `Assets/Data/`
2. Create → Combat → Character Build Preset
3. Name: e.g., `TestWarrior_GlassCannon`
4. Assign a test sprite (can be any image for now)
5. Set PresetName (e.g., "Glass Cannon Set")
6. Set stat modifiers (can leave at 0 for testing)

Repeat for at least 1 hero with 1-3 presets.

### B. Create Test Hero

1. Right-click `Assets/Data/`
2. Create → Data → Hero
3. Name: `TestWarrior`
4. Fill in:
   - Hero Name: "Test Warrior"
   - Image: Any sprite
   - Health: 100
   - Attack Power: 20
   - Magic Power: 10
   - Defense: 15
   - Deck: Add at least 5 CardData assets
   - BuildPresets: Add your CharacterBuildPreset(s) from above

### C. Create CharacterTransitionData Asset

1. Right-click `Assets/Data/`
2. Create → Data → Character Transition Data
3. Name: `CharacterTransitionData`

---

## ⏱️ Step 2: Setup UI Prefabs (1 minute)

1. Create a UI Canvas for selection (or use existing)
2. Add 4 Button objects for slots (Slot1Button, Slot2Button, Slot3Button, Slot4Button)
3. Each slot should have an Image child (SlotImage) to display the selected preset sprite
4. Add a TextMeshPro object below slots for "Current Build: ..."
5. Add a Save Button

---

## ⏱️ Step 3: Scene Setup (2 minutes)

1. Create an empty GameObject → Name: `MainSelectionUI`
2. Add your MainSelectionUI script (see CORRECTED_ARCHITECTURE.md for example)
3. Assign:

   - transitionData: CharacterTransitionData asset
   - slotImages: The 4 SlotImage components
   - currentBuildText: The TMP_Text for build name
   - saveButton: The Save Button
   - presetSelectionUI: Reference to your PresetSelectionUI script

4. Create another GameObject → Name: `PresetSelectionUI`
5. Add your PresetSelectionUI script (see CORRECTED_ARCHITECTURE.md for example)
6. Assign:
   - presetImages: 3 Image components for preset options
   - presetButtons: 3 Buttons for selecting presets
   - titleText: TMP_Text for the title

---

## ✅ Test It!

1. Press Play in Character Selection scene
2. **Expected**: 4 slots appear, clicking a slot shows preset options for the hero
3. Selecting a preset updates the slot image and "Current Build" text
4. Clicking Save transitions to combat (if hooked up)

**If it works, you're done! If not, see troubleshooting below.**

---

## 🐛 Quick Troubleshooting

### Slot images don't update

**Fix**: Check that slotImages are assigned in MainSelectionUI, and that preset sprites are set in CharacterBuildPreset assets.

### Preset selection UI doesn't appear

**Fix**: Ensure PresetSelectionUI is assigned in MainSelectionUI and is enabled/disabled correctly.

### Save doesn't work

**Fix**: Make sure Save Button is assigned and OnClick is hooked to MainSelectionUI.OnSaveClicked().

---

## 📈 Next Steps

1. Create 3-4 more heroes, each with 3 presets and sprites
2. Polish UI (add animations, tooltips, etc.)
3. Integrate with combat (see CORRECTED_ARCHITECTURE.md and MatchSetupSystem)

---

## 📚 Full Documentation

- **Full Setup**: `SETUP_CHARACTER_SYSTEM.md`
- **UI Details**: `UI_SETUP_GUIDE.md`
- **Architecture**: `CORRECTED_ARCHITECTURE.md`
- **Implementation**: `IMPLEMENTATION_SUMMARY.md`

---

**Time to Working System: ~5 minutes**
