/*
 * SACRIFICE HP EFFECT DOCUMENTATION
 * 
 * How it works:
 * This effect sacrifices a percentage of the caster's current HP to perform an action.
 * Used by abilities like "Guardian's Oath" that cost HP to activate.
 * The HP sacrifice happens BEFORE the inner effect executes.
 * 
 * Design reasoning:
 * HP sacrifice is a common mechanic in RPGs for powerful abilities.
 * Separated into its own effect for reusability across different abilities.
 * Executes as a compound action: Sacrifice HP → Execute inner effect
 * Uses percentage of current HP to scale with caster's remaining health.
 * 
 * Integration:
 * - Creates a SelfDamageGA action to reduce caster's HP
 * - Then executes the inner effect (e.g., shield allies, buff self)
 * - Both actions are chained through ActionSystem reactions
 * 
 * Examples:
 * - Guardian's Oath: Sacrifice 25% current HP → Shield all allies
 * - Blood Pact: Sacrifice 30% current HP → Deal massive damage
 * - Desperate Gambit: Sacrifice 50% current HP → Fully heal ally
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Effect that sacrifices caster's HP before executing an inner effect
/// </summary>
[System.Serializable]
public class SacrificeHpEffect : Effects
{
    /// <summary>
    /// Percentage of caster's CURRENT HP to sacrifice (0.25 = 25%)
    /// </summary>
    [SerializeField] [Range(0f, 1f)] private float hpSacrificePercent = 0.25f;
    
    /// <summary>
    /// The effect to execute after sacrificing HP
    /// </summary>
    [SerializeReference] private Effects innerEffect;
    
    /// <summary>
    /// Whether the sacrifice amount should be used by the inner effect
    /// (e.g., shield allies for the amount sacrificed)
    /// </summary>
    [SerializeField] private bool usesacrificeAmountInEffect = false;
    public bool UseSacrificeAmountInEffect => usesacrificeAmountInEffect;
    
    /// <summary>
    /// Minimum HP the caster must have to perform sacrifice (prevents death)
    /// </summary>
    [SerializeField] private int minimumRemainingHp = 1;

    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        if (caster == null)
        {
            Debug.LogWarning("[SacrificeHpEffect] Caster is null!");
            return null;
        }
        
        // Calculate sacrifice amount
        int sacrificeAmount = Mathf.RoundToInt(caster.CurrentHealth * hpSacrificePercent);
        
        // Ensure caster survives the sacrifice
        int remainingHp = caster.CurrentHealth - sacrificeAmount;
        if (remainingHp < minimumRemainingHp)
        {
            sacrificeAmount = caster.CurrentHealth - minimumRemainingHp;
            Debug.LogWarning($"[SacrificeHpEffect] Reduced sacrifice to {sacrificeAmount} to keep {caster.name} alive (min HP: {minimumRemainingHp})");
        }
        
        if (sacrificeAmount <= 0)
        {
            Debug.LogWarning($"[SacrificeHpEffect] {caster.name} cannot sacrifice HP (current: {caster.CurrentHealth}, min: {minimumRemainingHp})");
            return null;
        }
        
        Debug.Log($"[SacrificeHpEffect] {caster.name} sacrifices {sacrificeAmount} HP ({hpSacrificePercent:P0} of {caster.CurrentHealth})");
        
        // Create self-damage action for HP sacrifice
        DealDamageGA sacrificeDamage = new DealDamageGA(
            amount: sacrificeAmount,
            targets: new List<CombatantView> { caster },
            caster: caster
        );
        
        // Add as a reaction to be processed immediately
        ActionSystem.Instance.AddReaction(sacrificeDamage);
        
        // Execute inner effect if it exists
        if (innerEffect != null)
        {
            // If using sacrifice amount, we could store it in a context variable
            // For now, just execute the inner effect normally
            GameAction innerAction = innerEffect.GetGameAction(targets, caster);
            
            // If the inner effect needs the sacrifice amount (e.g., for shielding),
            // it should be a custom effect that can access this value
            // For now, we just return the inner action
            return innerAction;
        }
        
        // If no inner effect, just return the sacrifice damage
        return sacrificeDamage;
    }
    
    /// <summary>
    /// Gets the amount of HP that would be sacrificed
    /// </summary>
    public int GetSacrificeAmount(CombatantView caster)
    {
        if (caster == null) return 0;
        
        int sacrificeAmount = Mathf.RoundToInt(caster.CurrentHealth * hpSacrificePercent);
        int remainingHp = caster.CurrentHealth - sacrificeAmount;
        
        if (remainingHp < minimumRemainingHp)
        {
            sacrificeAmount = caster.CurrentHealth - minimumRemainingHp;
        }
        
        return Mathf.Max(0, sacrificeAmount);
    }
}
