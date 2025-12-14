using UnityEngine;
using System.Collections.Generic;

/* SPECIAL CARD DATA
 * 
 * Purpose: Defines a special card obtained from mini-boss defeats
 * 
 * How it works:
 * - Special cards are powerful buff cards with unique team-wide effects
 * - Unlike regular CardData, special cards use a simplified structure (sprite-only UI)
 * - Special cards persist across maps if not played, but are consumed when used
 * - Three types as per AllMoves.md:
 *   1. Dagát ng Kabisayaan: +15% DMG to all players, 2 stacks
 *   2. Daragang Magayon: +25% DEF to 2 players, 1 stack
 *   3. Bundok Pulag: No cooldown for all skills, 4 rounds
 * 
 * Integration:
 * - Referenced by RewardData.specialCardReward
 * - Collected by SpecialCardCollectionData when mini-boss is defeated
 * - Displayed by SpecialCardPanelUI in combat scene
 * - Played via special card button clicks, consumed after use
 */

/// <summary>
/// ScriptableObject that defines a special card from mini-boss defeats.
/// Special cards have unique team-wide effects and simplified sprite-only UI.
/// </summary>
[CreateAssetMenu(fileName = "New Special Card", menuName = "SpecialCards/Special Card Data")]
public class SpecialCardData : ScriptableObject
{
    /// <summary>
    /// Type of special card effect
    /// </summary>
    public enum SpecialCardEffectType
    {
        /// <summary>
        /// Dagát ng Kabisayaan: +15% DMG to all players for 2 turns
        /// </summary>
        DamageUp,
        
        /// <summary>
        /// Daragang Magayon: +25% DEF to all players for 1 turn
        /// </summary>
        DefenseUp,
        
        /// <summary>
        /// Bundok Pulag: No cooldown for all skills for 4 rounds
        /// </summary>
        NoCooldown
    }
    
    #region Identification
    
    [Header("Identification")]
    
    /// <summary>
    /// Unique identifier for this special card
    /// </summary>
    [Tooltip("Unique ID for this special card - used for persistence")]
    [SerializeField] private string cardId;
    public string CardId => cardId;
    
    /// <summary>
    /// Display name shown in UI
    /// </summary>
    [Tooltip("Display name shown when the card is obtained or used")]
    [SerializeField] private string cardName;
    public string CardName => cardName;
    
    #endregion
    
    #region Visuals
    
    [Header("Visuals (Simplified UI)")]
    
    /// <summary>
    /// The card's sprite shown in the special card panel
    /// </summary>
    [Tooltip("Main sprite displayed in the special card UI slot")]
    [SerializeField] private Sprite cardSprite;
    public Sprite CardSprite => cardSprite;
    
    /// <summary>
    /// Optional glow or border sprite
    /// </summary>
    [Tooltip("Optional glow effect sprite shown behind the card")]
    [SerializeField] private Sprite glowSprite;
    public Sprite GlowSprite => glowSprite;
    
    #endregion
    
    #region Effect Configuration
    
    [Header("Effect Configuration")]
    
    /// <summary>
    /// The type of effect this card applies
    /// </summary>
    [Tooltip("What effect this special card applies when used")]
    [SerializeField] private SpecialCardEffectType effectType;
    public SpecialCardEffectType EffectType => effectType;
    
    /// <summary>
    /// Effect percentage (e.g., 15 for +15% DMG, 25 for +25% DEF)
    /// </summary>
    [Tooltip("Percentage value for the effect (e.g., 15 = +15%)")]
    [SerializeField] private int effectPercentage = 15;
    public int EffectPercentage => effectPercentage;
    
    /// <summary>
    /// Effect duration in turns/stacks
    /// </summary>
    [Tooltip("Duration of the effect in turns or stacks")]
    [SerializeField] private int duration = 2;
    public int Duration => duration;
    
    /// <summary>
    /// Number of targets (legacy field, kept for compatibility but DefenseUp now applies to all players)
    /// </summary>
    [Tooltip("Legacy field - DefenseUp now applies to all players. Kept for backwards compatibility.")]
    [SerializeField] private int targetCount = 2;
    public int TargetCount => targetCount;
    
    #endregion
    
    #region Metadata
    
    [Header("Metadata")]
    
    /// <summary>
    /// Description of the card's effect
    /// </summary>
    [Tooltip("Description shown in tooltips or popups")]
    [TextArea(2, 4)]
    [SerializeField] private string description;
    public string Description => description;
    
    /// <summary>
    /// The map this special card came from
    /// </summary>
    [Tooltip("Which map's mini-boss grants this card")]
    [SerializeField] private string sourceMapId;
    public string SourceMapId => sourceMapId;
    
    #endregion
    
    #region Helpers
    
    /// <summary>
    /// Returns a formatted description of the card's effect
    /// </summary>
    public string GetEffectDescription()
    {
        return effectType switch
        {
            SpecialCardEffectType.DamageUp => 
                $"+{effectPercentage}% damage to all allies for {duration} turns",
            SpecialCardEffectType.DefenseUp => 
                $"+{effectPercentage}% defense to all allies for {duration} turns",
            SpecialCardEffectType.NoCooldown => 
                $"No cooldowns for all skills for {duration} rounds",
            _ => description
        };
    }
    
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(cardId))
        {
            Debug.LogWarning($"[SpecialCardData] {name} is missing a cardId!", this);
        }
        if (cardSprite == null)
        {
            Debug.LogWarning($"[SpecialCardData] {name} is missing a cardSprite!", this);
        }
    }
    
    #endregion
}
