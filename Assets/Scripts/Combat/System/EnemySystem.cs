
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

/* ENEMY SYSTEM DOCUMENTATION
 *
 * How it works:
 * - Spawns enemies on the board at the start of combat
 * - Makes all enemies attack during their turn with proper caster tracking
 * - Handles enemy attack animations and damage creation
 * - Uses PerformEffectGA actions for enemy attacks and effects, supporting perk reactive targeting
 *
 * Design reasoning:
 * - PerformEffectGA actions now carry caster information for perk system integration
 * - Caster tracking is attached to PerformEffectGA, allowing perks to react to the correct enemy
 * - Same enemy attack flow works with both normal combat and perk reactions
 * - Clean separation between animation and damage creation
 *
 * Integration: Works with EnemyBoardView for display, supports perk system through caster tracking in PerformEffectGA
 */

/// <summary>
/// System that controls all enemy behavior and combat actions
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Acts as the brain that controls what enemies do during combat</para>
///
/// <para><strong>What it does:</strong> This system is like the AI controller for all enemies.
/// It spawns enemies at the start of combat, decides when they attack, and handles
/// their attack animations. When it's the enemies' turn, this system makes each
/// enemy attack the player with proper caster tracking. The key feature for the perk system
/// is that it creates PerformEffectGA actions with caster information, enabling perks to react
/// to the correct enemy attacker.</para>
///
/// <para><strong>Perk system integration:</strong> When enemies attack, this system creates
/// PerformEffectGA actions that include caster info (the attacking enemy). This lets perks know
/// which enemy attacked and can target that specific enemy with reactive effects like counter-attacks
/// or damage reflection. Caster tracking is now handled via PerformEffectGA, not AttackHeroGA or IHaveCaster.</para>
///
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Combat starts and enemies get spawned on the board</item>
/// <item>Player finishes their turn and enemy turn begins</item>
/// <item>System creates PerformEffectGA actions with caster info for each enemy attack or effect</item>
/// <item>Enemies animate forward, deal damage with caster tracking, then move back</item>
/// <item>Perks can react to PerformEffectGA actions and target the correct attacking enemy</item>
/// </list>
///
/// <para><strong>Works with:</strong> EnemyBoardView for visuals, perk system for reactive targeting via PerformEffectGA</para>
/// </remarks>
// This system manages all enemy behavior, turns, and combat actions in the card game
// It's like the "AI controller" that handles what enemies do during their turn
// SEQUENTIAL SPAWNING: Enemies spawn one at a time. When one dies, the next is spawned.
public class EnemySystem : Singleton<EnemySystem>
{
    /// <summary>
    /// Public access to all enemy views currently on the battlefield
    /// </summary>
    /// <remarks>
    /// Provides easy access to the list of all active enemies for other systems.
    /// Used by card effects, targeting systems, and damage calculations.
    /// Gets the list from EnemyBoardView to maintain single source of truth.
    /// </remarks>
    public List<EnemyView> EnemyViews { get => enemyBoardView.EnemyViews; }
    
    /// <summary>
    /// Queue of enemies waiting to be spawned sequentially
    /// </summary>
    /// <remarks>
    /// Enemies spawn one at a time. When the current enemy is defeated,
    /// the next enemy from this queue is spawned automatically.
    /// </remarks>
    private Queue<EnemyData> enemyQueue = new Queue<EnemyData>();
    
    /// <summary>
    /// Total number of enemies in this battle (for determining miniboss vs main boss)
    /// </summary>
    private int totalEnemyCount = 0;
    
    /// <summary>
    /// Current spawn index (0 = first enemy/miniboss, 1 = second enemy/main boss)
    /// </summary>
    private int currentSpawnIndex = 0;
    
    /// <summary>
    /// Checks if there are more enemies waiting to spawn
    /// </summary>
    public bool HasRemainingEnemies => enemyQueue.Count > 0;
    
    /// <summary>
    /// The visual board where enemies appear and are displayed to players
    /// </summary>
    /// <remarks>
    /// This component handles showing enemies on screen and managing their positions.
    /// Assign the EnemyBoardView GameObject in the Inspector.
    /// </remarks>
    // Reference to the visual board where enemies are displayed - assigned in Unity Inspector
    [SerializeField] private EnemyBoardView enemyBoardView;

