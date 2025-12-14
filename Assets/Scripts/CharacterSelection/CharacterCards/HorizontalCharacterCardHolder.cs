using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;
using System.Linq;


/// <summary>
/// BREAKING CHANGE (vNEXT):
/// - HorizontalCharacterCardHolder now supports multi-card spawning (cardsToSpawn, default 7).
/// - Legacy prefabs/scenes that expect a single slot must set cardsToSpawn=1 and autoSpawnOnStart=true to preserve old behavior.
/// - autoSpawnOnStart now defaults to false for safety; set to true only if you want automatic card spawning on Start.
/// - If using CharacterSelectionManager for dynamic card generation, leave autoSpawnOnStart as false.
/// </summary>
public class HorizontalCharacterCardHolder : MonoBehaviour
{
    [SerializeField] private CharacterCard selectedCard;
    [SerializeReference] private CharacterCard hoveredCard;
    [SerializeField] private GameObject slotPrefab;
    private RectTransform rect;
    [Header("Spawn Settings")]
    [Tooltip("Number of cards to spawn if autoSpawnOnStart is true. Default is 7. Set to 1 for legacy single-slot behavior. (See migration note at top of script.)")]
    [SerializeField] private int cardsToSpawn = 7;
    [Tooltip("If true, will spawn cards on Start (legacy: set true for old behavior). If using CharacterSelectionManager for dynamic generation, leave false. (See migration note at top of script.)")]
    [SerializeField] private bool autoSpawnOnStart = false;

    public List<CharacterCard> characterCards;

    bool isCrossing = false;
    [SerializeField] private bool tweenCardReturn = true;
    
    [Header("Visual Handler")]
    [Tooltip("Reference to VisualHandler GameObject - will be hidden when preset overlay is active")]
    [SerializeField] private GameObject visualHandler;

    void Start()
    {

        // Only auto-spawn if enabled (for backward compatibility)
        if (autoSpawnOnStart)
        {
            if (slotPrefab == null)
            {
                Debug.LogWarning($"[HorizontalCharacterCardHolder] slotPrefab is not assigned on GameObject '{gameObject.name}'. Skipping card spawn.", this);
                return;
            }
            for (int i = 0; i < cardsToSpawn; i++)
            {
                Instantiate(slotPrefab, transform);
            }
        }

        rect = GetComponent<RectTransform>();
        InitializeCards();
    }
    
    /// <summary>
    /// Initializes or refreshes the card list and event subscriptions.
    /// Called by Start() and can be called externally after dynamic card generation.
    /// </summary>
    public void RefreshCards()
    {
        InitializeCards();
    }
    
    /// <summary>
    /// Internal method to scan for cards and wire up event subscriptions
    /// </summary>
    private void InitializeCards()
    {
        // Unsubscribe from old cards (if any)
        if (characterCards != null)
        {
            foreach (CharacterCard card in characterCards)
            {
                if (card != null)
                {
                    card.PointerEnterEvent.RemoveListener(CardPointerEnter);
                    card.PointerExitEvent.RemoveListener(CardPointerExit);
                    card.BeginDragEvent.RemoveListener(BeginDrag);
                    card.EndDragEvent.RemoveListener(EndDrag);
                }
            }
        }
        
        // Re-scan for all cards
        characterCards = GetComponentsInChildren<CharacterCard>().ToList();

        int characterCardCount = 0;

        // Subscribe to all card events
        foreach (CharacterCard characterCard in characterCards)
        {
            characterCard.PointerEnterEvent.AddListener(CardPointerEnter);
            characterCard.PointerExitEvent.AddListener(CardPointerExit);
            characterCard.BeginDragEvent.AddListener(BeginDrag);
            characterCard.EndDragEvent.AddListener(EndDrag);
            characterCard.name = characterCardCount.ToString();
            
            // Initialize slot assignment if parent has CharacterSlot component
            CharacterSlot parentSlot = characterCard.transform.parent?.GetComponent<CharacterSlot>();
            if (parentSlot != null)
            {
                parentSlot.AssignCard(characterCard);
            }
            
            characterCardCount++;
        }

        StartCoroutine(UpdateVisualIndexes());
    }
    
