using System.Collections; // Import for IEnumerator and coroutines
using UnityEngine; // Import Unity engine functionality

// System that handles all damage calculations and visual effects in the game
// This processes damage actions and applies them to targets with visual feedback
public class DamageSystem : MonoBehaviour
{
    // Visual effect prefab that plays when damage is dealt (like hit sparks, blood, etc.)
    [SerializeField] private GameObject damageVFX;
    // Called when this GameObject becomes active - registers damage handling methods
    void OnEnable()
    {
        // Register this system to handle DealDamageGA actions when they occur
        ActionSystem.AttachPerformer<DealDamageGA>(DealDamagePerformer);
    }

    // Called when this GameObject is disabled - unregisters to prevent memory leaks
    void OnDisable()
    {
        // Unregister the damage handling method
        ActionSystem.DetachPerformer<DealDamageGA>();
    }
    // The main method that processes damage actions and applies damage to targets
    private IEnumerator DealDamagePerformer(DealDamageGA dealDamageGA)
    {
        // Loop through every target that should receive damage
        foreach (var target in dealDamageGA.Targets)
        {
            // Apply the damage amount to this target (reduces their health)
            target.Damage(dealDamageGA.Amount);
            // Spawn a visual effect at the target's position to show damage was dealt
            Instantiate(damageVFX, target.transform.position, Quaternion.identity);
            // Wait 0.15 seconds before damaging the next target (for visual timing)
            yield return new WaitForSeconds(0.15f);
        }
        // Wait one frame before continuing (required for coroutines)
        yield return null;
    }
}
