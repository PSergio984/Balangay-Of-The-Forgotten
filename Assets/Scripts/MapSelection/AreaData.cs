using UnityEngine;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "New Area Data", menuName = "Map Selection/Area Data")]
public class AreaData : ScriptableObject
{
    public string AreaName;
    public List<MapData> Maps = new List<MapData>();
}
