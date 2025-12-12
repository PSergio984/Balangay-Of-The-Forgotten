/*
 * STATUS EFFECTS UI DOCUMENTATION
 * 
 * How it works:
 * This manages all the status effect icons for one combatant. When a combatant gets
 * a status effect, this creates a UI element to show it. When effects change or wear
 * off, this updates or removes the display. It's like a container that shows all the
 * ongoing effects affecting one character.
 * 
 * Design reasoning:
 * We use a dictionary to quickly find and update specific effect displays. Each effect
 * type gets its own UI element that can be updated when stacks change. When effects
 * reach 0 stacks, we clean up the UI automatically. This keeps the display clean and
 * only shows active effects.
 * 
 * Name Mapping System:
 * Multiple cards can apply the same status effect type with different names.
 * For example, "Bonecracked", "Bind", and "Moonfall" all apply DEFENSE_DOWN but
 * come from different cards. The name mapping system allows displaying custom names
 * for each effect source to help players understand where effects came from.
 * 
 * Integration:
 * - Called by combatant systems when status effects change
 * - Creates/updates/removes StatusEffectUI elements as needed
 * - Maps effect types to their visual sprites for consistent display
 * - Optional name mapping for distinguishing same-type effects from different sources
 */

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the display of all status effects for one combatant
/// </summary>
public class StatusEffectsUI : MonoBehaviour
{
    /// <summary>
    /// Template used to create new status effect UI elements
    /// </summary>
    /// <remarks>
    /// This prefab gets copied every time we need to show a new status effect.
    /// Should contain the image and text components for displaying effects.
    /// </remarks>
    [SerializeField] private StatusEffectUI statusEffectUIPrefab;
    
    /// <summary>
    /// Sprites for different status effect types
    /// </summary>
    /// <remarks>
    /// These images represent each type of status effect visually.
    /// Armor sprite for armor effects, burn sprite for burn effects, etc.
    /// </remarks>
    [SerializeField] private Sprite armorSprite, attackUpSprite, attackDownSprite, burnSprite, critUpSprite, critDownSprite, defenseDownSprite, defenseUpSprite, dmgUpSprite, ignoreDefenseSprite, invulnerableSprite, rageSprite, shieldSprite, stunSprite, devouredSprite, tauntSprite, tempHpSprite, restingSprite, chargingSprite;
    
    /// <summary>
    /// Dictionary tracking all currently displayed status effect UIs
    /// </summary>
    /// <remarks>
    /// Maps each effect type to its UI element for quick lookup and updates.
    /// Lets us find the right UI to update when effect stacks change.
    /// </remarks>
    private Dictionary<StatusEffectType, StatusEffectUI> statusEffectUIs = new();
    
    /// <summary>
    /// Dictionary mapping effect types and stack counts to display names
    /// </summary>
    /// <remarks>
    /// Used to show custom names for effects from different card sources.
    /// Key format: (StatusEffectType, stackCount) → "Effect Name"
    /// Example: (DEFENSE_DOWN, 10) → "Bonecracked"
    ///          (DEFENSE_DOWN, 15) → "Bind"
    ///          (DEFENSE_DOWN, 20) → "Moonfall"
    /// </remarks>
    private static readonly Dictionary<(StatusEffectType type, int stacks), string> effectNameMapping = new()
    {
        // Example mappings - add your specific card-based effect names here
        // DEFENSE_DOWN effects with different stack counts from different cards:
        // { (StatusEffectType.DEFENSE_DOWN, 10), "Bonecracked" },
        // { (StatusEffectType.DEFENSE_DOWN, 15), "Bind" },
        // { (StatusEffectType.DEFENSE_DOWN, 20), "Moonfall" },
        
        // Add more mappings as needed for your cards
    };
    
    /// <summary>
    /// Dictionary storing the current display name for each active effect type
    /// </summary>
    /// <remarks>
    /// Tracks what name is being displayed for each effect so it persists
    /// even when stacks change. Cleared when effect is removed.
    /// </remarks>
    private Dictionary<StatusEffectType, string> activeEffectNames = new();

