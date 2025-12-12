using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays collected relics as decorative trophies in the combat scene.
/// Relics are visual representations of defeated main bosses.
/// Uses fixed slots assigned in the Inspector for easier positioning and layout control.
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Shows trophy icons for each main boss the player has defeated</para>
/// <para><strong>Usage:</strong> Assign RelicCollectionData and 4 Image slot references in Inspector</para>
/// <para><strong>Pattern:</strong> Similar to HeroBoardView - uses fixed slot references instead of dynamic spawning</para>
/// </remarks>
public class RelicDisplayUI : MonoBehaviour
{
    [Header("Data Reference")]
    [Tooltip("Reference to the RelicCollectionData ScriptableObject")]
    [SerializeField] private RelicCollectionData relicCollection;
    
    [Header("Relic Slots")]
    [Tooltip("Fixed slots for relic icons. Assign 4 Image components in the Inspector.")]
    [SerializeField] private List<Image> relicSlots = new List<Image>();
    
    [Header("Display Settings")]
    [Tooltip("Container GameObject (optional - used for hiding when empty)")]
    [SerializeField] private GameObject container;
    
    [Tooltip("Hide the container if no relics are collected")]
    [SerializeField] private bool hideWhenEmpty = true;
    
    private void Awake()
    {
        ValidateSlots();
    }
    
    private void Start()
    {
        RefreshDisplay();
    }
    
    private void OnEnable()
    {
        // Refresh when re-enabled (e.g., returning from menu)
        RefreshDisplay();
    }
    
    /// <summary>
    /// Validates that slots are properly assigned
    /// </summary>
    private void ValidateSlots()
    {
        if (relicSlots == null || relicSlots.Count == 0)
        {
            Debug.LogWarning("[RelicDisplayUI] No relic slots assigned! Please assign 4 Image components in the Inspector.", this);
            return;
        }
        
        // Validate each slot is assigned
        for (int i = 0; i < relicSlots.Count; i++)
        {
            if (relicSlots[i] == null)
            {
                Debug.LogWarning($"[RelicDisplayUI] Relic slot {i} is not assigned!", this);
            }
        }
    }
    
    /// <summary>
    /// Refreshes the display to show current collected relics
    /// </summary>
    public void RefreshDisplay()
    {
        if (relicCollection == null)
        {
            Debug.LogWarning("[RelicDisplayUI] RelicCollectionData not assigned!", this);
            if (hideWhenEmpty && container != null)
            {
                container.SetActive(false);
            }
            return;
        }
        
        if (relicSlots == null || relicSlots.Count == 0)
        {
            Debug.LogWarning("[RelicDisplayUI] No relic slots assigned!", this);
            return;
        }
        
        // Load collection data
        relicCollection.Load();
        
        IReadOnlyList<RelicData> relics = relicCollection.GetAllRelics();
        int relicCount = relics.Count;
        
        // Hide container if empty and hideWhenEmpty is true
        if (hideWhenEmpty && container != null)
        {
            container.SetActive(relicCount > 0);
        }
        
        // Update each slot
        for (int i = 0; i < relicSlots.Count; i++)
        {
            if (relicSlots[i] == null) continue;
            
            if (i < relicCount && i < relics.Count)
            {
                // Show relic sprite
                RelicData relic = relics[i];
                relicSlots[i].sprite = relic.RelicSprite;
                relicSlots[i].enabled = true;
                relicSlots[i].color = Color.white;
            }
            else
            {
                // Hide empty slot (keep Image component but disable it)
                relicSlots[i].enabled = false;
            }
        }
        
        Debug.Log($"[RelicDisplayUI] Displaying {relicCount} collected relics out of {relicSlots.Count} slots.");
    }
    
    /// <summary>
    /// Called when a new relic is collected to update display immediately
    /// </summary>
    public void OnRelicCollected()
    {
        RefreshDisplay();
    }
}
