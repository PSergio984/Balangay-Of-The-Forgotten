using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Sirenix.OdinInspector;
/* CARD DATA DOCUMENTATION
 * 
 * Purpose: ScriptableObject that defines the design-time properties and data for cards
 * 
 * How it works:
 * - Stores all card information like name, cost, image, and effects
 * - Can be created as assets in the project for different card types
 * - Supports both manual targeting effects and automatic targeting effects
 * - Used as templates to create runtime Card instances during gameplay
 * 
 * Integration: Used by Card model and CardSystem to create playable cards
 */

/// <summary>
/// ScriptableObject asset that defines a card's properties, effects, and behavior
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Stores the design-time definition of a card that can be created as game assets</para>
/// 
/// <para><strong>What it does:</strong> This ScriptableObject holds all the information 
/// needed to define a card - its name, stamina cost, artwork, and all its effects. 
/// Designers can create different CardData assets for each card type, and the game 
/// will use these to create actual playable cards during gameplay.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Designers create CardData assets in the project</item>
/// <item>Each asset defines one card type with its properties</item>
/// <item>Game loads these assets and creates Card instances from them</item>
/// <item>Cards inherit all properties and effects from their CardData</item>
/// <item>Supports both manual targeting and automatic targeting effects</item>
/// </list>
/// 
/// <para><strong>Effect Types:</strong></para>
/// <list type="bullet">
/// <item>ManualTargetEffect - Player chooses the target (like single-target damage)</item>
/// <item>OtherEffects - Automatic targeting based on target modes (like area damage)</item>
/// </list>
///  
/// <para><strong>Works with:</strong> Card model for runtime instances, CardSystem for gameplay</para>
/// 
/// <para><strong>How to use:</strong> Create menu "Data/Card" to make new card assets, set properties in Inspector</para>
/// </remarks>
[CreateAssetMenu(menuName = "Data/Card")]
public class CardData : ScriptableObject
{
    [Title("Card Information", "Basic properties of this card", TitleAlignments.Centered)]
    [BoxGroup("Basic Info")]
    /// <summary>
    /// The name and description text displayed on the card
    /// </summary>
    /// <remarks>
    /// This property holds the card's title and description that players see.
    /// Set this in the Inspector to define what the card is called and what it does.
    /// </remarks>
    [field: SerializeField] 
    [field: BoxGroup("Basic Info")]
    [field: LabelText("Card Description")]
    [field: MultiLineProperty(3)]
    [field: Required("Card must have a description!")]
    [field: ValidateInput("@!string.IsNullOrWhiteSpace($value)", "Description cannot be empty or whitespace")]
    public string Description { get; private set; }

    /// <summary>
    /// Additional information text displayed on the card
    /// </summary>
    /// <remarks>
    /// This property holds detailed information about the card's mechanics, lore, or usage tips.
    /// This complements the Description field by providing extra context or flavor text.
    /// Set this in the Inspector to give players more details about the card.
    /// </remarks>
    [field: SerializeField] 
    [field: BoxGroup("Basic Info")]
    [field: LabelText("Card Information")]
    [field: MultiLineProperty(3)]
    [field: Required("Card must have information!")]
    [field: ValidateInput("@!string.IsNullOrWhiteSpace($value)", "Information cannot be empty or whitespace")]
    public string Information { get; private set; }

    [HorizontalGroup("Basic Info/Stats", 0.7f)]
    /// <summary>
    /// The stamina cost required to play this card
    /// </summary>
    /// <remarks>
    /// This property defines how much stamina the player must spend to play this card.
    /// Set this in the Inspector to balance the card's power level with its cost.
    /// </remarks>
    [field: SerializeField] 
    [field: HorizontalGroup("Basic Info/Stats")]
    [field: LabelText("Stamina Cost")]
    [field: Range(0, 100)]
    [field: InfoBox("@\"Stamina Cost: \" + Stamina + (Stamina == 0 ? \" (FREE!)\" : Stamina >= 80 ? \" (Expensive)\" : \" (Average)\")", InfoMessageType.None)]
    public int Stamina { get; private set; }

