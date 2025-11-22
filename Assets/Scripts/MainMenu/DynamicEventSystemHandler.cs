using UnityEngine.EventSystems;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem;
using System.Collections;
using AudioSystem;


/* DYNAMIC EVENT SYSTEM HANDLER DOCUMENTATION
 * 
 * Purpose: Manages UI navigation, selection animations, and sound feedback for menu systems
 * 
 * How it works:
 * - Attaches EventTrigger components to selectables for hover/select detection
 * - Scales selected UI elements using DOTween for visual feedback
 * - Tracks last selected element to restore focus on navigation input
 * - Plays audio feedback on selection events
 * 
 * Integration: Base class for menu screens, handles controller + mouse navigation with animations
 */


/// <summary>
/// Handles UI selection events, animations, and audio feedback for menu navigation systems
/// </summary>
/// <remarks>
/// <para><strong>Why:</strong> Provides consistent visual/audio feedback across all menu screens for better UX</para>
/// <para><strong>How:</strong> Listens to EventSystem selection events, scales selected UI, plays sounds, restores focus on input</para>
/// </remarks>
public class DynamicEventSystemHandler : MonoBehaviour
{
    [Header("References")]
    /// <summary>
    /// List of all UI selectables in this menu (buttons, toggles, etc.)
    /// </summary>
    public List<Selectable> Selectables = new List<Selectable>();

    [Header("Controls")]
    /// <summary>
    /// Input action for navigation (D-pad/arrow keys) - used to restore focus
    /// </summary>
    [SerializeField] protected InputActionReference _navigateReference;

    [Header("Animations")]
    /// <summary>
    /// Scale multiplier for selected elements (default: 1.1 = 110% size)
    /// </summary>
    [SerializeField] protected float _selectedAnimationScale = 1.1f;
    
    /// <summary>
    /// Duration in seconds for scale animation (default: 0.25s)
    /// </summary>
    [SerializeField] protected float _scaleDuration = 0.25f;
    
    /// <summary>
    /// Tracks the last selected UI element for focus restoration
    /// </summary>
    [SerializeField] protected Selectable _lastSelected;
    
    [Header("Sounds")]
    /// <summary>
    /// Audio clip to play when a UI element is selected
    /// </summary>
    [SerializeField] protected SoundData _OnSelectSound;

    /// <summary>
    /// Maps each selectable to its original scale for animation reset
    /// </summary>
    protected Dictionary<Selectable, Vector3> _scales = new Dictionary<Selectable, Vector3>();

    /// <summary>
    /// Sound builder for playing selection audio effects
    /// </summary>
    private SoundBuilder soundBuilder;


    /// <summary>
    /// Initializes sound system for audio feedback
    /// </summary>
    public virtual void Awake()
    {
        // Initialize sound builder for audio feedback
        if (SoundManager.Instance != null)
        {
            soundBuilder = SoundManager.Instance.CreateSoundBuilder();
        }
        else
        {
            Debug.LogWarning("[MenuEventSystemHandler] SoundManager.Instance is null. SoundBuilder will not be initialized.");
        }
    }


    /// <summary>
    /// Subscribes to input events and resets all UI elements to original scale
    /// </summary>
    public virtual void OnEnable()
    {
        // Subscribe to navigation input for focus restoration
        if (_navigateReference != null && _navigateReference.action != null)
        {
            _navigateReference.action.performed += OnNavigate;
        }

        // Reset all selectables to original size (fixes scale issues from previous sessions)
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
    /// Waits one frame then selects the first UI element (ensures EventSystem is ready)
    /// </summary>
    protected virtual IEnumerator SelectAfterDelay()
    {
        // Wait one frame to ensure EventSystem is fully initialized
        yield return null;
        
        // Safety checks to prevent NullReferenceExceptions
        if (EventSystem.current == null || Selectables.Count == 0 || Selectables[0] == null)
            yield break;
            
        EventSystem.current.SetSelectedGameObject(Selectables[0].gameObject);
    }


    /// <summary>
    /// Unsubscribes from input events on disable
    /// </summary>
    public virtual void OnDisable()
    {
        if (_navigateReference != null && _navigateReference.action != null)
        {
            _navigateReference.action.performed -= OnNavigate;
        }
    }
    
    /// <summary>
    /// Attaches EventTrigger component with select/deselect/hover listeners to a UI selectable
    /// </summary>
    /// <param name="selectable">The UI element to attach listeners to</param>
    protected virtual void AddSelectionListeners(Selectable selectable)
    {
        // Get or create EventTrigger component
        EventTrigger trigger = selectable.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = selectable.gameObject.AddComponent<EventTrigger>();
        }

        // Add SELECT event (controller/keyboard selection)
        EventTrigger.Entry SelectEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Select
        };
        SelectEntry.callback.AddListener(OnSelect);
        trigger.triggers.Add(SelectEntry);

