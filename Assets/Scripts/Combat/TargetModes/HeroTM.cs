/*
 * HERO TARGET MODE DOCUMENTATION
 * 
 * How it works:
 * This targeting mode always targets the player's current hero character. When cards or
 * perks use this targeting mode, they will affect the current player hero.
 * If the current hero is dead, returns an empty list (dead heroes cannot be targeted).
 * 
 * Design reasoning:
 * We need a simple way for effects to target the player specifically. Self-buffs,
 * healing cards, and protective perks often need to target just the hero. This
 * targeting mode makes that easy and consistent across all systems.
 * 
 * Integration:
 * - Used by cards and perks that need to target the player specifically
 * - Always returns the hero from CurrentHeroUtil for consistent targeting
 * - Simple implementation of the TargetMode pattern for hero-specific effects
 * - Dead heroes are filtered out and cannot be targeted
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Targeting mode that always targets the player's current hero character (if alive)
/// </summary>
public class HeroTM : TargetMode
{
    /// <summary>
    /// Returns a list containing the current hero (if alive), or empty list if dead
    /// </summary>
    public override List<CombatantView> GetTargets()
    {
        var currentHero = CurrentHeroUtil.GetCurrentHero();
        
        // Return empty list if hero is null or dead
        if (currentHero == null || currentHero.IsDead)
        {
            return new List<CombatantView>();
        }
        
        // Create a list with just the living hero as the target
        return new List<CombatantView> { currentHero };
    }
}

