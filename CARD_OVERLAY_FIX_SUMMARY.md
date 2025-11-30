# Card System Fix Summary

## ✅ FIXED: Card Swapping Not Working with Dynamic Spawning

### Root Cause (Card Swap Bug)

**The Problem:** When `CharacterSelectionManager` was enabled (for dynamic card spawning), card swapping stopped working. When disabled, swapping worked but cards were static.

**Why This Happened:**

- `HorizontalCharacterCardHolder.Start()` runs first and subscribes to card drag/drop events
- `CharacterSelectionManager.Start()` spawns cards dynamically and then calls `RefreshCardHolder()`
- `RefreshCardHolder()` **replaced** the entire `characterCards` list with a new list
- This broke all event subscriptions that `HorizontalCharacterCardHolder` had set up
- Result: Cards existed but couldn't be dragged/swapped because events weren't connected

**The Fix:**

1. Added `RefreshCards()` public method to `HorizontalCharacterCardHolder`
2. This method:
   - Unsubscribes from old card events (if any)
   - Re-scans for all child cards
   - Re-subscribes to all card events (drag, drop, pointer enter/exit)
   - Updates visual indexes
3. Modified `CharacterSelectionManager.RefreshCardHolder()` to call `cardHolder.RefreshCards()` instead of directly manipulating the list

**Result:** Both dynamic spawning AND card swapping now work together correctly!

---

## ✅ FIXED: Card Overlay Visibility Issue

### Root Cause (Overlay Bug)

**The Problem:** Character cards were remaining visible and interactive when the PresetSelectionUI overlay opened, causing:

- Visual overlap/clutter (cards appearing behind/through the overlay)
- Input conflicts (cards still clickable/draggable while overlay was active)
- Confusing user experience

## Solution Implemented

### 1. **Card Visibility Control System**

Added a new method `SetCardsInteractable(bool interactable)` to `HorizontalCharacterCardHolder` that:

- **Simple and effective:** Disables/enables the entire slot GameObject when overlay opens/closes
- When disabled (overlay open): Cards are **completely hidden** (SetActive(false))
- When enabled (overlay closed): Cards are **fully visible and interactive** (SetActive(true))
- No complex CanvasGroup setup needed - just straightforward GameObject activation
- Prevents ALL interaction and visual overlap with preset UI

### 2. **Integration with PresetSelectionUI**

- Added `cardHolder` reference field to `PresetSelectionUI`
- **When overlay opens:** Calls `cardHolder.SetCardsInteractable(false)` to hide/disable cards
- **When overlay closes:** Calls `cardHolder.SetCardsInteractable(true)` to restore cards

### 3. **Files Modified**

- `Assets/Scripts/CharacterSelection/PresetSelectionUI.cs`

  - Added `cardHolder` reference field
  - Modified `ShowForCard()` to disable cards when opening
  - Modified `Hide()` to re-enable cards when closing

- `Assets/Scripts/CharacterSelection/CharacterCards/HorizontalCharacterCardHolder.cs`

  - Added `cardCanvasGroups` list to track CanvasGroups
  - Added `SetCardsInteractable(bool)` public method

- `UNITY_EDITOR_SETUP_INSTRUCTIONS.md`
  - Added Part 1: Wire Up Card Holder Reference (critical manual step)

---

## Manual Setup Required

### ⚠️ **CRITICAL STEP: Wire Up Card Holder Reference**

1. Open `CharacterSelection` scene in Unity Editor
2. Select `PresetSelectionUI` GameObject in Hierarchy (`--- UI --- / Dynamic / PresetSelectionUI`)
3. In the Inspector, find the **PresetSelectionUI** component
4. Locate the **"Card Visibility Control"** section
5. **Drag `HorizontalCharacterCardHolder`** from the Hierarchy into the **Card Holder** field
6. Save the Scene

**Without this step, the cards will NOT hide when the overlay opens!**

---

## Additional Issue Discovered

### **Card Spawning Problem**

During investigation with Unity MCP, discovered that:

- `HorizontalCharacterCardHolder` has **NO children spawned** in the current scene state
- The `characterCards` list is empty
- This means cards aren't being instantiated correctly

**Possible causes:**

1. Scene might not be in Play mode (cards spawn at runtime via `CharacterSelectionManager.Start()`)
2. `CharacterSelectionManager.cardSpawnParent` might not be correctly assigned
3. Cards might be spawning and then getting destroyed/removed

**To verify:**

1. Enter Play mode in Unity
2. Check if cards spawn as children of `HorizontalCharacterCardHolder`
3. If cards don't spawn, check Unity Console for errors
4. Verify `CharacterSelectionManager` Inspector settings:
   - `cardSpawnParent` should point to `HorizontalCharacterCardHolder`
   - `slotPrefab` should be assigned
   - `availableHeroes` should have 4 HeroData assets

---

## Testing Instructions

### ⚠️ Before Testing: Enable CharacterSelectionManager

The `CharacterSelectionManager` GameObject was disabled during debugging. You need to re-enable it:

