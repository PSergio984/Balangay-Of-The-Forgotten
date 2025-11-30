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
        
        // Generate character cards
        GenerateCharacterCards();

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

        // Update card holder's internal list
        if (cardHolder != null)
        {
            cardHolder.characterCards = cardSpawnParent.GetComponentsInChildren<CharacterCard>().ToList();

            // Update visual indexes
            for (int i = 0; i < cardHolder.characterCards.Count; i++)
            {
                var card = cardHolder.characterCards[i];
                if (card.CharacterCardVisual != null)
                {
                    card.CharacterCardVisual.UpdateIndex(i);
                }
            }
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
        if (card.BoundHeroData == null)
        {
            Debug.LogWarning("[CharacterSelectionManager] Card has no bound hero data!");
            return;
        }

        if (suppressSelectionCallback)
        {
            // Prevent reentrancy during forced deselection
            return;
        }

        if (isSelected)
        {
            // Check if already selected
            if (selectedHeroes.Contains(card.BoundHeroData))
            {
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
            selectedHeroes.Remove(card.BoundHeroData);
            Debug.Log($"[CharacterSelectionManager] Deselected: {card.BoundHeroData.HeroName} ({selectedHeroes.Count}/{maxSelections})");
        }

        UpdateSelectionUI();
    }
    
    /// <summary>
    /// Updates the selection UI (count text, confirm button state)
    /// </summary>
    private void UpdateSelectionUI()
    {
        // Update selection count text
        if (selectionCountText != null)
        {
            selectionCountText.text = $"Selected: {selectedHeroes.Count}/{maxSelections}";
        }
        
        // Enable/disable confirm button based on selection validity
        if (confirmButton != null)
        {
            bool canConfirm = selectedHeroes.Count >= minSelections && selectedHeroes.Count <= maxSelections;
            confirmButton.interactable = canConfirm;
        }
    }
    
    /// <summary>
    /// Confirms the selection and transitions to combat scene
    /// Call this from a UI button
    /// </summary>
    public void ConfirmSelection()
    {
        // Validate selection
        if (selectedHeroes.Count < minSelections)
        {
            Debug.LogWarning($"[CharacterSelectionManager] Must select at least {minSelections} character(s)!");
            return;
        }
        
        if (selectedHeroes.Count > maxSelections)
        {
            Debug.LogWarning($"[CharacterSelectionManager] Cannot select more than {maxSelections} characters!");
            return;
        }
        
        if (transitionData == null)
        {
            Debug.LogError("[CharacterSelectionManager] CharacterTransitionData is null! Cannot proceed.");
            return;
        }
        
        // Write selected heroes to slot-based CharacterTransitionData
        if (transitionData.CharacterSlots == null || transitionData.CharacterSlots.Length != maxSelections)
            transitionData.CharacterSlots = new CharacterSlotData[maxSelections];
        for (int i = 0; i < maxSelections; i++)
        {
            if (i < selectedHeroes.Count && selectedHeroes[i] != null)
            {
                // Find the card for this hero to get the selected preset
                CharacterCard card = spawnedCards.FirstOrDefault(c => c.BoundHeroData == selectedHeroes[i]);
                CharacterBuildPreset preset = (card != null) ? card.SelectedPreset : null;

                transitionData.CharacterSlots[i] = new CharacterSlotData(i)
                {
                    Hero = selectedHeroes[i],
                    SelectedPreset = preset
                };
            }
            else
            {
                transitionData.CharacterSlots[i] = new CharacterSlotData(i);
            }
        }

        Debug.Log($"[CharacterSelectionManager] Confirmed selection of {selectedHeroes.Count} heroes: {string.Join(", ", selectedHeroes.Select(h => h.HeroName))}");

        // Load combat scene
        StartCoroutine(LoadCombatScene());
    }
    
    /// <summary>
    /// Loads the combat scene asynchronously
    /// </summary>
    private IEnumerator LoadCombatScene()
    {
        // Optional: Show loading screen here
        
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(combatSceneName);
        
        // Wait until the scene is fully loaded
        while (!asyncLoad.isDone)
        {
            // Optional: Update loading progress UI
            // float progress = asyncLoad.progress / 0.9f;
            yield return null;
        }
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
        
        Debug.Log("[CharacterSelectionManager] Selection cancelled");
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
    }
}
