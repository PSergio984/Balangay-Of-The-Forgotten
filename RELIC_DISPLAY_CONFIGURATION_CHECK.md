# Relic Display Configuration Verification

## Issues Fixed

### 1. **RelicDisplayUI Not Updating When Relics Are Collected** ✅ FIXED

**Problem**: When a relic was added via `VictoryDefeatUI.ProcessRewardCollection()`, the `RelicDisplayUI` component wasn't notified, so the display didn't update until the scene was reloaded.

**Solution**: Added `NotifyRelicDisplayUI()` method in `VictoryDefeatUI` that finds and calls `OnRelicCollected()` on the `RelicDisplayUI` component when a relic is successfully added.

**Also Fixed**: Added `NotifySpecialCardPanelUI()` for consistency with special cards.

## Configuration Checklist

### ✅ RelicDisplayUI Component Setup

1. **Open Combat Scene** (`Assets/Scenes/Combat.unity`)

2. **Select Relic Panel GameObject** in Hierarchy

3. **Check RelicDisplayUI Component** in Inspector:
   - [ ] **Relic Collection**: Must be assigned to `Relic Collection.asset`
     - Path: `Assets/Data/Data Persistence/Relic Collection.asset`
   - [ ] **Relic Slots**: List must have **Size = 4**
     - [ ] Element 0: Assigned to a GameObject with Image component
     - [ ] Element 1: Assigned to a GameObject with Image component
     - [ ] Element 2: Assigned to a GameObject with Image component
     - [ ] Element 3: Assigned to a GameObject with Image component
   - [ ] **Container** (Optional): Can be assigned to `Relic Panel` GameObject
   - [ ] **Hide When Empty**: Check if you want to hide the panel when no relics

### ✅ Relic Slots Setup

1. **Under Relic Panel**, create 4 child GameObjects:

   - `RelicSlot_0`
   - `RelicSlot_1`
   - `RelicSlot_2`
   - `RelicSlot_3`

2. **For each slot**:
   - [ ] Has `Image` component
   - [ ] Positioned correctly (vertically stacked or as desired)
   - [ ] Size set appropriately (e.g., 64x64 or match your design)

### ✅ VictoryDefeatUI Component Setup

1. **Select VictoryDefeatUI GameObject** in Hierarchy

2. **Check VictoryDefeatUI Component** in Inspector:
   - [ ] **Relic Collection**: Must be assigned to `Relic Collection.asset`
     - Path: `Assets/Data/Data Persistence/Relic Collection.asset`
   - [ ] **Special Card Collection**: Must be assigned (for special cards)
   - [ ] **Reward Chest UI**: Must be assigned

### ✅ Fallback Reward Data (For Testing)

1. **Select MatchSetupSystem GameObject** in Hierarchy

2. **Check MatchSetupSystem Component** in Inspector:
   - [ ] **Fallback Main Boss Reward**: Must be assigned
   - [ ] **Fallback Main Boss Reward > Associated Relic**: Must have a `RelicData` assigned
     - This is the relic that will be collected when testing

### ✅ RelicData Asset Setup

1. **Open RelicData asset** (e.g., `Assets/Data/Rewards/Luhain.asset`)

2. **Check each RelicData**:
   - [ ] **Relic Id**: Must have a unique ID (e.g., `"Relic_Luhain"`)
   - [ ] **Relic Name**: Display name (e.g., `"Luhain"`)
   - [ ] **Relic Sprite**: Must have a sprite assigned
   - [ ] **Source Map Id**: Optional, but recommended

### ✅ RelicCollectionData Asset Setup

1. **Open Relic Collection asset** (`Assets/Data/Data Persistence/Relic Collection.asset`)

2. **Check RelicCollectionData**:
   - [ ] **Available Relics**: List must contain all possible relic assets
     - This allows the system to load relics by ID from PlayerPrefs
   - [ ] All relic assets you want to collect must be in this list

## Testing Steps

