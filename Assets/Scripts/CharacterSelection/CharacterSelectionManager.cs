using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

/* CHARACTER SELECTION MANAGER
 * 
 * Purpose: Manages the character selection flow - card generation, selection logic, and scene transition
 * 
 * How it works:
 * - Dynamically instantiates character cards based on available HeroData
 * - Tracks player selections and enforces selection limits
 * - Writes selected characters to CharacterTransitionData before transitioning to combat
 * 
 * Integration: 
 * - Reads available heroes from Inspector-assigned list or Resources folder
 * - Works with HorizontalCharacterCardHolder for card layout
 * - Writes to CharacterTransitionData for combat scene to read
 */

/// <summary>
/// Manages character selection, card generation, and scene transition to combat
/// </summary>
public class CharacterSelectionManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private List<HeroData> availableHeroes;
    [Tooltip("The ScriptableObject used to pass selected heroes to the combat scene")]
    [SerializeField] private CharacterTransitionData transitionData;
    
    [Header("UI References")]
    [SerializeField] private HorizontalCharacterCardHolder cardHolder;
    [SerializeField] private CardPresetManager cardPresetManager;
    [Tooltip("The prefab containing CharacterCard component - will be spawned for each hero")]
    [SerializeField] private GameObject characterCardPrefab;
    [Tooltip("Parent transform where character card slots will be spawned")]
    [SerializeField] private Transform cardSpawnParent;
    [Tooltip("Slot prefab that contains the character card")]
    [SerializeField] private GameObject slotPrefab;
    
    [Header("Selection Settings")]
    [SerializeField] private int minSelections = 1;
    [SerializeField] private int maxSelections = 3;
    [Tooltip("Name of the combat scene to load after selection")]
    [SerializeField] private string combatSceneName = "Combat";
    
    [Header("UI Feedback")]
    [SerializeField] private Button confirmButton;
    [SerializeField] private TMPro.TMP_Text selectionCountText;
    
    private List<HeroData> selectedHeroes = new List<HeroData>();
    private List<CharacterCard> spawnedCards = new List<CharacterCard>();

    // Reentrancy guard for selection/deselection
    private bool suppressSelectionCallback = false;

    private void Start()
    {
        // Clear previous session data (Editor state pollution prevention)
        #if UNITY_EDITOR
        if (transitionData != null)
        {
            transitionData.Clear();
        }
        #endif
        
        // Validate references
        if (transitionData == null)
        {
            Debug.LogError("[CharacterSelectionManager] CharacterTransitionData reference is missing! Assign it in the Inspector.");
            return;
        }
        
        if (availableHeroes == null || availableHeroes.Count == 0)
        {
            Debug.LogWarning("[CharacterSelectionManager] No heroes available. Attempting to load from Resources/Heroes/");
            LoadHeroesFromResources();
        }
        
        // Initialize UI
        UpdateSelectionUI();
        
        // Subscribe to preset selection changes
        if (cardPresetManager != null)
        {
            Debug.Log("[CharacterSelectionManager] Subscribing to PresetSelectionChanged event");
            cardPresetManager.PresetSelectionChanged += UpdateSelectionUI;
        }
        else
        {
            Debug.LogError("[CharacterSelectionManager] cardPresetManager is NULL! Cannot subscribe to event!");
        }
        
        // Generate character cards
        GenerateCharacterCards();

        // Wire up confirm button event
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(ConfirmSelection);
            Debug.Log("[CharacterSelectionManager] ConfirmSelection listener added to confirmButton");
        }
        else
        {
            Debug.LogError("[CharacterSelectionManager] confirmButton is NULL! Cannot wire up event.");
        }

        // After cards are generated, wire up preset manager (fixes timing issue)
        // (Removed: WireUpExistingCards is now only called after cardHolder.characterCards is populated in RefreshCardHolder)
    }
    
    /// <summary>
    /// Attempts to load heroes from Resources/Heroes/ folder as fallback
    /// </summary>
    private void LoadHeroesFromResources()
    {
        HeroData[] loadedHeroes = Resources.LoadAll<HeroData>("Heroes");
        if (loadedHeroes != null && loadedHeroes.Length > 0)
        {
            availableHeroes = new List<HeroData>(loadedHeroes);
            Debug.Log($"[CharacterSelectionManager] Loaded {availableHeroes.Count} heroes from Resources/Heroes/");
        }
        else
        {
            Debug.LogError("[CharacterSelectionManager] No heroes found! Create HeroData assets and assign them to availableHeroes.");
        }
    }
    
    /// <summary>
    /// Dynamically generates character cards for all available heroes
    /// </summary>
    private void GenerateCharacterCards()
    {
        if (availableHeroes == null || availableHeroes.Count == 0)
        {
            Debug.LogError("[CharacterSelectionManager] Cannot generate cards - no heroes available!");
            return;
        }

        if (cardSpawnParent == null)
        {
            Debug.LogError("[CharacterSelectionManager] cardSpawnParent is null! Assign the parent transform in Inspector.");
            return;
        }

        if (slotPrefab == null)
        {
            Debug.LogError("[CharacterSelectionManager] slotPrefab is null! Assign the slot prefab in the Inspector before generating cards.");
            return;
        }

        // Clear any existing cards
        ClearExistingCards();

        // Spawn a card for each available hero
        foreach (HeroData hero in availableHeroes)
        {
            if (hero == null)
            {
                Debug.LogWarning("[CharacterSelectionManager] Skipping null hero in availableHeroes list");
                continue;
            }

            // Instantiate slot (which contains the card)
            GameObject slotObj = Instantiate(slotPrefab, cardSpawnParent);

            // Get the CharacterCard component from the slot's child
            CharacterCard card = slotObj.GetComponentInChildren<CharacterCard>();

            if (card == null)
            {
                Debug.LogError($"[CharacterSelectionManager] Slot prefab doesn't contain a CharacterCard component! Skipping {hero.HeroName}");
                Destroy(slotObj);
                continue;
            }

            // Bind the hero data to the card
            card.Initialize(hero);

            // Subscribe to selection events
            card.SelectEvent.AddListener(OnCardSelectionChanged);

            // Track the spawned card
            spawnedCards.Add(card);
            
            // Wire up for preset selection if manager is available
            if (cardPresetManager != null)
            {
                cardPresetManager.WireUpCard(card);
            }
        }
        
        Debug.Log($"[CharacterSelectionManager] Generated {spawnedCards.Count} character cards");
        
        // Notify card holder to update visuals
        if (cardHolder != null)
        {
            StartCoroutine(RefreshCardHolder());
        }
    }
    
    /// <summary>
    /// Clears all existing character cards
    /// </summary>
    private void ClearExistingCards()
    {
        // Unsubscribe from events
        foreach (CharacterCard card in spawnedCards)
        {
            if (card != null)
            {
                card.SelectEvent.RemoveListener(OnCardSelectionChanged);
            }
        }
        
        spawnedCards.Clear();
        
        // Destroy existing slot GameObjects safely
        for (int i = cardSpawnParent.childCount - 1; i >= 0; i--)
        {
            Destroy(cardSpawnParent.GetChild(i).gameObject);
        }
    }
    
    /// <summary>
    /// Waits a frame for card holder to refresh after card generation
    /// </summary>
    private IEnumerator RefreshCardHolder()
    {
        yield return new WaitForEndOfFrame();

        // Let the card holder re-initialize its card list and event subscriptions
        if (cardHolder != null)
        {
            cardHolder.RefreshCards();
        }

        // Now that the cardHolder list is up-to-date, wire up preset manager
        if (cardPresetManager != null)
        {
            cardPresetManager.WireUpExistingCards();
        }
    }
    
    /// <summary>
    /// Called when a card's selection state changes
    /// </summary>
    /// <param name="card">The card that was selected/deselected</param>
    /// <param name="isSelected">True if selected, false if deselected</param>
    private void OnCardSelectionChanged(CharacterCard card, bool isSelected)
    {
        Debug.Log($"=== OnCardSelectionChanged: {card?.BoundHeroData?.HeroName ?? "NULL"}, isSelected={isSelected} ===");
        
        if (card.BoundHeroData == null)
        {
            Debug.LogWarning("[CharacterSelectionManager] Card has no bound hero data!");
            return;
        }

        if (suppressSelectionCallback)
        {
            Debug.Log("[CharacterSelectionManager] Suppressing callback (reentrancy guard)");
            return;
        }

        if (isSelected)
        {
            // Check if already selected
            if (selectedHeroes.Contains(card.BoundHeroData))
            {
                Debug.Log($"[CharacterSelectionManager] {card.BoundHeroData.HeroName} already selected, ignoring");
                return;
            }

            // Deterministic: If maxSelections reached, forcibly remove oldest before adding new
            if (selectedHeroes.Count >= maxSelections)
            {
                Debug.LogWarning($"[CharacterSelectionManager] Maximum {maxSelections} characters can be selected! Removing oldest selection.");
                if (selectedHeroes.Count > 0)
                {
                    var oldestHero = selectedHeroes[0];
                    CharacterCard oldestCard = spawnedCards.FirstOrDefault(c => c.BoundHeroData == oldestHero);
                    // Remove oldest hero from selection list first
                    selectedHeroes.RemoveAt(0);
                    Debug.Log($"[CharacterSelectionManager] Removed oldest: {oldestHero.HeroName}");
                    if (oldestCard != null)
                    {
                        // Use reentrancy guard to prevent callback from double-removing
                        suppressSelectionCallback = true;
                        oldestCard.Deselect(); // Only visual deselect; selection list already updated
                        suppressSelectionCallback = false;
                    }
                    else
                    {
                        Debug.LogWarning($"[CharacterSelectionManager] Oldest hero removed from selection list directly (no card found).");
                    }
                }
            }
            // Now add the new selection
            selectedHeroes.Add(card.BoundHeroData);
            Debug.Log($"[CharacterSelectionManager] Selected: {card.BoundHeroData.HeroName} ({selectedHeroes.Count}/{maxSelections})");
        }
        else
        {
            // Remove from selection (if present)
            bool wasRemoved = selectedHeroes.Remove(card.BoundHeroData);
            Debug.Log($"[CharacterSelectionManager] Deselected: {card.BoundHeroData.HeroName} (was in list: {wasRemoved}) ({selectedHeroes.Count}/{maxSelections})");
        }

        // Log current selection state
        Debug.Log($"[CharacterSelectionManager] Current selectedHeroes: {string.Join(", ", selectedHeroes.Select(h => h.HeroName))}");

        UpdateSelectionUI();
    }
    
    /// <summary>
    /// Updates the selection UI (preset count text, confirm button state)
    /// </summary>
    private void UpdateSelectionUI()
    {
        Debug.Log($"[CharacterSelectionManager] UpdateSelectionUI called");
        Debug.Log($"[CharacterSelectionManager] spawnedCards.Count: {spawnedCards.Count}");
        
        // Debug: Log each card's preset status
        for (int i = 0; i < spawnedCards.Count; i++)
        {
            var card = spawnedCards[i];
            if (card != null)
            {
                Debug.Log($"[CharacterSelectionManager] Card {i}: {card.BoundHeroData?.HeroName ?? "NULL"}, Preset: {card.SelectedPreset?.PresetName ?? "NULL"}");
            }
            else
            {
                Debug.Log($"[CharacterSelectionManager] Card {i}: NULL CARD");
            }
        }
        
        // Count how many cards (ALL spawned heroes) have presets selected
        int presetsSelected = spawnedCards.Count(card => card != null && card.SelectedPreset != null);
        int totalHeroes = spawnedCards.Count;
        Debug.Log($"[CharacterSelectionManager] Presets assigned: {presetsSelected}/{totalHeroes}");
        
        // Update preset count text to show ALL cards
        if (selectionCountText != null)
        {
            selectionCountText.text = $"Presets Selected: {presetsSelected}/{totalHeroes}";
            Debug.Log($"[CharacterSelectionManager] Updated UI text: 'Presets Selected: {presetsSelected}/{totalHeroes}'");
        }
        else
        {
            Debug.LogWarning($"[CharacterSelectionManager] selectionCountText is NULL!");
        }
        
        // Enable/disable confirm button based on preset selection
        if (confirmButton != null)
        {
            bool isValid = IsValidSelection();
            confirmButton.interactable = isValid;
            Debug.Log($"[CharacterSelectionManager] Confirm button interactable set to: {isValid}");
        }
    }
    
    /// <summary>
    /// Confirms the selection and transitions to combat scene
    /// Call this from a UI button
    /// </summary>
    public void ConfirmSelection()
    {
        Debug.Log("=== [CharacterSelectionManager] ConfirmSelection() CALLED ===");
        
        if (transitionData == null)
        {
            Debug.LogError("[CharacterSelectionManager] CharacterTransitionData is null! Cannot proceed.");
            return;
        }
        
        // Validate that ALL spawned heroes have presets
        bool isValid = IsValidSelection();
        Debug.Log($"[CharacterSelectionManager] IsValidSelection returned: {isValid}");
        
        if (!isValid)
        {
            Debug.LogWarning("Cannot confirm: Not all heroes have presets assigned!");
            
            // Debug: Show which heroes don't have presets
            foreach (var card in spawnedCards)
            {
                if (card == null || card.BoundHeroData == null) continue;
                
                if (card.SelectedPreset == null)
                {
                    Debug.LogWarning($"  - {card.BoundHeroData.HeroName}: NO PRESET SELECTED");
                }
                else
                {
                    Debug.Log($"  - {card.BoundHeroData.HeroName}: Preset = {card.SelectedPreset.PresetName}");
                }
            }
            return;
        }

        // Use CardPresetManager to prepare transition data (handles all cards with presets)
        if (cardPresetManager != null)
        {
            int validSelections = cardPresetManager.PrepareTransitionData();
            Debug.Log($"[CharacterSelectionManager] CardPresetManager prepared {validSelections} slots");
            
            if (validSelections == 0)
            {
                Debug.LogError("[CharacterSelectionManager] No valid selections prepared!");
                return;
            }
        }
        else
        {
            Debug.LogError("[CharacterSelectionManager] CardPresetManager is null!");
            return;
        }

        Debug.Log($"[CharacterSelectionManager] Confirmed selection of {spawnedCards.Count} heroes");

        // Load map scene
        LoadMapScene();
    }

    /// <summary>
    /// Returns true if the selection is valid: selectedHeroes.Count is within min/max and all selected heroes have a spawned card with a non-null SelectedPreset.
    /// </summary>
    private bool IsValidSelection()
    {
        Debug.Log($"=== [IsValidSelection] START ===");
        
        int totalHeroes = spawnedCards.Count;
        int heroesWithPresets = 0;
        
        Debug.Log($"[IsValidSelection] Checking if ALL {totalHeroes} heroes have presets");
        
        // Check that ALL spawned cards have presets (not just selected ones)
        foreach (var card in spawnedCards)
        {
            if (card == null || card.BoundHeroData == null) continue;
            
            if (card.SelectedPreset != null)
            {
                heroesWithPresets++;
                Debug.Log($"[IsValidSelection] ✓ {card.BoundHeroData.HeroName} has preset '{card.SelectedPreset.PresetName}'");
            }
            else
            {
                Debug.Log($"[IsValidSelection] ✗ {card.BoundHeroData.HeroName} has NO preset");
            }
        }
        
        bool allHavePresets = (heroesWithPresets == totalHeroes) && (totalHeroes > 0);
        Debug.Log($"[IsValidSelection] Result: {(allHavePresets ? "VALID" : "INVALID")} - {heroesWithPresets}/{totalHeroes} heroes have presets");
        
        return allHavePresets;
    }
    
    /// <summary>
    /// Loads the combat scene asynchronously
    /// </summary>
    private void LoadMapScene()
    {
       SceneController.Instance
            .NewTransition()
            .Unload(SceneDatabase.Scenes.CharacterSelection)
            .Load(SceneDatabase.Slots.SessionContent, SceneDatabase.Scenes.Lore, setActive: true)
            .WithLoadingVideo("loading")
            .Perform();
    }
    
    /// <summary>
    /// Cancels selection and returns to previous scene (optional)
    /// </summary>
    public void CancelSelection()
    {
        // Clear selection
        selectedHeroes.Clear();
        
        // Deselect all cards
        foreach (CharacterCard card in spawnedCards)
        {
            if (card != null && card.selected)
            {
                card.Deselect();
            }
        }
        
        UpdateSelectionUI();
        
    }
    
    /// <summary>
    /// [TEST] Skips character selection and transitions directly to the map scene.
    /// Use this for testing scene transitions without selecting characters.
    /// </summary>
    [ContextMenu("Tools/Skip Character Selection For Testing")]
    private void SkipCharacterSelectionForTesting()
    {
        Debug.Log("[CharacterSelectionManager] TEST: Skipping character selection, transitioning to MapSelection...");

        // Null-check for transitionData (same as ConfirmSelection)
        if (transitionData == null)
        {
            Debug.LogError("[CharacterSelectionManager] CharacterTransitionData reference is missing! Assign it in the Inspector.");
            return;
        }

        // Optionally assign default presets to all heroes for testing
        if (cardPresetManager != null)
        {
            foreach (var card in spawnedCards)
            {
                if (card != null && card.BoundHeroData != null && card.SelectedPreset == null)
                {
                    // Try to auto-assign first available preset for testing
                    var presets = card.BoundHeroData.BuildPresets;
                    if (presets != null && presets.Count > 0)
                    {
                        card.SelectedPreset = presets[0];
                        Debug.Log($"[TEST] Auto-assigned preset '{presets[0].PresetName}' to {card.BoundHeroData.HeroName}");
                    }
                }
            }
            // Prepare transition data with auto-assigned presets
            int validSelections = cardPresetManager.PrepareTransitionData();
            Debug.Log($"[TEST] Prepared {validSelections} hero slots for transition");
        }

        // Force transition to map scene
        LoadMapScene();
    }
    
    private void OnDestroy()
    {
        // Clean up event subscriptions
        foreach (CharacterCard card in spawnedCards)
        {
            if (card != null)
            {
                card.SelectEvent.RemoveListener(OnCardSelectionChanged);
            }
        }
        
        // Unsubscribe from preset selection changes
        if (cardPresetManager != null)
        {
            cardPresetManager.PresetSelectionChanged -= UpdateSelectionUI;
        }
        
        // Clean up confirm button listener
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(ConfirmSelection);
        }
    }
}
