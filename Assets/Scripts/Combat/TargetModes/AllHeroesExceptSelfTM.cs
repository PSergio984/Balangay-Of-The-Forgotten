using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/* ALL HEROES EXCEPT SELF TARGET MODE DOCUMENTATION
 * 
 * Purpose: Target mode that selects all living heroes except the caster
 * 
 * How it works:
 * - Gets all living heroes from HeroSystem
 * - Filters out the current hero (caster)
 * - Used for effects like "heal all allies except self" or "buff all allies"
 * 
 * Design reasoning:
 * Some hero abilities need to affect all allies but not themselves (sacrifice, buff others, etc.)
 * This provides a simple way to target all other heroes without manual selection.
 * 
 * Integration: 
 * - Inherits from TargetMode
 * - Uses HeroSystem for hero list
 * - Uses CurrentHeroUtil to get the caster (current hero)
 * 
 * Used by:
 * - Sacrifice: Heal all allies except self
 * - Guardian's Oath: Shield all allies except self
 */

/// <summary>
/// Target mode that selects all living heroes except the current hero (caster)
/// </summary>
public class AllHeroesExceptSelfTM : TargetMode
{
    /// <summary>
    /// Returns all living heroes except the current hero
    /// </summary>
    /// <returns>List of all living heroes excluding the caster</returns>
    public override List<CombatantView> GetTargets()
    {
        var currentHero = CurrentHeroUtil.GetCurrentHero();
        
        // Get all living heroes and filter out the current hero
        var allHeroes = HeroSystem.Instance.HeroViews;
        var allies = allHeroes.Where(h => h != null && !h.IsDead && h != currentHero).ToList<CombatantView>();
        
        return allies;
    }
}

