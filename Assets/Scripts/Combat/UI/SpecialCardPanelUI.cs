using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// Displays available special cards and allows players to use them in combat.
/// Special cards are collected from mini-boss defeats and provide powerful team-wide buffs.
/// Cards are consumed when used and persist if not used.
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Shows usable special cards and handles activation</para>
/// <para><strong>Usage:</strong> Assign SpecialCardCollectionData and UI references in Inspector</para>
/// </remarks>
public class SpecialCardPanelUI : MonoBehaviour
{
    [Header("Data Reference")]
    [Tooltip("Reference to the SpecialCardCollectionData ScriptableObject")]
    [SerializeField] private SpecialCardCollectionData specialCardCollection;
    
    [Header("UI References")]
    [Tooltip("Container for special card buttons (HorizontalLayoutGroup recommended)")]
    [SerializeField] private RectTransform cardContainer;
    
    [Tooltip("Prefab for special card button")]
    [SerializeField] private GameObject specialCardButtonPrefab;
    
    [Tooltip("Panel header text")]
    [SerializeField] private TextMeshProUGUI headerText;
    
    [Header("Display Settings")]
    [Tooltip("Maximum number of special cards to display")]
    [SerializeField] private int maxDisplaySlots = 3;
    
    [Tooltip("Hide the panel if no special cards are available")]
    [SerializeField] private bool hideWhenEmpty = true;
    
    [Header("Animation Settings")]
    [Tooltip("Duration for card use animation")]
    [SerializeField] private float useAnimationDuration = 0.5f;
    
    // List of spawned card buttons
    private List<SpecialCardButtonUI> cardButtons = new List<SpecialCardButtonUI>();
    
    // Currently being processed
    private bool isProcessing = false;
    
    private void Start()
    {
        InitializeCardSlots();
        RefreshDisplay();
    }
    
    private void OnEnable()
    {
        // Refresh when re-enabled
        RefreshDisplay();
    }
    
    /// <summary>
    /// Creates special card button slots
    /// </summary>
    private void InitializeCardSlots()
    {
        if (cardContainer == null)
        {
            Debug.LogWarning("[SpecialCardPanelUI] Card container not assigned!", this);
            return;
        }
        
        // Clear existing buttons
        foreach (Transform child in cardContainer)
        {
            Destroy(child.gameObject);
        }
        cardButtons.Clear();
        
        // Create button slots
        for (int i = 0; i < maxDisplaySlots; i++)
        {
            GameObject buttonObject;
            
            if (specialCardButtonPrefab != null)
            {
                buttonObject = Instantiate(specialCardButtonPrefab, cardContainer);
            }
            else
            {
                // Create basic button if no prefab
                buttonObject = CreateDefaultCardButton(i);
            }
            
            SpecialCardButtonUI buttonUI = buttonObject.GetComponent<SpecialCardButtonUI>();
            if (buttonUI == null)
            {
                buttonUI = buttonObject.AddComponent<SpecialCardButtonUI>();
            }
            
            // Subscribe to button click
            int slotIndex = i;
            buttonUI.OnCardClicked += () => OnSpecialCardClicked(slotIndex);
            
            cardButtons.Add(buttonUI);
            buttonObject.SetActive(false); // Hidden until card is available
        }
    }
    
    /// <summary>
    /// Creates a default card button with basic UI elements
    /// </summary>
    private GameObject CreateDefaultCardButton(int index)
    {
        GameObject buttonObject = new GameObject($"SpecialCardSlot_{index}");
        buttonObject.transform.SetParent(cardContainer);
        
        // Add Image for card background
        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.2f, 0.6f, 1f, 1f); // Blue tint
        
        // Add Button
        Button button = buttonObject.AddComponent<Button>();
        
        // Set size
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(80, 100);
        
