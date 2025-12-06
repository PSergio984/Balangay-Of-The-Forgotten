/*
 * BUFF DEBUFF SYSTEM DOCUMENTATION
 * 
 * How it works:
 * This system handles buff/debuff removal and management.
 * Processes RemoveDebuffsGA to cleanse debuffs from targets.
 * Provides utility methods for buff/debuff checking.
 * 
 * Design reasoning:
 * Debuff removal is a core support mechanic.
 * We need a central place to define what counts as a debuff.
 * This system works with RemoveDebuffsGA which has the list of debuff types.
 * 
 * Integration:
 * - Attaches performer for RemoveDebuffsGA
 * - Uses StatusEffectType categorization from RemoveDebuffsGA
 * - Works with CombatantView's status effect tracking
 * 
 * Used by:
 * - Blessing of the Sun (Apolaki): Remove all debuffs from allies
 * - Cleansing Light (Support): Remove debuffs from target
 * - Purify (Support): Remove specific debuff
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// System that manages buff/debuff removal and tracking
/// </summary>
public class BuffDebuffSystem : MonoBehaviour
{
    /// <summary>
    /// Array of status effect types that are considered buffs (positive effects)
    /// </summary>
    public static readonly StatusEffectType[] BuffTypes = new StatusEffectType[]
    {
        // Defensive Buffs
        StatusEffectType.ARMOR,
        StatusEffectType.DEFENSE_UP,
        StatusEffectType.INVULNERABLE,
        StatusEffectType.SHIELD,
        StatusEffectType.TEMP_HP,
        
        // Offensive Buffs
        StatusEffectType.ATTACK_UP,
        StatusEffectType.CRIT_UP,
        StatusEffectType.DMG_UP,
        StatusEffectType.IGNORE_DEFENSE,
        StatusEffectType.HIT_UP,
        StatusEffectType.RAGE,
        StatusEffectType.FOCUSED,
        StatusEffectType.BLESSED,
    };

    private void OnEnable()
    {
        // Register the performer for RemoveDebuffsGA
        ActionSystem.AttachPerformer<RemoveDebuffsGA>(RemoveDebuffPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<RemoveDebuffsGA>();
    }

    /// <summary>
    /// Removes debuffs from all targets
    /// </summary>
    /// <param name="action">The remove debuff action to process</param>
    /// <returns>Coroutine for action processing</returns>
    private IEnumerator RemoveDebuffPerformer(RemoveDebuffsGA action)
    {
        foreach (var target in action.Targets)
        {
            if (target == null) continue;
            
            if (action.RemoveAll)
            {
                // Remove ALL debuffs
                int totalRemoved = 0;
                
                foreach (var debuffType in RemoveDebuffsGA.DebuffTypes)
                {
                    int stacks = target.GetStatusEffectStacks(debuffType);
                    if (stacks > 0)
                    {
                        target.RemoveStatusEffect(debuffType, stacks);
                        totalRemoved++;
                        Debug.Log($"[BuffDebuffSystem] Removed {stacks} stacks of {debuffType} from {target.name}");
                    }
                }
                
                if (totalRemoved > 0)
                {
                    Debug.Log($"[BuffDebuffSystem] {target.name} cleansed of {totalRemoved} debuff type(s)!");
                    // TODO: Play cleanse VFX
                }
                else
                {
                    Debug.Log($"[BuffDebuffSystem] {target.name} had no debuffs to remove.");
                }
            }
            else
            {
                // Remove specific debuff
                if (action.SpecificDebuff.HasValue)
                {
                    int stacks = target.GetStatusEffectStacks(action.SpecificDebuff.Value);
                    if (stacks > 0)
                    {
                        target.RemoveStatusEffect(action.SpecificDebuff.Value, stacks);
                        Debug.Log($"[BuffDebuffSystem] Removed {stacks} stacks of {action.SpecificDebuff.Value} from {target.name}");
                        // TODO: Play cleanse VFX
                    }
                    else
                    {
                        Debug.Log($"[BuffDebuffSystem] {target.name} didn't have {action.SpecificDebuff.Value}.");
                    }
                }
                else
                {
                    Debug.LogWarning($"[BuffDebuffSystem] RemoveDebuffsGA called with RemoveAll=false and SpecificDebuff=null for target {target.name}. No debuff type specified.");
                }
            }
            
            yield return null;
        }
    }

    /// <summary>
    /// Checks if a status effect type is a debuff
    /// </summary>
    /// <param name="effectType">The status effect to check</param>
    /// <returns>True if this is a debuff</returns>
    public static bool IsDebuff(StatusEffectType effectType)
    {
        foreach (var debuffType in RemoveDebuffsGA.DebuffTypes)
        {
            if (effectType == debuffType) return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if a status effect type is a buff
    /// </summary>
    /// <param name="effectType">The status effect to check</param>
    /// <returns>True if this is a buff (positive effect)</returns>
    public static bool IsBuff(StatusEffectType effectType)
    {
        foreach (var buffType in BuffTypes)
        {
            if (effectType == buffType) return true;
        }
        return false;
    }

    /// <summary>
    /// Gets all active debuffs on a combatant
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <returns>Dictionary of debuff types and their stack counts</returns>
    public static Dictionary<StatusEffectType, int> GetActiveDebuffs(CombatantView combatant)
    {
        var activeDebuffs = new Dictionary<StatusEffectType, int>();
        
        if (combatant == null) return activeDebuffs;
        
        foreach (var debuffType in RemoveDebuffsGA.DebuffTypes)
        {
            int stacks = combatant.GetStatusEffectStacks(debuffType);
            if (stacks > 0)
            {
                activeDebuffs[debuffType] = stacks;
            }
        }
        
        return activeDebuffs;
    }

    /// <summary>
    /// Checks if a combatant has any debuffs
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <returns>True if the combatant has at least one debuff</returns>
    public static bool HasAnyDebuff(CombatantView combatant)
    {
        if (combatant == null) return false;
        
        foreach (var debuffType in RemoveDebuffsGA.DebuffTypes)
        {
            if (combatant.GetStatusEffectStacks(debuffType) > 0) return true;
        }
        
        return false;
    }

    /// <summary>
    /// Gets the total count of debuff stacks on a combatant
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <returns>Total number of debuff stacks</returns>
    public static int GetTotalDebuffStacks(CombatantView combatant)
    {
        if (combatant == null) return 0;
        
        int total = 0;
        foreach (var debuffType in RemoveDebuffsGA.DebuffTypes)
        {
            total += combatant.GetStatusEffectStacks(debuffType);
        }
        
        return total;
    }
}
