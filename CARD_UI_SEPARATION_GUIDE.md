# Card-Based Preset System - UI Separation Fix Guide

## 🎯 Overview

This guide addresses the following issues in the character card preset selection system:

1. ✅ **Fixed:** Cards now show **Role Card sprite** instead of hero portrait by default
2. ✅ **Fixed:** "Current Build" text is now **separate from the card** and fixed under the slot
3. ✅ **Fixed:** When dragging cards between slots, the **slot's build text updates correctly**

## 🏗️ Architecture Changes

### Old Structure (Issues):

```
Slot (GameObject)
└── CharacterCard (draggable)
    └── CharacterCardVisual
        ├── Card Image (moves/rotates)
        └── BuildPresetText (❌ moves/rotates with card)
```

### New Structure (Fixed):

```
Slot (GameObject) + CharacterSlot component
├── CardSpawnPoint (Transform, card is child of this)
│   └── CharacterCard (draggable)
│       └── CharacterCardVisual
│           └── Card Image (moves/rotates, shows RoleCard or PresetSprite)
└── BuildPresetText (✅ FIXED under slot, doesn't move)
```

## 📦 New Component: CharacterSlot.cs

**Purpose:** Manages the relationship between a slot and its assigned card, including fixed build text display.

**Key Features:**

- Tracks which card is currently in the slot
- Updates build text when card is assigned/removed or preset changes
- Subscribes to card preset selection events
- Build text remains fixed (doesn't move/rotate with card)

**Location:** `Assets/Scripts/CharacterSelection/CharacterSlot.cs`

## 🔧 Code Changes Made

### 1. CharacterSlot.cs (NEW)

- Created new component to manage slot-card relationship
- Handles build text updates independently of card movement
- Subscribes to card SelectEvent for automatic updates

### 2. HorizontalCharacterCardHolder.cs (UPDATED)

- Modified `Swap()` method to update slot assignments during drag-and-drop
- Added slot initialization in `Start()` method
- Now calls `CharacterSlot.AssignCard()` and `RemoveCard()` during swaps

### 3. CharacterCardVisual.cs (UPDATED)

- **Removed** `buildPresetText` field (now handled by CharacterSlot)
- Updated `UpdatePresetData()` to only handle card sprite changes
- Added comment explaining build text is now in CharacterSlot

### 4. PresetSelectionUI.cs (UPDATED)

- Updated `UpdateCardVisual()` to notify parent slot when preset changes
- Slot's build text automatically updates via `CharacterSlot.UpdateBuildText()`

### 5. CharacterCard.cs (NO CHANGES NEEDED)

- Already uses RoleCard sprite correctly in `Initialize()` method
- Fallback to portrait if RoleCard is null

## 🎨 Unity Inspector Setup (REQUIRED USER ACTION)

### Step 1: Update Slot Prefab

1. **Locate your slot prefab** (e.g., `Assets/Prefabs/CharacterSelection/CharacterSlot.prefab`)
2. **Add CharacterSlot component:**

   - Select the slot prefab root
   - Add Component → CharacterSlot

3. **Create/Update Card Spawn Point:**

   - Add an empty child GameObject named "CardSpawnPoint"
   - Position it where cards should appear in the slot
   - This will be the parent for spawned cards

4. **Create "Current Build" Text:**

   - Right-click slot root → UI → Text - TextMeshPro
   - Name it: `BuildPresetText`
   - Position it **BELOW** the CardSpawnPoint (e.g., Y offset -100)
   - Set text: "None Selected"
   - Style: Center alignment, appropriate font size

5. **Assign References in CharacterSlot:**

   - Select slot prefab root
   - In CharacterSlot component:
     - **Build Preset Text:** Assign the BuildPresetText TMP_Text
     - **Card Spawn Point:** Assign the CardSpawnPoint Transform

6. **Save the prefab**

### Step 2: Remove BuildPresetText from CharacterCardVisual Prefab

1. **Open CharacterCardVisual prefab**
2. **Find and DELETE** the BuildPresetText child (if it exists)
   - This text should NO LONGER be a child of the card
3. **Save the prefab**

### Step 3: Verify HeroData Assets Have RoleCard Sprites

1. **Navigate to** `Assets/Data/`
2. **For each HeroData asset** (Mandirigma, Mangangayaw, Bagani, Babaylan):
   - Select the asset
   - In Inspector, find **"Role Card"** field
   - **Assign the base card sprite** (card showing hero with no stat values)
   - Ensure it's **different from the Image (portrait)** field
   - If you don't have RoleCard sprites yet, the system will fall back to portraits (but you should create them)

### Step 4: Update Scene Hierarchy

If your scene already has slots instantiated:

1. **For each existing slot GameObject:**

   - Add CharacterSlot component
   - Add BuildPresetText as child (TMP_Text)
   - Position BuildPresetText below the card
   - Assign references in CharacterSlot component
   - Ensure CharacterCard is child of CardSpawnPoint (not direct child of slot)

2. **Update HorizontalCharacterCardHolder reference:**
   - Ensure `slotPrefab` field points to your updated slot prefab

## 🧪 Testing Checklist

### Test 1: Card Sprite Display

- [ ] All 4 cards spawn with **RoleCard sprite** (not hero portrait)
- [ ] RoleCard sprites are visually distinct for each hero
- [ ] No console errors about missing sprites

### Test 2: Build Text Fixed Position

- [ ] "Current Build" text appears **below each slot**
- [ ] Hover over card → text **DOES NOT move**
- [ ] Drag card → text **DOES NOT move or rotate**
- [ ] Text remains readable and in correct position throughout

### Test 3: Preset Selection

- [ ] Click card → PresetSelectionUI opens
- [ ] Select preset → card sprite changes from RoleCard to PresetSprite
- [ ] "Current Build" text updates to show preset name
- [ ] Text format: "Current Build:\n[Preset Name]"

### Test 4: Drag and Drop

- [ ] Drag card to new slot
- [ ] **New slot's build text** shows the card's preset name
- [ ] **Old slot's build text** shows "None Selected" (or new card's preset if swapped)
- [ ] Swapping two cards → both slots update correctly
- [ ] Dragging card with no preset → shows "None Selected"

