using SerializeReferenceEditor;
using UnityEngine;
using Sirenix.OdinInspector;

/* PERK DATA DOCUMENTATION
 * 
 * How it works:
 * - Creates ScriptableObject assets that define perk behaviors
 * - Stores perk visual (image) and gameplay data (condition, effect, targeting)
 * - Uses SerializeReference to allow different condition and effect types in the same field
 * - Provides targeting options for how the perk effect gets applied
 * 
 * Design reasoning:
 * - SerializeReference allows different perk condition types (OnEnemyAttack, OnCardPlay, etc.) in the same field
 * - SR attribute from SerializeReferenceEditor package provides better Inspector dropdown for type selection
 * - ScriptableObject design makes perks data-driven and easily editable by designers
 * - Targeting flags provide flexibility for different perk application modes
 * 
 * Integration: Used by perk system, works with PerkCondition and AutoTargetEffect class families
 */

/// <summary>
/// ScriptableObject that defines a perk's visual appearance and gameplay behavior
/// </summary>
/// <remarks>
/// <para><strong>SerializeReference explanation:</strong> This attribute allows Unity to serialize 
/// references to abstract parent types and their child implementations. Without it, Unity 
/// can only serialize concrete types directly. The SR attribute comes from the SerializeReferenceEditor 
/// package and provides a better dropdown interface in the Inspector for selecting which specific 
/// child type to use (like OnEnemyAttackCondition vs OnCardPlayCondition).</para>
/// 
/// <para><strong>Why use SerializeReference:</strong> Enables different child classes in Unity's serialization. 
/// Instead of having separate fields for each condition type, we can have one PerkCondition field 
/// that can be any child class. This makes the system extensible - adding new condition types doesn't 
/// require changing this data structure.</para>
/// </remarks>
[CreateAssetMenu(menuName = "Data/Perk")]
public class PerkData : ScriptableObject
{
    [Title("Perk Appearance", "Visual representation of this perk", TitleAlignments.Centered)]
    
    /// <summary>
    /// Visual icon displayed for this perk in the UI
    /// </summary>
    /// <remarks>
    /// Sprite that represents this perk in menus, character sheets, or perk selection screens.
    /// Assign in the Inspector to give each perk a distinctive visual identity.
    /// </remarks>
    [field: SerializeField]
    [field: PreviewField(80)]
    [field: LabelText("Perk Icon")]
    [field: Required("Perk needs an icon for UI display!")]
    [field: AssetsOnly]
    [field: InfoBox("This icon represents the perk in menus and character sheets", InfoMessageType.Info)]
    public Sprite Image { get; private set; }
    
    [Title("Perk Behavior", "When and how this perk triggers", TitleAlignments.Centered)]
    
    /// <summary>
    /// Condition that determines when this perk triggers
    /// </summary>
    /// <remarks>
    /// Uses SerializeReference to allow any PerkCondition child class (OnEnemyAttackCondition, 
    /// OnCardPlayCondition, etc.). The SR attribute provides an Inspector dropdown to select 
    /// which specific condition type to use. This makes perks flexible and data-driven.
    /// </remarks>
    [field: SerializeReference, SR]
    [field: LabelText("Trigger Condition")]
    [field: Required("Perk must have a trigger condition!")]
    [field: InfoBox("Choose when this perk should activate (enemy attack, card play, etc.)", InfoMessageType.None)]
    [field: ValidateInput("@PerkCondition != null", "Perk condition cannot be null")]
    public PerkCondition PerkCondition { get; private set; }
    
    /// <summary>
    /// Effect that gets applied when the perk condition is met
    /// </summary>
    /// <remarks>
    /// Uses SerializeReference to allow any AutoTargetEffect child class (damage, healing, 
    /// buffs, etc.). The SR attribute provides an Inspector dropdown for effect selection. 
    /// This allows designers to mix and match conditions with effects freely.
    /// </remarks>
    [field: SerializeReference, SR]
    [field: LabelText("Perk Effect")]
    [field: Required("Perk must have an effect!")]
    [field: InfoBox("Choose what happens when the perk triggers (damage, heal, buff, etc.)", InfoMessageType.None)]
    [field: ValidateInput("@AutoTargetEffect != null", "Perk effect cannot be null")]
    public AutoTargetEffect AutoTargetEffect { get; private set; }
    
    [Title("Targeting Options", "How the perk selects its targets", TitleAlignments.Centered)]
    [InfoBox("Configure how this perk determines who gets affected when it triggers", InfoMessageType.Info)]
    
    [HorizontalGroup("Targeting")]
    /// <summary>
    /// Whether this perk should automatically determine its targets
    /// </summary>
    /// <remarks>
    /// When true, the perk uses the AutoTargetEffect's built-in targeting logic.
    /// When false, requires manual target specification. Most perks use auto-targeting 
    /// for simplicity and consistency.
    /// </remarks>
    [field: SerializeField]
    [field: HorizontalGroup("Targeting")]
    [field: LabelText("Auto Target")]
    [field: InfoBox("Uses automatic targeting", InfoMessageType.Info, "@UseAutoTarget")]
    [field: InfoBox("Requires manual targeting", InfoMessageType.Warning, "@!UseAutoTarget")]
    public bool UseAutoTarget { get; private set; } = true;
    
    [HorizontalGroup("Targeting")]
    /// <summary>
    /// Whether this perk should target the entity that triggered the action
    /// </summary>
    /// <remarks>
    /// When true, the perk targets whoever caused the triggering action (like the attacking enemy).
    /// When false, uses normal targeting logic. Useful for reactive perks that affect the attacker.
    /// </remarks>
    [field: SerializeField]
    [field: HorizontalGroup("Targeting")]
    [field: LabelText("Target Caster")]
    [field: InfoBox("🎯 Targets the action caster", InfoMessageType.Info, "@UseActionCasterAsTarget")]
    public bool UseActionCasterAsTarget { get; private set; } = false;
    
    [InfoBox("@GetPerkSummary()", InfoMessageType.None)]
    
    private string GetPerkSummary()
    {
        if (PerkCondition == null || AutoTargetEffect == null)
            return "⚠️ Incomplete perk setup - add condition and effect";
            
        var summary = $"📋 PERK SUMMARY:\n";
        summary += $"• Triggers: {(PerkCondition != null ? PerkCondition.GetType().Name.Replace("Condition", "") : "Not Set")}\n";
        summary += $"• Effect: {(AutoTargetEffect?.effects != null ? AutoTargetEffect.effects.GetType().Name.Replace("Effect", "") : "Not Set")}\n";
        summary += $"• Targeting: {(UseAutoTarget ? "Automatic" : "Manual")}";
        
        if (UseActionCasterAsTarget)
            summary += " → Targets action caster";
            
        return summary;
    }
}
