# UI Setup Guide - Card-Based Preset System

This guide shows how to integrate build preset selection with your existing character card UI system.

## ⚠️ Critical: Fixed Stats Architecture

**IMPORTANT:** Build presets use **FIXED stats** that completely **override** hero base stats.

- UI should display the preset's exact stats (e.g., HP: 650), not "Base + Modifier"
- When showing preset details, use `SelectedPreset.Health`, NOT `Hero.Health + Preset.HealthModifier`
- All stat displays must reference the preset's final values

---

## System Overview

Your existing card system (`CharacterCard`, `CharacterCardVisual`, `HorizontalCharacterCardHolder`) already handles:

- Card display and selection
- Drag and drop
- Visual feedback

**New additions integrate preset selection:**

- Click a card → Opens preset selection UI overlay
- Select a preset → Updates card to show preset name and stats
- Card displays current build name below character info

---

## Required UI Structure

```
Canvas (GameObject)
├── HorizontalCharacterCardHolder (existing)
│   └── [Character Cards spawn here]
├── CardPresetManager (NEW - manages card↔preset interaction)
└── PresetSelectionUI (NEW - overlay panel, initially disabled)
    ├── PanelBackground (Image)
    ├── TitleText (TextMeshPro) "Select Build for: [Hero Name]"
    ├── Preset1Button (Button)
    │   └── PresetImage (Image) ← Shows preset sprite
    ├── Preset2Button (Button)
    │   └── PresetImage (Image)
    ├── Preset3Button (Button)
    │   └── PresetImage (Image)
    ├── CurrentBuildText (TextMeshPro) "Current: [Build Name]"
    └── BackButton (Button)
```

---

## Step-by-Step Setup

### Step 1: Add Build Preset Text to CharacterCardVisual Prefab

Your character cards already show hero portrait, name, and HP. Now add preset name display:

1. Open your **CharacterCardVisual** prefab
2. Find the hierarchy with `CharacterNameText` and `HealthText`
3. Add a new **UI → Text - TextMeshPro** as a sibling
4. Name it: `BuildPresetText`
5. Position it below the health text (or wherever you want)
6. Set placeholder text: "No Build"
7. **Select CharacterCardVisual component** in Inspector
8. In **Character Data Display** section, assign `BuildPresetText` to the **Build Preset Text** field

### Step 2: Create Preset Selection UI Overlay

This is a full-screen overlay that appears when clicking a card:

1. In your scene, under Canvas, add **UI → Panel** (or GameObject with Image)
2. Name it: `PresetSelectionPanel`
3. Set **RectTransform** to stretch full screen (anchors: 0,0 to 1,1)
4. Set **Image color** to semi-transparent black (e.g., `#000000AA`) for overlay effect
5. **Disable the GameObject** (it will show when a card is clicked)
6. Add **PresetSelectionUI** script component to this panel

Inside `PresetSelectionPanel`, create:

**Title Text:**

- Add **UI → Text - TextMeshPro**
- Name: `TitleText`
- Position: Top-center
- Text: "Select Build for: Hero Name"
- Font Size: 32

**Preset Buttons (create 3 of these):**
For each preset slot (1-3):

- Add **UI → Button**
- Name: `Preset1Button`, `Preset2Button`, `Preset3Button`
- Position: Horizontal row in center
- Size: 200x300 (adjust to fit your preset sprites)
- Inside each button, add **UI → Image** child
- Name: `PresetImage`
- This will show the preset sprite

**Current Build Text:**

- Add **UI → Text - TextMeshPro**
- Name: `CurrentBuildText`
- Position: Below preset buttons
- Text: "Current: None"
- Font Size: 24

**Back Button:**

- Add **UI → Button**
- Name: `BackButton`
- Position: Bottom-right or wherever you prefer
- Set button text: "Back" or "Close"

### Step 3: Link PresetSelectionUI Script

Select `PresetSelectionPanel`, find the **PresetSelectionUI** component:

- **Panel Root**: Assign the PresetSelectionPanel itself (this GameObject)
- **Title Text**: Drag `TitleText` TMP_Text
- **Preset Images**: Drag the 3 `PresetImage` Image components (array size 3)
- **Preset Buttons**: Drag the 3 `Preset1Button`, `Preset2Button`, `Preset3Button` (array size 3)
- **Back Button**: Drag `BackButton`
- **Current Build Text**: Drag `CurrentBuildText` TMP_Text

### Step 4: Add CardPresetManager to Scene

This connects card clicks to the preset UI:

1. Create empty GameObject in scene
2. Name: `CardPresetManager`
3. Add **CardPresetManager** script component
4. In Inspector:
   - **Card Holder**: Drag your `HorizontalCharacterCardHolder` GameObject
   - **Preset Selection UI**: Drag the `PresetSelectionPanel` (with PresetSelectionUI script)
   - **Transition Data**: Drag your `CharacterTransitionData` ScriptableObject asset
   - **Auto Wire Cards**: ✓ Enabled (automatically hooks up cards on Start)
   - **Enable Card Click To Open Presets**: ✓ Enabled

---

## How It Works

1. **Cards spawn** (via your existing system or CharacterSelectionManager)
2. **Player clicks a card** → Card's `SelectEvent` fires
3. **CardPresetManager** catches the event → Opens `PresetSelectionUI`
4. **Preset UI shows** → Displays 3 preset options for that hero
5. **Player selects preset** → Card updates to show preset name and stats
6. **Player clicks Back** → Preset UI closes, card shows updated info
7. **Repeat** for other cards as desired
8. **When ready** → Call `CardPresetManager.PrepareTransitionData()` to collect all selections

---

## Example Flow

