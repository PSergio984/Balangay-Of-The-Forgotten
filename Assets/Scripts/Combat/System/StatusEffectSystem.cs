/*
 * STATUS EFFECT SYSTEM DOCUMENTATION
 * 
 * How it works:
 * This system processes AddStatusEffectGA actions and actually applies the status
 * effects to combatants. When cards or perks want to give someone armor or burn,
 * they create an action and this system handles the execution. Simple processor
 * that bridges between actions and combatant status effect changes.
 * 
 * Design reasoning:
 * We separate the action creation from the execution to keep things clean. Cards
 * and effects just need to create actions - they don't need to know how to apply
 * status effects. This system handles all the actual application logic and can
 * add visual effects or other processing as needed.
 * 
 * Integration:
 * - Listens for AddStatusEffectGA actions from the ActionSystem
 * - Calls AddStatusEffect on target combatants to apply effects
 * - Shows popup notification when status effects are applied
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// System that processes status effect actions and applies them to combatants
/// </summary>
public class StatusEffectSystem : MonoBehaviour
{
    [Header("Status Effect Popup Colors")]
    [Tooltip("Color for buff effects (positive)")]
    [SerializeField] private Color buffColor = new Color(0.4f, 1f, 0.4f, 1f); // Green
    
    [Tooltip("Color for debuff effects (negative)")]
    [SerializeField] private Color debuffColor = new Color(1f, 0.4f, 0.4f, 1f); // Red
    
    [Tooltip("Color for neutral effects")]
    [SerializeField] private Color neutralColor = new Color(1f, 1f, 0.4f, 1f); // Yellow
    
    [Tooltip("Vertical offset for status effect popup")]
    [SerializeField] private float popupOffsetY = 1.5f;

    // List of buff-type status effects (positive effects)
    private static readonly HashSet<StatusEffectType> BuffEffects = new HashSet<StatusEffectType>
    {
        StatusEffectType.SHIELD,
        StatusEffectType.ATTACK_UP,
        StatusEffectType.DEFENSE_UP,
        StatusEffectType.CRIT_UP,
        StatusEffectType.DMG_UP,
        StatusEffectType.FOCUSED,
        StatusEffectType.RAGE,
        StatusEffectType.INVULNERABLE,
        StatusEffectType.ARMOR,
        StatusEffectType.TEMP_HP
    };

    // List of debuff-type status effects (negative effects)
    private static readonly HashSet<StatusEffectType> DebuffEffects = new HashSet<StatusEffectType>
    {
        StatusEffectType.BURN,
        StatusEffectType.ATTACK_DOWN,
        StatusEffectType.DEFENSE_DOWN,
        StatusEffectType.CRIT_DOWN,
        StatusEffectType.STUN,
        StatusEffectType.DEVOURED,
        StatusEffectType.TAUNT
    };

    /// <summary>
    /// Subscribe to handle AddStatusEffectGA actions when this system starts
    /// </summary>
    private void OnEnable()
    {
        ActionSystem.AttachPerformer<AddStatusEffectGA>(AddStatusEffectPerformer);
    }

    /// <summary>
    /// Unsubscribe from actions when this system stops to prevent errors
    /// </summary>
    private void OnDisable()
    {
        ActionSystem.DetachPerformer<AddStatusEffectGA>();
    }

    /// <summary>
    /// Processes an AddStatusEffectGA action by applying the effect to all targets
    /// </summary>
    /// <param name="addStatusEffectGA">The action containing effect type, stacks, and targets</param>
    /// <returns>Coroutine that applies effects to each target</returns>
    /// <remarks>
    /// Goes through each target and adds the specified status effect with the given
    /// stack count. Shows a popup notification with the effect name and stack count.
    /// </remarks>
    private IEnumerator AddStatusEffectPerformer(AddStatusEffectGA addStatusEffectGA)
    {
        // Apply the status effect to each target in the list
        foreach (var target in addStatusEffectGA.Targets)
        {
            // Add the specified effect with the specified stack count
            target.AddStatusEffect(addStatusEffectGA.StatusEffectType, addStatusEffectGA.StackCount);
            
            // Show popup notification for the status effect
            ShowStatusEffectPopup(target, addStatusEffectGA.StatusEffectType, addStatusEffectGA.StackCount);
            
            yield return null;
        }
    }

    /// <summary>
    /// Displays a popup showing the status effect being applied
    /// </summary>
    private void ShowStatusEffectPopup(CombatantView target, StatusEffectType effectType, int stackCount)
    {
        // Get position above the target
        SpriteRenderer spriteRenderer = target.GetComponentInChildren<SpriteRenderer>();
        Vector3 popupPosition = spriteRenderer != null ? spriteRenderer.transform.position : target.transform.position;
        popupPosition.y += popupOffsetY;
        
        // Store position for VFX AFTER applying offset (so VFX appears at same height as popup)
        Vector3 effectPosition = popupPosition;
        
        // Determine color based on effect type
        Color popupColor = GetEffectColor(effectType);
        
        // Format the effect name with stack count
        string effectName = FormatEffectName(effectType);
        string displayText = stackCount > 1 ? $"+{stackCount} {effectName}" : effectName;
        
        // Create the popup
        DamagePopUp.CreateTextPopUp(popupPosition, displayText, popupColor, DamagePopUp.PopUpAnimationMode.FadeOnly);
        
        // Play appropriate VFX based on effect type
        PlayStatusEffectVFX(target, effectType, effectPosition);
    }

    /// <summary>
    /// Plays VFX and SFX for the applied status effect
    /// </summary>
    private void PlayStatusEffectVFX(CombatantView target, StatusEffectType effectType, Vector3 position)
    {
        if (CombatVFXManager.Instance == null) return;
        
        // Play appropriate effect based on type
        if (BuffEffects.Contains(effectType))
        {
            CombatVFXManager.Instance.PlayBuffEffect(position);
        }
        else if (DebuffEffects.Contains(effectType))
        {
            CombatVFXManager.Instance.PlayDebuffEffect(position);
        }
        
        // Special effects for specific types
        switch (effectType)
        {
            case StatusEffectType.SHIELD:
            case StatusEffectType.ARMOR:
                CombatVFXManager.Instance.PlayShieldEffect(position);
                break;
            case StatusEffectType.BURN:
                CombatVFXManager.Instance.PlayFireEffect(position);
                break;
        }
    }

    /// <summary>
    /// Gets the appropriate color for a status effect type
    /// </summary>
    private Color GetEffectColor(StatusEffectType effectType)
    {
        if (BuffEffects.Contains(effectType))
            return buffColor;
        if (DebuffEffects.Contains(effectType))
            return debuffColor;
        return neutralColor;
    }

    /// <summary>
    /// Formats the status effect type into a readable name
    /// </summary>
    private string FormatEffectName(StatusEffectType effectType)
    {
        // Convert enum name to readable format (e.g., "AttackUp" -> "Attack Up")
        string name = effectType.ToString();
        
        // Insert spaces before capital letters
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (char c in name)
        {
            if (char.IsUpper(c) && sb.Length > 0)
                sb.Append(' ');
            sb.Append(c);
        }
        
        return sb.ToString();
    }
}
