**Status: Ready-to-implement. This document matches the current implementation branch.**

# Character Build Preset System - Corrected Architecture

## System Overview

This system allows players to select 4 character slots, each mapped to a specific hero (Mandirigma, Manggagayaw, Bagani, Babaylan), and choose build presets (Glass Cannon, Berserker, Bruiser, etc.) for each hero. **Slot order determines spawn and attack order in combat.**

> **Implemented:** Only Option A (fixed slot mapping) is supported. Each slot is hard-mapped to a hero. Player-selectable hero assignment (Option B) is not implemented in this branch. See the "Critical Design Decisions" section for details.

---

## Visual Flow

// ...existing code...

### User clicks Slot 1:

```
┌────────────────────────────────────────────────────────────┐
│       PRESET SELECTION CANVAS (Overlays Main Canvas)       │
├────────────────────────────────────────────────────────────┤
│                                                             │
│         Select Build for Hero: Mandirigma                   │
│                                                             │
│  ┌────────────┐  ┌────────────┐  ┌────────────┐          │
│  │ Glass      │  │ Berserker  │  │ Bruiser    │          │
│  │ Cannon     │  │ Set        │  │ Set        │          │
│  │ Set        │  │            │  │            │          │
│  │            │  │            │  │            │          │
│  │ HP: 650    │  │ HP: 700    │  │ HP: 800    │          │
│  │ ATK: High  │  │ ATK: High  │  │ ATK: Med   │          │
│  │ DEF: Low   │  │ DEF: Med   │  │ DEF: High  │          │
│  └────────────┘  └────────────┘  └────────────┘          │
│                                                             │
│                      [Back Button]                          │
└────────────────────────────────────────────────────────────┘
```

### User selects "Glass Cannon Set":

```
┌────────────────────────────────────────────────────────────┐
│          MAIN SELECTION CANVAS (Back to Main View)         │
├────────────────────────────────────────────────────────────┤
│                                                             │
│  [Slot 1]    [Slot 2]    [Slot 3]    [Slot 4]            │
│  Mandirigma   Empty       Empty       Empty                │
│  (preset img)                                               │
│                                                             │
│  Current Build: Glass Cannon Set                            │
│                                                             │
│                        [Save Button]                        │
└────────────────────────────────────────────────────────────┘
```

---

## Data Flow

// ...existing code...

---

## Key Components

### 1. CharacterBuildPreset (ScriptableObject)

```csharp
using UnityEngine;

public class CharacterBuildPreset : ScriptableObject
{
    public Sprite PresetSprite; // Sprite with baked-in name + stats
    public string PresetName;   // e.g., "Glass Cannon Set"
    public int HealthModifier;
    public int AttackModifier;
    public int MagicModifier;
    public int DefenseModifier;

    /// <summary>
    /// (No-op) Stat combination is handled by CharacterSlotData.GetFinalStats.
    /// This method is intentionally left empty. To get the final stats, use:
    ///   slot.GetFinalStats() // sums hero base stats + preset modifiers
    /// Calculation order: final = hero base + preset modifier (no mutation)
    /// </summary>
    public void ApplyModifiers(HeroData hero)
    {
        // No-op: see CharacterSlotData.GetFinalStats for stat calculation logic
    }
}
```

### 2. HeroData (ScriptableObject) - Updated

```csharp
using UnityEngine;
using System.Collections.Generic;

public class HeroData : ScriptableObject
{
    public string HeroName;
    public Sprite Portrait;
    public int BaseHealth;
    public int BaseAttack;
    public int BaseMagic;
    public int BaseDefense;
    public List<CharacterBuildPreset> BuildPresets; // typically 3 presets per hero
}
```

### 3. CharacterSlotData (Class)