    /// <summary>
    /// Coroutine to update visual indexes after a short delay
    /// </summary>
    private IEnumerator UpdateVisualIndexes()
    {
        yield return new WaitForSecondsRealtime(.1f);
        for (int i = 0; i < characterCards.Count; i++)
        {
            if (characterCards[i].CharacterCardVisual != null)
                characterCards[i].CharacterCardVisual.UpdateIndex(transform.childCount);
        }
    }

    private void BeginDrag(CharacterCard characterCard)
    {
        selectedCard = characterCard;
    }


    void EndDrag(CharacterCard characterCard)
    {
        if (selectedCard == null)
            return;

        selectedCard.transform.DOLocalMove(selectedCard.selected ? new Vector3(0,selectedCard.selectionOffset,0) : Vector3.zero, tweenCardReturn ? .15f : 0).SetEase(Ease.OutBack);

        rect.sizeDelta += Vector2.right;
        rect.sizeDelta -= Vector2.right;

        selectedCard = null;

    }

    void CardPointerEnter(CharacterCard characterCard)
    {
        hoveredCard = characterCard;
    }

    void CardPointerExit(CharacterCard characterCard)
    {
        hoveredCard = null;
    }
    
    /// <summary>
    /// Enables or disables visibility for all cards (used when preset overlay opens/closes)
    /// Hides both the card slots AND the visual handler
    /// </summary>
    public void SetCardsInteractable(bool interactable)
    {
        // Hide/show the card slots
        if (characterCards != null)
        {
            for (int i = 0; i < characterCards.Count; i++)
            {
                CharacterCard card = characterCards[i];
                if (card == null) continue;
                
                // Find the slot parent (CharacterCardSlot GameObject)
                Transform slotTransform = card.transform.parent;
                if (slotTransform == null) continue;
                
                // Disable/enable the slot GameObject
                slotTransform.gameObject.SetActive(interactable);
            }
        }
        
        // Also hide/show the VisualHandler (contains CharacterCardVisual clones)
        if (visualHandler != null)
        {
            visualHandler.SetActive(interactable);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (hoveredCard != null)
            {
                Destroy(hoveredCard.transform.parent.gameObject);
                characterCards.Remove(hoveredCard);

            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            foreach (CharacterCard characterCard in characterCards)
            {
                characterCard.Deselect();
            }
        }

        if (selectedCard == null)
            return;

        if (isCrossing)
            return;

        for (int i = 0; i < characterCards.Count; i++)
        {

            if (selectedCard.transform.position.x > characterCards[i].transform.position.x)
            {
                if (selectedCard.ParentIndex() < characterCards[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }

            if (selectedCard.transform.position.x < characterCards[i].transform.position.x)
            {
                if (selectedCard.ParentIndex() > characterCards[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }
        }
    }

    void Swap(int index)
    {
        isCrossing = true;

        Transform focusedParent = selectedCard.transform.parent;
        Transform crossedParent = characterCards[index].transform.parent;

        // Get the slot components (if they exist)
        CharacterSlot focusedSlot = focusedParent.GetComponent<CharacterSlot>();
        CharacterSlot crossedSlot = crossedParent.GetComponent<CharacterSlot>();

        // Update slot assignments before swapping parents
        if (focusedSlot != null)
        {
            focusedSlot.RemoveCard();
        }
        if (crossedSlot != null)
        {
            crossedSlot.RemoveCard();
        }

        characterCards[index].transform.SetParent(focusedParent);
        characterCards[index].transform.localPosition = Vector3.zero;
        selectedCard.transform.SetParent(crossedParent);

        // Update slot assignments after swapping
        if (focusedSlot != null)
        {
            focusedSlot.AssignCard(characterCards[index]);
        }
        if (crossedSlot != null)
        {
            crossedSlot.AssignCard(selectedCard);
        }

        isCrossing = false;

        if (characterCards[index].CharacterCardVisual == null)
            return;

        bool swapIsRight = characterCards[index].ParentIndex() > selectedCard.ParentIndex();
        characterCards[index].CharacterCardVisual.Swap(swapIsRight ? -1 : 1);

        //Updated Visual Indexes
        foreach (CharacterCard characterCard in characterCards)
        {
            characterCard.CharacterCardVisual.UpdateIndex(transform.childCount);
        }
    }

}
