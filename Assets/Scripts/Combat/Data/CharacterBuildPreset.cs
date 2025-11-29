using UnityEngine;
using Sirenix.OdinInspector;

/* CHARACTER BUILD PRESET
 *
 * Purpose: Defines a specific build/loadout for a hero (Glass Cannon, Berserker, Bruiser, etc.)
 *
 * How it works:
 * - Each hero has multiple build presets (3 shown in selection UI)
 * - Each preset has its own sprite with baked-in stats/name
 * - Preset defines fixed stats (overrides hero base stats)
 * - Selected preset gets passed to combat along with HeroData
 *
 * Integration: Referenced by HeroData, selected in PresetSelectionUI, passed via CharacterTransitionData
 */

/// <summary>
/// Defines a character build preset with visual sprite and fixed stats (overrides hero base stats)
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
    
    [Title("Preset Stats", "These values override the hero's base stats when this preset is selected", TitleAlignments.Centered)]

    [HorizontalGroup("Health")]
    [field: SerializeField]
    [field: LabelText("Health (Override)")]
    [field: Range(50, 2000)]
    [field: InfoBox("Final HP: This value will override the hero's base Health when this preset is selected.", InfoMessageType.None)]
    public int Health { get; private set; }

    [HorizontalGroup("Attack")]
    [field: SerializeField]
    [field: LabelText("Attack Power (Override)")]
    [field: Range(0, 400)]
    public float AttackPower { get; private set; }

    [HorizontalGroup("Magic")]
    [field: SerializeField]
    [field: LabelText("Magic Power (Override)")]
    [field: Range(0, 400)]
    public float MagicPower { get; private set; }

    [HorizontalGroup("Defense")]
    [field: SerializeField]
    [field: LabelText("Defense (Override)")]
    [field: Range(1, 400)]
    public float Defense { get; private set; }
    
    [Title("Build Description")]
    
    [field: SerializeField]
    [field: LabelText("Description")]
    [field: TextArea(3, 6)]
    [field: InfoBox("Optional tooltip description for this build")]
    public string Description { get; private set; }
    
    /// <summary>
    /// Returns the fixed stats for this preset (overrides hero base stats)
    /// </summary>
    public (int health, float attack, float magic, float defense) GetPresetStats()
    {
        return (Health, AttackPower, MagicPower, Defense);
    }
}
