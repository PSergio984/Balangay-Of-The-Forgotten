using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

/* ENEMY SYSTEM DOCUMENTATION
 * 
 * Purpose: Controls all enemy behavior and actions during combat
 * 
 * How it works:
 * - Spawns enemies on the board at the start of combat
 * - Makes all enemies attack during their turn
 * - Handles enemy attack animations and damage
 * - Acts as the "AI brain" that controls what enemies do
 * 
 * Integration: Works with EnemyBoardView for display, DamageSystem for attacks, ActionSystem for timing
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
/// enemy attack the player with cool animations.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Combat starts and enemies get spawned on the board</item>
/// <item>Player finishes their turn and enemy turn begins</item>
/// <item>System makes each enemy perform their attack</item>
/// <item>Enemies animate forward, deal damage, then move back</item>
/// <item>Turn switches back to player after all enemies are done</item>
/// </list>
/// 
/// <para><strong>Needs:</strong> EnemyBoardView for displaying enemies, ActionSystem for turn management</para>
/// 
/// <para><strong>Works with:</strong> EnemyBoardView for visuals, DamageSystem for attacks, HeroSystem for targets</para>
/// 
/// <para><strong>How to use:</strong> Put this on a GameObject and assign EnemyBoardView in Inspector</para>
/// </remarks>
// This system manages all enemy behavior, turns, and combat actions in the card game
// It's like the "AI controller" that handles what enemies do during their turn
public class EnemySystem : Singleton<EnemySystem>
{
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
        // Register a method to handle when an enemy attacks the hero
        ActionSystem.AttachPerformer<AttackHeroGA>(AttackHeroPerformer);
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
        // Unregister the attack hero handler
        ActionSystem.DetachPerformer<AttackHeroGA>();
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
    /// It goes through each enemy and makes them attack the player.
    /// Runs as a coroutine to work with the game's timing system.
    /// </remarks>
    // Handles what happens during the enemy turn - makes all enemies attack
    private IEnumerator EnemyTurnPerformer(EnemyTurnGA enemyTurnGA)
    {
        // Loop through every enemy currently on the board
        foreach (var enemy in enemyBoardView.EnemyViews)
        {
            // Create an attack action for this enemy to attack the hero
            AttackHeroGA attackHeroGA = new(enemy);
            // Add this attack to the action queue to be processed
            ActionSystem.Instance.AddReaction(attackHeroGA);
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
    /// This method creates the visual attack sequence with animations.
    /// The enemy moves forward, deals damage, then moves back to their position.
    /// Makes combat feel more dynamic and engaging for players.
    /// </remarks>
    // Handles the visual animation and damage when an enemy attacks the hero
    private IEnumerator AttackHeroPerformer(AttackHeroGA attackHeroGA)
    {
        // Get the enemy that's performing the attack
        EnemyView attacker = attackHeroGA.Attacker;
        // Animate the enemy moving forward (attack windup) - moves left 1 unit in 0.15 seconds
        Tween tween = attacker.transform.DOMoveX(attacker.transform.position.x - 1f, 0.15f);
        // Wait for the forward movement animation to complete
        yield return tween.WaitForCompletion();
        // Animate the enemy moving back to original position - moves right 1 unit in 0.25 seconds
        attacker.transform.DOMoveX(attacker.transform.position.x + 1f, 0.25f);
        // Create a damage action using the enemy's attack power, targeting the hero
        DealDamageGA dealDamageGA = new(attacker.AttackPower, new() { HeroSystem.Instance.HeroView });
        // Add the damage action to the queue to actually hurt the hero
        ActionSystem.Instance.AddReaction(dealDamageGA);
    }
}
