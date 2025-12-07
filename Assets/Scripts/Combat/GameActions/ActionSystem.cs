using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/* ACTION SYSTEM DOCUMENTATION
 * 
 * Purpose: The brain of the card game that handles every action and reaction
 * 
 * How it works:
 * - Processes all game actions in the right order (like playing cards, attacking, etc.)
 * - Makes sure only one action happens at a time (turn-based rules)
 * - Handles reactions that happen before, during, and after each action
 * - Works like a chain reaction system for combo effects
 * 
 * Integration: The core system that all other game systems use to do things
 */

/// <summary>
/// The main system that handles all actions and reactions in the card game
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Acts like the game's brain that processes every action in the right order</para>
/// 
/// <para><strong>What it does:</strong> This system is like the engine of the card game. 
/// When anything happens (playing a card, attacking, healing, etc.), it goes through this 
/// system. It makes sure everything happens in the right order and that reactions 
/// (like "when you play a spell, draw a card") work correctly.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Something creates an action (like "play this card")</item>
/// <item>System processes PRE reactions (things that happen before the action)</item>
/// <item>System executes the main action (the actual card effect)</item>
/// <item>System processes POST reactions (things that happen after the action)</item>
/// <item>Each reaction can trigger more reactions, creating chain effects</item>
/// </list>
/// 
/// <para><strong>Examples:</strong></para>
/// <list type="bullet">
/// <item>Player plays attack card → PRE: gain energy → MAIN: deal damage → POST: draw card</item>
/// <item>Enemy dies → triggers "when enemy dies" effects → might trigger more effects</item>
/// <item>Like Hearthstone's action processing but simpler to understand</item>
/// </list>
/// 
/// <para><strong>Key Features:</strong></para>
/// <list type="bullet">
/// <item>Turn-based locking (only one action at a time)</item>
/// <item>Three-phase processing (Pre → Main → Post)</item>
/// <item>Chain reaction system for combos</item>
/// <item>Custom action types with their own logic</item>
/// <item>Global passive abilities that react to any action</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> Every other system in the game uses this to do actions</para>
/// 
/// <para><strong>How to use:</strong> Other systems call Perform() to do actions, register logic with AttachPerformer()</para>
/// </remarks>
// The main system that handles all actions in the card game
// This is like the "game engine" that processes every move/action in order
// Similar to how Hearthstone processes card effects in a specific sequence
public class ActionSystem : Singleton<ActionSystem>
{
    /// <summary>
    /// Currently executing list of reaction actions (temporary storage during processing)
    /// </summary>
    /// <remarks>
    /// This holds reactions while they're being processed. Gets swapped out for different 
    /// phases (pre, perform, post) as the action flows through the system.
    /// </remarks>
    // Currently executing list of reaction actions (temporary storage during processing)
    private List<GameAction> reactions = null;
    
    /// <summary>
    /// Flag to prevent multiple actions from running at the same time
    /// </summary>
    /// <remarks>
    /// Like a "turn lock" - only one action can be processed at a time.
    /// Prevents chaos from multiple actions trying to execute simultaneously.
    /// </remarks>
   
    public bool isPerforming { get; private set; } = false;
    
    /// <summary>
    /// Dictionary that stores reactions that happen BEFORE specific action types
    /// </summary>
    /// <remarks>
    /// Key = Type of action (like "PlayCardAction"), Value = List of reactions to that action type.
    /// Example: "Before any attack card is played, gain 1 block"
    /// </remarks>
    // Dictionary that stores reactions that happen BEFORE specific action types
    // Key = Type of action (like "PlayCardAction"), Value = List of reactions to that action type
    // Example: "Before any attack card is played, gain 1 block"
    private static Dictionary<Type, List<Action<GameAction>>> preSubs = new();
    
    /// <summary>
    /// Dictionary that stores reactions that happen AFTER specific action types
    /// </summary>
    /// <remarks>
    /// Example: "After any spell is cast, deal 1 damage to all enemies"
    /// </remarks>
    // Dictionary that stores reactions that happen AFTER specific action types  
    // Example: "After any spell is cast, deal 1 damage to all enemies"
    private static Dictionary<Type, List<Action<GameAction>>> postSubs = new();
    
