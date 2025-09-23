using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// Target mode that returns the current enemy (the one whose turn it is or who performed the action).
/// Used for effects like self-buffs, recoil, or self-targeted enemy actions.
/// </summary>
public class SelfEnemyTM : TargetMode
{
    /// <summary>
    /// Returns a list containing the first enemy in EnemySystem.Instance.EnemyViews.
    /// </summary>
    /// <remarks>
    /// This method returns the first enemy in the enemy list, which works for most cases where there is only one enemy.
    /// If there are no enemies, returns an empty list.
    /// </remarks>
    public override List<CombatantView> GetTargets()
    {
        var enemies = EnemySystem.Instance.EnemyViews;
        if (enemies == null || enemies.Count == 0)
            return new List<CombatantView>();
        return new List<CombatantView> { enemies[0] };
    }
}
