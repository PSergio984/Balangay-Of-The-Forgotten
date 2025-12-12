using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Target mode that selects all living combatants (heroes and enemies) currently on the battlefield.
/// Used for effects that target everyone, such as global debuffs or resets.
/// Dead combatants are automatically filtered out.
/// </summary>
public class EveryoneTM : TargetMode
{
    /// <summary>
    /// Returns all living combatants (heroes and enemies) currently on the battlefield as targets.
    /// </summary>
    public override List<CombatantView> GetTargets()
    {
        var all = new List<CombatantView>();
        // Add all living heroes (filter out dead heroes)
        all.AddRange(HeroSystem.Instance.HeroViews.Where(h => h != null && !h.IsDead));
        // Add all living enemies (filter out dead enemies)
        all.AddRange(EnemySystem.Instance.EnemyViews.Where(e => e != null && !e.IsDead));
        return all;
    }
}
