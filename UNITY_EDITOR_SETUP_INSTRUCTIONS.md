# Unity Editor Setup Instructions

## Required Manual Setup for PresetSelectionUI

### Part 1: Wire Up Card Holder Reference

**CRITICAL:** PresetSelectionUI needs a reference to HorizontalCharacterCardHolder to hide/show cards.

1. **Select PresetSelectionUI** in the Hierarchy (`--- UI --- / Dynamic / PresetSelectionUI`)
2. **In the Inspector**, find the **PresetSelectionUI** component
3. **Locate the "Card Visibility Control" section**
4. **Drag the `HorizontalCharacterCardHolder` GameObject** from the Hierarchy into the **Card Holder** field
5. **Save the Scene**

---

### Part 1.5: Wire Up Visual Handler Reference

**CRITICAL:** HorizontalCharacterCardHolder needs a reference to VisualHandler to hide card visuals.

1. **Select HorizontalCharacterCardHolder** in the Hierarchy (`--- UI --- / Dynamic / HorizontalCharacterCardHolder`)
2. **In the Inspector**, find the **HorizontalCharacterCardHolder** component
3. **Locate the "Visual Handler" section** (may need to scroll down)
4. **Drag the `VisualHandler` GameObject** from the Hierarchy into the **Visual Handler** field
   - VisualHandler is located at: `--- UI --- / Dynamic / VisualHandler`
5. **Save the Scene**

---

### Part 2: Add "Select Preset" Button

The code has been updated to support a "Select Preset" button, but you need to add it in the Unity Editor:

1. **Open the CharacterSelection scene** in Unity Editor

2. **Locate the PresetSelectionUI GameObject** in the Hierarchy

   - It should be under: `--- UI --- / Dynamic / PresetSelectionUI`

3. **Find the PanelBackground child** (or whichever GameObject contains the preset buttons)

4. **Add a new Button**:

   - Right-click on the appropriate parent GameObject (likely near the BackButton)
   - Select `UI > Button - TextMeshPro` (or `UI > Button`)
   - Name it: `SelectPresetButton`

5. **Position the Button**:

   - Place it near the preset display area
   - Suggested position: Below the 3 preset buttons or next to the BackButton
   - Make sure it's visually distinct from the BackButton

6. **Configure Button Text**:

   - Update the child Text component to say: **"Select Preset"** or **"Confirm"**
   - Style it appropriately (font size, color, etc.)

7. **Wire up in Inspector**:

   - Select the `PresetSelectionUI` GameObject (parent level)
   - In the Inspector, find the `PresetSelectionUI` component
   - Drag the newly created `SelectPresetButton` GameObject into the **selectPresetButton** field

8. **Save the Scene**

---

## Verification Steps

After setting up the button, test the following flow:

### Test 1: Card Swapping

1. Enter Play Mode
2. Drag a character card and drop it on another slot
3. **Expected**: Cards should swap positions smoothly with animation
4. **Expected**: The card should snap back to Vector3.zero position after drag

### Test 2: Preset Selection Flow

1. Click on a character card
2. **Expected**: PresetSelectionUI overlay opens
3. Click on one of the 3 preset buttons
4. **Expected**: That preset image highlights in green
5. **Expected**: "Select Preset" button becomes enabled
6. Click "Select Preset" button
7. **Expected**:
   - Preset is saved to the card
   - Character Slot's build text updates to show the preset name
   - PresetSelectionUI closes
8. **Expected**: "Presets Selected: 1/4" text increments

### Test 3: Cancel Button

1. Click on a character card
2. Click on a preset to highlight it (but DON'T click Select Preset)
3. Click the "Back" or "Close" button
4. **Expected**: UI closes without saving the preset
5. **Expected**: Character Slot build text remains unchanged

### Test 4: All Presets Required

1. Select presets for only 3 out of 4 characters
2. **Expected**: "Presets Selected: 3/4" displays
3. **Expected**: Save/Start Combat button is DISABLED
4. Select a preset for the 4th character
5. **Expected**: "Presets Selected: 4/4" displays
6. **Expected**: Save/Start Combat button becomes ENABLED

---

## Troubleshooting

### If card swapping doesn't work:

- Check that HorizontalCharacterCardHolder has the updated script
- Verify that characterCards list is being populated in Start()
- Check Console for any errors related to transform hierarchy

### If "Select Preset" button is always disabled:

- Verify the button is assigned in the PresetSelectionUI Inspector field
- Check that clicking a preset button calls OnPresetSelected correctly
- Look for Console errors

### If build text doesn't update:

- Verify CharacterSlot component is attached to slot GameObjects
- Check that buildPresetText field is assigned in CharacterSlot Inspector
- Ensure AssignCard is being called when cards are placed in slots

---

## Code Changes Summary

### Files Modified:

1. **HorizontalCharacterCardHolder.cs**

   - Fixed EndDrag to restore DOLocalMove animation
   - Removed obsolete `selected` field references in Swap method

2. **CharacterSelectionManager.cs**

   - Refactored UpdateSelectionUI to show preset count instead of hero selection
   - Button now enables based on CardPresetManager.AreAllPresetsSelected()

3. **PresetSelectionUI.cs**
   - Added selectPresetButton field and currentlyHighlightedPresetIndex tracking
   - Modified OnPresetSelected to only highlight (not save immediately)
   - Added OnSelectPresetClicked method to save highlighted preset
   - Updated OnBackClicked to act as proper cancel (no save)
   - Enhanced visual highlighting to distinguish selected vs saved presets

---

## Next Steps After Setup

Once you've added the button in Unity Editor:

1. Save the scene
2. Enter Play Mode
3. Run through all verification tests above
4. If any issues occur, check the Console for error messages
5. Verify all components are properly assigned in Inspector

Good luck! 🎮
