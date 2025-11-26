using UnityEngine;
using System.Collections.Generic;


/*
 * 
 * Purpose: ScriptableObject container for map configuration data
 * 
 * How it works:
 * - Stores all data for a single map (ID, scene, visuals, unlock state)
 * - Created as asset files in the project (right-click → Map Selection/Map Data)
 * - Referenced by MapButton to display and load the correct map
 * - Contains combat-specific data (enemies, background) for dynamic level setup
 * 
 * Integration: Used by map selection system to configure each playable map,
 *              and by MatchSetupSystem to set up combat based on selected level
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
    
    
    [Header("Combat Setup")]
    
    /// <summary>
    /// List of enemies that will spawn when this map is loaded in combat
    /// </summary>
    /// <remarks>
    /// <para><strong>Why:</strong> Each map can have different enemy configurations</para>
    /// <para><strong>How:</strong> Assign EnemyData assets in Inspector for this level's enemies</para>
    /// </remarks>
    [Tooltip("Enemies that will appear in combat for this level")]
    [SerializeField] private List<EnemyData> enemyDatas = new List<EnemyData>();
    
    /// <summary>
    /// Public getter for the enemy data list used by MatchSetupSystem (read-only view)
    /// </summary>
    /// <remarks>
    /// <para><strong>Why:</strong> Returns read-only view to prevent external mutation of internal state</para>
    /// </remarks>
    public IReadOnlyList<EnemyData> EnemyDatas => enemyDatas;
    
    /// <summary>
    /// Background sprite displayed in the combat scene for this map
    /// </summary>
    /// <remarks>
    /// <para><strong>Why:</strong> Each map should have a unique visual environment</para>
    /// <para><strong>How:</strong> Assign a background sprite in Inspector, MatchSetupSystem applies it</para>
    /// </remarks>
    [Tooltip("Background image shown during combat for this level")]
    [SerializeField] private Sprite combatBackgroundSprite;
    
    /// <summary>
    /// Public getter for combat background sprite used by MatchSetupSystem
    /// </summary>
    public Sprite CombatBackgroundSprite => combatBackgroundSprite;
    
    
    /// <summary>
    /// Runtime reference to the UI button representing this map (set by map selection manager)
    /// </summary>
    public GameObject MapButtonObj { get; set; }

    [Header("Music")]
    /// <summary>
    /// Music to play for this map if no enemy-specific music is set.
    /// </summary>
    [Tooltip("Music to play for this map if no enemy-specific music is set.")]
    public AudioSystem.SoundData MapMusic;
}
