using UnityEngine;

[CreateAssetMenu(fileName = "New Map Data", menuName = "Map Selection/Map Data")]
public class MapData : ScriptableObject
{
    [Header("Map Stats")]
    public string MapId;
    [Tooltip("For Starting Levels")]
    public bool IsUnlockedByDefault;
    [SerializeField] private SceneField Scene;
    [SerializeField] private string MapName;
    [SerializeField] private Sprite MapThumbnail;
    public GameObject MapButtonObj { get; set; }

    
}
