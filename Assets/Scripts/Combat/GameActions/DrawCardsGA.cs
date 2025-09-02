using UnityEngine;

/* DRAW CARDS GAME ACTION DOCUMENTATION
 * 
 * Purpose: Represents drawing multiple cards from the deck in one action
 * 
 * How it works:
 * - This action tells the game to draw a specific number of cards
 * - CardSystem processes this and moves cards from deck to hand
 * - Can handle drawing any number of cards (1, 5, 10, etc.)
 * 
 * Integration: Works with CardSystem and ActionSystem for card management
 */

/// <summary>
/// Game action that draws multiple cards from the deck to the player's hand
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Tells the game to draw a specific number of cards</para>
/// 
/// <para><strong>What it does:</strong> This action represents drawing multiple cards 
/// from the player's deck into their hand. It's used at the start of turns, 
/// by card effects, or any time multiple cards need to be drawn at once.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Action gets created with the number of cards to draw</item>
/// <item>CardSystem receives this action</item>
/// <item>CardSystem moves the specified number of cards from deck to hand</item>
/// <item>Hand view updates to show all the new cards</item>
/// </list>
/// 
/// <para><strong>Needs:</strong> CardSystem must be in the scene to handle this action</para>
/// 
/// <para><strong>Works with:</strong> CardSystem for actual card drawing, ActionSystem for processing</para>
/// 
/// <para><strong>How to use:</strong> Create this action with the number of cards you want to draw</para>
/// </remarks>
public class DrawCardsGA : GameAction
{
    /// <summary>
    /// The number of cards to draw from the deck
    /// </summary>
    /// <remarks>
    /// This property stores how many cards should be moved from the deck to the hand.
    /// Can be any positive number, but the actual cards drawn may be limited by deck size.
    /// </remarks>
    public int Amount { get; set; }
    
    /// <summary>
    /// Creates a new multiple card draw action
    /// </summary>
    /// <param name="amount">How many cards to draw from the deck</param>
    /// <remarks>
    /// Creates an action that will draw the specified number of cards.
    /// If the deck has fewer cards than requested, only the available cards will be drawn.
    /// </remarks>
    public DrawCardsGA(int amount)
    {
        // Store how many cards this action should draw
        Amount = amount;
    }
}