```csharp
public class CharacterSlotData
{
    public HeroData Hero { get; set; }
    public CharacterBuildPreset SelectedPreset { get; set; }
    public int SlotIndex { get; private set; } // 0-3

    public CharacterSlotData(int slotIndex)
    {
        SlotIndex = slotIndex;
        Hero = null;
        SelectedPreset = null;
    }

    public bool IsComplete => Hero != null && SelectedPreset != null;

    public (int health, float attack, float magic, float defense) GetFinalStats()
    {
        if (Hero == null)
            return (0, 0, 0, 0);
        int health = Hero.BaseHealth;
        float attack = Hero.BaseAttack;
        float magic = Hero.BaseMagic;
        float defense = Hero.BaseDefense;
        if (SelectedPreset != null)
        {
            health += SelectedPreset.HealthModifier;
            attack += SelectedPreset.AttackModifier;
            magic += SelectedPreset.MagicModifier;
            defense += SelectedPreset.DefenseModifier;
        }
        return (health, attack, magic, defense);
    }
}
```

### 4. CharacterTransitionData (ScriptableObject) - Updated

```csharp
using UnityEngine;

public class CharacterTransitionData : ScriptableObject
{
    public CharacterSlotData[] CharacterSlots = new CharacterSlotData[4];

    /// <summary>
    /// Returns true if at least one slot is non-null and complete (has both Hero and Preset).
    /// </summary>
    public bool HasValidData()
    {
        if (CharacterSlots == null) return false;
        foreach (var slot in CharacterSlots)
        {
            if (slot != null && slot.IsComplete)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns an array of all non-null, complete slots in original slot index order.
    /// Order is preserved: slot 0 is first, slot 3 is last. Only slots with both Hero and Preset are included.
    /// </summary>
    public CharacterSlotData[] GetCompleteSlots()
    {
        if (CharacterSlots == null) return new CharacterSlotData[0];
        List<CharacterSlotData> result = new List<CharacterSlotData>(4);
        for (int i = 0; i < CharacterSlots.Length; i++)
        {
            var slot = CharacterSlots[i];
            if (slot != null && slot.IsComplete)
                result.Add(slot);
        }
        return result.ToArray();
    }

    /// <summary>
    /// Clears all slot references and resets transient fields. After calling, all slots are empty but slot indices are preserved.
    /// </summary>
    public void Clear()
    {
        if (CharacterSlots == null || CharacterSlots.Length != 4)
            CharacterSlots = new CharacterSlotData[4];
        for (int i = 0; i < CharacterSlots.Length; i++)
            CharacterSlots[i] = new CharacterSlotData(i);
    }
}
```

### 5. Main Selection UI (To Be Created by You)

```csharp
// Responsibilities:
- Display 4 clickable slots
- Show "Current Build: X" text below slots
- Handle slot clicks → show preset selection
- Update slot visuals when preset selected
- Save button → write to CharacterTransitionData
```

### 6. Preset Selection UI (To Be Created by You)

```csharp
// Responsibilities:
- Show 3 preset cards (sprites from HeroData.BuildPresets)
- Handle preset selection
- Hide self and return to main canvas
- Notify main UI to update slot display
```

### 7. MatchSetupSystem (Updated Integration)

```csharp
// Modified to:
- Read CharacterSlots[] from CharacterTransitionData
- Spawn heroes in slot order (0, 1, 2, 3)
- Apply preset modifiers to base stats
- Attack order follows slot order
```

---

## Implementation Checklist

### Assets to Create in Unity:

#### 1. Create Build Preset Assets

```
For each hero (Mandirigma, Manggagayaw, Bagani, Babaylan):

1. Create 3 CharacterBuildPreset assets:
   - Assets/Data/Presets/Mandirigma_GlassCannon.asset
   - Assets/Data/Presets/Mandirigma_Berserker.asset
   - Assets/Data/Presets/Mandirigma_Bruiser.asset

2. For each preset, set:
   - PresetSprite: Assign the sprite containing character + build info + stats
   - PresetName: "Glass Cannon Set", etc.
   - Stat Modifiers: +50 ATK, -100 HP, etc.
```