    //performers - these are methods that execute specific game actions

    /// <summary>
    /// Sets up enemy action handling when this system turns on
    /// </summary>
    /// <remarks>
    /// Unity calls this automatically when the GameObject becomes active.
    /// Registers this system to handle enemy-related actions like turns and attacks.
    /// </remarks>
    // Called when this GameObject becomes active - sets up action listeners
    void OnEnable()
    {
        // Register a method to handle when it's the enemy's turn
        ActionSystem.AttachPerformer<EnemyTurnGA>(EnemyTurnPerformer);
        // Register a POST reaction to clean up after enemy turn completes
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
        // Register a method to handle when an enemy needs to be killed/removed
        ActionSystem.AttachPerformer<KillEnemyGA>(KillEnemyPerformer);
    }

    /// <summary>
    /// Cleans up enemy action handling when this system turns off
    /// </summary>
    /// <remarks>
    /// Unity calls this automatically when the GameObject becomes inactive.
    /// Unregisters action handlers to prevent memory problems.
    /// </remarks>
    // Called when this GameObject is disabled - cleans up action listeners to prevent memory leaks
    void OnDisable()
    {
        // Unregister the enemy turn handler
        ActionSystem.DetachPerformer<EnemyTurnGA>();
        // Unsubscribe from POST reaction to prevent accumulation (especially important for Singleton)
        // Note: UnsubscribeReaction has a known bug but we still call it to attempt cleanup
        // Other systems (HeroSystem, StaminaSystem, CooldownSystem) follow the same pattern
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
        // Unregister the kill enemy handler
        ActionSystem.DetachPerformer<KillEnemyGA>();
    }

    // This class will manage enemy behavior and actions

    /// <summary>
    /// Initializes the enemy queue and spawns the first enemy for sequential combat
    /// </summary>
    /// <param name="enemyDatas">List of enemy information in order of appearance</param>
    /// <remarks>
    /// SEQUENTIAL SPAWNING: This method queues all enemies but only spawns the first one.
    /// When the first enemy is defeated, the next will spawn automatically via KillEnemyPerformer.
    /// This creates a wave-based combat experience where players face enemies one at a time.
    /// </remarks>
    public void Setup(List<EnemyData> enemyDatas)
    {
        // Defensive: Guard against null enemyDatas
        if (enemyDatas == null)
        {
            Debug.LogWarning("[EnemySystem.Setup] Called with null enemyDatas parameter. Aborting setup. Expected a non-null List<EnemyData>.");
            enemyQueue.Clear();
            return;
        }

        // Clear any previous queue data (in case of battle restart)
        enemyQueue.Clear();
        currentSpawnIndex = 0;

        // Queue all enemies for sequential spawning
        foreach (var enemyData in enemyDatas)
        {
            enemyQueue.Enqueue(enemyData);
        }

        // Store total count for overlay logic (first = miniboss, second = main boss)
        totalEnemyCount = enemyDatas.Count;

        // Log the setup
        Debug.Log($"[EnemySystem] Queued {enemyQueue.Count} enemies for sequential spawning");

        // Spawn the first enemy to start combat (with overlay if applicable)
        StartCoroutine(SpawnNextEnemyCoroutine());
    }
    
