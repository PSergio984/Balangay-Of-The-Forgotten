using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/* HIGHEST HP TARGET MODE DOCUMENTATION
 * 
 * Purpose: Target mode that selects the hero with the highest current HP
 * 
 * How it works:
 * - Gets all heroes from HeroSystem
 * - Finds the hero with the maximum CurrentHealth value
 * - Returns that hero as the single target
 * - Perfect for boss abilities that target the tankiest party member
 * 
 * Integration: Inherits from TargetMode, works with HeroSystem
 * 
 * Used by:
 * - Celestial Judgement (Bathala): Targets highest HP hero
 */

/// <summary>
/// Target mode that selects the hero with the highest current HP
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Picks the hero with the most HP for single-target abilities</para>
/// 
/// <para><strong>What it does:</strong> This targeting mode finds the hero with the highest 
/// current health points and selects them as the target. It's used for boss abilities that 
/// prioritize targeting the tankiest party member, like Bathala's Celestial Judgement ultimate.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Gets all heroes from HeroSystem</item>
/// <item>Uses LINQ to find the hero with maximum CurrentHealth</item>
/// <item>Returns that hero as the only target</item>
/// <item>If multiple heroes have the same max HP, returns the first one found</item>
/// </list>
/// 
/// <para><strong>Perfect for abilities like:</strong></para>
/// <list type="bullet">
/// <item>Celestial Judgement - Bathala's ultimate that targets highest HP hero</item>
/// <item>Any boss ability that prioritizes the tankiest target</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> HeroSystem for hero list, LINQ for max selection</para>
/// 
/// <para><strong>How to use:</strong> Assign to AutoTargetEffect in EnemyMoveData for highest HP targeting</para>
/// </remarks>
[System.Serializable]
public class HighestHPTargetTM : TargetMode
{
    /// <summary>
    /// Selects the hero with the highest current HP as the target
    /// </summary>
    /// <returns>List containing the hero with highest HP</returns>
    /// <remarks>
    /// This method gets all heroes and finds the one with maximum CurrentHealth.
    /// Returns a list with just that hero for consistency with other target modes.
    /// If no heroes exist, returns an empty list.
    /// </remarks>
    public override List<CombatantView> GetTargets()
    {
        var heroes = HeroSystem.Instance.HeroViews;
        
        if (heroes == null || heroes.Count == 0)
        {
            Debug.LogWarning("[HighestHPTargetTM] No heroes available to target!");
            return new List<CombatantView>();
        }
        
        // Find the hero with the highest current HP
        var highestHPHero = heroes.OrderByDescending(hero => hero.CurrentHealth).FirstOrDefault();
        
        if (highestHPHero == null)
        {
            Debug.LogWarning("[HighestHPTargetTM] Could not find a valid hero target!");
            return new List<CombatantView>();
        }
        
        Debug.Log($"[HighestHPTargetTM] Selected {highestHPHero.name} with {highestHPHero.CurrentHealth} HP as target");
        
        // Return the hero with highest HP as a single-item list
        return new List<CombatantView> { highestHPHero };
    }
}