### Test 1: Configuration Validation

1. **Enter Play Mode**
2. **Check Console** for validation messages:
   - Should see: `[RelicDisplayUI] Displaying X collected relics out of 4 slots.`
   - If slots missing: `[RelicDisplayUI] No relic slots assigned!`
   - If collection missing: `[RelicDisplayUI] RelicCollectionData not assigned!`

### Test 2: Relic Collection Flow

1. **Set up fallback reward**:

   - In `MatchSetupSystem`, assign `Fallback Main Boss Reward`
   - In that reward, assign `Associated Relic` to a `RelicData` asset

2. **Enter Play Mode**
3. **Defeat the main boss** (second enemy)
4. **Open the reward chest**
5. **Check Console** for:

   - `[RelicCollectionData] NEW RELIC OBTAINED: 'RelicName' (RelicId). Total relics: X`
   - `[VictoryDefeatUI] Collected relic: RelicName`
   - `[VictoryDefeatUI] Notified RelicDisplayUI to refresh display.`
   - `[RelicDisplayUI] Displaying X collected relics out of 4 slots.`

6. **Check Relic Panel**:
   - The relic sprite should appear in the first slot
   - Empty slots should have `Image.enabled = false`

### Test 3: Persistence

1. **Collect a relic** (as in Test 2)
2. **Exit Play Mode**
3. **Re-enter Play Mode**
4. **Check Relic Panel**:
   - The relic should still be visible (loaded from PlayerPrefs)

## Common Issues & Solutions

### Issue: Relic not appearing after collection

**Check**:

1. Is `RelicCollectionData` assigned in both `VictoryDefeatUI` and `RelicDisplayUI`?
2. Does the `RewardData.AssociatedRelic` have a valid `RelicData` assigned?
3. Does the `RelicData` have a `RelicSprite` assigned?
4. Are the 4 `Relic Slots` assigned in `RelicDisplayUI`?
5. Check Console for error messages

**Solution**: Verify all assignments in Inspector as per checklist above.

### Issue: Console shows "RelicDisplayUI not found in scene"

**Check**:

1. Is `RelicDisplayUI` component attached to a GameObject in the scene?
2. Is the GameObject active?

**Solution**: Ensure `RelicDisplayUI` is on an active GameObject in the scene.

### Issue: Slots show but no sprites

**Check**:

1. Does `RelicData.RelicSprite` have a sprite assigned?
2. Are relics actually being collected? (Check Console logs)
3. Is `RelicCollectionData.Load()` being called? (It's called automatically in `RefreshDisplay()`)

**Solution**:

- Assign sprites to `RelicData` assets
- Check Console for collection logs
- Manually call `RelicDisplayUI.RefreshDisplay()` in Play Mode to test

### Issue: Wrong relic in wrong slot

**Note**: Relics are displayed in the order they were collected. The first collected relic goes to slot 0, second to slot 1, etc.

**Solution**: This is expected behavior. If you want specific ordering, you'll need to modify the display logic.

## SpecialCardPanelUI Configuration

The same notification system was added for `SpecialCardPanelUI`. Verify:

1. **SpecialCardPanelUI Component**:

   - [ ] `Special Card Collection` assigned
   - [ ] `Card Container` assigned
   - [ ] `Special Card Button Prefab` assigned (optional)

2. **VictoryDefeatUI Component**:

   - [ ] `Special Card Collection` assigned

3. **Test**: Collect a special card from mini-boss reward and verify it appears in the panel.

## Summary

The main issue was that `RelicDisplayUI` wasn't being notified when relics were collected. This is now fixed with automatic notification from `VictoryDefeatUI`.

**Key Points**:

- ✅ Relics are now automatically displayed when collected
- ✅ Both `RelicDisplayUI` and `SpecialCardPanelUI` are notified
- ✅ Configuration validation provides helpful warnings
- ✅ All data persists via PlayerPrefs

Make sure all Inspector assignments are correct as per the checklist above!
