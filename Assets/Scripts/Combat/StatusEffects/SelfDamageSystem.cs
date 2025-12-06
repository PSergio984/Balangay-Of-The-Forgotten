/*
 * SELF DAMAGE SYSTEM DOCUMENTATION
 * 
 * How it works:
 * This system processes self-inflicted damage from ability costs.
 * Self-damage bypasses shields and defensive reactions.
 * Used for HP-cost abilities like Sacrifice and Daybreak Fury.
 * 
 * Design reasoning:
 * Self-damage is intentional and should not be blocked or reduced.
 * It's a cost, not an attack, so it doesn't trigger on-damage reactions.
 * We apply damage directly using CombatantView.Damage() for proper animation handling.
 * 
 * Integration:
 * - Attaches performer for SelfDamageGA
 * - Uses CombatantView.Damage() for proper health updates and animations
 * - Does not trigger on-damage perks or effects (it's a cost, not an attack)
 * 
 * Used by:
 * - Sacrifice (Support): Lose 200 HP
 * - Guardian's Oath (Bagani): Sacrifice 25% current HP
 * - Daybreak Fury (Apolaki): Cost 30% of current HP
 */

using UnityEngine;
using System.Collections;

/// <summary>
/// System that processes self-damage ability costs
/// </summary>
public class SelfDamageSystem : MonoBehaviour
{
    private void OnEnable()
    {
        // Register the performer for SelfDamageGA
        ActionSystem.AttachPerformer<SelfDamageGA>(SelfDamagePerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<SelfDamageGA>();
    }

    /// <summary>
    /// Applies self-damage to the target
    /// </summary>
    /// <param name="action">The self-damage action to process</param>
    /// <returns>Coroutine for action processing</returns>
    private IEnumerator SelfDamagePerformer(SelfDamageGA action)
    {
        if (action.Target == null)
        {
            Debug.LogWarning("[SelfDamageSystem] Target is null, skipping self-damage.");
            yield break;
        }
        
        int damage = action.CalculateTotalDamage();
        
        if (damage <= 0)
        {
            Debug.Log("[SelfDamageSystem] No damage to apply.");
            yield break;
        }
        
        // Apply damage using CombatantView.Damage() method
        // This properly handles animations and health updates
        // Note: Self-damage uses the same animation as regular damage for now
        // TODO: Consider adding a separate self-damage animation/VFX
        action.Target.Damage(damage);
        
        Debug.Log($"[SelfDamageSystem] {action.Target.name} takes {damage} self-damage. HP: {action.Target.CurrentHealth}/{action.Target.MaxHealth}");
        
        // Check for death
        if (action.Target.CurrentHealth <= 0)
        {
            Debug.Log($"[SelfDamageSystem] {action.Target.name} was defeated by their own ability!");
            // Death handling is done by other systems (DamageSystem checks for death)
        }
        
        yield return null;
    }
}
