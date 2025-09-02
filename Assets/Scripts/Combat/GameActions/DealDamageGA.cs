using System.Collections.Generic; 
using UnityEngine;

/* DEAL DAMAGE GA DOCUMENTATION
 * 
 * Why this exists: Keeps damage planning separate from actually hurting characters
 * 
 * Design reasoning:
 * - Separates "want to damage" from "actually damage" for cleaner code organization
 * - Lets different parts of the game create damage without knowing how it works inside
 * - Allows damage changes, blocking, and other systems to step in and modify things
 * - Works the same way for hitting one target or many targets at once
 * 
 * System integration: Part of the plan-then-do pattern for turn-based combat
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
/// <para><strong>Benefits of this design:</strong></para>
/// <list type="bullet">
/// <item>Other systems can see damage coming and react before it happens</item>
/// <item>Damage planning stays separate from actually changing health</item>
/// <item>Easy to add damage-changing effects like armor, buffs, or shields</item>
/// <item>All damage works the same way whether from spells, attacks, or effects</item>
/// <item>Supports undo and replay features since damage is planned in steps</item>
/// </list>
/// 
/// <para><strong>How it flows:</strong> Plan → Wait in line → Process → Others react</para>
/// </remarks>
public class DealDamageGA : GameAction
{
    /// <summary>
    /// Starting damage amount before any changes or calculations
    /// </summary>
    /// <remarks>
    /// Raw damage number that gets handled by DamageSystem.
    /// Things like armor, buffs, or resistances get applied when processing.
    /// </remarks>
    public int Amount { get; set; }
    
    /// <summary>
    /// Who will receive the damage
    /// </summary>
    /// <remarks>
    /// Works for both hitting one target or many targets.
    /// List approach makes area effects work without special handling.
    /// </remarks>
    public List<CombatantView> Targets { get; set; }
    
    /// <summary>
    /// Creates damage action with safe copy of targets
    /// </summary>
    /// <param name="amount">Starting damage before changes</param>
    /// <param name="targets">Characters who will get hurt</param>
    /// <remarks>
    /// Safe copy prevents outside changes to the list from messing up planned actions.
    /// Important for keeping the action system working properly where planned actions can't be changed.
    /// </remarks>
    public DealDamageGA(int amount, List<CombatantView> targets)
    {
        Amount = amount;
        // Safe copy prevents outside changes to planned action
        Targets = new(targets);
    }
}
