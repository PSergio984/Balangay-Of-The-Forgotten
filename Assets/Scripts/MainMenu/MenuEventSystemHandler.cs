using UnityEngine.EventSystems;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem;
using System.Collections;
using AudioSystem;

/* MENU EVENT SYSTEM HANDLER
 * 
 * Purpose: Makes menu buttons feel alive with animations and sounds
 * 
 * How it works:
 * - When you hover over or select a button, it grows bigger and plays a sound
 * - Remembers which button you were on if you lose focus
 * - Works with both controller and mouse
 * 
 * Integration: Used as the base for all menu screens (pause menu, main menu, settings, etc.)
 */


/// <summary>
/// Handles button animations and sounds for menu screens
/// </summary>
/// <remarks>
/// <para><strong>Why:</strong> Makes menus feel responsive and professional (like AAA games)</para>
/// <para><strong>How:</strong> Listens when buttons are selected, then plays sounds and growth animations</para>
/// </remarks>
public class MenuEventSystemHandler : MonoBehaviour
{
    [Header("References")]
    /// <summary>
    /// All the buttons/UI elements in this menu that can be selected
    /// </summary>
    public List<Selectable> Selectables = new List<Selectable>();
    
    /// <summary>
    /// Which button should be selected first when the menu opens
    /// </summary>
    [SerializeField] protected Selectable _firstSelected;

    [Header("Controls")]
    /// <summary>
    /// Controller/keyboard input for navigating the menu (D-pad, arrow keys, joystick)
    /// </summary>
    [SerializeField] protected InputActionReference _navigateReference;

    [Header("Animations")]
    /// <summary>
    /// How much bigger selected buttons become (1.1 = 10% bigger)
    /// </summary>
    [SerializeField] protected float _selectedAnimationScale = 1.1f;
    
    /// <summary>
    /// How fast the button grows/shrinks (in seconds)
    /// </summary>
    [SerializeField] protected float _scaleDuration = 0.25f;
    
    /// <summary>
    /// Buttons that should NOT animate (stay the same size when selected)
    /// </summary>
    [SerializeField] protected List<GameObject> _animationExclusions = new List<GameObject>();
    
    /// <summary>
    /// The last button that was selected (used to restore focus if lost)
    /// </summary>
    [SerializeField] protected Selectable _lastSelected;
    
      
    [Header("Sounds")]
    /// <summary>
    /// Audio clip to play when a UI element is selected
    /// </summary>
    [SerializeField] protected SoundData _OnSelectSound;


    /// <summary>
    /// Remembers the original size of each button (so we can reset them correctly)
    /// </summary>
    protected Dictionary<Selectable, Vector3> _scales = new Dictionary<Selectable, Vector3>();

    /// <summary>
    /// Helper that plays sound effects
    /// </summary>
    private SoundBuilder soundBuilder;


    /// <summary>
    /// Runs when the menu is created - sets up sounds and button listeners
    /// </summary>
    public virtual void Awake()
    {
        // Set up the sound system
        if (SoundManager.Instance != null)
        {
            soundBuilder = SoundManager.Instance.CreateSoundBuilder();
        }
        else
        {
            Debug.LogWarning("[MenuEventSystemHandler] SoundManager.Instance is null. SoundBuilder will not be initialized.");
        }

        // For each button in the menu:
        foreach (var selectable in Selectables)
        {
            // Attach listeners so we know when it's selected/deselected
            AddSelectionListeners(selectable);
            
            // Remember its original size
            _scales.Add(selectable, selectable.transform.localScale);
        }
    }


    /// <summary>
    /// Runs when the menu becomes active - sets up input and resets button sizes
    /// </summary>
    public virtual void OnEnable()
    {
        // Start listening for controller/keyboard navigation input
        if (_navigateReference != null && _navigateReference.action != null)
        {
            _navigateReference.action.performed += OnNavigate;
        }

        // Reset all buttons to their original size (in case they were left scaled from before)
        for (int i = 0; i < Selectables.Count; i++)
        {
            if (Selectables[i] == null)
                continue;
            
            if (_scales.ContainsKey(Selectables[i]))
            {
                Selectables[i].transform.localScale = _scales[Selectables[i]];
            }
        }
    }
    
    /// <summary>
    /// Waits a moment then selects the first button (gives Unity time to set up)
    /// </summary>
    protected virtual IEnumerator SelectAfterDelay()
    {
        // Wait one frame for Unity's EventSystem to fully wake up
        yield return null;
        
        // Safety check - make sure everything exists before selecting
        if (EventSystem.current == null || _firstSelected == null || _firstSelected.gameObject == null)
            yield break;
            
        // Select the first button
        EventSystem.current.SetSelectedGameObject(_firstSelected.gameObject);
    }


    /// <summary>
    /// Runs when the menu is closed - stops listening for input
    /// </summary>
    public virtual void OnDisable()
    {
        // Stop listening for navigation input
        if (_navigateReference != null && _navigateReference.action != null)
        {
            _navigateReference.action.performed -= OnNavigate;
        }
        // Note: Button animations are cleaned up automatically by DOTween
    }
    
