using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class CardViewHoverSystem : Singleton<CardViewHoverSystem>
{
    [SerializeField] private CardView cardViewHover;

    public void Show(Card card, Vector3 position)
    {
        cardViewHover.gameObject.SetActive(true);
        cardViewHover.Setup(card);
        cardViewHover.transform.position = position;
    }

    public void Hide()
    {
        cardViewHover.gameObject.SetActive(false);
    }



}
