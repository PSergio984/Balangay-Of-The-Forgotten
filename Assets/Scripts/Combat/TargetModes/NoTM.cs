using System.Collections.Generic;
using UnityEngine;

/* NO TARGET MODE DOCUMENTATION
 * 
 * Purpose: Target mode for effects that don't need any targets to function
 * 
 * How it works:
 * - Returns null instead of a target list
 * - Used for effects that work without needing specific targets
 * - Perfect for self-effects, global effects, or utility cards
 * - Examples: card draw, stamina gain, self-buffs, board changes
 * 
 * Integration: Inherits from TargetMode, works with any Effects that don't require targets
 */

/// <summary>
/// Target mode for effects that don't require any targets to function properly
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Handles effects that work without needing specific targets</para>
/// 
/// <para><strong>What it does:</strong> This targeting mode returns no targets (null) 
/// because some card effects don't need to target anyone. These are typically 
/// utility effects, self-buffs, card manipulation, or global effects that 
/// affect the game state without targeting specific characters.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Card effect requests targets for execution</item>
/// <item>This target mode returns null (no targets needed)</item>
/// <item>Effect processes without any target restrictions</item>
/// <item>Effect applies its changes globally or to the player</item>
/// </list>
/// 
/// <para><strong>Perfect for cards like:</strong></para>
/// <list type="bullet">
/// <item>"Draw Cards" - draws cards from deck (no targets needed)</item>
/// <item>"Gain Stamina" - restores player's stamina</item>
/// <item>"Self Heal" - heals the player character</item>
/// <item>"Time Warp" - affects turn order globally</item>
/// <item>"Meditation" - provides player buffs</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> Any Effects that can handle null target lists</para>
/// 
/// <para><strong>How to use:</strong> Assign to AutoTargetEffect in CardData for utility/self-effect cards</para>
/// </remarks>
public class NoTM : TargetMode
{
    /// <summary>
    /// Returns null because this target mode is for effects that don't need any targets
    /// </summary>
    /// <returns>null - no targets are selected or needed</returns>
    /// <remarks>
    /// This method always returns null because effects using this target mode
    /// don't need specific targets to function. The effect will handle the null
    /// target list appropriately and apply its changes without targeting restrictions.
    /// </remarks>
    public override List<CombatantView> GetTargets()
    {
        // Return null because no targets are needed for this type of effect
        return null;
    }
}
