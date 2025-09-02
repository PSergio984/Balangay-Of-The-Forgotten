using UnityEngine;

/* DISCARD ALL CARDS GA DOCUMENTATION
 * 
 * Purpose: Action that removes all cards from the player's hand
 * 
 * How it works:
 * - Gets rid of every card the player is currently holding
 * - Usually happens at end of combat or when certain effects trigger
 * - Clears the hand completely for a fresh start
 * - Can be used for powerful effects that require sacrificing your hand
 * 
 * Integration: Works with CardSystem to remove cards, processed by ActionSystem
 */

/// <summary>
/// Game action that discards all cards from the player's hand
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Removes every card the player is currently holding</para>
/// 
/// <para><strong>What it does:</strong> This action gets rid of all cards in the player's hand 
/// at once. This might happen at the end of combat, when certain powerful card effects 
/// trigger, or when the game needs to reset the player's hand. It's like throwing away 
/// your entire hand of cards to start fresh.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Something creates this action (end of combat, card effect, etc.)</item>
/// <item>ActionSystem processes the action</item>
/// <item>CardSystem removes all cards from the player's hand</item>
/// <item>Hand becomes empty and ready for new cards</item>
/// <item>Any "on discard" effects from the cards get triggered</item>
/// </list>
/// 
/// <para><strong>Examples:</strong></para>
/// <list type="bullet">
/// <item>End of combat - discard remaining cards before leaving battle</item>
/// <item>Powerful spell - "Discard your hand, then draw 5 new cards"</item>
/// <item>Debuff effect - enemy forces you to discard everything</item>
/// <item>Turn reset - start next turn with empty hand</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> CardSystem for hand management, ActionSystem for processing</para>
/// 
/// <para><strong>How to use:</strong> Create this action when you need to clear the player's hand completely</para>
/// </remarks>
public class DiscardAllCardsGA : GameAction
{
  
}