    /// <summary>
    /// The artwork/image displayed on the card
    /// </summary>
    /// <remarks>
    /// This property holds the sprite that appears as the card's visual artwork.
    /// Assign a sprite asset in the Inspector to give the card its visual appearance.
    /// </remarks>
    [field: SerializeField]
    [field: HorizontalGroup("Basic Info/Stats", 0.3f)]
    [field: PreviewField(75)]
    [field: LabelText("Card Art")]
    [field: Required("Card needs artwork!")]
    [field: AssetsOnly]
    public Sprite Image { get; private set; }

    /// <summary>
    /// The role icon displayed on the card (tank, fighter, support, etc.)
    /// </summary>
    /// <remarks>
    /// This property holds the sprite that shows what class/role this card belongs to.
    /// Examples: sword icon for fighter cards, shield for tank cards, staff for support.
    /// Assign a role-specific sprite asset in the Inspector to categorize the card.
    /// This helps players quickly identify what type of card they're looking at.
    /// </remarks>
    [field: SerializeField]
    [field: HorizontalGroup("Basic Info/Stats", 0.3f)]
    [field: PreviewField(75)]
    [field: LabelText("Card Role Icon")]
    [field: Required("Card needs Role Icon!")]
    [field: AssetsOnly]
    public Sprite RoleIcon { get; private set; }

    [Title("Card Effects", "Define what this card does when played", TitleAlignments.Centered)]
    [InfoBox("Manual Target Effect: Player chooses the target (like single-target damage)\n" +
             "Other Effects: Automatic targeting (like area damage, self-buffs)", InfoMessageType.Info)]
    
    [InfoBox("@GetCardValidationMessage()", InfoMessageType.Warning, "HasCardValidationIssues")]
    
    /// <summary>
    /// Single effect that requires manual target selection by the player
    /// </summary>
    /// <remarks>
    /// This property holds an effect where the player must choose the target.
    /// Examples: single-target damage spell, targeted heal, specific enemy debuff.
    /// Can be null if the card doesn't have any manual targeting effects.
    /// Set this in the Inspector for cards that need player target selection.
    /// </remarks>
    [field: SerializeReference] 
    [field: ShowInInspector]
    [field: LabelText("Main Effect (Manual Target)")]
    [field: InfoBox("This effect requires the player to choose a target", InfoMessageType.None, "@ManualTargetEffect != null")]
    public Effects ManualTargetEffect { get; private set; } = null;
        
    /// <summary>
    /// List of effects that automatically select their own targets
    /// </summary>
    /// <remarks>
    /// This property contains effects that don't need player input for targeting.
    /// Each effect has its own target mode (all enemies, random target, no target, etc.).
    /// Cards can have multiple automatic effects that trigger when the card is played.
    /// Examples: area damage + card draw, self-heal + enemy debuff, multiple different effects.
    /// Set these in the Inspector for cards with automatic or multiple effects.
    /// </remarks>
    //can have 1 effect, where you pick a target, also can have multiple other effects  where target is selected auto
    [field: SerializeField] 
    [field: LabelText("Secondary Effects (Auto-Target)")]
    [field: ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true)]
    [field: ValidateInput("@ValidateOtherEffects()", "One or more auto-target effects have missing components")]
    [field: InfoBox("@GetOtherEffectsInfo()", InfoMessageType.Info, "@OtherEffects != null && OtherEffects.Count > 0")]
    public List<AutoTargetEffect> OtherEffects { get; private set; }
    
    // Validation methods for better debugging
    private bool HasCardValidationIssues()
    {
        return ManualTargetEffect == null && (OtherEffects == null || OtherEffects.Count == 0);
    }
    
    private string GetCardValidationMessage()
    {
        if (ManualTargetEffect == null && (OtherEffects == null || OtherEffects.Count == 0))
            return "⚠️ This card has no effects! Add either a Manual Target Effect or Other Effects.";
        return "";
    }
    
    private bool ValidateOtherEffects()
    {
        if (OtherEffects == null) return true;
        
        for (int i = 0; i < OtherEffects.Count; i++)
        {
            var effect = OtherEffects[i];
            if (effect == null) return false;
            if (effect.targetMode == null || effect.effects == null) return false;
        }
        return true;
    }
    
    private string GetOtherEffectsInfo()
    {
        if (OtherEffects == null || OtherEffects.Count == 0) return "";
        return $"💡 This card has {OtherEffects.Count} auto-target effect(s)";
    }
}