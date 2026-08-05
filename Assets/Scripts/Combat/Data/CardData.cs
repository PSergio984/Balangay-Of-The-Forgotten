using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Sirenix.OdinInspector;
using AudioSystem;
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
    /// The title of the card
    /// </summary>
    /// <remarks>
    /// This property holds the card's title that players see.
    /// Set this in the Inspector to define the card's title text.
    /// </remarks>
    [field: SerializeField]
    [field: BoxGroup("Basic Info")]
    [field: LabelText("Card title")]
    [field: MultiLineProperty(3)]
    [field: Required("Card must have a title!")]
    [field: ValidateInput("@!string.IsNullOrWhiteSpace($value)", "title cannot be empty or whitespace")]
    public string Title { get; private set; }

    /// <summary>
    /// <summary>
    /// The target mode of the card (choose what this card targets when played)
    /// </summary>
    /// <remarks>
    /// Select from options: Area of Attack, Single Ally, All Allies, Self, Single Target.
    /// </remarks>
    [field: SerializeField]
    [field: BoxGroup("Basic Info")]
    [field: LabelText("Target Mode")]
    [field: Required("Card must have a Target Mode!")]
    [field: EnumToggleButtons] // Now shows "Area of Attack" instead of "AreaOfAttack"
    public CardTargetMode Target { get; private set; }
    /// <summary>
    /// Additional Description text displayed on the card
    /// <summary>
    /// Description text displayed on the card
    /// </summary>
    /// <remarks>
    /// This property holds detailed information about the card's mechanics, lore, or usage tips.
    /// Set this in the Inspector to give players more details about the card.
    /// </remarks>
    [field: SerializeField]
    [field: BoxGroup("Basic Info")]
    [field: LabelText("Card Description")]
    [field: MultiLineProperty(3)]
    [field: Required("Card must have Description!")]
    [field: ValidateInput("@!string.IsNullOrWhiteSpace($value)", "Description cannot be empty or whitespace")]
    public string Description { get; private set; }    /// <summary>
    /// The stamina cost required to play this card
    /// </summary>
    /// <remarks>
    /// This property defines how much stamina the player must spend to play this card.
    /// Set this in the Inspector to balance the card's power level with its cost.
    /// </remarks>
    // [field: SerializeField] 
    // [field: HorizontalGroup("Basic Info/Stats")]
    // [field: LabelText("Stamina Cost")]
    // [field: Range(0, 100)]
    // [field: InfoBox("@\"Stamina Cost: \" + Stamina + (Stamina == 0 ? \" (FREE!)\" : Stamina >= 80 ? \" (Expensive)\" : \" (Average)\")", InfoMessageType.None)]
    // public int Stamina { get; private set; }

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
    public Sprite Art { get; private set; }

    /// <summary>
    /// The background display image for the card
    /// </summary>
    /// <remarks>
    /// This property holds the sprite that appears as the card's background image.
    /// Assign a sprite asset in the Inspector to give the card its visual background appearance.
    /// </remarks>
    [field: SerializeField]
    [field: HorizontalGroup("Basic Info/Stats", 0.3f)]
    [field: PreviewField(75)]
    [field: LabelText("Card Background Art")]
    [field: Required("Card needs Background Art!")]
    [field: AssetsOnly]
    public Sprite BackgroundArt { get; private set; }

    /// <summary>
    /// The role defines the visual theme of the card (border, icon, etc.).
    /// </summary>
    [field: SerializeField]
    [field: BoxGroup("Basic Info")]
    [field: Required("Card must have a role!")]
    [field: AssetsOnly]
    [field: LabelText("Card Role")]
    public CardRoleData RoleData { get; private set; }

    /// <summary>
    /// The cooldown duration for this card (number of rounds before it can be played again)
    /// </summary>
    /// <remarks>
    /// After playing a card with cooldown, it becomes unplayable for this many rounds.
    /// 0 means no cooldown - the card can be played every turn.
    /// Cooldown decreases by 1 at the start of each player turn.
    /// </remarks>
    [field: SerializeField]
    [field: BoxGroup("Basic Info")]
    [field: LabelText("Cooldown (Rounds)")]
    [field: Range(0, 10)]
    [field: InfoBox("@Cooldown == 0 ? \"No cooldown - can play every turn\" : \"After playing, wait \" + Cooldown + \" round(s) to play again\"", InfoMessageType.None)]
    public int Cooldown { get; private set; } = 0;


    [Title("Audio", "Sound effect for this card", TitleAlignments.Centered)]
    
    /// <summary>
    /// Sound effect played when this card is played
    /// </summary>
    /// <remarks>
    /// Optional sound that plays when the card is cast/played.
    /// If left empty, no sound will play for this card.
    /// </remarks>
    [field: SerializeField]
    [field: LabelText("Card Sound Effect")]
    [field: InfoBox("Optional: Assign a sound effect to play when this card is played", InfoMessageType.None)]
    [field: AssetsOnly]
    public SoundData SoundData { get; private set; }

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



public enum CardTargetMode
{
    [LabelText("Area of Attack")]
    AreaOfAttack,

    [LabelText("Single Ally")]
    SingleAlly,

    [LabelText("All Allies")]
    AllAllies,

    [LabelText("Self")]
    Self,

    [LabelText("Single Target")]
    SingleTarget,

    [LabelText("Everyone")]
    Everyone,
}

    public static class CardTargetModeExtensions
    {
        public static string ToDisplayString(this CardTargetMode targetMode)
        {
            return targetMode switch
            {
                CardTargetMode.AreaOfAttack => "Area of Attack",
                CardTargetMode.SingleAlly => "Single Ally", 
                CardTargetMode.AllAllies => "All Allies",
                CardTargetMode.Self => "Self",
                CardTargetMode.SingleTarget => "Single Target",
                CardTargetMode.Everyone => "Everyone",
                _ => targetMode.ToString()
            };
        }
    }