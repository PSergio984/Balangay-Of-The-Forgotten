/*
 * LIFESTEAL EFFECT DOCUMENTATION
 * 
 * How it works:
 * This effect deals damage to a target and heals the caster.
 * Healing can be flat, magic-scaled, or a percentage of damage dealt.
 * The heal is calculated after damage is applied (for true lifesteal).
 * 
 * Design reasoning:
 * Lifesteal is a sustain mechanic that rewards dealing damage.
 * Eclipse Fang uses a hybrid approach: flat heal + magic scaling.
 * We support both "heal equal to damage" and "heal independent of damage" styles.
 * 
 * Integration:
 * - Used by lifesteal abilities like Eclipse Fang
 * - Creates LifestealDamageGA for processing by LifestealSystem
 * - Damage goes through normal damage pipeline (armor applies)
 * 
 * Used by:
 * - Eclipse Fang (Bakunawa): Heals for 50 (+100% MAG), deals 110% MAG damage
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Effect that deals damage and heals the caster
/// </summary>
[System.Serializable]
public class LifestealEffect : Effects
{
    [Header("Damage Settings")]
    
    /// <summary>
    /// Base damage before stat scaling
    /// </summary>
    [SerializeField]
    [Tooltip("Base damage before stat scaling")]
    private float baseDamage = 0f;
    
    /// <summary>
    /// Damage multiplier based on caster's ATK
    /// </summary>
    [SerializeField]
    [Tooltip("Damage multiplier for ATK (1.0 = 100% ATK)")]
    private float damageAttackAmp = 0f;
    
    /// <summary>
    /// Damage multiplier based on caster's MAG
    /// </summary>
    [SerializeField]
    [Tooltip("Damage multiplier for MAG (1.1 = 110% MAG)")]
    private float damageMagicAmp = 1.1f;
    
    [Header("Heal Settings")]
    
    /// <summary>
    /// Flat heal amount
    /// </summary>
    [SerializeField]
    [Tooltip("Flat heal amount")]
    private float flatHeal = 50f;
    
    /// <summary>
    /// Heal multiplier based on caster's MAG
    /// </summary>
    [SerializeField]
    [Tooltip("Heal multiplier for MAG (1.0 = 100% MAG)")]
    private float healMagicAmp = 1f;
    
    /// <summary>
    /// Percentage of damage dealt to heal (true lifesteal)
    /// </summary>
    [SerializeField]
    [Tooltip("Percentage of damage dealt to heal (1.0 = 100% lifesteal)")]
    [Range(0f, 2f)]
    private float lifestealPercent = 0f;
    
    [Header("Accuracy")]
    
    /// <summary>
    /// Hit chance for the damage portion
    /// </summary>
    [SerializeField]
    [Tooltip("Hit chance (1.0 = 100%)")]
    [Range(0f, 1f)]
    private float accuracy = 1f;

    /// <summary>
    /// Creates a lifesteal damage action
    /// </summary>
    /// <param name="targets">Who receives the damage (first target used)</param>
    /// <param name="caster">Who deals damage and receives healing</param>
    /// <returns>LifestealDamageGA for processing</returns>
    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        if (caster == null)
        {
            Debug.LogWarning("[LifestealEffect] Caster is null.");
            return null;
        }
        
        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning("[LifestealEffect] No targets specified.");
            return null;
        }
        
        // Check hit chance
        if (!DamageCalculator.IsHit(accuracy))
        {
            Debug.Log("[LifestealEffect] Attack missed, no lifesteal.");
            return null;
        }
        
        // Calculate damage amount
        float damageAmount = baseDamage;
        damageAmount += damageAttackAmp * caster.AttackPower;
        damageAmount += damageMagicAmp * caster.MagicPower;
        
        // Target the first valid target
        CombatantView target = null;
        foreach (var t in targets)
        {
            if (t != null)
            {
                target = t;
                break;
            }
        }
        
        if (target == null)
        {
            Debug.LogWarning("[LifestealEffect] No valid target found.");
            return null;
        }
        
        Debug.Log($"[LifestealEffect] {caster.name} attacks {target.name} for {damageAmount:F1} damage with lifesteal");
        
        return new LifestealDamageGA(damageAmount, target, caster, flatHeal, healMagicAmp, lifestealPercent);
    }
}
