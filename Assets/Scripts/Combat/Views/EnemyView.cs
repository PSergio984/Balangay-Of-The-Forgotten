using TMPro;
using UnityEngine;

/* ENEMY VIEW DOCUMENTATION
 * 
 * Purpose: Visual representation of enemy characters in combat
 * 
 * How it works:
 * - Extends CombatantView to get basic health and damage functionality
 * - Adds enemy-specific features like attack power display
 * - Shows enemy stats (health, attack) to the player
 * - Updates UI when enemy stats change
 * 
 * Integration: Created by EnemySystem, inherits from CombatantView for combat functionality
 */

/// <summary>
/// Visual representation of an enemy character in combat
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Shows enemy characters on screen with their stats and health</para>
/// 
/// <para><strong>What it does:</strong> This represents an enemy that the player can see and 
/// fight against. It shows the enemy's health, attack power, and visual appearance. 
/// It inherits basic combat functionality like taking damage from CombatantView, but 
/// adds enemy-specific features like displaying attack power.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>EnemySystem creates this when spawning enemies</item>
/// <item>Gets set up with enemy data (health, attack, image, name)</item>
/// <item>Displays attack power so player knows how dangerous the enemy is</item>
/// <item>Inherits damage handling, health display, and visual effects from CombatantView</item>
/// <item>Updates attack display when attack power changes</item>
/// </list>
/// 
/// <para><strong>Needs:</strong> EnemyData to set up stats, UI text components for display</para>
/// 
/// <para><strong>Works with:</strong> EnemySystem for creation, CombatantView for base functionality</para>
/// 
/// <para><strong>How to use:</strong> EnemySystem creates these automatically, assign UI components in prefab</para>
/// </remarks>
public class EnemyView : CombatantView
{

    /// <summary>
    /// The EnemyData asset containing stats, moveset, and other info for this enemy
    /// </summary>
    public EnemyData Data { get; private set; }

    /// <summary>
    /// Sets up this enemy view with data from an EnemyData asset
    /// </summary>
    /// <param name="enemyData">Contains all the stats and appearance info for this enemy</param>
    /// <remarks>
    /// This initializes the enemy with all its starting values. Sets attack power,
    /// updates the attack display, then calls the base setup for health, image, and name.
    /// Also stores the EnemyData reference for moveset and other logic.
    /// </remarks>
    public void Setup(EnemyData enemyData)
    {
        Data = enemyData;
        // Set up the base combatant properties without UI (UI assigned later in AssignHealthBar)
        SetupBaseWithoutUI(enemyData.Health, enemyData.Image, enemyData.EnemyName, enemyData.MagicPower, enemyData.AttackPower, enemyData.Defense);

        // Force-assign the animation controller in case Setup is called before Awake
        animationController = GetComponent<CombatantAnimationController>();
        var animCtrl = AnimationController;
        if (animCtrl != null)
        {
            if (enemyData.AnimatorOverride != null)
            {
                Debug.Log($"[EnemyView] Setting AnimatorOverrideController: {enemyData.AnimatorOverride.name}", this);
                animCtrl.SetAnimatorOverride(enemyData.AnimatorOverride);
            }
            else
            {
                Debug.LogWarning($"[EnemyView] EnemyData.AnimatorOverride is null for enemy: {enemyData.EnemyName}", this);
            }
            // Force play idle animation after setup
            animCtrl.PlayIdle();
        }
        else
        {
            Debug.LogWarning($"[EnemyView] AnimationController is null on {gameObject.name}", this);
        }
    }

    /// <summary>
    /// Assigns the health bar UI components from the scene to this enemy.
    /// </summary>
    /// <param name="healthBarSlider">The Slider UI element to use for this enemy's health bar.</param>
    /// <param name="healthBarFill">The Image component for health bar fill color.</param>
    /// <param name="healthBarText">The Text component displaying health values.</param>
    /// <param name="nameText">The Text component displaying the enemy name.</param>
    /// <remarks>
    /// <para><strong>Why separate from prefab:</strong> Enemies use static scene-based health bars</para>
    /// <para><strong>When called:</strong> After Setup() in EnemyBoardView.AddEnemy()</para>
    /// <para><strong>What it does:</strong> Links all health bar UI to this enemy and updates values</para>
    /// </remarks>
    public void AssignHealthBar(UnityEngine.UI.Slider healthBarSlider, UnityEngine.UI.Image healthBarFill, TMPro.TMP_Text healthBarText, TMPro.TMP_Text nameText)
    {
        // Assign all health bar components
        this.sliderHealth = healthBarSlider;
        this.fillHealth = healthBarFill;
        this.healthText = healthBarText;
        this.NameText = nameText;
        
        // Debug: Check visibility issues
        if (sliderHealth != null)
        {
            // Ensure GameObject is active
            if (!sliderHealth.gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"[EnemyView] Health bar GameObject '{sliderHealth.gameObject.name}' is inactive! Activating it...", sliderHealth);
                sliderHealth.gameObject.SetActive(true);
            }
            
            // Check Canvas
            Canvas canvas = sliderHealth.GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError($"[EnemyView] Health bar '{sliderHealth.gameObject.name}' is not under a Canvas! It won't render.", sliderHealth);
            }
            else
            {
                // Ensure Canvas is active and enabled
                if (!canvas.gameObject.activeInHierarchy)
                {
                    Debug.LogWarning($"[EnemyView] Canvas '{canvas.gameObject.name}' is inactive! Activating it...", canvas);
                    canvas.gameObject.SetActive(true);
                }
                
                // Check Canvas Group (might be blocking rendering)
                CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
                if (canvasGroup != null && (!canvasGroup.interactable || canvasGroup.alpha <= 0.01f))
                {
                    Debug.LogWarning($"[EnemyView] CanvasGroup on '{canvas.gameObject.name}' might be blocking visibility (alpha: {canvasGroup.alpha}, interactable: {canvasGroup.interactable})", canvasGroup);
                }
                
                Debug.Log($"[EnemyView] Health bar assigned to Canvas '{canvas.gameObject.name}' (Render Mode: {canvas.renderMode}, Active: {canvas.gameObject.activeInHierarchy})", sliderHealth);
            }
            
