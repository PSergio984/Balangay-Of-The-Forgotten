/*
 * ARMOR STATUS EFFECT SYSTEM DOCUMENTATION
 * 
 * How it works:
 * This system implements armor as a status effect that subscribes to damage events.
 * Instead of hardcoding armor logic in the Damage() method, this system listens
 * for DealDamageGA pre-events and modifies the damage amount before it's processed.
 * This creates a cleaner separation of concerns and makes armor more modular.
 * 
 * Design reasoning:
 * Moving armor logic out of the Damage() method and into an event-based system
 * makes the code more maintainable and extensible. Other defensive status effects
 * can follow the same pattern. The damage calculation becomes more transparent
 * since all modifications happen before the actual damage application.
 * 
 * Integration:
 * - Subscribes to DealDamageGA pre-events from ActionSystem
 * - Modifies damage amount based on target's armor stacks
 * - Reduces armor stacks after absorbing damage
 * - Works with the existing status effect UI and management system
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// System that handles armor status effects by modifying incoming damage
/// </summary>
/// <remarks>
/// This system implements armor as a reactive status effect that intercepts
/// damage before it's applied. This is a cleaner approach than hardcoding
/// armor logic directly in the damage method.
/// </remarks>
public class ArmorStatusEffectSystem : MonoBehaviour
{
    /// <summary>
    /// Subscribe to damage pre-events when this system starts
    /// </summary>
    private void OnEnable()
    {
        // Subscribe to PRE events for DealDamageGA to modify damage before it's applied
        ActionSystem.SubscribeReaction<DealDamageGA>(OnDamageAboutToBeDealt, ReactionTiming.PRE);
    }

    /// <summary>
    /// Unsubscribe from damage events when this system stops
    /// </summary>
    private void OnDisable()
    {
        // Unsubscribe from damage pre-events to prevent errors
        ActionSystem.UnsubscribeReaction<DealDamageGA>(OnDamageAboutToBeDealt, ReactionTiming.PRE);
    }

    /// <summary>
    /// Modifies incoming damage based on targets' armor status effects
    /// </summary>
    /// <param name="damageAction">The damage action that's about to be processed</param>
    /// <remarks>
    /// This method is called BEFORE damage is actually applied, allowing us to
    /// modify the damage amount based on armor. For each target with armor:
    /// 1. Calculate how much damage armor can absorb
    /// 2. Reduce the damage amount accordingly
    /// 3. Remove armor stacks that were used to absorb damage
    /// 
    /// This approach keeps armor logic separate from the core damage system.
    /// </remarks>
    private void OnDamageAboutToBeDealt(DealDamageGA damageAction)
    {
        // Check each target for armor and modify damage accordingly
        for (int i = 0; i < damageAction.Targets.Count; i++)
        {
            var target = damageAction.Targets[i];
            int armorStacks = target.GetStatusEffectStacks(StatusEffectType.ARMOR);
            
            // Only process if target has armor
            if (armorStacks > 0)
            {
                int remainingDamage = damageAction.Amount;
                
                // Armor completely absorbs the damage
                if (armorStacks >= remainingDamage)
                {
                    // Remove armor stacks equal to damage absorbed
                    target.RemoveStatusEffect(StatusEffectType.ARMOR, remainingDamage);
                    // Set damage to zero since armor absorbed it all
                    damageAction.Amount = 0;
                }
                // Armor partially absorbs damage
                else
                {
                    // Remove all armor stacks
                    target.RemoveStatusEffect(StatusEffectType.ARMOR, armorStacks);
                    // Reduce damage by armor amount
                    damageAction.Amount -= armorStacks;
                }
            }
        }
    }
}
