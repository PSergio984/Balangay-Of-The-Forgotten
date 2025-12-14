# Relic Display UI Setup Guide

## Overview

The `RelicDisplayUI` component has been refactored to use **fixed slots** assigned in the Inspector, similar to the `HeroBoardView` pattern. This makes it easier to position and layout relic slots in the Unity Editor.

## Key Changes

### Before (Dynamic Spawning)

- Slots were created dynamically at runtime
- Used a container with a prefab to spawn slots
- Less control over individual slot positioning

### After (Fixed Slots)

- **4 fixed Image slots** assigned directly in the Inspector
- Full control over slot positioning and layout
- Similar pattern to `HeroBoardView` which uses `List<Transform> slots`

## Implementation Summary

### Component Structure

```csharp
public class RelicDisplayUI : MonoBehaviour
{
    [SerializeField] private RelicCollectionData relicCollection;  // Data source
    [SerializeField] private List<Image> relicSlots;                 // 4 fixed slots
    [SerializeField] private GameObject container;                   // Optional container
    [SerializeField] private bool hideWhenEmpty = true;              // Hide when no relics
}
```

### How It Works

1. **Data Source**: `RelicCollectionData` ScriptableObject tracks all collected relics
2. **Fixed Slots**: 4 `Image` components assigned in Inspector represent the slots
3. **Display Logic**: When a relic is unlocked, its sprite is assigned to the corresponding slot
4. **Empty Slots**: Unused slots have their `Image.enabled = false`

## Setup Instructions

### Step 1: Create Relic Slots in Unity Editor

1. **Open the Combat Scene**

   - Navigate to `Assets/Scenes/Combat.unity`

2. **Locate the Relic Panel**

   - Find `Canvas > Relic Panel` in the Hierarchy

3. **Create 4 Slot GameObjects**

   - Under `Relic Panel`, create 4 child GameObjects:
     - `RelicSlot_0`
     - `RelicSlot_1`
     - `RelicSlot_2`
     - `RelicSlot_3`

4. **Add Image Components**

   - For each slot GameObject:
     - Add `Image` component (if not present)
     - Set desired size (e.g., 64x64 or match your design)
     - Position them vertically or horizontally as desired
     - **Important**: Leave `Image.enabled = true` (script will handle visibility)

5. **Optional: Add Layout Group**
   - Add `Vertical Layout Group` or `Horizontal Layout Group` to `Relic Panel` for automatic spacing
   - Configure spacing, padding, and alignment as needed

### Step 2: Configure RelicDisplayUI Component

1. **Select Relic Panel** in Hierarchy

2. **In Inspector, find RelicDisplayUI component**:

   - **Relic Collection**: Assign your `RelicCollectionData` ScriptableObject
     - Path: `Assets/Data/Data Persistence/Relic Collection.asset`
   - **Relic Slots**: Expand the list and set **Size = 4**
     - **Element 0**: Drag `RelicSlot_0` GameObject
     - **Element 1**: Drag `RelicSlot_1` GameObject
     - **Element 2**: Drag `RelicSlot_2` GameObject
     - **Element 3**: Drag `RelicSlot_3` GameObject
   - **Container** (Optional): Assign `Relic Panel` GameObject if you want to hide the entire panel when empty
   - **Hide When Empty**: Check if you want to hide slots when no relics are collected

### Step 3: Create Relic Data Assets

1. **Create RelicData ScriptableObjects**

   - Right-click in Project window → `Create > Relics > Relic Data`
   - Name them (e.g., `Luhain`, `Pangil`, `Korona`, `Silang`)

2. **Configure Each Relic**:

   - **Relic Id**: Unique identifier (e.g., `"Relic_Luhain"`)
   - **Relic Name**: Display name (e.g., `"Luhain"`)
   - **Relic Sprite**: Assign the sprite asset (e.g., the crab sprite)
   - **Description**: Optional lore text
   - **Source Map Id**: Which map this relic comes from

3. **Add to RelicCollectionData**:
   - Open `Assets/Data/Data Persistence/Relic Collection.asset`
   - In **Available Relics** list, add all possible relic assets
   - This allows the system to load relics by ID from PlayerPrefs

### Step 4: Test the Setup

