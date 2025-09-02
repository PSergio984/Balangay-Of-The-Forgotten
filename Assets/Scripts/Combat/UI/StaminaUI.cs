using TMPro;
using UnityEngine;

/* STAMINA UI DOCUMENTATION
 * 
 * Purpose: Displays the player's current stamina on screen
 * 
 * How it works:
 * - Shows stamina number in UI text
 * - Gets updated by StaminaSystem when stamina changes
 * - Simple display component that just shows the current value
 * - Helps players know how much stamina they have left
 * 
 * Integration: Updated by StaminaSystem, displays info for player decision-making
 */

/// <summary>
/// UI component that displays the player's current stamina
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Shows how much stamina the player has left to play cards</para>
/// 
/// <para><strong>What it does:</strong> This is a simple UI component that displays the player's 
/// stamina number on screen. When the player spends stamina (by playing cards) or gets 
/// stamina refilled (at turn start), this updates to show the new amount. It helps players 
/// make decisions about which cards they can afford to play.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>StaminaSystem calls UpdateStaminaText() when stamina changes</item>
/// <item>Updates the text display to show the new stamina amount</item>
/// <item>Players can see this number to plan their card plays</item>
/// <item>Simple text display - no complex logic needed</item>
/// </list>
/// 
/// <para><strong>Example:</strong> If player has 5 stamina and plays a card costing 3 stamina, 
/// this will update to show "2" so they know they have 2 stamina left.</para>
/// 
/// <para><strong>Works with:</strong> StaminaSystem calls the update method, TextMeshPro for display</para>
/// 
/// <para><strong>How to use:</strong> Assign a TextMeshPro component in Inspector, StaminaSystem handles updates</para>
/// </remarks>
public class StaminaUI : MonoBehaviour
{
    /// <summary>
    /// Text component that displays the stamina number
    /// </summary>
    /// <remarks>
    /// This TextMeshPro component shows the current stamina value to the player.
    /// Should be positioned where players can easily see it during their turn.
    /// </remarks>
    [SerializeField] private TMP_Text stamina;

    /// <summary>
    /// Updates the stamina display to show the current stamina amount
    /// </summary>
    /// <param name="currentStamina">The new stamina value to display</param>
    /// <remarks>
    /// Called by StaminaSystem whenever stamina changes (spending or refilling).
    /// Simply converts the number to text and displays it. Keeps the UI in sync
    /// with the actual stamina value so players always have accurate information.
    /// </remarks>
    public void UpdateStaminaText(int currentStamina)
    {
        // Convert the stamina number to text and display it
        stamina.text = currentStamina.ToString();
    }
}
