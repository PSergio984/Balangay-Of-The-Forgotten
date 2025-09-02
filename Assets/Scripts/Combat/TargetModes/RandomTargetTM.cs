using System.Collections.Generic;
using UnityEngine;

/* RANDOM TARGET MODE DOCUMENTATION
 * 
 * Purpose: Target mode that randomly selects one enemy from all available enemies
 * 
 * How it works:
 * - Gets all enemies from EnemySystem
 * - Uses Random.Range to pick one enemy randomly
 * - Returns single random enemy as target for single-target abilities
 * - Perfect for cards that hit one random target
 * 
 * Integration: Inherits from TargetMode, works with EnemySystem and Unity's Random system
 */

/// <summary>
/// Target mode that randomly selects one enemy for single-target abilities with random targeting
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Picks one random enemy for cards that hit a single random target</para>
/// 
/// <para><strong>What it does:</strong> This targeting mode randomly selects one enemy 
/// from all enemies currently on the battlefield. It's used for single-target abilities 
/// where the player doesn't choose the target - instead, the game picks one randomly. 
/// This adds an element of chance and unpredictability to certain cards.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Card effect needs a single random target</item>
/// <item>This target mode gets all enemies from EnemySystem</item>
/// <item>Uses Unity's Random.Range to pick a random index</item>
/// <item>Returns the randomly selected enemy as the only target</item>
/// <item>Effect gets applied to that one lucky (or unlucky) enemy</item>
/// </list>
/// 
/// <para><strong>Perfect for cards like:</strong></para>
/// <list type="bullet">
/// <item>"Magic Missile" - hits one random enemy</item>
/// <item>"Curse" - applies a random debuff to one enemy</item>
/// <item>"Lightning Bolt" - strikes one random target</item>
/// <item>"Charm" - mind controls a random enemy</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> EnemySystem for enemy list, Unity Random for selection</para>
/// 
/// <para><strong>How to use:</strong> Assign to AutoTargetEffect in CardData for random single-target cards</para>
/// </remarks>
public class RandomTargetTM : TargetMode
{
   /// <summary>
   /// Randomly selects one enemy from all available enemies as the target
   /// </summary>
   /// <returns>List containing one randomly selected enemy CombatantView</returns>
   /// <remarks>
   /// This method gets all enemies and uses Random.Range to pick one randomly.
   /// Returns a list with just the selected enemy for consistency with other target modes.
   /// If no enemies exist, this could cause an error, so ensure enemies are present before use.
   /// </remarks>
   public override List<CombatantView> GetTargets()
   {
       // Pick one random enemy from all available enemies using random index
       CombatantView target = EnemySystem.Instance.EnemyViews[Random.Range(0, EnemySystem.Instance.EnemyViews.Count)];
       // Return the randomly selected enemy as a single-item list
       return new() { target };
   }
}
