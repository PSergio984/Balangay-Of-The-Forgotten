/*
 * STUN EFFECT DOCUMENTATION
 * 
 * How it works:
 * This effect has a chance to stun targets for a specified duration.
 * When processed, it rolls for stun on each target up to a maximum count.
 * Successfully stunned targets receive the STUN status effect.
 * 
 * Design reasoning:
 * Stun chance is common in boss attacks (70% for Skyhammer, 50% for Thunderous).
 * We support limiting how many targets can be stunned per attack.
 * The effect handles the RNG internally so the system is clean.
 * 
 * Integration:
 * - Used by cards and enemy moves that can stun
 * - Creates ApplyStunGA for targets that fail the stun resist roll
 * - Works with StunStatusEffectSystem for turn skipping
 * 
 * Used by:
 * - Skyhammer (Bathala): 70% chance to stun 1 player for 1 turn
 * - Thunderous Decree (Bathala): 50% chance to stun 2 players for 1 turn
 * - Radiant Charge (Apolaki): 30% chance to stun 1 enemy
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Effect that has a chance to stun targets
/// </summary>
[System.Serializable]
public class StunEffect : Effects
{
    /// <summary>
    /// Chance to stun each target (0-1, e.g., 0.7 = 70%)
    /// </summary>
    [SerializeField] 
    [Tooltip("Chance to stun each target (0-1, e.g., 0.7 = 70%)")]
    [Range(0f, 1f)]
    private float stunChance = 0.5f;
    
    /// <summary>
    /// How many turns the stun lasts
    /// </summary>
    [SerializeField]
    [Tooltip("How many turns the stun lasts")]
    [Min(1)]
    private int stunDuration = 1;
    
    /// <summary>
    /// Maximum number of targets that can be stunned by this effect
    /// </summary>
    [SerializeField]
    [Tooltip("Maximum number of targets that can be stunned (0 = unlimited)")]
    [Min(0)]
    private int maxStunnedTargets = 1;

    /// <summary>
    /// Creates a stun action for targets that fail the stun roll
    /// </summary>
    /// <param name="targets">Potential targets to stun</param>
    /// <param name="caster">Who is causing the stun</param>
    /// <returns>ApplyStunGA with successfully stunned targets, or null if none stunned</returns>
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        if (targets == null || targets.Count == 0)
            return null;
        
        List<CombatantView> stunnedTargets = new List<CombatantView>();
        int maxToStun = maxStunnedTargets > 0 ? maxStunnedTargets : targets.Count;        
        
        // Shuffle targets for random selection when limited
        List<CombatantView> shuffledTargets = new List<CombatantView>(targets);
        for (int i = shuffledTargets.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = shuffledTargets[i];
            shuffledTargets[i] = shuffledTargets[j];
            shuffledTargets[j] = temp;
        }
        
        // Select up to maxToStun targets and roll each once
        int targetsToCheck = Mathf.Min(maxToStun, shuffledTargets.Count);
        for (int i = 0; i < targetsToCheck; i++)
        {
            var target = shuffledTargets[i];
            if (target == null) continue;
            
            // Roll for stun
            float roll = Random.value;
            if (roll < stunChance)
            {
                stunnedTargets.Add(target);
                Debug.Log($"[StunEffect] {target.name} STUNNED! Roll: {roll:F2} <= {stunChance:F2}");
            }
            else
            {
                Debug.Log($"[StunEffect] {target.name} resisted stun. Roll: {roll:F2} > {stunChance:F2}");
            }
        }
        
        // Return null if no targets were stunned (no action needed)
        if (stunnedTargets.Count == 0)
        {
            Debug.Log("[StunEffect] No targets were stunned.");
            return null;
        }
        
        return new ApplyStunGA(stunnedTargets, stunDuration, caster);
    }
}
