using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

/* CARD SYSTEM DESIGN
 * 
 * How it works:
 * - Handles drawing cards from deck to hand with animations
 * - Manages card playing including manual targeting and regular effects
 * - Processes card-related game actions and reactions
 * - Controls card lifecycle from deck through hand to discard pile
 * - Routes manual target effects separately from auto-target effects
 * 
 * Design reasoning:
 * - Separates manual targeting from auto-targeting for cleaner effect processing
 * - Manual target effects get the player-selected target directly
 * - Other effects still use their own target modes for flexibility
 * - Same card playing flow handles both targeting types seamlessly
 * 
 * Integration: Works with ActionSystem, HandView, EffectSystem, ManualTargetingSystem, and other card components
 */

/// <summary>
/// Core system that manages all card functionality including deck, hand, and card actions
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Controls all card-related gameplay mechanics and state</para>
/// 
/// <para><strong>What it does:</strong> This system handles everything related to cards - 
/// drawing from deck, managing the hand, playing cards, discarding, and shuffling. 
/// It also handles card animations and integrates with the action system to process 
/// card-related actions.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Sets up deck from card data at game start</item>
/// <item>Draws cards from deck to hand when requested</item>
/// <item>Handles card playing with effects and stamina costs</item>
/// <item>Manages discarding and deck refilling</item>
/// <item>Responds to enemy turns by discarding/drawing cards</item>
/// </list>
/// 
/// <para><strong>Needs:</strong> HandView for card display, ActionSystem for processing, card prefabs</para>
/// 
/// <para><strong>Works with:</strong> ActionSystem, HandView, EffectSystem, StaminaSystem</para>
/// 
/// <para><strong>How to use:</strong> Set up references in Inspector, system handles card management automatically</para>
/// </remarks>
public class CardSystem : Singleton<CardSystem>
{
    /// <summary>
    /// Visual display system for cards in the player's hand
    /// </summary>
    /// <remarks>
    /// This component handles the visual representation and layout of cards in the hand.
    /// Assign the HandView GameObject in the Inspector.
    /// </remarks>
    [SerializeField] private HandView handView;

    /// <summary>
    /// Cards available to be drawn (the deck)
    /// </summary>
    /// <remarks>
    /// This list contains all cards that can be drawn from the deck.
    /// Cards are removed when drawn and added back when deck is refilled.
    /// </remarks>
    private readonly List<Card> drawPile = new();
    
    /// <summary>
    /// Cards that have been used and discarded
    /// </summary>
    /// <remarks>
    /// This list contains cards that have been played or discarded.
    /// These cards get shuffled back into the deck when it's empty.
    /// </remarks>
    private readonly List<Card> discardPile = new();
    
    /// <summary>
    /// Cards currently in the player's hand
    /// </summary>
    /// <remarks>
    /// This list tracks which cards the player currently has available to play.
    /// Cards are added when drawn and removed when played or discarded.
    /// </remarks>
    private readonly List<Card> hand = new();
    
    /// <summary>
    /// World position where new cards appear when drawn
    /// </summary>
    /// <remarks>
    /// Transform that defines where cards start their animation when being drawn.
    /// Assign a GameObject position in the Inspector to set the draw pile location.
    /// </remarks>
    [SerializeField] private Transform drawPilePoint;
    
    /// <summary>
    /// World position where cards move when discarded
    /// </summary>
    /// <remarks>
    /// Transform that defines where cards animate to when being discarded.
    /// Assign a GameObject position in the Inspector to set the discard pile location.
    /// </remarks>
    [SerializeField] private Transform discardPilePoint;

    /// <summary>
    /// Sets up event handlers when this system becomes active
    /// </summary>
    /// <remarks>
    /// Unity calls this automatically when the GameObject goes from inactive to active.
    /// Registers this system to handle card-related actions and enemy turn reactions.
    /// </remarks>
void OnEnable()
{
    // Register performers for card-related actions
    ActionSystem.AttachPerformer<DrawCardsGA>(DrawCardsPerformer);
    ActionSystem.AttachPerformer<DiscardAllCardsGA>(DiscardAllCardsPerformer);
    ActionSystem.AttachPerformer<PlayCardsGA>(PlayCardPerformer);
    // Listen for enemy turns to handle cards automatically
    ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
    ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
}

/// <summary>
/// Cleans up event handlers when this system becomes inactive
/// </summary>
/// <remarks>
/// Unity calls this automatically when the GameObject goes from active to inactive.
/// Unregisters all handlers to prevent memory leaks and system errors.
/// </remarks>
void OnDisable()
{
    ActionSystem.DetachPerformer<DrawCardsGA>();
    ActionSystem.DetachPerformer<DiscardAllCardsGA>();
    ActionSystem.DetachPerformer<PlayCardsGA>();
    ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
    ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
}
// Performers

