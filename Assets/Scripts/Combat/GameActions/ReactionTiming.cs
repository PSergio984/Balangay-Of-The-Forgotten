using UnityEngine;

// Enum to specify when a reaction should trigger during an action
// Pre = before the main action, Post = after the main action
public enum ReactionTiming
{
    PRE,  // Happens before the main action (like "whenever you play a card, gain 1 energy")
    POST  // Happens after the main action (like "whenever you deal damage, draw a card")
}

