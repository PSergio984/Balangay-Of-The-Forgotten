using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

/* CARD SYSTEM DOCUMENTATION
 * 
 * Purpose: Manages all card-related functionality including deck, hand, and card actions
 * 
 * How it works:
 * - Handles drawing cards from deck to hand with animations
 * - Manages card playing, discarding, and deck shuffling
 * - Processes card-related game actions and reactions
 * - Controls card lifecycle from deck through hand to discard pile
 * 
 * Integration: Works with ActionSystem, HandView, EffectSystem, and other card components
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

void OnEnable()
{
    ActionSystem.AttachPerformer<DrawCardsGA>(DrawCardsPerformer);
    ActionSystem.AttachPerformer<DiscardAllCardsGA>(DiscardAllCardsPerformer);
    ActionSystem.AttachPerformer<PlayCardsGA>(PlayCardPerformer);
    ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
    ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
}

void OnDisable()
{
    ActionSystem.DetachPerformer<DrawCardsGA>();
    ActionSystem.DetachPerformer<DiscardAllCardsGA>();
    ActionSystem.DetachPerformer<PlayCardsGA>();
    ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
    ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
}
// Performers

public void Setup(List<CardData> deckData)
{
    foreach (var cardData in deckData)
    {
        Card card = new(cardData);
        drawPile.Add(card);
    }
}

    private IEnumerator DrawCardsPerformer(DrawCardsGA drawCardsGA)
    {
        int actualAmount = Mathf.Min(drawCardsGA.Amount, drawPile.Count);
        int notDrawAmount = drawCardsGA.Amount - actualAmount;

        for (int i = 0; i < actualAmount; i++)
        {
            yield return DrawCard();
        }

        if (notDrawAmount > 0)
        {
            RefillDeck();
            for (int i = 0; i < notDrawAmount; i++)
            {
                yield return DrawCard();
            }
        }
    }

    private IEnumerator DiscardAllCardsPerformer(DiscardAllCardsGA discardAllCardsGA)
    {
        foreach (var card in hand)
        {
            discardPile.Add(card);
            CardView cardView = handView.RemoveCard(card);
            yield return DiscardCard(cardView);
        }
        
        hand.Clear();
    }
    private IEnumerator PlayCardPerformer(PlayCardsGA playCardsGA)
    {
        hand.Remove(playCardsGA.Card);
        CardView cardView = handView.RemoveCard(playCardsGA.Card);
        // Additional card playing logic would go here
        yield return DiscardCard(cardView);
        SpendStaminaGA spendStaminaGA = new (playCardsGA.Card.Stamina);
        ActionSystem.Instance.AddReaction(spendStaminaGA);
        //performs effects
        foreach (var effect in playCardsGA.Card.Effects)
        {
            PerformEffectGA performEffectGA = new(effect);
            ActionSystem.Instance.AddReaction(performEffectGA);
        }
    }

    //reactions
    private void EnemyTurnPreReaction(EnemyTurnGA enemyTurnGA)
    {
        DiscardAllCardsGA discardAllCardsGA = new();
        ActionSystem.Instance.AddReaction(discardAllCardsGA);
    }

    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        DrawCardsGA drawCardsGA = new(5);
        ActionSystem.Instance.AddReaction(drawCardsGA);
    }


  private IEnumerator DrawCard()
    {
        Card card = drawPile.Draw();
        hand.Add(card);
        CardView cardView = CardViewCreator.Instance.CreateCardView(card, drawPilePoint.position, drawPilePoint.rotation);
        yield return handView.AddCard(cardView);
    }

    private void RefillDeck()
    {
        drawPile.AddRange(discardPile);
        discardPile.Clear();
    }

    private IEnumerator DiscardCard(CardView cardView)
    {
        cardView.transform.DOScale(Vector3.zero, 0.15f);
        Tween tween = cardView.transform.DOMove(discardPilePoint.position, 0.15f);
        yield return tween.WaitForCompletion();
        Destroy(cardView.gameObject);
    }
}
