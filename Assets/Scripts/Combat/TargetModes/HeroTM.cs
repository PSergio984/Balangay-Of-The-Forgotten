/*
 * HERO TARGET MODE DOCUMENTATION
 * 
 * How it works:
 * This targeting mode always targets the player's hero character. When cards or
 * perks use this targeting mode, they will affect the player no matter what.
 * Simple targeting that always returns the hero as the only target.
 * 
 * Design reasoning:
 * We need a simple way for effects to target the player specifically. Self-buffs,
 * healing cards, and protective perks often need to target just the hero. This
 * targeting mode makes that easy and consistent across all systems.
 * 
 * Integration:
 * - Used by cards and perks that need to target the player specifically
 * - Always returns the hero from HeroSystem for consistent targeting
 * - Simple implementation of the TargetMode pattern for hero-specific effects
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Targeting mode that always targets the player's hero character
/// </summary>
public class HeroTM : TargetMode
{
    /// <summary>
    /// Returns a list containing the targeted hero (by index) or all heroes if no index is specified
    /// </summary>

        public override List<CombatantView> GetTargets()
        {
            // Create a list with just the hero as the target
            List<CombatantView> targets = new()
            {
                CurrentHeroUtil.GetCurrentHero()
            };
            return targets;
        }
}

