
using AudioSystem;
using DG.Tweening;
using UnityEngine;

/* CARD VIEW CREATOR DOCUMENTATION
 * 
 * Purpose: Factory that creates and animates card visual representations
 * 
 * How it works:
 * - Creates new card views from a prefab template
 * - Positions cards at specified locations with rotation
 * - Adds smooth scaling animation when cards appear
 * - Sets up the card with its data and makes it interactive
 * 
 * Integration: Used by CardSystem to spawn cards, works with DOTween for animations
 */

/// <summary>
/// Singleton class responsible for creating and animating card view instances
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Factory that creates the visual cards players see and interact with</para>
/// 
/// <para><strong>What it does:</strong> This is the factory that creates all the card visuals 
/// in the game. When the player draws cards, plays cards, or cards appear anywhere on screen, 
/// this system creates them from a prefab template. It also adds a nice scaling animation 
/// so cards don't just pop into existence - they smoothly scale up with a bounce effect.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>CardSystem requests a new card view to be created</item>
/// <item>This system instantiates a card prefab at the specified position</item>
/// <item>Sets the card scale to zero for animation preparation</item>
/// <item>Animates the card scaling from zero to full size with bounce</item>
/// <item>Sets up the card with its data and returns the finished card view</item>
/// </list>
/// 
/// <para><strong>Features:</strong></para>
/// <list type="bullet">
/// <item>Smooth appearance animation with DOTween</item>
/// <item>Bounce effect for satisfying card creation</item>
/// <item>Singleton pattern for easy access</item>
/// <item>Prefab-based card creation for consistency</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> CardSystem for card creation, DOTween for animations, CardView prefabs</para>
/// 
/// <para><strong>How to use:</strong> Assign card prefab in Inspector, other systems call CreateCardView()</para>
/// </remarks>
// Singleton class responsible for creating and animating card view instances
public class CardViewCreator : Singleton<CardViewCreator>
{
    /// <summary>
    /// Prefab reference for the card that will be instantiated
    /// </summary>
    /// <remarks>
    /// This is the template used to create all card visuals in the game.
    /// Should be a prefab with CardView component and all necessary UI elements.
    /// Assign this in the Inspector.
    /// </remarks>
    // Prefab reference for the card that will be instantiated
    [SerializeField] private CardView cardPrefab;

    [SerializeField] SoundData cardSpawnSound;

    /// <summary>
    /// Creates a new card view with position, rotation, and smooth scaling animation
    /// </summary>
    /// <param name="card">The card data to set up the view with</param>
    /// <param name="position">Where to position the card in world space</param>
    /// <param name="rotation">What rotation to give the card</param>
    /// <returns>The created and animated card view</returns>
    /// <remarks>
    /// Public method to create a new card view with position and rotation.
    /// Creates the card from prefab, adds smooth scaling animation, sets up the card 
    /// data, and returns the finished card view ready for interaction.
    /// </remarks>
    // Public method to create a new card view with position and rotation
    public CardView CreateCardView(Card card,Vector3 position, Quaternion rotation)
    {
        // Instantiate a new card view from the prefab at the specified position and rotation
        CardView cardView = Instantiate(cardPrefab, position, rotation);
        
        // Set up the card with its data first (name, cost, description, etc.)
        cardView.Setup(card);
        
        // Mark as positioning to prevent hover during creation animation
        cardView.SetPositioning(true);
        
        // Set the initial scale to zero to prepare for animation
        cardView.transform.localScale = Vector3.zero;

        // Animate the card scaling from zero to full size with a bounce effect over 0.5 seconds
        // When animation completes, update the original scale for hover system
        cardView.transform.DOScale(Vector3.one, 0.5f)
            .SetEase(Ease.OutBack)
            .OnComplete(() => cardView.UpdateOriginalPosition());
        
        SoundManager.Instance.CreateSoundBuilder()
            .WithPosition(position)
            .Play(cardSpawnSound);
        
        // Return the created and animated card view
        return cardView;
    }
    
    
}
