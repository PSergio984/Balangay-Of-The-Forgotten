using System.Collections.Generic;
using UnityEngine;

public class MatchSetupSystem : MonoBehaviour
{
   [SerializeField] private HeroData heroData;
    
private void Start()
{
    HeroSystem.Instance.Setup(heroData);
    CardSystem.Instance.Setup(heroData.Deck);
    DrawCardsGA drawCardsGA = new DrawCardsGA(5);
    ActionSystem.Instance.Perform(drawCardsGA);
}
}
