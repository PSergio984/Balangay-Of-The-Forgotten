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
    [field: LabelText("Health Points")]
    [field: Range(50, 500)]
    [field: InfoBox("@\"HP: \" + Health + (Health <= 80 ? \" (Fragile)\" : Health >= 200 ? \" (Tanky)\" : \" (Normal)\")", InfoMessageType.None)]
    public int Health { get; private set; }
    
    [Title("Hero Deck", "Cards available to this hero", TitleAlignments.Centered)]
    
    [field: SerializeField]
    [field: LabelText("Starting Deck")]
    [field: Required("Hero needs a deck!")]
    [field: AssetsOnly]
    [field: ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true, ShowPaging = true, NumberOfItemsPerPage = 8)]
    [field: ValidateInput("@ValidateDeck()", "Deck has issues that need to be fixed")]
    [field: InfoBox("@GetDeckInfo()", InfoMessageType.Info, "@Deck != null && Deck.Count > 0")]
    public List<CardData> Deck { get; private set; }
    
    [Title("Hero Preview", "Detailed stats and deck analysis", TitleAlignments.Centered)]
    
    [ShowInInspector, ReadOnly]
    [MultiLineProperty(15)]
    [LabelText("Hero Stats Preview")]
    [InfoBox("Click 'Generate Preview' to update this information", InfoMessageType.None)]
    private string heroStatsPreview = "Click 'Generate Preview' to see detailed hero stats and deck analysis...";
    
    [HorizontalGroup("Preview Buttons")]
    [Button("Generate Preview", ButtonSizes.Medium)]
    [GUIColor(0.4f, 0.8f, 1f)]
    private void GeneratePreview()
    {
        heroStatsPreview = GetDetailedHeroPreview();
    }
    
    [HorizontalGroup("Preview Buttons")]
    [Button("Copy to Clipboard", ButtonSizes.Medium)]
    [GUIColor(0.8f, 1f, 0.4f)]
    private void CopyPreviewToClipboard()
    {
        if (!string.IsNullOrEmpty(heroStatsPreview))
        {
            GUIUtility.systemCopyBuffer = heroStatsPreview;
            Debug.Log("Hero preview copied to clipboard!");
        }
    }
    
    [HorizontalGroup("Preview Buttons")]
    [Button("Analyze Deck Balance", ButtonSizes.Medium)]
    [GUIColor(1f, 0.8f, 0.4f)]
    private void AnalyzeDeckBalance()
    {
        if (Deck == null || Deck.Count == 0)
        {
            Debug.LogWarning("No deck to analyze!");
            return;
        }
        
        Debug.Log("=== DECK ANALYSIS ===");
        Debug.Log($"Total Cards: {Deck.Count}");
        Debug.Log($"Average Stamina Cost: {GetAverageStaminaCost():F1}");
        Debug.Log($"Stamina Distribution: {GetStaminaDistribution()}");
        Debug.Log($"Deck Recommendation: {GetDeckRecommendation()}");
    }
    
    private bool ValidateDeck()
    {
        if (Deck == null || Deck.Count == 0) return false;
        
        // Check for null cards
        if (Deck.Any(card => card == null)) return false;
        
        // Check for reasonable deck size
        if (Deck.Count < 5 || Deck.Count > 50) return false;
        
        return true;
    }
    
    private string GetDeckInfo()
    {
        if (Deck == null || Deck.Count == 0) return "";
        
        int totalCards = Deck.Count;
        float avgCost = GetAverageStaminaCost();
        string sizeCategory = totalCards <= 15 ? "Small" : totalCards <= 25 ? "Medium" : "Large";
        
        return $"🃏 {totalCards} cards | Avg cost: {avgCost:F1} | {sizeCategory} deck";
    }
    
    private string GetDetailedHeroPreview()
    {
        if (string.IsNullOrWhiteSpace(HeroName)) return "⚠️ Hero needs a name!";
        if (Health == 0) return "⚠️ Hero has zero health!";
        if (Deck == null || Deck.Count == 0) return "⚠️ Hero needs a deck!";
        
        var preview = new System.Text.StringBuilder();
        preview.AppendLine($"🛡️ HERO: {HeroName}");
        preview.AppendLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        preview.AppendLine($"❤️  Health: {Health} HP");
        preview.AppendLine($"🃏  Deck Size: {Deck.Count} cards");
        preview.AppendLine($"⚡ Avg Stamina: {GetAverageStaminaCost():F1}");
        preview.AppendLine($"🎯 Hero Type: {GetHeroType()}");
        preview.AppendLine();
        preview.AppendLine($"📊 DECK ANALYSIS:");
        preview.AppendLine($"• {GetStaminaDistribution()}");
        preview.AppendLine($"• {GetDeckBalance()}");
        preview.AppendLine($"• {GetDeckRecommendation()}");
        preview.AppendLine();
        preview.AppendLine($"⚔️ COMBAT READINESS:");
        preview.AppendLine($"• {GetSurvivabilityRating()}");
        preview.AppendLine($"• {GetPlaystyleDescription()}");
        
        return preview.ToString();
    }
    
    private float GetAverageStaminaCost()
    {
        if (Deck == null || Deck.Count == 0) return 0f;
        return (float)Deck.Where(card => card != null).Average(card => card.Stamina);
    }
    
    private string GetStaminaDistribution()
    {
        if (Deck == null || Deck.Count == 0) return "No cards";
        
        var validCards = Deck.Where(card => card != null).ToList();
        int lowCost = validCards.Count(card => card.Stamina <= 2);
        int midCost = validCards.Count(card => card.Stamina >= 3 && card.Stamina <= 5);
        int highCost = validCards.Count(card => card.Stamina >= 6);
        
        return $"Low cost: {lowCost}, Mid cost: {midCost}, High cost: {highCost}";
    }
    
    private string GetHeroType()
    {
        if (Health <= 80) return "Glass Cannon (Low HP)";
        if (Health <= 120) return "Balanced (Normal HP)";
        if (Health <= 180) return "Durable (High HP)";
        return "Tank (Very High HP)";
    }
    
    private string GetDeckBalance()
    {
        var avgCost = GetAverageStaminaCost();
        
        if (avgCost <= 2.5f) return "Fast deck (Low stamina costs)";
        if (avgCost <= 4f) return "Balanced deck (Mixed costs)";
        return "Slow deck (High stamina costs)";
    }
    
    private string GetDeckRecommendation()
    {
        var avgCost = GetAverageStaminaCost();
        var deckSize = Deck?.Count ?? 0;
        
        if (deckSize < 10) return "Consider adding more cards for consistency";
        if (deckSize > 30) return "Consider reducing deck size for reliability";
        if (avgCost > 5f) return "Add more low-cost cards for early game";
        if (avgCost < 2f) return "Add some high-impact expensive cards";
        return "Deck has good balance and size";
    }
    
    private string GetSurvivabilityRating()
    {
        if (Health <= 80) return "Low survivability - needs careful play";
        if (Health <= 120) return "Moderate survivability - balanced approach";
        if (Health <= 180) return "High survivability - can take risks";
        return "Excellent survivability - very durable";
    }
    
    private string GetPlaystyleDescription()
    {
        var avgCost = GetAverageStaminaCost();
        
        if (Health > 150 && avgCost > 4f) return "Control playstyle - outlast enemies with powerful cards";
        if (Health < 100 && avgCost < 3f) return "Aggro playstyle - win quickly with cheap cards";
        if (avgCost >= 3f && avgCost <= 4f) return "Midrange playstyle - balanced offense and defense";
        return "Flexible playstyle - adapt to situations";
    }
}
