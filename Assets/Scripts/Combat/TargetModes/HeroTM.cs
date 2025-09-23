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
    /// <param name="heroIndex">Optional index of the hero to target. If not provided, targets the first hero.</param>
    public List<CombatantView> GetTargets(int? heroIndex = null)
    {
        var heroes = HeroSystem.Instance.HeroViews;
        if (heroes.Count == 0)
            return new List<CombatantView>();
        if (heroIndex.HasValue && heroIndex.Value >= 0 && heroIndex.Value < heroes.Count)
            return new List<CombatantView> { heroes[heroIndex.Value] };
        // Default: target the first hero (for compatibility)
        return new List<CombatantView> { heroes[0] };
    }

    // For compatibility with base TargetMode signature
    public override List<CombatantView> GetTargets()
    {
        return GetTargets(null);
    }
}

