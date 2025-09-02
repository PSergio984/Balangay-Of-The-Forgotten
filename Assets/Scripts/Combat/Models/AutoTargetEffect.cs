using SerializeReferenceEditor;
using UnityEngine;
[System.Serializable]
public class AutoTargetEffect
{
    [field: SerializeReference,SR] public TargetMode targetMode { get; private set; }
    [field: SerializeReference,SR] public Effects effects { get; private set; }

}
