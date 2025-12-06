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
 * - Uses AnimationCurves for easy visual editing of Y-offset and rotation
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
/// <item>AnimationCurves let you visually edit the Y-offset and rotation curves</item>
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
/// <item>AnimationCurve-based Y-offset and rotation for easy editing</item>
/// <item>Smooth animations when hand changes</item>
/// <item>Automatic spacing and positioning</item>
/// <item>Natural rotation along curve</item>
/// <item>Depth layering for proper card ordering</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> CardSystem for hand management, DOTween for animations, Unity Splines for layout</para>
/// 
/// <para><strong>How to use:</strong> Set up spline curve in Inspector, adjust AnimationCurves for Y-offset and rotation</para>
/// </remarks>
// Manages the visual representation and positioning of cards in the player's hand or deck
public class HandView : MonoBehaviour
{
    [Header("Spline Settings")]
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
    /// Controls spacing between cards in the hand. Higher values produce smaller spacing; must be > 0.01.
    /// </summary>
    [Tooltip("Controls spacing between cards in the hand. Higher values produce smaller spacing; must be > 0.01.")]
    [SerializeField, Range(0.01f, 100f)] private float cardSpacingDivisor = 10f;

    [Header("Curve-Based Positioning (Easy Editing)")]
    /// <summary>
    /// Use AnimationCurves instead of relying purely on spline for Y-offset and rotation.
    /// This makes it much easier to edit the hand shape visually.
    /// </summary>
    [Tooltip("Enable to use AnimationCurves for Y-offset and rotation instead of spline-only positioning")]
    [SerializeField] private bool useCurveBasedPositioning = true;

