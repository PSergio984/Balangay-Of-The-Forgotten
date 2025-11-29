using UnityEngine;
using Sirenix.OdinInspector;

/* CHARACTER BUILD PRESET
 * 
 * Purpose: Defines a specific build/loadout for a hero (Glass Cannon, Berserker, Bruiser, etc.)
 * 
 * How it works:
 * - Each hero has multiple build presets (3 shown in selection UI)
 * - Each preset has its own sprite with baked-in stats/name
 * - Preset applies stat modifiers to base hero stats
 * - Selected preset gets passed to combat along with HeroData
 * 
 * Integration: Referenced by HeroData, selected in PresetSelectionUI, passed via CharacterTransitionData
 */

/// <summary>
/// Defines a character build preset with visual sprite and stat modifiers
/// </summary>
[CreateAssetMenu(fileName = "CharacterBuildPreset", menuName = "Data/Character Build Preset")]
public class CharacterBuildPreset : ScriptableObject
{
    [Title("Preset Visual", "The sprite contains the build name and stats", TitleAlignments.Centered)]
    
    [field: SerializeField]
    [field: PreviewField(200)]
    [field: LabelText("Preset Card Sprite")]
    [field: Required("Preset needs a sprite!")]
    [field: InfoBox("This sprite should contain:\n• Character portrait\n• Build name (e.g., 'Glass Cannon Set')\n• Stat values (HP, ATK, MAG, DEF)\n• Visual theme/icons", InfoMessageType.Info)]
    public Sprite PresetSprite { get; private set; }
    
    [Title("Preset Identification")]
    
    [field: SerializeField]
    [field: LabelText("Preset Name")]
    [field: Required("Must have a name!")]
    [field: InfoBox("Examples: 'Glass Cannon Set', 'Berserker Set', 'Bruiser Set'")]
    public string PresetName { get; private set; }
    
    [Title("Stat Modifiers", "Applied to base hero stats", TitleAlignments.Centered)]
    
    [HorizontalGroup("Health")]
    [field: SerializeField]
    [field: LabelText("Health Modifier")]
    [field: Range(-50, 100)]
    [field: InfoBox("Final HP: Base + HealthModifier (actual value depends on assigned hero)", InfoMessageType.None)]
    public int HealthModifier { get; private set; }
    
    [HorizontalGroup("Attack")]
    [field: SerializeField]
    [field: LabelText("Attack Modifier")]
    [field: Range(-50f, 100f)]
    public float AttackModifier { get; private set; }
    
    [HorizontalGroup("Magic")]
    [field: SerializeField]
    [field: LabelText("Magic Modifier")]
    [field: Range(-50f, 100f)]
    public float MagicModifier { get; private set; }
    
    [HorizontalGroup("Defense")]
    [field: SerializeField]
    [field: LabelText("Defense Modifier")]
    [field: Range(-50f, 100f)]
    public float DefenseModifier { get; private set; }
    
    [Title("Build Description")]
    
    [field: SerializeField]
    [field: LabelText("Description")]
    [field: TextArea(3, 6)]
    [field: InfoBox("Optional tooltip description for this build")]
    public string Description { get; private set; }
    
    /// <summary>
    /// Applies this preset's modifiers to base stats
    /// </summary>
    public (int health, float attack, float magic, float defense) ApplyModifiers(int baseHealth, float baseAttack, float baseMagic, float baseDefense)
    {
        int health = Mathf.Max(0, baseHealth + HealthModifier);
        float attack = Mathf.Max(0.0f, baseAttack + AttackModifier);
        float magic = Mathf.Max(0.0f, baseMagic + MagicModifier);
        float defense = Mathf.Max(0.0f, baseDefense + DefenseModifier);
        return (health, attack, magic, defense);
    }
}
