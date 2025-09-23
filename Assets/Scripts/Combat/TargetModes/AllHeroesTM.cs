using UnityEngine;

using System.Collections.Generic;

/// <summary>
/// Target mode that selects all heroes currently in the party.
/// Used for effects that target every hero at once (e.g., party-wide buffs or heals).
/// </summary>
public class AllHeroesTM : TargetMode
{
    /// <summary>
    /// Returns all heroes currently in the party as targets.
    /// </summary>
    public override List<CombatantView> GetTargets()
    {
        // Get all heroes from the hero management system and return them as targets
        return new List<CombatantView>(HeroSystem.Instance.HeroViews);
    }
}