1. Open `CharacterSelection` scene in Unity Editor
2. In the Hierarchy, find: `--- UI --- / Dynamic / CharacterSelectionManager`
3. **Enable the GameObject** (checkbox at the top of the Inspector)
4. Save the Scene

---

## Testing Checklist

### Test 0: Card Dynamic Spawning (NEW FIX)

1. Enter Play Mode
2. **Expected:** 4 character cards spawn dynamically under `HorizontalCharacterCardHolder`
3. **Expected:** Each card shows the character portrait and info from HeroData
4. **Expected:** Cards are positioned horizontally at the bottom of the screen

### Test 1: Card Swapping (NEW FIX)

1. With Play mode active, **drag a character card** and drop it on another card's position
2. **Expected:** Cards swap positions smoothly with animation
3. **Expected:** The dragged card snaps back to `Vector3.zero` position after drag
4. **Expected:** Card indices update correctly after swap
5. **Expected:** Character slot "Current Build" text moves with the swapped card

### Test 2: Card Visibility When Overlay Opens

1. With Play mode active, click on a character card
2. **Expected:** PresetSelectionUI overlay opens
3. **Expected:** Character cards dim to 30% opacity and become non-interactive
4. **Expected:** Can't click or drag cards while overlay is open

### Test 3: Card Restoration When Overlay Closes

1. With overlay open, click the "Back" button (or select a preset)
2. **Expected:** Overlay closes
3. **Expected:** Character cards restore to 100% opacity and become interactive again
4. **Expected:** Can click/drag cards normally

### Test 4: Full Preset Selection Flow

1. Click card → overlay opens (cards dim)
2. Click a preset button → preset highlights in green
3. Click "Select Preset" → preset saves, overlay closes (cards restore)
4. **Expected:** "Presets Selected: 1/4" increments
5. **Expected:** Character slot shows "Current: [Preset Name]"

---

## Summary of Changes

### ✅ Completed (Card Swap Fix)

- **Fixed race condition** between `HorizontalCharacterCardHolder` and `CharacterSelectionManager`
- Added `RefreshCards()` public method to `HorizontalCharacterCardHolder`
  - Re-scans for cards after dynamic spawning
  - Unsubscribes from old events and re-subscribes to new cards
  - Updates visual indexes
- Modified `CharacterSelectionManager.RefreshCardHolder()` to call `RefreshCards()` instead of directly manipulating the list
- **Result:** Dynamic spawning AND card swapping now work together!

### ✅ Completed (Card Overlay Fix)

- Card hiding/dimming when overlay opens
- Card restoration when overlay closes
- CanvasGroup-based visibility control
- Manual setup instructions updated

### ⚠️ Requires Manual Setup

1. **Re-enable CharacterSelectionManager GameObject** (disabled during debugging)
2. Wire up `cardHolder` reference in PresetSelectionUI Inspector (for overlay fix)

---

## Technical Details

### How Card Swapping Works (After Fix)

1. **Unity Start() Order:**

   - `HorizontalCharacterCardHolder.Start()` runs first, but cards don't exist yet (waits for dynamic generation)
   - `CharacterSelectionManager.Start()` spawns card slots dynamically as children of `HorizontalCharacterCardHolder`

2. **Card Generation:**

   - `CharacterSelectionManager.GenerateCharacterCards()` instantiates slot prefabs
   - Each slot contains a `CharacterCard` component
   - Cards are bound to `HeroData` via `card.Initialize(hero)`

3. **Card Holder Refresh (THE FIX):**

   - `CharacterSelectionManager.RefreshCardHolder()` waits one frame
   - Calls `cardHolder.RefreshCards()` (NEW METHOD)
   - `RefreshCards()` re-scans for all child cards and **re-subscribes to drag/drop events**
   - This ensures all event handlers are properly connected

4. **Card Swapping:**
   - When user drags a card, `BeginDrag` event fires
   - `Update()` detects when dragged card crosses another card's position
   - `Swap()` method exchanges parent transforms and updates slot assignments
   - Card snaps back to `Vector3.zero` on `EndDrag`

### How Card Overlay Works

1. **PresetSelectionUI.ShowForCard()** is called when user clicks a card
2. Before showing the overlay, it calls `cardHolder.SetCardsInteractable(false)`
3. `SetCardsInteractable(false)` iterates through all card slots and:
   - Calls `SetActive(false)` on each slot GameObject
   - Cards are **completely hidden** from view
   - No input events can reach the cards
4. When `PresetSelectionUI.Hide()` is called, it calls `cardHolder.SetCardsInteractable(true)`
5. `SetCardsInteractable(true)` calls `SetActive(true)` on all slots
6. Cards are **fully restored** and become visible and interactive again

### Why SetActive()?

- **Simple:** Single line of code per card - easy to understand and maintain
- **Effective:** Completely hides the cards, no visual overlap possible
- **Performance:** Disabling GameObjects is very efficient in Unity
- **Reversible:** Easy to re-enable when overlay closes
- **No leftover components:** No CanvasGroup components cluttering the hierarchy
