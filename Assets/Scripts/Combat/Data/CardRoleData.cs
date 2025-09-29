using UnityEngine;
using Sirenix.OdinInspector;
/* CARD ROLE DATA DOCUMENTATION
 *
 * Purpose: ScriptableObject that defines all visual assets for a specific card role (e.g., border, icon, etc.)
 *
 * How it works:
 * - Stores all role-based sprites (icon, borders) for a card role
 * - Assigned to CardData to control the card's visual theme in the UI
 * - Allows designers to update role visuals in one place for all cards of that role
 *
 * Integration:
 * - Used by CardData to determine which role visuals to use
 * - Card, CardView, and CardViewCreator access these assets for rendering
 * - Enables scalable, data-driven role visuals (no hardcoded sprite switching)
 *
 * Usage:
 * - Create a CardRoleData asset for each role (e.g., Mandiriigma, Babaylan)
 * - Assign the asset to CardData's RoleData field
 * - Update sprites in the asset to change visuals for all cards of that role
 */

/// <summary>
/// ScriptableObject that defines all visual assets for a specific card role (icon, borders, etc).
/// Assign this asset to CardData to control role-based visuals in the UI.
/// </summary>
[CreateAssetMenu(menuName = "Card/RoleData")]
public class CardRoleData : ScriptableObject
{
    [Title("Role Visuals")]
    
    /// <summary>
    /// The icon that represents this role (displayed on the card).
    /// </summary>
    [Tooltip("The icon that represents this role.")]
    [PreviewField(75, ObjectFieldAlignment.Left)]
    [Required]
    public Sprite RoleIcon;


    /// <summary>
    /// The background circle of the role (displayed on the card).
    /// </summary>
    [Tooltip("The background circle of the role.")]
    [PreviewField(75, ObjectFieldAlignment.Left)]
    [Required]
    public Sprite RoleCircleIcon;

    /// <summary>
    /// The main border sprite used for cards of this role.
    /// </summary>
    [Tooltip("The main border sprite used for cards of this role.")]
    [PreviewField(75, ObjectFieldAlignment.Left)]
    [Required]
    public Sprite MainBorderSprite;

    /// <summary>
    /// The inner/darker border sprite used for cards of this role.
    /// </summary>
    [Tooltip("The inner border sprite used for cards of this role.")]
    [PreviewField(75, ObjectFieldAlignment.Left)]
    [Required]
    public Sprite DarkBorderSprite;

    /// <summary>
    /// The lower border sprite used for cards of this role.
    /// </summary>
    [Tooltip("The lower border sprite used for cards of this role.")]
    [PreviewField(75, ObjectFieldAlignment.Left)]
    [Required]
    public Sprite LowerBorderSprite;
}
