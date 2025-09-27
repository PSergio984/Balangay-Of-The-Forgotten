using UnityEngine;
using Sirenix.OdinInspector;
[CreateAssetMenu(menuName = "Card/RoleData")]
public class CardRoleData : ScriptableObject
{

    [Title("Role Visuals")]
    [Tooltip("The icon that represents this role.")]
    [PreviewField(75, ObjectFieldAlignment.Left)]
    [Required]
    public Sprite RoleIcon;

    [Tooltip("The Backgground of the role sprite used for cards of this role.")]
    [PreviewField(75, ObjectFieldAlignment.Left)]
    [Required]
    public Sprite RoleBackgroundSprite;

    [Tooltip("The main border sprite used for cards of this role.")]
    [PreviewField(75, ObjectFieldAlignment.Left)]
    [Required]
    public Sprite MainBorderSprite;

    [Tooltip("The inner border sprite used for cards of this role.")]
    [PreviewField(75, ObjectFieldAlignment.Left)]
    [Required]
    public Sprite InnerBorderSprite;
    [Tooltip("The lower border sprite used for cards of this role.")]
    [PreviewField(75, ObjectFieldAlignment.Left)]
    [Required]
    public Sprite LowerBorderSprite;
}
