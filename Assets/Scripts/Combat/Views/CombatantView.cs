using System.Collections;
using System.Collections.Generic;
using DG.Tweening; // Import DOTween library for smooth animations
using TMPro; // Import TextMeshPro for UI text components
using UnityEngine;
using UnityEngine.UI; // Import Unity engine functionality
/* COMBATANT VIEW DOCUMENTATION
 * 
 * Purpose: Base class for all characters that can fight (heroes, enemies, etc.)
 * 
 * How it works:
 * - Manages health tracking and display for any fighting character
 * - Handles damage effects like screen shake when hurt
 * - Provides visual setup for character appearance and name
 * - Updates UI automatically when health changes
 * 
 * Integration: Base class for HeroView and EnemyView, works with damage system
 */

/// <summary>
/// Base class for any character that can participate in combat
/// </summary>
/// </list>
/// 
/// <para><strong>Features:</strong></para>
/// <list type="bullet">
/// <item>Health management (current and max)</item>
/// <item>Visual damage feedback with DOTween shake</item>
/// <item>Automatic UI updates</item>
/// <item>Character appearance setup</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> DamageSystem for taking damage, DOTween for effects, UI components</para>
/// 
/// <para><strong>How to use:</strong> Inherit from this class for specific character types (HeroView, EnemyView)</para>
/// </remarks>
// Base class for any character that can participate in combat (heroes, enemies, etc.)
// Handles visual representation, health management, and damage effects for all combatants
public class CombatantView : MonoBehaviour
{
    /// <summary>
    /// UI text component that displays the current health points
    /// </summary>
    /// <remarks>
    /// Shows the character's health in format "HP: X" so players can see 
    /// how much health each character has remaining.
    /// For enemies: assigned at runtime from scene health bar.
    /// For heroes: assigned in prefab.
    /// </remarks>
    // UI text component that displays the current health points
    [SerializeField] protected TMP_Text healthText;

    /// <summary>
    /// UI text component that shows the character's name
    /// </summary>
    /// <remarks>
    /// Displays the character's name so players can identify who is who.
    /// Helps distinguish between different enemies or characters.
    /// For enemies: assigned at runtime from scene health bar.
    /// For heroes: assigned in prefab.
    /// </remarks>
    // UI text component that shows the character's name
    [SerializeField] protected TMP_Text NameText;
    

    /// <summary>
    /// UI text component that shows the character's magic power
    /// </summary>
    [SerializeField] private TMP_Text MagicText;
    /// <summary>
    /// UI text component that shows the character's attack power
    /// </summary>
    [SerializeField] private TMP_Text AttackText;

    /// <summary>
    /// UI text component that shows the character's defense stat
    /// </summary>
    [SerializeField] private TMP_Text DefenseText;

    /// <summary>
    /// UI component that manages and displays all status effects for this combatant
    /// </summary>
    /// <remarks>
    /// Shows visual icons and stack counts for all active status effects.
    /// Automatically updates when effects are added, removed, or change stacks.
    /// </remarks>
    [SerializeField] private StatusEffectsUI statusEffectsUI;

    /// <summary>
    /// Dictionary tracking all active status effects and their stack counts
    /// </summary>
    /// <remarks>
    /// Maps each status effect type to its current stack count.
    /// Used for quick lookup during damage calculation and effect management.
    /// </remarks>
    private Dictionary<StatusEffectType, int> statusEffects = new();

    /// <summary>
    /// Animation controller that manages animation state transitions for this combatant
    /// </summary>
    /// <remarks>
    /// Handles all animation triggers and state changes during combat.
    /// If not assigned, animations will not play but combat will still function.
    /// Assign in Inspector or will attempt to find automatically in Awake.
    /// </remarks>
    [SerializeField] protected CombatantAnimationController animationController;

