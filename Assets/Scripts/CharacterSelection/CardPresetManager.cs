using UnityEngine;
using System.Collections.Generic;

/* CARD PRESET MANAGER
 * 
 * Purpose: Manages the interaction between character cards and preset selection UI
 * 
 * How it works:
 * - Listens for card click events (SelectEvent from CharacterCard)
 * - Opens PresetSelectionUI when a card is clicked
 * - Tracks all card-preset pairs for final scene transition
 * 
 * Integration: Bridges CharacterCard, HorizontalCharacterCardHolder, and PresetSelectionUI
 */

/// <summary>
/// Manager that connects character card clicks to preset selection UI
/// </summary>
public class CardPresetManager : MonoBehaviour
{
    // Event raised whenever a preset selection changes
    public event System.Action PresetSelectionChanged;
    [Header("References")]
    [SerializeField] private HorizontalCharacterCardHolder cardHolder;
    [SerializeField] private PresetSelectionUI presetSelectionUI;
    [SerializeField] private CharacterTransitionData transitionData;
    
    [Header("Settings")]
    [SerializeField] private bool autoWireCards = true;
    [Tooltip("If true, clicking a card opens preset selection. If false, only manual ShowPresetSelection calls work.")]
    [SerializeField] private bool enableCardClickToOpenPresets = true;
    
    // Removed auto-wiring from Start. Now must be called after cards are generated.
    void Start()
    {
        // Intentionally left blank. Call WireUpExistingCards() from CharacterSelectionManager after card generation.
    }
    
    /// <summary>
    /// Automatically wires up all existing character cards to open preset selection on click
    /// </summary>
    public void WireUpExistingCards()
    {
        if (cardHolder == null)
        {
            Debug.LogWarning("[CardPresetManager] No cardHolder assigned. Cannot wire up cards.");
            return;
        }
        
        if (cardHolder.characterCards == null || cardHolder.characterCards.Count == 0)
        {
            Debug.LogWarning("[CardPresetManager] No cards found in cardHolder.");
            return;
        }
        
        foreach (CharacterCard card in cardHolder.characterCards)
        {
            if (card != null && card.SelectEvent != null)
            {
                // Listen for card selection events
                card.SelectEvent.AddListener(OnCardClicked);
            }
        }
        
        Debug.Log($"[CardPresetManager] Wired up {cardHolder.characterCards.Count} cards for preset selection");
    }
    
    /// <summary>
    /// Called when a character card is clicked/selected
    /// </summary>
    private void OnCardClicked(CharacterCard card, bool selected)
    {
        if (!enableCardClickToOpenPresets) return;
        
        if (card == null || !selected) return;
        
        // Only open preset selection if card has hero data
        if (card.BoundHeroData != null)
        {
            ShowPresetSelectionForCard(card);
        }
        else
        {
            Debug.LogWarning("[CardPresetManager] Card clicked but has no BoundHeroData");
        }
    }
    
    /// <summary>
    /// Opens the preset selection UI for a specific card
    /// </summary>
    public void ShowPresetSelectionForCard(CharacterCard card)
    {
        if (presetSelectionUI == null)
        {
            Debug.LogError("[CardPresetManager] PresetSelectionUI not assigned!");
            return;
        }
        
        if (card == null || card.BoundHeroData == null)
        {
            Debug.LogWarning("[CardPresetManager] Cannot show preset selection for invalid card");
            return;
        }
        
        // Check if hero has presets
        if (card.BoundHeroData.BuildPresets == null || card.BoundHeroData.BuildPresets.Count == 0)
        {
            Debug.LogWarning($"[CardPresetManager] Hero '{card.BoundHeroData.HeroName}' has no build presets assigned");
            return;
        }
        
		presetSelectionUI.ShowForCard(card);
        }

    /// <summary>
    /// Call this when a preset is actually selected/confirmed by the user
    /// </summary>
    public void NotifyPresetSelectionChanged()
    {
        int subscriberCount = PresetSelectionChanged?.GetInvocationList().Length ?? 0;
        Debug.Log($"[CardPresetManager] NotifyPresetSelectionChanged called - Subscriber count: {subscriberCount}");
        PresetSelectionChanged?.Invoke();
        Debug.Log($"[CardPresetManager] PresetSelectionChanged event invoked");
    }
    
    /// <summary>
    /// Manually wire up a newly spawned card (call this if cards are spawned after Start)
    /// </summary>
    public void WireUpCard(CharacterCard card)
    {
        if (card != null && card.SelectEvent != null)
        {
            // Prevent duplicate registration by always removing before adding
            card.SelectEvent.RemoveListener(OnCardClicked);
            card.SelectEvent.AddListener(OnCardClicked);
        }
    }
    