    /// <summary>
    /// Dictionary that stores the main logic for how each action type actually executes
    /// </summary>
    /// <remarks>
    /// Key = Type of action, Value = Function that defines what that action actually does.
    /// Example: AttackAction -> "reduce target's health by damage amount"
    /// </remarks>
    private static Dictionary<Type, Func<GameAction,IEnumerator>> performers = new();

    /// <summary>
    /// Main method to execute any action in the game
    /// </summary>
    /// <param name="action">The action to perform</param>
    /// <param name="OnPerformFinished">Callback function to run when the entire action chain is complete</param>
    /// <remarks>
    /// This is like pressing "play" on a card or ability.
    /// OnPerformFinished = callback function to run when the entire action chain is complete
    /// </remarks>
    // Main method to execute any action in the game
    // This is like pressing "play" on a card or ability
    // OnPerformFinished = callback function to run when the entire action chain is complete
    public void Perform(GameAction action, System.Action OnPerformFinished = null)
    {
        // Prevent multiple actions from running simultaneously (like turn-based rules)
        if(isPerforming) return;
        
        // Lock the system while processing this action
        isPerforming = true;
        
        // Start the action processing chain as a coroutine (allows for animations/delays)
        StartCoroutine(Flow(action,() => {
            // Unlock the system when done
            isPerforming = false;
            // Run any cleanup code that was passed in
            OnPerformFinished?.Invoke();
        }));
    }
    
    /// <summary>
    /// Method for other systems to add additional reactions during action processing
    /// </summary>
    /// <param name="gameAction">The reaction action to add to the current processing queue</param>
    /// <remarks>
    /// This is used internally during the Flow to queue up additional reactions.
    /// WARNING: This method should only be called during active Flow() execution.
    /// If called outside of Flow(), the action will be lost and a warning will be logged.
    /// </remarks>
    // Method for other systems to add additional reactions during action processing
    // This is used internally during the Flow to queue up additional reactions
    public void AddReaction(GameAction gameAction)
    {
        // Skip null actions (some effects like ConditionalEffect return null when condition isn't met)
        if (gameAction == null)
        {
            return;
        }
        
        // Check if we're in an active Flow execution using the isPerforming flag
        // This is the reliable indicator of whether reactions can be added
        if (!isPerforming)
        {
            Debug.LogWarning($"[ActionSystem] AddReaction() called outside of active Flow() execution. " +
                           $"Action '{gameAction.GetType().Name}' will be lost. " +
                           $"Ensure AddReaction() is only called during action processing (inside performers or reactions).");
            return;
        }
        
        // Defensive check: reactions should always be set during Flow execution, but verify it's not null
        if (reactions == null)
        {
            Debug.LogError($"[ActionSystem] reactions list is null during active Flow execution. " +
                         $"This indicates a bug in the ActionSystem. Action '{gameAction.GetType().Name}' will be lost.");
            return;
        }
        
        reactions.Add(gameAction);
    }

    /// <summary>
    /// The main action processing pipeline - this is the heart of the system
    /// </summary>
    /// <param name="action">The action to process through all phases</param>
    /// <param name="OnFlowFinished">Callback to run when all phases are complete</param>
    /// <returns>Coroutine that processes through Pre -> Main -> Post phases</returns>
    /// <remarks>
    /// This processes actions in the correct order: Pre -> Main -> Post (like a assembly line).
    /// </remarks>
    private IEnumerator Flow(GameAction action, Action OnFlowFinished = null)
    {
        // PHASE 1: Process all PRE-reactions (things that happen before the main action)
        // Example: "Before you play any card, gain 1 energy"
        reactions = action.PreReactions;
        PerformSubscribers(action, preSubs);  // Trigger any global pre-reactions
        yield return PerformReactions();      // Execute all queued pre-reactions

        // PHASE 2: Process the MAIN action and its perform reactions
        // Example: The actual card effect like "Deal 5 damage"  
        reactions = action.PerformReactions;
        yield return PerformPerformer(action);  // Execute the main action logic
        yield return PerformReactions();        // Execute any reactions that happen during main action

        // PHASE 3: Process all POST-reactions (things that happen after the main action)
        // Example: "After you deal damage, draw a card"
        reactions = action.PostReactions;
        PerformSubscribers(action, postSubs);  // Trigger any global post-reactions  
        yield return PerformReactions();       // Execute all queued post-reactions

        // Reset reactions to null after Flow completes to prevent stale references
        // This ensures AddReaction() can properly detect when called outside of Flow execution
        reactions = null;

        // All phases complete - run any cleanup code
        OnFlowFinished?.Invoke();
    }

