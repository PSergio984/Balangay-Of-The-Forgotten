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
    [field: Range(1, 400)]
    [field: ValidateInput("@AttackPower > 0", "Attack Power must be greater than 0")]
    public int AttackPower { get; private set; }
    
    [HorizontalGroup("Combat Stats")]
    [field: SerializeField]
    [field: LabelText("Attack Speed (ATSP)")]
    [field: Range(1, 200)]
    [field: ValidateInput("@AttackSpeed > 0", "Attack Speed must be greater than 0")]
    public int AttackSpeed { get; private set; }
    
    [HorizontalGroup("Combat Stats")]
    [field: SerializeField]
    [field: LabelText("Magic Power (MAG)")]
    [field: Range(1, 300)]
    [field: ValidateInput("@MagicPower > 0", "Magic Power must be greater than 0")]
    public int MagicPower { get; private set; }
    
    [HorizontalGroup("Defense Stats")]
    [field: SerializeField]
    [field: LabelText("Defense (DEF)")]
    [field: Range(1, 400)]
    [field: ValidateInput("@Defense > 0", "Defense must be greater than 0")]
    public int Defense { get; private set; }
    
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
}
