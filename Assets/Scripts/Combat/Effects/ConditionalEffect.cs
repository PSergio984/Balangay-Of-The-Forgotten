/*
 * CONDITIONAL EFFECT DOCUMENTATION
 * 
 * How it works:
 * This is a wrapper effect that checks a condition before executing an inner effect.
 * If the condition is not met, the effect does nothing (returns null action).
 * Used for abilities like "Last Stand" that only work when HP is below a threshold.
 * 
 * Design reasoning:
 * Separates condition checking from effect execution for cleaner code organization.
 * Allows any effect to be made conditional without modifying the effect itself.
 * Supports multiple condition types (HP percentage, status effects, etc.).
 * 
 * Integration:
 * - Used by cards/abilities that have conditional requirements
 * - Wraps existing effects (like AddStatusEffectEffect)
 * - Checked before the inner effect's GetGameAction is called
 * 
 * Examples:
 * - Last Stand: Only apply +50% defense if HP < 20%
 * - Desperate Strike: Only deal bonus damage if HP < 50%
 * - Enrage: Only activate if ally was defeated this turn
 */

using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

/// <summary>
/// Effect wrapper that only executes if a condition is met
/// </summary>
[System.Serializable]
public class ConditionalEffect : Effects
{
    /// <summary>
    /// The condition type to check before executing
    /// </summary>
    [SerializeField]
    [LabelText("Condition Type")]
    [InfoBox("Choose when this effect should execute")]
    private ConditionType conditionType;
    
    /// <summary>
    /// HP percentage threshold (0-1, e.g., 0.2 = 20% HP)
    /// Used when conditionType is HP_BELOW or HP_ABOVE
    /// </summary>
    [SerializeField]
    [LabelText("HP Threshold (%)")]
    [Range(0f, 1f)]
    [ShowIf("@conditionType == ConditionType.HP_BELOW || conditionType == ConditionType.HP_ABOVE")]
    [InfoBox("@\"Will activate when HP is \" + (conditionType == ConditionType.HP_BELOW ? \"below\" : \"above\") + \" \" + (hpThreshold * 100) + \"%\"", InfoMessageType.None)]
    private float hpThreshold = 0.2f;
    
    /// <summary>
    /// The target to check the condition on
    /// SELF = caster, FIRST_TARGET = first target in list
    /// </summary>
    [SerializeField]
    [LabelText("Check Condition On")]
    [InfoBox("Which combatant to check the condition on")]
    private ConditionTarget conditionTarget = ConditionTarget.SELF;
    
    /// <summary>
    /// The effect to execute if condition is met
    /// </summary>
    [SerializeReference]
    [ShowInInspector]
    [LabelText("Inner Effect (Execute if condition passes)")]
    [InfoBox("This effect will only execute if the condition is met")]
    private Effects innerEffect;
    
    /// <summary>
    /// Whether to show a warning message if condition is not met
    /// </summary>
    [SerializeField]
    [LabelText("Show Warning on Failure")]
    private bool showWarningIfFailed = true;
    
    /// <summary>
    /// Custom message to display if condition fails
    /// </summary>
    [SerializeField]
    [LabelText("Failure Message")]
    [ShowIf("showWarningIfFailed")]
    [MultiLineProperty(2)]
    private string failureMessage = "Condition not met!";

    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        // Special handling for DAMAGE_HIT condition (needs caster context and checks each target)
        if (conditionType == ConditionType.DAMAGE_HIT)
        {
            if (caster == null)
            {
                Debug.LogWarning("[ConditionalEffect] DAMAGE_HIT condition requires a caster, but caster is null!");
                return null;
            }
            
            if (targets == null || targets.Count == 0)
            {
                Debug.LogWarning("[ConditionalEffect] DAMAGE_HIT condition requires targets, but targets list is empty!");
                return null;
            }
            
            // Filter targets to only those that were hit by the caster's damage
            List<CombatantView> hitTargets = new List<CombatantView>();
            foreach (var target in targets)
            {
                if (target == null) continue;
                
                if (HitTargetTracker.WasTargetHit(caster, target))
                {
                    hitTargets.Add(target);
                    Debug.Log($"[ConditionalEffect] Damage Hit Check: {target.name} was HIT by {caster.name} - INCLUDED");
                }
                else
                {
                    Debug.Log($"[ConditionalEffect] Damage Hit Check: {target.name} was MISSED by {caster.name} - EXCLUDED");
                }
            }
            
            // If no targets were hit, don't execute the effect
            if (hitTargets.Count == 0)
            {
                if (showWarningIfFailed)
                {
                    Debug.Log($"[ConditionalEffect] {failureMessage} - No targets were hit by damage");
                }
                return null;
            }
            
            // Condition met for at least some targets, execute inner effect with only hit targets
            if (innerEffect == null)
            {
                Debug.LogWarning("[ConditionalEffect] Inner effect is null!");
                return null;
            }
            
            Debug.Log($"[ConditionalEffect] Condition met for {hitTargets.Count}/{targets.Count} targets, executing inner effect");
            return innerEffect.GetGameAction(hitTargets, caster);
        }
        
