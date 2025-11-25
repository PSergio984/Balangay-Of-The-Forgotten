using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


/* LEVEL SELECT SYSTEM EVENT HANDLER
 * 
 * Purpose: Makes level selection buttons interactive - shows info and moves player icon
 * 
 * How it works:
 * - When you hover over a level button, it shows the level name
 * - It moves the player icon to the selected button
 * - Changes button color to red when selected, back to normal when not
 * 
 * FOCUS PERSISTENCE SYSTEM:
 * - When clicking non-map UI elements (like Start Test button), the selected map button
 *   ACTIVELY RESTORES its EventSystem focus via a coroutine
 * - This ensures:
 *   1. Visual state (red color, scale) is preserved
 *   2. LevelTransitionData.SelectedMapData remains valid for StartCombat
 *   3. Header text showing the level name stays visible
 * - Only resets when navigating to another map button (normal map-to-map navigation)
 * 
 * Integration: Works with MapSelectManager to create an interactive level select screen
 */


/// <summary>
/// Makes level buttons interactive - displays level info and moves player marker
/// </summary>
/// <remarks>
/// <para><strong>Why:</strong> Makes the level select screen feel alive and shows which level you're picking</para>
/// <para><strong>How:</strong> Listens when you select a button, then shows level name and moves player icon there</para>
/// </remarks>
public class LevelSelectSystemEventHandler : DynamicEventSystemHandler
{
    /// <summary>
    /// The picture/background of the currently selected button
    /// </summary>
    private Image _selectableImage;
    
    /// <summary>
    /// The map button script attached to the selected button
    /// </summary>
    private MapButton _mapButton;

    /// <summary>
    /// Reference to the main map screen manager that controls everything
    /// </summary>
    private MapSelectManager _mapSelectManager;

    /// <summary>
    /// Tracks if we've moved the player icon at least once (prevents first-time glitch)
    /// </summary>
    private bool _initialMoveComplete;


    /// <summary>
    /// Runs when this script first loads - finds the MapSelectManager
    /// </summary>
    public override void Awake()
    {
        // Call the parent class Awake first (sets up sounds and stuff)
        base.Awake();
        
        // Look for the MapSelectManager in parent GameObjects
        _mapSelectManager = GetComponentInParent<MapSelectManager>();
        
        // If we can't find it, show an error and turn off this script
        if (_mapSelectManager == null)
        {
            Debug.LogError("[LevelSelectSystemEventHandler] Could not find MapSelectManager in parent hierarchy. Disabling component.", this);
            enabled = false;
        }
    }


    /// <summary>
    /// Runs when mouse enters a button - disabled here to prevent unwanted behavior
    /// </summary>
    public override void OnPointerEnter(BaseEventData eventData)
    {
        // Empty on purpose - we don't want mouse hover to do anything special
    }


    /// <summary>
    /// Runs when mouse leaves a button - disabled here to prevent unwanted behavior
    /// </summary>
    public override void OnPointerExit(BaseEventData eventData)
    {
        // Empty on purpose - we don't want mouse leaving to do anything special
    }


    /// <summary>
    /// Runs when a level button gets selected (clicked or controller navigation)
    /// </summary>
    /// <param name="eventData">Info about which button was selected</param>
    /// <remarks>
    /// <para><strong>Focus Persistence:</strong> When a MapButton is selected, its MapData is stored
    /// in LevelTransitionData so that StartCombat can use it even if the button loses EventSystem focus.</para>
    /// </remarks>
    public override void OnSelect(BaseEventData eventData)
    {
        // Check if this is a MapButton being selected
        MapButton newMapButton = eventData.selectedObject != null ? eventData.selectedObject.GetComponent<MapButton>() : null;
        
        // If we're selecting a new MapButton (not a non-map UI element)
        if (newMapButton != null)
        {
            // Reset the previous map button's visual state if there was one
            if (_mapButton != null && _mapButton != newMapButton && _selectableImage != null)
            {
                _selectableImage.color = _mapButton.ReturnColor;
            }
            
            // Update references to new map button
            _selectableImage = eventData.selectedObject.GetComponent<Image>();
            _mapButton = newMapButton;
            
            // Store selected map in LevelTransitionData for StartCombat to use
            if (_mapButton.levelTransitionData != null)
            {
                if (_mapButton.MapData != null)
                {
                    _mapButton.levelTransitionData.SelectedMapData = _mapButton.MapData;
                    Debug.Log($"[LevelSelectSystemEventHandler] Set SelectedMapData to: {_mapButton.MapData.MapId}", this);
                }
                else
                {
                    Debug.LogWarning($"[LevelSelectSystemEventHandler] MapButton '{_mapButton.gameObject.name}' has null MapData!", this);
                }
            }
            else
            {
                Debug.LogError($"[LevelSelectSystemEventHandler] MapButton '{_mapButton.gameObject.name}' has no LevelTransitionData assigned! Cannot pass data to combat scene.", this);
            }
        }
        
        // First do the normal selection stuff (play sound, scale animation)
        base.OnSelect(eventData);

        // If we successfully found a MapButton component
        if (_mapButton != null)
        {
            // Check if we have a MapSelectManager and a text field to show level name
            if (_mapSelectManager != null && _mapSelectManager.LevelHeaderText != null)
            {
                // If the button has map data, show the level name
                if (_mapButton.MapData != null)
                {
                    _mapSelectManager.LevelHeaderText.SetText(_mapButton.MapData.MapId);
                }
                else
                {
                    // If no data found, just show "Unknown"
                    _mapSelectManager.LevelHeaderText.SetText("Unknown");
                }
                
                // Get the button's position on screen
                RectTransform rectTrans = eventData.selectedObject.GetComponent<RectTransform>();

                // Move the player icon to this button (but skip the first time to prevent glitches)
                if (_initialMoveComplete && newMapButton != null)
                    _mapSelectManager.MovePlayerToButton(_mapSelectManager.PlayerObj, rectTrans, _mapSelectManager.WorldSpaceCanvasRect);

                // Remember that we've done the first move now
                _initialMoveComplete = true;
            }

            // Change the button's color to red to show it's selected
            if (_selectableImage != null)
            {
                _selectableImage.color = Color.red;
            }
        }
    }


