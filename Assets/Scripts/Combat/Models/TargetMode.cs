using System.Collections.Generic;
using UnityEngine;

/* TARGET MODE DOCUMENTATION
 * 
 * Purpose: Base class for all targeting behaviors that determine who gets affected by effects
 * 
 * How it works:
 * - Abstract base class that defines how targets are selected
 * - Each specific target mode implements GetTargets() differently
 * - Used by AutoTargetEffect to automatically choose targets for card effects
 * - Allows flexible targeting without hardcoding target selection in effects
 * 
 * Integration: Inherited by specific target modes, used by AutoTargetEffect and card system
 */

/// <summary>
/// Base class for all targeting behaviors that determine who gets affected by card effects
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Provides common structure for all target selection methods</para>
/// 
/// <para><strong>What it does:</strong> This abstract class defines how different targeting 
/// modes work. Each targeting mode (like "all enemies", "random target", "no targets") 
/// inherits from this class and implements its own way of selecting targets. This allows 
/// card effects to work with any targeting behavior without knowing the specific details.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Specific target modes inherit from this base class</item>
/// <item>Each mode implements GetTargets() to return different target lists</item>
/// <item>Card effects use target modes to find who to affect</item>
/// <item>Target selection happens automatically based on the mode type</item>
/// </list>
/// 
/// <para><strong>Examples of implementations:</strong></para>
/// <list type="bullet">
/// <item>AllEnemiesTM - targets all enemies on the battlefield</item>
/// <item>RandomTargetTM - picks one random enemy</item>
/// <item>NoTM - returns no targets (for effects that don't need targets)</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> AutoTargetEffect for target selection, Effects for execution</para>
/// 
/// <para><strong>How to use:</strong> Inherit from this class and implement GetTargets() for new targeting behaviors</para>
/// </remarks>
[System.Serializable]
public abstract class TargetMode
{
    /// <summary>
    /// Gets the list of targets that should be affected by an effect using this targeting mode
    /// </summary>
    /// <returns>List of CombatantView objects that represent the selected targets</returns>
    /// <remarks>
    /// This abstract method must be implemented by all target mode types.
    /// It defines how the specific targeting behavior selects who gets affected.
    /// Returns null if no targets are needed (like for card draw effects).
    /// </remarks>
    public abstract List<CombatantView> GetTargets();
}