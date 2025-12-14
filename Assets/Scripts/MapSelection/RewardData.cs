using UnityEngine;

/* REWARD DATA
 * 
 * Purpose: Contains all visual and reward data for enemy defeat rewards
 * 
 * How it works:
 * - Mini-boss (first enemy) rewards: Include special cards (buff cards)
 * - Main boss (second enemy) rewards: Include relics (decorative trophies)
 * - Visual data includes chest sprites, glow effects, and reward item sprites
 * 
 * Integration:
 * - Assigned to MapData.minibossReward and MapData.mainBossReward
 * - Used by RewardChestUI to display and animate chest opening
 * - VictoryDefeatUI reads special card/relic data to add to collections
 */

/// <summary>
/// Data container for enemy defeat rewards (chest sprites, glow, reward items)
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Contains all visual and reward data for a single enemy defeat reward</para>
/// <para><strong>Usage:</strong> Created as ScriptableObject assets, assigned to MapData for each enemy type</para>
/// <para><strong>Mini-boss rewards:</strong> Include special cards that grant powerful buffs</para>
/// <para><strong>Main boss rewards:</strong> Include relics that are decorative trophies</para>
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

    [Header("Relic Reward (Main Boss Only)")]
    /// <summary>
    /// Optional relic obtained from defeating the main boss (second enemy)
    /// </summary>
    /// <remarks>
    /// <para><strong>Why:</strong> Main bosses grant decorative trophy relics</para>
    /// <para><strong>How:</strong> Assign a RelicData asset here for main boss RewardData</para>
    /// <para><strong>Note:</strong> Leave null for mini-boss rewards</para>
    /// </remarks>
    [Tooltip("Relic reward from main boss defeat (leave null for mini-boss rewards)")]
    [SerializeField] private RelicData associatedRelic;
    
    /// <summary>
    /// The relic associated with this reward (null if not a relic reward)
    /// </summary>
    public RelicData AssociatedRelic => associatedRelic;

    [Header("Special Card Reward (Mini-Boss Only)")]
    /// <summary>
    /// Optional special card obtained from defeating the mini-boss (first enemy)
    /// </summary>
    /// <remarks>
    /// <para><strong>Why:</strong> Mini-bosses grant powerful buff cards</para>
    /// <para><strong>How:</strong> Assign a SpecialCardData asset here for mini-boss RewardData</para>
    /// <para><strong>Note:</strong> Leave null for main boss rewards</para>
    /// </remarks>
    [Tooltip("Special card reward from mini-boss defeat (leave null for main boss rewards)")]
    [SerializeField] private SpecialCardData specialCardReward;
    
    /// <summary>
    /// The special card associated with this reward (null if not a special card reward)
    /// </summary>
    public SpecialCardData SpecialCardReward => specialCardReward;
}

