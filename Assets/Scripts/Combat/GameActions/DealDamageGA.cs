using System.Collections.Generic; 
using UnityEngine;

// Game Action that represents dealing damage to one or more targets
// This is used when attacks, spells, or abilities need to hurt characters
public class DealDamageGA : GameAction
{
    // The amount of damage this action will deal to targets
    public int Amount { get; set; }
    // List of all combatants (heroes/enemies) that will receive this damage
    public List<CombatantView> Targets { get; set; }
    // Constructor that creates a new damage action with specified amount and targets
    public DealDamageGA(int amount, List<CombatantView> targets)
    {
        // Store the damage amount to be dealt
        Amount = amount;
        // Create a new list copy of the targets to avoid reference issues
        Targets = new(targets);
    }
}
