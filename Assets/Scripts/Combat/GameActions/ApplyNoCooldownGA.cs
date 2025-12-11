/*
 * APPLY NO COOLDOWN GAME ACTION DOCUMENTATION
 * 
 * How it works:
 * This action applies NO_COOLDOWN status with duration tracking.
 * While active, all skills for the affected combatants have no cooldown.
 * Duration determines how many rounds the effect lasts.
 * 
 * Design reasoning:
 * Bundok Pulag special card grants no cooldowns for 4 rounds.
 * This is a powerful team-wide buff that enables aggressive skill spam.
 * Duration tracked separately so status effect can be shown in UI.
 * 
 * Integration:
 * - Created by SpecialCardPanelUI when Bundok Pulag card is played
 * - Processed by NoCooldownSystem
 * - CooldownSystem checks for NO_COOLDOWN status before incrementing cooldowns
 * - Duration ticked down by StatusEffectTickSystem
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Game action that applies NO_COOLDOWN status to combatants
/// </summary>
public class ApplyNoCooldownGA : GameAction
{
    /// <summary>
    /// List of combatants who will receive the no cooldown buff
    /// </summary>
    public List<CombatantView> Targets { get; private set; }
    
    /// <summary>
    /// How many rounds the no cooldown effect lasts
    /// </summary>
    public int Duration { get; private set; }

    /// <summary>
    /// Creates a new no cooldown action
    /// </summary>
    /// <param name="targets">Who should receive the no cooldown buff</param>
    /// <param name="duration">How many rounds it lasts</param>
    public ApplyNoCooldownGA(List<CombatantView> targets, int duration)
    {
        if (targets == null || targets.Count == 0)
            throw new System.ArgumentException("Targets cannot be null or empty", nameof(targets));
        if (duration <= 0)
            throw new System.ArgumentException("Duration must be positive", nameof(duration));

        Targets = new List<CombatantView>(targets);
        Duration = duration;
    }
}