    /// <summary>
    /// Attaches event listeners to a button so we know when it's hovered/selected
    /// </summary>
    /// <param name="selectable">The button to add listeners to</param>
    protected virtual void AddSelectionListeners(Selectable selectable)
    {
        // Find or create the EventTrigger component (lets us detect events)
        EventTrigger trigger = selectable.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = selectable.gameObject.AddComponent<EventTrigger>();
        }

        // Listen for SELECTION (controller/keyboard picks this button)
        EventTrigger.Entry SelectEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Select
        };
        SelectEntry.callback.AddListener(OnSelect);
        trigger.triggers.Add(SelectEntry);

        // Listen for DESELECTION (controller/keyboard moves away from this button)
        EventTrigger.Entry DeselectEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Deselect
        };
        DeselectEntry.callback.AddListener(OnDeselect);
        trigger.triggers.Add(DeselectEntry);

        // Listen for MOUSE HOVER (mouse enters this button)
        EventTrigger.Entry PointerEnter = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        PointerEnter.callback.AddListener(OnPointerEnter);
        trigger.triggers.Add(PointerEnter);

        // Listen for MOUSE EXIT (mouse leaves this button)
        EventTrigger.Entry PointerExit = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        PointerExit.callback.AddListener(OnPointerExit);
    }


    /// <summary>
    /// Runs when a button is selected - plays sound and grows the button
    /// </summary>
    /// <param name="eventData">Info about which button was selected</param>
    public void OnSelect(BaseEventData eventData)
    {
        soundBuilder?.WithRandomPitch()?.Play(_OnSelectSound);
        if (eventData.selectedObject == null)
            return;

        // Get the button component
        Selectable sel = eventData.selectedObject.GetComponent<Selectable>();
        if (sel == null)
            return;

        // Remember this button (in case we need to return to it later)
        _lastSelected = sel;

        // If this button is in the "don't animate" list, skip the animation
        if (_animationExclusions.Contains(eventData.selectedObject))
            return;

        // Look up the button's original size
        Vector3 originalScale;
        if (!_scales.TryGetValue(sel, out originalScale))
            originalScale = Vector3.one;

        // Calculate new size (original × 1.1 = 10% bigger)
        Vector3 newScale = originalScale * _selectedAnimationScale;
        
        // Stop any ongoing animations on this button
        DOTween.Kill(eventData.selectedObject.transform);
        
        // Smoothly grow the button to the new size
        eventData.selectedObject.transform.DOScale(newScale, _scaleDuration);
    }


    /// <summary>
    /// Runs when a button is deselected - shrinks the button back to normal
    /// </summary>
    /// <param name="eventData">Info about which button was deselected</param>
    public void OnDeselect(BaseEventData eventData)
    {
        if (eventData.selectedObject == null)
            return;

        // If this button doesn't animate, skip it
        if (_animationExclusions.Contains(eventData.selectedObject))
            return;

        Selectable sel = eventData.selectedObject.GetComponent<Selectable>();
        if (sel == null)
            return;

        // Look up the button's original size
        Vector3 targetScale;
        if (!_scales.TryGetValue(sel, out targetScale))
            targetScale = Vector3.one;

        // Stop any ongoing animations
        DOTween.Kill(eventData.selectedObject.transform);
        
        // Smoothly shrink the button back to original size
        eventData.selectedObject.transform.DOScale(targetScale, _scaleDuration);
    }


    /// <summary>
    /// Runs when mouse hovers over a button - selects that button
    /// </summary>
    /// <param name="eventData">Info about where the mouse is</param>
    /// <remarks>
    /// <para><strong>Why:</strong> Allows mixing mouse and controller - hovering with mouse selects the button</para>
    /// </remarks>
    public void OnPointerEnter(BaseEventData eventData)
    {
        PointerEventData pointerEventData = eventData as PointerEventData;
        if (pointerEventData != null)
        {
            // Try to find a button in the parent objects
            Selectable sel = pointerEventData.pointerEnter.GetComponentInParent<Selectable>();
            if (sel == null)
            {
                // If not in parent, try finding in child objects
                sel = pointerEventData.pointerEnter.GetComponentInChildren<Selectable>();
            }
            
            // If we found a button, select it
            if (sel != null)
            {
                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(sel.gameObject);
                }
            }
        }
    }
    
    /// <summary>
    /// Runs when mouse leaves a button - clears the selection
    /// </summary>
    /// <param name="eventData">Info about where the mouse is</param>
    public void OnPointerExit(BaseEventData eventData)
    {
        PointerEventData pointerEventData = eventData as PointerEventData;
        if (pointerEventData != null)
        {
            // Clear the selected object when mouse leaves
            pointerEventData.selectedObject = null;
        }
    }


    /// <summary>
    /// Runs when player uses controller/keyboard navigation - restores focus if lost
    /// </summary>
    /// <param name="context">Info about the input event</param>
    /// <remarks>
    /// <para><strong>Why:</strong> Fixes the "lost controller focus" bug where navigation stops working</para>
    /// </remarks>
    protected virtual void OnNavigate(InputAction.CallbackContext context)
    {
        // If nothing is selected but we remember a previously selected button, select it again
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null && _lastSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(_lastSelected.gameObject);
        }
    }
}