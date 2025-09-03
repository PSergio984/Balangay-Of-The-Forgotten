using System.Collections.Generic;
using UnityEngine;

/* DRAW CARDS EFFECT DOCUMENTATION
 * 
 * How it works:
 * - Contains a draw amount that can be set in Inspector
 * - Creates a DrawCardsGA action when triggered
 * - Doesn't need targets since it affects the player's hand directly
 * - Perfect for utility cards and perks that provide card advantage
 * 
 * Design reasoning:
 * - Simple effect that works for both cards and perks
 * - Ignores targets and caster since card drawing affects player directly
 * - Same effect can be used by cards ("Draw 2 cards") and perks ("Draw 1 card when attacked")
 * - Clean separation between effect logic and who triggered it
 * 
 * Integration: Inherits from Effects base class, works with CardSystem and perk system
 */

/// <summary>
/// Card and perk effect that draws additional cards from the deck into the player's hand
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Provides card advantage by drawing extra cards from the deck</para>
/// 
/// <para><strong>What it does:</strong> This effect is used on utility cards and perks that let 
/// the player draw more cards from their deck. When triggered, it creates a draw action that 
/// adds the specified number of cards to the player's hand. This gives card advantage and 
/// more options for future turns.</para>
/// 
/// <para><strong>Perk system integration:</strong> Perks can use this effect to give the player 
/// cards when certain conditions are met. For example, a perk might draw a card whenever 
/// the player is attacked, or when they play a certain type of card.</para>
/// 
/// <para><strong>Examples:</strong></para>
/// <list type="bullet">
/// <item>"Study" card - draw 2 cards</item>
/// <item>Learning perk - draw 1 card when attacked</item>
/// <item>Focus perk - draw 1 card when playing defensive cards</item>
/// <item>Preparation perk - draw cards at start of turn</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> Effects base class, CardSystem for drawing, perk system for triggers</para>
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
    /// <param name="targets">Not used for this effect since card drawing affects player directly</param>
    /// <param name="caster">Not used for this effect since drawing doesn't depend on who triggered it</param>
    /// <returns>DrawCardsGA action that will draw the specified number of cards</returns>
    /// <remarks>
    /// This method is called by both the card system and perk system when the effect should be executed.
    /// Ignores the targets and caster parameters since card drawing affects the player directly
    /// regardless of who triggered it or what the targets were. Creates an action that the 
    /// CardSystem will process to draw cards from deck.
    /// </remarks>
    public override GameAction GetGameAction(List<CombatantView> targets,CombatantView caster)
    {
        // Create a draw cards action with the specified amount
        DrawCardsGA drawCardsGA = new(drawAmount);
        // Return the draw action to be processed by CardSystem
        return drawCardsGA;
    }
}
