
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
 * - Creates AttackHeroGA actions that support perk reactive targeting
 * 
 * Design reasoning:
 * - AttackHeroGA actions implement IHaveCaster so perks can target attackers
 * - DealDamageGA actions include caster info for perk system integration
 * - Same enemy attack flow works with both normal combat and perk reactions
 * - Clean separation between animation and damage creation
 * 
 * Integration: Works with EnemyBoardView for display, supports perk system through caster tracking
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
/// enemy attack the player with cool animations. The key feature for the perk system 
/// is that it creates actions with proper caster tracking.</para>
/// 
/// <para><strong>Perk system integration:</strong> When enemies attack, this system creates 
/// AttackHeroGA actions that implement IHaveCaster. This lets perks know which enemy 
/// attacked and can target that specific enemy with reactive effects like counter-attacks 
/// or damage reflection.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Combat starts and enemies get spawned on the board</item>
/// <item>Player finishes their turn and enemy turn begins</item>
/// <item>System creates AttackHeroGA actions with caster info for each enemy</item>
/// <item>Enemies animate forward, deal damage with caster tracking, then move back</item>
/// <item>Perks can react to attacks and target the attacking enemy</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> EnemyBoardView for visuals, perk system for reactive targeting</para>
/// </remarks>
// This system manages all enemy behavior, turns, and combat actions in the card game
// It's like the "AI controller" that handles what enemies do during their turn
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
        // Unregister the kill enemy handler
        ActionSystem.DetachPerformer<KillEnemyGA>();
    }

    // This class will manage enemy behavior and actions

    /// <summary>
    /// Creates and displays all enemies at the start of combat
    /// </summary>
    /// <param name="enemyDatas">List of enemy information to create enemies from</param>
    /// <remarks>
    /// This method takes enemy data and creates actual enemy characters on the board.
    /// Like spawning the enemy team that the player will fight against.
    /// </remarks>
    // Sets up all enemies at the start of the match - like spawning the enemy team
    public void Setup(List<EnemyData> enemyDatas)
    {
        // Loop through each enemy data (stats, name, image, etc.)
        foreach (var enemyData in enemyDatas)
        {
            // Create and display the enemy on the game board
            enemyBoardView.AddEnemy(enemyData);
        }
    }

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
    /// </remarks>
    // Handles what happens during the enemy turn - makes all enemies attack
    private IEnumerator EnemyTurnPerformer(EnemyTurnGA enemyTurnGA)
    {

        // Use StatusEffectTickSystem to process all enemy status effect ticks
        StatusEffectTickSystem.Instance.TickStatusEffects(enemyBoardView.EnemyViews.ConvertAll(e => (CombatantView)e));

        // After status effect ticks, make all enemies use a move from their moveset
        foreach (var enemy in enemyBoardView.EnemyViews)
        {
            // Skip dead enemies
            if (enemy.CurrentHealth <= 0)
                continue;

            // Get the moveset from EnemyData
            var moveset = enemy.Data?.Moveset;
            if (moveset == null || moveset.Count == 0)
            {
                // Fallback: basic attack if no moveset
                AttackHeroGA fallbackAttack = new(enemy);
                ActionSystem.Instance.AddReaction(fallbackAttack);
                continue;
            }

            // Randomly select a move
            var move = moveset[Random.Range(0, moveset.Count)];

            // Handle manual target effect (single-target, e.g., attack or debuff)
            if (move.ManualTargetEffect != null)
            {
                // For now, target a random hero (could be improved with AI logic)
                var heroTargets = HeroSystem.Instance.HeroViews;
                var target = heroTargets[Random.Range(0, heroTargets.Count)];
                PerformEffectGA performEffectGA = new(move.ManualTargetEffect, new List<CombatantView> { target });
                ActionSystem.Instance.AddReaction(performEffectGA);
            }

            // Handle auto-target effects (area, self-buff, etc.)
            if (move.OtherEffects != null && move.OtherEffects.Count > 0)
            {
                foreach (var effectWrapper in move.OtherEffects)
                {
                    List<CombatantView> targets = effectWrapper.targetMode.GetTargets();
                    PerformEffectGA performEffectGA = new(effectWrapper.effects, targets);
                    ActionSystem.Instance.AddReaction(performEffectGA);
                }
            }
        }
        // Wait one frame before continuing (required for coroutines)
        yield return null;
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
        
        // Animate the enemy moving forward (attack windup) - moves left 1 unit in 0.15 seconds
        Tween tween = attacker.transform.DOMoveX(attacker.transform.position.x - 1f, 0.15f);
        // Wait for the forward movement animation to complete
        yield return tween.WaitForCompletion();
        // Animate the enemy moving back to original position - moves right 1 unit in 0.25 seconds
        attacker.transform.DOMoveX(attacker.transform.position.x + 1f, 0.25f);
        // Create a damage action with caster tracking for perk system
        DealDamageGA dealDamageGA = new(attacker.AttackPower, new() { HeroSystem.Instance.HeroViews[Random.Range(0, 4)] }, attackHeroGA.Caster);
        // Add the damage action to the queue to actually hurt the hero
        ActionSystem.Instance.AddReaction(dealDamageGA);
    }
    
    /// <summary>
    /// Handles the death sequence when an enemy is killed
    /// </summary>
    /// <param name="killEnemyGA">The kill action containing which enemy to remove</param>
    /// <returns>Waits for the removal animation to complete</returns>
    /// <remarks>
    /// This method processes enemy deaths by calling the board view's removal method.
    /// The enemy gets removed with a nice scaling animation before being destroyed.
    /// Keeps the battlefield clean by removing defeated enemies.
    /// </remarks>
    private IEnumerator KillEnemyPerformer(KillEnemyGA killEnemyGA)
    {
        // Use the board view to remove the enemy with animation
        yield return enemyBoardView.RemoveEnemy(killEnemyGA.EnemyView);
    }
}