    /// <summary>
    /// Recursively processes all reactions in the current reactions list
    /// </summary>
    /// <returns>Coroutine that processes each reaction through the full Flow</returns>
    /// <remarks>
    /// Each reaction is itself a GameAction, so it goes through the full Flow process.
    /// This creates a chain reaction system (like combo effects in card games)
    /// </remarks>
    // Recursively processes all reactions in the current reactions list
    // Each reaction is itself a GameAction, so it goes through the full Flow process
    // This creates a chain reaction system (like combo effects in card games)
    private IEnumerator PerformReactions()
    {
        // Process each reaction one by one (important for turn-based timing)
        foreach(var reaction in reactions)
        {
            // Skip null reactions (defensive check - should not happen if AddReaction is used correctly)
            if (reaction == null)
            {
                Debug.LogWarning("[ActionSystem] Skipping null reaction in PerformReactions");
                continue;
            }
            
            // Each reaction goes through the full Flow process (Pre->Main->Post)
            yield return Flow(reaction);
        }
    }
    
    /// <summary>
    /// Executes the main logic/effect of an action
    /// </summary>
    /// <param name="action">The action whose main effect should be executed</param>
    /// <returns>Coroutine that runs the action's custom logic</returns>
    /// <remarks>
    /// This is where the actual  action does happens.
    ///  Executes the main logic/effect of an action
    /// Example: AttackAction actually reduces the target's health here
    /// </remarks>

    private IEnumerator PerformPerformer(GameAction action)
    {
        // Get the specific type of this action (AttackAction, HealAction, etc.)
        Type type = action.GetType();
        
        // Check if we have custom logic registered for this action type
        if(performers.ContainsKey(type))
        {
            // Execute the custom logic function for this action type
            yield return performers[type](action);
        }
        // If no custom logic is registered, the action does nothing (like a placeholder)
    }
    
    /// <summary>
    /// Triggers all global reactions that are subscribed to this specific action type
    /// </summary>
    /// <param name="action">The action that triggered these reactions</param>
    /// <param name="subs">Dictionary of subscribed reactions (either pre or post)</param>
    /// <remarks>
    /// This is how passive abilities work: "Whenever you play a spell, gain 1 mana"
    /// </remarks>
    // Triggers all global reactions that are subscribed to this specific action type
    // This is how passive abilities work: "Whenever you play a spell, gain 1 mana"
    private void PerformSubscribers(GameAction action, Dictionary<Type, List<Action<GameAction>>> subs)
    {
        // Get the specific type of action being performed
        Type type = action.GetType();
        
        // Check if any global reactions are subscribed to this action type
        if(subs.ContainsKey(type)){
            // Execute each subscribed reaction function
            foreach(var sub in subs[type])
            {
                // Call the reaction function, passing in the action that triggered it
                sub(action);
            }
        }
    }

    /// <summary>
    /// Registers custom logic for how a specific action type should execute
    /// </summary>
    /// <typeparam name="T">The specific action type to register logic for, it needs to inherit from <see cref="GameAction"/>
    /// it basically enforces that T is a GameAction or subclass of GameAction and method can only be called with those types
    /// </typeparam>
    /// <typeparam name="Func<T, IEnumerator>">it needs to have a param of a class that inherits from GameAction 
    /// and return it as IEnumerator for coroutines</typeparam>
    /// <param name="performer">Function that defines what this action type does, </param>
    /// <remarks>
    /// This is like defining what happens when you play a specific type of card.
    /// Example: AttachPerformer&lt;AttackAction&gt;(attack => DealDamage(attack.target, attack.damage))
    /// </remarks>
    // Registers custom logic for how a specific action type should execute
    // This is like defining what happens when you play a specific type of card
    // Example: AttachPerformer<AttackAction>(attack => DealDamage(attack.target, attack.damage))

