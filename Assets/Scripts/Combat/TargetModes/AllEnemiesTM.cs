using System.Collections.Generic;
using UnityEngine;

/* ALL ENEMIES TARGET MODE DOCUMENTATION
 * 
 * Purpose: Target mode that selects all enemies currently on the battlefield
 * 
 * How it works:
 * - Gets the complete list of all enemies from EnemySystem
 * - Returns all enemies as targets for area-of-effect abilities
 * - Used for cards that affect every enemy at once
 * - Perfect for spells like "Fireball" or "Lightning Storm"
 * 
 * Integration: Inherits from TargetMode, works with EnemySystem for enemy lists
 */

/// <summary>
/// Target mode that selects all enemies on the battlefield for area-of-effect abilities
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Targets every enemy for cards that affect all enemies at once</para>
/// 
/// <para><strong>What it does:</strong> This targeting mode gets all enemies currently 
/// on the battlefield and returns them as targets. It's used for area-of-effect abilities 
/// that should hit every enemy, like damage spells, debuffs, or status effects that 
/// affect the entire enemy team.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Card effect needs targets for execution</item>
/// <item>This target mode gets called to select targets</item>
/// <item>Retrieves complete enemy list from EnemySystem</item>
/// <item>Returns all enemies as a target list</item>
/// <item>Effect gets applied to every enemy on the battlefield</item>
/// </list>
/// 
/// <para><strong>Perfect for cards like:</strong></para>
/// <list type="bullet">
/// <item>"Fireball" - deals damage to all enemies</item>
/// <item>"Poison Cloud" - applies poison to all enemies</item>
/// <item>"Freeze" - slows all enemies</item>
/// <item>"Lightning Storm" - hits every enemy with lightning</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> EnemySystem for enemy tracking, any Effects that can handle multiple targets</para>
/// 
/// <para><strong>How to use:</strong> Assign to AutoTargetEffect in CardData for area-of-effect cards</para>
/// </remarks>
public class AllEnemiesTM : TargetMode
{
    /// <summary>
    /// Returns all enemies currently on the battlefield as targets
    /// </summary>
    /// <returns>Complete list of all enemy CombatantViews for area-of-effect targeting</returns>
    /// <remarks>
    /// This method gets all enemies from the EnemySystem and returns them as targets.
    /// Creates a new list to prevent external modification of the original enemy list.
    /// If no enemies exist, returns an empty list.
    /// </remarks>
    public override List<CombatantView> GetTargets()
    {
        // Get all enemies from the enemy management system and return them as targets
        return new List<CombatantView>(EnemySystem.Instance.EnemyViews);
    }
}
