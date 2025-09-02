using UnityEngine;
using System.Collections.Generic;

/* GAME ACTION DOCUMENTATION
 * 
 * Purpose: The blueprint for every action that can happen in the card game
 * 
 * How it works:
 * - Every action (playing cards, attacking, drawing, etc.) inherits from this
 * - Each action has three phases: before, during, and after
 * - Actions can trigger other actions, creating chain reactions
 * - Like building blocks that combine to create complex game effects
 * 
 * Integration: Base class for all specific actions, works with ActionSystem for processing
 */

/// <summary>
/// Base class for every action that can happen in the card game
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> The foundation that all game actions are built on</para>
/// 
/// <para><strong>What it does:</strong> This is like a template that every action in the game 
/// follows. Whether you're playing a card, attacking an enemy, drawing cards, or healing, 
/// they all inherit from this class. It makes sure every action works the same way and 
/// can have reactions that happen before, during, and after the main effect.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Any game action inherits from this class (AttackAction, HealAction, etc.)</item>
/// <item>Each action has three lists of reactions that can be triggered</item>
/// <item>PreReactions happen before the main action (like preparation effects)</item>
/// <item>PerformReactions happen during the main action (like the main effect)</item>
/// <item>PostReactions happen after the main action (like cleanup or bonus effects)</item>
/// </list>
/// 
/// <para><strong>Examples:</strong></para>
/// <list type="bullet">
/// <item>Playing a Fire Spell: PRE → gain mana, PERFORM → deal damage, POST → apply burn</item>
/// <item>Drawing a Card: PRE → check hand size, PERFORM → add to hand, POST → trigger "on draw" effects</item>
/// <item>Like a recipe with preparation, cooking, and serving steps</item>
/// </list>
/// 
/// <para><strong>Key Features:</strong></para>
/// <list type="bullet">
/// <item>Three-phase reaction system for complex interactions</item>
/// <item>Chain reaction support (actions can create more actions)</item>
/// <item>Consistent structure for all game actions</item>
/// <item>Easy to extend for new action types</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> ActionSystem processes these, all specific action classes inherit from this</para>
/// 
/// <para><strong>How to use:</strong> Create new action classes that inherit from this, add to reaction lists as needed</para>
/// </remarks>
// Base class for any action that can happen in the card game (like playing a card, dealing damage, drawing cards, etc.)
// Think of this like a "move" in chess - it's something that changes the game state
public abstract class GameAction
{
    /// <summary>
    /// Actions that happen before the main action starts
    /// </summary>
    /// <remarks>
    /// These are like preparation effects that happen first. Examples could be 
    /// gaining energy, drawing cards, or checking conditions before the main 
    /// action executes. Think of it like getting ready before doing something.
    /// </remarks>
    // Actions that happen BEFORE this main action executes
    // Example: If playing an attack card, PreReactions might be "gain 1 energy" or "draw a card"
    public List<GameAction> PreReactions {get; private set;} = new();
    
    /// <summary>
    /// Actions that happen during the main action execution
    /// </summary>
    /// <remarks>
    /// These are the main effects that happen as part of this action. For example,
    /// if this is an attack action, these might be dealing damage and applying 
    /// status effects. These happen in the middle of the action processing.
    /// </remarks>
    // Actions that happen DURING the main action execution
    // Example: If playing a fireball card, PerformReactions might be "deal 3 damage" then "apply burn effect"
    public List<GameAction> PerformReactions {get; private set;} = new();
    
    /// <summary>
    /// Actions that happen after the main action completes
    /// </summary>
    /// <remarks>
    /// These are like cleanup or bonus effects that happen at the end. Examples 
    /// could be discarding the card, ending the turn, or triggering "after" 
    /// abilities. Think of it like finishing touches after the main work is done.
    /// </remarks>
    // Actions that happen AFTER this main action completes
    // Example: After playing any card, PostReactions might be "discard this card" or "end turn if hand is empty"
    public List<GameAction> PostReactions {get; private set;} = new();

}
