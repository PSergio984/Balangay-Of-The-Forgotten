using UnityEngine;

using System.Collections.Generic;

/// <summary>
/// Target mode that selects all combatants (heroes and enemies) currently on the battlefield.
/// Used for effects that target everyone, such as global debuffs or resets.
/// </summary>
public class EveryoneTM : TargetMode
{
    /// <summary>
    /// Returns all combatants (heroes and enemies) currently on the battlefield as targets.
    /// </summary>
    public override List<CombatantView> GetTargets()
    {
        var all = new List<CombatantView>();
        // Add all heroes
        all.AddRange(HeroSystem.Instance.HeroViews);
        // Add all enemies
        all.AddRange(EnemySystem.Instance.EnemyViews);
        return all;
    }
}