        // Add icon child
        GameObject iconChild = new GameObject("Icon");
        iconChild.transform.SetParent(buttonObject.transform);
        Image iconImage = iconChild.AddComponent<Image>();
        RectTransform iconRect = iconChild.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.1f, 0.3f);
        iconRect.anchorMax = new Vector2(0.9f, 0.9f);
        iconRect.offsetMin = Vector2.zero;
        iconRect.offsetMax = Vector2.zero;
        
        return buttonObject;
    }
    
    /// <summary>
    /// Refreshes the display to show current available special cards
    /// </summary>
    public void RefreshDisplay()
    {
        if (specialCardCollection == null)
        {
            Debug.LogWarning("[SpecialCardPanelUI] SpecialCardCollectionData not assigned!", this);
            if (hideWhenEmpty && cardContainer != null)
            {
                cardContainer.gameObject.SetActive(false);
            }
            return;
        }
        
        // Load collection data
        specialCardCollection.Load();
        
        IReadOnlyList<SpecialCardData> cards = specialCardCollection.GetAvailableCards();
        int cardCount = cards.Count;
        
        // Update header if assigned
        if (headerText != null)
        {
            headerText.text = cardCount > 0 ? $"Special Cards ({cardCount})" : "Special Cards";
        }
        
        // Hide container if empty and hideWhenEmpty is true
        if (hideWhenEmpty)
        {
            gameObject.SetActive(cardCount > 0);
        }
        
        // Update each slot
        for (int i = 0; i < cardButtons.Count; i++)
        {
            if (i < cardCount && i < cards.Count)
            {
                // Show card
                SpecialCardData card = cards[i];
                cardButtons[i].SetupCard(card);
                cardButtons[i].gameObject.SetActive(true);
            }
            else
            {
                // Hide empty slot
                cardButtons[i].gameObject.SetActive(false);
            }
        }
        
        Debug.Log($"[SpecialCardPanelUI] Displaying {cardCount} available special cards.");
    }
    
    /// <summary>
    /// Called when a special card button is clicked
    /// </summary>
    private void OnSpecialCardClicked(int slotIndex)
    {
        if (isProcessing)
        {
            Debug.LogWarning("[SpecialCardPanelUI] Already processing a card!");
            return;
        }
        
        if (specialCardCollection == null)
        {
            return;
        }
        
        IReadOnlyList<SpecialCardData> cards = specialCardCollection.GetAvailableCards();
        if (slotIndex < 0 || slotIndex >= cards.Count)
        {
            Debug.LogWarning($"[SpecialCardPanelUI] Invalid slot index: {slotIndex}");
            return;
        }
        
        SpecialCardData card = cards[slotIndex];
        UseSpecialCard(card, slotIndex);
    }
    
    /// <summary>
    /// Uses a special card, applying its effect to the team
    /// </summary>
    private void UseSpecialCard(SpecialCardData card, int slotIndex)
    {
        if (card == null)
        {
            return;
        }
        
        isProcessing = true;
        
        Debug.Log($"[SpecialCardPanelUI] Using special card: {card.CardName}");
        
        // Play use animation
        if (slotIndex < cardButtons.Count)
        {
            SpecialCardButtonUI button = cardButtons[slotIndex];
            button.PlayUseAnimation(useAnimationDuration, () =>
            {
                // Apply the card effect
                ApplyCardEffect(card);
                
                // Remove from collection (consume on use)
                specialCardCollection.UseSpecialCard(card);
                
                // Refresh display
                RefreshDisplay();
                
                isProcessing = false;
            });
        }
        else
        {
            // No animation, just apply
            ApplyCardEffect(card);
            specialCardCollection.UseSpecialCard(card);
            RefreshDisplay();
            isProcessing = false;
        }
    }
    
    /// <summary>
    /// Applies the special card's effect to all player combatants
    /// </summary>
    private void ApplyCardEffect(SpecialCardData card)
    {
        if (card == null)
        {
            return;
        }
        
        // Get all player combatants
        List<CombatantView> playerCombatants = GetPlayerCombatants();
        
        if (playerCombatants.Count == 0)
        {
            Debug.LogWarning("[SpecialCardPanelUI] No player combatants found to apply effect!");
            return;
        }
        
        switch (card.EffectType)
        {
            case SpecialCardData.SpecialCardEffectType.DamageUp:
                ApplyDamageUpEffect(playerCombatants, card);
                break;
                
            case SpecialCardData.SpecialCardEffectType.DefenseUpTwoTargets:
                ApplyDefenseUpEffect(playerCombatants, card);
                break;
                
            case SpecialCardData.SpecialCardEffectType.NoCooldown:
                ApplyNoCooldownEffect(playerCombatants, card);
                break;
                
            default:
                Debug.LogWarning($"[SpecialCardPanelUI] Unknown effect type: {card.EffectType}");
                break;
        }
        
        Debug.Log($"[SpecialCardPanelUI] Applied {card.EffectType} effect from {card.CardName}");
    }
    
    /// <summary>
    /// Applies damage up effect to all player combatants
    /// </summary>
    private void ApplyDamageUpEffect(List<CombatantView> targets, SpecialCardData card)
    {
        // Create and perform the ApplyDamageUpGA action
        var action = new ApplyDamageUpGA(targets, card.EffectPercentage, card.Duration);
        ActionSystem.Instance.Perform(action);
    }
    
    /// <summary>
    /// Applies defense up effect to selected targets (or first 2 if no selection)
    /// </summary>
    private void ApplyDefenseUpEffect(List<CombatantView> targets, SpecialCardData card)
    {
        // For now, apply to first N targets based on TargetCount
        int targetCount = Mathf.Min(card.TargetCount, targets.Count);
        List<CombatantView> selectedTargets = targets.GetRange(0, targetCount);
        
        // Create and perform the ApplyDefenseUpGA action
        var action = new ApplyDefenseUpGA(selectedTargets, card.EffectPercentage, card.Duration);
        ActionSystem.Instance.Perform(action);
    }
    
    /// <summary>
    /// Applies no cooldown effect to all player combatants
    /// </summary>
    private void ApplyNoCooldownEffect(List<CombatantView> targets, SpecialCardData card)
    {
        // Create and perform the ApplyNoCooldownGA action
        var action = new ApplyNoCooldownGA(targets, card.Duration);
        ActionSystem.Instance.Perform(action);
    }
    
    /// <summary>
    /// Gets all living player combatants currently in combat
    /// </summary>
    private List<CombatantView> GetPlayerCombatants()
    {
        List<CombatantView> players = new List<CombatantView>();
        
        // Get player heroes from HeroSystem (filter out dead heroes)
        if (HeroSystem.Instance != null && HeroSystem.Instance.HeroViews != null)
        {
            players.AddRange(HeroSystem.Instance.HeroViews.Where(h => h != null && !h.IsDead));
        }
        
        return players;
    }
}
