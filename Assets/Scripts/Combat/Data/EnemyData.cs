using UnityEngine;
using Sirenix.OdinInspector;

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
    public int AttackPower { get; private set; }
    
    [HorizontalGroup("Secondary Stats")]
    [field: SerializeField]
    [field: LabelText("Magic Power (MAG)")]
    [field: Range(1, 500)]
    [field: ValidateInput("@MagicPower > 0", "Magic Power must be greater than 0")]
    public int MagicPower { get; private set; }
    
    [HorizontalGroup("Secondary Stats")]
    [field: SerializeField]
    [field: LabelText("Defense (DEF)")]
    [field: Range(1, 500)]
    [field: ValidateInput("@Defense > 0", "Defense must be greater than 0")]
    public int Defense { get; private set; }
    
    
}
