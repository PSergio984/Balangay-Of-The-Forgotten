using UnityEngine;

/* SPEND STAMINA GA DOCUMENTATION
 * 
 * Purpose: Action that uses up player's stamina when playing cards or abilities
 * 
 * How it works:
 * - Contains the amount of stamina to spend
 * - Gets processed by StaminaSystem to reduce current stamina
 * - Prevents player from playing cards if they don't have enough stamina
 * - Essential for turn-based resource management
 * 
 * Integration: Created when playing cards, processed by StaminaSystem through ActionSystem
 */

/// <summary>
/// Game action that represents spending stamina to play cards or use abilities
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> The action used whenever the player needs to spend stamina</para>
/// 
/// <para><strong>What it does:</strong> This action represents the player using up their stamina 
/// to play cards or use abilities. Each card has a stamina cost, and when the player plays it, 
/// this action is created to deduct that cost from their current stamina. It's like paying 
/// the "energy cost" to do something in the game.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Player tries to play a card or use an ability</item>
/// <item>System checks if player has enough stamina</item>
/// <item>If yes, creates this action with the stamina cost</item>
/// <item>ActionSystem processes the action</item>
/// <item>StaminaSystem reduces player's current stamina by the amount</item>
/// <item>UI updates to show new stamina value</item>
/// </list>
/// 
/// <para><strong>Examples:</strong></para>
/// <list type="bullet">
/// <item>Playing a "Fireball" card that costs 3 stamina</item>
/// <item>Using a special ability that costs 5 stamina</item>
/// <item>Playing multiple cards in one turn until stamina runs out</item>
/// <item>Any action that requires spending player resources</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> StaminaSystem processes this, CardSystem creates these when playing cards</para>
/// 
/// <para><strong>How to use:</strong> Created automatically when playing cards, specify the stamina cost amount</para>
/// </remarks>
public class SpendStaminaGA : GameAction
{
  /// <summary>
  /// The amount of stamina to spend/deduct from the player
  /// </summary>
  /// <remarks>
  /// This is how much stamina will be removed from the player's current total.
  /// Should match the cost of whatever card or ability is being used.
  /// </remarks>
  public int Amount { get; set; }
  
   /// <summary>
   /// Creates a new spend stamina action with the specified amount
   /// </summary>
   /// <param name="amount">How much stamina to spend</param>
   /// <remarks>
   /// Constructor that creates a new stamina spending action. Usually called when 
   /// playing cards or using abilities that have a stamina cost.
   /// </remarks>
   public SpendStaminaGA(int amount)
   {
       // Store the amount of stamina to spend
       Amount = amount;
   }
}
