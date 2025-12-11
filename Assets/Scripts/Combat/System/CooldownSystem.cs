using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* COOLDOWN SYSTEM DOCUMENTATION
 * 
 * Purpose: Manages card cooldowns for all heroes
 * 
 * How it works:
 * - Tracks when cards are played and starts their cooldown
 * - Reduces cooldowns ONCE per ROUND (after enemy turn completes)
 * - Cards that were just played THIS ROUND skip the first reduction
 * - This ensures cooldowns count full rounds correctly in multi-hero gameplay
 * - Notifies CardViews when cooldown state changes for UI updates
 * - Integrates with ActionSystem for turn-based cooldown management
 * 
 * IMPORTANT: Cooldowns reduce ONCE per ROUND (after enemy turn), NOT once per hero turn.
 * This prevents cooldowns from being reduced multiple times when multiple heroes act.
 * 
 * Cooldown Timing Example (1-turn cooldown, multi-hero):
 * - Round 1: Hero 0 plays card → cooldown = 1, justPlayedThisTurn = true
 * - Round 1: Heroes 1, 2, 3 take their turns (cooldown unchanged)
 * - Round 1 End: Enemy turn completes → ReduceCooldown skips this card → cooldown stays 1
 * - Round 2: Hero 0's card drawn with cooldown = 1 → card unavailable, shows "1" on badge
 * - Round 2 End: Enemy turn completes → cooldown reduced to 0
 * - Round 3: Hero 0's card drawn with cooldown = 0 → card is now playable
 * 
 * Cooldown Timing Example (2-turn cooldown, multi-hero):
 * - Round 1: Hero plays card → cooldown = 2, justPlayedThisTurn = true
 * - Round 1 End: Enemy turn completes → skip (just played) → cooldown stays 2
 * - Round 2: Card drawn with cooldown = 2 → unavailable, shows "2"
 * - Round 2 End: Enemy turn completes → cooldown reduced to 1
 * - Round 3: Card drawn with cooldown = 1 → unavailable, shows "1"
 * - Round 3 End: Enemy turn completes → cooldown reduced to 0
 * - Round 4: Card drawn with cooldown = 0 → card is now playable
 * 
 * Integration: Works with CardSystem, CardView, ActionSystem for cooldown handling
 */

/// <summary>
/// System that manages card cooldowns for all heroes
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Controls card cooldown state and UI updates</para>
/// 
/// <para><strong>What it does:</strong> This system manages the cooldown mechanic for cards.
/// When a card with cooldown is played, it becomes unusable for a number of rounds.
/// The cooldown decreases each turn until it reaches 0 and the card becomes playable again.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Listens for PlayCardsGA to start cooldown when cards are played</item>
/// <item>Listens for EnemyTurnGA (POST) to reduce cooldowns after enemy turn</item>
/// <item>Fires events when cooldown changes so CardViews can update visuals</item>
/// <item>Provides methods to check if a card can be played</item>
/// </list>
/// 
/// <para><strong>Needs:</strong> ActionSystem for processing cooldown actions</para>
/// 
/// <para><strong>Works with:</strong> CardSystem, CardView, ActionSystem</para>
/// </remarks>
public class CooldownSystem : Singleton<CooldownSystem>
{
    /// <summary>
    /// Event fired when any card's cooldown changes
    /// </summary>
    /// <remarks>
    /// CardViews subscribe to this to update their visual state.
    /// Passes the Card that changed so views can check if it's their card.
    /// </remarks>
    public static event Action<Card> OnCooldownChanged;
    
    /// <summary>
    /// Event fired when all cooldowns are reduced (at turn start)
    /// </summary>
    /// <remarks>
    /// Fired after all cards have had their cooldowns reduced.
    /// UI can use this to refresh all card displays at once.
    /// </remarks>
    public static event Action OnAllCooldownsReduced;

    /// <summary>
    /// Sets up event handlers when this system becomes active
    /// </summary>
    void OnEnable()
    {
        // Register performer for reduce cooldown action
        ActionSystem.AttachPerformer<ReduceCooldownGA>(ReduceCooldownPerformer);
        
        // Subscribe to PlayCardsGA to start cooldown when cards are played
        ActionSystem.SubscribeReaction<PlayCardsGA>(OnCardPlayed, ReactionTiming.POST);
        
        // Subscribe to EnemyTurnGA POST to reduce cooldowns ONCE per round after ALL heroes have acted
        // This ensures cooldowns are only reduced once per round, not once per hero turn.
        //
        // Cooldown timing with JustPlayedThisTurn flag:
        // - Round 1: Hero plays card → cooldown = 1, JustPlayedThisTurn = true
        // - Round 1 End: Enemy turn completes → cooldown skipped (just played flag set)
        // - Round 2: Card drawn with cooldown = 1 → card unavailable
        // - Round 2 End: Enemy turn completes → cooldown reduced to 0
        // - Round 3: Card drawn with cooldown = 0 → card available
        ActionSystem.SubscribeReaction<EnemyTurnGA>(OnRoundEnd, ReactionTiming.POST);
    }
    
    /// <summary>
    /// Cleans up event handlers when this system becomes inactive
    /// </summary>
    void OnDisable()
    {
        ActionSystem.DetachPerformer<ReduceCooldownGA>();
        ActionSystem.UnsubscribeReaction<PlayCardsGA>(OnCardPlayed, ReactionTiming.POST);
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(OnRoundEnd, ReactionTiming.POST);
    }
    
