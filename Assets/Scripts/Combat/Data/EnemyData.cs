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
    
    [HorizontalGroup("Stats")]
    [field: SerializeField]
    [field: LabelText("Health Points")]
    [field: Range(1, 3000)]
    [field: InfoBox("@\"HP: \" + Health + (Health <= 20 ? \" (Weak)\" : Health >= 100 ? \" (Tank)\" : \" (Normal)\")", InfoMessageType.None)]
    public int Health { get; private set; }
    
    [HorizontalGroup("Stats")]
    [field: SerializeField]
    [field: LabelText("Attack Damage")]
    [field: Range(1, 500)]
    [field: InfoBox("@\"ATK: \" + AttackPower + (AttackPower <= 5 ? \" (Weak)\" : AttackPower >= 20 ? \" (Strong)\" : \" (Normal)\")", InfoMessageType.None)]
    public int AttackPower { get; private set; }
    
    [InfoBox("@GetEnemyStatsInfo()", InfoMessageType.Info)]
    
    [Title("Enemy Preview", "Detailed stats and analysis", TitleAlignments.Centered)]
    
    [ShowInInspector, ReadOnly]
    [MultiLineProperty(6)]
    [LabelText("Enemy Stats Preview")]
    [InfoBox("Click 'Generate Preview' to update this information", InfoMessageType.None)]
    private string enemyStatsPreview = "Click 'Generate Preview' to see detailed enemy stats...";
    
    [HorizontalGroup("Preview Buttons")]
    [Button("Generate Preview", ButtonSizes.Medium)]
    [GUIColor(0.4f, 0.8f, 1f)]
    private void GeneratePreview()
    {
        enemyStatsPreview = GetDetailedEnemyPreview();
    }
    
    [HorizontalGroup("Preview Buttons")]
    [Button("Copy to Clipboard", ButtonSizes.Medium)]
    [GUIColor(0.8f, 1f, 0.4f)]
    private void CopyPreviewToClipboard()
    {
        if (!string.IsNullOrEmpty(enemyStatsPreview))
        {
            GUIUtility.systemCopyBuffer = enemyStatsPreview;
            Debug.Log("Enemy preview copied to clipboard!");
        }
    }
    
    private string GetDetailedEnemyPreview()
    {
        if (string.IsNullOrWhiteSpace(EnemyName)) return "⚠️ Enemy needs a name!";
        if (Health == 0 || AttackPower == 0) return "⚠️ Enemy has zero stats!";
        
        var preview = new System.Text.StringBuilder();
        preview.AppendLine($"🗡️ ENEMY: {EnemyName}");
        preview.AppendLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        preview.AppendLine($"❤️  Health: {Health:N0} HP");
        preview.AppendLine($"⚔️  Attack: {AttackPower:N0} DMG");
        preview.AppendLine($"🛡️  Toughness: {GetToughnessRating()}");
        preview.AppendLine($"⚡ Threat Level: {GetThreatLevel()}");
        preview.AppendLine();
        preview.AppendLine($"📊 ANALYSIS:");
        preview.AppendLine($"• {GetEnemyType()}");
        preview.AppendLine($"• {GetPlayerSurvivalInfo()}");
        preview.AppendLine($"• {GetRecommendedStrategy()}");
        
        return preview.ToString();
    }
    
    private string GetEnemyStatsInfo()
    {
        if (Health == 0 || AttackPower == 0) return "⚠️ Enemy has zero stats!";
        
        float ratio = (float)Health / AttackPower;
        if (ratio > 10) return "💪 This is a tanky enemy (high HP, low ATK)";
        if (ratio < 3) return "⚡ This is a glass cannon (low HP, high ATK)";
        return "⚖️ This enemy has balanced stats";
    }
    
    private string GetToughnessRating()
    {
        if (Health <= 50) return "Fragile";
        if (Health <= 200) return "Normal";
        if (Health <= 800) return "Tough";
        if (Health <= 1500) return "Very Tough";
        return "Extremely Tough";
    }
    
    private string GetThreatLevel()
    {
        int totalPower = Health + (AttackPower * 10); // Weight attack more heavily
        
        if (totalPower <= 300) return "Low";
        if (totalPower <= 800) return "Medium";
        if (totalPower <= 1500) return "High";
        if (totalPower <= 2500) return "Very High";
        return "Extreme";
    }
    
    private string GetEnemyType()
    {
        float ratio = (float)Health / AttackPower;
        
        if (ratio > 15) return "Tank (High HP, Low Damage)";
        if (ratio > 8) return "Bruiser (Good HP, Moderate Damage)";
        if (ratio > 4) return "Balanced (Even HP and Damage)";
        if (ratio > 2) return "Striker (Moderate HP, High Damage)";
        return "Glass Cannon (Low HP, Very High Damage)";
    }
    
    private string GetPlayerSurvivalInfo()
    {
        int turnsToKillPlayer = 100 / AttackPower; // Assuming 100 player HP
        
        if (turnsToKillPlayer <= 2) return "Can kill player in 1-2 hits! Very dangerous!";
        if (turnsToKillPlayer <= 4) return "Can kill player in 3-4 hits. Dangerous.";
        if (turnsToKillPlayer <= 7) return "Player can survive 5-7 hits. Moderate threat.";
        return "Player can survive many hits. Low immediate danger.";
    }
    
    private string GetRecommendedStrategy()
    {
        float ratio = (float)Health / AttackPower;
        
        if (ratio > 10) return "Use sustained damage over time. Avoid direct confrontation.";
        if (ratio > 5) return "Balance offense and defense. Use moderate damage cards.";
        if (ratio < 3) return "Strike fast before it can retaliate. Use high damage cards.";
        return "Flexible approach. Adapt based on hand and situation.";
    }
}
