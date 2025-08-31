using UnityEngine;

public class PlayCardsGA : GameAction
{
    public Card Card { get; private set; }
   public PlayCardsGA(Card card)
    {
        this.Card = card;
    }
}