    /// <summary>
    /// Coroutine that spawns the next enemy from the queue, or triggers victory if none remain
    /// Shows overlay before first enemy (miniboss) and second enemy (main boss)
    /// </summary>
    /// <remarks>
    /// SEQUENTIAL SPAWNING: This method is called initially and after each enemy defeat.
    /// If enemies remain in the queue, spawns the next one.
    /// If the queue is empty, all enemies are defeated and victory is triggered.
    /// 
    /// OVERLAY INTEGRATION: Shows enemy spawn overlay before:
    /// - First enemy (index 0) = Miniboss
    /// - Second enemy (index 1) = Main Boss
    /// </remarks>
    private IEnumerator SpawnNextEnemyCoroutine()
    {
        // Check if there are more enemies to spawn
        if (enemyQueue.Count > 0)
        {
            // Peek at the next enemy (don't dequeue yet - we need it for overlay)
            EnemyData nextEnemyData = enemyQueue.Peek();
            
            // Show overlay before first enemy (miniboss) or second enemy (main boss)
            // Index 0 = first enemy (miniboss), Index 1 = second enemy (main boss)
            if (currentSpawnIndex == 0 || (currentSpawnIndex == 1 && totalEnemyCount >= 2))
            {
                // Show overlay animation with appropriate banner
                if (EnemySpawnOverlayUI.Instance != null)
                {
                    // Index 0 = Mini Boss, Index 1 = Main Boss
                    bool isMainBoss = (currentSpawnIndex == 1 && totalEnemyCount >= 2);
                    EnemySpawnOverlayUI.Instance.ShowEnemySpawnOverlay(isMainBoss);
                    
                    // Wait for overlay animation to complete
                    // Overlay duration: fadeIn (0.5s) + slide (0.8s) + bounce (0.3s + 0.15s) + hold (1.5s) + fadeOut (0.5s) ≈ 3.75s
                    float overlayDuration = 4.0f; // Add small buffer
                    yield return new WaitForSeconds(overlayDuration);
                }
                
                // For first enemy only: Wait for Battle Start banner to complete before spawning
                // Sequence: Overlay -> Battle Start -> Enemy Spawn
                if (currentSpawnIndex == 0 && CombatPhaseUI.Instance != null)
                {
                    // Wait for Battle Start banner to finish animating
                    // Battle Start duration: fadeIn (0.4s) + hold (1.0s) + slideOut (0.5s) = 1.9s
                    // But CombatPhaseManager waits 2.5s total, so we'll wait for IsAnimating to be false
                    while (CombatPhaseUI.Instance.IsAnimating)
                    {
                        yield return new WaitForSeconds(0.1f); // Check every 0.1 seconds
                    }
                    
                    // Add small buffer after Battle Start completes
                    yield return new WaitForSeconds(0.2f);
                }
            }
            
            // Now dequeue and spawn the enemy
            enemyQueue.Dequeue();
            enemyBoardView.AddEnemy(nextEnemyData);
            
            Debug.Log($"[EnemySystem] Spawned enemy: {nextEnemyData.EnemyName} (spawn index: {currentSpawnIndex}). {enemyQueue.Count} enemies remaining.");
            
            // Increment spawn index for next enemy
            currentSpawnIndex++;
        }
        else
        {
            // No more enemies - trigger victory
            Debug.Log("[EnemySystem] All enemies defeated! Victory!");
            TriggerVictory();
        }
    }
    
    /// <summary>
    /// Handles victory condition when all enemies are defeated
    /// </summary>
    /// <remarks>
    /// Called automatically when the last enemy is defeated and the queue is empty.
    /// Shows victory banner and allows player to continue to map selection.
    /// </remarks>
    private void TriggerVictory()
    {
        Debug.Log("[EnemySystem] ===== VICTORY =====");
        Debug.Log("[EnemySystem] All enemies have been defeated!");
        
        // Show victory banner
        if (VictoryDefeatUI.Instance != null)
        {
            VictoryDefeatUI.Instance.ShowVictory();
        }
        else
        {
            Debug.LogWarning("[EnemySystem] VictoryDefeatUI.Instance is null! Cannot show victory banner.", this);
        }
    }

    [Header("Move Name Display")]
    [Tooltip("Color for enemy move name popups")]
    [SerializeField] private Color moveNameColor = new Color(1f, 0.5f, 0.5f, 1f); // Light red
    
    [Tooltip("Vertical offset for move name popup above enemy")]
    [SerializeField] private float moveNameOffsetY = 2f;

