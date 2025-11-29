# UI Setup Guide - Slot-Based Preset System

This guide shows how to structure your UI for the new 4-slot character preset selection system.

---

## Required UI Structure

```
MainSelectionCanvas (GameObject)
├── Slot1Button (Button)
│   └── SlotImage (Image) ← Shows selected preset sprite
├── Slot2Button (Button)
│   └── SlotImage (Image)
├── Slot3Button (Button)
│   └── SlotImage (Image)
├── Slot4Button (Button)
│   └── SlotImage (Image)
├── CurrentBuildText (TextMeshPro)
├── SaveButton (Button)
└── PresetSelectionUI (GameObject, initially disabled)
   ├── TitleText (TextMeshPro)
   ├── Preset1Button (Button)
   │   └── PresetImage (Image)
   ├── Preset2Button (Button)
   │   └── PresetImage (Image)
   ├── Preset3Button (Button)
   │   └── PresetImage (Image)
   └── BackButton (Button)
```

---

## Step-by-Step Setup

### Step 1: Create Main Selection Canvas

1. Add a Canvas to your scene (if not present)
2. Add 4 Buttons for slots (Slot1Button, Slot2Button, Slot3Button, Slot4Button)
3. Each slot button should have an Image child (SlotImage) to display the selected preset's sprite
4. Add a TextMeshPro object below the slots for "Current Build: ..."
5. Add a Save Button

### Step 2: Create Preset Selection UI

1. Add a GameObject (PresetSelectionUI) under the Canvas, set it inactive by default
2. Add a TitleText (TextMeshPro) at the top
3. Add 3 Buttons (Preset1Button, Preset2Button, Preset3Button), each with an Image child (PresetImage)
4. Add a BackButton (Button) at the bottom

---

## Linking UI to Scripts

### MainSelectionUI Script

Assign these references in the Inspector:

- **transitionData**: The `CharacterTransitionData` ScriptableObject asset. This asset stores the slot selection data, transition timings, animation clip references, and any parameters used by `MainSelectionUI` to animate character slot changes and pass data to the combat scene. You can create it via **Assets > Create > Data > Character Transition Data** (usually in `Assets/ScriptableObjects/` or `Assets/Data/`). For more details, see the [CharacterTransitionData class](../Scripts/SceneController/CharacterTransitionData.cs) or the setup guide.
- **slotImages**: The 4 SlotImage components
- **currentBuildText**: The TMP_Text for build name
- **saveButton**: The Save Button
- **presetSelectionUI**: Reference to your PresetSelectionUI script

### PresetSelectionUI Script

Assign these references in the Inspector:

- presetImages: 3 Image components for preset options
- presetButtons: 3 Buttons for selecting presets
- titleText: TMP_Text for the title
- BackButton: Button for back/close functionality (must be wired to the PresetSelectionUI back/close handler)

---

## How It Works

1. Player clicks a slot button
2. PresetSelectionUI appears, showing 3 preset options for that hero
3. Player selects a preset; the slot image updates to show the preset's sprite
4. "Current Build" text updates to show the selected preset name
5. Player repeats for other slots, then clicks Save

---

## Example Layout

```
┌─────────────────────────────────────────────┐
│ [Slot1] [Slot2] [Slot3] [Slot4]           │
│   img     img     img     img              │
│                                           │
│ Current Build: Glass Cannon Set           │
│                                           │
│                [Save]                     │
└─────────────────────────────────────────────┘
```

---

## Tips

- Preset sprites should contain all info (portrait, build name, stats)
- No need for dynamic text overlays on slot images
- Use TMP_Text for "Current Build" and UI labels
- Use UnityEvents to wire up button clicks to your scripts

---

## Testing

1. Play the scene
2. Click each slot, select a preset, verify the slot image and build text update
3. Click Save to proceed

---

## Setting Up CharacterCard Prefab

