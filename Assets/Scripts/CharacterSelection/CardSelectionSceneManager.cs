using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/* CARD SELECTION SCENE MANAGER (EXAMPLE)
 * 
 * Purpose: Example script showing how to wire up card-based preset selection for scene transitions
 * 
 * How it works:
 * - Manages the "Start Combat" button
 * - Validates selection and shows feedback
 * - Prepares transition data and loads combat scene
 * 
 * Integration: Optional helper script - you can implement this logic in your own scene manager
 */

/// <summary>
/// Example manager for card-based character preset selection scene
/// </summary>
/// <remarks>
/// This is an EXAMPLE script showing one way to handle scene transitions.
/// Feel free to adapt this to your existing scene management system.
/// </remarks>
public class CardSelectionSceneManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CardPresetManager cardPresetManager;
    [SerializeField] private Button startCombatButton;
    [SerializeField] private TMP_Text feedbackText;
    
    [Header("Scene Settings")]
    [SerializeField] private string combatSceneName = "Combat";
    // Removed: No longer using min/max selection counts
    // Now requires exactly 4 characters with presets selected
    private const int REQUIRED_CHARACTERS = 4;

    // OnValidate removed - no longer using min/max selection validation
    
    void Start()
    {
        // Wire up start button
        if (startCombatButton != null)
        {
            startCombatButton.onClick.AddListener(OnStartCombatClicked);
        }
        
        // Initial feedback
        UpdateFeedback("Select a build preset for each of the 4 characters");
        
        // Update button state every frame
        UpdateButtonState();
    }
    
    private void Update()
    {
        // Continuously check if all presets are selected to enable/disable button
        UpdateButtonState();
    }
    
    private void UpdateButtonState()
    {
        if (startCombatButton == null || cardPresetManager == null)
            return;
            
        bool allPresetsSelected = cardPresetManager.AreAllPresetsSelected();
        startCombatButton.interactable = allPresetsSelected;
        
        if (allPresetsSelected)
        {
            UpdateFeedback("Ready! All characters have presets selected.");
        }
    }
    
    /// <summary>
    /// Called when Start Combat button is clicked
    /// </summary>
    private void OnStartCombatClicked()
    {
        if (cardPresetManager == null)
        {
            Debug.LogError("[CardSelectionSceneManager] CardPresetManager not assigned!");
            UpdateFeedback("ERROR: Manager not configured", true);
            return;
        }
        
        // Validate all 4 presets are selected
        if (!cardPresetManager.AreAllPresetsSelected())
        {
            UpdateFeedback($"All {REQUIRED_CHARACTERS} characters must have a preset selected!", true);
            return;
        }
        
        // Collect selections and prepare transition data
        int selectedCount = cardPresetManager.PrepareTransitionData();
        
        if (selectedCount != REQUIRED_CHARACTERS)
        {
            UpdateFeedback($"ERROR: Expected {REQUIRED_CHARACTERS} characters, got {selectedCount}", true);
            return;
        }
        
        // Valid selection - proceed to combat
        UpdateFeedback($"Loading combat with {REQUIRED_CHARACTERS} characters...");
        LoadCombatScene();
    }
    
    /// <summary>
    /// Loads the combat scene
    /// </summary>
    private void LoadCombatScene()
    {
        if (string.IsNullOrWhiteSpace(combatSceneName))
        {
            Debug.LogError("[CardSelectionSceneManager] Combat scene name not set!");
            return;
        }
        
        SceneManager.LoadScene(combatSceneName);
    }
    
    /// <summary>
    /// Updates the feedback text display
    /// </summary>
    private void UpdateFeedback(string message, bool isError = false)
    {
        if (feedbackText == null) return;
        
        feedbackText.text = message;
        feedbackText.color = isError ? Color.red : Color.white;
    }
    
    void OnDestroy()
    {
        if (startCombatButton != null)
        {
            startCombatButton.onClick.RemoveListener(OnStartCombatClicked);
        }
    }
}
