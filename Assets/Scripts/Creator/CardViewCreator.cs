using System;
using DG.Tweening;
using UnityEngine;

// Singleton class responsible for creating and animating card view instances
public class CardViewCreator : Singleton<CardViewCreator>
{
    // Prefab reference for the card that will be instantiated
    [SerializeField] private CardView cardPrefab;

    // Public method to create a new card view with position and rotation
    public CardView CreateCardView(Vector3 position, Quaternion rotation)
    {
        // Instantiate a new card view from the prefab at the specified position and rotation
        CardView cardView = Instantiate(cardPrefab, position, rotation);
        // Set the initial scale to zero to prepare for animation
        cardView.transform.localScale = Vector3.zero;
        // Animate the card scaling from zero to full size with a bounce effect over 0.5 seconds
        cardView.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        // Return the created and animated card view
        return cardView;
    }
    
}
