using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages overlay GameObjects for dialogue system.
/// All overlay images should be children of the overlayParent GameObject.
/// </summary>
public class DialogueOverlayManager : MonoBehaviour
{
    [Header("Overlay Container")]
    [Tooltip("Parent GameObject containing all overlay image GameObjects")]
    public Transform overlayParent;
    
    [Header("Excluded GameObjects")]
    [Tooltip("GameObjects to exclude from overlay management (e.g., Glow component)")]
    public List<GameObject> excludedGameObjects = new List<GameObject>();
    
    // Cache of all overlay GameObjects found in the parent
    private List<GameObject> allOverlayGameObjects = new List<GameObject>();
    
    private void Awake()
    {
        // Find all overlay GameObjects in the parent
        RefreshOverlayList();
    }
    
    /// <summary>
    /// Refreshes the list of overlay GameObjects from the parent
    /// </summary>
    public void RefreshOverlayList()
    {
        allOverlayGameObjects.Clear();
        
        if (overlayParent == null)
        {
            Debug.LogWarning("[DialogueOverlayManager] Overlay parent not assigned!");
            return;
        }
        
        // Get all child GameObjects
        for (int i = 0; i < overlayParent.childCount; i++)
        {
            GameObject child = overlayParent.GetChild(i).gameObject;
            
            // Skip excluded GameObjects
            if (excludedGameObjects.Contains(child))
                continue;
            
            allOverlayGameObjects.Add(child);
        }
    }
    
    /// <summary>
    /// Disables all overlay GameObjects except the specified one
    /// </summary>
    /// <param name="exception">The overlay GameObject to keep enabled (null = disable all)</param>
    public void DisableAllOverlaysExcept(GameObject exception)
    {
        foreach (GameObject overlayObj in allOverlayGameObjects)
        {
            if (overlayObj == null)
                continue;
            
            // Skip the exception (the one we want to show)
            if (overlayObj == exception)
                continue;
            
            // Disable the GameObject
            if (overlayObj.activeSelf)
            {
                overlayObj.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Disables all overlay GameObjects
    /// </summary>
    public void DisableAllOverlays()
    {
        DisableAllOverlaysExcept(null);
    }
    
    /// <summary>
    /// Gets all overlay GameObjects
    /// </summary>
    public List<GameObject> GetAllOverlays()
    {
        return new List<GameObject>(allOverlayGameObjects);
    }
}