#### 2. Update Hero Data Assets

```
For each HeroData asset:
1. Add the 3 build presets to BuildPresets list
2. Verify base stats are set correctly
```

#### 3. Create CharacterTransitionData Asset

```
1. Right-click Assets/Data/
2. Create → Data → Character Transition Data
3. Name: "CharacterTransitionData"
```

### UI to Create in Unity:

#### 1. Main Selection Canvas

```
GameObject: MainSelectionCanvas
├── Slot1Button (Button)
│   └── SlotImage (Image) - shows preset sprite when selected
├── Slot2Button (Button)
│   └── SlotImage (Image)
├── Slot3Button (Button)
│   └── SlotImage (Image)
├── Slot4Button (Button)
│   └── SlotImage (Image)
├── CurrentBuildText (TextMeshPro) - "Current Build: Glass Cannon Set"
└── SaveButton (Button)
```

#### 2. Preset Selection Canvas

```
GameObject: PresetSelectionCanvas (Initially disabled)
├── TitleText (TextMeshPro) - "Select Build for: {HeroName}"
├── Preset1Button (Button)
│   └── PresetSprite (Image) - shows CharacterBuildPreset.PresetSprite
├── Preset2Button (Button)
│   └── PresetSprite (Image)
├── Preset3Button (Button)
│   └── PresetSprite (Image)
└── BackButton (Button)
```

### Scripts to Create:

#### 1. MainSelectionUI.cs

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MainSelectionUI : MonoBehaviour
{
    [SerializeField] private CharacterTransitionData transitionData;
    [SerializeField] private PresetSelectionUI presetSelectionUI;
    [SerializeField] private Image[] slotImages; // 4 slots
    [SerializeField] private TMP_Text currentBuildText;
    [SerializeField] private Button saveButton;

    [Header("Scene Management")]
    [Tooltip("Name of the combat scene to load. Must match the name in Build Settings exactly.")]
    [SerializeField] private string combatSceneName = "Combat";

    // Explicit hero references for Option A (fixed mapping)
    [SerializeField] private HeroData mandirigmaHeroData;
    [SerializeField] private HeroData manggagayawHeroData;
    [SerializeField] private HeroData baganiHeroData;
    [SerializeField] private HeroData babaylanHeroData;

    // Track which slot is being edited
    private int currentSlotIndex = -1;

    void Start()
    {
        // Defensive initialization: ensure CharacterSlots is always length 4 and non-null
        if (transitionData.CharacterSlots == null || transitionData.CharacterSlots.Length != 4)
        {
            transitionData.CharacterSlots = new CharacterSlotData[4];
        }
        for (int i = 0; i < 4; i++)
        {
            if (transitionData.CharacterSlots[i] == null)
                transitionData.CharacterSlots[i] = new CharacterSlotData(i);
        }

        // Wire up slot buttons (assign listeners in Inspector or here)
        // ...
    }

    public void OnSlotClicked(int slotIndex)
    {
        currentSlotIndex = slotIndex;

        // Option A (fixed mapping): Each slot is mapped to a specific hero. Option B (player-selectable) is not implemented in this branch.
        HeroData hero = GetHeroForSlot(slotIndex);
        if (hero == null)
        {
            Debug.LogWarning($"[MainSelectionUI] No hero assigned for slot {slotIndex}.");
            return;
        }
        if (presetSelectionUI == null)
        {
            Debug.LogError("[MainSelectionUI] PresetSelectionUI reference is missing!");
            return;
        }
        // Show preset selection for this hero
        presetSelectionUI.ShowPresets(hero, OnPresetSelected);
    }

    /// <summary>
    /// Returns the fixed hero for a given slot index (Option A: Fixed Assignment)
    /// </summary>
    private HeroData GetHeroForSlot(int slotIndex)
    {
        switch (slotIndex)
        {
            case 0: return mandirigmaHeroData;
            case 1: return manggagayawHeroData;
            case 2: return baganiHeroData;
            case 3: return babaylanHeroData;
            default: return null;
        }
    }

    private void OnPresetSelected(CharacterBuildPreset preset)
    {
        // Defensive: check slot index and nulls
        if (currentSlotIndex < 0 || currentSlotIndex >= 4)
        {
            Debug.LogWarning($"[MainSelectionUI] Invalid slot index: {currentSlotIndex}");
            return;
        }
        if (preset == null)
        {
            Debug.LogWarning("[MainSelectionUI] Null preset selected.");
            return;
        }
        var slot = transitionData.CharacterSlots[currentSlotIndex];
        if (slot == null)
        {
            Debug.LogWarning($"[MainSelectionUI] Slot {currentSlotIndex} is null.");
            return;
        }
        slot.SelectedPreset = preset;

        // Update slot visual
        if (slotImages != null && currentSlotIndex < slotImages.Length && slotImages[currentSlotIndex] != null)
        {
            slotImages[currentSlotIndex].sprite = preset.PresetSprite;
        }

        // Update current build text
        if (currentBuildText != null)
            currentBuildText.text = $"Current Build: {preset.PresetName}";
    }

    public void OnSaveClicked()
    {
        // Defensive: ensure at least one slot is complete before saving
        bool hasComplete = false;
        foreach (var slot in transitionData.CharacterSlots)
        {
            if (slot != null && slot.IsComplete)
            {
                hasComplete = true;
                break;
            }
        }
        if (!hasComplete)
        {
            Debug.LogWarning("[MainSelectionUI] No complete slots to save.");
            return;
        }

        // TODO: Save transitionData as needed (e.g., persist to disk, PlayerPrefs, etc.)
        // Transition to combat scene (ensure scene name is correct and included in build settings)
        SceneManager.LoadScene(combatSceneName);
    }
}
```

#### 2. PresetSelectionUI.cs

```csharp
public class PresetSelectionUI : MonoBehaviour
{
    // Wire the BackButton's OnClick to OnBackClicked() in the Inspector to allow users to close this UI without making a selection.
    [SerializeField] private Image[] presetImages; // 3 preset slots
    [SerializeField] private Button[] presetButtons;
    [SerializeField] private TMP_Text titleText;

