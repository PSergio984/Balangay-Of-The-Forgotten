using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


// The main system that handles all actions in the card game
// This is like the "game engine" that processes every move/action in order
// Similar to how Hearthstone processes card effects in a specific sequence
public class ActionSystem : Singleton<ActionSystem>
{
    // Currently executing list of reaction actions (temporary storage during processing)
    private List<GameAction> reactions = null;
    
    // Flag to prevent multiple actions from running at the same time
    // Like a "turn lock" - only one action can be processed at a time
    public bool isPerforming { get; private set; } = false;
    
    // Dictionary that stores reactions that happen BEFORE specific action types
    // Key = Type of action (like "PlayCardAction"), Value = List of reactions to that action type
    // Example: "Before any attack card is played, gain 1 block"
    private static Dictionary<Type, List<Action<GameAction>>> preSubs = new();
    
    // Dictionary that stores reactions that happen AFTER specific action types  
    // Example: "After any spell is cast, deal 1 damage to all enemies"
    private static Dictionary<Type, List<Action<GameAction>>> postSubs = new();
    
    // Dictionary that stores the main logic for how each action type actually executes
    // Key = Type of action, Value = Function that defines what that action actually does
    // Example: AttackAction -> "reduce target's health by damage amount"
    private static Dictionary<Type, Func<GameAction,IEnumerator>> performers = new();

    // Main method to execute any action in the game
    // This is like pressing "play" on a card or ability
    // OnPerformFinished = callback function to run when the entire action chain is complete
    public void Perform(GameAction action,System.Action OnPerformFinished = null)
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
    
    // Method for other systems to add additional reactions during action processing
    // This is used internally during the Flow to queue up additional reactions
    public void AddReaction(GameAction gameAction)
    {
        reactions?.Add(gameAction);
    }

    // The main action processing pipeline - this is the heart of the system
    // This processes actions in the correct order: Pre -> Main -> Post (like a assembly line)
    // Similar to how Hearthstone processes card effects in phases
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

        // All phases complete - run any cleanup code
        OnFlowFinished?.Invoke();
    }

    // Recursively processes all reactions in the current reactions list
    // Each reaction is itself a GameAction, so it goes through the full Flow process
    // This creates a chain reaction system (like combo effects in card games)
    private IEnumerator PerformReactions()
    {
        // Process each reaction one by one (important for turn-based timing)
        foreach(var reaction in reactions)
        {
            // Each reaction goes through the full Flow process (Pre->Main->Post)
            yield return Flow(reaction);
        }
    }
    
    // Executes the main logic/effect of an action
    // This is where the actual "meat" of what an action does happens
    // Example: AttackAction actually reduces the target's health here
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

    // Registers custom logic for how a specific action type should execute
    // This is like defining what happens when you play a specific type of card
    // Example: AttachPerformer<AttackAction>(attack => DealDamage(attack.target, attack.damage))
    public static void AttachPerformer<T>(Func<T, IEnumerator> performer) where T : GameAction
    {
        // Get the type of action we're registering logic for
        Type type = typeof(T);
        
        // Create a wrapper function that converts the generic GameAction to the specific type
        IEnumerator wrappedPerformer(GameAction action)=> performer((T)action);
        
        // Check if we already have logic registered for this action type
        if(performers.ContainsKey(type)){
            // Add to existing logic (allows for multiple effects on one action type)
            performers[type] += wrappedPerformer;
        }
        else{
            // Register new logic for this action type
            performers.Add(type, wrappedPerformer);
        }
    }
    
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

    // Registers a global reaction that triggers whenever a specific action type happens
    // This is how you implement passive abilities and triggered effects
    // Example: SubscribeReaction<AttackAction>(attack => player.GainEnergy(1), ReactionTiming.Post)
    // Means: "After any attack, the player gains 1 energy"
    public static void SubscribeReaction<T>(Action<T> reaction,ReactionTiming timing) where T: GameAction
    {
        // Choose the correct dictionary based on timing (before or after the action)
        Dictionary<Type,List<Action<GameAction>>> subs = timing == ReactionTiming.PRE ? preSubs : postSubs;
        
        // Create a wrapper function that converts GameAction to the specific type
        void wrappedReaction(GameAction action) => reaction((T)action);
        
        // Check if we already have reactions registered for this action type
        if(subs.ContainsKey(typeof(T))){
            // Add this reaction to the existing list
            subs[typeof(T)].Add(wrappedReaction);
        }
        else{
            // Create a new list for this action type and add our reaction
            subs.Add(typeof(T),new ());
            subs[typeof(T)].Add(wrappedReaction);
        }
    }

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