using UnityEngine;
using System.Collections.Generic;
// Test system for managing card creation and addition to hand during gameplay
public class TestSystem : MonoBehaviour
{
    [SerializeField] private List<CardData> deckData;

    private void Start()
    {
        CardSystem.Instance.Setup(deckData);
    }


}