    /// <summary>
    /// Updates the display for a specific status effect type
    /// </summary>
    /// <param name="statusEffectType">Which type of effect to update</param>
    /// <param name="stackCount">How many stacks of this effect are now active (0 = remove)</param>
    /// <remarks>
    /// This is the main method that handles all status effect UI changes. When an effect
    /// reaches 0 stacks, it removes the UI. When an effect has stacks, it creates or updates
    /// the UI to show the current count. Automatically handles creating/destroying UI elements.
    /// </remarks>
    public void UpdateStatusEffectUI(StatusEffectType statusEffectType, int stackCount)
    {
        // If stack count is 0, the effect has worn off - remove its UI
        if (stackCount == 0)
        {
            if (statusEffectUIs.ContainsKey(statusEffectType))
            {
                // Get the UI element for this effect type
                StatusEffectUI statusEffectUI = statusEffectUIs[statusEffectType];
                // Remove it from our tracking dictionary
                statusEffectUIs.Remove(statusEffectType);
                // Clear any stored name for this effect
                activeEffectNames.Remove(statusEffectType);
                // Destroy the visual element so it disappears from screen
                Destroy(statusEffectUI.gameObject);
            }
        }
        else
        {
            // Effect has stacks - create UI if it doesn't exist yet
            if (!statusEffectUIs.ContainsKey(statusEffectType))
            {
                // Create a new UI element for this effect
                StatusEffectUI statusEffectUI = Instantiate(statusEffectUIPrefab, transform);
                // Add it to our tracking dictionary
                statusEffectUIs.Add(statusEffectType, statusEffectUI);
            }
            // Get the appropriate sprite for this effect type
            Sprite sprite = GetSpriteByType(statusEffectType);
            
            // Try to get a custom name for this effect type and stack count
            string effectName = GetEffectDisplayName(statusEffectType, stackCount);
            
            // Update the UI with name if available
            if (!string.IsNullOrEmpty(effectName))
            {
                statusEffectUIs[statusEffectType].Set(sprite, stackCount, effectName);
            }
            else
            {
                // No custom name - just show icon and stacks
                statusEffectUIs[statusEffectType].Set(sprite, stackCount);
            }
        }
    }
    
    /// <summary>
    /// Updates the display for a specific status effect type with a custom name
    /// </summary>
    /// <param name="statusEffectType">Which type of effect to update</param>
    /// <param name="stackCount">How many stacks of this effect are now active (0 = remove)</param>
    /// <param name="effectName">Custom display name for this effect (e.g., "Bonecracked")</param>
    /// <remarks>
    /// Use this overload when you want to explicitly specify the effect name.
    /// The name will be stored and persist even when stacks change until the effect is removed.
    /// </remarks>
    public void UpdateStatusEffectUI(StatusEffectType statusEffectType, int stackCount, string effectName)
    {
        // Store the custom name for this effect type
        if (stackCount > 0 && !string.IsNullOrEmpty(effectName))
        {
            activeEffectNames[statusEffectType] = effectName;
        }
        
        // If stack count is 0, the effect has worn off - remove its UI
        if (stackCount == 0)
        {
            if (statusEffectUIs.ContainsKey(statusEffectType))
            {
                // Get the UI element for this effect type
                StatusEffectUI statusEffectUI = statusEffectUIs[statusEffectType];
                // Remove it from our tracking dictionary
                statusEffectUIs.Remove(statusEffectType);
                // Clear any stored name for this effect
                activeEffectNames.Remove(statusEffectType);
                // Destroy the visual element so it disappears from screen
                Destroy(statusEffectUI.gameObject);
            }
        }
        else
        {
            // Effect has stacks - create UI if it doesn't exist yet
            if (!statusEffectUIs.ContainsKey(statusEffectType))
            {
                // Create a new UI element for this effect
                StatusEffectUI statusEffectUI = Instantiate(statusEffectUIPrefab, transform);
                // Add it to our tracking dictionary
                statusEffectUIs.Add(statusEffectType, statusEffectUI);
            }
            // Get the appropriate sprite for this effect type
            Sprite sprite = GetSpriteByType(statusEffectType);
            
            // Use the provided name or the stored active name
            string displayName = !string.IsNullOrEmpty(effectName) ? effectName : 
                                 activeEffectNames.TryGetValue(statusEffectType, out var storedName) ? storedName : null;
            
            // Update the UI with name
            if (!string.IsNullOrEmpty(displayName))
            {
                statusEffectUIs[statusEffectType].Set(sprite, stackCount, displayName);
            }
            else
            {
                statusEffectUIs[statusEffectType].Set(sprite, stackCount);
            }
        }
    }
    
