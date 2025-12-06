/*
 * REMOVE DEBUFF EFFECT DOCUMENTATION
 * 
 * How it works:
 * This effect removes debuffs from targets.
 * Can remove all debuffs or a specific debuff type.
 * Useful for cleanse/purify abilities.
 * 
 * Design reasoning:
 * Debuff removal is a core support mechanic.
 * Some abilities remove all debuffs (cleanse), others remove specific ones.
 * The effect is flexible enough to handle both cases.
 * 
 * Integration:
 * - Used by cleanse and purify abilities
 * - Creates RemoveDebuffsGA for processing by BuffDebuffSystem
 * - Works with the standard status effect tracking
 * 
 * Used by:
 * - Blessing of the Sun (Apolaki): Remove all debuffs from allies
 * - Cleansing Light (Support): Remove debuffs from target
 * - Purify (Support): Remove specific debuff
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Effect that removes debuffs from targets
/// </summary>
[System.Serializable]
public class RemoveDebuffEffect : Effects
{
    /// <summary>
    /// Whether to remove ALL debuffs (true) or just a specific one (false)
    /// </summary>
    [SerializeField]
    [Tooltip("Remove all debuffs? If false, only removes the specific debuff below.")]
    private bool removeAllDebuffs = true;
    
    /// <summary>
    /// Specific debuff to remove (only used if removeAllDebuffs is false)
    /// </summary>
    [SerializeField]
    [Tooltip("Which specific debuff to remove (ignored if Remove All Debuffs is true)")]
    private StatusEffectType specificDebuff = StatusEffectType.BURN;
    
    /// <summary>
    /// Whether to include the caster in the cleanse
    /// </summary>
    [SerializeField]
    [Tooltip("Include caster in the cleanse (for self-cleanse abilities)")]
    private bool includeCaster = true;

    /// <summary>
    /// Creates a debuff removal action for the specified targets
    /// </summary>
    /// <param name="targets">Who should have debuffs removed</param>
    /// <param name="caster">Who is casting the cleanse</param>
    /// <returns>RemoveDebuffsGA for processing</returns>
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        if (targets == null)
        {
            Debug.LogWarning("[RemoveDebuffEffect] Targets list is null.");
            return null;
        }
        
        List<CombatantView> validTargets = new List<CombatantView>();
        
        foreach (var target in targets)
        {
            if (target == null) continue;
            if (!includeCaster && target == caster) continue;
            validTargets.Add(target);
        }        
        if (validTargets.Count == 0)
        {
            Debug.LogWarning("[RemoveDebuffEffect] No valid targets for debuff removal.");
            return null;
        }
        
        if (removeAllDebuffs)
        {
            Debug.Log($"[RemoveDebuffEffect] Removing all debuffs from {validTargets.Count} target(s)");
            return new RemoveDebuffsGA(validTargets);
        }
        else
        {
            // Check if the specific debuff is actually a debuff type
            if (!RemoveDebuffsGA.IsDebuff(specificDebuff))
            {
                Debug.LogWarning($"[RemoveDebuffEffect] {specificDebuff} is not a debuff, nothing to remove.");
                return null;
            }
            
            Debug.Log($"[RemoveDebuffEffect] Removing {specificDebuff} from {validTargets.Count} target(s)");
            return new RemoveDebuffsGA(validTargets, specificDebuff);
        }
    }
}