using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* DEVOURED STATUS EFFECT SYSTEM DOCUMENTATION
 * 
 * How it works:
 * This system handles DEVOURED status effect with separate MAG percentage and duration tracking.
 * MAG percentage determines DoT damage, duration determines how long it lasts.
 * The system applies DoT damage at turn end based on MAG percentage or fixed damage.
 * 
 * Design reasoning:
 * Devoured debuffs deal damage over time based on MAG percentage or fixed damage.
 * Percentage and duration are tracked separately for clarity.
 * Stacks represent the duration (2 stacks = 2 turns remaining).
 * Damage is calculated from MAG percentage or fixed damage each turn.
 * 
 * Integration:
 * - Performs ApplyDevouredGA actions to grant Devoured debuffs
 * - StatusEffectTickSystem calls TickDevoured() each turn for DoT and duration countdown
 * - Stacks represent duration, damage tracked separately
 * 
 * Used by:
 * - Lunar Devour (Bakunawa): 20% MAG damage (fixed at 60HP) for 2 turns
 */

/// <summary>
/// System that handles DEVOURED status effect with separate MAG percentage and duration
/// </summary>
public class DevouredSystem : MonoBehaviour
{
    /// <summary>
    /// Tracks MAG damage percentage for each combatant (key = instance ID)
    /// </summary>
    private static Dictionary<int, int> magDamagePercentages = new Dictionary<int, int>();
    
    /// <summary>
    /// Tracks fixed damage for each combatant (key = instance ID, 0 = use MAG%)
    /// </summary>
    private static Dictionary<int, int> fixedDamages = new Dictionary<int, int>();
    
    /// <summary>
    /// Tracks remaining duration for each combatant (key = instance ID)
    /// </summary>
    private static Dictionary<int, int> devouredDurations = new Dictionary<int, int>();
    
    /// <summary>
    /// Tracks the caster who applied Devoured (for MAG calculation)
    /// </summary>
    private static Dictionary<int, CombatantView> devouredCasters = new Dictionary<int, CombatantView>();
    
    /// <summary>
    /// Lookup to get combatant reference from instance ID
    /// </summary>
    private static Dictionary<int, CombatantView> combatantLookup = new Dictionary<int, CombatantView>();

    private void OnEnable()
    {
        // Register performer for ApplyDevouredGA
        ActionSystem.AttachPerformer<ApplyDevouredGA>(ApplyDevouredPerformer);
    }

    private void OnDisable()
    {
        // Unregister performer
        ActionSystem.DetachPerformer<ApplyDevouredGA>();
    }

    /// <summary>
    /// Performs ApplyDevouredGA action to grant Devoured debuffs
    /// </summary>
    private IEnumerator ApplyDevouredPerformer(ApplyDevouredGA action)
    {
        foreach (var target in action.Targets)
        {
            if (target == null) continue;
            
            int instanceId = target.GetInstanceID();
            
            // Store or update Devoured data
            magDamagePercentages[instanceId] = action.MagDamagePercentage;
            fixedDamages[instanceId] = action.FixedDamage;
            devouredDurations[instanceId] = action.Duration;
            devouredCasters[instanceId] = action.Caster;
            combatantLookup[instanceId] = target;
            
            // Apply status effect with duration as stacks (for UI display)
            target.AddStatusEffect(StatusEffectType.DEVOURED, action.Duration);
            
            string damageDesc = action.FixedDamage > 0 
                ? $"{action.FixedDamage} fixed damage" 
                : $"{action.MagDamagePercentage}% MAG";
            
            Debug.Log($"[DevouredSystem] {target.name} gains Devoured: {damageDesc} per turn for {action.Duration} turn(s)");
        }
        
        yield return null;
    }

