using System.Collections.Generic; 
using UnityEngine;

/* DEAL DAMAGE GA DOCUMENTATION
 * 
 * How it works:
 * - Keeps damage planning separate from actually hurting characters
 * - Implements IHaveCaster so perks can know who caused the damage
 * - Allows damage changes, blocking, and other systems to step in and modify things
 * - Works the same way for hitting one target or many targets at once
 * 
 * Design reasoning:
 * - Separates "want to damage" from "actually damage" for cleaner code organization
 * - Caster tracking enables perks to react to damage sources
 * - Lets different parts of the game create damage without knowing how it works inside
 * - Plan-then-do approach supports complex combat features and perk interactions
 * 
 * Integration: Part of the plan-then-do pattern, supports perk system through caster tracking
 */

/// <summary>
/// Action that represents wanting to hurt someone, kept separate from actually doing it
/// </summary>
/// <remarks>
/// <para><strong>Main purpose:</strong> Makes the damage system flexible by using a plan-then-do approach</para>
/// 
/// <para><strong>Why this approach:</strong> Instead of directly changing health numbers, 
/// this creates a "plan to damage" that other parts of the game can see and change. 
/// This lets us have complex combat features like damage shields, armor calculations, 
/// critical hits, and special effects without the different parts of the game being 
/// too connected to each other.</para>
/// 
/// <para><strong>Perk system integration:</strong> The Caster property lets perks know 
/// who is causing damage. This enables reactive perks that can modify damage based on 
/// who is dealing it, or apply effects back to the damage dealer.</para>
/// 
/// <para><strong>Benefits for perks:</strong></para>
/// <list type="bullet">
/// <item>Perks can see who is dealing damage and react accordingly</item>
/// <item>Damage reflection perks can target the original damage source</item>
/// <item>Different perk effects based on whether damage comes from allies or enemies</item>
/// <item>Supports complex perk interactions like "when X deals damage to Y"</item>
/// </list>
/// 
/// <para><strong>How it flows:</strong> Plan → Perks can see and react → Process → Others react</para>
/// </remarks>
public class DealDamageGA : GameAction, IHaveCaster
{
    /// <summary>
    /// Starting damage amount before any changes or calculations (uniform damage)
    /// </summary>
    /// <remarks>
    /// Raw damage number that gets handled by DamageSystem.
    /// Things like armor, buffs, or resistances get applied when processing.
    /// Used when all targets take the same damage amount.
    /// </remarks>
    public float Amount { get; set; }
    
    /// <summary>
    /// Optional per-target damage amounts (for defense-based calculations)
    /// </summary>
    /// <remarks>
    /// When set, each target receives their corresponding damage value from this list.
    /// This allows proper per-target defense calculations in AoE attacks.
    /// If null, falls back to Amount for uniform damage across all targets.
    /// List indices must match Targets list indices.
    /// </remarks>
    public List<float> PerTargetDamages { get; set; }
    
    /// <summary>
    /// Who will receive the damage
    /// </summary>
    /// <remarks>
    /// Works for both hitting one target or many targets.
    /// List approach makes area effects work without special handling.
    /// </remarks>
    public List<CombatantView> Targets { get; set; }
    
    /// <summary>
    /// Who is responsible for dealing this damage (for perk system)
    /// </summary>
    /// <remarks>
    /// Tracks the source of damage so perks can react appropriately.
    /// Enables perks like "when you deal damage" or "when enemies damage you".
    /// Critical for reactive perk targeting and effect attribution.
    /// </remarks>
    public CombatantView Caster { get; set; }

    /// <summary>
    /// Creates damage action with uniform damage for all targets
    /// </summary>
    /// <param name="amount">Starting damage before changes (applied to all targets)</param>
    /// <param name="targets">Characters who will get hurt</param>
    /// <param name="caster">Who is responsible for this damage</param>
    /// <remarks>
    /// Safe copy prevents outside changes to the list from messing up planned actions.
    /// Caster parameter enables perk system to know who caused the damage.
    /// Important for keeping the action system working properly where planned actions can't be changed.
    /// Use this constructor when all targets should take the same damage amount.
    /// </remarks>
    public DealDamageGA(float amount, List<CombatantView> targets, CombatantView caster)
    {
        Amount = amount;
        // Safe copy prevents outside changes to planned action
        Targets = new(targets);
        // Store who is causing this damage for perk system
        Caster = caster;
        // PerTargetDamages is null, so DamageSystem will use Amount for all targets
        PerTargetDamages = null;
    }
    
    /// <summary>
    /// Creates damage action with per-target damage amounts (for defense-based calculations)
    /// </summary>
    /// <param name="perTargetDamages">Individual damage amounts for each target</param>
    /// <param name="targets">Characters who will get hurt (must match perTargetDamages count)</param>
    /// <param name="caster">Who is responsible for this damage</param>
    /// <remarks>
    /// Use this constructor when each target should take different damage based on their defense.
    /// The perTargetDamages list must have the same count as targets list.
    /// Each index in perTargetDamages corresponds to the same index in targets.
    /// DamageSystem will apply the specific damage value to each target.
    /// </remarks>
    public DealDamageGA(List<float> perTargetDamages, List<CombatantView> targets, CombatantView caster)
    {
        // Validate parameters before making safe copies
        if (perTargetDamages == null)
            throw new System.ArgumentNullException(nameof(perTargetDamages), "perTargetDamages cannot be null (DealDamageGA constructor)");
        if (targets == null)
            throw new System.ArgumentNullException(nameof(targets), "targets cannot be null (DealDamageGA constructor)");
        if (perTargetDamages.Count != targets.Count)
            throw new System.ArgumentException($"perTargetDamages.Count (={perTargetDamages.Count}) must match targets.Count (={targets.Count}) in DealDamageGA constructor", nameof(perTargetDamages));

        // Store per-target damages for processing
        PerTargetDamages = new(perTargetDamages);
        // Safe copy prevents outside changes to planned action
        Targets = new(targets);
        // Store who is causing this damage for perk system
        Caster = caster;
        // Amount is not used when PerTargetDamages is set; set to float.NaN as a sentinel value to indicate unused
        Amount = float.NaN;
    }
}
