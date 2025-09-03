
/* EFFECTS BASE CLASS DOCUMENTATION
 * 
 * Purpose: Base class for all card effects and abilities in the game
 * 
 * How it works:
 * - Abstract base class that all specific effects inherit from
 * - Forces each effect type to implement GetGameAction() method
 * - Allows effects to be stored in lists and processed uniformly
 * - NOW SUPPORTS CASTER TRACKING: Effects can know who caused them for perk system
 * 
 * Integration: Used by EffectSystem, Card system, Perk system, and all specific effect implementations
 * 
 * NEW PERK SYSTEM INTEGRATION:
 * The caster parameter in GetGameAction lets perks track who did what action.
 * This is crucial for reactive perks that need to target the source of damage/effects.
 */

using System.Collections.Generic;

/// <summary>
/// Base class for all card effects that can be performed in the game
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Provides common structure for all card effects and abilities</para>
/// 
/// <para><strong>What it does:</strong> This abstract class serves as the foundation 
/// for all card effects like damage, healing, card draw, etc. It ensures that 
/// every effect can be converted into a GameAction that the action system 
/// can process.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Specific effects inherit from this base class</item>
/// <item>Each effect implements GetGameAction() to define what it does</item>
/// <item>EffectSystem calls GetGameAction() to convert effects to actions</item>
/// <item>Actions get processed by appropriate game systems</item>
/// </list>
/// 
/// <para><strong>Needs:</strong> Concrete implementations must override GetGameAction()</para>
/// 
/// <para><strong>Works with:</strong> EffectSystem for processing, Card for storage, ActionSystem for execution</para>
/// 
/// <para><strong>How to use:</strong> Inherit from this class and implement GetGameAction() for new effects</para>
/// </remarks>
[System.Serializable]
public abstract class Effects
{
    /// <summary>
    /// Converts this effect into a concrete game action that can be executed
    /// </summary>
    /// <param name="targets">Who this effect should target</param>
    /// <param name="caster">Who caused this effect (for perk system tracking)</param>
    /// <returns>A GameAction that represents what this effect does</returns>
    /// <remarks>
    /// This abstract method must be implemented by all effect types.
    /// It defines how the effect translates into an action that game systems can process.
    /// For example, a damage effect would return a DealDamageGA action.
    /// 
    /// PERK SYSTEM INTEGRATION:
    /// The caster parameter is the key new addition for perks. It lets effects track
    /// who caused them to happen. This is essential for reactive perks that need to
    /// know "who dealt damage to me" or "who I just attacked" for proper targeting.
    /// </remarks>
    public abstract GameAction GetGameAction(List<CombatantView> targets,CombatantView caster);
}
