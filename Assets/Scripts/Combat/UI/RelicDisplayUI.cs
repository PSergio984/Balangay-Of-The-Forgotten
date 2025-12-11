using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays collected relics as decorative trophies in the combat scene.
/// Relics are visual representations of defeated main bosses.
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Shows trophy icons for each main boss the player has defeated</para>
/// <para><strong>Usage:</strong> Assign RelicCollectionData and icon slots in Inspector</para>
/// </remarks>
public class RelicDisplayUI : MonoBehaviour
{
    [Header("Data Reference")]
    [Tooltip("Reference to the RelicCollectionData ScriptableObject")]
    [SerializeField] private RelicCollectionData relicCollection;
    
    [Header("Display Settings")]
    [Tooltip("Container for relic icon slots (HorizontalLayoutGroup recommended)")]
    [SerializeField] private RectTransform relicContainer;
    
    [Tooltip("Prefab for individual relic icon slots")]
    [SerializeField] private GameObject relicIconPrefab;
    
    [Tooltip("Maximum number of relics to display")]
    [SerializeField] private int maxDisplaySlots = 4;
    
    [Tooltip("Hide the container if no relics are collected")]
    [SerializeField] private bool hideWhenEmpty = true;
    
    // List of spawned relic icons
    private List<Image> relicIcons = new List<Image>();
    
    private void Start()
    {
        InitializeRelicSlots();
        RefreshDisplay();
    }
    
    private void OnEnable()
    {
        // Refresh when re-enabled (e.g., returning from menu)
        RefreshDisplay();
    }
    
    /// <summary>
    /// Creates empty relic icon slots based on maxDisplaySlots
    /// </summary>
    private void InitializeRelicSlots()
    {
        if (relicContainer == null)
        {
            Debug.LogWarning("[RelicDisplayUI] Relic container not assigned!", this);
            return;
        }
        
        if (relicIconPrefab == null)
        {
            Debug.LogWarning("[RelicDisplayUI] Relic icon prefab not assigned! Using basic Image components.", this);
        }
        
        // Clear existing icons
        foreach (Transform child in relicContainer)
        {
            Destroy(child.gameObject);
        }
        relicIcons.Clear();
        
        // Create icon slots
        for (int i = 0; i < maxDisplaySlots; i++)
        {
            GameObject iconObject;
            
            if (relicIconPrefab != null)
            {
                iconObject = Instantiate(relicIconPrefab, relicContainer);
            }
            else
            {
                // Create basic Image if no prefab
                iconObject = new GameObject($"RelicSlot_{i}");
                iconObject.transform.SetParent(relicContainer);
                Image image = iconObject.AddComponent<Image>();
                
                // Set default size
                RectTransform rect = iconObject.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(64, 64);
            }
            
            Image iconImage = iconObject.GetComponent<Image>();
            if (iconImage != null)
            {
                iconImage.enabled = false; // Hidden until relic is collected
                relicIcons.Add(iconImage);
            }
            
            iconObject.SetActive(true);
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
            if (hideWhenEmpty && relicContainer != null)
            {
                relicContainer.gameObject.SetActive(false);
            }
            return;
        }
        
        // Load collection data
        relicCollection.Load();
        
        IReadOnlyList<RelicData> relics = relicCollection.GetAllRelics();
        int relicCount = relics.Count;
        
        // Hide container if empty and hideWhenEmpty is true
        if (hideWhenEmpty && relicContainer != null)
        {
            relicContainer.gameObject.SetActive(relicCount > 0);
        }
        
        // Update each slot
        for (int i = 0; i < relicIcons.Count; i++)
        {
            if (i < relicCount && i < relics.Count)
            {
                // Show relic
                RelicData relic = relics[i];
                relicIcons[i].sprite = relic.RelicSprite;
                relicIcons[i].enabled = true;
                relicIcons[i].color = Color.white;
            }
            else
            {
                // Hide empty slot
                relicIcons[i].enabled = false;
            }
        }
        
        Debug.Log($"[RelicDisplayUI] Displaying {relicCount} collected relics.");
    }
    
    /// <summary>
    /// Called when a new relic is collected to update display immediately
    /// </summary>
    public void OnRelicCollected()
    {
        RefreshDisplay();
    }
}