    /// <summary>
    /// Visual component that displays the character's sprite/image
    /// </summary>
    /// <remarks>
    /// Shows the character's visual appearance - their portrait or sprite.
    /// This is what players see to identify the character visually.
    /// </remarks>
    // Visual component that displays the character's sprite/image
    [SerializeField] private SpriteRenderer spriteRenderer;

    /// <summary>
    /// The maximum health this combatant can have (starting health)
    /// </summary>
    /// <remarks>
    /// This is the full health the character starts with and can't exceed.
    /// Used for healing limits and displaying health as a fraction.
    /// </remarks>
    // The maximum health this combatant can have (starting health)
    public int MaxHealth { get; private set; }

    /// <summary>
    /// The current health points this combatant has remaining
    /// </summary>
    /// <remarks>
    /// This goes down when taking damage and up when healing.
    /// When it reaches zero, the character is defeated.
    /// </remarks>
    // The current health points this combatant has remaining
    public int CurrentHealth { get; private set; }

    /// <summary>
    /// Whether this combatant has been defeated (health reached zero)
    /// </summary>
    /// <remarks>
    /// When true, the combatant cannot be targeted, does not process status effects,
    /// and is visually greyed out. Use MarkAsDead() to set this and Revive() to reset.
    /// </remarks>
    public bool IsDead { get; private set; } = false;
    
    /// <summary>
    /// Cached original sprite color for restoring after revival
    /// </summary>
    private Color originalSpriteColor = Color.white;
    
    /// <summary>
    /// Whether the original sprite color has been cached
    /// </summary>
    private bool hasOriginalColor = false;

    /// <summary>
    /// UI Slider component for health bar visualization
    /// </summary>
    /// <remarks>
    /// For enemies: assigned at runtime from scene health bar.
    /// For heroes: assigned in prefab.
    /// </remarks>
    [SerializeField] protected Slider sliderHealth;

    /// <summary>
    /// Gradient for health bar color (green to red)
    /// </summary>
    [SerializeField] protected Gradient gradientHealth;
    
    /// <summary>
    /// Fill image for health bar color
    /// </summary>
    /// <remarks>
    /// For enemies: assigned at runtime from scene health bar.
    /// For heroes: assigned in prefab.
    /// </remarks>
    [SerializeField] protected Image fillHealth;
    
    [Header("Health Bar Animation")]
    [Tooltip("Duration for health bar slider animation")]
    [SerializeField] protected float healthAnimDuration = 0.4f;
    
    [Tooltip("Ease type for health decrease (damage)")]
    [SerializeField] protected Ease healthDecreaseEase = Ease.OutQuint;
    
    [Tooltip("Ease type for health increase (healing)")]
    [SerializeField] protected Ease healthIncreaseEase = Ease.OutBack;
    
    [Tooltip("Enable pulsing animation when health changes")]
    [SerializeField] protected bool enableHealthPulse = true;
    
    [Tooltip("Scale multiplier for health text pulse")]
    [SerializeField] protected float healthPulseScale = 1.2f;

        /// <summary>
        /// The defense stat of this combatant (used in damage reduction)
        /// </summary>
        public float Defense { get; protected set; }

        /// <summary>
        /// The attack power stat of this combatant (used for base damage)
        /// </summary>
        public float AttackPower { get; protected set; }

        /// <summary>
        /// The magic power stat of this combatant (used for magic skills)
        /// </summary>
        public float MagicPower { get; protected set; }
        

    /// <summary>
    /// Validates and caches component references
    /// </summary>
    /// <remarks>
    /// Called automatically by Unity when component initializes.
    /// Finds animation controller if not assigned in Inspector.
    /// </remarks>
    protected virtual void Awake()
    {
        // Eagerly assign the animation controller from the component on this GameObject
        if (animationController == null)
        {
            animationController = GetComponent<CombatantAnimationController>();
        }
        
        if (animationController == null)
        {
            Debug.LogWarning($"[CombatantView] No CombatantAnimationController found on {gameObject.name}. Animations will not play.", this);
        }
    }
    
