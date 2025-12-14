/*
 * SKIP TURN EFFECT DOCUMENTATION
 * 
 * How it works:
 * This effect forces the caster to skip their next turn(s).
 * Used as a cost or consequence of powerful abilities.
 * Can optionally be a "charging" state that buffs the next attack.
 * 
 * Design reasoning:
 * Many powerful abilities have a "rest" period afterwards.
 * Celestial Judgement requires Bathala to rest 1 turn.
 * Shadow Dive is a charging state that doubles the next attack.
 * 
 * Integration:
 * - Typically added as a POST reaction to powerful abilities
 * - Creates SkipTurnGA for processing by RestingStatusEffectSystem
 * - Applies RESTING or CHARGING status effect
 * 
 * Used by:
 * - Celestial Judgement (Bathala): Rest 1 turn after
 * - Sunburst Nova (Apolaki): Skip 1 turn after
 * - Shadow Dive (Bakunawa): Skip 1 turn, next attack deals 2x damage
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Effect that forces the caster to skip turns (rest or charge)
/// </summary>
[System.Serializable]
public class SkipTurnEffect : Effects
{
    /// <summary>
    /// How many turns to skip
    /// </summary>
    [SerializeField]
    [Tooltip("How many turns to skip")]
    [Min(1)]
    private int turnsToSkip = 1;
    
    /// <summary>
    /// Whether this is a charging state (buffs next attack)
    /// </summary>
    [SerializeField]
    [Tooltip("Is this a charging state? (Buffs next attack)")]
    private bool isCharging = false;
    
    /// <summary>
    /// Damage multiplier for next attack if charging
    /// </summary>
    [SerializeField]
    [Tooltip("Damage multiplier for next attack when charging (2.0 = double damage)")]
    [Range(1f, 5f)]
    private float chargingMultiplier = 2f;

    /// <summary>
    /// Creates a skip turn action for the caster
    /// </summary>
    /// <param name="targets">Ignored - always affects caster</param>
    /// <param name="caster">Who will skip their turn</param>
    /// <returns>SkipTurnGA for processing</returns>
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        if (caster == null)
        {
            Debug.LogWarning("[SkipTurnEffect] Caster is null, cannot skip turn.");
            return null;
        }
        
        if (isCharging)
        {
            Debug.Log($"[SkipTurnEffect] {caster.name} is charging for {turnsToSkip} turn(s), next attack: {chargingMultiplier}x damage");
            return new SkipTurnGA(caster, turnsToSkip, chargingMultiplier);
        }
        else
        {
            Debug.Log($"[SkipTurnEffect] {caster.name} will rest for {turnsToSkip} turn(s)");
            return new SkipTurnGA(caster, turnsToSkip);
        }
    }
}
