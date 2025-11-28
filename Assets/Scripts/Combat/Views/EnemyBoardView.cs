using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;

/* ENEMY BOARD VIEW DOCUMENTATION
 * 
 * Purpose: Manages where enemies appear on the battlefield
 * 
 * How it works:
 * - Has predefined slots where enemies can be placed
 * - Creates and positions enemy views in the correct spots
 * - Keeps track of all enemies currently on the board
 * - Works like a grid system for enemy placement
 * - Assigns health bar UI sliders to each enemy at runtime
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
/// organized so enemies don't overlap and appear in the right positions. Additionally,
/// it manages the assignment of health bar UI sliders to each enemy at runtime.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Has a list of Transform slots (positions) for enemy placement</item>
/// <item>Has a list of UI Slider components for enemy health bars</item>
/// <item>When adding an enemy, uses the next available slot and health bar</item>
/// <item>Creates the enemy view at the slot's position and rotation</item>
/// <item>Assigns the corresponding health bar to the enemy at runtime</item>
/// <item>Parents the enemy to the slot for organization</item>
/// <item>Keeps a list of all active enemies for easy access</item>
/// </list>
/// 
/// <para><strong>Example:</strong> If you have 3 enemy slots and 3 health bars, enemies will 
/// appear in slot 1 with health bar 1, slot 2 with health bar 2, and slot 3 with health bar 3.</para>
/// 
/// <para><strong>Works with:</strong> EnemySystem for enemy management, EnemyViewCreator for spawning</para>
/// 
/// <para><strong>How to use:</strong> Set up Transform slots and health bar Sliders in Inspector, 
/// EnemySystem calls AddEnemy() which automatically handles health bar assignment</para>
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
    /// List of health bar UI sliders for each enemy position
    /// </summary>
    /// <remarks>
    /// <para><strong>Purpose:</strong> UI Slider components that display each enemy's health</para>
    /// <para><strong>Setup:</strong> Assign in Inspector - one slider for each enemy slot</para>
    /// <para><strong>Runtime:</strong> Automatically assigned to enemies when they spawn</para>
    /// <para><strong>Important:</strong> List order must match the slots list order</para>
    /// <para><strong>Example:</strong> slots[0] pairs with healthBarSliders[0], etc.</para>
    /// </remarks>
    [SerializeField] private List<Slider> healthBarSliders;

    /// <summary>
    /// List of health bar fill images for each enemy position
    /// </summary>
    /// <remarks>
    /// <para><strong>Purpose:</strong> Image components for health bar color (green to red gradient)</para>
    /// <para><strong>Setup:</strong> Assign in Inspector - one fill image for each enemy slot</para>
    /// <para><strong>Important:</strong> List order must match slots and sliders list order</para>
    /// </remarks>
    [SerializeField] private List<Image> healthBarFills;

    /// <summary>
    /// List of health text components for each enemy position
    /// </summary>
    /// <remarks>
    /// <para><strong>Purpose:</strong> Text components displaying health values (e.g., "50/100")</para>
    /// <para><strong>Setup:</strong> Assign in Inspector - one text for each enemy slot</para>
    /// <para><strong>Important:</strong> List order must match slots list order</para>
    /// </remarks>
    [SerializeField] private List<TMPro.TMP_Text> healthBarTexts;

    /// <summary>
    /// List of name text components for each enemy position
    /// </summary>
    /// <remarks>
    /// <para><strong>Purpose:</strong> Text components displaying enemy names</para>
    /// <para><strong>Setup:</strong> Assign in Inspector - one text for each enemy slot</para>
    /// <para><strong>Important:</strong> List order must match slots list order</para>
    /// </remarks>
    [SerializeField] private List<TMPro.TMP_Text> nameTexts;

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
    /// <para><strong>What it does:</strong> Creates a new enemy view using the EnemyViewCreator, 
    /// positions it in the next available slot, assigns the corresponding health bar UI, 
    /// and adds it to the list of active enemies.</para>
    /// 
    /// <para><strong>Runtime Health Bar Assignment:</strong> After creating the enemy, this method 
    /// automatically calls AssignHealthBar() to link the UI slider to the enemy. This ensures 
    /// the health bar displays correctly immediately after the enemy spawns.</para>
    /// 
    /// <para><strong>Validation:</strong> Includes safety checks to handle missing health bars 
    /// gracefully with debug warnings while allowing combat to continue.</para>
    /// </remarks>
    public void AddEnemy(EnemyData enemyData)
    {
        // Calculate the index for the next enemy position
        int enemyIndex = EnemyViews.Count;
        
        // Validate that we have an available slot
        if (enemyIndex >= slots.Count)
        {
            Debug.LogError($"[EnemyBoardView] Cannot add enemy: no available slots! Current enemies: {enemyIndex}, Available slots: {slots.Count}");
            return;
        }
        
        // Get the next available slot based on how many enemies we already have
        Transform slot = slots[enemyIndex];
        
        // Create a new enemy view at the slot's position and rotation
        EnemyView enemyView = EnemyViewCreator.Instance.CreateEnemyView(enemyData, slot.position, slot.rotation);
        
        // Make the enemy a child of the slot for organization
        enemyView.transform.parent = slot;
        
        // Add the new enemy to our list of active enemies
        EnemyViews.Add(enemyView);
        
        // --- RUNTIME HEALTH BAR ASSIGNMENT ---
        // Assign all health bar UI components to this enemy at runtime
        // This happens AFTER Setup() is called, so MaxHealth and CurrentHealth are already initialized
        if (healthBarSliders != null && enemyIndex < healthBarSliders.Count &&
            healthBarFills != null && enemyIndex < healthBarFills.Count &&
            healthBarTexts != null && enemyIndex < healthBarTexts.Count &&
            nameTexts != null && enemyIndex < nameTexts.Count)
        {
            Slider healthBarSlider = healthBarSliders[enemyIndex];
            Image healthBarFill = healthBarFills[enemyIndex];
            TMPro.TMP_Text healthBarText = healthBarTexts[enemyIndex];
            TMPro.TMP_Text nameText = nameTexts[enemyIndex];
            
            if (healthBarSlider != null && healthBarFill != null && healthBarText != null && nameText != null)
            {
                // Assign all health bar components to the enemy view
                enemyView.AssignHealthBar(healthBarSlider, healthBarFill, healthBarText, nameText);
                Debug.Log($"[EnemyBoardView] Assigned health bar components {enemyIndex} to enemy '{enemyData.EnemyName}'");
            }
            else
            {
                Debug.LogWarning($"[EnemyBoardView] One or more health bar components at index {enemyIndex} are null! Enemy '{enemyData.EnemyName}' will not have a complete health bar.");
            }
        }
        else
        {
            Debug.LogWarning($"[EnemyBoardView] Not enough health bar components for enemy at index {enemyIndex}. Assign health bar sliders, fills, texts, and name texts in Inspector or enemy '{enemyData.EnemyName}' will not display health.");
        }
    }
    
    /// <summary>
    /// Removes an enemy from the battlefield with smooth scaling animation
    /// </summary>
    /// <param name="enemyView">The enemy view to remove from the battlefield</param>
    /// <returns>Coroutine that completes when the removal animation finishes</returns>
    /// <remarks>
    /// Removes an enemy from the battlefield with a nice visual effect. First removes 
    /// the enemy from the active list, then plays a shrinking animation where the enemy 
    /// scales down to zero over 0.25 seconds. After the animation completes, destroys 
    /// the enemy GameObject. This creates a satisfying death/removal effect.
    /// </remarks>
    public IEnumerator RemoveEnemy(EnemyView enemyView)
    {
        // Remove the enemy from our active enemies list
        EnemyViews.Remove(enemyView);
        // Create a scaling animation that shrinks the enemy to zero size over 0.25 seconds
        Tween tween = enemyView.transform.DOScale(Vector3.zero, 0.25f);
        // Wait for the scaling animation to complete
        yield return tween.WaitForCompletion();
        // Destroy the enemy GameObject after the animation finishes
        Destroy(enemyView.gameObject);
    }
}
