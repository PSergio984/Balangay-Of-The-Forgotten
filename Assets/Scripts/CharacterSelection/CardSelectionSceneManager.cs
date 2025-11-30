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
    [SerializeField] private int minRequiredSelections = 1;
    [SerializeField] private int maxAllowedSelections = 4;

    private void OnValidate()
    {
        bool changed = false;
        if (minRequiredSelections < 0)
        {
            minRequiredSelections = 0;
            changed = true;
        }
        if (minRequiredSelections > maxAllowedSelections)
        {
            maxAllowedSelections = minRequiredSelections;
            changed = true;
        }
        if (changed)
        {
            Debug.LogWarning($"[CardSelectionSceneManager] minRequiredSelections and/or maxAllowedSelections were out of bounds and have been corrected. minRequiredSelections={minRequiredSelections}, maxAllowedSelections={maxAllowedSelections}", this);
        }
    }
    
    void Start()
    {
        // Wire up start button
        if (startCombatButton != null)
        {
            startCombatButton.onClick.AddListener(OnStartCombatClicked);
        }
        
        // Initial feedback
        UpdateFeedback("Click cards to select build presets");
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
        
        // Collect selections
        int selectedCount = cardPresetManager.PrepareTransitionData();
        
        // Validate
        if (selectedCount < minRequiredSelections)
        {
            UpdateFeedback($"Please select at least {minRequiredSelections} character(s) with presets", true);
            return;
        }
        
        if (selectedCount > maxAllowedSelections)
        {
            UpdateFeedback($"Too many selections! Maximum is {maxAllowedSelections}", true);
            return;
        }
        
        // Success - transition to combat
        UpdateFeedback($"Starting combat with {selectedCount} characters...");
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
