using UnityEngine;
using System.Collections.Generic;

/* TEST SYSTEM DOCUMENTATION
 * 
 * Purpose: Sets up card system for testing and development purposes
 * 
 * How it works:
 * - Provides a simple way to initialize the card system with test data
 * - Sets up the deck with predefined cards for testing gameplay
 * - Used during development to quickly test card mechanics
 * 
 * Integration: Works with CardSystem to provide test card data
 */

/// <summary>
/// Development system for testing card functionality with predefined card data
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Sets up the card system with test cards for development</para>
/// 
/// <para><strong>What it does:</strong> This system provides an easy way to test 
/// card functionality during development. It sets up the CardSystem with a 
/// predefined list of cards so developers can quickly test gameplay without 
/// needing complex setup.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Game starts and TestSystem activates</item>
/// <item>TestSystem sends test card data to CardSystem</item>
/// <item>CardSystem sets up the deck with test cards</item>
/// <item>Game is ready for testing with predefined cards</item>
/// </list>
/// 
/// <para><strong>Needs:</strong> CardSystem must be in the scene to receive test data</para>
/// 
/// <para><strong>Works with:</strong> CardSystem for deck setup and initialization</para>
/// 
/// <para><strong>How to use:</strong> Put this script on a GameObject and assign test cards in Inspector</para>
/// </remarks>
// Test system for managing card creation and addition to hand during gameplay
public class TestSystem : MonoBehaviour
{
    /// <summary>
    /// List of card data used for testing the card system
    /// </summary>
    /// <remarks>
    /// This field holds the test cards that will be used to set up the deck.
    /// Assign CardData assets in the Inspector to provide test cards for development.
    /// </remarks>
    [SerializeField] private List<CardData> deckData;

    /// <summary>
    /// Sets up the card system with test data when the game starts
    /// </summary>
    /// <remarks>
    /// Called automatically by Unity when the game starts.
    /// Sends the test card data to CardSystem for deck initialization.
    /// </remarks>
    // private void Start()
    // {
    //     // Set up CardSystem with the test card data for development testing
    //     CardSystem.Instance.Setup(deckData);
    // }
}
