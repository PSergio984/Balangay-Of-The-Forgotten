using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.Linq;

[CreateAssetMenu(menuName = "Data/Hero")]
public class HeroData : ScriptableObject
{
    [Title("Hero Information", "Basic properties of this hero", TitleAlignments.Centered)]
    
    [HorizontalGroup("Basic Info", 0.3f)]
    [field: SerializeField]
    [field: PreviewField(100)]
    [field: LabelText("Hero Portrait")]
    [field: Required("Hero needs a portrait!")]
    [field: AssetsOnly]
    public Sprite Image { get; private set; }
    
    [HorizontalGroup("Basic Info", 0.3f)]
    [field: SerializeField]
    [field: PreviewField(100)]
    [field: LabelText("Role Card (Base)")]
    [field: Required("Hero needs a role card sprite!")]
    [field: AssetsOnly]
    [field: InfoBox("This is the default card sprite shown before any preset is selected.\nShould show the character without stat values.")]
    public Sprite RoleCard { get; private set; }
    
    [VerticalGroup("Basic Info/Details")]
    [field: SerializeField]
    [field: LabelText("Hero Name")]
    [field: Required("Hero must have a name!")]
    [field: ValidateInput("@!string.IsNullOrWhiteSpace($value)", "Name cannot be empty or whitespace")]
    public string HeroName { get; private set; }
    
    [VerticalGroup("Basic Info/Details")]
    [field: SerializeField]
    [field: LabelText("Health Points (HP)")]
    [field: Range(50, 2000)]
    [field: InfoBox("@\"HP: \" + Health + (Health <= 80 ? \" (Fragile)\" : Health >= 200 ? \" (Tanky)\" : \" (Normal)\")", InfoMessageType.None)]
    public int Health { get; private set; }
    
    [Title("Hero Stats", "Combat attributes", TitleAlignments.Centered)]
    
    [HorizontalGroup("Combat Stats")]
    [field: SerializeField]
    [field: LabelText("Attack Power (ATK)")]
    [field: Range(0, 400)]
    [field: ValidateInput("@AttackPower >= 0", "Attack Power can't be negative")]
    public float AttackPower { get; private set; }
    
    
    [HorizontalGroup("Combat Stats")]
    [field: SerializeField]
    [field: LabelText("Magic Power (MAG)")]
    [field: Range(0, 400)]
    [field: ValidateInput("@MagicPower >= 0", "Magic Power can't be negative")]
    public float MagicPower { get; private set; }
    
    [HorizontalGroup("Defense Stats")]
    [field: SerializeField]
    [field: LabelText("Defense (DEF)")]
    [field: Range(1, 400)]
    [field: ValidateInput("@Defense > 0", "Defense must be greater than 0")]
    public float Defense { get; private set; }
    
    [Title("Animation", "Animation controller for this hero", TitleAlignments.Centered)]
    [field: SerializeField]
    [field: LabelText("Hero Animator Override Controller")]
    [field: AssetsOnly]
    public AnimatorOverrideController AnimatorOverride { get; private set; }

    [field: SerializeField]
    [field: LabelText("Turn Profile Animator Override Controller")]
    [field: AssetsOnly]
    public AnimatorOverrideController TurnProfileOverride { get; private set; }
    [Title("Build Presets", "Available build variants for this hero", TitleAlignments.Centered)]
    [field: SerializeField]
    [field: LabelText("Available Build Presets")]
    [field: Required("Hero needs at least one build preset!")]
    [field: AssetsOnly]
    [field: ListDrawerSettings(ShowIndexLabels = true, DraggableItems = false, NumberOfItemsPerPage = 3)]
    [field: InfoBox("Typically 3 presets: Glass Cannon, Berserker, Bruiser, etc.\nEach preset has its own sprite with baked-in stats/name display.")]
    [field: ValidateInput("@ValidatePresets()", "Must have at least 1 preset, recommended 3")]
    public List<CharacterBuildPreset> BuildPresets { get; private set; }

    [Title("Hero Deck", "Cards available to this hero", TitleAlignments.Centered)]
    [field: SerializeField]
    [field: LabelText("Starting Deck")]
    [field: Required("Hero needs a deck!")]
    [field: AssetsOnly]
    [field: ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true, ShowPaging = true, NumberOfItemsPerPage = 8)]
    [field: ValidateInput("@ValidateDeck()", "Deck has issues that need to be fixed")]
    public List<CardData> Deck { get; private set; }
    
    private bool ValidateDeck()
    {
        if (Deck == null || Deck.Count == 0) return false;
        
        // Check for null cards
        if (Deck.Any(card => card == null)) return false;
        
        // Check for reasonable deck size
        if (Deck.Count < 5 || Deck.Count > 50) return false;
        
        return true;
    }
    
    private bool ValidatePresets()
    {
        if (BuildPresets == null || BuildPresets.Count == 0) return false;
        
        // Check for null presets
        if (BuildPresets.Any(preset => preset == null)) return false;
        
        return true;
    }
}
