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
 * Integration:
 * - Called by combatant systems when status effects change
 * - Creates/updates/removes StatusEffectUI elements as needed
 * - Maps effect types to their visual sprites for consistent display
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
    [SerializeField] private Sprite armorSprite, attackUpSprite, burnSprite, critUpSprite, defenseDownSprite, defenseUpSprite, dmgUpSprite, ignoreDefenseSprite, invulnerableSprite, tauntSprite, tempHpSprite;
    
    /// <summary>
    /// Dictionary tracking all currently displayed status effect UIs
    /// </summary>
    /// <remarks>
    /// Maps each effect type to its UI element for quick lookup and updates.
    /// Lets us find the right UI to update when effect stacks change.
    /// </remarks>
    private Dictionary<StatusEffectType, StatusEffectUI> statusEffectUIs = new();

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
            // Update the UI to show the current stack count and sprite
            statusEffectUIs[statusEffectType].Set(sprite, stackCount);
        }
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
            StatusEffectType.BURN => burnSprite,
            StatusEffectType.CRIT_UP => critUpSprite,
            StatusEffectType.DEFENSE_DOWN => defenseDownSprite,
            StatusEffectType.DEFENSE_UP => defenseUpSprite,
            StatusEffectType.DMG_UP => dmgUpSprite,
            StatusEffectType.IGNORE_DEFENSE => ignoreDefenseSprite,
            StatusEffectType.INVULNERABLE => invulnerableSprite,
            StatusEffectType.TAUNT => tauntSprite,
            StatusEffectType.TEMP_HP => tempHpSprite,
            _ => null,
        };
    }

}
