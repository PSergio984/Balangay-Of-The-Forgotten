# Combat System Configuration Guide

## Overview

This guide covers the configuration of all major combat systems in the Balangay Of The Forgotten game. Follow these steps to properly set up heroes, enemies, combat, status effects, special cards, and audio.

---

## Table of Contents

1. [Hero Death System](#1-hero-death-system)
2. [Combat State Reset](#2-combat-state-reset)
3. [Status Effect UI Configuration](#3-status-effect-ui-configuration)
4. [Sound Data Assets](#4-sound-data-assets)
5. [Special Card System](#5-special-card-system)
6. [Target Mode Configuration](#6-target-mode-configuration)

---

## 1. Hero Death System

### What It Does

When a hero's health reaches zero, the death system:

- Greys out the hero sprite (RGBA: 0.4, 0.4, 0.4, 0.7)
- Disables the hero's animator (stops animations)
- Prevents the hero from being targeted by enemies or effects
- Skips the hero during card discard and status effect tick phases

### Configuration Required

**No additional configuration needed!** The death system is automatically integrated into:

- `CombatantView.cs` - Death state tracking via `IsDead` property
- `DamageSystem.cs` - Calls `MarkAsDead()` when health ≤ 0
- `EnemySystem.cs` - Filters dead heroes from targeting
- `HeroSystem.cs` - Skips dead heroes during turn reactions
- All `TargetMode` classes - Filter dead combatants from targeting

### How to Test

1. In Unity Editor, enter Play Mode with a combat scene
2. Reduce a hero's health to 0 (via debug commands or enemy attacks)
3. Verify:
   - Hero sprite turns grey
   - Hero stops animating
   - Enemies skip attacking the dead hero
   - Status effects don't tick on dead heroes

### Reviving Heroes

To revive a dead hero in code:

```csharp
heroView.Revive();        // Restores color and animator
heroView.ResetToMaxHP();  // Optional: reset health to max
```

---

## 2. Combat State Reset

### What It Does

Ensures each combat encounter starts with a clean slate:

- Resets `ActionSystem.isPerforming` flag
- Clears any pending action reactions
- Stops all running coroutines in ActionSystem

### Configuration Required

**No additional configuration needed!** The reset is automatically called in `MatchSetupSystem.SetupSequence()`.

### Verifying the Fix

This fixes the bug where "attack doesn't work after returning from a boss fight":

1. Complete a boss fight
2. Return to main menu
3. Start a new fight
4. Verify all actions work correctly

### Manual Reset (if needed)

If you need to reset combat state manually:

```csharp
ActionSystem.Instance.ResetCombatState();
```

---

## 3. Status Effect UI Configuration

### What It Does

The StatusEffectsUI system now supports:

- Displaying status effect icons with stack counts
- Optional name display for distinguishing same-type effects from different cards
- Automatic icon removal when stacks reach 0

### Configuration Steps

#### Step 1: Update StatusEffectUI Prefab

1. Navigate to your StatusEffectUI prefab
2. Add a new TextMeshPro - Text (UI) child for the effect name
3. In the `StatusEffectUI` component, assign the new text to `effectNameText` field
4. Position the name text below the icon (suggested anchors: bottom-center)

#### Step 2: Configure Name Mapping (Optional)

If you want automatic name mapping based on effect type and stacks, edit `StatusEffectsUI.cs`:

```csharp
private static readonly Dictionary<(StatusEffectType type, int stacks), string> effectNameMapping = new()
{
    // DEFENSE_DOWN effects from different cards:
    { (StatusEffectType.DEFENSE_DOWN, 10), "Bonecracked" },
    { (StatusEffectType.DEFENSE_DOWN, 15), "Bind" },
    { (StatusEffectType.DEFENSE_DOWN, 20), "Moonfall" },

    // Add more mappings as needed for your cards
};
```

#### Step 3: Using Custom Names in Code

To apply an effect with a custom name:

```csharp
statusEffectsUI.UpdateStatusEffectUI(StatusEffectType.DEFENSE_DOWN, 15, "Bind");
```

### Assigned Sprites

Ensure all these sprites are assigned in StatusEffectsUI:

- `armorSprite`
- `attackUpSprite`, `attackDownSprite`
- `burnSprite`
- `critUpSprite`, `critDownSprite`
- `defenseDownSprite`, `defenseUpSprite`
- `dmgUpSprite`
- `ignoreDefenseSprite`
- `invulnerableSprite`
- `rageSprite`
- `shieldSprite`
- `stunSprite`
- `devouredSprite`
- `tauntSprite`
- `tempHpSprite`
- `restingSprite`, `chargingSprite`

---

## 4. Sound Data Assets

### What It Does

Creates SoundData ScriptableObject assets for all combat audio clips, properly organized by category.

### Running the Batch Creator

1. In Unity Editor, go to menu: **Tools > Audio > Create Combat SoundData Assets**
2. Click to run the batch creator
3. Assets will be created in: `Assets/Data/Audio/SFX/Combat/`

### Folder Structure Created

```
Assets/Data/Audio/SFX/Combat/
├── Bosses/
│   ├── Apolaki/
│   │   ├── DaybreakFury_SFX.asset
│   │   ├── RadiantCharge_SFX.asset
│   │   └── ...
│   ├── Bakunawa/
│   ├── Bathala/
│   └── Mayari/
├── Heroes/
│   ├── Babaylan/
│   ├── Bagani/
│   ├── Mandirigma/
│   └── Mangangayaw/
└── MiniBosses/
    ├── Kapre/
    ├── Manananggal/
    ├── Sirena/
    └── Tiyanak/
```

### Configuring SoundData Assets

After creation, you may want to customize each asset:

1. Select the asset in Project window
2. Configure in Inspector:
   - **Volume**: 0-2 (default: 1)
   - **Pitch**: 0.1-3 (default: 1)
   - **Mixer Group**: Assign to SFX mixer group
   - **Spatial Blend**: 0 = 2D, 1 = 3D (combat sounds typically 0)
   - **Priority**: 0-256 (128 = default, 0 = highest priority)

### Using SoundData in Code

```csharp
// In your audio manager or effect system:
[SerializeField] private SoundData attackSound;

void PlayAttackSound()
{
    AudioManager.Instance.Play(attackSound);
}
```

---

## 5. Special Card System

### What It Does

Special cards are powerful buff cards obtained from mini-boss defeats:

- **Dagát ng Kabisayaan**: +15% DMG to all players, 2 stacks
- **Daragang Magayon**: +25% DEF to 2 players, 1 stack
- **Bundok Pulag**: No cooldown for all skills, 4 rounds

### Configuration Steps

#### Step 1: Create SpecialCardCollectionData

1. Right-click in Project window
2. Select **Create > Data > Special Card Collection Data**
3. Name it `SpecialCardCollectionData`

#### Step 2: Create SpecialCardData Assets

1. Right-click in Project window
2. Select **Create > SpecialCards > Special Card Data**
3. Create one for each special card type

For each SpecialCardData, configure:

- **Card Id**: Unique identifier (e.g., "dagat_ng_kabisayaan")
- **Card Name**: Display name (e.g., "Dagát ng Kabisayaan")
- **Card Sprite**: Visual sprite for the card
- **Effect Type**: DamageUp, DefenseUpTwoTargets, or NoCooldown
- **Effect Percentage**: Amount of buff (e.g., 15 for 15%)
- **Duration**: Number of turns
- **Target Count**: How many targets to affect

#### Step 3: Add Cards to Collection

1. Select your SpecialCardCollectionData asset
2. In the **All Special Card Assets** list, add all created SpecialCardData assets

#### Step 4: Configure SpecialCardPanelUI

In your combat scene:

1. Create a UI panel for special cards
2. Add the `SpecialCardPanelUI` component
3. Assign references:
   - **Special Card Collection**: Your SpecialCardCollectionData asset
   - **Card Container**: RectTransform for card buttons (use HorizontalLayoutGroup)
   - **Special Card Button Prefab**: Prefab with SpecialCardButtonUI component
   - **Max Display Slots**: Maximum cards to show (default: 3)

#### Step 5: Create SpecialCardButton Prefab

Create a prefab with:

- `SpecialCardButtonUI` component
- Image for card icon
- (Optional) TextMeshProUGUI for card name
- Button component

### Integration with Victory/Rewards

Special cards are awarded via the reward system:

1. In RewardData, assign the `specialCardReward` field
2. VictoryDefeatUI calls `SpecialCardCollectionData.AddSpecialCard()` when awarded

---

## 6. Target Mode Configuration

### What It Does

Target modes determine which combatants can be targeted by effects. All modes now automatically filter out dead combatants.

### Available Target Modes

| Target Mode         | Description                              |
| ------------------- | ---------------------------------------- |
| `AllHeroesTM`       | All living heroes                        |
| `HeroTM`            | Current active hero (if alive)           |
| `RandomTargetTM`    | One random living hero                   |
| `HighestHPTargetTM` | Living hero with highest HP              |
| `AllEnemiesTM`      | All enemies                              |
| `EveryoneTM`        | All living combatants (heroes + enemies) |

### Creating New Target Modes

1. Create a new class inheriting from `TargetMode`
2. Override `GetTargets()` method
3. Always filter dead combatants:

```csharp
public class MyCustomTM : TargetMode
{
    public override List<CombatantView> GetTargets()
    {
        var heroes = HeroSystem.Instance.HeroViews;
        // Always filter dead combatants
        return heroes.Where(h => h != null && !h.IsDead).ToList<CombatantView>();
    }
}
```

---

## Troubleshooting

### Common Issues

#### "Attack doesn't work after returning from boss fight"

- **Cause**: ActionSystem.isPerforming was not reset
- **Solution**: The fix is now automatic in MatchSetupSystem

#### "Dead heroes still being targeted"

- **Cause**: Target mode not filtering IsDead
- **Solution**: All target modes are now updated to filter dead combatants

#### "Status effect icons not showing"

- **Cause**: Missing sprite assignment
- **Solution**: Check StatusEffectsUI component has all sprites assigned

#### "Special card panel not appearing"

- **Cause**: SpecialCardCollectionData not assigned or no cards collected
- **Solution**: Verify data assignment and check if `hideWhenEmpty` is true

### Debug Commands

Add these to your debug console or menu:

```csharp
// Kill current hero (for testing death system)
CurrentHeroUtil.GetCurrentHero().TakeDamage(9999);

// Revive all heroes
foreach (var hero in HeroSystem.Instance.HeroViews)
{
    hero.Revive();
    hero.ResetToMaxHP();
}

// Reset combat state manually
ActionSystem.Instance.ResetCombatState();

// Add test special card
specialCardCollection.AddSpecialCard(testSpecialCardData);
```

---

## Summary Checklist

- [ ] StatusEffectUI prefab has `effectNameText` assigned (optional)
- [ ] StatusEffectsUI has all status effect sprites assigned
- [ ] Run "Tools > Audio > Create Combat SoundData Assets" to create audio assets
- [ ] Configure SoundData assets with appropriate mixer groups
- [ ] Create SpecialCardCollectionData asset
- [ ] Create SpecialCardData assets for each special card type
- [ ] Configure SpecialCardPanelUI in combat scene
- [ ] Test hero death system (grey out, targeting skip)
- [ ] Test combat state reset (new fights work correctly)
- [ ] Test special card activation and consumption

---

_Last Updated: December 2024_
_For Balangay Of The Forgotten - Turn-Based Combat System_