The CharacterCard prefab is used to visually display each hero's portrait, name, and stats within each slot of the preset selection UI. Before your preset selection UI can show correct visuals, you must ensure your CharacterCard prefab is set up as described below.

### Step 1: Locate Your CharacterCard Prefab

1. Navigate to `Assets/Prefabs/CharacterSelection/` (or wherever your prefab is)
2. Double-click the **CharacterCard** prefab to enter Prefab editing mode
3. Find the **CharacterCardVisual** GameObject in the hierarchy

---

### Step 2: Add Character Portrait Image

**Create the Image:**

1. Right-click on the appropriate parent (likely inside ShakeParent/TiltParent)
2. Select **UI → Image**
3. Rename to `CharacterPortrait`

**Configure RectTransform:**

```
Anchors: Center (0.5, 0.5)
Position: (0, 50, 0) - adjust based on your card layout
Width: 128
Height: 128
Scale: (1, 1, 1)
```

**Configure Image Component:**

```
Source Image: None (will be set at runtime)
Image Type: Simple
Preserve Aspect: ✓ Enabled
Raycast Target: ✗ Disabled (optional, for performance)
```

**Position Tips:**

- Place near top/center of card
- Leave room for name text below
- Consider card frame/borders

---

### Step 3: Add Character Name Text

**Create the Text:**

1. Right-click on the same parent as Portrait
2. Select **UI → Text - TextMeshPro**
3. Rename to `CharacterNameText`

**Configure RectTransform:**

```
Anchors: Center (0.5, 0.5)
Position: (0, -20, 0) - below portrait
Width: 150
Height: 30
```

**Configure TextMeshProUGUI Component:**

```
Text: "Character Name" (placeholder)
Font: Your game font
Font Size: 20-24
Alignment: Center (Horizontal & Vertical)
Color: White (#FFFFFF) or your theme color
Wrapping: Enabled
Overflow: Ellipsis (truncate long names)
```

**Style Enhancements:**

- Add Outline effect (black, size 0.2) for readability
- Add Shadow for depth
- Consider gradient for visual appeal

---

### Step 4: Add Health Text

**Create the Text:**

1. Right-click on the same parent
2. Select **UI → Text - TextMeshPro**
3. Rename to `HealthText`

**Configure RectTransform:**

```
Anchors: Bottom-Center (0.5, 0)
Position: (0, 10, 0) - near bottom of card
Width: 100
Height: 25
```

**Configure TextMeshProUGUI Component:**

```
Text: "HP: 100" (placeholder)
Font: Your game font
Font Size: 14-16
Alignment: Center
Color: Green (#00FF00) or theme color
```

**Optional Enhancements:**

- Use icon before text (heart icon)
- Color code based on HP (green = healthy, yellow = medium, red = low)
- Add background panel for contrast

---

### Step 5: Link to CharacterCardVisual Component

**Select the CharacterCardVisual GameObject**

**In the Inspector, find "Character Data Display" section:**

1. **Character Portrait** field:

   - Drag the `CharacterPortrait` Image you created
   - OR click the circle icon → select CharacterPortrait from the list

2. **Character Name Text** field:

   - Drag the `CharacterNameText` TextMeshPro you created

3. **Health Text** field:
   - Drag the `HealthText` TextMeshPro you created

**Verification:**

- All three fields should show references (not "None")
- Hover over each to verify it's the correct component

---

## Example Layout Positions

### Vertical Card Layout (Portrait-style)

```
┌─────────────────┐
│  [Card Border]  │
│                 │
│   ┌─────────┐   │ ← CharacterPortrait (0, 60, 0)
│   │         │   │
│   │ Portrait│   │
│   │         │   │
│   └─────────┘   │
│                 │
│  "Warrior"      │ ← CharacterNameText (0, -10, 0)
│                 │
│   ⚔️ ATK: 25    │ ← Optional stats
│   🛡️ DEF: 15    │
│                 │
│  HP: 150        │ ← HealthText (0, -70, 0)
└─────────────────┘
```

