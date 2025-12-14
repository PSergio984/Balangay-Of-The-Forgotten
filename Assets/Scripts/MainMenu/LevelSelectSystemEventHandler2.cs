using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;


/* LEVEL SELECT SYSTEM EVENT HANDLER 2
 * 
 * Purpose: Simplified version of LevelSelectSystemEventHandler - works with pre-set buttons
 * 
 * How it works:
 * - When you select a level button, it shows the level name and boss info
 * - Changes button color to red when selected, back to normal when not
 * - No player marker movement, no line renderers, no dynamic button creation
 * 
 * FOCUS PERSISTENCE SYSTEM:
 * - When clicking non-map UI elements (like Start Test button), the selected map button
 *   PRESERVES its visual state by NOT calling base.OnDeselect()
 * - This ensures:
 *   1. Visual state (red color, scale) is preserved (no scale animation runs)
 *   2. LevelTransitionData.SelectedMapData remains valid for StartCombat (set in OnSelect)
 *   3. Header text showing the level name stays visible
 * - Only resets visual state when navigating to another MapButton (map-to-map navigation)
 * 
 * Integration: Works with MapSelectManager2 to provide a simplified map selection experience
 */


/// <summary>
/// Simplified level selection event handler - works with pre-set buttons, no animations
/// </summary>
/// <remarks>
/// <para><strong>Why:</strong> Simplified UX without player marker movement or line renderers</para>
/// <para><strong>How:</strong> Listens when you select a button, then shows level name and boss info</para>
/// </remarks>
public class LevelSelectSystemEventHandler2 : DynamicEventSystemHandler
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
    /// Reference to the simplified map screen manager
    /// </summary>
    private MapSelectManager2 _mapSelectManager;


    /// <summary>
    /// Runs when this script first loads - finds the MapSelectManager2
    /// </summary>
    public override void Awake()
    {
        // Call the parent class Awake first (sets up sounds and stuff)
        base.Awake();
        
        // Look for the MapSelectManager2 in parent GameObjects
        _mapSelectManager = GetComponentInParent<MapSelectManager2>();
        
        // If we can't find it, show an error and turn off this script
        if (_mapSelectManager == null)
        {
            Debug.LogError("[LevelSelectSystemEventHandler2] Could not find MapSelectManager2 in parent hierarchy. Disabling component.", this);
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
    /// <remarks>
    /// <para><strong>Critical for focus persistence:</strong> The base class OnPointerExit clears
    /// the selectedObject reference, which causes EventSystem to deselect. By overriding and doing
    /// NOTHING, we prevent the selection from being cleared when clicking anywhere.</para>
    /// </remarks>
    public override void OnPointerExit(BaseEventData eventData)
    {
        // DO NOTHING - prevent base class from clearing selectedObject
        // This is critical for maintaining map button selection when clicking non-map UI
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
                    // Only log if the map actually changed (reduce duplicate logs)
                    if (_mapButton.levelTransitionData.SelectedMapData != _mapButton.MapData)
                    {
                        _mapButton.levelTransitionData.SelectedMapData = _mapButton.MapData;
                        Debug.Log($"[LevelSelectSystemEventHandler2] Set SelectedMapData to: {_mapButton.MapData.MapId}", this);
                    }
                }
                else
                {
                    Debug.LogWarning($"[LevelSelectSystemEventHandler2] MapButton '{_mapButton.gameObject.name}' has null MapData!", this);
                }
            }
            else
            {
                Debug.LogError($"[LevelSelectSystemEventHandler2] MapButton '{_mapButton.gameObject.name}' has no LevelTransitionData assigned! Cannot pass data to combat scene.", this);
            }
        }
        
        // First do the normal selection stuff (play sound, scale animation)
        base.OnSelect(eventData);

        // If we successfully found a MapButton component
        if (_mapButton != null)
        {
            // Check if we have a MapSelectManager2 and a text field to show level name
            if (_mapSelectManager != null && _mapSelectManager.LevelHeaderText != null)
            {
                // If the button has map data, show the level name
                if (_mapButton.MapData != null)
                {
                    _mapSelectManager.LevelHeaderText.SetText(_mapButton.MapData.MapId);
                    
                    // Update boss image if available
                    if (_mapSelectManager.CurrentBossImage != null)
                    {
                        if (_mapButton.MapData.BossImage != null)
                        {
                            _mapSelectManager.CurrentBossImage.sprite = _mapButton.MapData.BossImage;
                        }
                        else
                        {
                            _mapSelectManager.CurrentBossImage.sprite = null;
                        }
                    }
                    
                    // Update boss name text if available
                    if (_mapSelectManager.CurrentBossImageText != null)
                    {
                        if (!string.IsNullOrEmpty(_mapButton.MapData.BossName))
                        {
                            _mapSelectManager.CurrentBossImageText.SetText(_mapButton.MapData.BossName);
                        }
                        else
                        {
                            _mapSelectManager.CurrentBossImageText.SetText("Unknown");
                        }
                    }
                }
                else
                {
                    // If no data found, just show "Unknown"
                    _mapSelectManager.LevelHeaderText.SetText("Unknown");
                    if (_mapSelectManager.CurrentBossImageText != null)
                        _mapSelectManager.CurrentBossImageText.SetText("Unknown");
                    if (_mapSelectManager.CurrentBossImage != null)
                        _mapSelectManager.CurrentBossImage.sprite = null;
                }
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
    /// If focus moves to a non-map UI element (like Start Test button), we preserve the visual state
    /// and do NOT call base.OnDeselect to prevent scale animation from running.</para>
    /// </remarks>
    public override void OnDeselect(BaseEventData eventData)
    {
        // If we have a valid MapButton that's being deselected
        if (_mapButton != null)
        {
            // IMPORTANT: Check the NEW selection (where focus is going)
            // currentSelectedGameObject is ALREADY the new selection by the time OnDeselect fires
            GameObject newSelection = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            MapButton newMapButton = newSelection != null ? newSelection.GetComponent<MapButton>() : null;
            
            // Only reset visual state when navigating from one MapButton to another MapButton
            if (newMapButton != null && newMapButton != _mapButton)
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
            // If newMapButton is null OR it's the SAME button being reselected
            // DO NOT reset visual state - preserve everything
            else
            {
                // DO NOT call base.OnDeselect() - prevents scale animation from running
                // The visual state (scale, color) is preserved by not doing anything
                // The LevelTransitionData.SelectedMapData remains valid (set in OnSelect)
            }
        }
        else
        {
            // Not a map button, do normal deselection
            base.OnDeselect(eventData);
        }
    }
}