    /// <summary>
    /// Ensures the animation controller is assigned (call if Setup happens before Awake)
    /// </summary>
    protected void EnsureAnimationController()
    {
        if (animationController == null)
        {
            animationController = GetComponent<CombatantAnimationController>();
        }
    }
    
    // Expose animationController to subclasses (e.g., HeroView)
    protected CombatantAnimationController AnimationController
    {
        get
        {
            // Lazy initialization if not yet assigned
            if (animationController == null)
            {
                animationController = GetComponent<CombatantAnimationController>();
            }
            return animationController;
        }
    }

    /// <summary>
    /// Sets up the basic properties of this combatant (for heroes with prefab UI)
    /// </summary>
    /// <param name="health">Starting health value (becomes both current and max health)</param>
    /// <param name="image">Sprite image to display for this character</param>
    /// <param name="name">Name to display for this character</param>
    /// <param name="magicPower">Magic power stat</param>
    /// <param name="attackPower">Attack power stat</param>
    /// <param name="defense">Defense stat</param>
    /// <remarks>
    /// Called by HeroView to initialize with all UI components assigned in prefab.
    /// For enemies, use SetupBaseWithoutUI instead since UI is assigned at runtime.
    /// </remarks>
    protected void SetupBase(int health, Sprite image, string name, float magicPower, float attackPower, float defense)
    {
        MaxHealth = CurrentHealth = health;
        previousHealth = health; // Initialize previous health
        MagicPower = magicPower;
        AttackPower = attackPower;
        Defense = defense;
        spriteRenderer.sprite = image;
        NameText.text = name;
        MagicText.text = $"MAG: {MagicPower}";
        AttackText.text = $"ATK: {AttackPower}";
        DefenseText.text = $"DEF: {Defense}";
        if (sliderHealth != null)
        {
            sliderHealth.maxValue = MaxHealth;
        }
        SetHealthImmediate(); // Use immediate set for initialization
    }

    /// <summary>
    /// Sets up the basic properties of this combatant without requiring UI (for enemies)
    /// </summary>
    /// <param name="health">Starting health value (becomes both current and max health)</param>
    /// <param name="image">Sprite image to display for this character</param>
    /// <param name="name">Name to display for this character (cached for later UI assignment)</param>
    /// <param name="magicPower">Magic power stat</param>
    /// <param name="attackPower">Attack power stat</param>
    /// <param name="defense">Defense stat</param>
    /// <remarks>
    /// Called by EnemyView to initialize stats and sprite without requiring UI components.
    /// UI components (health bar, name text, etc.) are assigned later via AssignHealthBar.
    /// </remarks>
    protected void SetupBaseWithoutUI(int health, Sprite image, string name, float magicPower, float attackPower, float defense)
    {
        MaxHealth = CurrentHealth = health;
        MagicPower = magicPower;
        AttackPower = attackPower;
        Defense = defense;
        if (spriteRenderer != null && image != null)
            spriteRenderer.sprite = image;
    }

    /// <summary>
    /// Stores previous health value for detecting healing vs damage
    /// </summary>
    private int previousHealth = -1;

    /// <summary>
    /// Updates health UI with smooth animation
    /// </summary>
    private void UpdateHealth()
    {
        // Determine if this is healing or damage
        bool isHealing = previousHealth >= 0 && CurrentHealth > previousHealth;
        previousHealth = CurrentHealth;
        
        AnimateHealthBar(isHealing);
        AnimateHealthText();
    }