### Horizontal Card Layout (Landscape)

```
┌──────────────────────────────┐
│                              │
│  ┌────────┐    Warrior       │
│  │        │    HP: 150       │
│  │Portrait│    ATK: 25       │
│  │        │    DEF: 15       │
│  └────────┘    MAG: 10       │
│                              │
└──────────────────────────────┘
```

---

## Testing the Setup

### In Prefab Edit Mode:

1. **Check Hierarchy:**

   - CharacterPortrait exists
   - CharacterNameText exists
   - HealthText exists

2. **Check References:**

   - Select CharacterCardVisual
   - All three fields in "Character Data Display" are assigned

3. **Check Visibility:**
   - All UI elements visible in Scene view
   - Text is readable
   - Portrait frame looks good

---

### In Play Mode:

1. **Run Character Selection Scene**
2. **Verify Each Card Shows:**

   - Hero portrait (from HeroData.Image)
   - Hero name (from HeroData.HeroName)
   - HP value (from HeroData.Health)

3. **If Blank:**
   - Check Console for errors
   - Verify HeroData assets have Image/Name/Health filled in
   - Verify CharacterCardVisual.UpdateCharacterData() is called

---

## Advanced Customization

### Add More Stats Display

```csharp
// In CharacterCardVisual.cs, add fields:
[SerializeField] private TMP_Text attackText;
[SerializeField] private TMP_Text defenseText;

// In UpdateCharacterData():
if (attackText != null)
{
    attackText.text = $"ATK: {heroData.AttackPower}";
}

if (defenseText != null)
{
    defenseText.text = $"DEF: {heroData.Defense}";
}
```

### Add Character Class Icon

```csharp
[SerializeField] private Image classIcon;

// In UpdateCharacterData():
if (classIcon != null && heroData.ClassIcon != null)
{
    classIcon.sprite = heroData.ClassIcon;
}
```

### Add Deck Size Display

```csharp
[SerializeField] private TMP_Text deckSizeText;

// In UpdateCharacterData():
if (deckSizeText != null && heroData.Deck != null)
{
    deckSizeText.text = $"{heroData.Deck.Count} Cards";
}
```

---

## Common Layout Issues

### Problem: Portrait is stretched/distorted

**Solution**: Enable "Preserve Aspect" on Image component

### Problem: Text is cut off

**Solution**: Increase Width/Height of RectTransform, enable Wrapping

### Problem: Elements overlap

**Solution**: Adjust Position values, check RectTransform anchors

### Problem: Text is unreadable

**Solution**: Add Outline/Shadow effects, increase font size, adjust color

### Problem: Portrait too large/small

**Solution**: Adjust Width/Height in RectTransform (keep equal for square)

---

## UI Best Practices

### Spacing Guidelines

- Minimum 10 units between elements
- Portrait should be largest element (25-40% of card height)
- Text should be readable at card scale (minimum 14pt font)

### Color Guidelines

- High contrast between text and background
- Use thematic colors (green = health, red = attack, blue = magic)
- Add outlines for text readability

### Animation Considerations

- All elements should be children of ShakeParent/TiltParent
- This ensures they animate with the card
- Don't animate text separately (will look jittery)

---

## Checklist Before Testing

- [ ] CharacterPortrait Image created and positioned
- [ ] CharacterNameText TMP created and styled
- [ ] HealthText TMP created and positioned
- [ ] All three assigned to CharacterCardVisual component
- [ ] Placeholder text looks good in Scene view
- [ ] Elements don't overlap
- [ ] Text is readable
- [ ] Prefab saved
- [ ] HeroData assets have required data (Image, Name, Health)

---

## Next Steps

1. ✅ Complete prefab UI setup (this guide)
2. Create 3-5 HeroData assets with real data
3. Configure CharacterSelectionManager in scene
4. Test card generation in Play mode
5. Integrate with combat system

---

**Reference**: See `SETUP_CHARACTER_SYSTEM.md` for full system setup.
