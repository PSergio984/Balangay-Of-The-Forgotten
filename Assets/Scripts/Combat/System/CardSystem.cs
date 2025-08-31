using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
public class CardSystem : Singleton<CardSystem>
{

[SerializeField] private HandView handView;

private readonly List<Card> drawPile = new();
private readonly List<Card> discardPile = new();
private readonly List<Card> hand = new();
[SerializeField] private Transform drawPilePoint;
[SerializeField] private Transform discardPilePoint;

void OnEnable()
{
    ActionSystem.AttachPerformer<DrawCardsGA>(DrawCardsPerformer);
    ActionSystem.AttachPerformer<DiscardAllCardsGA>(DiscardAllCardsPerformer);
    ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
    ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
}

void OnDisable()
{
    ActionSystem.DetachPerformer<DrawCardsGA>();
    ActionSystem.DetachPerformer<DiscardAllCardsGA>();
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
