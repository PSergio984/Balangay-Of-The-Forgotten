using UnityEngine;

/* COMBATANT ANIMATION CONTROLLER DOCUMENTATION
 * 
 * Purpose: Manages animation state transitions for all combatants (heroes and enemies)
 * 
 * How it works:
 * - Interfaces with Unity's Animator component to trigger animation states
 * - Uses cached StringToHash for performance (avoids string allocations)
 * - Provides both specific methods (PlayIdle, PlayAttack) and generic SetState method
 * - Validates animator presence and logs warnings if missing
 * - Supports animation events for callbacks (VFX, SFX, attack timing)
 * 
 * Integration: Used by CombatantView as a component, triggered by combat systems
 */

/// <summary>
/// Animation states available for all combatants in combat
/// </summary>
/// <remarks>
/// Defines all possible animation states that characters can be in during combat.
/// Used for type-safe animation triggering and state management.
/// Maps directly to Animator Controller parameters.
/// </remarks>
public enum CombatantAnimState
{
    /// <summary>Default standing/waiting animation</summary>
    Idle,
    /// <summary>Attack animation when dealing damage</summary>
    Attack,
    /// <summary>Hit/hurt animation when taking damage</summary>
    Hit,
    /// <summary>Death/defeat animation when health reaches zero</summary>
    Dead,
    /// <summary>Victory/celebration animation when winning</summary>
    Victory,
    /// <summary>Casting/channeling animation for magic abilities</summary>
    Cast,
    /// <summary>Defending/blocking animation</summary>
    Defend
}

/// <summary>
/// Controls animation state transitions for a combatant character
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Centralized animation control for combat characters</para>
/// 
/// <para><strong>What it does:</strong> This component manages all animation state changes
/// for a combatant. It communicates with Unity's Animator component to trigger the right
/// animations at the right time. Uses cached animator hashes for performance and provides
/// both specific methods (like PlayAttack) and a generic SetState method for flexibility.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Component is attached to same GameObject as CombatantView</item>
/// <item>Validates and caches Animator component reference in Awake</item>
/// <item>Combat systems call PlayAttack(), PlayHit(), etc. when events occur</item>
/// <item>Controller triggers the appropriate animator parameter</item>
/// <item>Animator Controller handles transitions between animation states</item>
/// </list>
/// 
/// <para><strong>Performance:</strong> Uses StringToHash to cache animator parameter IDs,
/// avoiding string allocations and lookups during gameplay.</para>
/// 
/// <para><strong>Extensibility:</strong> Easy to add new states by:
/// 1. Adding to CombatantAnimState enum
/// 2. Adding cached hash field
/// 3. Adding case to SetState switch
/// 4. Adding specific method if desired</para>
/// 
/// <para><strong>Works with:</strong> CombatantView for integration, DamageSystem for hit/death,
/// card/ability systems for attack/cast animations, Animator Controller for state machine</para>
/// </remarks>
public class CombatantAnimationController : MonoBehaviour
{
    /// <summary>
    /// Reference to the Unity Animator component that plays animations
    /// </summary>
    /// <remarks>
    /// Assigned in Inspector or found automatically in Awake.
    /// Should reference an Animator with a controller that has the required parameters.
    /// </remarks>
    [SerializeField] private Animator animator;
    
    // --- Cached Animator Hashes for Performance ---
    // Using StringToHash avoids string allocations and improves performance
    // These are static because hash values are the same across all instances
    private static readonly int IdleHash = Animator.StringToHash("Idle");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int DeadHash = Animator.StringToHash("Dead");
    private static readonly int VictoryHash = Animator.StringToHash("Victory");
    private static readonly int CastHash = Animator.StringToHash("Cast");
    private static readonly int DefendHash = Animator.StringToHash("Defend");
    
    /// <summary>
    /// Validates and caches component references
    /// </summary>
    /// <remarks>
    /// Called automatically by Unity when component initializes.
    /// Finds Animator component if not assigned in Inspector.
    /// Logs warning if animator is missing to help with debugging.
    /// </remarks>
    private void Awake()
    {
        // If animator not assigned in Inspector, try to find it
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // Validate animator presence for debugging
        if (animator == null)
        {
            Debug.LogWarning($"[CombatantAnimationController] No Animator found on {gameObject.name}. Animations will not play.", this);
        }
    }

    /// <summary>
    /// Assigns a new AnimatorOverrideController instance to the Animator at runtime.
    /// </summary>
    /// <param name="overrideController">The override controller to assign (from HeroData)</param>
    public void SetAnimatorOverride(AnimatorOverrideController overrideController)
    {
        if (animator != null && overrideController != null)
        {
            animator.runtimeAnimatorController = overrideController;
            Debug.Log($"[CombatantAnimationController] AnimatorOverrideController set at runtime: {overrideController.name} (Base: {overrideController.runtimeAnimatorController?.name})", animator);
            // Force rebind to ensure Animator uses the new override controller
            animator.Rebind();
        }
        else
        {
            if (animator == null)
            {
                Debug.LogWarning($"[CombatantAnimationController] Cannot assign override: Animator reference is null on {gameObject.name}", this);
            }
            if (overrideController == null)
            {
                Debug.LogWarning($"[CombatantAnimationController] Cannot assign override: OverrideController is null on {gameObject.name}", this);
            }
        }
    }
    
