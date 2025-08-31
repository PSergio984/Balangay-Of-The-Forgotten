using UnityEngine;
using System.Collections.Generic;

// Base class for any action that can happen in the card game (like playing a card, dealing damage, drawing cards, etc.)
// Think of this like a "move" in chess - it's something that changes the game state
public abstract class GameAction
{
    // Actions that happen BEFORE this main action executes
    // Example: If playing an attack card, PreReactions might be "gain 1 energy" or "draw a card"
    public List<GameAction> PreReactions {get; private set;} = new();
    
    // Actions that happen DURING the main action execution
    // Example: If playing a fireball card, PerformReactions might be "deal 3 damage" then "apply burn effect"
    public List<GameAction> PerformReactions {get; private set;} = new();
    
    // Actions that happen AFTER this main action completes
    // Example: After playing any card, PostReactions might be "discard this card" or "end turn if hand is empty"
    public List<GameAction> PostReactions {get; private set;} = new();

}