    /// <summary>
    /// Applies DoT damage and reduces duration for a combatant
    /// Called by StatusEffectTickSystem at the end of each turn
    /// </summary>
    /// <param name="combatant">The combatant whose Devoured should tick</param>
    public static void TickDevoured(CombatantView combatant)
    {
        if (combatant == null) return;
        
        int instanceId = combatant.GetInstanceID();
        if (!devouredDurations.ContainsKey(instanceId)) return;
        
        // Calculate damage based on MAG percentage or fixed damage
        int damage = 0;
        CombatantView caster = devouredCasters.ContainsKey(instanceId) ? devouredCasters[instanceId] : null;
        
        if (fixedDamages.ContainsKey(instanceId) && fixedDamages[instanceId] > 0)
        {
            // Use fixed damage
            damage = fixedDamages[instanceId];
        }
        else if (magDamagePercentages.ContainsKey(instanceId) && caster != null)
        {
            // Calculate damage from MAG percentage
            int magPercentage = magDamagePercentages[instanceId];
            damage = Mathf.RoundToInt(caster.MagicPower * (magPercentage / 100f));
        }
        else
        {
            Debug.LogWarning($"[DevouredSystem] Cannot calculate Devoured damage for {combatant.name} - missing damage data or caster");
            // Still reduce duration even if damage calculation fails
        }
        
        // Apply DoT damage if calculated
        if (damage > 0)
        {
            DealDamageGA dotDamage = new DealDamageGA(damage, new List<CombatantView> { combatant }, null);
            ActionSystem.Instance.AddReaction(dotDamage);
            Debug.Log($"[DevouredSystem] {combatant.name} takes {damage} Devoured damage");
        }
        
        // Reduce duration
        int remainingTurns = devouredDurations[instanceId] - 1;
        
        if (remainingTurns <= 0)
        {
            // Remove expired debuff
            magDamagePercentages.Remove(instanceId);
            fixedDamages.Remove(instanceId);
            devouredDurations.Remove(instanceId);
            devouredCasters.Remove(instanceId);
            combatantLookup.Remove(instanceId);
            combatant.RemoveStatusEffect(StatusEffectType.DEVOURED, combatant.GetStatusEffectStacks(StatusEffectType.DEVOURED));
            Debug.Log($"[DevouredSystem] {combatant.name}'s Devoured expired");
        }
        else
        {
            devouredDurations[instanceId] = remainingTurns;
            // Update status effect stacks to match remaining turns
            int currentStacks = combatant.GetStatusEffectStacks(StatusEffectType.DEVOURED);
            if (currentStacks != remainingTurns)
            {
                combatant.RemoveStatusEffect(StatusEffectType.DEVOURED, currentStacks);
                combatant.AddStatusEffect(StatusEffectType.DEVOURED, remainingTurns);
            }
        }
    }

    /// <summary>
    /// Gets the Devoured damage that will be dealt next turn
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <returns>Damage amount that will be dealt, or 0 if none</returns>
    public static int GetRemainingDamage(CombatantView combatant)
    {
        if (combatant == null) return 0;
        
        int instanceId = combatant.GetInstanceID();
        if (!devouredDurations.ContainsKey(instanceId)) return 0;
        
        CombatantView caster = devouredCasters.ContainsKey(instanceId) ? devouredCasters[instanceId] : null;
        
        if (fixedDamages.ContainsKey(instanceId) && fixedDamages[instanceId] > 0)
        {
            return fixedDamages[instanceId];
        }
        else if (magDamagePercentages.ContainsKey(instanceId) && caster != null)
        {
            int magPercentage = magDamagePercentages[instanceId];
            return Mathf.RoundToInt(caster.MagicPower * (magPercentage / 100f));
        }
        
        return 0;
    }

    /// <summary>
    /// Checks if a combatant has the DEVOURED debuff
    /// </summary>
    /// <param name="combatant">The combatant to check</param>
    /// <returns>True if DEVOURED is active</returns>
    public static bool IsDevoured(CombatantView combatant)
    {
        if (combatant == null) return false;
        
        int instanceId = combatant.GetInstanceID();
        return devouredDurations.ContainsKey(instanceId) && devouredDurations[instanceId] > 0;
    }
}
