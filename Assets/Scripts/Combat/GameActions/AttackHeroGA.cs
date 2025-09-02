using UnityEngine;

// Game Action that represents an enemy attacking the hero player
// This is like a "combat move" that gets processed by the action system
public class AttackHeroGA : GameAction
{
    // The enemy that is performing the attack - stored as reference
    public EnemyView Attacker { get; private set; }
    
    // Constructor - creates a new attack action with the specified enemy as attacker
    public AttackHeroGA(EnemyView attacker)
    {
        // Store which enemy is doing the attacking
        Attacker = attacker;
    }
}