    /// <summary>
    /// Makes all enemies perform their actions during enemy turn
    /// </summary>
    /// <param name="enemyTurnGA">The action that triggered the enemy turn</param>
    /// <returns>Waits one frame for proper timing</returns>
    /// <remarks>
    /// This method runs when it becomes the enemies' turn to act.
    /// 
    /// TURN ORDER: Burn damage is applied FIRST, then surviving enemies attack.
    /// This prevents enemies from attacking after dying to burn damage.
    /// 
    /// BURN INTEGRATION: Checks each enemy for burn stacks and applies burn damage.
    /// Enemies take burn damage before attacking, same as the hero burn system.
    /// 
    /// ATTACK INTEGRATION: Creates AttackHeroGA actions with proper caster tracking.
    /// This enables the perk system to know which enemy attacked for reactive targeting.
    /// Only living enemies (CurrentHealth > 0) can attack.
    /// 
    /// MOVE NAME DISPLAY: Shows the enemy's move name as a popup when they act.
    /// </remarks>
    // Handles what happens during the enemy turn - makes all enemies attack
    private IEnumerator EnemyTurnPerformer(EnemyTurnGA enemyTurnGA)
    {
        // Wait for enemy turn banner animation to complete before starting attacks
        // Banner animation duration: fadeInDuration (0.4s) + holdDuration (1.0s) + slideOutDuration (0.5s) = ~1.9s
        // Add a small buffer for safety
        float bannerAnimationDuration = 2.0f;
        
        // Wait for banner to finish animating
        yield return new WaitForSeconds(bannerAnimationDuration);

        // Use StatusEffectTickSystem to process all enemy status effect ticks
        StatusEffectTickSystem.Instance.TickStatusEffects(enemyBoardView.EnemyViews.ConvertAll(e => (CombatantView)e));

        // After status effect ticks, make all enemies use a move from their moveset
        foreach (var enemy in enemyBoardView.EnemyViews)
        {
            // Skip dead enemies
            if (enemy.CurrentHealth <= 0)
                continue;

            // Check if enemy should skip turn (RESTING or CHARGING)
            if (RestingStatusEffectSystem.ShouldSkipTurn(enemy))
            {
                Debug.Log($"[EnemySystem] {enemy.name} is RESTING/CHARGING, skipping turn");
                // Decrement will be handled by StatusEffectTickSystem
                continue;
            }

            // Get the moveset from EnemyData
            var moveset = enemy.Data?.Moveset;
            if (moveset == null || moveset.Count == 0)
            {
                // Fallback: basic attack if no moveset
                ShowMoveNamePopup(enemy, "Attack");
                AttackHeroGA fallbackAttack = new(enemy);
                ActionSystem.Instance.AddReaction(fallbackAttack);
                continue;
            }

            // Randomly select a move
            var move = moveset[Random.Range(0, moveset.Count)];

            // Show the move name as a popup above the enemy
            string moveName = !string.IsNullOrEmpty(move.Description) ? move.Description : "Unknown Move";
            Debug.Log($"[EnemySystem] {enemy.name} selected move: '{moveName}' (Move has ManualTargetEffect: {move.ManualTargetEffect != null}, OtherEffects count: {move.OtherEffects?.Count ?? 0})");
            ShowMoveNamePopup(enemy, moveName);

            // Start tracking hit targets for this move sequence
            HitTargetTracker.StartMoveSequence(enemy);

            // Handle manual target effect (single-target, e.g., attack or debuff)
            if (move.ManualTargetEffect != null)
            {
                // For now, target a random hero (could be improved with AI logic)
                var heroTargets = HeroSystem.Instance.HeroViews;
                if (heroTargets == null || heroTargets.Count == 0)
                {
                    Debug.LogWarning($"[EnemySystem] {enemy.name} tried to use move '{moveName}' but no heroes are available to target!");
                    continue;
                }
                var target = heroTargets[Random.Range(0, heroTargets.Count)];
                if (target == null)
                {
                    Debug.LogWarning($"[EnemySystem] {enemy.name} selected null target for move '{moveName}'!");
                    continue;
                }
                Debug.Log($"[EnemySystem] {enemy.name} using move '{moveName}' (ManualTargetEffect) targeting {target.name}");
                PerformEffectGA performEffectGA = new(move.ManualTargetEffect, new List<CombatantView> { target }, enemy);
                ActionSystem.Instance.AddReaction(performEffectGA);
            }

            // Handle auto-target effects (area, self-buff, etc.)
            if (move.OtherEffects != null && move.OtherEffects.Count > 0)
            {
                foreach (var effectWrapper in move.OtherEffects)
                {
                    // Defensive check: Ensure effectWrapper is valid
                    if (effectWrapper == null)
                    {
                        Debug.LogWarning($"[EnemySystem] {enemy.name} move '{moveName}' has null effectWrapper in OtherEffects!");
                        continue;
                    }

                    // Defensive check: Ensure effects is not null
                    if (effectWrapper.effects == null)
                    {
                        Debug.LogWarning($"[EnemySystem] {enemy.name} move '{moveName}' has null effects in effectWrapper!");
                        continue;
                    }

                    // Defensive check: Ensure targetMode is not null
                    if (effectWrapper.targetMode == null)
                    {
                        Debug.LogWarning($"[EnemySystem] {enemy.name} move '{moveName}' has null targetMode in effectWrapper!");
                        continue;
                    }

                    // Set caster on target mode if it's TargetsHitByPreviousEffectTM
                    if (effectWrapper.targetMode is TargetsHitByPreviousEffectTM hitTargetMode)
                    {
                        hitTargetMode.SetCaster(enemy);
                    }

                    List<CombatantView> targets = null;
                    try
                    {
                        targets = effectWrapper.targetMode.GetTargets();
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"[EnemySystem] Exception getting targets for {enemy.name} move '{moveName}': {ex.Message}\n{ex.StackTrace}");
                        continue;
                    }

                    if (targets == null)
                    {
                        Debug.LogWarning($"[EnemySystem] {enemy.name} move '{moveName}' targetMode returned null targets list!");
                        targets = new List<CombatantView>();
                    }

                    Debug.Log($"[EnemySystem] {enemy.name} using move '{moveName}' (OtherEffect: {effectWrapper.effects.GetType().Name}) targeting {targets.Count} target(s)");
                    PerformEffectGA performEffectGA = new(effectWrapper.effects, targets, enemy);
                    ActionSystem.Instance.AddReaction(performEffectGA);
                }
            }

            // Log if move has no effects configured
            if (move.ManualTargetEffect == null && (move.OtherEffects == null || move.OtherEffects.Count == 0))
            {
                Debug.LogWarning($"[EnemySystem] {enemy.name} move '{moveName}' has no effects configured! Move will do nothing.");
            }

        }
        