    private System.Action<CharacterBuildPreset> onPresetSelected;
    private HeroData currentHero;

    public void ShowPresets(HeroData hero, System.Action<CharacterBuildPreset> callback)
    {
        currentHero = hero;
        onPresetSelected = callback;

        titleText.text = $"Select Build for: {hero.HeroName}";

        // Display the 3 presets
        for (int i = 0; i < 3; i++)
        {
            // Always remove previous listeners
            presetButtons[i].onClick.RemoveAllListeners();

            if (i < hero.BuildPresets.Count)
            {
                presetImages[i].sprite = hero.BuildPresets[i].PresetSprite;
                presetImages[i].enabled = true;
                presetButtons[i].interactable = true;
                int index = i; // Capture for closure
                presetButtons[i].onClick.AddListener(() => OnPresetButtonClicked(index));
            }
            else
            {
                // Clear/disable unused buttons/images
                presetImages[i].sprite = null;
                presetImages[i].enabled = false;
                presetButtons[i].interactable = false;
            }
        }

        gameObject.SetActive(true);
    }

    private void OnPresetButtonClicked(int presetIndex)
    {
        var preset = currentHero.BuildPresets[presetIndex];
        onPresetSelected?.Invoke(preset);

        // Hide this UI
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Called by the BackButton to close the preset selection UI without making a selection.
    /// Wire this to the BackButton's OnClick in the Inspector.
    /// </summary>
    public void OnBackClicked()
    {
        gameObject.SetActive(false);
    }
}
```

#### 3. Update MatchSetupSystem.cs

```csharp
// In MatchSetupSystem.cs, replace the hero spawning logic:

[SerializeField] private CharacterTransitionData characterTransitionData;

private void Start()
{
    // Validate character data
    if (!characterTransitionData.HasValidData())
    {
        Debug.LogError("[MatchSetupSystem] No character data!");
        return;
    }

    // Get complete slots in order
    CharacterSlotData[] slots = characterTransitionData.GetCompleteSlots();

    // Spawn heroes in slot order
    for (int i = 0; i < slots.Length; i++)
    {
        SpawnHeroFromSlot(slots[i], i);
    }

    // Clear transition data (always, not just in Editor)
    characterTransitionData.Clear();

    // Continue with enemy spawning, etc.
}

private void SpawnHeroFromSlot(CharacterSlotData slot, int spawnIndex)
{
    // Get final stats with preset modifiers
    var (health, attack, magic, defense) = slot.GetFinalStats();

    Debug.Log($"[MatchSetupSystem] Spawning {slot.Hero.HeroName} with {slot.SelectedPreset.PresetName}");
    Debug.Log($"  Stats: HP={health}, ATK={attack}, MAG={magic}, DEF={defense}");

    // TODO: Your existing hero spawning logic
    // - Instantiate hero prefab
    // - Apply modified stats
    // - Set position based on spawnIndex
    // - Assign deck from slot.Hero.Deck
}
```

---

## Critical Design Decisions for You:

### Q1: How are heroes assigned to slots?

**Implemented: Option A (Fixed Assignment)**

- Slot 0 always = Mandirigma
- Slot 1 always = Manggagayaw
- Slot 2 always = Bagani
- Slot 3 always = Babaylan

> **Note:** The provided MainSelectionUI implementation only supports Option A (fixed slot mapping). If you require player-selectable hero assignment (Option B), you must implement a hero selection step before preset selection. See project documentation for extension guidance.

**Recommendation:** Option A is simpler and matches your images. The current codebase and UI are designed for this approach.

### Q2: Must all 4 slots be filled?

- **Yes**: Require all 4 heroes before allowing Save
- **No**: Allow 1-4 heroes, spawn only complete slots

**Recommendation**: Allow 1-4 for flexibility.

### Q3: Sprite Content

Your preset sprites should contain:

- ✅ Character portrait
- ✅ Build name ("Glass Cannon Set")
- ✅ Stat values (HP: 650, ATK: 85, etc.)
- ✅ Visual theme/icons

**No separate UI text elements needed** - it's all in the sprite!

---

## Summary

---

## 📋 Implementation Checklist (Quick Reference)

### Assets to Create

- [ ] 12 preset sprites (4 heroes × 3 presets each, each sprite contains portrait, build name, stats)
- [ ] 12 CharacterBuildPreset assets (one for each preset, assign sprite, name, stat modifiers)
- [ ] 4 HeroData assets (each with BuildPresets list referencing its 3 presets)
- [ ] 1 CharacterTransitionData asset

### UI to Build

- [ ] MainSelectionCanvas with 4 slot buttons (each with SlotImage), CurrentBuildText, SaveButton
- [ ] PresetSelectionCanvas with 3 preset buttons (each with PresetImage), TitleText, BackButton

### Scripts to Implement

- [ ] MainSelectionUI (handles slot clicks, updates slot images, manages CharacterTransitionData)
- [ ] PresetSelectionUI (shows preset options for selected hero, notifies MainSelectionUI)
- [ ] Update MatchSetupSystem to read CharacterTransitionData.CharacterSlots[] and apply preset modifiers

### Integration Steps

- [ ] Assign all references in Inspector (slotImages, presetImages, etc.)
- [ ] Test: Click slot → select preset → slot image updates → Save → combat scene
- [ ] In combat, verify correct hero, preset, and stats are used (see Debug.Log output)

---

**Order matters:** Slot index 0-3 determines spawn position and attack order in combat!