    /// <summary>
    /// Animates the health bar slider smoothly from current to new value
    /// </summary>
    private void AnimateHealthBar(bool isHealing)
    {
        if (sliderHealth == null) return;
        
        // Kill any existing health bar animation
        DOTween.Kill(sliderHealth);
        
        // Choose ease based on whether healing or taking damage
        Ease ease = isHealing ? healthIncreaseEase : healthDecreaseEase;
        
        // Animate slider value
        sliderHealth.DOValue(CurrentHealth, healthAnimDuration).SetEase(ease);
        
        // Animate fill color
        if (fillHealth != null && gradientHealth != null)
        {
            float targetNormalized = (float)CurrentHealth / MaxHealth;
            Color targetColor = gradientHealth.Evaluate(targetNormalized);
            fillHealth.DOColor(targetColor, healthAnimDuration).SetEase(ease);
        }
    }

    /// <summary>
    /// Animates the health text with a subtle pulse effect
    /// </summary>
    private void AnimateHealthText()
    {
        if (healthText == null) return;
        
        // Update text immediately
        healthText.text = CurrentHealth + "/" + MaxHealth;
        
        // Apply pulse animation if enabled
        if (enableHealthPulse)
        {
            // Kill any existing text animation
            DOTween.Kill(healthText.transform);
            
            // Pulse animation: scale up then back to normal
            healthText.transform.localScale = Vector3.one;
            healthText.transform.DOPunchScale(Vector3.one * (healthPulseScale - 1f), 0.3f, 1, 0.5f);
        }
    }

    /// <summary>
    /// Immediately sets health without animation (for initialization)
    /// </summary>
    private void SetHealthImmediate()
    {
        previousHealth = CurrentHealth;
        
        if (sliderHealth != null)
        {
            sliderHealth.value = CurrentHealth;
            if (fillHealth != null && gradientHealth != null)
            {
                fillHealth.color = gradientHealth.Evaluate(sliderHealth.normalizedValue);
            }
        }
        
        if (healthText != null)
        {
            healthText.text = CurrentHealth + "/" + MaxHealth;
        }
    }

    /// <summary>
    /// Applies damage to this combatant, reducing health and playing damage effects
    /// </summary>
    /// <param name="damageAmount">How much damage to deal to this character</param>
    /// <remarks>
    /// This is called by the damage system when this character gets hurt.
    /// Reduces health, prevents going below zero, plays screen shake effect,
    /// and updates the health display. Makes combat feel impactful with visual feedback.
    /// 
    /// IMPROVED DESIGN: Armor calculation is now handled by ArmorStatusEffectSystem
    /// through pre-event subscription. This keeps the damage method focused on
    /// applying final damage and visual effects rather than complex calculations.
    /// </remarks>
    // Applies damage to this combatant, reducing health and playing damage effects
    public void Damage(int damageAmount)
    {
        // Apply damage to health (armor has already been calculated by ArmorStatusEffectSystem)
        CurrentHealth -= damageAmount;
        if (CurrentHealth < 0)
        {
            CurrentHealth = 0;
        }

        // Use the property to ensure lazy initialization of animation controller
        var animCtrl = AnimationController;
        
        // Play hit animation if animation controller exists
        if (animCtrl != null)
        {
            Debug.Log($"[CombatantView] Damage called on {gameObject.name}, triggering PlayHit animation", this);
            animCtrl.PlayHit();
        }
        else
        {
            Debug.LogWarning($"[CombatantView] Damage called on {gameObject.name}, but AnimationController is NULL! Check if CombatantAnimationController component exists.", this);
        }

        // Different hit movement for enemies vs heroes
        // Enemies move left (backwards) when hit, heroes get a subtle shake
        if (this is EnemyView)
        {
            // Enemy: Move left (backwards) when hit, then return
            // Get hit movement settings from EnemySystem (editable in Unity Inspector)
            if (EnemySystem.Instance != null)
            {
                float hitMoveDistance = EnemySystem.Instance.EnemyHitMoveDistance;
                float hitMoveDuration = EnemySystem.Instance.EnemyHitMoveDuration;
                float returnDuration = EnemySystem.Instance.EnemyHitReturnDuration;
                
                Vector3 originalPos = transform.position;
                transform.DOMoveX(originalPos.x - hitMoveDistance, hitMoveDuration)
                    .OnComplete(() => {
                        // Return to original position after moving left
                        transform.DOMoveX(originalPos.x, returnDuration);
                    });
            }
            else
            {
                // Fallback if EnemySystem is not available
                Vector3 originalPos = transform.position;
                transform.DOMoveX(originalPos.x - 0.2f, 0.1f)
                    .OnComplete(() => {
                        transform.DOMoveX(originalPos.x, 0.15f);
                    });
            }
        }
        else
        {
            // Hero: Play a subtle screen shake animation when taking damage (0.15 seconds, 0.15 intensity)
            // Reduced intensity for better UX - just a small shake to indicate hit
            transform.DOShakePosition(0.15f, 0.15f);
        }
        
        // Update the health display to show the new health value (animated)
        UpdateHealth();
    }

