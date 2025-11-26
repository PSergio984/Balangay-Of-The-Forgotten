using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using AudioSystem;

[CreateAssetMenu(menuName = "Data/Enemy")]
public class EnemyData : ScriptableObject
{
    [Title("Enemy Information", "Basic properties of this enemy", TitleAlignments.Centered)]

    [HorizontalGroup("Basic Info", 0.3f)]
    [field: SerializeField]
    [field: PreviewField(100)]
    [field: LabelText("Enemy Art")]
    [field: Required("Enemy needs artwork!")]
    [field: AssetsOnly]
    public Sprite Image { get; private set; }

    [VerticalGroup("Basic Info/Details")]
    [field: SerializeField]
    [field: LabelText("Enemy Name")]
    [field: Required("Enemy must have a name!")]
    [field: ValidateInput("@!string.IsNullOrWhiteSpace($value)", "Name cannot be empty or whitespace")]
    public string EnemyName { get; private set; }

    [Title("Combat Stats", "Health and damage values", TitleAlignments.Centered)]

    [HorizontalGroup("Primary Stats")]
    [field: SerializeField]
    [field: LabelText("Health Points (HP)")]
    [field: Range(1, 6000)]
    [field: ValidateInput("@Health > 0", "Health must be greater than 0")]
    [field: InfoBox("@\"HP: \" + Health + (Health <= 20 ? \" (Weak)\" : Health >= 100 ? \" (Tank)\" : \" (Normal)\")", InfoMessageType.None)]
    public int Health { get; private set; }

    [HorizontalGroup("Primary Stats")]
    [field: SerializeField]
    [field: LabelText("Attack Power (ATK)")]
    [field: Range(1, 500)]
    [field: ValidateInput("@AttackPower > 0", "Attack Power must be greater than 0")]
    [field: InfoBox("@\"ATK: \" + AttackPower + (AttackPower <= 5 ? \" (Weak)\" : AttackPower >= 20 ? \" (Strong)\" : \" (Normal)\")", InfoMessageType.None)]
    public float AttackPower { get; private set; }

    [HorizontalGroup("Secondary Stats")]
    [field: SerializeField]
    [field: LabelText("Magic Power (MAG)")]
    [field: Range(1, 500)]
    [field: ValidateInput("@MagicPower > 0", "Magic Power must be greater than 0")]
    public float MagicPower { get; private set; }

    [HorizontalGroup("Secondary Stats")]
    [field: SerializeField]
    [field: LabelText("Defense (DEF)")]
    [field: Range(1, 500)]
    [field: ValidateInput("@Defense > 0", "Defense must be greater than 0")]
    public float Defense { get; private set; }

    [field: SerializeField]
    [field: LabelText("Enemy Moveset ")]
    [field: Required("Enemy needs a moveset!")]
    [field: AssetsOnly]
    [field: ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true, ShowPaging = true, NumberOfItemsPerPage = 8)]
    [field: ValidateInput("@ValidateMoveset()", "Moveset has issues that need to be fixed")]
    public List<EnemyMoveData> Moveset { get; private set; }

    [Header("Music")]
    [Tooltip("Music to play when this enemy appears in combat.")]
    public AudioSystem.SoundData CombatMusic;

     private bool ValidateMoveset()
    {
        if (Moveset == null || Moveset.Count == 0) return false;
        
        // Check for null moves
        if (Moveset.Any(move => move == null)) return false;

        // Check for reasonable moveset size
        if (Moveset.Count < 3 || Moveset.Count > 50) return false;
        
        return true;
    }
    
}
