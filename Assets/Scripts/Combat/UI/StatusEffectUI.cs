/*
 * STATUS EFFECT UI DOCUMENTATION
 * 
 * How it works:
 * This represents a single status effect icon on screen. Shows the effect's image,
 * how many stacks of that effect are active, and optionally a name to distinguish
 * between different cards that apply the same status effect type.
 * 
 * Design reasoning:
 * We keep this simple - just an icon, a number, and optional name. Players can quickly 
 * see what effects are active and their strength without complex UI. The stack count lets
 * players understand how powerful the effect is (like 3 armor vs 1 armor). The name
 * helps distinguish effects from different sources (e.g., "Bonecracked" vs "Bind" both
 * applying DEFENSE_DOWN but from different cards).
 * 
 * Integration:
 * - Created by StatusEffectsUI when a new effect is applied
 * - Set method updates the visual display when effect stacks change
 * - Gets destroyed when the effect wears off completely
 */

using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI element that displays a single status effect with its stack count and optional name
/// </summary>
public class StatusEffectUI : MonoBehaviour
{
    /// <summary>
    /// The image that shows the status effect's icon
    /// </summary>
    /// <remarks>
    /// Displays the visual representation of the status effect (armor shield, fire, etc).
    /// Should be set up in the prefab to look good on screen.
    /// </remarks>
    [SerializeField] private Image image;
    
    /// <summary>
    /// Text that shows how many stacks of this effect are active
    /// </summary>
    /// <remarks>
    /// Displays numbers like "3" for 3 stacks of armor or "2" for 2 burn stacks.
    /// Helps players understand the strength of the effect.
    /// </remarks>
    [SerializeField] private TMP_Text stackCountText;
    
    /// <summary>
    /// Optional text that shows the name of this specific status effect
    /// </summary>
    /// <remarks>
    /// Used to distinguish between different cards that apply the same status effect type.
    /// For example, "Bonecracked", "Bind", and "Moonfall" all apply DEFENSE_DOWN but
    /// from different cards with different stack counts.
    /// Can be null if not using name display.
    /// </remarks>
    [SerializeField] private TMP_Text effectNameText;
    
    /// <summary>
    /// Updates this UI to show a specific status effect with its current stack count
    /// </summary>
    /// <param name="sprite">The icon image to show for this effect</param>
    /// <param name="stackCount">How many stacks of this effect are currently active</param>
    /// <remarks>
    /// Called by StatusEffectsUI when an effect is applied or its stacks change.
    /// Updates both the visual icon and the number display so players can see
    /// exactly what effect is active and how strong it is.
    /// </remarks>
    public void Set(Sprite sprite, int stackCount)
    {
        // Set the visual icon for this status effect
        image.sprite = sprite;
        // Show the current number of stacks as text
        stackCountText.text = stackCount.ToString();
    }
    
    /// <summary>
    /// Updates this UI to show a specific status effect with its current stack count and name
    /// </summary>
    /// <param name="sprite">The icon image to show for this effect</param>
    /// <param name="stackCount">How many stacks of this effect are currently active</param>
    /// <param name="effectName">The display name for this effect (e.g., "Bonecracked")</param>
    /// <remarks>
    /// Called by StatusEffectsUI when an effect is applied or its stacks change.
    /// Updates the visual icon, number display, and name so players can see
    /// exactly what effect is active, how strong it is, and where it came from.
    /// Use this overload when you want to display a custom name for the effect.
    /// </remarks>
    public void Set(Sprite sprite, int stackCount, string effectName)
    {
        // Set the visual icon for this status effect
        image.sprite = sprite;
        // Show the current number of stacks as text
        stackCountText.text = stackCount.ToString();
        // Show the effect name if the text component exists
        if (effectNameText != null && !string.IsNullOrEmpty(effectName))
        {
            effectNameText.text = effectName;
            effectNameText.gameObject.SetActive(true);
        }
        else if (effectNameText != null)
        {
            effectNameText.gameObject.SetActive(false);
        }
    }
}