    /// <summary>
    /// Heals this combatant, increasing health with visual effects
    /// </summary>
    /// <param name="healAmount">How much health to restore</param>
    /// <remarks>
    /// Called by healing effects and abilities. Increases health up to MaxHealth,
    /// plays a healing visual indicator, and updates the health display with
    /// the healing-specific animation (green pulse, upward direction).
    /// </remarks>
    public void Heal(int healAmount)
    {
        // Cannot heal if dead
        if (IsDead) return;
        
        // Apply healing (clamped to MaxHealth)
        CurrentHealth = Mathf.Min(CurrentHealth + healAmount, MaxHealth);
        
        // Update the health display (animation will detect this is healing)
        UpdateHealth();
    }

    /// <summary>
    /// Marks this combatant as dead, applying visual effects and preventing further targeting
    /// </summary>
    /// <remarks>
    /// Called when health reaches zero. Greys out the sprite, stops animations,
    /// and sets the IsDead flag. Dead combatants cannot be targeted, do not process
    /// status effect ticks, and skip card discard/draw logic.
    /// 
    /// NOTE: Death animation is commented out as it's not yet implemented.
    /// When animation is ready, uncomment the animator.SetTrigger("Death") line.
    /// </remarks>
    public void MarkAsDead()
    {
        if (IsDead) return; // Already dead
        
        IsDead = true;
        
        Debug.Log($"[CombatantView] {gameObject.name} has been marked as DEAD", this);
        
        // Cache original sprite color if not yet cached
        if (spriteRenderer != null && !hasOriginalColor)
        {
            originalSpriteColor = spriteRenderer.color;
            hasOriginalColor = true;
        }
        
        // Grey out the sprite to show death state visually
        if (spriteRenderer != null)
        {
            // Set to grey with some transparency
            spriteRenderer.color = new Color(0.4f, 0.4f, 0.4f, 0.7f);
        }
        
        // Stop the animator to freeze animation
        if (animationController != null)
        {
            // Disable animator to freeze animation in current frame
            var animator = animationController.GetComponent<Animator>();
            if (animator != null)
            {
                animator.enabled = false;
                // NOTE: Death animation not yet implemented - commented out for now
                // When animation is ready, enable animator and play death:
                // animator.enabled = true;
                // animator.SetTrigger("Death");
            }
        }
    }

    /// <summary>
    /// Revives this combatant from death, restoring visual state
    /// </summary>
    /// <remarks>
    /// Used for resurrection mechanics or when resetting combat state.
    /// Restores original sprite color and re-enables animations.
    /// Does not restore health - call ResetToMaxHP() or Heal() separately.
    /// </remarks>
    public void Revive()
    {
        if (!IsDead) return; // Not dead
        
        IsDead = false;
        
        Debug.Log($"[CombatantView] {gameObject.name} has been REVIVED", this);
        
        // Restore original sprite color
        if (spriteRenderer != null && hasOriginalColor)
        {
            spriteRenderer.color = originalSpriteColor;
        }
        
        // Re-enable animator
        if (animationController != null)
        {
            var animator = animationController.GetComponent<Animator>();
            if (animator != null)
            {
                animator.enabled = true;
                animationController.PlayIdle();
            }
        }
    }

