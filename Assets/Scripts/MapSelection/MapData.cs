using UnityEngine;


/*
 * 
 * Purpose: ScriptableObject container for map configuration data
 * 
 * How it works:
 * - Stores all data for a single map (ID, scene, visuals, unlock state)
 * - Created as asset files in the project (right-click → Map Selection/Map Data)
 * - Referenced by MapButton to display and load the correct map
 * 
 * Integration: Used by map selection system to configure each playable map
 */

/// <summary>
/// ScriptableObject data container for map configuration and metadata
/// </summary>
/// <remarks>
/// <para><strong>Why:</strong> Centralize map data in reusable assets instead of hardcoding values</para>
/// <para><strong>How:</strong> Create asset files in editor, assign to MapButtons for configuration</para>
/// <para><strong>What:</strong> Like a data card for each map - contains everything needed to display and load it</para>
/// </remarks>
[CreateAssetMenu(fileName = "New Map Data", menuName = "Map Selection/Map Data")]
public class MapData : ScriptableObject
{
    [Header("Map Stats")]
    
    /// <summary>
    /// Unique identifier for this map (e.g., "Forest_01", "Desert_Boss")
    /// </summary>
    public string MapId;
    
    /// <summary>
    /// True if map is unlocked from the start (for tutorial/starting levels)
    /// </summary>
    [Tooltip("For Starting Levels")]
    public bool IsUnlockedByDefault;
    
    /// <summary>
    /// Scene reference containing the combat/gameplay area for this map
    /// </summary>
    [SerializeField] private SceneField Scene;
    
    /// <summary>
    /// Display name shown to player in UI (e.g., "Enchanted Forest")
    /// </summary>
    [SerializeField] private string MapName;
    
    /// <summary>
    /// Visual preview image shown in map selection screen
    /// </summary>
    [SerializeField] private Sprite mapThumbnail;

    /// <summary>
    /// Visual preview image shown in map selection screen (public getter)
    /// </summary>
    public Sprite MapThumbnail => mapThumbnail;
    
    /// <summary>
    /// Runtime reference to the UI button representing this map (set by map selection manager)
    /// </summary>
    public GameObject MapButtonObj { get; set; }
}
