using UnityEngine;

/* REFILL STAMINA GA DOCUMENTATION
 * 
 * Purpose: Action that restores player's stamina back to full
 * 
 * How it works:
 * - Restores stamina to maximum value (usually 99)
 * - Typically happens at the start of player's turn
 * - Allows player to play cards again after enemy turn
 * - Essential for turn-based stamina management cycle
 * 
 * Integration: Created by StaminaSystem after enemy turns, processed by ActionSystem
 */

/// <summary>
/// Game action that restores the player's stamina back to maximum
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> The action that gives the player their stamina back</para>
/// 
/// <para><strong>What it does:</strong> This action restores the player's stamina back to 
/// the maximum amount (usually 99 points). This typically happens at the start of each 
/// player turn, after the enemies have taken their actions. It's like getting a fresh 
/// supply of energy to play cards with for the new turn.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Enemy turn ends and control returns to player</item>
/// <item>StaminaSystem automatically creates this action</item>
/// <item>ActionSystem processes the refill action</item>
/// <item>StaminaSystem sets player's stamina back to maximum</item>
/// <item>UI updates to show full stamina</item>
/// <item>Player can now play cards again</item>
/// </list>
/// 
/// <para><strong>Examples:</strong></para>
/// <list type="bullet">
/// <item>Start of new turn - player gets full stamina refresh</item>
/// <item>Special card effect - "Restore all stamina immediately"</item>
/// <item>Healing item - "Refill stamina to maximum"</item>
/// <item>Turn cycle - automatic stamina restoration</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> StaminaSystem creates and processes this, ActionSystem handles it</para>
/// 
/// <para><strong>How to use:</strong> Usually created automatically by game systems, no parameters needed</para>
/// </remarks>
public class RefillStaminaGA : GameAction
{
    // No properties needed - this action simply restores stamina to maximum
    // The StaminaSystem knows what the maximum value is
}