```
┌─────────────────────────────────────────────────┐
│ [Card1] [Card2] [Card3] [Card4]               │  ← Your existing cards
│  Hero1   Hero2   Hero3   Hero4                │
│ "Tank"  "Mage"   ...     ...                  │
└─────────────────────────────────────────────────┘
           ↓ Player clicks Card1

┌──────────────────────────────────────────────────┐
│         Select Build for: Hero1                  │  ← Overlay appears
│                                                  │
│  [Glass Cannon]  [Berserker]  [Bruiser]         │  ← 3 preset options
│   HP: 650        HP: 700      HP: 800           │
│   ATK: High      ATK: High    ATK: Med          │
│                                                  │
│  Current: Tank                                   │  ← Shows current
│                                [Back]            │
└──────────────────────────────────────────────────┘
           ↓ Player clicks "Glass Cannon"

┌─────────────────────────────────────────────────┐
│ [Card1]         [Card2] [Card3] [Card4]        │  ← Card updated!
│  Hero1           Hero2   Hero3   Hero4         │
│ "Glass Cannon"  "Mage"   ...     ...           │  ← Shows new build
│  HP: 650                                        │  ← Shows preset HP
└─────────────────────────────────────────────────┘
```

---

## Integration with Combat Scene

When ready to transition to combat:

```csharp
// In your scene transition script (e.g., on Save button click):
CardPresetManager manager = FindObjectOfType<CardPresetManager>();
int selectedCount = manager.PrepareTransitionData();

if (selectedCount > 0)
{
    SceneManager.LoadScene("Combat");
    // MatchSetupSystem will read CharacterTransitionData.CharacterSlots
}
else
{
    Debug.LogWarning("No cards with presets selected!");
}
```

---

## Tips

- Preset sprites should show full character + build info (artist creates these)
- Card displays "Build Name" below character name
- HP updates to show preset's fixed value (not hero base)
- If hero has no presets assigned, clicking card logs a warning
- PresetSelectionUI automatically highlights currently selected preset
- Use semi-transparent overlay to dim cards behind preset selection
- Back button closes overlay without changing selection

---

## Testing Checklist

- [ ] Cards display correctly with hero data
- [ ] Clicking a card opens preset selection overlay
- [ ] Overlay shows 3 preset options (or however many hero has)
- [ ] Clicking a preset updates card's "Build Name" text
- [ ] Card HP updates to show preset's fixed HP value
- [ ] Currently selected preset is highlighted in overlay
- [ ] Back button closes overlay
- [ ] Clicking another card shows that hero's presets
- [ ] Can select presets for multiple cards
- [ ] PrepareTransitionData() collects all card-preset pairs correctly

---

## Advanced: Adding More Stats Display

If you want to show ATK, DEF, MAG on cards (beyond just HP), follow this pattern:

### 1. Add Text Fields to Prefab

- Add **UI → Text - TextMeshPro** for each stat (AttackText, DefenseText, MagicText)
- Position them below HP text or in a stat row

### 2. Add Fields to CharacterCardVisual.cs

```csharp
// Add to Character Data Display section:
[SerializeField] private TMP_Text attackText;
[SerializeField] private TMP_Text defenseText;
[SerializeField] private TMP_Text magicText;

// Update UpdateCharacterData() method:
if (attackText != null)
    attackText.text = $"ATK: {heroData.AttackPower}";
if (defenseText != null)
    defenseText.text = $"DEF: {heroData.Defense}";
if (magicText != null)
    magicText.text = $"MAG: {heroData.MagicPower}";

// Update UpdatePresetData() method:
if (attackText != null)
    attackText.text = $"ATK: {preset.AttackPower}";
if (defenseText != null)
    defenseText.text = $"DEF: {preset.Defense}";
if (magicText != null)
    magicText.text = $"MAG: {preset.MagicPower}";
```

### 3. Assign in Inspector

- Drag each new TMP_Text to corresponding field in CharacterCardVisual component

---

## Troubleshooting

**Cards not responding to clicks:**

- Check CardPresetManager is in scene with Card Holder assigned
- Verify autoWireCards is enabled
- Check Console for warnings about missing hero data

**Preset UI not showing:**

- Verify PresetSelectionPanel starts disabled
- Check PresetSelectionUI script has all references assigned
- Ensure hero has BuildPresets assigned in Inspector

**Card not updating after preset selection:**

- Check CharacterCardVisual has BuildPresetText field assigned
- Verify UpdatePresetData() is being called (add Debug.Log to test)
- Ensure preset has valid data (Health, PresetName, PresetSprite)

**Stats showing wrong values:**

- Confirm you're using preset stats (fixed values), not hero base stats
- Check UpdatePresetData() updates all stat displays
- Verify preset ScriptableObject has correct values

---

## Final Checklist

- [ ] BuildPresetText added to CharacterCardVisual prefab
- [ ] BuildPresetText field assigned in CharacterCardVisual component
- [ ] PresetSelectionPanel created with all UI elements
- [ ] PresetSelectionUI script configured with all references
- [ ] CardPresetManager in scene with Card Holder, Preset UI, and Transition Data assigned
- [ ] Hero assets have BuildPresets assigned (3 presets recommended)
- [ ] Tested: Click card → preset UI opens
- [ ] Tested: Select preset → card updates build name and stats
- [ ] Tested: Back button closes preset UI
- [ ] PrepareTransitionData() ready to call before scene transition

---

## Reference

- **Setup Guide**: `SETUP_CHARACTER_SYSTEM.md` - Full system setup
- **Architecture**: `CORRECTED_ARCHITECTURE.md` - Design decisions and data flow
- **Quick Start**: `QUICK_START.md` - Minimal setup steps
- **Implementation**: `IMPLEMENTATION_SUMMARY.md` - Complete system overview
