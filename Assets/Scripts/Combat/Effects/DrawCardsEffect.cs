using System.Collections.Generic;
using UnityEngine;

/* DRAW CARDS EFFECT DOCUMENTATION
 * 
 * Purpose: Card effect that draws additional cards from the deck
 * 
 * How it works:
 * - Contains a draw amount that can be set in Inspector
 * - Creates a DrawCardsGA action when the card is played
 * - Doesn't need targets since it affects the player's hand directly
 * - Perfect for utility cards that provide card advantage
 * 
 * Integration: Inherits from Effects base class, works with CardSystem for card drawing
 */

/// <summary>
/// Card effect that draws additional cards from the deck into the player's hand
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Provides card advantage by drawing extra cards from the deck</para>
/// 
/// <para><strong>What it does:</strong> This effect is used on utility cards that let 
/// the player draw more cards from their deck. When a card with this effect is played, 
/// it creates a draw action that adds the specified number of cards to the player's 
/// hand. This gives card advantage and more options for future turns.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Player plays a card that has this effect attached</item>
/// <item>Card system calls GetGameAction() to get the effect</item>
/// <item>This method creates a DrawCardsGA action with the draw amount</item>
/// <item>CardSystem processes the action and draws cards from deck to hand</item>
/// <item>Player gets more cards to use in combat</item>
/// </list>
/// 
/// <para><strong>Examples:</strong></para>
/// <list type="bullet">
/// <item>"Study" card - draw 2 cards</item>
/// <item>"Inspiration" card - draw 1 card</item>
/// <item>"Research" card - draw 3 cards</item>
/// <item>"Focus" card - draw 1 card and gain stamina</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> Effects base class, CardSystem for drawing mechanics</para>
/// 
/// <para><strong>How to use:</strong> Attach to utility card prefabs, set draw amount in Inspector, use NoTM target mode</para>
/// </remarks>
public class DrawCardsEffect : Effects
{
    /// <summary>
    /// How many cards this effect draws from the deck
    /// </summary>
    /// <remarks>
    /// The number of cards that will be drawn from the deck when this effect triggers.
    /// Set this value in the Inspector to match the card's intended utility value.
    /// </remarks>
    [SerializeField] private int drawAmount;

    /// <summary>
    /// Creates a draw cards action with the specified amount
    /// </summary>
    /// <param name="targets">Not used for this effect since card drawing doesn't need targets</param>
    /// <returns>DrawCardsGA action that will draw the specified number of cards</returns>
    /// <remarks>
    /// This method is called by the card system when the card effect should be executed.
    /// Ignores the targets parameter since card drawing affects the player directly.
    /// Creates an action that the CardSystem will process to draw cards from deck.
    /// </remarks>
    public override GameAction GetGameAction(List<CombatantView> targets)
    {
        // Create a draw cards action with the specified amount
        DrawCardsGA drawCardsGA = new(drawAmount);
        // Return the draw action to be processed by CardSystem
        return drawCardsGA;
    }
}
