using UnityEngine;

/* PLAY CARDS GA DESIGN
 * 
 * How it works:
 * - Contains reference to which card is being played
 * - Can optionally store a manually selected target for targeted cards
 * - Triggers the card's effects when processed by ActionSystem
 * - Spends stamina and removes card from hand
 * - Core action for all card-based gameplay including manual targeting
 * 
 * Design reasoning:
 * - Single action handles both regular cards and manual target cards for consistency
 * - Optional target parameter keeps the action flexible for different card types
 * - Manual target gets passed through to effect system for precise targeting
 * - Same stamina and hand management regardless of targeting method
 * 
 * Integration: Created by CardView when player plays cards, processed by CardSystem
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
    /// The manually selected target for cards that need specific targeting
    /// </summary>
    /// <remarks>
    /// This property holds the target that the player manually selected using the targeting system.
    /// Only used for cards with ManualTargetEffects - remains null for regular cards.
    /// Gets set when player uses targeting arrow to select a specific enemy.
    /// </remarks>
    public HeroView ManualTarget { get; set; }
    
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
    /// Constructor for regular cards that don't need manual targeting.
    /// Sets ManualTarget to null since no specific target was selected.
    /// </remarks>
    public PlayCardsGA(Card card)
    {
        // Store which card is being played
        Card = card;
        // No manual target for regular cards
        ManualTarget = null;
    }
    
    /// <summary>
    /// Creates a new play card action with a manually selected target
    /// </summary>
    /// <param name="card">The card being played by the player</param>
    /// <param name="manualTarget">The specific enemy target selected by the player</param>
    /// <remarks>
    /// Constructor for cards that need manual targeting (like single-target damage spells).
    /// Stores both the card and the specific target that the player selected using the targeting system.
    /// </remarks>
    public PlayCardsGA(Card card, HeroView manualTarget)
    {
        // Store which card is being played
        Card = card;
        // Store the manually selected target
        ManualTarget = manualTarget;
    }
}
