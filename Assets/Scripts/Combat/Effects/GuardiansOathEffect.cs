/*
 * GUARDIANS OATH EFFECT DOCUMENTATION
 * 
 * How it works:
 * Guardian's Oath: Sacrifice 25% current HP, shield all allies (except caster) 
 * for the amount sacrificed for 2 turns.
 * 
 * This is a compound effect that:
 * 1. Calculates 25% of caster's current HP
 * 2. Damages caster for that amount (sacrifice)
 * 3. Applies shield to all allies EXCEPT the caster for that amount
 * 
 * Design reasoning:
 * Combined into single effect for atomic execution and proper shield calculation.
 * Shield amount is based on HP sacrificed, so both must happen together.
 * Uses SHIELD status effect which is already implemented and working.
 * Excludes caster from shield targets as per ability design.
 * 
 * Integration:
 * - Creates SelfDamageGA for HP sacrifice
 * - Creates AddStatusEffectGA for shield application
 * - Both queued as reactions for sequential execution
 * - Shield uses SHIELD status effect (already has duration/ticking)
 * 
 * Used by:
 * - Bagani's Guardian's Oath ability (CD 4, unstackable)
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Effect for Guardian's Oath: Sacrifice HP to shield all allies
/// </summary>
[System.Serializable]
public class GuardiansOathEffect : Effects
{
    /// <summary>
    /// Percentage of caster's CURRENT HP to sacrifice (default 0.25 = 25%)
    /// </summary>
    [SerializeField] [Range(0f, 1f)] private float hpSacrificePercent = 0.25f;
    
    /// <summary>
    /// Minimum HP the caster must have to perform sacrifice
    /// </summary>
    [SerializeField] private int minimumRemainingHp = 1;
    
    /// <summary>
    /// Duration of the shield in turns (default 2)
    /// Note: This is informational - shield stacks represent HP, not duration
    /// Duration is managed by StatusEffectTickSystem
    /// </summary>
    [SerializeField] private int shieldDuration = 2;
    public int ShieldDuration => shieldDuration;

    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        if (caster == null)
        {
            Debug.LogWarning("[GuardiansOathEffect] Caster is null!");
            return null;
        }
        
        // Calculate sacrifice amount (25% of current HP)
        int sacrificeAmount = Mathf.RoundToInt(caster.CurrentHealth * hpSacrificePercent);
        
        // Ensure caster survives the sacrifice
        int remainingHp = caster.CurrentHealth - sacrificeAmount;
        if (remainingHp < minimumRemainingHp)
        {
            sacrificeAmount = caster.CurrentHealth - minimumRemainingHp;
            Debug.LogWarning($"[GuardiansOathEffect] Reduced sacrifice to {sacrificeAmount} to keep {caster.name} alive");
        }
        
        if (sacrificeAmount <= 0)
        {
            Debug.LogWarning($"[GuardiansOathEffect] {caster.name} cannot sacrifice HP (current: {caster.CurrentHealth})");
            return null;
        }
        
        Debug.Log($"[GuardiansOathEffect] {caster.name} sacrifices {sacrificeAmount} HP to protect allies");
        
        // 1. Create self-damage action for HP sacrifice
        DealDamageGA sacrificeDamage = new DealDamageGA(
            amount: sacrificeAmount,
            targets: new List<CombatantView> { caster },
            caster: caster
        );
        ActionSystem.Instance.AddReaction(sacrificeDamage);
        
        // 2. Filter targets to exclude caster
        List<CombatantView> allyTargets = new List<CombatantView>();
        foreach (var target in targets)
        {
            if (target != caster)
            {
                allyTargets.Add(target);
            }
        }
        
        if (allyTargets.Count == 0)
        {
            Debug.LogWarning("[GuardiansOathEffect] No allies to shield (all targets excluded caster)");
            // Still execute sacrifice, but no shield
            return sacrificeDamage;
        }
        
        // 3. Create shield effect for allies
        // Shield stacks = HP amount to absorb (the sacrificed HP amount)
        AddStatusEffectGA shieldAllies = new AddStatusEffectGA(
            statusEffectType: StatusEffectType.SHIELD,
            stackCount: sacrificeAmount, // Shield absorbs the sacrificed amount
            targets: allyTargets
        );
        
        Debug.Log($"[GuardiansOathEffect] Shielding {allyTargets.Count} allies for {sacrificeAmount} HP each");
        
        // Return the shield action (sacrifice already queued as reaction)
        return shieldAllies;
    }
}