        // Add DESELECT event (when focus leaves element)
        EventTrigger.Entry DeselectEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Deselect
        };
        DeselectEntry.callback.AddListener(OnDeselect);
        trigger.triggers.Add(DeselectEntry);

        // Add POINTER ENTER event (mouse hover)
        EventTrigger.Entry PointerEnter = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        PointerEnter.callback.AddListener(OnPointerEnter);
        trigger.triggers.Add(PointerEnter);

        // Add POINTER EXIT event (mouse unhover)
        EventTrigger.Entry PointerExit = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        PointerExit.callback.AddListener(OnPointerExit);
    }


    /// <summary>
    /// Called when a UI element is selected - plays sound and scales up the element
    /// </summary>
    /// <param name="eventData">Event data containing the selected GameObject</param>
    public virtual void OnSelect(BaseEventData eventData)
    {
        // Play selection sound effect
        soundBuilder?.Play(_OnSelectSound);

        if (eventData.selectedObject == null)
            return;

        // Cache selectable component reference
        Selectable sel = eventData.selectedObject.GetComponent<Selectable>();
        if (sel == null)
            return;

        // Track for focus restoration
        _lastSelected = sel;

        // Get original scale from cached dictionary (prevents scale compounding)
        Vector3 originalScale;
        if (!_scales.TryGetValue(sel, out originalScale))
            originalScale = Vector3.one;

        // Calculate target scale and animate
        Vector3 newScale = originalScale * _selectedAnimationScale;
        
        // Kill existing tweens to prevent conflicts, then start new scale animation
        DOTween.Kill(eventData.selectedObject.transform);
        eventData.selectedObject.transform.DOScale(newScale, _scaleDuration);
    }


    /// <summary>
    /// Called when a UI element is deselected - scales element back to original size
    /// </summary>
    /// <param name="eventData">Event data containing the deselected GameObject</param>
    public virtual void OnDeselect(BaseEventData eventData)
    {
        if (eventData.selectedObject == null)
            return;

        Selectable sel = eventData.selectedObject.GetComponent<Selectable>();
        if (sel == null)
            return;

        // Retrieve original scale from dictionary
        Vector3 targetScale;
        if (!_scales.TryGetValue(sel, out targetScale))
            targetScale = Vector3.one;

        // Kill existing tweens and animate back to original scale
        DOTween.Kill(eventData.selectedObject.transform);
        eventData.selectedObject.transform.DOScale(targetScale, _scaleDuration);
    }


    /// <summary>
    /// Called when mouse enters a UI element - sets EventSystem selection for hybrid input support
    /// </summary>
    /// <param name="eventData">Pointer event data containing the hovered GameObject</param>
    public virtual void OnPointerEnter(BaseEventData eventData)
    {
        PointerEventData pointerEventData = eventData as PointerEventData;
        if (pointerEventData != null)
        {
            // Try to find Selectable component in parent first, then children
            Selectable sel = pointerEventData.pointerEnter.GetComponentInParent<Selectable>();
            if (sel == null)
            {
                sel = pointerEventData.pointerEnter.GetComponentInChildren<Selectable>();
            }
            
            // Set EventSystem selection to enable hybrid mouse+controller navigation
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
    /// Called when mouse exits a UI element - clears selected object reference
    /// </summary>
    /// <param name="eventData">Pointer event data</param>
    public virtual void OnPointerExit(BaseEventData eventData)
    {
        PointerEventData pointerEventData = eventData as PointerEventData;
        if (pointerEventData != null)
        {
            // Clear selection reference on mouse exit
            pointerEventData.selectedObject = null;
        }
    }


    /// <summary>
    /// Called when navigation input is detected - restores focus to last selected element if nothing is selected
    /// </summary>
    /// <param name="context">Input action callback context</param>
    /// <remarks>
    /// <para><strong>Why:</strong> Prevents "lost focus" bug where controller navigation stops working after certain actions</para>
    /// </remarks>
    protected virtual void OnNavigate(InputAction.CallbackContext context)
    {
        // If no UI element is currently selected but we have a last selected reference, restore focus
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == null && _lastSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(_lastSelected.gameObject);
        }
    }


    #region Helper methods
    
    /// <summary>
    /// Adds a selectable to the managed list
    /// </summary>
    /// <param name="selectable">Selectable UI element to add</param>
    public void AddSelectable(Selectable selectable)
    {
        Selectables.Add(selectable);
    }

    /// <summary>
    /// Initializes all selectables - attaches listeners and caches original scales
    /// </summary>
    /// <remarks>
    /// <para><strong>When:</strong> Call after dynamically populating Selectables list (e.g., after spawning buttons)</para>
    /// </remarks>
    public void InitSelectables()
    {
        foreach (var selectable in Selectables)
        {
            AddSelectionListeners(selectable);
            _scales.TryAdd(selectable, selectable.transform.localScale);
        }
    }

    /// <summary>
    /// Selects the first UI element after one frame delay
    /// </summary>
    /// <remarks>
    /// <para><strong>When:</strong> Call when menu becomes active to set initial controller focus</para>
    /// </remarks>
    public void SetFirstSelected()
    {
        StartCoroutine(SelectAfterDelay());
    }
    
    #endregion
}