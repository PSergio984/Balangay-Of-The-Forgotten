using UnityEngine;
using System.Collections.Generic;

/* ENEMY BOARD VIEW DOCUMENTATION
 * 
 * Purpose: Manages where enemies appear on the battlefield
 * 
 * How it works:
 * - Has predefined slots where enemies can be placed
 * - Creates and positions enemy views in the correct spots
 * - Keeps track of all enemies currently on the board
 * - Works like a grid system for enemy placement
 * 
 * Integration: Used by EnemySystem to organize enemy placement, works with EnemyViewCreator
 */

/// <summary>
/// Manages the placement and organization of enemies on the battlefield
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Controls where enemies appear and how they're arranged on screen</para>
/// 
/// <para><strong>What it does:</strong> This is like the battlefield manager for enemies. 
/// It has predefined slots (positions) where enemies can be placed, and when new enemies 
/// are added to the battle, it puts them in the next available slot. It keeps everything 
/// organized so enemies don't overlap and appear in the right positions.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Has a list of Transform slots (positions) for enemy placement</item>
/// <item>When adding an enemy, uses the next available slot</item>
/// <item>Creates the enemy view at the slot's position and rotation</item>
/// <item>Parents the enemy to the slot for organization</item>
/// <item>Keeps a list of all active enemies for easy access</item>
/// </list>
/// 
/// <para><strong>Example:</strong> If you have 3 enemy slots, enemies will appear in slot 1, 
/// then slot 2, then slot 3 as they're added to the battle.</para>
/// 
/// <para><strong>Works with:</strong> EnemySystem for enemy management, EnemyViewCreator for spawning</para>
/// 
/// <para><strong>How to use:</strong> Set up Transform slots in Inspector, EnemySystem calls AddEnemy()</para>
/// </remarks>
public class EnemyBoardView : MonoBehaviour
{
    /// <summary>
    /// List of positions where enemies can be placed on the battlefield
    /// </summary>
    /// <remarks>
    /// These are Transform objects that define where each enemy should appear.
    /// Set these up in the Inspector to control enemy positioning and spacing.
    /// </remarks>
    [SerializeField] private List<Transform> slots;
    
    /// <summary>
    /// List of all enemy views currently active on the battlefield
    /// </summary>
    /// <remarks>
    /// Keeps track of every enemy that's been added to the board.
    /// Other systems can use this to find all enemies for targeting, effects, etc.
    /// </remarks>
    public List<EnemyView> EnemyViews { get; private set; } = new();
    
    /// <summary>
    /// Adds a new enemy to the battlefield in the next available slot
    /// </summary>
    /// <param name="enemyData">Data containing enemy stats, appearance, and behavior</param>
    /// <remarks>
    /// Creates a new enemy view using the EnemyViewCreator, positions it in the next 
    /// available slot, and adds it to the list of active enemies. The enemy gets 
    /// parented to the slot for proper organization in the hierarchy.
    /// </remarks>
    public void AddEnemy(EnemyData enemyData)
    {
        // Get the next available slot based on how many enemies we already have
        Transform slot = slots[EnemyViews.Count];
        // Create a new enemy view at the slot's position and rotation
        EnemyView enemyView = EnemyViewCreator.Instance.CreateEnemyView(enemyData, slot.position, slot.rotation);
        // Make the enemy a child of the slot for organization
        enemyView.transform.parent = slot;
        // Add the new enemy to our list of active enemies
        EnemyViews.Add(enemyView);
    }
}