        // Don't wait here - let the ActionSystem process all reactions automatically
        // The POST reaction will handle cleanup after all actions complete
        Debug.Log("[EnemySystem] All enemy moves queued. Reactions will be processed by ActionSystem.");
        
        // Wait one frame before continuing (required for coroutines)
        yield return null;
    }

    /// <summary>
    /// POST reaction that runs after all enemy turn actions complete
    /// </summary>
    /// <param name="enemyTurnGA">The enemy turn action that just completed</param>
    /// <remarks>
    /// This runs after all PerformEffectGA reactions have been processed.
    /// Used to clean up hit target trackers and perform any final enemy turn cleanup.
    /// </remarks>
    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        Debug.Log("[EnemySystem] Enemy turn POST reaction - cleaning up hit target trackers");
        // Clear all hit target trackers after all enemy moves complete
        // This happens after all reactions (PerformEffectGA -> DealDamageGA, etc.) are processed
        HitTargetTracker.ClearAll();
    }

    /// <summary>
    /// Displays the enemy's move name as a popup above them
    /// </summary>
    /// <param name="enemy">The enemy performing the move</param>
    /// <param name="moveName">The name of the move to display</param>
    private void ShowMoveNamePopup(EnemyView enemy, string moveName)
    {
        // Get position above the enemy
        SpriteRenderer spriteRenderer = enemy.GetComponentInChildren<SpriteRenderer>();
        Vector3 popupPosition = spriteRenderer != null ? spriteRenderer.transform.position : enemy.transform.position;
        popupPosition.y += moveNameOffsetY;
        
        // Create a text popup with the move name using a smaller scale than damage popups
        // Damage numbers use scale 1.0f, but text popups should be smaller (0.5f = 50% size)
        // This makes move names appear smaller than damage numbers
        DamagePopUp.CreateTextPopUp(popupPosition, moveName, moveNameColor, DamagePopUp.PopUpAnimationMode.FadeOnly, 0.5f);
    }
  
    /// <summary>
    /// Handles the animation and damage when an enemy attacks the player
    /// </summary>
    /// <param name="attackHeroGA">The attack action containing which enemy is attacking</param>
    /// <returns>Waits for animations to complete before continuing</returns>
    /// <remarks>
    /// This method creates the visual attack sequence with animations and damage creation.
    /// 
    /// ANIMATION: The enemy moves forward, deals damage with proper caster tracking, then moves back.
    /// The caster info gets passed to DealDamageGA so perks can know who dealt the damage.
    /// </remarks>
    // Handles the visual animation and damage when an enemy attacks the hero
    private IEnumerator AttackHeroPerformer(AttackHeroGA attackHeroGA)
    {
        // Get the enemy that's performing the attack
        EnemyView attacker = attackHeroGA.Attacker;
        
        // Check if the enemy is still alive (might have died to burn damage)
        if (attacker.CurrentHealth <= 0)
        {
            // Enemy is dead, cancel the attack silently
            yield break;
        }
        
        // Play attack animation on the enemy
        attacker.PlayAnimation(CombatantAnimState.Attack);
        
        // Animate the enemy moving forward (attack windup) - moves left 1 unit in 0.15 seconds
        Tween tween = attacker.transform.DOMoveX(attacker.transform.position.x - 1f, 0.15f);
        // Wait for the forward movement animation to complete
        yield return tween.WaitForCompletion();
        
        // Wait for the attack animation to reach its impact point
        float attackDuration = attacker.GetAnimationDuration(CombatantAnimState.Attack);
        yield return new WaitForSeconds(attackDuration * 0.4f); // Wait for impact moment (40% of animation)
        
        // Create a damage action with caster tracking for perk system
        var heroViews = HeroSystem.Instance.HeroViews;
        if (heroViews == null || heroViews.Count == 0)
        {
            Debug.LogWarning("[EnemySystem] No heroes available to target for damage.");
            yield break;
        }
        int randomIndex = Random.Range(0, heroViews.Count);
        DealDamageGA dealDamageGA = new(attacker.AttackPower, new() { heroViews[randomIndex] }, attackHeroGA.Caster);
        // Add the damage action to the queue to actually hurt the hero
        ActionSystem.Instance.AddReaction(dealDamageGA);
        
        // Wait for remaining attack animation
        yield return new WaitForSeconds(attackDuration * 0.6f);
        
        // Animate the enemy moving back to original position - moves right 1 unit in 0.25 seconds
        Tween backTween = attacker.transform.DOMoveX(attacker.transform.position.x + 1f, 0.25f);
        yield return backTween.WaitForCompletion();
        
        // Return enemy to idle animation
        attacker.PlayIdleAnimation();
    }
    
    /// <summary>
    /// Handles the death sequence when an enemy is killed, then spawns the next enemy
    /// </summary>
    /// <param name="killEnemyGA">The kill action containing which enemy to remove</param>
    /// <returns>Waits for the removal animation to complete</returns>
    /// <remarks>
    /// SEQUENTIAL SPAWNING: This method processes enemy deaths and triggers the next spawn.
    /// 1. Removes the defeated enemy with animation
    /// 2. Waits for removal to complete
    /// 3. Spawns the next enemy from the queue (or triggers victory if none remain)
    /// This creates smooth transitions between enemies in wave-based combat.
    /// </remarks>
    private IEnumerator KillEnemyPerformer(KillEnemyGA killEnemyGA)
    {
        // Use the board view to remove the enemy with animation
        yield return enemyBoardView.RemoveEnemy(killEnemyGA.EnemyView);
        
        // Wait a brief moment for visual clarity before spawning next enemy
        yield return new WaitForSeconds(0.5f);
        
        // Spawn the next enemy or trigger victory (with overlay if applicable)
        yield return StartCoroutine(SpawnNextEnemyCoroutine());
    }
}
