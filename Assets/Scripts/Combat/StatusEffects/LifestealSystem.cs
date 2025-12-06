/*
 * LIFESTEAL SYSTEM DOCUMENTATION
 * 
 * How it works:
 * This system processes lifesteal damage actions.
 * Deals damage to a target and heals the caster.
 * Healing can be flat, magic-scaled, or percentage of damage dealt.
 * 
 * Design reasoning:
 * Lifesteal is a sustain mechanic that rewards dealing damage.
 * The damage goes through normal pipeline (armor applies via DealDamageGA).
 * The heal creates a HealGA for proper processing.
 * 
 * Integration:
 * - Attaches performer for LifestealDamageGA
 * - Creates DealDamageGA for the damage (integrates with damage systems)
 * - Creates HealGA for the heal (integrates with heal systems)
 * 
 * Used by:
 * - Eclipse Fang (Bakunawa): Heals for 50 (+100% MAG), deals 110% MAG damage
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// System that processes lifesteal damage and healing
/// </summary>
public class LifestealSystem : MonoBehaviour
{
    private void OnEnable()
    {
        // Register the performer for LifestealDamageGA
        ActionSystem.AttachPerformer<LifestealDamageGA>(LifestealPerformer);
    }

    private void OnDisable()
    {
        ActionSystem.DetachPerformer<LifestealDamageGA>();
    }

    /// <summary>
    /// Processes lifesteal: deals damage then heals caster
    /// </summary>
    /// <param name="action">The lifesteal action to process</param>
    /// <returns>Coroutine for action processing</returns>
    private IEnumerator LifestealPerformer(LifestealDamageGA action)
    {
        if (action.Target == null)
        {
            Debug.LogWarning("[LifestealSystem] Target is null, skipping lifesteal.");
            yield break;
        }
        
        if (action.Caster == null)
        {
            Debug.LogWarning("[LifestealSystem] Caster is null, skipping lifesteal.");
            yield break;
        }
        
        // Calculate final damage amount
        int damageAmount = Mathf.RoundToInt(action.DamageAmount);
        
        // Store target's health before damage for actual damage calculation
        int healthBefore = action.Target.CurrentHealth;
        
        // Create a DealDamageGA to properly process damage through the action system
        // This ensures all damage modifiers, shields, armor, etc. are applied correctly
        var damageAction = new DealDamageGA(
            amount: damageAmount,
            targets: new List<CombatantView> { action.Target },
            caster: action.Caster
        );
        
        // Flag to track completion
        bool damageComplete = false;
        float startTime = Time.realtimeSinceStartup;
        float maxDuration = 5f; // seconds

        // Execute damage through the ActionSystem with a callback
        ActionSystem.Instance.Perform(damageAction, () =>
        {
            damageComplete = true;
        });

        // Wait for damage action to complete, with timeout
        while (!damageComplete && (Time.realtimeSinceStartup - startTime) < maxDuration)
        {
            yield return null;
        }

        if (!damageComplete)
        {
            Debug.LogError($"[LifestealSystem] Damage action did not complete within {maxDuration} seconds. Proceeding with cleanup.");
            // Optionally: set actualDamageDealt to 0 or handle as needed
        }
        
        // Calculate actual damage dealt by comparing health before and after
        // This accounts for shields, armor, and other damage reduction
        int healthAfter = 0;
        string targetName = "<dead>";
        if (action.Target != null)
        {
            healthAfter = action.Target.CurrentHealth;
            targetName = action.Target.name;
        }
        int actualDamageDealt = Mathf.Max(0, healthBefore - healthAfter);

        Debug.Log($"[LifestealSystem] {action.Caster.name} dealt {actualDamageDealt} actual damage to {targetName}");
        
        // Calculate heal amount based on actual damage dealt
        float healAmount = action.FlatHeal;
        healAmount += action.HealMagicAmp * action.Caster.MagicPower;
        healAmount += action.LifestealPercent * actualDamageDealt;
        
        int finalHeal = Mathf.RoundToInt(healAmount);
        
        if (finalHeal > 0)
        {
            // Create a HealGA to properly process the healing through the action system
            var healTargets = new List<HealTarget> { new HealTarget(action.Caster, finalHeal) };
            var healAction = new HealGA(healTargets, action.Caster);
            
            // Add heal as a reaction to process after damage
            ActionSystem.Instance.AddReaction(healAction);
            
            Debug.Log($"[LifestealSystem] {action.Caster.name} will heal for {finalHeal}");
        }
        
        // TODO: Play lifesteal VFX (damage + heal combined effect)
        
        yield return null;
    }
}
