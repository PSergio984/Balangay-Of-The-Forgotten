using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using DG.Tweening;

/* ENEMY BOARD VIEW DOCUMENTATION
 * 
 * Purpose: Manages where enemies appear on the battlefield (SEQUENTIAL SPAWNING MODE)
 * 
 * How it works:
 * - SEQUENTIAL MODE: Only one enemy exists at a time, always in slot 0
 * - Creates and positions each enemy view in the same slot position
 * - Reuses slot 0 and health bar UI for each sequential enemy
 * - Tracks the single active enemy on the board
 * - Assigns health bar UI from slot 0 to each enemy at runtime
 * 
 * Sequential Spawning Flow:
 * 1. Enemy 1 spawns in slot 0 with slot 0 health bar
 * 2. Player defeats Enemy 1
 * 3. Enemy 1 is removed from slot 0
 * 4. Enemy 2 spawns in slot 0 with slot 0 health bar (reused)
 * 5. Repeat until all enemies defeated
 * 
 * Integration: Used by EnemySystem to organize sequential enemy placement, works with EnemyViewCreator
 */

/// <summary>
/// Manages the placement and organization of enemies on the battlefield (SEQUENTIAL MODE)
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Controls where enemies appear in sequential wave-based combat</para>
/// 
/// <para><strong>SEQUENTIAL SPAWNING MODE:</strong> This system now operates in sequential mode where 
/// only ONE enemy exists at a time. Each enemy spawns in the same slot (slot 0) and reuses 
/// the same health bar UI. When one enemy is defeated, the next enemy spawns in the same position 
/// with the same UI elements, creating a wave-based combat experience.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Always uses slot 0 (the first slot) for enemy placement</item>
/// <item>Always uses health bar UI components from index 0</item>
/// <item>When adding an enemy, places it in slot 0 regardless of previous enemies</item>
/// <item>Reuses the same UI elements (slider, fill, text) for each sequential enemy</item>
/// <item>Only tracks one active enemy at a time in the EnemyViews list</item>
/// <item>Creates smooth wave-based combat flow</item>
/// </list>
/// 
/// <para><strong>Example:</strong> You have 3 enemies in the queue. Enemy 1 spawns in slot 0 with 
/// health bar 0. After defeat, Enemy 2 spawns in slot 0 with health bar 0 (same UI reused). 
/// After defeat, Enemy 3 spawns in slot 0 with health bar 0 (same UI reused again).</para>
/// 
/// <para><strong>Works with:</strong> EnemySystem for sequential enemy management, EnemyViewCreator for spawning</para>
/// 
/// <para><strong>How to use:</strong> Set up at least one Transform slot and one set of health bar UI 
/// components in Inspector. EnemySystem will automatically spawn enemies sequentially using slot 0.</para>
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
    /// Adds a new enemy to the battlefield in slot 0 (sequential spawning mode)
    /// </summary>
    /// <param name="enemyData">Data containing enemy stats, appearance, and behavior</param>
    /// <remarks>
    /// <para><strong>SEQUENTIAL SPAWNING MODE:</strong> This method always uses slot 0 (the first slot)
    /// for enemy placement. Since only one enemy exists at a time in sequential mode, we always reuse
    /// the same slot and health bar UI for each new enemy.</para>
    /// 
    /// <para><strong>Runtime Health Bar Assignment:</strong> Always assigns health bar components from index 0
    /// since we're in single-enemy mode. The UI is reused for each sequential enemy.</para>
    /// 
    /// <para><strong>Validation:</strong> Includes safety checks to handle missing health bars 
    /// gracefully with debug warnings while allowing combat to continue.</para>
    /// </remarks>
    public void AddEnemy(EnemyData enemyData)
    {
        // SEQUENTIAL SPAWNING: Always use slot 0 since only one enemy exists at a time
        const int SLOT_INDEX = 0;
        
        // Validate that slot 0 exists
        if (slots == null || slots.Count == 0)
        {
            Debug.LogError($"[EnemyBoardView] Cannot add enemy: no slots configured! Assign enemy slots in Inspector.");
            return;
        }
        
        // Get slot 0 (the primary enemy slot for sequential spawning)
        Transform slot = slots[SLOT_INDEX];
        
        // Create a new enemy view at the slot's position and rotation
        EnemyView enemyView = EnemyViewCreator.Instance.CreateEnemyView(enemyData, slot.position, slot.rotation);
        
        // Make the enemy a child of the slot for organization
        enemyView.transform.parent = slot;
        
        // Add the new enemy to our list of active enemies (should only have 1 in sequential mode)
        EnemyViews.Add(enemyView);
        
        // --- RUNTIME HEALTH BAR ASSIGNMENT (SEQUENTIAL MODE) ---
        // Assign health bar UI components from index 0 (reused for each sequential enemy)
        if (healthBarSliders != null && SLOT_INDEX < healthBarSliders.Count &&
            healthBarFills != null && SLOT_INDEX < healthBarFills.Count &&
            healthBarTexts != null && SLOT_INDEX < healthBarTexts.Count &&
            nameTexts != null && SLOT_INDEX < nameTexts.Count)
        {
            Slider healthBarSlider = healthBarSliders[SLOT_INDEX];
            Image healthBarFill = healthBarFills[SLOT_INDEX];
            TMPro.TMP_Text healthBarText = healthBarTexts[SLOT_INDEX];
            TMPro.TMP_Text nameText = nameTexts[SLOT_INDEX];
            
            if (healthBarSlider != null && healthBarFill != null && healthBarText != null && nameText != null)
            {
                // Assign all health bar components to the enemy view
                enemyView.AssignHealthBar(healthBarSlider, healthBarFill, healthBarText, nameText);
                Debug.Log($"[EnemyBoardView] Assigned health bar components (slot {SLOT_INDEX}) to enemy '{enemyData.EnemyName}'");
            }
            else
            {
                Debug.LogWarning($"[EnemyBoardView] One or more health bar components at slot {SLOT_INDEX} are null! Enemy '{enemyData.EnemyName}' will not have a complete health bar.");
            }
        }
        else
        {
            Debug.LogWarning($"[EnemyBoardView] Health bar components not configured for slot {SLOT_INDEX}. Assign health bar sliders, fills, texts, and name texts in Inspector or enemy '{enemyData.EnemyName}' will not display health.");
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