    /// <summary>
    /// Curve that controls the Y-offset of cards along the hand (0 = left, 1 = right).
    /// The curve value is multiplied by positioningInfluence.
    /// Tip: Use a bell curve (peak in middle) for a natural fan shape.
    /// </summary>
    [Tooltip("Y-offset curve along the hand (0=left, 1=right). Bell curve = natural fan. Value is multiplied by Positioning Influence.")]
    [SerializeField] private AnimationCurve positioningCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 2f),      // Left edge, starts low
        new Keyframe(0.5f, 1f, 0f, 0f),    // Center, highest point
        new Keyframe(1f, 0f, -2f, 0f)      // Right edge, ends low
    );

    /// <summary>
    /// How much the positioning curve affects the Y position of cards.
    /// Higher values = more pronounced curve effect.
    /// </summary>
    [Tooltip("How much the Y-offset curve affects card positions. Higher = more pronounced curve.")]
    [SerializeField, Range(0f, 2f)] private float positioningInfluence = 0.3f;

    /// <summary>
    /// Curve that controls the Z-rotation (tilt) of cards along the hand (0 = left, 1 = right).
    /// The curve value is multiplied by rotationInfluence.
    /// Tip: Use a linear curve from 1 to -1 for cards to fan outward.
    /// </summary>
    [Tooltip("Rotation curve along the hand (0=left, 1=right). Linear 1 to -1 = cards fan outward. Value is multiplied by Rotation Influence.")]
    [SerializeField] private AnimationCurve rotationCurve = new AnimationCurve(
        new Keyframe(0f, 1f, 0f, -2f),     // Left edge, rotated right
        new Keyframe(0.5f, 0f, -2f, -2f),  // Center, no rotation
        new Keyframe(1f, -1f, -2f, 0f)     // Right edge, rotated left
    );

    /// <summary>
    /// How much the rotation curve affects the Z-rotation of cards (in degrees).
    /// Higher values = more card tilt.
    /// </summary>
    [Tooltip("How much the rotation curve affects card rotation in degrees. Higher = more tilt.")]
    [SerializeField, Range(0f, 45f)] private float rotationInfluence = 15f;

    [Header("Base Position Settings")]
    /// <summary>
    /// The base Y position for the center of the hand.
    /// Cards will be offset from this position based on the positioning curve.
    /// </summary>
    [Tooltip("Base Y position for the hand center. Cards offset from here based on curve.")]
    [SerializeField] private float baseYPosition = -2.5f;

    /// <summary>
    /// The width of the hand spread (how far left/right cards can go).
    /// </summary>
    [Tooltip("Width of the hand spread. Higher = cards spread further apart horizontally.")]
    [SerializeField, Range(1f, 10f)] private float handWidth = 4f;

    /// <summary>
    /// Maximum number of cards before the hand starts to compress spacing.
    /// </summary>
    [Tooltip("Cards compress spacing when hand size exceeds this number.")]
    [SerializeField, Range(3, 15)] private int maxCardsBeforeCompression = 7;
    
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
    /// Z offset per card for layering (keeps cards visually separated in Z)
    /// </summary>
    private const float CardZOffset = 0.01f;

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

        if (useCurveBasedPositioning)
        {
            // Use AnimationCurve-based positioning for easier visual editing
            yield return UpdateCardPositionsCurveBased(duration);
        }
        else
        {
            // Use original spline-based positioning
            yield return UpdateCardPositionsSplineBased(duration);
        }
        
        // Update original positions after animation completes and mark positioning as done
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null)
            {
                cards[i].UpdateOriginalPosition();
            }
        }
    }

    /// <summary>
    /// Updates card positions using AnimationCurves for Y-offset and rotation.
    /// This is the easier-to-edit method that uses visual curves in the Inspector.
    /// </summary>
    private IEnumerator UpdateCardPositionsCurveBased(float duration)
    {
        int cardCount = cards.Count;
        
        // Calculate effective spacing - compress if too many cards
        float effectiveWidth = handWidth;
        if (cardCount > maxCardsBeforeCompression)
        {
            // Compress the hand width slightly when there are many cards
            effectiveWidth = handWidth * (maxCardsBeforeCompression / (float)cardCount);
        }

        // Calculate spacing between cards
        float cardSpacing = cardCount > 1 ? effectiveWidth / (cardCount - 1) : 0f;
        float startX = cardCount > 1 ? -effectiveWidth / 2f : 0f;

        // Loop through each card to set its position and rotation
        for (int i = 0; i < cardCount; i++)
        {
            if (cards[i] == null)
                continue;

            // Calculate normalized position along the hand (0 = leftmost, 1 = rightmost)
            float t = cardCount > 1 ? (float)i / (cardCount - 1) : 0.5f;

            // Calculate X position (horizontal spread)
            float xPos = startX + i * cardSpacing;

            // Evaluate the positioning curve to get Y-offset
            float yOffset = positioningCurve.Evaluate(t) * positioningInfluence;
            float yPos = baseYPosition + yOffset;

            // Evaluate the rotation curve to get Z-rotation (tilt)
            float zRotation = rotationCurve.Evaluate(t) * rotationInfluence;

            // Calculate final world position (Z offset matches spline-based method: negative Z/back)
            Vector3 targetPosition = transform.position + new Vector3(xPos, yPos, 0f) + CardZOffset * i * Vector3.back;

            // Calculate final rotation (tilt around Z-axis)
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, zRotation);

            // Animate card movement to target position
            cards[i].transform.DOMove(targetPosition, duration);
            // Animate card rotation to target rotation
            cards[i].transform.DORotate(targetRotation.eulerAngles, duration);
        }
        
        // Wait for the animation to complete before finishing the coroutine
        yield return new WaitForSeconds(duration);
    }

    /// <summary>
    /// Updates card positions using the original spline-based system.
    /// Kept for backwards compatibility if spline positioning is preferred.
    /// </summary>
    private IEnumerator UpdateCardPositionsSplineBased(float duration)
    {
        // Validate and clamp cardSpacingDivisor to avoid division by zero
        float safeSpacingDivisor = Mathf.Max(cardSpacingDivisor, 0.01f);
        if (cardSpacingDivisor < 0.01f)
        {
            Debug.LogWarning($"[HandView] cardSpacingDivisor was too small (value: {cardSpacingDivisor}). Clamped to 0.01 to avoid division by zero.", this);
        }
        // Calculate spacing between cards
        float cardSpacing = 1f / safeSpacingDivisor;
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
            // Animate card movement to spline position with slight depth offset for layering (use shared CardZOffset)
            cards[i].transform.DOMove(splinePosition + transform.position + CardZOffset * i * Vector3.back, duration);
            // Animate card rotation to match spline orientation
            cards[i].transform.DORotate(rotation.eulerAngles, duration);
        }
        
        // Wait for the animation to complete before finishing the coroutine
        yield return new WaitForSeconds(duration);
    }
}