    /// <summary>
    /// Resets this combatant's health to maximum (for combat reset between fights)
    /// </summary>
    /// <remarks>
    /// Called when initializing a new combat or resetting after previous fight.
    /// Sets CurrentHealth to MaxHealth and updates the health display.
    /// </remarks>
    public void ResetToMaxHP()
    {
        CurrentHealth = MaxHealth;
        SetHealthImmediate();
        Debug.Log($"[CombatantView] {gameObject.name} HP reset to max: {MaxHealth}", this);
    }

    /// <summary>
    /// Clears all active status effects from this combatant
    /// </summary>
    /// <remarks>
    /// Called when resetting combat state between fights.
    /// Removes all status effects and updates the UI to reflect the cleared state.
    /// </remarks>
    public void ClearAllStatusEffects()
    {
        // Get all effect types currently active
        var effectTypes = new List<StatusEffectType>(statusEffects.Keys);
        
        // Clear the dictionary
        statusEffects.Clear();
        
        // Update UI for each cleared effect (sets stack to 0, which removes the icon)
        foreach (var type in effectTypes)
        {
            if (statusEffectsUI != null)
            {
                statusEffectsUI.UpdateStatusEffectUI(type, 0);
            }
        }
        
        Debug.Log($"[CombatantView] {gameObject.name} cleared all status effects", this);
    }

    /// <summary>
    /// Adds stacks of a status effect to this combatant
    /// </summary>
    /// <param name="type">The type of status effect to add</param>
    /// <param name="stackCount">How many stacks to add</param>
    /// <remarks>
    /// Called by status effect systems to apply effects like armor or burn.
    /// If the effect already exists, adds to the existing stacks.
    /// Updates the UI automatically to show the new effect.
    /// </remarks>
    public void AddStatusEffect(StatusEffectType type, int stackCount)
    {
        AddStatusEffect(type, stackCount, null);
    }
    
    /// <summary>
    /// Adds stacks of a status effect to this combatant with a custom display name
    /// </summary>
    /// <param name="type">The type of status effect to add</param>
    /// <param name="stackCount">How many stacks to add</param>
    /// <param name="customName">Custom display name for this effect (e.g., "Bonecracked", "Rage", "Moonfall")</param>
    /// <remarks>
    /// Called by status effect systems to apply effects with custom names.
    /// The custom name allows using consolidated move sprites instead of default effect sprites.
    /// Examples: "Bonecracked" for DEFENSE_DOWN, "Rage" for RAGE, "Moonfall" for DEFENSE_DOWN
    /// If the effect already exists, adds to the existing stacks and preserves the custom name.
    /// </remarks>
    public void AddStatusEffect(StatusEffectType type, int stackCount, string customName)
    {
        // Add to existing stacks or create new entry
        if (statusEffects.ContainsKey(type))
        {
            statusEffects[type] += stackCount;
        }
        else
        {
            statusEffects.Add(type, stackCount);
        }
        // Update the visual display to show the new stack count with custom name (if provided)
        if (!string.IsNullOrEmpty(customName))
        {
            statusEffectsUI.UpdateStatusEffectUI(type, GetStatusEffectStacks(type), customName);
        }
        else
        {
            statusEffectsUI.UpdateStatusEffectUI(type, GetStatusEffectStacks(type));
        }
    }
    
    /// <summary>
    /// Removes stacks of a status effect from this combatant
    /// </summary>
    /// <param name="type">The type of status effect to remove</param>
    /// <param name="stackCount">How many stacks to remove</param>
    /// <remarks>
    /// Called when effects wear off or get consumed (like armor blocking damage).
    /// Removes the effect completely if stacks reach zero or below.
    /// Updates the UI automatically to reflect the change.
    /// </remarks>
    public void RemoveStatusEffect(StatusEffectType type, int stackCount)
    {
        // Reduce stacks if the effect exists
        if (statusEffects.ContainsKey(type))
        {
            statusEffects[type] -= stackCount;
            // Remove completely if no stacks remain
            if (statusEffects[type] <= 0)
            {
                statusEffects.Remove(type);
            }
        }
        // Update the visual display (will hide if no stacks remain)
        statusEffectsUI.UpdateStatusEffectUI(type, GetStatusEffectStacks(type));
    }

