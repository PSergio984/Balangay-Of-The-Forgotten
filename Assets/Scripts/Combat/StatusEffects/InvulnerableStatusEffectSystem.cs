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
    /// Modifies incoming damage based on targets' invulnerable status effects
    /// </summary>
    /// <param name="damageAction">The damage action that's about to be processed</param>
    /// <remarks>
    /// This method is called BEFORE damage is actually applied, allowing us to
    /// block damage for invulnerable targets. For each target with invulnerable:
    /// 1. Check if target has invulnerable stacks
    /// 2. Block all incoming damage for that target
    /// 
    /// This approach keeps invulnerable logic separate from the core damage system.
    /// 
    /// UPDATED: Now handles both uniform damage (Amount) and per-target damage (PerTargetDamages).
    /// When per-target damage is used, only the specific target's damage is blocked.
    /// </remarks>
    private void OnDamageAboutToBeDealt(DealDamageGA damageAction)
    {
        // Check if per-target damage is being used
        // Only treat per-target damages as valid if non-null, non-empty, and count matches targets
        bool hasPerTargetDamages = false;
        if (damageAction.PerTargetDamages != null && damageAction.PerTargetDamages.Count > 0)
        {
            if (damageAction.PerTargetDamages.Count == damageAction.Targets.Count)
            {
                hasPerTargetDamages = true;
            }
            else
            {
                Debug.LogWarning($"[InvulnerableStatusEffectSystem] PerTargetDamages.Count (={damageAction.PerTargetDamages.Count}) does not match Targets.Count (={damageAction.Targets.Count}); falling back to uniform damage.");
                hasPerTargetDamages = false;
            }
        }

        // Check each target for Invulnerable and block damage if present
        for (int i = 0; i < damageAction.Targets.Count; i++)
        {
            var target = damageAction.Targets[i];
            int invulStacks = target.GetStatusEffectStacks(StatusEffectType.INVULNERABLE);

            // If target is invulnerable, block all incoming damage
            if (invulStacks > 0)
            {
                if (hasPerTargetDamages)
                {
                    // Block damage for this specific target
                    damageAction.PerTargetDamages[i] = 0f;
                }
                else
                {
                    // Block uniform damage (affects all targets)
                    damageAction.Amount = 0f;
                }
                // Optionally: Remove one stack per hit (if desired)
                // target.RemoveStatusEffect(StatusEffectType.INVULNERABLE, 1);
            }
        }
    }
}
