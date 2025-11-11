using UnityEngine.EventSystems;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem;
using System.Collections;
using AudioSystem;

public class MenuEventSystemHandler : MonoBehaviour
{
    [Header("References")]
    public List<Selectable> Selectables = new List<Selectable>();
    [SerializeField] protected Selectable _firstSelected;

    [Header("Controls")]
    [SerializeField] protected InputActionReference _navigateReference;

    [Header("Animations")]
    [SerializeField] protected float _selectedAnimationScale = 1.1f;
    [SerializeField] protected float _scaleDuration = 0.25f;
    [SerializeField] protected List<GameObject> _animationExclusions = new List<GameObject>();
    [SerializeField] protected Selectable _lastSelected;
    [Header("Sounds")]
    [SerializeField] protected SoundData _OnSelectSound;



    protected Dictionary<Selectable, Vector3> _scales = new Dictionary<Selectable, Vector3>();

    /// <summary>
    /// Sound builder for playing card-related audio effects
    /// </summary>
    private SoundBuilder soundBuilder;


    public virtual void Awake()
    {
        // Initialize soundBuilder in Awake to ensure it's ready for event listeners
        if (SoundManager.Instance != null)
        {
            soundBuilder = SoundManager.Instance.CreateSoundBuilder();
        }
        else
        {
            Debug.LogWarning("[MenuEventSystemHandler] SoundManager.Instance is null. SoundBuilder will not be initialized.");
        }

        foreach (var selectable in Selectables)
        {
            AddSelectionListeners(selectable);
            _scales.Add(selectable, selectable.transform.localScale);
        }
    }


    public virtual void OnEnable()
    {
        if (_navigateReference != null && _navigateReference.action != null)
        {
            _navigateReference.action.performed += OnNavigate;
        }

        // Ensure all selectables are reset back to original size
        for (int i = 0; i < Selectables.Count; i++)
        {
            if (Selectables[i] == null)
                continue;
            
            if (_scales.ContainsKey(Selectables[i]))
            {
                Selectables[i].transform.localScale = _scales[Selectables[i]];
            }
        }    }
    protected virtual IEnumerator SelectAfterDelay()
    {
        // Wait one frame to ensure EventSystem is ready
        yield return null;
        EventSystem.current.SetSelectedGameObject(_firstSelected.gameObject);
    }

    public virtual void OnDisable()
    {
    _navigateReference.action.performed -= OnNavigate;
    // No need to kill _scaleUpTween/_scaleDownTween; per-object tweens are managed via DOTween.Kill(transform)
    }
    protected virtual void AddSelectionListeners(Selectable selectable)
    {
        // Add listener
        EventTrigger trigger = selectable.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = selectable.gameObject.AddComponent<EventTrigger>();
        }

        // Add SELECT event
        EventTrigger.Entry SelectEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Select
        };
        SelectEntry.callback.AddListener(OnSelect);
        trigger.triggers.Add(SelectEntry);

        // Add DESELECT event
        EventTrigger.Entry DeselectEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.Deselect
        };
        DeselectEntry.callback.AddListener(OnDeselect);
        trigger.triggers.Add(DeselectEntry);

        // Add ONPOINTERENTER event
        EventTrigger.Entry PointerEnter = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        PointerEnter.callback.AddListener(OnPointerEnter);
        trigger.triggers.Add(PointerEnter);

        // Add ONPOINTEREXIT event
        EventTrigger.Entry PointerExit = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        PointerExit.callback.AddListener(OnPointerExit);
        trigger.triggers.Add(PointerExit);

    }

    public void OnSelect(BaseEventData eventData)
    {
        soundBuilder?.Play(_OnSelectSound);

        if (eventData.selectedObject == null)
            return;

        // Only call GetComponent<Selectable>() once
        Selectable sel = eventData.selectedObject.GetComponent<Selectable>();
        if (sel == null)
            return;

        _lastSelected = sel;

        if (_animationExclusions.Contains(eventData.selectedObject))
            return;

        // Use the original scale from _scales, never compound
        Vector3 originalScale;
        if (!_scales.TryGetValue(sel, out originalScale))
            originalScale = Vector3.one;

        Vector3 newScale = originalScale * _selectedAnimationScale;
        // Kill any existing tweens on this transform before starting a new one
        DOTween.Kill(eventData.selectedObject.transform);
        eventData.selectedObject.transform.DOScale(newScale, _scaleDuration);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        // Defensive: null-check eventData.selectedObject
        if (eventData.selectedObject == null)
            return;

        if (_animationExclusions.Contains(eventData.selectedObject))
            return;

        Selectable sel = eventData.selectedObject.GetComponent<Selectable>();
        if (sel == null)
            return;

        // Try to get the original scale, fallback to Vector3.one if not found
        Vector3 targetScale;
        if (!_scales.TryGetValue(sel, out targetScale))
            targetScale = Vector3.one;

        // Kill any existing tweens on this transform before starting a new one
        DOTween.Kill(eventData.selectedObject.transform);
        eventData.selectedObject.transform.DOScale(targetScale, _scaleDuration);
    }

    public void OnPointerEnter(BaseEventData eventData)
    {
        PointerEventData pointerEventData = eventData as PointerEventData;
        if (pointerEventData != null)
        {
            Selectable sel = pointerEventData.pointerEnter.GetComponentInParent<Selectable>();
            if (sel == null)
            {
                sel = pointerEventData.pointerEnter.GetComponentInChildren<Selectable>();
            }
            if (sel != null)
            {
                if (EventSystem.current != null)
                {
                    EventSystem.current.SetSelectedGameObject(sel.gameObject);
                }
            }
            // else: do nothing, don't set selection        
        }
    }
    public void OnPointerExit(BaseEventData eventData)
    {
        PointerEventData pointerEventData = eventData as PointerEventData;
        if (pointerEventData != null)
        {
            pointerEventData.selectedObject = null;
        }
    }

    protected virtual void OnNavigate(InputAction.CallbackContext context)
    {
        if (EventSystem.current.currentSelectedGameObject == null && _lastSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(_lastSelected.gameObject);
        }
    }














}