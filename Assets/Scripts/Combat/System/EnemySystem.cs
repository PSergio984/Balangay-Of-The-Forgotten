using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening; // Import DOTween library for smooth animations

// This system manages all enemy behavior, turns, and combat actions in the card game
// It's like the "AI controller" that handles what enemies do during their turn
public class EnemySystem : Singleton<EnemySystem>
{
    // Reference to the visual board where enemies are displayed - assigned in Unity Inspector
    [SerializeField] private EnemyBoardView enemyBoardView;
    
    //performers - these are methods that execute specific game actions

    // Called when this GameObject becomes active - sets up action listeners
    void OnEnable()
    {
        // Register a method to handle when it's the enemy's turn
        ActionSystem.AttachPerformer<EnemyTurnGA>(EnemyTurnPerformer);
        // Register a method to handle when an enemy attacks the hero
        ActionSystem.AttachPerformer<AttackHeroGA>(AttackHeroPerformer);
    }
    
    // Called when this GameObject is disabled - cleans up action listeners to prevent memory leaks
    void OnDisable()
    {
        // Unregister the enemy turn handler
        ActionSystem.DetachPerformer<EnemyTurnGA>();
        // Unregister the attack hero handler
        ActionSystem.DetachPerformer<AttackHeroGA>();
    }
    
    // This class will manage enemy behavior and actions

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
