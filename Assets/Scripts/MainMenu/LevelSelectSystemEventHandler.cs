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
    protected void Awake()
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
    public override void OnSelect(BaseEventData eventData)
    {
        // First do the normal selection stuff (play sound, scale animation)
        base.OnSelect(eventData);

        // Get the image component from the selected button (to change its color)
        _selectableImage = eventData.selectedObject != null ? eventData.selectedObject.GetComponent<Image>() : null;
        
        // Get the MapButton script from the selected button (to read level info)
        _mapButton = eventData.selectedObject != null ? eventData.selectedObject.GetComponent<MapButton>() : null;

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
                if (_initialMoveComplete)
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
    public override void OnDeselect(BaseEventData eventData)
    {
        // First do the normal deselection stuff (scale back to normal)
        base.OnDeselect(eventData);

        // If we have a valid MapButton
        if (_mapButton != null)
        {
            // Clear the level name text (nothing selected now)
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
    }
}