            RectTransform parentRect = sliderHealth.GetComponent<RectTransform>();
            if (parentRect != null)
            {
                // Debug: Log current state
                Debug.Log($"[EnemyView] Health bar RectTransform - Position: {parentRect.position}, LocalPosition: {parentRect.localPosition}, Scale: {parentRect.localScale}, SizeDelta: {parentRect.sizeDelta}, Active: {parentRect.gameObject.activeInHierarchy}", parentRect);
                
                // Fix parent scale issue: Reset parent scale to (1,1,1) to prevent children from scaling
                if (parentRect.localScale != Vector3.one)
                {
                    // Store the desired size before resetting scale
                    Vector2 currentSize = parentRect.sizeDelta;
                    Vector3 currentScale = parentRect.localScale;
                    
                    // Calculate what the size should be at scale 1
                    Vector2 targetSize = new Vector2(
                        currentSize.x * currentScale.x,
                        currentSize.y * currentScale.y
                    );
                    
                    Debug.Log($"[EnemyView] Resetting scale from {currentScale} to (1,1,1), adjusting size from {currentSize} to {targetSize}", parentRect);
                    
                    // Reset scale to (1,1,1) - this prevents children from scaling
                    parentRect.localScale = Vector3.one;
                    
                    // Resize parent using sizeDelta instead of scale
                    if (parentRect.anchorMin == parentRect.anchorMax)
                    {
                        // If using fixed anchors, adjust sizeDelta
                        parentRect.sizeDelta = targetSize;
                    }
                    else
                    {
                        // If using stretch anchors, adjust Left/Top/Right/Bottom offsets
                        float left = parentRect.offsetMin.x;
                        float bottom = parentRect.offsetMin.y;
                        float right = parentRect.offsetMax.x;
                        float top = parentRect.offsetMax.y;
                        
                        // Scale the offsets to match the desired size
                        parentRect.offsetMin = new Vector2(left * currentScale.x, bottom * currentScale.y);
                        parentRect.offsetMax = new Vector2(right * currentScale.x, top * currentScale.y);
                    }
                }
                
                // Ensure the healthbar is visible (not zero size)
                if (parentRect.sizeDelta.x <= 0.1f || parentRect.sizeDelta.y <= 0.1f)
                {
                    Debug.LogWarning($"[EnemyView] Health bar has very small or zero size: {parentRect.sizeDelta}. This might make it invisible!", parentRect);
                }
            }
        }
        
        // Update all components immediately with current values
        if (sliderHealth != null)
        {
            sliderHealth.maxValue = MaxHealth;
            sliderHealth.minValue = 0;
            sliderHealth.value = CurrentHealth;
            
            // Make Fill completely independent - properly configured to stretch based on health
            if (sliderHealth.fillRect != null)
            {
                float normalized = Mathf.Clamp01((float)CurrentHealth / MaxHealth);
                var fillRect = sliderHealth.fillRect;
                
                // Make Fill completely independent with left-to-right stretch based on health
                // Set anchors to stretch from left (0) to normalized value (health percentage)
                fillRect.anchorMin = Vector2.zero;
                fillRect.anchorMax = new Vector2(normalized, 1f);
                
                // Clear all offsets to ensure it fills exactly from left edge
                fillRect.offsetMin = Vector2.zero;
                fillRect.offsetMax = Vector2.zero;
                
                // Set pivot to left edge so it scales from left
                fillRect.pivot = new Vector2(0f, 0.5f);
                
                // Force layout rebuild to apply changes immediately
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(fillRect);
                
                // Also rebuild parent to ensure proper layout
                if (fillRect.parent != null)
                {
                    UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(fillRect.parent as RectTransform);
                }
            }
        }
        
        if (fillHealth != null && gradientHealth != null)
        {
            float normalized = (float)CurrentHealth / MaxHealth;
            fillHealth.color = gradientHealth.Evaluate(normalized);
        }
        
        if (healthText != null)
        {
            healthText.text = $"{CurrentHealth}/{MaxHealth}";
        }
        
        if (NameText != null && Data != null)
        {
            NameText.text = Data.EnemyName;
        }
    }
}
