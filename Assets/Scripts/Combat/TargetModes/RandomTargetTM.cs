using System.Collections.Generic;
using UnityEngine;

/* RANDOM TARGET MODE DOCUMENTATION
 * 
 * Purpose: Target mode that randomly selects one hero from all available heroes
 * 
 * How it works:
 * - Gets all heroes from HeroSystem
 * - Uses Random.Range to pick one hero randomly
 * - Returns single random hero as target for single-target abilities
 * - Perfect for cards that hit one random party member
 * 
 * Integration: Inherits from TargetMode, works with HeroSystem and Unity's Random system
 */

/// <summary>
/// Target mode that randomly selects one hero for single-target abilities with random targeting
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Picks one random hero for cards that hit a single random party member</para>
/// 
/// <para><strong>What it does:</strong> This targeting mode randomly selects one hero 
/// from all heroes currently in the party. It's used for single-target abilities 
/// where the player doesn't choose the target - instead, the game picks one randomly. 
/// This adds an element of chance and unpredictability to certain cards.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Card effect needs a single random target</item>
/// <item>This target mode gets all heroes from HeroSystem</item>
/// <item>Uses Unity's Random.Range to pick a random index</item>
/// <item>Returns the randomly selected hero as the only target</item>
/// <item>Effect gets applied to that one lucky (or unlucky) hero</item>
/// </list>
/// 
/// <para><strong>Perfect for cards like:</strong></para>
/// <list type="bullet">
/// <item>"Chaos Bolt" - strikes one random party member</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> HeroSystem for hero list, Unity Random for selection</para>
/// 
/// <para><strong>How to use:</strong> Assign to AutoTargetEffect in CardData for random single-target cards</para>
/// </remarks>
public class RandomTargetTM : TargetMode
{
   /// <summary>
   /// Randomly selects one hero from all available heroes as the target
   /// </summary>
   /// <returns>List containing one randomly selected hero CombatantView</returns>
   /// <remarks>
   /// This method gets all heroes and uses Random.Range to pick one randomly.
   /// Returns a list with just the selected hero for consistency with other target modes.
   /// If no heroes exist, this could cause an error, so ensure heroes are present before use.
   /// </remarks>
   public override List<CombatantView> GetTargets()
   {
       // Pick one random hero from all available heroes using random index
       CombatantView target = HeroSystem.Instance.HeroViews[Random.Range(0, CurrentHeroUtil.GetHeroCount())];
       // Return the randomly selected hero as a single-item list
       return new() { target };
   }
}
