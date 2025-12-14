/*
 * SKIP TURN GAME ACTION DOCUMENTATION
 * 
 * How it works:
 * This action forces a combatant to skip their next turn(s).
 * Different from stun - this is a voluntary skip (like boss resting after ultimate)
 * or a mechanical requirement rather than a crowd control effect.
 * 
 * Design reasoning:
 * Some abilities require the user to skip turns (Celestial Judgement, Sunburst Nova).
 * This is separate from stun because:
 * 1. It can't be cleansed (it's part of the ability)
 * 2. It doesn't trigger "when stunned" effects
 * 3. It's self-imposed, not enemy-imposed
 * 
 * Integration:
 * - Added as POST reaction to abilities that require resting
 * - Processed by RestingStatusEffectSystem
 * - Applies RESTING status effect instead of STUN
 * 
 * Used by:
 * - Celestial Judgement (Bathala): Rest for 1 turn after
 * - Sunburst Nova (Apolaki): Skip 1 turn after attack
 * - Shadow Dive (Bakunawa): Skip 1 turn while charging
 */

using UnityEngine;

/// <summary>
/// Game action that forces a combatant to skip their next turn(s)
/// </summary>
/// <remarks>
/// This applies the RESTING status effect, which is different from STUN.
/// Resting cannot be cleansed and doesn't trigger stun-related perks.
/// </remarks>
public class SkipTurnGA : GameAction
{
    /// <summary>
    /// The combatant who will skip their turn
    /// </summary>
    public CombatantView Target { get; private set; }
    
    /// <summary>
    /// How many turns to skip
    /// </summary>
    public int TurnsToSkip { get; private set; }
    
    /// <summary>
    /// Whether this is from charging (grants bonus on next attack)
    /// </summary>
    public bool IsCharging { get; private set; }
    
    /// <summary>
    /// Multiplier for next attack if charging (e.g., 2.0 for double damage)
    /// </summary>
    public float ChargingMultiplier { get; private set; }

    /// <summary>
    /// Creates a skip turn action for resting
    /// </summary>
    /// <param name="target">Who will skip their turn</param>
    /// <param name="turnsToSkip">How many turns to skip</param>
    public SkipTurnGA(CombatantView target, int turnsToSkip)
    {
        if (target == null)
            throw new System.ArgumentNullException(nameof(target), "Target cannot be null.");
        if (turnsToSkip < 1)
            throw new System.ArgumentOutOfRangeException(nameof(turnsToSkip), "Turns to skip must be at least 1.");
        Target = target;
        TurnsToSkip = turnsToSkip;
        IsCharging = false;
        ChargingMultiplier = 1f;
    }
    
    /// <summary>
    /// Creates a skip turn action for charging (like Shadow Dive)
    /// </summary>
    /// <param name="target">Who will skip their turn to charge</param>
    /// <param name="turnsToSkip">How many turns to charge</param>
    /// <param name="chargingMultiplier">Damage multiplier for next attack</param>
    public SkipTurnGA(CombatantView target, int turnsToSkip, float chargingMultiplier)
        : this(target, turnsToSkip)
    {
        if (chargingMultiplier <= 0f)
            throw new System.ArgumentOutOfRangeException(nameof(chargingMultiplier), "Charging multiplier must be greater than zero.");
        IsCharging = true;
        ChargingMultiplier = chargingMultiplier;
    }
}
