using UnityEngine;

/* DRAW CARD GAME ACTION DOCUMENTATION
 * 
 * Purpose: Represents a single card draw action in the game
 * 
 * How it works:
 * - This action tells the game to draw one card from the deck
 * - Gets processed by CardSystem to actually move a card from deck to hand
 * - Used for individual card draws (different from DrawCardsGA which draws multiple)
 * 
 * Integration: Works with CardSystem and ActionSystem for card management
 */

/// <summary>
/// Game action that draws one card from the deck to the player's hand
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Tells the game to draw exactly one card</para>
/// 
/// <para><strong>What it does:</strong> This action represents drawing a single card 
/// from the player's deck into their hand. It's used when specific effects or 
/// abilities need to draw just one card instead of multiple cards.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Action gets created when something needs to draw one card</item>
/// <item>CardSystem receives this action</item>
/// <item>CardSystem moves one card from deck to hand</item>
/// <item>Hand view updates to show the new card</item>
/// </list>
/// 
/// <para><strong>Needs:</strong> CardSystem must be in the scene to handle this action</para>
/// 
/// <para><strong>Works with:</strong> CardSystem for actual card drawing, ActionSystem for processing</para>
/// 
/// <para><strong>How to use:</strong> Create this action when you need to draw exactly one card</para>
/// </remarks>
public class DrawCardGA : GameAction
{
    /// <summary>
    /// Creates a new single card draw action
    /// </summary>
    /// <remarks>
    /// Simple constructor that creates an action to draw one card.
    /// No parameters needed since it always draws exactly one card.
    /// </remarks>
    public DrawCardGA()
    {
        // No setup needed - this action always draws exactly one card
    }
}
