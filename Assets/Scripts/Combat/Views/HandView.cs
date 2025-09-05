using System.Collections;
using System.Collections.Generic;
using System.Linq;  
using DG.Tweening;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

/* HAND VIEW DOCUMENTATION
 * 
 * Purpose: Manages the visual arrangement of cards in the player's hand
 * 
 * How it works:
 * - Arranges cards along a curved spline path for natural hand appearance
 * - Smoothly animates cards when adding or removing from hand
 * - Automatically repositions all cards to maintain even spacing
 * - Creates an attractive fan-like arrangement of cards
 * 
 * Integration: Used by CardSystem to display cards, works with CardView and DOTween
 */

/// <summary>
/// Manages the visual representation and positioning of cards in the player's hand
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Creates an attractive visual arrangement of cards in the player's hand</para>
/// 
/// <para><strong>What it does:</strong> This system manages how cards look when they're in 
/// the player's hand. Instead of just lining them up straight, it arranges them along a 
/// curved path (spline) to create a natural fan-like appearance, similar to how you might 
/// hold cards in real life. When cards are added or removed, it smoothly animates all 
/// the other cards to new positions.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Uses a curved spline path to define where cards should be positioned</item>
/// <item>When adding cards, puts them in the list and repositions everything</item>
/// <item>When removing cards, finds the card and updates remaining positions</item>
/// <item>Calculates spacing so cards are evenly distributed along the curve</item>
/// <item>Animates cards smoothly to new positions with DOTween</item>
/// <item>Applies proper rotation so cards follow the curve naturally</item>
/// </list>
/// 
/// <para><strong>Features:</strong></para>
/// <list type="bullet">
/// <item>Curved spline-based card arrangement</item>
/// <item>Smooth animations when hand changes</item>
/// <item>Automatic spacing and positioning</item>
/// <item>Natural rotation along curve</item>
/// <item>Depth layering for proper card ordering</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> CardSystem for hand management, DOTween for animations, Unity Splines for layout</para>
/// 
/// <para><strong>How to use:</strong> Set up spline curve in Inspector, CardSystem calls AddCard/RemoveCard methods</para>
/// </remarks>
// Manages the visual representation and positioning of cards in the player's hand or deck
public class HandView : MonoBehaviour
{
    /// <summary>
    /// Curved path that defines where cards should be positioned in the hand
    /// </summary>
    /// <remarks>
    /// This spline creates the fan-like curve that cards follow when arranged in the hand.
    /// Design this curve in the scene to get the desired hand appearance.
    /// </remarks>
    // curved path for the cards positions
    [SerializeField] private SplineContainer splineContainer;
    
    /// <summary>
    /// List of all card views currently displayed in the hand
    /// </summary>
    /// <remarks>
    /// Keeps track of every card that's currently in the player's hand.
    /// Used for positioning calculations and card management.
    /// </remarks>
    // List to store all card views currently in the hand
    private readonly List<CardView> cards = new();

    /// <summary>
    /// Adds a new card to the hand and updates all card positions with animation
    /// </summary>
    /// <param name="cardView">The card view to add to the hand</param>
    /// <returns>Coroutine that completes when positioning animation finishes</returns>
    /// <remarks>
    /// Coroutine to add a new card to the hand and update all card positions.
    /// Adds the card to the list then repositions all cards with smooth animation.
    /// </remarks>
    // Coroutine to add a new card to the hand and update all card positions
    public IEnumerator AddCard(CardView cardView)
    {
        // Add the new card to the cards list
        cards.Add(cardView);
        // Update positions of all cards with animation over 0.5 seconds
        yield return UpdateCardPositions(0.5f);
    }
    
    /// <summary>
    /// Removes a specific card from the hand and updates remaining card positions
    /// </summary>
    /// <param name="card">The card data to find and remove from the hand</param>
    /// <returns>The CardView that was removed, or null if card wasn't found</returns>
    /// <remarks>
    /// Finds the card view matching the given card data, removes it from the hand,
    /// and repositions the remaining cards with a quick animation.
    /// </remarks>
    public CardView RemoveCard(Card card)
    {
        // Find the card view that matches the card data
        CardView cardView = GetCardView(card);
        // Return null if the card wasn't found in the hand
        if (cardView == null) return null;
        // Remove the card from the hand list
        cards.Remove(cardView);
        // Update positions of remaining cards with quick animation
        StartCoroutine(UpdateCardPositions(0.15f));
        // Return the removed card view
        return cardView;
    }

    /// <summary>
    /// Finds the card view that matches the given card data
    /// </summary>
    /// <param name="card">The card data to search for</param>
    /// <returns>The matching CardView, or null if not found</returns>
    /// <remarks>
    /// Uses LINQ to search through all cards in the hand and find the one
    /// that matches the given card data.
    /// </remarks>
    private CardView GetCardView(Card card)
    {
        // Use LINQ to find the first card view that matches the card data
        return cards.Where(cardView => cardView.Card == card).FirstOrDefault();
    }

    /// <summary>
    /// Updates the positions and rotations of all cards in the hand with animation
    /// </summary>
    /// <param name="duration">How long the animation should take in seconds</param>
    /// <returns>Coroutine that completes when animation finishes</returns>
    /// <remarks>
    /// This method recalculates where each card should be positioned along the spline
    /// and animates them to their new positions. It handles spacing, rotation, and
    /// depth layering to create an attractive hand layout.
    /// </remarks>
    // idk, just update positions of the cards when adding
    private IEnumerator UpdateCardPositions(float duration)
    {
        // Exit early if there are no cards to position
        if (cards.Count == 0)
        {
            yield break;
        }

        // Mark all cards as positioning to prevent hover during animation
        foreach (var card in cards)
        {
            if (card != null)
            {
                card.SetPositioning(true);
            }
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
        
        // Update original positions after animation completes and mark positioning as done
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null)
            {
                cards[i].UpdateOriginalPosition();
            }
        }
    }
}