    /// <summary>
    /// Initializes the card deck from a list of card data assets
    /// </summary>
    /// <param name="deckData">List of CardData assets that define the deck contents</param>
    /// <remarks>
    /// This method sets up the initial deck by creating Card instances from CardData assets.
    /// Each CardData gets converted into a playable Card and added to the draw pile.
    /// Call this once at game start to prepare the deck for play.
    /// </remarks>
    public void Setup(List<CardData> deckData)
    {
        foreach (var cardData in deckData)
        {
            // Create a playable card from each card design
            Card card = new(cardData);
            drawPile.Add(card);
        }
    }

    /// <summary>
    /// Processes drawing the specified number of cards from deck to hand
    /// </summary>
    /// <param name="drawCardsGA">Action containing the number of cards to draw</param>
    /// <returns>IEnumerator for coroutine execution with card drawing animations</returns>
    /// <remarks>
    /// This method handles drawing cards with deck management. If there aren't enough
    /// cards in the draw pile, it refills the deck from discards and continues drawing.
    /// Each card draw includes animation as cards move from deck to hand.
    /// </remarks>
    private IEnumerator DrawCardsPerformer(DrawCardsGA drawCardsGA)
    {
        // Calculate how many cards we can draw right now
        int actualAmount = Mathf.Min(drawCardsGA.Amount, drawPile.Count);
        int notDrawAmount = drawCardsGA.Amount - actualAmount;

        for (int i = 0; i < actualAmount; i++)
        {
            yield return DrawCard();
        }

        // Need more cards? Shuffle discards back and keep drawing
        if (notDrawAmount > 0)
        {
            RefillDeck();
            for (int i = 0; i < notDrawAmount; i++)
            {
                yield return DrawCard();
            }
        }
    }

    /// <summary>
    /// Processes discarding all cards currently in the player's hand
    /// </summary>
    /// <param name="discardAllCardsGA">Action that triggers discarding all hand cards</param>
    /// <returns>IEnumerator for coroutine execution with discard animations</returns>
    /// <remarks>
    /// This method removes all cards from the hand and moves them to the discard pile.
    /// Each card gets animated as it moves from hand to discard pile.
    /// Typically called at the end of the player's turn or when enemy turn starts.
    /// </remarks>
    private IEnumerator DiscardAllCardsPerformer(DiscardAllCardsGA discardAllCardsGA)
    {
        foreach (var card in hand)
        {
            // Get the visual representation and remove it from hand display
            CardView cardView = handView.RemoveCard(card);
            yield return DiscardCard(cardView);
        }
        
        // Clear the hand data after all visual cards are discarded
        hand.Clear();
    }
    /// <summary>
    /// Handles the complete process of playing a card including effects and targeting
    /// </summary>
    /// <param name="playCardsGA">The action containing the card to be played</param>
    /// <returns>IEnumerator for coroutine execution</returns>
    /// <remarks>
    /// This method processes the full card playing sequence: removes card from hand,
    /// spends stamina, and executes effects with proper targeting. Manual target effects
    /// use the player-selected target, while other effects use their own target modes.
    /// </remarks>
    private IEnumerator PlayCardPerformer(PlayCardsGA playCardsGA)
    {
        // Remove the played card from the player's hand
        hand.Remove(playCardsGA.Card);
        // Get the visual card from the hand display and remove it
        CardView cardView = handView.RemoveCard(playCardsGA.Card);
        // Move the card to the discard pile with animation
        yield return DiscardCard(cardView);
        // Create action to spend the card's stamina cost
        SpendStaminaGA spendStaminaGA = new (playCardsGA.Card.Stamina);
        ActionSystem.Instance.AddReaction(spendStaminaGA);
        
        // Handle manual target effects (like single-target damage spells)
        if (playCardsGA.Card.ManualTargetEffects != null)
        {
            // Create effect action with the manually selected target
            PerformEffectGA performEffectGA = new(playCardsGA.Card.ManualTargetEffects, new() { playCardsGA.ManualTarget });
            ActionSystem.Instance.AddReaction(performEffectGA);
        }
        
        // Process all other effects on the card with their individual targeting
        foreach (var effectWrapper in playCardsGA.Card.OtherEffects)
        {
            // Use the effect's target mode to determine who gets affected
            List<CombatantView> targets = effectWrapper.targetMode.GetTargets();
            // Create an action to perform this specific effect on its targets
            PerformEffectGA performEffectGA = new(effectWrapper.effects,targets);
            // Add the effect action to be processed by the EffectSystem
            ActionSystem.Instance.AddReaction(performEffectGA);
        }
    }

    
    /// <summary>
    /// REACTIONS
    /// Reacts to enemy turn start by discarding all cards in hand
    /// </summary>
    /// <param name="enemyTurnGA">The enemy turn action that triggered this reaction</param>
    /// <remarks>
    /// This reaction happens before the enemy turn fully begins.
    /// Automatically discards all cards in the player's hand to clear it for the next turn.
    /// Part of the turn cycle management to reset the player's hand state.
    /// </remarks>
    private void EnemyTurnPreReaction(EnemyTurnGA enemyTurnGA)
    {
        DiscardAllCardsGA discardAllCardsGA = new();
        ActionSystem.Instance.AddReaction(discardAllCardsGA);
    }

