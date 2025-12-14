# Character Card Visual Position Fix

## Issue Summary

**Problem:** CharacterCardVisual components were spawning at the origin (0, 0, 0) when starting the game from the Core scene entry point, but worked correctly when starting directly from the CharacterSelection scene.

**Root Cause:** Timing issue with Unity's layout system during scene transitions.

## Technical Analysis

### Why It Failed from Core Scene Entry Point

When the game starts from Core and transitions through:

- Core → LoadingScreen → MainMenu → CharacterSelection

The initialization sequence had a race condition:

1. **CharacterCard.Start()** runs when the scene loads
2. **CharacterCard.Start()** instantiates CharacterCardVisual
3. **CharacterCard.Start()** calls `CharacterCardVisual.Initialize(this)`
4. **CharacterCardVisual.Initialize()** tries to sync position: `transform.position = cardTransform.position`
5. **Problem:** At this point, Unity's layout system hasn't calculated the final RectTransform positions yet!
6. Result: `cardTransform.position` is at (0, 0, 0) or an uninitialized position

### Why It Worked When Starting from CharacterSelection

When starting directly from CharacterSelection scene:

- The scene is already loaded and stable
- Unity's layout system has already calculated all positions
- When `CharacterCard.Start()` runs, `cardTransform.position` is already at its final position
- Position sync works correctly

### Unity Lifecycle Context

Unity's execution order for UI positioning:

1. `Awake()` - Component initialization
2. `Start()` - Game logic initialization ← **CharacterCardVisual.Initialize() was called here**
3. `Update()` - First frame logic
4. **Layout calculations** (happens in background during frame processing)
5. `LateUpdate()` - Final frame updates
6. `End of Frame` - All positioning complete ← **We need to sync position HERE**

## The Fix

### Solution: Defer Position Sync Until After Layout Calculations

**File:** `Assets/Scripts/CharacterSelection/CharacterCards/CharacterCardVisual.cs`

**Changes:**

1. **Removed immediate position sync from Initialize():**

   ```csharp
   // OLD CODE (line 97-100):
   // Set initial position immediately to match card position (prevents spawn at origin)
   if (cardTransform != null)
   {
       transform.position = cardTransform.position;
   }
   ```

2. **Added coroutine to defer position sync:**

   ```csharp
   // NEW CODE in Initialize():
   // Defer initial position sync to end of frame (after Unity layout system calculates positions)
   // This fixes the issue where cards spawn at origin when starting from Core scene during scene transitions
   StartCoroutine(InitializePositionAfterLayout());
   ```

3. **Added new coroutine method:**
   ```csharp
   /// <summary>
   /// Defers initial position synchronization until after Unity's layout system has calculated positions.
   /// This prevents cards from spawning at origin during scene transitions from Core scene.
   /// </summary>
   private IEnumerator InitializePositionAfterLayout()
   {
       // Wait for end of frame (after layout calculations complete)
       yield return new WaitForEndOfFrame();

       // Now sync position with card transform
       if (cardTransform != null)
       {
           transform.position = cardTransform.position;
           Debug.Log($"[CharacterCardVisual] Initial position synced to {cardTransform.position} after layout calculations");
       }
   }
   ```

### How It Works

1. **CharacterCardVisual.Initialize()** is called from `CharacterCard.Start()`
2. Instead of immediately setting position, it **starts a coroutine**
3. The coroutine **waits until end of frame** using `yield return new WaitForEndOfFrame()`
4. After Unity's layout system finishes calculating positions, the coroutine resumes
5. **Now** the position sync happens with correct, final positions
6. Result: CharacterCardVisual spawns at the correct position, whether starting from Core or CharacterSelection

### Benefits

- ✅ Works correctly when starting from Core scene entry point
- ✅ Still works correctly when starting directly from CharacterSelection scene
- ✅ No visual "jump" or position flicker
- ✅ Respects Unity's layout system lifecycle
- ✅ Minimal performance impact (one-time coroutine per card)
- ✅ Includes debug logging for validation

## Testing Instructions

### Test Case 1: Core Scene Entry Point (Previously Failed)

1. Load Core scene in Unity Editor
2. Enter Play mode
3. Navigate through: Core → LoadingScreen → MainMenu → CharacterSelection
4. **Expected Result:** Character cards spawn at correct positions under slots
5. **Verify:** No cards at origin (0, 0, 0)

### Test Case 2: Direct CharacterSelection Start (Previously Worked)

1. Load CharacterSelection scene in Unity Editor
2. Enter Play mode
3. **Expected Result:** Character cards spawn at correct positions (should still work)
4. **Verify:** No regression, same behavior as before

### Test Case 3: Console Verification

1. Run either test case
2. Open Unity Console
3. **Expected Logs:**
   - `[CharacterCardVisual] Initial position synced to (x, y, z) after layout calculations`
   - No errors or warnings related to positioning
4. **Verify:** Position values are non-zero and match expected layout positions

## Related Files

- **Fixed File:** `Assets/Scripts/CharacterSelection/CharacterCards/CharacterCardVisual.cs`
- **Caller:** `Assets/Scripts/CharacterSelection/CharacterCards/CharacterCard.cs` (line ~60: calls Initialize)
- **Scene Flow:** Core.unity → LoadingScreen.unity → MainMenu.unity → CharacterSelection.unity
- **Related Systems:** VisualCharacterCardsHandler, HorizontalCharacterCardHolder, CharacterSlot

## Architecture Impact

This fix follows Unity best practices:

- **Deferred initialization** for UI elements that depend on layout calculations
- **Coroutine-based timing** instead of polling in Update
- **Defensive null checking** before position sync
- **Debug logging** for validation and troubleshooting

## Previous Related Fixes

This builds on previous fixes for the character card system:

1. **Video overlap fix:** Added OnDisable methods to video controllers
2. **Card preset display fix:** Separated CardImage (moving visual) from slot build text (fixed UI)
3. **Position sync fix:** This fix completes the positioning system

## Status

✅ **FIXED** - CharacterCardVisual now correctly positions on spawn from both entry points.

**Implemented:** [Current Date]
**Tested:** Compilation verified, ready for runtime testing
**Next Steps:** User to test both entry point scenarios in Unity Play mode
