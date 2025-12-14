using UnityEngine;

/* RELIC DATA
 * 
 * Purpose: Defines a single relic (decorative trophy) obtained from defeating main bosses
 * 
 * How it works:
 * - Each relic is a ScriptableObject asset with unique ID, sprite, and metadata
 * - Relics are obtained from the RewardData of the second enemy (main boss)
 * - Relics are purely decorative and do not provide gameplay effects
 * - Relics persist across all maps and display in the combat UI relic panel
 * 
 * Integration: 
 * - Referenced by RewardData.associatedRelic
 * - Collected by RelicCollectionData when main boss is defeated
 * - Displayed by RelicPanelUI in combat scene
 */

/// <summary>
/// ScriptableObject that defines a single decorative relic.
/// Relics are trophies obtained from defeating main bosses.
/// </summary>
[CreateAssetMenu(fileName = "New Relic", menuName = "Relics/Relic Data")]
public class RelicData : ScriptableObject
{
    [Header("Identification")]
    
    /// <summary>
    /// Unique identifier for this relic (e.g., "Relic_Dagat_Boss")
    /// </summary>
    [Tooltip("Unique ID for this relic - must match across saves")]
    [SerializeField] private string relicId;
    public string RelicId => relicId;
    
    /// <summary>
    /// Display name shown in UI (e.g., "Pearl of the Deep")
    /// </summary>
    [Tooltip("Display name shown in the relic panel")]
    [SerializeField] private string relicName;
    public string RelicName => relicName;
    
    [Header("Visuals")]
    
    /// <summary>
    /// The relic's icon sprite displayed in the UI
    /// </summary>
    [Tooltip("Sprite shown in the relic collection panel")]
    [SerializeField] private Sprite relicSprite;
    public Sprite RelicSprite => relicSprite;
    
    [Header("Metadata")]
    
    /// <summary>
    /// Optional lore or flavor text description
    /// </summary>
    [Tooltip("Optional description or lore text")]
    [TextArea(2, 4)]
    [SerializeField] private string description;
    public string Description => description;
    
    /// <summary>
    /// The map ID this relic came from (e.g., "Dagat_ng_Kabisayaan")
    /// </summary>
    [Tooltip("Which map this relic is obtained from")]
    [SerializeField] private string sourceMapId;
    public string SourceMapId => sourceMapId;
    
    /// <summary>
    /// Validates that required fields are set
    /// </summary>
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(relicId))
        {
            Debug.LogWarning($"[RelicData] {name} is missing a relicId!", this);
        }
        if (relicSprite == null)
        {
            Debug.LogWarning($"[RelicData] {name} is missing a relicSprite!", this);
        }
    }
}