    /// <summary>
    /// Handles starting cooldown when a card is played
    /// </summary>
    /// <param name="playCardsGA">The play card action that just completed</param>
    /// <remarks>
    /// Only starts cooldown if the card was actually played (not cancelled due to failed conditions).
    /// If a conditional card's condition fails, it remains in hand, so we check if the card is still
    /// in hand - if it is, the play was cancelled and we don't set cooldown.
    /// </remarks>
    private void OnCardPlayed(PlayCardsGA playCardsGA)
    {
        Card card = playCardsGA.Card;
        
        // Check if card is still in hand - if so, the play was cancelled (condition failed)
        // Only set cooldown if the card was actually played (removed from hand)
        if (CardSystem.Instance != null && CardSystem.Instance.IsCardInHand(card))
        {
            Debug.Log($"[CooldownSystem] Card '{card.Title}' play was cancelled (condition failed) - not setting cooldown");
            return; // Card is still in hand, play was cancelled, don't set cooldown
        }
        
        // Card was successfully played (not in hand anymore) - start cooldown if it has one
        if (card.BaseCooldown > 0)
        {
            card.StartCooldown();
            Debug.Log($"[CooldownSystem] Card '{card.Title}' cooldown started: {card.CurrentCooldown} rounds");
            
            // Notify listeners that this card's cooldown changed
            OnCooldownChanged?.Invoke(card);
        }
    }
    
    /// <summary>
    /// Handles reducing cooldowns at the end of each ROUND (after enemy turn)
    /// </summary>
    /// <param name="enemyTurnGA">The enemy turn action that just completed</param>
    /// <remarks>
    /// Triggered ONCE per round after the enemy turn completes.
    /// This ensures cooldowns are reduced exactly once per round, not once per hero turn.
    /// 
    /// Combined with JustPlayedThisTurn flag on cards:
    /// - Round 1: Hero plays card → cooldown = 1, JustPlayedThisTurn = true
    /// - Round 1 End: This method runs → card's JustPlayedThisTurn flag clears but cooldown NOT reduced
    /// - Round 2: Card drawn with cooldown = 1 → card unavailable
    /// - Round 2 End: This method runs → cooldown reduced to 0
    /// - Round 3: Card available
    /// </remarks>
    private void OnRoundEnd(EnemyTurnGA enemyTurnGA)
    {
        // Create and queue a reduce cooldown action for all heroes
        ReduceCooldownGA reduceCooldownGA = new ReduceCooldownGA(-1);
        ActionSystem.Instance.AddReaction(reduceCooldownGA);
    }
    
    /// <summary>
    /// Processes reducing cooldowns for all cards
    /// </summary>
    /// <param name="reduceCooldownGA">The reduce cooldown action to process</param>
    /// <returns>IEnumerator for coroutine execution</returns>
    private IEnumerator ReduceCooldownPerformer(ReduceCooldownGA reduceCooldownGA)
    {
        // Get all cards that need cooldown reduction
        // This includes cards in hand, discard pile, and draw pile
        List<Card> allCards = GetAllCardsForCooldownReduction(reduceCooldownGA.HeroIndex);
        
        int cardsReduced = 0;
        int cardsSkipped = 0;
        foreach (Card card in allCards)
        {
            if (card.IsOnCooldown)
            {
                bool wasReduced = card.ReduceCooldown();
                
                if (wasReduced)
                {
                    cardsReduced++;
                    Debug.Log($"[CooldownSystem] Card '{card.Title}' cooldown reduced to: {card.CurrentCooldown}");
                }
                else
                {
                    cardsSkipped++;
                    Debug.Log($"[CooldownSystem] Card '{card.Title}' cooldown skipped (just played this turn), stays at: {card.CurrentCooldown}");
                }
                
                // Notify listeners that this card's cooldown changed
                OnCooldownChanged?.Invoke(card);
            }
        }
        
        if (cardsReduced > 0 || cardsSkipped > 0)
        {
            Debug.Log($"[CooldownSystem] Reduced cooldown for {cardsReduced} cards, skipped {cardsSkipped} cards (just played)");
        }
        
        // Notify that all cooldowns have been processed
        OnAllCooldownsReduced?.Invoke();
        
        yield return null;
    }
    
    /// <summary>
    /// Gets all cards that should have cooldown reduced
    /// </summary>
    /// <param name="heroIndex">The hero index, or -1 for all heroes</param>
    /// <returns>List of all cards to process</returns>
    private List<Card> GetAllCardsForCooldownReduction(int heroIndex)
    {
        // Get all cards from CardSystem (includes cards in draw pile, hand, and discard pile)
        if (CardSystem.Instance != null)
        {
            return CardSystem.Instance.GetAllCardsForHero(heroIndex);
        }
        
        Debug.LogWarning("[CooldownSystem] CardSystem.Instance is null, cannot get cards for cooldown reduction");
        return new List<Card>();
    }
    
    /// <summary>
    /// Checks if a card can be played (not on cooldown)
    /// </summary>
    /// <param name="card">The card to check</param>
    /// <returns>True if the card can be played, false if on cooldown</returns>
    public bool CanPlayCard(Card card)
    {
        return !card.IsOnCooldown;
    }
    
    /// <summary>
    /// Notifies listeners that a card's cooldown changed (public method for external systems)
    /// </summary>
    /// <param name="card">The card whose cooldown changed</param>
    public void NotifyCooldownChanged(Card card)
    {
        OnCooldownChanged?.Invoke(card);
    }
}
