
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class CharacterCard : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
    // Stores the selected build preset for this card (used by PresetSelectionUI)
    [HideInInspector]
    public CharacterBuildPreset SelectedPreset;
    private Canvas canvas;
    private Image imageComponent;
    [SerializeField] private bool instantiateVisual = true;
    private VisualCharacterCardsHandler visualHandler;
    private Vector3 offset;

    [Header("Movement")]
    [SerializeField] private float moveSpeedLimit = 50;

    [Header("Selection")]
    public bool selected;
    public float selectionOffset = 50;
    private float pointerDownTime;
    private float pointerUpTime;

    [Header("Visual")]
    [SerializeField] private GameObject CharacterCardVisualPrefab;
    [HideInInspector] public CharacterCardVisual CharacterCardVisual;
    
    [Header("Data Binding")]
    [HideInInspector] public HeroData BoundHeroData;

    [Header("States")]
    public bool isHovering;
    public bool isDragging;
    [HideInInspector] public bool wasDragged;

    [Header("Events")]
    [HideInInspector] public UnityEvent<CharacterCard> PointerEnterEvent;
    [HideInInspector] public UnityEvent<CharacterCard> PointerExitEvent;
    [HideInInspector] public UnityEvent<CharacterCard, bool> PointerUpEvent;
    [HideInInspector] public UnityEvent<CharacterCard> PointerDownEvent;
    [HideInInspector] public UnityEvent<CharacterCard> BeginDragEvent;
    [HideInInspector] public UnityEvent<CharacterCard> EndDragEvent;
    [HideInInspector] public UnityEvent<CharacterCard, bool> SelectEvent;

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        imageComponent = GetComponent<Image>();

        if (!instantiateVisual)
            return;

        visualHandler = FindAnyObjectByType<VisualCharacterCardsHandler>();
        CharacterCardVisual = Instantiate(CharacterCardVisualPrefab, visualHandler ? visualHandler.transform : canvas.transform).GetComponent<CharacterCardVisual>();
        CharacterCardVisual.Initialize(this);

        // If BoundHeroData was set before Start, update the visual
        if (BoundHeroData != null)
        {
            CharacterCardVisual.UpdateCharacterData(BoundHeroData);
        }
    }
    
    /// <summary>
    /// Initializes this card with hero data for display and selection
    /// </summary>
    /// <param name="heroData">The hero data to bind to this card</param>
    /// <remarks>
    /// Called by CharacterSelectionManager when dynamically spawning cards.
    /// Updates the visual representation to show character information.
    /// Sets the card's base image sprite to the hero's sprite for static display.
    /// </remarks>
    public void Initialize(HeroData heroData)
    {
        if (heroData == null)
        {
            Debug.LogError("[CharacterCard] Initialize called with null heroData.");
            return;
        }
        BoundHeroData = heroData;
        
        // Ensure imageComponent is retrieved (in case Initialize is called before Start)
        if (imageComponent == null)
        {
            imageComponent = GetComponent<Image>();
        }
        
        // Update card base image with role card sprite (default card with no stats)
        if (imageComponent != null)
        {
            if (heroData.RoleCard != null)
            {
                Debug.Log($"[CharacterCard] Setting RoleCard sprite for hero '{heroData.HeroName}'.");
                // Display role card sprite (base card without stat values)
                imageComponent.sprite = heroData.RoleCard;
                // No preset selected yet
                SelectedPreset = null;
            }
            else if (heroData.Image != null)
            {
                // Fallback to hero portrait if no role card available
                imageComponent.sprite = heroData.Image;
                Debug.LogWarning($"[CharacterCard] HeroData '{heroData.HeroName}' has no RoleCard sprite! Using hero portrait as fallback.");
                // No preset selected yet (ensure consistent clearing)
                SelectedPreset = null;
            }
            else
            {
                Debug.LogWarning($"[CharacterCard] HeroData '{heroData.HeroName}' has no RoleCard or Image sprite assigned!");
            }
        }
        
        // Update visual representation if already created
        if (CharacterCardVisual != null)
        {
            CharacterCardVisual.UpdateCharacterData(BoundHeroData);
        }
    }

    void Update()
    {
        ClampPosition();

        if (isDragging)
        {
            Vector2 targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition) - offset;
            Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
            Vector2 velocity = direction * Mathf.Min(moveSpeedLimit, Vector2.Distance(transform.position, targetPosition) / Time.deltaTime);
            transform.Translate(velocity * Time.deltaTime);
        }
    }

    void ClampPosition()
    {
        Vector2 screenBounds = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z));
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, -screenBounds.x, screenBounds.x);
        clampedPosition.y = Mathf.Clamp(clampedPosition.y, -screenBounds.y, screenBounds.y);
        transform.position = new Vector3(clampedPosition.x, clampedPosition.y, 0);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        BeginDragEvent.Invoke(this);
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = mousePosition - (Vector2)transform.position;
        isDragging = true;
        canvas.GetComponent<GraphicRaycaster>().enabled = false;
        imageComponent.raycastTarget = false;

        wasDragged = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        EndDragEvent.Invoke(this);
        isDragging = false;
        canvas.GetComponent<GraphicRaycaster>().enabled = true;
        imageComponent.raycastTarget = true;

        StartCoroutine(FrameWait());

        IEnumerator FrameWait()
        {
            yield return new WaitForEndOfFrame();
            wasDragged = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEnterEvent.Invoke(this);
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExitEvent.Invoke(this);
        isHovering = false;
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        PointerDownEvent.Invoke(this);
        pointerDownTime = Time.time;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        pointerUpTime = Time.time;

        PointerUpEvent.Invoke(this, pointerUpTime - pointerDownTime > .2f);

        if (pointerUpTime - pointerDownTime > .2f)
            return;

        if (wasDragged)
            return;

        selected = !selected;
        SelectEvent.Invoke(this, selected);

        if (selected)
            transform.localPosition += (CharacterCardVisual.transform.up * selectionOffset);
        else
            transform.localPosition = Vector3.zero;
    }

    public void Deselect()
    {
        if (selected)
        {
            selected = false;
            if (selected)
                transform.localPosition += (CharacterCardVisual.transform.up * 50);
            else
                transform.localPosition = Vector3.zero;
        }
    }


    public int SiblingAmount()
    {
        return transform.parent.CompareTag("Slot") ? transform.parent.parent.childCount - 1 : 0;
    }

    public int ParentIndex()
    {
        return transform.parent.CompareTag("Slot") ? transform.parent.GetSiblingIndex() : 0;
    }

    public float NormalizedPosition()
    {
        return transform.parent.CompareTag("Slot") ? ExtensionMethods.Remap((float)ParentIndex(), 0, (float)(transform.parent.parent.childCount - 1), 0, 1) : 0;
    }

    private void OnDestroy()
    {
        if(CharacterCardVisual != null)
        {
            Destroy(CharacterCardVisual.gameObject);
        }
    }
}
