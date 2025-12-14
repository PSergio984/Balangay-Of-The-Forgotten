using UnityEngine;
using TMPro;

/* CHARACTER SLOT
 * 
 * Purpose: Represents a single slot that can hold a character card with fixed UI elements
 * 
 * How it works:
 * - Acts as a container for a character card
 * - Maintains fixed "Current Build" text that doesn't move/rotate with the card
 * - Updates build text when card is assigned/removed or when preset changes
 * - Tracks which card is currently occupying this slot
 * 
 * Integration: Works with HorizontalCharacterCardHolder and CharacterCard drag-and-drop system
 */

/// <summary>
/// Container slot for character cards with fixed build text display
/// </summary>
public class CharacterSlot : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text buildPresetText;
    [Tooltip("The spawn point where the card will be positioned when in this slot")]
    [SerializeField] private Transform cardSpawnPoint;
    
    [Header("Runtime State")]
    [HideInInspector] public CharacterCard AssignedCard;
    
    private void Start()
    {
        // Initialize with empty state
        UpdateBuildText();
    }
    
    /// <summary>
    /// Assigns a card to this slot and updates the build text
    /// </summary>
    public void AssignCard(CharacterCard card)
    {
        if (card == null)
        {
            Debug.LogWarning("[CharacterSlot] Attempted to assign null card.");
            return;
        }
        
        // Update assignment
        AssignedCard = card;
        
        // Subscribe to preset selection changes
        if (card.SelectEvent != null)
        {
            // Remove old listener if any
            card.SelectEvent.RemoveListener(OnCardPresetChanged);
            // Add new listener
            card.SelectEvent.AddListener(OnCardPresetChanged);
        }
        
        // Update build text to reflect the card's current preset
        UpdateBuildText();
    }
    
    /// <summary>
    /// Removes the card from this slot and clears the build text
    /// </summary>
    public void RemoveCard()
    {
        if (AssignedCard != null)
        {
            // Unsubscribe from events
            if (AssignedCard.SelectEvent != null)
            {
                AssignedCard.SelectEvent.RemoveListener(OnCardPresetChanged);
            }
        }
        
        AssignedCard = null;
        UpdateBuildText();
    }
    
    /// <summary>
    /// Called when the assigned card's preset selection changes
    /// </summary>
    private void OnCardPresetChanged(CharacterCard card, bool selected)
    {
        // Update build text when preset changes
        UpdateBuildText();
    }
    
    /// <summary>
    /// Updates the build text based on the assigned card's current preset
    /// </summary>
    public void UpdateBuildText()
    {
        if (buildPresetText == null)
        {
            return;
        }
        
        // No card assigned
        if (AssignedCard == null || AssignedCard.BoundHeroData == null)
        {
            buildPresetText.text = "None Selected";
            return;
        }
        
        // Card assigned but no preset selected
        if (AssignedCard.SelectedPreset == null)
        {
            buildPresetText.text = "None Selected";
            return;
        }
        
        // Card and preset assigned
        string presetName = AssignedCard.SelectedPreset.PresetName;
        if (string.IsNullOrWhiteSpace(presetName))
        {
            buildPresetText.text = "Unnamed Build";
        }
        else
        {
            buildPresetText.text = $"Current Build:\n{presetName}";
        }
    }
    
    /// <summary>
    /// Gets the spawn point transform for positioning cards
    /// </summary>
    public Transform GetCardSpawnPoint()
    {
        return cardSpawnPoint != null ? cardSpawnPoint : transform;
    }
    
    private void OnDestroy()
    {
        // Clean up event listeners
        if (AssignedCard != null && AssignedCard.SelectEvent != null)
        {
            AssignedCard.SelectEvent.RemoveListener(OnCardPresetChanged);
        }
    }
}