### Test 5: Edge Cases

- [ ] Card with no preset → text shows "None Selected"
- [ ] Card with unnamed preset → text shows "Unnamed Build"
- [ ] Selecting same preset twice → no errors
- [ ] Rapid card dragging → text updates smoothly
- [ ] All 4 heroes with different presets → all work correctly

## 🐛 Troubleshooting

### Issue: Cards still show hero portrait instead of RoleCard

**Solution:** Check HeroData assets in Inspector. Ensure RoleCard field is assigned with the correct base card sprite (not portrait).

### Issue: "Current Build" text moves with card

**Solution:**

- Verify BuildPresetText is a child of **Slot**, NOT CharacterCard or CharacterCardVisual
- Check that CharacterSlot component exists on slot GameObject
- Ensure CharacterSlot.buildPresetText reference is assigned in Inspector

### Issue: Build text doesn't update on drag

**Solution:**

- Verify HorizontalCharacterCardHolder has the updated Swap() logic
- Check console for errors related to CharacterSlot
- Ensure slot prefab has CharacterSlot component with references assigned

### Issue: Null reference errors for CharacterSlot

**Solution:**

- Make sure slot prefab root has CharacterSlot component
- Verify all slots in scene have CharacterSlot component
- Check that CharacterSelectionManager spawns cards correctly

### Issue: Build text shows wrong preset after drag

**Solution:**

- Check that CharacterSlot.AssignCard() is called during swap
- Verify CharacterCard.SelectedPreset is being set correctly
- Ensure PresetSelectionUI updates both card visual and slot text

## 📋 Summary of Changes

| Component                            | Change    | Reason                                              |
| ------------------------------------ | --------- | --------------------------------------------------- |
| **CharacterSlot.cs**                 | NEW       | Manages slot-card relationship and fixed build text |
| **HorizontalCharacterCardHolder.cs** | UPDATED   | Swap() now updates slot assignments                 |
| **CharacterCardVisual.cs**           | UPDATED   | Removed buildPresetText (moved to slot)             |
| **PresetSelectionUI.cs**             | UPDATED   | Notifies slot when preset changes                   |
| **CharacterCard.cs**                 | NO CHANGE | Already uses RoleCard correctly                     |

## 🎉 Expected Result

After completing the Unity Inspector setup:

1. ✅ Cards spawn with **RoleCard sprite** (base card, no stats)
2. ✅ "Current Build" text is **fixed under each slot**
3. ✅ Text **DOES NOT move or rotate** when card is dragged
4. ✅ Selecting preset → card changes to **PresetSprite** (with stats)
5. ✅ Dragging card to new slot → **both slots' text update correctly**
6. ✅ Smooth, polished UX with proper separation of card and fixed UI

## 📞 Next Steps

1. **Complete the Unity Inspector setup** (Steps 1-4 above)
2. **Enter Play mode** and test using the Testing Checklist
3. **Report any issues** with specific error messages or unexpected behavior
4. **Iterate** on styling and positioning as needed for your game's aesthetic

---

**Note:** The code changes are already complete. You only need to update the Unity Inspector references and test the system!
