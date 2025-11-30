using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
// Ensure CharacterBuildPreset is visible
using System;

/* PRESET SELECTION UI
 * 
 * Purpose: Overlay UI that appears when player clicks a character card to select a build preset
 * 
 * How it works:
 * - Shows up to 3 build presets for the selected hero
 * - Displays preset sprites (with baked-in stats/name)
 * - Player clicks a preset to apply it to the selected card
 * - Closes after selection or when Back is clicked
 * 
 * Integration: Works with CharacterCard click events and CharacterCardVisual display
 */

/// <summary>
/// UI panel for selecting build presets for a character card
/// </summary>
public class PresetSelectionUI : MonoBehaviour
{
    private const int MAX_PRESETS = 3;
    // Store delegates for safe removal
    private UnityEngine.Events.UnityAction[] presetButtonDelegates = new UnityEngine.Events.UnityAction[MAX_PRESETS];
    [Header("UI References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Image[] presetImages; // 3 preset display images
    [SerializeField] private Button[] presetButtons; // 3 preset selection buttons
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_Text currentBuildText; // Shows "Current: [Build Name]"
    
    [Header("Visual Feedback")]
    [SerializeField] private Color selectedPresetColor = Color.green;
    [SerializeField] private Color unselectedPresetColor = Color.white;
    
    private CharacterCard currentCard;
    private HeroData currentHero;
    
    void Start()
    {
        // Hide panel initially
        if (panelRoot != null)
            panelRoot.SetActive(false);
        
        // Wire up button events
        if (backButton != null)
            backButton.onClick.AddListener(OnBackClicked);
        
        if (presetButtons == null)
        {
            Debug.LogWarning("[PresetSelectionUI] presetButtons array is not assigned!", this);
            return;
        }
        for (int i = 0; i < presetButtons.Length && i < MAX_PRESETS; i++)
        {
            int index = i; // Capture for closure
            if (presetButtons[i] != null)
            {
                presetButtonDelegates[i] = () => OnPresetSelected(index);
                presetButtons[i].onClick.AddListener(presetButtonDelegates[i]);
            }
        }
    }
    
    /// <summary>
    /// Opens the preset selection UI for a specific character card
    /// </summary>
    public void ShowForCard(CharacterCard card)
    {
        if (card == null || card.BoundHeroData == null)
        {
            Debug.LogWarning("[PresetSelectionUI] Cannot show UI for null card or card without hero data");
            return;
        }
        currentCard = card;
        currentHero = card.BoundHeroData;
        // Update UI
        UpdateUI();
        // Show panel
        if (panelRoot != null)
            panelRoot.SetActive(true);
    }
    
    /// <summary>
    /// Updates the UI to show available presets for current hero
    /// </summary>
    private void UpdateUI()
    {
        if (currentHero == null) return;
        // Update title
        if (titleText != null)
        {
            string heroName = string.IsNullOrWhiteSpace(currentHero.HeroName) ? "Unknown Hero" : currentHero.HeroName;
            titleText.text = $"Select Build for: {heroName}";
        }
        // Get available presets
        List<CharacterBuildPreset> presets = currentHero.BuildPresets;
        // Update preset displays (up to 3, and up to the minimum of both arrays)
        int maxSlots = Mathf.Min(
            presetImages != null ? presetImages.Length : 0,
            presetButtons != null ? presetButtons.Length : 0,
            MAX_PRESETS);
        for (int i = 0; i < maxSlots; i++)
        {
            bool hasPreset = (presets != null && i < presets.Count && presets[i] != null);
            // Handle preset image
            if (i < presetImages.Length && presetImages[i] != null)
            {
                if (hasPreset)
                {
                    presetImages[i].sprite = presets[i].PresetSprite;
                    presetImages[i].gameObject.SetActive(true);
                    // Highlight if currently selected
                    bool isSelected = (currentCard.SelectedPreset == presets[i]);
                    presetImages[i].color = isSelected ? selectedPresetColor : unselectedPresetColor;
                }
                else
                {
                    presetImages[i].gameObject.SetActive(false);
                }
            }
            // Handle preset button
            if (i < presetButtons.Length && presetButtons[i] != null)
            {
                if (hasPreset)
                {
                    presetButtons[i].gameObject.SetActive(true);
                }
                else
                {
                    presetButtons[i].gameObject.SetActive(false);
                }
            }
        }
        // Hide any extra images beyond maxSlots
        for (int i = maxSlots; i < (presetImages != null ? presetImages.Length : 0); i++)
        {
            if (presetImages[i] != null)
                presetImages[i].gameObject.SetActive(false);
        }
        // Hide any extra buttons beyond maxSlots
        for (int i = maxSlots; i < (presetButtons != null ? presetButtons.Length : 0); i++)
        {
            if (presetButtons[i] != null)
                presetButtons[i].gameObject.SetActive(false);
        }
        // Update current build text
        UpdateCurrentBuildText();
    }
    
    /// <summary>
    /// Updates the "Current Build" text display
    /// </summary>
    private void UpdateCurrentBuildText()
    {
        if (currentBuildText == null) return;
        if (currentCard != null && currentCard.SelectedPreset != null)
        {
            string presetName = string.IsNullOrWhiteSpace(currentCard.SelectedPreset.PresetName) ? "Unnamed Build" : currentCard.SelectedPreset.PresetName;
            currentBuildText.text = $"Current: {presetName}";
        }
        else
        {
            currentBuildText.text = "Current: None";
        }
    }
    
    /// <summary>
    /// Called when a preset button is clicked
    /// </summary>
    private void OnPresetSelected(int index)
    {
        if (currentHero == null || currentHero.BuildPresets == null) return;
        if (index >= 0 && index < currentHero.BuildPresets.Count)
        {
            CharacterBuildPreset preset = currentHero.BuildPresets[index];
            if (preset != null && currentCard != null)
            {
                // Apply preset to card
                currentCard.SelectedPreset = preset;
                // Update visuals
                UpdateUI();
                UpdateCardVisual();
                Debug.Log($"[PresetSelectionUI] Selected preset '{preset.PresetName}' for card");
            }
        }
    }
    
    /// <summary>
    /// Updates the card's visual to show selected preset
    /// </summary>
    private void UpdateCardVisual()
    {
        if (currentCard == null || currentCard.CharacterCardVisual == null) return;
        
        // Update card to show preset info
        currentCard.CharacterCardVisual.UpdatePresetData(currentCard.SelectedPreset);
        
        // Update the slot's build text (if card is in a slot)
        CharacterSlot parentSlot = currentCard.transform.parent?.GetComponent<CharacterSlot>();
        if (parentSlot != null)
        {
            parentSlot.UpdateBuildText();
        }
    }
    
    /// <summary>
    /// Called when back button is clicked
    /// </summary>
    private void OnBackClicked()
    {
        Hide();
    }
    
    /// <summary>
    /// Hides the preset selection UI
    /// </summary>
    public void Hide()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
        
        currentCard = null;
        currentHero = null;
    }
    
    // No static dictionary needed; use CharacterCard.SelectedPreset instead
    
    void OnDestroy()
    {
        // Clean up button listeners
        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackClicked);
        for (int i = 0; i < presetButtons.Length && i < MAX_PRESETS; i++)
        {
            if (presetButtons[i] != null && presetButtonDelegates[i] != null)
                presetButtons[i].onClick.RemoveListener(presetButtonDelegates[i]);
        }
    }
}
