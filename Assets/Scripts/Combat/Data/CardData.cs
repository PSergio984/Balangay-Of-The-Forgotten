using System.Collections.Generic;
using SerializeReferenceEditor;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Card")]
public class CardData : ScriptableObject
{
    [field: SerializeField] public string Description { get; private set; }
    [field: SerializeField] public int Stamina { get; private set; }
    [field: SerializeField] public Sprite Image { get; private set; }

    [field: SerializeReference, SR] public Effects ManualTargetEffect { get; private set; } = null;
    //can have 1 effect, where you pick a target, also can have multiple other effects  where target is selected auto
    [field: SerializeField] public List<AutoTargetEffect> OtherEffects { get; private set; }
}
