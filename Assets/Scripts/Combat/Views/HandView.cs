using System.Collections;
using System.Collections.Generic;
using System.Linq;  
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

// Manages the visual representation and positioning of cards in the player's hand or deck
public class HandView : MonoBehaviour
{
    // curved path for the cards positions
    [SerializeField] private SplineContainer splineContainer;
    // List to store all card views currently in the hand
    private readonly List<CardView> cards = new();

    // Coroutine to add a new card to the hand and update all card positions
    public IEnumerator AddCard(CardView cardView)
    {
        // Add the new card to the cards list
        cards.Add(cardView);
        // Update positions of all cards with animation over 0.5 seconds
        yield return UpdateCardPositions(0.5f);
    }
        public CardView RemoveCard(Card card)
    {
        CardView cardView = GetCardView(card);
        if (cardView == null) return null;
        cards.Remove(cardView);
        StartCoroutine(UpdateCardPositions(0.15f));
        return cardView;
    }

    private CardView GetCardView(Card card)
    {
        return cards.Where(cardView => cardView.Card == card).FirstOrDefault();
    }


    // idk, just update positions of the cards when adding
    private IEnumerator UpdateCardPositions(float duration)
    {
        // Exit early if there are no cards to position
        if (cards.Count == 0)
        {
            yield break;
        }

        // Calculate spacing between cards (10% of spline length per card)
        float cardSpacing = 1f / 10f;
        // Calculate the starting position for the first card to center the hand
        float firstCardPosition = 0.5f - (cards.Count - 1) * cardSpacing / 2f;

        // Loop through each card to set its position and rotation
        for (int i = 0; i < cards.Count; i++)
        {
            // Calculate the parameter position along the spline for this card
            float p = firstCardPosition + i * cardSpacing;
            // Get the world position at this point on the spline
            Vector3 splinePosition = splineContainer.EvaluatePosition(p);
            // Get the forward direction (tangent) at this point on the spline
            Vector3 forward = splineContainer.EvaluateTangent(p);
            // Get the up vector at this point on the spline
            Vector3 up = splineContainer.EvaluateUpVector(p);
            // Calculate rotation to align card with spline orientation
            Quaternion rotation = Quaternion.LookRotation(-up, Vector3.Cross(-up, forward).normalized);
            // Animate card movement to spline position with slight depth offset for layering
            cards[i].transform.DOMove(splinePosition + transform.position + 0.01f * i * Vector3.back, duration);
            // Animate card rotation to match spline orientation
            cards[i].transform.DORotate(rotation.eulerAngles, duration);
        }
        // Wait for the animation to complete before finishing the coroutine
        yield return new WaitForSeconds(duration);
    }
}
