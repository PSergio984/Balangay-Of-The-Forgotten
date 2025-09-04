using SerializeReferenceEditor;
using UnityEngine;
using Sirenix.OdinInspector;

/* AUTO TARGET EFFECT DOCUMENTATION
 * 
 * Purpose: Combines an effect with its targeting mode for automatic target selection
 * 
 * How it works:
 * - Pairs a card effect with a target mode that determines who gets affected
 * - Target mode automatically selects targets (all enemies, random enemy, etc.)
 * - Effect gets applied to the selected targets without player input
 * - Allows cards to have multiple effects with different targeting rules
 * 
 * Integration: Used by Card system and CardData to define multi-effect cards
 */

/// <summary>
/// Wrapper that combines a card effect with its automatic targeting behavior
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Links effects to their targeting rules for automatic execution</para>
/// 
/// <para><strong>What it does:</strong> This class pairs up a card effect (like damage 
/// or healing) with a targeting mode (like "all enemies" or "random ally"). When 
/// a card is played, each AutoTargetEffect automatically finds its targets and 
/// applies its effect without needing player input for target selection.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Card contains a list of AutoTargetEffect instances</item>
/// <item>When card is played, each wrapper gets processed</item>
/// <item>Target mode determines who gets affected (enemies, allies, etc.)</item>
/// <item>Effect gets applied to all selected targets</item>
/// <item>Multiple effects can have different targeting rules on the same card</item>
/// </list>
/// 
/// <para><strong>Examples:</strong></para>
/// <list type="bullet">
/// <item>Damage effect + AllEnemiesTM = hits all enemies</item>
/// <item>Heal effect + RandomTargetTM = heals one random ally</item>
/// <item>Draw effect + NoTM = draws cards (no targets needed)</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> TargetMode classes, Effects classes, Card system</para>
/// 
/// <para><strong>How to use:</strong> Set up in CardData Inspector with target mode and effect references</para>
/// </remarks>
[System.Serializable]
public class AutoTargetEffect
{
    /// <summary>
    /// The targeting behavior that determines who this effect affects
    /// </summary>
    /// <remarks>
    /// This property defines how targets are selected for the effect.
    /// Examples: AllEnemiesTM targets all enemies, RandomTargetTM picks one random target.
    /// Set this in the Inspector to match the intended targeting behavior.
    /// </remarks>
    [field: SerializeReference]
    [field: ShowInInspector, PropertyOrder(1)]
    [field: InfoBox("Choose how this effect selects targets")]
    [field: LabelText("Target Selection")]
    public TargetMode targetMode { get; private set; }

    /// <summary>
    /// The actual effect that gets applied to the selected targets
    /// </summary>
    /// <remarks>
    /// This property holds the effect that will be performed on the targets.
    /// Examples: DealDamageEffect for damage, HealEffect for healing, DrawCardsEffect for card draw.
    /// Set this in the Inspector to define what the effect actually does.
    /// </remarks>
    [field: SerializeReference]
    [field: ShowInInspector, PropertyOrder(2)]
    [field: InfoBox("Choose what effect to apply")]
    [field: LabelText("Effect Type")]
    public Effects effects { get; private set; }
}
