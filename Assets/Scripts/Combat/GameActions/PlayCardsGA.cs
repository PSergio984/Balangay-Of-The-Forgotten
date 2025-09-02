using UnityEngine;

/* PLAY CARDS GA DOCUMENTATION
 * 
 * Purpose: Action representing the player playing a card from their hand
 * 
 * How it works:
 * - Contains reference to which card is being played
 * - Triggers the card's effects when processed
 * - Usually spends stamina and removes card from hand
 * - Core action for all card-based gameplay
 * 
 * Integration: Created by CardSystem when player clicks cards, processed by ActionSystem
 */

/// <summary>
/// Game action that represents the player playing a card from their hand
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> The main action for all card-based gameplay</para>
/// 
/// <para><strong>What it does:</strong> This action represents the player playing a card 
/// from their hand. When the player clicks on a card and chooses to play it, this action 
/// is created with a reference to that card. When processed, it triggers all the card's 
/// effects, spends the required stamina, and removes the card from the hand.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Player clicks on a card in their hand</item>
/// <item>System checks if player has enough stamina</item>
/// <item>If yes, creates this action with the card reference</item>
/// <item>ActionSystem processes the play card action</item>
/// <item>Card's effects get executed (damage, healing, etc.)</item>
/// <item>Stamina gets spent and card gets removed from hand</item>
/// </list>
/// 
/// <para><strong>Examples:</strong></para>
/// <list type="bullet">
/// <item>Playing "Fireball" card - deals damage to enemies</item>
/// <item>Playing "Heal" card - restores player health</item>
/// <item>Playing "Draw Cards" card - adds more cards to hand</item>
/// <item>Any card the player chooses to use from their hand</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> CardSystem creates these, ActionSystem processes them, triggers card effects</para>
/// 
/// <para><strong>How to use:</strong> Created when player plays cards, pass the card being played</para>
/// </remarks>
public class PlayCardsGA : GameAction
{
    /// <summary>
    /// The card that the player is playing
    /// </summary>
    /// <remarks>
    /// Reference to the specific card being played from the player's hand.
    /// Contains all the card's data like effects, costs, name, and description.
    /// </remarks>
    public Card Card { get; private set; }
    
   /// <summary>
   /// Creates a new play card action with the specified card
   /// </summary>
   /// <param name="card">The card being played by the player</param>
   /// <remarks>
   /// Constructor that creates a new card playing action. Stores which card 
   /// is being played so the system knows what effects to trigger.
   /// </remarks>
   public PlayCardsGA(Card card)
    {
        // Store which card is being played
        this.Card = card;
    }
}
