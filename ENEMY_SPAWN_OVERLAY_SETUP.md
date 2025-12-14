# Enemy Spawn Overlay & Victory/Defeat UI Setup Guide

## Overview

This guide explains how to set up the new Enemy Spawn Overlay and Victory/Defeat UI systems in Unity.

## 1. Enemy Spawn Overlay UI Setup

### GameObject Structure

Create a new GameObject in your Combat scene:

```
EnemySpawnOverlayUI (GameObject)
├── Canvas (Canvas component - Screen Space Overlay, Sort Order: 1000)
│   ├── BlackBackground (Image - Full screen, Color: Black)
│   └── EnemyPortraitImage (Image - 16:9 aspect ratio, centered)
```

### Component Setup

1. **Add EnemySpawnOverlayUI Component**

   - Add `EnemySpawnOverlayUI` script to the root GameObject
   - Assign references in Inspector:
     - **Overlay Canvas**: The Canvas component
     - **Black Background**: The black Image component
     - **Banner Image**: The Image component for displaying banner sprites (Mini Boss Battle, Main Boss Battle)
     - **Mini Boss Banner Sprite**: The sprite asset for "Mini Boss Battle" banner
     - **Main Boss Banner Sprite**: The sprite asset for "Main Boss Battle" banner

2. **Canvas Configuration**

   - Render Mode: Screen Space - Overlay
   - Sort Order: 1000 (to appear above other UI)
   - Canvas Scaler: Scale With Screen Size
   - Reference Resolution: 1920x1080

3. **Black Background Image**

   - Anchor: Stretch to fill entire screen
   - Color: Black (0, 0, 0, 255)
   - Add CanvasGroup component (auto-added by script if missing)

4. **Banner Image**

   - Anchor: Center
   - Size: 1920x1080 (16:9 ratio) or adjust to fit your design
   - Preserve Aspect: Enabled
   - Add CanvasGroup component (auto-added by script if missing)
   - This will display the "Mini Boss Battle" or "Main Boss Battle" banner sprites

5. **Banner Sprites (In Inspector)**
   - Assign **Mini Boss Banner Sprite**: The "Mini Boss Battle" banner sprite (from Assets/Art/UI/Combat/minibossbattlebanner.png)
   - Assign **Main Boss Banner Sprite**: The "Main Boss Battle" banner sprite (from Assets/Art/UI/Combat/mainbossbattlebanner.png)

### Animation Settings (Optional)

Adjust these in the Inspector if needed:

- **Fade In Duration**: 0.5s (default)
- **Slide Duration**: 0.8s (default)
- **Bounce Distance**: 50 pixels (default)
- **Bounce Duration**: 0.3s (default)
- **Hold Duration**: 1.5s (default)
- **Fade Out Duration**: 0.5s (default)

## 2. Victory/Defeat UI Setup

### GameObject Structure

Create a new GameObject in your Combat scene:

```
VictoryDefeatUI (GameObject)
├── Canvas (Canvas component - Screen Space Overlay, Sort Order: 2000)
│   ├── BannerPanel (RectTransform - Container)
│   │   ├── BannerBackground (Image - Optional decorative background)
│   │   ├── VictoryImage (Image - Victory banner sprite)
│   │   ├── DefeatImage (Image - Defeat banner sprite)
│   │   └── ContinueButton (Button)
│   │       └── Text (TMP_Text - "Continue")
```

### Component Setup

1. **Add VictoryDefeatUI Component**

   - Add `VictoryDefeatUI` script to the root GameObject
   - Assign references in Inspector:
     - **Banner Panel**: The RectTransform container
     - **Victory Image**: Image component for victory banner
     - **Defeat Image**: Image component for defeat banner
     - **Banner Background**: Optional background Image
     - **Continue Button**: The Button component
     - **Continue Button Text**: TMP_Text component (optional)

2. **Banner Sprites**

   - Assign **Victory Sprite**: Your victory banner sprite asset
   - Assign **Defeat Sprite**: Your defeat banner sprite asset

3. **Canvas Configuration**

   - Render Mode: Screen Space - Overlay
   - Sort Order: 2000 (to appear above other UI including overlay)
   - Canvas Scaler: Scale With Screen Size
   - Reference Resolution: 1920x1080

4. **Banner Images**

   - VictoryImage and DefeatImage should be centered
   - Add CanvasGroup components (auto-added by script if missing)
   - Initially hidden, shown by script

5. **Continue Button**
   - Position: Below or on the banner
   - Text: "Continue" (or customize)
   - Add CanvasGroup component (auto-added by script if missing)
   - Button will automatically transition to MapSelection scene when clicked

### Animation Settings (Optional)

Adjust these in the Inspector if needed:

- **Fade In Duration**: 0.4s (default)
- **Hold Duration**: 1.0s (default)
- **Button Fade In Duration**: 0.3s (default)

## 3. Integration Notes

### Enemy Spawn Overlay

- **Automatic Integration**: The overlay is automatically called by `EnemySystem` before spawning:
  - First enemy (index 0) = Shows "Mini Boss Battle" banner
  - Second enemy (index 1) = Shows "Main Boss Battle" banner
- **Banner Sprites**: Uses fixed banner sprites (not enemy portraits) to indicate the type of encounter
- **Timing**: Overlay animation completes before enemy spawns (approximately 4 seconds)

### Victory/Defeat UI

- **Victory**: Automatically triggered when all enemies are defeated (via `EnemySystem.TriggerVictory()`)
- **Defeat**: Automatically triggered when all heroes die (via `DamageSystem.CheckForDefeat()`)
- **Scene Transition**: Continue button uses `SceneController` to return to MapSelection scene

## 4. Testing Checklist

- [ ] Enemy spawn overlay appears before first enemy (miniboss)
- [ ] Enemy spawn overlay appears before second enemy (main boss)
- [ ] Overlay animation plays correctly (fade in → slide → bounce → fade out)
- [ ] Victory banner appears when all enemies are defeated
- [ ] Defeat banner appears when all heroes die
- [ ] Continue button appears after banner animation
- [ ] Continue button transitions to MapSelection scene correctly
- [ ] All animations are smooth and properly timed

## 5. Troubleshooting

### Overlay Not Showing

- Check that `EnemySpawnOverlayUI.Instance` exists in scene
- Verify Canvas is active and has correct Sort Order
- Check console for errors about missing references

### Victory/Defeat Not Triggering

- Verify `VictoryDefeatUI.Instance` exists in scene
- Check that all required references are assigned in Inspector
- Verify sprites are assigned for victory and defeat banners

### Scene Transition Not Working

- Ensure `SceneController.Instance` exists in scene
- Verify scene name "MapSelection" exists in build settings
- Check console for scene transition errors

## 6. Customization

### Changing Overlay Timing

Edit `EnemySpawnOverlayUI.cs` animation durations or adjust in Inspector.

### Changing Banner Sprites

Replace the sprite assets assigned in the Inspector - the script will use whatever sprites you assign.

### Customizing Continue Button

Modify the button's appearance, position, or text in the Unity Inspector. The script handles the click event automatically.
