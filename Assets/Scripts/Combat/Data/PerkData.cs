using SerializeReferenceEditor;
using UnityEngine;

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
    /// <summary>
    /// Visual icon displayed for this perk in the UI
    /// </summary>
    /// <remarks>
    /// Sprite that represents this perk in menus, character sheets, or perk selection screens.
    /// Assign in the Inspector to give each perk a distinctive visual identity.
    /// </remarks>
    [field: SerializeField] public Sprite Image { get; private set; }
    
    /// <summary>
    /// Condition that determines when this perk triggers
    /// </summary>
    /// <remarks>
    /// Uses SerializeReference to allow any PerkCondition child class (OnEnemyAttackCondition, 
    /// OnCardPlayCondition, etc.). The SR attribute provides an Inspector dropdown to select 
    /// which specific condition type to use. This makes perks flexible and data-driven.
    /// </remarks>
    [field: SerializeReference, SR] public PerkCondition PerkCondition { get; private set; }
    
    /// <summary>
    /// Effect that gets applied when the perk condition is met
    /// </summary>
    /// <remarks>
    /// Uses SerializeReference to allow any AutoTargetEffect child class (damage, healing, 
    /// buffs, etc.). The SR attribute provides an Inspector dropdown for effect selection. 
    /// This allows designers to mix and match conditions with effects freely.
    /// </remarks>
    [field: SerializeReference, SR] public AutoTargetEffect AutoTargetEffect { get; private set; }
    
    /// <summary>
    /// Whether this perk should automatically determine its targets
    /// </summary>
    /// <remarks>
    /// When true, the perk uses the AutoTargetEffect's built-in targeting logic.
    /// When false, requires manual target specification. Most perks use auto-targeting 
    /// for simplicity and consistency.
    /// </remarks>
    [field: SerializeField] public bool UseAutoTarget { get; private set; } = true;
    
    /// <summary>
    /// Whether this perk should target the entity that triggered the action
    /// </summary>
    /// <remarks>
    /// When true, the perk targets whoever caused the triggering action (like the attacking enemy).
    /// When false, uses normal targeting logic. Useful for reactive perks that affect the attacker.
    /// </remarks>
    [field: SerializeField] public bool UseActionCasterAsTarget { get; private set; } = false;
}
