/*
 * STATUS EFFECT UI DOCUMENTATION
 * 
 * How it works:
 * This represents a single status effect icon on screen. Shows the effect's image
 * and how many stacks of that effect are active. Simple visual display that tells
 * players what effects are currently affecting a combatant and how strong they are.
 * 
 * Design reasoning:
 * We keep this simple - just an icon and a number. Players can quickly see what
 * effects are active and their strength without complex UI. The stack count lets
 * players understand how powerful the effect is (like 3 armor vs 1 armor).
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
/// UI element that displays a single status effect with its stack count
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
}
