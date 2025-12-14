using System.Collections;
using UnityEngine;

/* HEAL SYSTEM DOCUMENTATION
 * 
 * Purpose: Handles healing characters and showing healing effects
 * 
 * How it works:
 * - Takes heal actions and applies healing to targets
 * - Shows visual effects when healing happens (like green particles or sparkles)
 * - Finds the right position to show effects on characters
 * - Waits between multiple targets for better visual timing
 * 
 * Integration: Works with ActionSystem for heal actions, CombatantView for health changes
 */

/// <summary>
/// System that handles all healing and visual effects when characters are healed
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Restores health to characters and shows healing effects</para>
/// 
/// <para><strong>What it does:</strong> This system takes healing actions (like when cards 
/// heal or abilities restore health) and actually restores the targets' health. It also 
/// spawns visual effects like green particles or healing sparkles right where the character 
/// got healed to make healing feel satisfying.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Something creates a heal action (card effect, ability, etc.)</item>
/// <item>This system receives the heal action</item>
/// <item>System restores health of all targets in the action</item>
/// <item>System spawns visual effects at each target's position</item>
/// <item>Waits a bit between targets for better visual timing</item>
/// </list>
/// 
/// <para><strong>Needs:</strong> ActionSystem for receiving heal actions, heal effect prefab (optional)</para>
/// 
/// <para><strong>Works with:</strong> ActionSystem for processing, CombatantView for health, visual effects</para>
/// 
/// <para><strong>How to use:</strong> Put this on a GameObject in the scene (can be on the same GameObject as DamageSystem)</para>
/// </remarks>
// System that handles all healing calculations and visual effects in the game
// This processes heal actions and applies them to targets with visual feedback
public class HealSystem : MonoBehaviour
{
    /// <summary>
    /// Visual effect that appears when healing is applied to show the heal
    /// </summary>
    /// <remarks>
    /// This is a prefab that gets spawned whenever someone is healed.
    /// Could be green particles, sparkles, or any visual that shows healing.
    /// Assign a GameObject prefab with visual effects in the Inspector (optional).
    /// </remarks>
    // Visual effect prefab that plays when healing is applied (like green particles, sparkles, etc.)
    [SerializeField] private GameObject healVFX;
    
    /// <summary>
    /// Sets up healing handling when this system turns on
    /// </summary>
    /// <remarks>
    /// Unity calls this automatically when the GameObject becomes active.
    /// Tells the action system that this component can handle heal actions.
    /// </remarks>
    // Called when this GameObject becomes active - registers heal handling methods
    void OnEnable()
    {
        // Register this system to handle HealGA actions when they occur
        ActionSystem.AttachPerformer<HealGA>(HealPerformer);
    }

    /// <summary>
    /// Cleans up healing handling when this system turns off
    /// </summary>
    /// <remarks>
    /// Unity calls this automatically when the GameObject becomes inactive.
    /// Stops handling heal actions to prevent memory problems.
    /// </remarks>
    // Called when this GameObject is disabled - unregisters to prevent memory leaks
    void OnDisable()
    {
        // Unregister the heal handling method
        ActionSystem.DetachPerformer<HealGA>();
    }
    
    /// <summary>
    /// Takes a heal action and actually heals all the targets
    /// </summary>
    /// <param name="healGA">The heal action containing who to heal and how much</param>
    /// <returns>Waits between targets and one frame at the end</returns>
    /// <remarks>
    /// This is the main method that processes healing. It goes through each target,
    /// restores their health, and shows a visual effect. Waits between targets so
    /// players can see each heal clearly instead of everything happening at once.
    /// </remarks>
    // The main method that processes heal actions and applies healing to targets
    private IEnumerator HealPerformer(HealGA healGA)
    {
        // Defensive check: Ensure heal action is valid
        if (healGA == null)
        {
            Debug.LogError("[HealSystem] HealGA is null! Cannot process healing.");
            yield return null;
            yield break;
        }

        // Defensive check: Ensure heal targets list is valid
        if (healGA.HealTargets == null || healGA.HealTargets.Count == 0)
        {
            Debug.LogWarning("[HealSystem] HealGA has no heal targets! Nothing to heal.");
            yield return null;
            yield break;
        }

        Debug.Log($"[HealSystem] Processing heal action with {healGA.HealTargets.Count} target(s)");
        
        // Loop through every target that should receive healing
        foreach (var healTarget in healGA.HealTargets)
        {
            // Skip null targets
            if (healTarget.Target == null)
            {
                Debug.LogWarning("[HealSystem] Skipping null target in heal action");
                continue;
            }

            // Skip if heal amount is 0 or negative
            if (healTarget.Amount <= 0f)
            {
                Debug.Log($"[HealSystem] Skipping {healTarget.Target.name} - heal amount is {healTarget.Amount}");
                continue;
            }

            // Find the sprite renderer to get the correct visual position
            SpriteRenderer spriteRenderer = healTarget.Target.GetComponentInChildren<SpriteRenderer>();
            Vector3 popupPosition = spriteRenderer != null ? spriteRenderer.transform.position : healTarget.Target.transform.position;

            // Show healing popup (green/positive number)
            int healAmount = Mathf.RoundToInt(healTarget.Amount);
            // Use green color for healing popups
            Color healColor = new Color(0.2f, 1f, 0.2f); // Bright green
            DamagePopUp.CreateTextPopUp(popupPosition, $"+{healAmount}", healColor, DamagePopUp.PopUpAnimationMode.ScaleAndFade, 1.0f);

            // Use CombatVFXManager for enhanced VFX/SFX if available
            if (CombatVFXManager.Instance != null)
            {
                CombatVFXManager.Instance.PlayHealEffect(popupPosition);
            }
            else if (healVFX != null)
            {
                // Fallback: use the old instantiation method with auto-destruction
                GameObject spawnedVFX = Instantiate(healVFX, popupPosition, Quaternion.identity);
                if (spawnedVFX != null)
                {
                    // Auto-destroy VFX after default duration (matches CombatVFXManager behavior)
                    float vfxDuration = 2.0f; // Default duration before destroying VFX
                    Destroy(spawnedVFX, vfxDuration);
                }
            }

            // Apply the healing amount to this target (restores their health)
            healTarget.Target.Heal(healAmount);

            Debug.Log($"[HealSystem] {healTarget.Target.name} healed for {healAmount} HP (now at {healTarget.Target.CurrentHealth}/{healTarget.Target.MaxHealth})");

            // Wait a bit between targets for better visual timing
            yield return new WaitForSeconds(0.3f);
        }
        
        // Wait one frame before continuing (required for coroutines)
        yield return null;
    }
}
