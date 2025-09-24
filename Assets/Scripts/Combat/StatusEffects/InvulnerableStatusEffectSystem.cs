using System.Collections.Generic;
using UnityEngine;

public class InvulnerableStatusEffectSystem : MonoBehaviour
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
        // Check each target for Invulnerable and block damage if present
        for (int i = 0; i < damageAction.Targets.Count; i++)
        {
            var target = damageAction.Targets[i];
            int invulStacks = target.GetStatusEffectStacks(StatusEffectType.INVULNERABLE);
            // If target is invulnerable, block all incoming damage
            if (invulStacks > 0)
            {
                damageAction.Amount = 0;
                // Optionally: Remove one stack per hit (if desired)
                // target.RemoveStatusEffect(StatusEffectType.INVULNERABLE, 1);
            }
        }
    }
}
