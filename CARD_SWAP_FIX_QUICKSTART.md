# Card Swap Fix - Quick Start Guide

## 🎯 What Was Fixed

**Problem:** Cards couldn't swap when `CharacterSelectionManager` was enabled (for dynamic spawning).

**Solution:** Fixed race condition between card holder initialization and dynamic card generation.

---

## ✅ Steps to Test (Do This Now!)

### 1. Enable CharacterSelectionManager

1. Open Unity Editor
2. Open `CharacterSelection` scene
3. In Hierarchy, navigate to: `--- UI --- / Dynamic / CharacterSelectionManager`
4. **Check the checkbox** at the top of Inspector to enable it
5. Save Scene (Ctrl+S)

### 2. Test in Play Mode

1. **Exit Play mode** if currently playing (to refresh scripts)
2. **Save the scene** (Ctrl+S)
3. **Press Play button**
4. **Verify:** 4 character cards spawn at the bottom
5. **Test drag & drop:** Drag a card and drop it on another card
6. **Expected:** Cards swap positions smoothly
7. **Expected:** Cards snap back to their slot positions
8. **Test preset selection:** Click a card to open overlay
9. **Expected:** Character cards are **COMPLETELY HIDDEN** (not visible at all)
10. **Expected:** Only preset selection UI is visible and interactive
11. Select a preset and close overlay
12. **Expected:** Cards instantly reappear and are fully interactive

---

## 🔧 What Changed

### Files Modified

1. **HorizontalCharacterCardHolder.cs**

   - Added `RefreshCards()` public method
   - Refactored `Start()` to use new `InitializeCards()` private method
   - `InitializeCards()` unsubscribes from old events and re-subscribes to new cards

2. **CharacterSelectionManager.cs**
   - Modified `RefreshCardHolder()` to call `cardHolder.RefreshCards()`
   - No longer directly manipulates the `characterCards` list

---

## 📋 Expected Behavior

### ✅ Working Features

- [x] Cards spawn dynamically from HeroData assets
- [x] Cards can be dragged and swapped
- [x] Cards snap back to slot positions after drag
- [x] Preset selection opens overlay
- [x] Cards are hidden/dimmed when overlay is active
- [x] Cards restore when overlay closes
- [x] Character slot "Current Build" text tracks with card

### 🎮 How to Swap Cards

1. **Click and hold** on a character card
2. **Drag** the card left or right
3. **Release** when hovering over another card
4. Cards will smoothly swap positions

---

## 🐛 If It Still Doesn't Work

### Check Inspector Settings

Open `CharacterSelectionManager` in Inspector and verify:

- **Available Heroes:** Should have 4 HeroData assets assigned
- **Card Holder:** Should point to `HorizontalCharacterCardHolder`
- **Card Preset Manager:** Should point to `CardPresetManager`
- **Card Spawn Parent:** Should point to `HorizontalCharacterCardHolder`
- **Slot Prefab:** Should point to `CharacterCardSlot` prefab

### Check Console

1. Open Unity Console (Window > General > Console)
2. Look for errors or warnings starting with `[CharacterSelectionManager]` or `[HorizontalCharacterCardHolder]`
3. If you see errors, report them for further debugging

---

## 📖 Full Details

See `CARD_OVERLAY_FIX_SUMMARY.md` for complete technical explanation and testing checklist.
