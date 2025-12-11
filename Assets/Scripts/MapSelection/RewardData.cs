using UnityEngine;

/// <summary>
/// Data container for enemy defeat rewards (chest sprites, glow, reward items)
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Contains all visual and reward data for a single enemy defeat reward</para>
/// <para><strong>Usage:</strong> Created as ScriptableObject assets, assigned to MapData for each enemy type</para>
/// </remarks>
[CreateAssetMenu(fileName = "New Reward Data", menuName = "Map Selection/Reward Data")]
public class RewardData : ScriptableObject
{
    [Header("Chest Visuals")]
    /// <summary>
    /// Sprite for the closed chest (shown initially)
    /// </summary>
    [Tooltip("The closed chest sprite shown before opening")]
    [SerializeField] private Sprite closedChestSprite;

    /// <summary>
    /// Sprite for the closed chest (public getter)
    /// </summary>
    public Sprite ClosedChestSprite => closedChestSprite;

    /// <summary>
    /// Sprite for the open chest (shown after clicking)
    /// </summary>
    [Tooltip("The open chest sprite shown after clicking")]
    [SerializeField] private Sprite openChestSprite;

    /// <summary>
    /// Sprite for the open chest (public getter)
    /// </summary>
    public Sprite OpenChestSprite => openChestSprite;

    /// <summary>
    /// Glow sprite that appears behind the chest
    /// </summary>
    [Tooltip("Glow effect sprite displayed behind the chest")]
    [SerializeField] private Sprite glowSprite;

    /// <summary>
    /// Glow sprite that appears behind the chest (public getter)
    /// </summary>
    public Sprite GlowSprite => glowSprite;

    [Header("Reward Item")]
    /// <summary>
    /// The reward item sprite that appears after opening the chest
    /// </summary>
    [Tooltip("The reward item sprite displayed after the chest opens")]
    [SerializeField] private Sprite rewardItemSprite;

    /// <summary>
    /// The reward item sprite that appears after opening the chest (public getter)
    /// </summary>
    public Sprite RewardItemSprite => rewardItemSprite;
}