        // For other condition types, use standard checking on a single target
        // Determine which combatant to check the condition on
        CombatantView checkTarget = conditionTarget == ConditionTarget.SELF ? caster : 
                                     (targets != null && targets.Count > 0 ? targets[0] : caster);
        
        if (checkTarget == null)
        {
            Debug.LogWarning("[ConditionalEffect] No valid target to check condition on!");
            return null;
        }
        
        // Check if condition is met using standard condition checking
        bool conditionMet = CheckCondition(checkTarget);
        
        if (!conditionMet)
        {
            if (showWarningIfFailed)
            {
                Debug.Log($"[ConditionalEffect] {failureMessage} (Target: {checkTarget.name})");
            }
            return null; // Condition not met, don't execute effect
        }
        
        // Condition met, execute inner effect
        if (innerEffect == null)
        {
            Debug.LogWarning("[ConditionalEffect] Inner effect is null!");
            return null;
        }
        
        Debug.Log($"[ConditionalEffect] Condition met for {checkTarget.name}, executing inner effect");
        return innerEffect.GetGameAction(targets, caster);
    }
    
    /// <summary>
    /// Public method to check if the condition is met without executing the effect
    /// Used to validate conditions before playing cards
    /// </summary>
    /// <param name="targets">List of potential targets</param>
    /// <param name="caster">The caster of the effect</param>
    /// <returns>True if condition is met, false otherwise</returns>
    public bool IsConditionMet(List<CombatantView> targets, CombatantView caster)
    {
        // Determine which combatant to check the condition on
        CombatantView checkTarget = conditionTarget == ConditionTarget.SELF ? caster : 
                                     (targets != null && targets.Count > 0 ? targets[0] : caster);
        
        if (checkTarget == null)
        {
            Debug.LogWarning("[ConditionalEffect] No valid target to check condition on!");
            return false;
        }
        
        return CheckCondition(checkTarget);
    }
    
    /// <summary>
    /// Checks if the condition is met for the specified combatant
    /// </summary>
    private bool CheckCondition(CombatantView target)
    {
        switch (conditionType)
        {
            case ConditionType.HP_BELOW:
                float hpPercentBelow = (float)target.CurrentHealth / (float)target.MaxHealth;
                bool belowThreshold = hpPercentBelow < hpThreshold;
                Debug.Log($"[ConditionalEffect] HP Check: {target.name} has {hpPercentBelow:P0} HP (threshold: {hpThreshold:P0}) - {(belowThreshold ? "PASSED" : "FAILED")}");
                return belowThreshold;
                
            case ConditionType.HP_ABOVE:
                float hpPercentAbove = (float)target.CurrentHealth / (float)target.MaxHealth;
                bool aboveThreshold = hpPercentAbove > hpThreshold;
                Debug.Log($"[ConditionalEffect] HP Check: {target.name} has {hpPercentAbove:P0} HP (threshold: {hpThreshold:P0}) - {(aboveThreshold ? "PASSED" : "FAILED")}");
                return aboveThreshold;
                
            case ConditionType.ALWAYS:
                return true;
                
            case ConditionType.DAMAGE_HIT:
                // This condition requires both target and caster to be available
                // We need to get the caster from the GetGameAction call context
                // For now, we'll check this in GetGameAction where we have access to caster
                Debug.LogWarning("[ConditionalEffect] DAMAGE_HIT condition should be checked in GetGameAction with caster context");
                return false;
                
            default:
                Debug.LogWarning($"[ConditionalEffect] Unknown condition type: {conditionType}");
                return false;
        }
    }
}

/// <summary>
/// Types of conditions that can be checked
/// </summary>
public enum ConditionType
{
    /// <summary>Effect always executes</summary>
    ALWAYS,
    
    /// <summary>Execute only if HP is below threshold</summary>
    HP_BELOW,
    
    /// <summary>Execute only if HP is above threshold</summary>
    HP_ABOVE,
    
    /// <summary>Execute only if the target was hit by the caster's damage in the current move sequence</summary>
    DAMAGE_HIT
}

/// <summary>
/// Which combatant to check the condition on
/// </summary>
public enum ConditionTarget
{
    /// <summary>Check condition on the caster</summary>
    SELF,
    
    /// <summary>Check condition on the first target</summary>
    FIRST_TARGET
}