    /// <summary>
    /// Runs when a level button is no longer selected (user moved to another button)
    /// </summary>
    /// <param name="eventData">Info about which button was deselected</param>
    /// <remarks>
    /// <para><strong>Focus Persistence:</strong> Only resets visual state when navigating to another MapButton.
    /// If focus moves to a non-map UI element (like Start Test button), we ACTIVELY RESTORE
    /// focus back to the map button. This ensures the map button stays selected in the EventSystem,
    /// maintains all visual state, and keeps LevelTransitionData valid for StartCombat.</para>
    /// </remarks>
    public override void OnDeselect(BaseEventData eventData)
    {
        // If we have a valid MapButton
        if (_mapButton != null)
        {
            // Check if the new selection is also a MapButton
            // If not, we want to restore focus back to the current map button
            GameObject newSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            MapButton newMapButton = newSelection != null ? newSelection.GetComponent<MapButton>() : null;
            
            // Only reset visual state when navigating from one MapButton to another MapButton
            // This preserves the "selected map" appearance when clicking non-map UI elements
            if (newMapButton != null)
            {
                // Call base to handle scale animation back to normal
                base.OnDeselect(eventData);
                
                // Clear the level name text (a new map button is now selected)
                if (_mapSelectManager != null && _mapSelectManager.LevelHeaderText != null)
                {
                    _mapSelectManager.LevelHeaderText.SetText("");
                }
                
                // Change button color back to what it was before (white or gray)
                if (_selectableImage != null)
                {
                    _selectableImage.color = _mapButton.ReturnColor;
                }
            }
            else
            {
                // FOCUS PERSISTENCE: User clicked a non-map UI element (like Start Test)
                // We need to ACTIVELY RESTORE focus back to the map button
                // This keeps the EventSystem selection on the map button, preserving:
                // 1. Visual state (red color, scale)
                // 2. LevelTransitionData.SelectedMapData validity
                // 3. Header text
                
                // Use a coroutine to restore focus after the current frame
                // This prevents fighting with the EventSystem during the same event
                StartCoroutine(RestoreFocusToMapButton());
            }
        }
        else
        {
            // Not a map button, do normal deselection
            base.OnDeselect(eventData);
        }
    }


    /// <summary>
    /// Restores EventSystem focus back to the currently tracked map button.
    /// Called when focus moves to a non-map UI element to maintain map selection.
    /// </summary>
    /// <remarks>
    /// <para><strong>Why coroutine:</strong> We need to wait one frame to prevent fighting with
    /// the EventSystem during the same event cycle. Setting selection immediately during OnDeselect
    /// can cause race conditions.</para>
    /// <para><strong>What it does:</strong> Waits until end of frame, then sets EventSystem selection
    /// back to the map button's GameObject. This triggers OnSelect again, reinforcing all visual state.</para>
    /// </remarks>
    private System.Collections.IEnumerator RestoreFocusToMapButton()
    {
        // Wait until the end of the current frame to avoid EventSystem conflicts
        yield return new WaitForEndOfFrame();
        
        // Double-check references are still valid (button might have been destroyed)
        if (_mapButton != null && _mapButton.gameObject != null && EventSystem.current != null)
        {
            Debug.Log($"[LevelSelectSystemEventHandler] Restoring focus to MapButton: {_mapButton.gameObject.name}", this);
            // Restore EventSystem selection to the map button
            EventSystem.current.SetSelectedGameObject(_mapButton.gameObject);
        }
        else
        {
            Debug.LogWarning("[LevelSelectSystemEventHandler] Cannot restore focus - MapButton or EventSystem is null", this);
        }
    }

}