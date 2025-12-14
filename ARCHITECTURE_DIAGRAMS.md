# Visual Architecture Diagrams - Card Preset System

## 🏗️ System Architecture Overview

### Before Fix (Issues):

```
┌─────────────────────────────────────────────────────┐
│                   Canvas                             │
│                                                      │
│  ┌────────────────────────────────────────────┐    │
│  │  HorizontalCharacterCardHolder             │    │
│  │                                             │    │
│  │  ┌───────────┐  ┌───────────┐  ┌────────┐ │    │
│  │  │  Slot 1   │  │  Slot 2   │  │ Slot 3 │ │    │
│  │  │           │  │           │  │        │ │    │
│  │  │  ┌─────┐  │  │  ┌─────┐  │  │ ┌────┐ │ │    │
│  │  │  │Card │  │  │  │Card │  │  │ │Card│ │ │    │
│  │  │  │ ┌─┐ │  │  │  │ ┌─┐ │  │  │ │┌─┐│ │ │    │
│  │  │  │ │█│ │  │  │  │ │█│ │  │  │ ││█││ │ │    │
│  │  │  │ └─┘ │  │  │  │ └─┘ │  │  │ │└─┘│ │ │    │
│  │  │  │"Text"   │  │  │"Text"   │  │"Txt│ │ │    │
│  │  │  │  ↑  │  │  │  │  ↑  │  │  │ │ ↑ │ │ │    │
│  │  │  └──│──┘  │  │  └──│──┘  │  │ └─│─┘ │ │    │
│  │  └─────│─────┘  └─────│─────┘  └───│───┘ │    │
│  │        └───────────────┴─────────────┘     │    │
│  │   ❌ Text moves/rotates with card!         │    │
│  └────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────┘
```

### After Fix (Correct):

```
┌─────────────────────────────────────────────────────────┐
│                   Canvas                                 │
│                                                          │
│  ┌────────────────────────────────────────────────┐    │
│  │  HorizontalCharacterCardHolder                 │    │
│  │                                                 │    │
│  │  ┌───────────┐  ┌───────────┐  ┌──────────┐   │    │
│  │  │  Slot 1   │  │  Slot 2   │  │  Slot 3  │   │    │
│  │  │+ Slot Cmp │  │+ Slot Cmp │  │+ Slot Cmp│   │    │
│  │  │           │  │           │  │          │   │    │
│  │  │  ┌─────┐  │  │  ┌─────┐  │  │  ┌─────┐│   │    │
│  │  │  │Card │  │  │  │Card │  │  │  │Card ││   │    │
│  │  │  │ ┌─┐ │  │  │  │ ┌─┐ │  │  │  │ ┌─┐ ││   │    │
│  │  │  │ │█│ │  │  │  │ │█│ │  │  │  │ │█│ ││   │    │
│  │  │  │ └─┘ │  │  │  │ └─┘ │  │  │  │ └─┘ ││   │    │
│  │  │  └─────┘  │  │  └─────┘  │  │  └─────┘│   │    │
│  │  │           │  │           │  │          │   │    │
│  │  │ "Build:  │  │  "Build:  │  │  "Build: │   │    │
│  │  │  Glass   │  │  Ranger"  │  │  Wall"   │   │    │
│  │  │  Cannon" │  │           │  │          │   │    │
│  │  └───────────┘  └───────────┘  └──────────┘   │    │
│  │   ✅ Text fixed under slot, doesn't move!     │    │
│  └────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────┘
```

## 🎴 Card Sprite Flow

### Initial State (No Preset Selected):

```
┌─────────────────────────┐
│   CharacterCard         │
│                         │
│   ┌─────────────────┐   │
│   │                 │   │
│   │   ROLE CARD     │   │  ← Shows HeroData.RoleCard
│   │   (No Stats)    │   │    (Base card sprite)
│   │                 │   │
│   │   [Hero Art]    │   │
│   │                 │   │
│   └─────────────────┘   │
│                         │
│  Slot Text:             │
│  "None Selected"        │
└─────────────────────────┘
```

### After Preset Selected:

```
┌─────────────────────────┐
│   CharacterCard         │
│                         │
│   ┌─────────────────┐   │
│   │ GLASS CANNON    │   │  ← Shows Preset.PresetSprite
│   │                 │   │    (Card with baked stats)
│   │   [Hero Art]    │   │
│   │                 │   │
│   │ HP: 450         │   │
│   │ ATK: 120        │   │
│   │ DEF: 20         │   │
│   └─────────────────┘   │
│                         │
│  Slot Text:             │
│  "Current Build:        │
│   Glass Cannon Set"     │
└─────────────────────────┘
```

## 🔄 Drag-and-Drop Flow

### Step 1: Initial State

```
Slot A                  Slot B
┌────────────┐         ┌────────────┐
│  Card 1    │         │  Card 2    │
│  [Warrior] │         │  [Mage]    │
│            │         │            │
│ "Build:    │         │ "Build:    │
│  Glass     │         │  Healer"   │
│  Cannon"   │         │            │
└────────────┘         └────────────┘
   Warrior/Glass        Mage/Healer
```

### Step 2: Dragging Card 1 to Slot B

