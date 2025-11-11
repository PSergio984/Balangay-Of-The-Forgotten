using UnityEngine;
using System.Collections.Generic;
/// <summary>
///  Area data ScriptableObject containing area name and list of maps
/// </summary>
/// <remarks>
/// <para><strong>Why:</strong> Centralize area data in reusable assets instead of hardcoding values</para>
/// <para><strong>How:</strong> Create asset files in editor, assign to MapSelectManager for configuration</para>
/// </summary>
[CreateAssetMenu(fileName = "New Area Data", menuName = "Map Selection/Area Data")]
public class AreaData : ScriptableObject
{
    /// <summary>
    /// Display name shown to player in UI (e.g., "Forest Region")
    /// </summary>
    public string AreaName;

    /// <summary>
    /// List of maps contained in this area
    /// </summary>
    public List<MapData> Maps = new List<MapData>();
}
