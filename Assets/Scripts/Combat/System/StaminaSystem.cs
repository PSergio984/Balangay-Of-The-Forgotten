using System;
using System.Collections;
using UnityEngine;

/* STAMINA SYSTEM DOCUMENTATION
 * 
 * Purpose: Manages the hero's stamina for playing cards
 * 
 * How it works:
 * - Tracks current stamina amount (starts at 99)
 * - Spends stamina when cards are played
 * - Refills stamina back to full after enemy turns
 * - Updates the UI to show current stamina
 * 
 * Integration: Works with CardSystem for card costs, ActionSystem for stamina actions
 */

/// <summary>
/// System that manages the hero's stamina for playing cards
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Controls how much stamina the player has to play cards</para>
/// 
/// <para><strong>What it does:</strong> This system tracks the player's stamina which is needed 
/// to play cards. Cards cost stamina to use, and when the player runs out they can't play 
/// more cards until their next turn. The stamina refills automatically after the enemies 
/// take their turn.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Player starts each turn with full stamina (99 points)</item>
/// <item>Playing cards costs stamina based on the card's cost</item>
/// <item>System checks if player has enough stamina before allowing card play</item>
/// <item>After enemy turn ends, stamina refills back to maximum</item>
/// <item>UI gets updated whenever stamina changes</item>
/// </list>
/// 
/// <para><strong>Needs:</strong> StaminaUI component to show stamina to player</para>
/// 
/// <para><strong>Works with:</strong> ActionSystem for stamina actions, CardSystem for checking costs</para>
/// 
/// <para><strong>How to use:</strong> Put this on a GameObject and assign the stamina UI in Inspector</para>
/// </remarks>
public class StaminaSystem : Singleton<StaminaSystem>
{
    /// <summary>
    /// UI component that shows the current stamina to the player
    /// </summary>
    /// <remarks>
    /// This displays the stamina number on screen so players know how much they have left.
    /// Updates automatically whenever stamina changes.
    /// Assign a StaminaUI component in the Inspector.
    /// </remarks>
    [SerializeField] private StaminaUI StaminaUI;
    
    /// <summary>
    /// The maximum amount of stamina the player can have
    /// </summary>
    /// <remarks>
    /// This is the full amount of stamina restored each turn.
    /// Currently set to 99 which should be enough for most card combinations.
    /// </remarks>
    private const int MAX_STAMINA = 99;
    
    /// <summary>
    /// How much stamina the player currently has available
    /// </summary>
    /// <remarks>
    /// This goes down when cards are played and gets refilled after enemy turns.
    /// Starts at maximum and gets updated by stamina actions.
    /// </remarks>
    private int currentStamina = MAX_STAMINA;

    /// <summary>
    /// Sets up stamina handling when this system turns on
    /// </summary>
    /// <remarks>
    /// Unity calls this automatically when the GameObject becomes active.
    /// Registers methods to handle spending/refilling stamina and enemy turn reactions.
    /// </remarks>
    void OnEnable()
    {
        // Register to handle actions that spend stamina (like playing cards)
        ActionSystem.AttachPerformer<SpendStaminaGA>(SpendStaminaPerformer);
        // Register to handle actions that refill stamina (like turn end)
        ActionSystem.AttachPerformer<RefillStaminaGA>(RefillStaminaPerformer);
        // Register to react after enemy turns finish (to refill stamina)
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }
    
    /// <summary>
    /// Cleans up stamina handling when this system turns off
    /// </summary>
    /// <remarks>
    /// Unity calls this automatically when the GameObject becomes inactive.
    /// Stops handling stamina actions to prevent memory problems.
    /// </remarks>
    void OnDisable()
    {
        // Stop handling spend stamina actions
        ActionSystem.DetachPerformer<SpendStaminaGA>();
        // Stop handling refill stamina actions
        ActionSystem.DetachPerformer<RefillStaminaGA>();
        // Stop reacting to enemy turn endings
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

    /// <summary>
    /// Checks if the player has enough stamina to do something
    /// </summary>
    /// <param name="stamina">Amount of stamina needed</param>
    /// <returns>True if player has enough stamina, false if not</returns>
    /// <remarks>
    /// Other systems call this before allowing stamina-costing actions.
    /// Prevents players from playing cards they can't afford.
    /// </remarks>
    public bool HasEnoughStamina(int stamina)
    {
        // Check if current stamina is greater than or equal to the required amount
        return currentStamina >= stamina;
    }

    /// <summary>
    /// Handles spending stamina when cards are played or actions are taken
    /// </summary>
    /// <param name="spendStaminaGA">Action containing how much stamina to spend</param>
    /// <returns>Waits one frame before continuing</returns>
    /// <remarks>
    /// This gets called automatically when something needs to spend stamina.
    /// Reduces current stamina and updates the UI display.
    /// </remarks>
    private IEnumerator SpendStaminaPerformer(SpendStaminaGA spendStaminaGA)
    {
        // Subtract the spending amount from current stamina
        currentStamina -= spendStaminaGA.Amount;
        // Update the UI to show the new stamina amount
        StaminaUI.UpdateStaminaText(currentStamina);
        // Wait one frame before continuing (required for coroutines)
        yield return null;
    }

    /// <summary>
    /// Handles refilling stamina back to maximum
    /// </summary>
    /// <param name="refillStaminaGA">Action to refill stamina (doesn't use any data from it)</param>
    /// <returns>Waits one frame before continuing</returns>
    /// <remarks>
    /// This gets called automatically when stamina should be restored.
    /// Sets stamina back to full and updates the UI display.
    /// </remarks>
    private IEnumerator RefillStaminaPerformer(RefillStaminaGA refillStaminaGA)
    {
        // Set stamina back to the maximum amount
        currentStamina = MAX_STAMINA;
        // Update the UI to show the full stamina amount
        StaminaUI.UpdateStaminaText(currentStamina);
        // Wait one frame before continuing (required for coroutines)
        yield return null;
    }

    /// <summary>
    /// Creates a refill stamina action after enemy turns end
    /// </summary>
    /// <param name="enemyTurnGA">The enemy turn action that just finished</param>
    /// <remarks>
    /// This gets called automatically after each enemy turn completes.
    /// Creates and queues a stamina refill action for the next player turn.
    /// Ensures player always starts their turn with full stamina.
    /// </remarks>
    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        // Create a new action to refill stamina
        RefillStaminaGA refillStaminaGA = new();
        // Add the refill action to be processed next
        ActionSystem.Instance.AddReaction(refillStaminaGA);
    }
}