    public static void AttachPerformer<T>(Func<T, IEnumerator> performer) where T : GameAction
    {
        // Get the type of action we're registering logic for
        Type type = typeof(T);

        // Create a wrapper function that converts the generic GameAction to the specific type
        IEnumerator wrappedPerformer(GameAction action) => performer((T)action);

        // Check if we already have logic registered for this action type
        if (performers.ContainsKey(type))
        {
            // Add to existing logic
            // Useful for UI and animations like adding visual effects on top of core logic
            performers[type] += wrappedPerformer;
        }
        else
        {
            // Register new logic for this action type
            performers.Add(type, wrappedPerformer);
        }
    }
    
    /// <summary>
    /// Removes all custom logic for a specific action type
    /// </summary>
    /// <typeparam name="T">The action type to remove logic for</typeparam>
    /// <remarks>
    /// This is like "disabling" a card type or removing a game mechanic
    /// </remarks>
    // Removes all custom logic for a specific action type
    // This is like "disabling" a card type or removing a game mechanic
    public static void DetachPerformer<T>() where T: GameAction 
    {
        Type type = typeof(T);
        // Remove the action type from our performers dictionary
        if(performers.ContainsKey(type)){
            performers.Remove(type);
        }
    }

    /// <summary>
    /// Registers a global reaction that triggers whenever a specific action type happens
    /// </summary>
    /// <typeparam name="T">The action type to react to</typeparam>
    /// <param name="reaction">Function to call when this action type happens</param>
    /// <param name="timing">Whether to react before or after the action</param>
    /// <remarks>
    /// This is how you implement passive abilities and triggered effects.
    /// Example: SubscribeReaction&lt;AttackAction&gt;(attack => player.GainEnergy(1), ReactionTiming.Post)
    /// Means: "After any attack, the player gains 1 energy"
    /// </remarks>
    public static void SubscribeReaction<T>(Action<T> reaction,ReactionTiming timing) where T: GameAction
    {
        // Choose the correct dictionary based on timing (before or after the action)
        // then add the reaction to that dictionary
        Dictionary<Type, List<Action<GameAction>>> subs = timing == ReactionTiming.PRE ? preSubs : postSubs;
        
        // Create a wrapper function that converts GameAction to the specific type
        void wrappedReaction(GameAction action) => reaction((T)action);
        
        // Check if we already have reactions registered for this action type
        if(subs.ContainsKey(typeof(T))){
            // Add this reaction to the existing list
            subs[typeof(T)].Add(wrappedReaction);
        }
        else{//If not → make a new list and add it.
            // Create a new list for this action type 
            subs.Add(typeof(T),new ());
            //and add our reaction
            subs[typeof(T)].Add(wrappedReaction);
        }
    }

    /// <summary>
    /// Removes a specific global reaction from the system
    /// </summary>
    /// <typeparam name="T">The action type to stop reacting to</typeparam>
    /// <param name="reaction">The reaction function to remove</param>
    /// <param name="timing">Whether this was a pre or post reaction</param>
    /// <remarks>
    /// This is like "losing" a passive ability or removing a triggered effect.
    /// Note: This currently has a bug - it creates a new function that won't match the original
    /// </remarks>
    // Removes a specific global reaction from the system
    // This is like "losing" a passive ability or removing a triggered effect
    // Note: This currently has a bug - it creates a new function that won't match the original
    public static void UnsubscribeReaction<T>(Action<T> reaction, ReactionTiming timing) where T : GameAction
    {
        // Choose the correct dictionary based on timing
        Dictionary<Type,List<Action<GameAction>>> subs = timing == ReactionTiming.PRE ? preSubs : postSubs;
        
        // Check if this action type has any registered reactions
        if(subs.ContainsKey(typeof(T))){
            // Create a wrapper (BUG: this won't match the original wrapper from SubscribeReaction)
            void wrappedReaction(GameAction action) => reaction((T)action);
            // Try to remove it (this likely won't work due to the wrapper mismatch)
            subs[typeof(T)].Remove(wrappedReaction);
        }
    }
}