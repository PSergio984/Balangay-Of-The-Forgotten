using System.Collections;
using UnityEngine;

/* DAMAGE SYSTEM DOCUMENTATION
 * 
 * Purpose: Handles hurting characters and showing damage effects
 * 
 * How it works:
 * - Takes damage actions and applies the damage to targets
 * - Shows visual effects when damage happens (like sparks or blood)
 * - Finds the right position to show effects on characters
 * - Waits between multiple targets for better visual timing
 * 
 * Integration: Works with ActionSystem for damage actions, CombatantView for health changes
 */

/// <summary>
/// System that handles all damage dealing and visual effects when characters get hurt
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Makes characters take damage and shows cool effects when it happens</para>
/// 
/// <para><strong>What it does:</strong> This system takes damage actions (like when enemies 
/// attack or cards deal damage) and actually hurts the targets. It also spawns visual 
/// effects like sparks or hit effects right where the character got hurt to make 
/// combat feel more exciting.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Something creates a damage action (enemy attack, card effect, etc.)</item>
/// <item>This system receives the damage action</item>
/// <item>System reduces health of all targets in the action</item>
/// <item>System spawns visual effects at each target's position</item>
/// <item>Waits a bit between targets for better visual timing</item>
/// </list>
/// 
/// <para><strong>Needs:</strong> ActionSystem for receiving damage actions, damage effect prefab</para>
/// 
/// <para><strong>Works with:</strong> ActionSystem for processing, CombatantView for health, visual effects</para>
/// 
/// <para><strong>How to use:</strong> Put this on a GameObject and assign a damage effect prefab in Inspector</para>
/// </remarks>
// System that handles all damage calculations and visual effects in the game
// This processes damage actions and applies them to targets with visual feedback
public class DamageSystem : MonoBehaviour
{
    /// <summary>
    /// Visual effect that appears when damage is dealt to show the hit
    /// </summary>
    /// <remarks>
    /// This is a prefab that gets spawned whenever someone takes damage.
    /// Could be sparks, blood, magic effects, or any visual that shows a hit.
    /// Assign a GameObject prefab with visual effects in the Inspector.
    /// </remarks>
    // Visual effect prefab that plays when damage is dealt (like hit sparks, blood, etc.)
    [SerializeField] private GameObject damageVFX;
    
    /// <summary>
    /// Sets up damage handling when this system turns on
    /// </summary>
    /// <remarks>
    /// Unity calls this automatically when the GameObject becomes active.
    /// Tells the action system that this component can handle damage actions.
    /// </remarks>
    // Called when this GameObject becomes active - registers damage handling methods
    void OnEnable()
    {
        // Register this system to handle DealDamageGA actions when they occur
        ActionSystem.AttachPerformer<DealDamageGA>(DealDamagePerformer);
    }

    /// <summary>
    /// Cleans up damage handling when this system turns off
    /// </summary>
    /// <remarks>
    /// Unity calls this automatically when the GameObject becomes inactive.
    /// Stops handling damage actions to prevent memory problems.
    /// </remarks>
    // Called when this GameObject is disabled - unregisters to prevent memory leaks
    void OnDisable()
    {
        // Unregister the damage handling method
        ActionSystem.DetachPerformer<DealDamageGA>();
    }
    
    /// <summary>
    /// Takes a damage action and actually hurts all the targets
    /// </summary>
    /// <param name="dealDamageGA">The damage action containing who to hurt and how much</param>
    /// <returns>Waits between targets and one frame at the end</returns>
    /// <remarks>
    /// This is the main method that processes damage. It goes through each target,
    /// reduces their health, and shows a visual effect. Waits between targets so
    /// players can see each hit clearly instead of everything happening at once.
    /// </remarks>
    // The main method that processes damage actions and applies damage to targets
    private IEnumerator DealDamagePerformer(DealDamageGA dealDamageGA)
    {
        // Loop through every target that should receive damage
        foreach (var target in dealDamageGA.Targets)
        {
            // Apply the damage amount to this target (reduces their health)
            target.Damage(dealDamageGA.Amount);

            // Find the sprite renderer to get the correct visual position
            SpriteRenderer spriteRenderer = target.GetComponentInChildren<SpriteRenderer>();
            Vector3 vfxPosition = spriteRenderer != null ? spriteRenderer.transform.position : target.transform.position;

            // Spawn a visual effect at the sprite's position to show damage was dealt
            Instantiate(damageVFX, vfxPosition, Quaternion.identity);
            // Wait 0.15 seconds before damaging the next target (for visual timing)
            yield return new WaitForSeconds(0.15f);
            
            /// <summary>
            /// Check if the target died from the damage and handle death
            /// </summary>
            /// <remarks>
            /// After dealing damage, check if the target's health reached zero or below.
            /// If it's an enemy that died, create a KillEnemyGA action to remove them.
            /// Hero death handling is planned for future implementation.
            /// </remarks>
            // Check if the target died from the damage (health reached zero or below)
            if(target.CurrentHealth <= 0)
                {
                // If the target is an enemy that died, create a kill enemy action
                if (target is EnemyView enemyView)
                {
                    // Create action to kill and remove the defeated enemy
                    KillEnemyGA killEnemyGA = new(enemyView);
                    // Add the kill action to be processed after damage
                    ActionSystem.Instance.AddReaction(killEnemyGA);
                }
                else
                {
                    //nothing here for now
                    //handles heroes death        
                }
                }
        }
        // Wait one frame before continuing (required for coroutines)
        yield return null;
    }
}
