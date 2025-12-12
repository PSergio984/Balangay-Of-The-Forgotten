using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Target mode that selects all living heroes currently in the party.
/// Used for effects that target every hero at once (e.g., party-wide buffs or heals).
/// Dead heroes are automatically filtered out.
/// </summary>
public class AllHeroesTM : TargetMode
{
    /// <summary>
    /// Returns all living heroes currently in the party as targets.
    /// </summary>
    public override List<CombatantView> GetTargets()
    {
        // Get all living heroes from the hero management system (filter out dead heroes)
        var allHeroes = HeroSystem.Instance.HeroViews;
        return allHeroes.Where(h => h != null && !h.IsDead).ToList<CombatantView>();
    }
}