    /// <summary>
    /// Gets the current number of stacks for a specific status effect
    /// </summary>
    /// <param name="type">The type of status effect to check</param>
    /// <returns>Number of stacks currently active (0 if effect not present)</returns>
    /// <remarks>
    /// Used by damage calculation and other systems to check effect strength.
    /// Returns 0 for effects that aren't currently active on this combatant.
    /// </remarks>
    public int GetStatusEffectStacks(StatusEffectType type)
    {
        // Return current stacks or 0 if effect not present
        if (statusEffects.ContainsKey(type)) return statusEffects[type];
        else return 0;
    }

    /// <summary>
    /// Triggers an animation state for this combatant
    /// </summary>
    /// <param name="state">The animation state to play</param>
    /// <remarks>
    /// <para><strong>Use case:</strong> External systems (cards, abilities, AI) can trigger animations</para>
    /// <para><strong>Example:</strong> <c>heroView.PlayAnimation(CombatantAnimState.Attack);</c></para>
    /// <para><strong>Safety:</strong> Safe to call even if animation controller is missing</para>
    /// </remarks>
    public void PlayAnimation(CombatantAnimState state)
    {
        if (animationController != null)
        {
            animationController.SetState(state);
        }
    }

    /// <summary>
    /// Returns to idle animation state
    /// </summary>
    /// <remarks>
    /// Call when character finishes an action and should return to default state.
    /// Safe to call even if animation controller is missing.
    /// </remarks>
    public void PlayIdleAnimation()
    {
        if (animationController != null)
        {
            animationController.PlayIdle();
        }
    }
    
    /// <summary>
    /// Plays an animation and waits for it to complete before returning
    /// </summary>
    /// <param name="state">The animation state to play</param>
    /// <param name="returnToIdle">If true, returns to idle animation after completing</param>
    /// <returns>Coroutine that waits for animation to finish</returns>
    /// <remarks>
    /// <para><strong>Use case:</strong> When you need to wait for an animation to finish 
    /// before continuing (e.g., hit animation before next target, attack animation before damage)</para>
    /// <para><strong>Example:</strong> <c>yield return target.PlayAnimationAndWait(CombatantAnimState.Hit);</c></para>
    /// <para><strong>Timing:</strong> Uses animation clip length plus a small buffer for transitions</para>
    /// </remarks>
    public IEnumerator PlayAnimationAndWait(CombatantAnimState state, bool returnToIdle = true)
    {
        if (animationController != null)
        {
            // Trigger the animation
            animationController.SetState(state);
            
            // Get the duration of this animation type
            float duration = animationController.GetAnimationDuration(state);
            
            // Wait for the animation to complete (with small buffer for transitions)
            yield return new WaitForSeconds(duration + 0.05f);
            
            // Optionally return to idle
            if (returnToIdle && state != CombatantAnimState.Dead && state != CombatantAnimState.Idle)
            {
                animationController.PlayIdle();
            }
        }
        else
        {
            // No animation controller, use fallback timing
            yield return new WaitForSeconds(0.3f);
        }
    }
    
    /// <summary>
    /// Gets the duration of a specific animation state
    /// </summary>
    /// <param name="state">The animation state to query</param>
    /// <returns>Duration in seconds, or a default fallback</returns>
    public float GetAnimationDuration(CombatantAnimState state)
    {
        if (animationController != null)
        {
            return animationController.GetAnimationDuration(state);
        }
        return 0.5f; // Default fallback
    }
}
