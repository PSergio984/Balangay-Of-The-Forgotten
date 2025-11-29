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
        characterCards = GetComponentsInChildren<CharacterCard>().ToList();

        int characterCardCount = 0;

        foreach (CharacterCard characterCard in characterCards)
        {
            characterCard.PointerEnterEvent.AddListener(CardPointerEnter);
            characterCard.PointerExitEvent.AddListener(CardPointerExit);
            characterCard.BeginDragEvent.AddListener(BeginDrag);
            characterCard.EndDragEvent.AddListener(EndDrag);
            characterCard.name = characterCardCount.ToString();
            characterCardCount++;
        }

        StartCoroutine(Frame());

        IEnumerator Frame()
        {
            yield return new WaitForSecondsRealtime(.1f);
            for (int i = 0; i < characterCards.Count; i++)
            {
                if (characterCards[i].CharacterCardVisual != null)
                    characterCards[i].CharacterCardVisual.UpdateIndex(transform.childCount);
            }
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

        characterCards[index].transform.SetParent(focusedParent);
        characterCards[index].transform.localPosition = characterCards[index].selected ? new Vector3(0, characterCards[index].selectionOffset, 0) : Vector3.zero;
        selectedCard.transform.SetParent(crossedParent);

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