    /// <summary>
    /// Checks if all 4 character cards have a preset selected
    /// </summary>
    /// <returns>True if all 4 cards have presets selected</returns>
    public bool AreAllPresetsSelected()
    {
        if (cardHolder == null || cardHolder.characterCards == null)
        {
            return false;
        }

        // Must have exactly 4 cards
        if (cardHolder.characterCards.Count != 4)
        {
            return false;
        }

        // All 4 cards must have a preset selected
        int presetsSelectedCount = 0;
        foreach (CharacterCard card in cardHolder.characterCards)
        {
            if (card != null && card.BoundHeroData != null && card.SelectedPreset != null)
            {
                presetsSelectedCount++;
            }
        }

        return presetsSelectedCount == 4;
    }

    /// <summary>
    /// Collects all selected cards with their presets and prepares transition data
    /// Cards are ordered by their visual position (left to right) based on sibling index
    /// </summary>
    /// <returns>Number of valid card-preset pairs</returns>
    public int PrepareTransitionData()
    {
        if (transitionData == null)
        {
            Debug.LogWarning("[CardPresetManager] TransitionData not assigned. Cannot prepare data.");
            return 0;
        }
        
        if (cardHolder == null || cardHolder.characterCards == null)
        {
            Debug.LogWarning("[CardPresetManager] No cards available to collect data from");
            return 0;
        }
        
        // Sort cards by their visual position (left to right) using sibling index
        // This ensures the order matches what the user sees on screen
        List<CharacterCard> sortedCards = new List<CharacterCard>(cardHolder.characterCards);
        sortedCards.Sort((a, b) =>
        {
            if (a == null || b == null) return 0;
            
            // Get the parent transform's sibling index (represents left-to-right position)
            Transform parentA = a.transform.parent;
            Transform parentB = b.transform.parent;
            
            if (parentA == null || parentB == null) return 0;
            
            // If parents are siblings, compare their sibling indices
            if (parentA.parent == parentB.parent && parentA.parent != null)
            {
                int indexA = parentA.GetSiblingIndex();
                int indexB = parentB.GetSiblingIndex();
                return indexA.CompareTo(indexB);
            }
            
            // Fallback: compare world X position (left to right)
            return a.transform.position.x.CompareTo(b.transform.position.x);
        });
        
        Debug.Log($"[CardPresetManager] Sorted {sortedCards.Count} cards by visual position (left to right)");
        
        List<CharacterSlotData> slots = new List<CharacterSlotData>();
        // Collect selected cards with presets in visual order (left to right)
        foreach (CharacterCard card in sortedCards)
        {
            if (card != null && card.BoundHeroData != null)
            {
                CharacterBuildPreset preset = card.SelectedPreset;
                // Only include cards with presets selected
                if (preset != null)
                {
                    CharacterSlotData slot = new CharacterSlotData(slots.Count)
                    {
                        Hero = card.BoundHeroData,
                        SelectedPreset = preset
                    };
                    slots.Add(slot);
                    Debug.Log($"[CardPresetManager] Added slot {slots.Count}: {card.BoundHeroData.HeroName} with preset {preset.PresetName}");
                }
            }
        }

        int validSelections = slots.Count;

        // Validate we have 1-4 slots
        if (validSelections == 0)
        {
            Debug.LogWarning("[CardPresetManager] No cards with presets selected. Cannot proceed.");
            return 0;
        }

        if (validSelections > 4)
        {
            Debug.LogWarning($"[CardPresetManager] {validSelections} cards selected, but only 4 allowed. Taking first 4.");
            slots = slots.GetRange(0, 4);
            validSelections = 4;
        }

        // Pad to 4 slots if needed (fill remaining with empty slots)
        while (slots.Count < 4)
        {
            slots.Add(new CharacterSlotData(slots.Count));
        }

        // Write to transition data
        transitionData.CharacterSlots = slots.ToArray();

        Debug.Log($"[CardPresetManager] Prepared transition data: {validSelections} selected, {slots.Count} total slots");
        return validSelections;
    }
    
    void OnDestroy()
    {
        // Clean up listeners
        if (cardHolder != null && cardHolder.characterCards != null)
        {
            foreach (CharacterCard card in cardHolder.characterCards)
            {
                if (card != null && card.SelectEvent != null)
                {
                    card.SelectEvent.RemoveListener(OnCardClicked);
                }
            }
        }
    }
}