```
Slot A                  Slot B (hover)
┌────────────┐         ┌────────────┐
│  (empty)   │         │  Card 2    │  ┌──────┐
│            │         │  [Mage]    │  │Card 1│ ← dragging
│            │         │            │  │[Warr]│
│ "Build:    │         │ "Build:    │  └──────┘
│  Glass     │         │  Healer"   │
│  Cannon"   │         │            │
└────────────┘         └────────────┘
   (stale text)         (about to swap)
```

### Step 3: After Swap Complete

```
Slot A                  Slot B
┌────────────┐         ┌────────────┐
│  Card 2    │         │  Card 1    │
│  [Mage]    │         │  [Warrior] │
│            │         │            │
│ "Build:    │         │ "Build:    │
│  Healer"   │  ✅     │  Glass     │  ✅
│            │         │  Cannon"   │
└────────────┘         └────────────┘
   Mage/Healer          Warrior/Glass
   (updated!)           (updated!)
```

## 🔧 CharacterSlot Component Diagram

```
┌──────────────────────────────────────────────┐
│         CharacterSlot Component              │
├──────────────────────────────────────────────┤
│                                              │
│  [SerializeField]                            │
│  ├─ buildPresetText: TMP_Text              │
│  └─ cardSpawnPoint: Transform               │
│                                              │
│  [HideInInspector]                           │
│  └─ AssignedCard: CharacterCard             │
│                                              │
│  Methods:                                    │
│  ├─ AssignCard(card)                        │
│  │   └─ Subscribe to card.SelectEvent       │
│  │   └─ UpdateBuildText()                   │
│  │                                           │
│  ├─ RemoveCard()                            │
│  │   └─ Unsubscribe from events             │
│  │   └─ UpdateBuildText()                   │
│  │                                           │
│  ├─ OnCardPresetChanged(card, selected)     │
│  │   └─ UpdateBuildText()                   │
│  │                                           │
│  └─ UpdateBuildText()                       │
│      └─ If no card: "None Selected"         │
│      └─ If no preset: "None Selected"       │
│      └─ If preset: "Current Build:\n[Name]" │
└──────────────────────────────────────────────┘
```

## 📊 Event Flow Diagram

### Preset Selection Event Flow:

```
User clicks Card
       │
       ├──> CardPresetManager.OnCardClicked()
       │         │
       │         └──> PresetSelectionUI.ShowForCard(card)
       │                    │
       │                    └──> Display 3 preset options
       │
User clicks Preset Button
       │
       ├──> PresetSelectionUI.OnPresetSelected(index)
       │         │
       │         ├──> card.SelectedPreset = preset
       │         │
       │         ├──> CharacterCardVisual.UpdatePresetData(preset)
       │         │         │
       │         │         └──> Update card image to PresetSprite
       │         │
       │         └──> CharacterSlot.UpdateBuildText()
       │                   │
       │                   └──> Display "Current Build: [Name]"
       │
       └──> Close PresetSelectionUI
```

### Drag-and-Drop Event Flow:

```
User drags Card from Slot A
       │
       ├──> CharacterCard.OnBeginDrag()
       │         │
       │         └──> HorizontalCharacterCardHolder.BeginDrag()
       │
User drops on Slot B
       │
       ├──> CharacterCard.OnEndDrag()
       │         │
       │         └──> HorizontalCharacterCardHolder.EndDrag()
       │                   │
       │                   └──> Detect overlap with Slot B
       │                             │
       │                             └──> Call Swap(indexB)
       │                                       │
       │                                       ├──> SlotA.RemoveCard()
       │                                       │      └──> Update text to "None Selected"
       │                                       │
       │                                       ├──> SlotB.RemoveCard()
       │                                       │      └──> Update text to "None Selected"
       │                                       │
       │                                       ├──> Move cards between slots
       │                                       │
       │                                       ├──> SlotA.AssignCard(CardB)
       │                                       │      └──> Update text to CardB preset
       │                                       │
       │                                       └──> SlotB.AssignCard(CardA)
       │                                              └──> Update text to CardA preset
       │
       └──> Both slots now show correct build text!
```

## 🎯 Key Architecture Points

### 1. Separation of Concerns

- **CharacterCard**: Handles card logic, movement, selection
- **CharacterCardVisual**: Handles card visual effects, sprite updates
- **CharacterSlot**: Handles slot-specific UI (build text) that doesn't move
- **HorizontalCharacterCardHolder**: Manages card collection and swapping

### 2. Event-Driven Updates

- CharacterSlot subscribes to card's SelectEvent
- When preset changes, slot automatically updates text
- No polling or manual refresh needed

### 3. Fixed vs. Dynamic UI

- **Dynamic (moves with card)**: Card sprite, hover effects, drag visuals
- **Fixed (stays with slot)**: Build text, slot background, slot frame

### 4. Data Flow

```
HeroData.RoleCard → CharacterCard.imageComponent (initial)
                            ↓
User selects preset → CharacterBuildPreset.PresetSprite
                            ↓
                    CharacterCard.imageComponent (updated)
                            ↓
                    CharacterSlot.buildPresetText (updated via event)
```

---

**These diagrams illustrate the complete architecture and flow of the card-based preset selection system with proper UI separation.**