    /// <summary>
    /// Reacts to enemy turn end by drawing a new hand of cards
    /// </summary>
    /// <param name="enemyTurnGA">The enemy turn action that triggered this reaction</param>
    /// <remarks>
    /// This reaction happens after the enemy turn fully completes.
    /// Automatically draws 5 cards to give the player a fresh hand for their next turn.
    /// Part of the turn cycle management to prepare the player for their turn.
    /// </remarks>
    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        DrawCardsGA drawCardsGA = new(5);
        ActionSystem.Instance.AddReaction(drawCardsGA);
    }


  /// <summary>
  /// Draws a single card from the deck and adds it to the hand with animation
  /// </summary>
  /// <returns>IEnumerator for coroutine execution during card draw animation</returns>
  /// <remarks>
  /// This method handles the complete process of drawing one card: removes it from
  /// the draw pile, adds it to the hand, creates the visual card, and animates it
  /// moving from the draw pile position to the hand layout.
  /// </remarks>
  private IEnumerator DrawCard()
    {
        // Remove card from deck and add to hand data
        Card card = drawPile.Draw();
        hand.Add(card);
        // Create visual card at deck position
        CardView cardView = CardViewCreator.Instance.CreateCardView(card, drawPilePoint.position, drawPilePoint.rotation);
        // Animate card moving to hand layout
        yield return handView.AddCard(cardView);
    }

    /// <summary>
    /// Refills the draw pile by moving all discarded cards back to it
    /// </summary>
    /// <remarks>
    /// This method is called when the draw pile is empty but more cards need to be drawn.
    /// Moves all cards from the discard pile back to the draw pile and clears the discard pile.
    /// Represents shuffling the discards back into the deck for continued play.
    /// </remarks>
    private void RefillDeck()
    {
        // Move all discarded cards back to draw pile (shuffle)
        drawPile.AddRange(discardPile);
        discardPile.Clear();
    }

    /// <summary>
    /// Discards a single card with animation and destroys its visual representation
    /// </summary>
    /// <param name="cardView">The visual card to be discarded</param>
    /// <returns>IEnumerator for coroutine execution during discard animation</returns>
    /// <remarks>
    /// This method handles the complete discard process: adds the card to discard pile,
    /// animates it scaling down and moving to discard position, then destroys the GameObject.
    /// Used when cards are played or when hand is cleared.
    /// </remarks>
    private IEnumerator DiscardCard(CardView cardView)
    {
        // Add card data to discard pile
        discardPile.Add(cardView.Card);
        // Animate card shrinking and moving to discard position
        cardView.transform.DOScale(Vector3.zero, 0.15f);
        Tween tween = cardView.transform.DOMove(discardPilePoint.position, 0.15f);
        yield return tween.WaitForCompletion();
        // Clean up the visual GameObject
        Destroy(cardView.gameObject);
    }
}