    /// <summary>
    /// Gets a display name for an effect based on type and stack count
    /// </summary>
    /// <param name="statusEffectType">The type of status effect</param>
    /// <param name="stackCount">The current stack count</param>
    /// <returns>Custom display name if mapped, or null for default display</returns>
    /// <remarks>
    /// First checks if there's already a stored active name for this effect type.
    /// Then checks the static mapping for type+stack combinations.
    /// Returns null if no custom name is configured.
    /// </remarks>
    private string GetEffectDisplayName(StatusEffectType statusEffectType, int stackCount)
    {
        // First check if we already have an active name stored (persists through stack changes)
        if (activeEffectNames.TryGetValue(statusEffectType, out string activeName))
        {
            return activeName;
        }
        
        // Check the static mapping for this type and stack count
        if (effectNameMapping.TryGetValue((statusEffectType, stackCount), out string mappedName))
        {
            // Store this as the active name for future stack changes
            activeEffectNames[statusEffectType] = mappedName;
            return mappedName;
        }
        
        return null;
    }
    
    /// <summary>
    /// Clears all displayed status effects and resets the tracking dictionaries
    /// </summary>
    /// <remarks>
    /// Call this when a combatant dies or when resetting combat state.
    /// Destroys all status effect UI elements and clears all tracking data.
    /// </remarks>
    public void ClearAllStatusEffects()
    {
        foreach (var kvp in statusEffectUIs)
        {
            if (kvp.Value != null)
            {
                Destroy(kvp.Value.gameObject);
            }
        }
        statusEffectUIs.Clear();
        activeEffectNames.Clear();
    }
    
    /// <summary>
    /// Gets the correct sprite image for a given status effect type
    /// </summary>
    /// <param name="statusEffectType">The type of effect to get a sprite for</param>
    /// <returns>The sprite that represents this effect type visually</returns>
    /// <remarks>
    /// This maps each status effect type to its visual representation.
    /// Add new cases here when adding new status effect types to the enum.
    /// Returns null for unknown types to avoid crashes.
    /// </remarks>
    private Sprite GetSpriteByType(StatusEffectType statusEffectType)
    {
        return statusEffectType switch
        {
            StatusEffectType.ARMOR => armorSprite,
            StatusEffectType.ATTACK_UP => attackUpSprite,
            StatusEffectType.ATTACK_DOWN => attackDownSprite,
            StatusEffectType.BURN => burnSprite,
            StatusEffectType.CHARGING => chargingSprite,
            StatusEffectType.CRIT_UP => critUpSprite,
            StatusEffectType.CRIT_DOWN => critDownSprite,
            StatusEffectType.DEFENSE_DOWN => defenseDownSprite,
            StatusEffectType.DEFENSE_UP => defenseUpSprite,
            StatusEffectType.DEFENSE_IGNORE => ignoreDefenseSprite,
            StatusEffectType.DEVOURED => devouredSprite,
            StatusEffectType.DMG_UP => dmgUpSprite,
            StatusEffectType.FOCUSED => ignoreDefenseSprite,
            StatusEffectType.INVULNERABLE => invulnerableSprite,
            StatusEffectType.RAGE => rageSprite,
            StatusEffectType.RESTING => restingSprite,
            StatusEffectType.SHIELD => shieldSprite,
            StatusEffectType.STUN => stunSprite,
            StatusEffectType.TAUNT => tauntSprite,
            StatusEffectType.TEMP_HP => tempHpSprite,
            _ => null,
        };
    }

}