1. **Enter Play Mode**
2. **Check Console** for validation messages:

   - Should see: `[RelicDisplayUI] Displaying X collected relics out of 4 slots.`
   - If slots are missing: `[RelicDisplayUI] No relic slots assigned!`

3. **Verify Display**:
   - Empty slots should have `Image.enabled = false`
   - Unlocked relics should show their sprites
   - Slots should be positioned as designed

## Relic Data Flow

### How Relics Are Collected

1. **Player defeats main boss** → `VictoryDefeatUI` detects victory
2. **RewardData.associatedRelic** is checked
3. **RelicCollectionData.AddRelic()** is called
4. **Relic is saved** to PlayerPrefs automatically
5. **RelicDisplayUI.RefreshDisplay()** is called (if subscribed to events)

### How Relics Are Displayed

1. **RelicDisplayUI.Start()** or **OnEnable()** calls `RefreshDisplay()`
2. **RelicCollectionData.Load()** loads saved relics from PlayerPrefs
3. **GetAllRelics()** returns list of collected `RelicData` objects
4. **For each slot**:
   - If relic exists at index: Set sprite and enable Image
   - If no relic: Disable Image component

## Configuration Verification Checklist

✅ **RelicDisplayUI Component**:

- [ ] `Relic Collection` assigned
- [ ] `Relic Slots` list has Size = 4
- [ ] All 4 slots have Image components assigned
- [ ] `Container` assigned (optional)
- [ ] `Hide When Empty` set as desired

✅ **Relic Slots**:

- [ ] 4 GameObjects created under Relic Panel
- [ ] Each has `Image` component
- [ ] Slots are positioned correctly
- [ ] Layout Group added if needed

✅ **Relic Data**:

- [ ] RelicData ScriptableObjects created
- [ ] Each has unique `RelicId`
- [ ] Each has `RelicSprite` assigned
- [ ] All relics added to `RelicCollectionData.allRelicAssets`

✅ **Testing**:

- [ ] No console errors in Play Mode
- [ ] Slots display correctly when relics are unlocked
- [ ] Empty slots are hidden properly
- [ ] Container hides when empty (if enabled)

## Troubleshooting

### Issue: Slots not showing relics

- **Check**: `RelicCollectionData` is assigned and has collected relics
- **Check**: `RelicCollectionData.Load()` is being called
- **Check**: Relic sprites are assigned in RelicData assets

### Issue: Slots are always visible even when empty

- **Expected behavior**: Slots are always present, but `Image.enabled = false` when empty
- **To hide container**: Enable `Hide When Empty` and assign `Container` GameObject

### Issue: Wrong relic in wrong slot

- **Check**: Relics are added in order via `RelicCollectionData.AddRelic()`
- **Note**: Slots display relics in the order they were collected

### Issue: Console warnings about missing slots

- **Fix**: Assign all 4 Image components in the `Relic Slots` list
- **Fix**: Ensure each slot GameObject has an `Image` component

## Pattern Comparison

### HeroBoardView Pattern (Reference)

```csharp
[SerializeField] private List<Transform> slots;  // Fixed slot positions
public void AddHero(HeroData heroData)
{
    Transform slot = slots[HeroViews.Count];  // Use next available slot
    // Spawn hero at slot position
}
```

### RelicDisplayUI Pattern (Similar)

```csharp
[SerializeField] private List<Image> relicSlots;  // Fixed slot Images
public void RefreshDisplay()
{
    for (int i = 0; i < relicSlots.Count; i++)
    {
        if (i < relics.Count)
        {
            relicSlots[i].sprite = relics[i].RelicSprite;  // Assign sprite
            relicSlots[i].enabled = true;
        }
    }
}
```

Both patterns use **fixed references** assigned in Inspector for easier setup and positioning control.

## Summary

The new implementation:

- ✅ Uses 4 fixed slots assigned in Inspector (easier setup)
- ✅ Follows the same pattern as `HeroBoardView` (consistent architecture)
- ✅ Provides full control over slot positioning and layout
- ✅ Automatically shows/hides sprites based on collected relics
- ✅ Validates configuration at runtime with helpful warnings

This approach is more maintainable and gives you complete control over the UI layout in the Unity Editor.
