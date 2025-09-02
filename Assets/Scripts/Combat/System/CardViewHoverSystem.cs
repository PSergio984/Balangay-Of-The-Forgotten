using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

/* CARD VIEW HOVER SYSTEM DOCUMENTATION
 * 
 * Purpose: Shows a big version of cards when players hover over them
 * 
 * How it works:
 * - Has one special card that shows bigger versions of hovered cards
 * - When player hovers over a card, this system shows the big version
 * - When player stops hovering, this system hides the big version
 * - Helps players read card details more easily
 * 
 * Integration: Works with CardView components for hover interactions
 */

/// <summary>
/// System that shows enlarged card views when players hover over cards
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Displays bigger versions of cards for easier reading</para>
/// 
/// <para><strong>What it does:</strong> This system manages a special large card display 
/// that appears when players move their mouse over cards. It makes cards easier 
/// to read by showing them in a bigger size with all the details clearly visible.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Player moves mouse over a small card</item>
/// <item>CardView tells this system to show the big version</item>
/// <item>System activates the big card and fills it with the card's info</item>
/// <item>Player moves mouse away from card</item>
/// <item>System hides the big card again</item>
/// </list>
/// 
/// <para><strong>Needs:</strong> A CardView component assigned for the big card display</para>
/// 
/// <para><strong>Works with:</strong> CardView components that trigger hover events</para>
/// 
/// <para><strong>How to use:</strong> Put this on a GameObject and assign a CardView for the hover display</para>
/// </remarks>
public class CardViewHoverSystem : Singleton<CardViewHoverSystem>
{
    /// <summary>
    /// The big card view that shows when players hover over cards
    /// </summary>
    /// <remarks>
    /// This is a special CardView component that displays enlarged versions of cards.
    /// Assign a CardView GameObject in the Inspector that will serve as the hover display.
    /// </remarks>
    [SerializeField] private CardView cardViewHover;

    /// <summary>
    /// Shows the big version of a card at a specific position
    /// </summary>
    /// <param name="card">The card data to display in the big view</param>
    /// <param name="position">Where on screen to show the big card</param>
    /// <remarks>
    /// This method activates the big card display and fills it with the provided card's information.
    /// The big card appears at the specified position so players can read it clearly.
    /// </remarks>
    public void Show(Card card, Vector3 position)
    {
        // Turn on the big card display
        cardViewHover.gameObject.SetActive(true);
        // Fill the big card with the hovered card's information
        cardViewHover.Setup(card);
        // Position the big card where it should appear
        cardViewHover.transform.position = position;
    }

    /// <summary>
    /// Hides the big card view when hovering ends
    /// </summary>
    /// <remarks>
    /// This method turns off the big card display when players move their mouse
    /// away from cards, returning the view to normal.
    /// </remarks>
    public void Hide()
    {
        // Turn off the big card display
        cardViewHover.gameObject.SetActive(false);
    }
}
