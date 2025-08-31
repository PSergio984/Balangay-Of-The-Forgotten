
using UnityEngine;

public class Card
{
    public string Title => data.Description;
    public string Description => data.Description;
    public Sprite image => data.Image;
    public int Stamina { get; private set; }
    private readonly CardData data;
    public Card(CardData cardData)
    {
        data = cardData;
        Stamina = cardData.Stamina;
    }
}
