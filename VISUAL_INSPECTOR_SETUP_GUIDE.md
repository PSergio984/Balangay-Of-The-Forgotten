# 🎨 VISUAL INSPECTOR SETUP GUIDE

## Quick Visual Reference for Unity Editor Setup

---

## 📍 Step 1: PresetSelectionUI Setup

### Where to Find It:

```
Hierarchy Window:
└── --- UI ---
    └── Dynamic
        └── PresetSelectionUI ← SELECT THIS
```

### What to Do:

1. **Click** `PresetSelectionUI` in Hierarchy
2. **Look** at Inspector window on the right
3. **Find** the section labeled **"Card Visibility Control"**
4. **Locate** the field named **"Card Holder"**

### What to Drag:

```
FROM Hierarchy:
└── --- UI ---
    └── Dynamic
        └── HorizontalCharacterCardHolder ← DRAG THIS

TO Inspector:
PresetSelectionUI Component
└── Card Visibility Control
    └── Card Holder [     ] ← DROP HERE
```

### After Setup Should Look Like:

```
PresetSelectionUI (Script)
├── Preset Manager      [CardPresetManager]
├── Preset Button Prefab [Preset Card]
├── Preset Container    [PresetButtonContainer]
└── Card Visibility Control
    └── Card Holder     [HorizontalCharacterCardHolder] ✓
```

---

## 📍 Step 2: HorizontalCharacterCardHolder Setup

### Where to Find It:

```
Hierarchy Window:
└── --- UI ---
    └── Dynamic
        └── HorizontalCharacterCardHolder ← SELECT THIS
```

### What to Do:

1. **Click** `HorizontalCharacterCardHolder` in Hierarchy
2. **Look** at Inspector window on the right
3. **Scroll down** to find the section labeled **"Visual Handler"**
4. **Locate** the field named **"Visual Handler"**

### What to Drag:

```
FROM Hierarchy:
└── --- UI ---
    └── Dynamic
        └── VisualHandler ← DRAG THIS

TO Inspector:
HorizontalCharacterCardHolder Component
└── Visual Handler
    └── Visual Handler [     ] ← DROP HERE
```

### After Setup Should Look Like:

```
HorizontalCharacterCardHolder (Script)
├── Auto Spawn On Start     [✓]
├── Cards To Spawn          [3]
├── Slot Prefab            [CharacterCardSlot]
├── Selected Card          [None]
├── Tween Card Return      [✓]
└── Visual Handler
    └── Visual Handler     [VisualHandler] ✓
```

---

## 🎯 Quick Checklist

### Before You Start:

- [ ] Unity Editor is open
- [ ] Scene is loaded (Character Selection scene)
- [ ] Hierarchy window is visible
- [ ] Inspector window is visible

### Step 1 - PresetSelectionUI:

- [ ] Selected PresetSelectionUI in Hierarchy
- [ ] Found "Card Visibility Control" section in Inspector
- [ ] Dragged HorizontalCharacterCardHolder to "Card Holder" field
- [ ] Field now shows "HorizontalCharacterCardHolder" (not "None")

### Step 2 - HorizontalCharacterCardHolder:

- [ ] Selected HorizontalCharacterCardHolder in Hierarchy
- [ ] Found "Visual Handler" section in Inspector (may need to scroll)
- [ ] Dragged VisualHandler to "Visual Handler" field
- [ ] Field now shows "VisualHandler" (not "None")

### Final Steps:

- [ ] Save Scene (Ctrl+S or File → Save)
- [ ] No errors in Console window
- [ ] Ready to test in Play mode!

---

## ❓ Troubleshooting Visual Guide

### Problem: "I don't see the Visual Handler field in Inspector"

**Possible Causes:**

1. Unity hasn't recompiled the script yet
2. You're looking at the wrong component

**Solutions:**

1. **Wait for compilation:**

   - Look at bottom-right of Unity Editor
   - Wait for spinning progress icon to finish
   - Check if Console shows "Compilation complete"

2. **Force recompile:**

   - Click Assets menu → Refresh (or press Ctrl+R)
   - Wait for compilation to complete
   - Check Inspector again

3. **Verify correct component:**
   - Make sure you selected HorizontalCharacterCardHolder GameObject (not a child)
   - In Inspector, verify component name says "HorizontalCharacterCardHolder (Script)"

### Problem: "I can't find VisualHandler in Hierarchy"

**Solution:**

- Expand the `--- UI ---` folder
- Expand the `Dynamic` folder
- Look for `VisualHandler` (should be at same level as HorizontalCharacterCardHolder)

### Problem: "Drag and drop isn't working"

**Solutions:**

1. **Make sure you're dragging the GameObject, not the component:**

   - Click the GameObject NAME in Hierarchy (left side)
   - Do NOT click the component icon or checkbox

2. **Make sure you're dropping in the correct field:**

   - Look for the small circle icon to the right of the field
   - You can also click this circle to get a selection window

3. **Alternative method:**
   - Click the small circle icon next to the field
   - A selection window will pop up
   - Type the GameObject name to search
   - Double-click the correct GameObject

---

## ✅ Success Verification

### How to Know It Worked:

#### Visual Confirmation:

- **PresetSelectionUI Inspector:**

  - "Card Holder" field shows `HorizontalCharacterCardHolder` (text is blue, not gray)
  - Click the field → it highlights the GameObject in Hierarchy

- **HorizontalCharacterCardHolder Inspector:**
  - "Visual Handler" field shows `VisualHandler` (text is blue, not gray)
  - Click the field → it highlights the GameObject in Hierarchy

#### Console Confirmation:

- No red error messages
- No warnings about "missing reference" or "null reference"

---

## 🚀 Ready to Test!

Once both fields are assigned and scene is saved:

1. **Click Play button** (top-center of Unity Editor)
2. **Wait for game to start**
3. **Click any character card**
4. **Verify:** ALL cards completely disappear (both slots AND visuals)
5. **Close overlay**
6. **Verify:** All cards reappear

**If cards don't disappear completely:**

- Exit Play mode
- Re-check Inspector assignments
- Make sure both fields are assigned (not "None")
- Save scene again
- Try Play mode again

---

**Need Help?**

- Check `COMPLETE_CARD_FIX_SUMMARY.md` for detailed technical explanation
- Check `UNITY_EDITOR_SETUP_INSTRUCTIONS.md` for text-based instructions
- Check Console window for error messages

---

**Last Updated:** 2025-01-19  
**Status:** Setup Required → Ready to Test → Success! 🎉