    // --- Public Animation Trigger Methods ---
    
    /// <summary>
    /// Triggers the idle/default animation state
    /// </summary>
    /// <remarks>
    /// Call when character is not performing any action.
    /// Default state for characters waiting for their turn.
    /// </remarks>
    public void PlayIdle()
    {
        if (animator != null)
        {
            animator.SetTrigger(IdleHash);
        }
    }
    
    /// <summary>
    /// Triggers the attack animation
    /// </summary>
    /// <remarks>
    /// Call when character performs an attack action.
    /// Should be called before or during damage application.
    /// Consider using animation events to time damage application with animation.
    /// </remarks>
    public void PlayAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger(AttackHash);
        }
    }
    
    /// <summary>
    /// Triggers the hit/hurt animation
    /// </summary>
    /// <remarks>
    /// Call when character takes damage.
    /// Works with damage shake effect for impactful feedback.
    /// Should be brief so gameplay continues smoothly.
    /// </remarks>
    public void PlayHit()
    {
        if (animator != null)
        {
            animator.SetTrigger(HitHash);
        }
    }
    
    /// <summary>
    /// Triggers the death/defeat animation
    /// </summary>
    /// <remarks>
    /// Call when character's health reaches zero.
    /// Should transition to a final state (not loop).
    /// Character typically becomes inactive after this animation.
    /// </remarks>
    public void PlayDead()
    {
        if (animator != null)
        {
            animator.SetTrigger(DeadHash);
        }
    }
    
    /// <summary>
    /// Triggers the victory/celebration animation
    /// </summary>
    /// <remarks>
    /// Call when combat is won.
    /// Optional state for polish and player satisfaction.
    /// </remarks>
    public void PlayVictory()
    {
        if (animator != null)
        {
            animator.SetTrigger(VictoryHash);
        }
    }
    
    /// <summary>
    /// Triggers the casting/channeling animation
    /// </summary>
    /// <remarks>
    /// Call when character casts magic or special abilities.
    /// Differentiates magic from physical attacks visually.
    /// </remarks>
    public void PlayCast()
    {
        if (animator != null)
        {
            animator.SetTrigger(CastHash);
        }
    }
    
    /// <summary>
    /// Triggers the defend/block animation
    /// </summary>
    /// <remarks>
    /// Call when character uses defensive abilities.
    /// Shows preparation or reaction to incoming attacks.
    /// </remarks>
    public void PlayDefend()
    {
        if (animator != null)
        {
            animator.SetTrigger(DefendHash);
        }
    }
    
    /// <summary>
    /// Generic method to set animation state by enum value
    /// </summary>
    /// <param name="state">The animation state to trigger</param>
    /// <remarks>
    /// <para><strong>Use case:</strong> When state is determined dynamically or from data</para>
    /// <para><strong>Benefits:</strong></para>
    /// <list type="bullet">
    /// <item>Single method for all state changes</item>
    /// <item>Useful for data-driven animation systems</item>
    /// <item>Works well with state machines and AI</item>
    /// </list>
    /// <para><strong>Alternative:</strong> Call specific methods (PlayAttack, etc.) when state is known at compile time</para>
    /// </remarks>
    public void SetState(CombatantAnimState state)
    {
        if (animator == null) return;
        
        switch (state)
        {
            case CombatantAnimState.Idle:
                PlayIdle();
                break;
            case CombatantAnimState.Attack:
                PlayAttack();
                break;
            case CombatantAnimState.Hit:
                PlayHit();
                break;
            case CombatantAnimState.Dead:
                PlayDead();
                break;
            case CombatantAnimState.Victory:
                PlayVictory();
                break;
            case CombatantAnimState.Cast:
                PlayCast();
                break;
            case CombatantAnimState.Defend:
                PlayDefend();
                break;
            default:
                Debug.LogWarning($"[CombatantAnimationController] Unknown animation state: {state}", this);
                break;
        }
    }
    
    // --- Animation Event Callbacks (Optional) ---
    // These methods can be called from animation events in the Animator
    // to synchronize code execution with specific animation frames
    
    /// <summary>
    /// Called by animation event when attack should deal damage
    /// </summary>
    /// <remarks>
    /// Add this as an animation event in your attack animation at the moment
    /// the weapon/attack should connect with the target.
    /// Allows precise timing of damage application with visual.
    /// </remarks>
    public void OnAttackImpact()
    {
        // Optional: Trigger damage here if using animation events for timing
        // This would require storing the pending damage action
        Debug.Log($"[CombatantAnimationController] Attack impact frame on {gameObject.name}");
    }
    
    /// <summary>
    /// Called by animation event when animation completes
    /// </summary>
    /// <remarks>
    /// Use this to signal when an animation finishes if you need to
    /// wait for animations to complete before continuing combat flow.
    /// </remarks>
    public void OnAnimationComplete()
    {
        Debug.Log($"[CombatantAnimationController] Animation completed on {gameObject.name}");
    }
}
