using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* HERO SYSTEM DOCUMENTATION
 *
 * Purpose: Manages the party of player heroes and their stats
 *
 * How it works:
 * - Holds reference to all hero visual displays (multi-hero/party support)
 * - Sets up each hero with their starting stats and appearance
 * - Provides access to all hero information for other systems
 * - Manages per-hero state during combat (health, status, hand, etc.)
 * - Handles turn cycle reactions for each hero: discarding/drawing hands, burn effects, etc.
 *
 * Integration: Works with HeroView for display and other systems for hero interactions
 *
 * DESIGN CHANGE - Why enemy turn reactions moved here:
 * Previously CardSystem handled discarding/drawing cards during enemy turns.
 * This was moved to HeroSystem because these reactions are about per-hero state management, not card mechanics.
 * The party system requires each hero to have their own hand and burn logic. This makes HeroSystem a general-purpose
 * party/hero state manager rather than having hero-related logic scattered in other systems.
 */

/// <summary>
/// System that manages the party of player heroes and their information
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Controls all player heroes and their stats in a party-based system</para>
///
/// <para><strong>What it does:</strong> This system manages everything about the player's party of heroes.
/// It sets up each hero's starting health, appearance, and other stats. Other parts of the game can use this system
/// to check or affect any hero during combat.</para>
///
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Game starts and all hero data gets loaded</item>
/// <item>System sets up each hero with their stats and appearance</item>
/// <item>HeroBoardView displays all heroes to the player</item>
/// <item>Other systems can access any hero info through this system</item>
/// </list>
///
/// <para><strong>Needs:</strong> HeroView component for displaying each hero, HeroData for stats</para>
///
/// <para><strong>Works with:</strong> HeroView for display, combat systems for health/damage</para>
///
/// <para><strong>How to use:</strong> Put this on a GameObject and assign the HeroBoardView in Inspector</para>
/// </remarks>
public class HeroSystem : Singleton<HeroSystem>
{
    // Expose all hero views for targeting
    public List<HeroView> HeroViews => HeroBoardView.HeroViews;
    
    /// <summary>
    /// Stored hero data list for accessing turn profile overrides
    /// </summary>
    private List<HeroData> storedHeroDatas;
    
    /// <summary>
    /// The visual display component that shows the hero to players
    /// </summary>
    /// <remarks>
    /// This property holds the HeroView component that displays the hero character.
    /// Other systems can access this to affect the hero's appearance or get hero info.
    /// Assign a HeroView GameObject in the Inspector.
    /// </remarks>
    [field: SerializeField] private HeroBoardView HeroBoardView;

    /// <summary>
    /// Sets up the hero character with their starting information
    /// </summary>
    /// <param name="heroData">The hero's stats, appearance, and starting values</param>
    /// <remarks>
    /// This method initializes the hero with all their starting information like health,
    /// appearance, name, and abilities. Called at the beginning of combat to prepare the hero.
    /// </remarks>
    /// 
    /// 

    public void Setup(List<HeroData> heroDatas)
        {
            // Store hero datas for turn profile access
            storedHeroDatas = heroDatas;
            
            foreach (var heroData in heroDatas)
            {
                // Tell the hero view to set up the hero's appearance and stats
                HeroBoardView.AddHero(heroData);
            }
        }
    
    /// <summary>
    /// Gets the turn profile override for the hero at the specified index
    /// </summary>
    /// <param name="heroIndex">Index of the hero (0-based)</param>
    /// <returns>The AnimatorOverrideController for the hero's turn profile, or null if not found</returns>
    public AnimatorOverrideController GetHeroTurnProfileOverride(int heroIndex)
    {
        if (storedHeroDatas == null || heroIndex < 0 || heroIndex >= storedHeroDatas.Count)
        {
            Debug.LogWarning($"[HeroSystem] Cannot get turn profile override for hero index {heroIndex}");
            return null;
        }
        if (storedHeroDatas[heroIndex] == null)
        {
            Debug.LogWarning($"[HeroSystem] HeroData at index {heroIndex} is null. Cannot get turn profile override.");
            return null;
        }
        return storedHeroDatas[heroIndex].TurnProfileOverride;
    }
    /// <summary>
    /// Subscribe to enemy turn reactions when this system starts
    /// </summary>
    /// <remarks>
    /// Sets up reactions to enemy turns for hero state management.
    /// PRE reaction: Discard hand to reset for new turn
    /// POST reaction: Apply burn damage and draw new hand
    /// 
    /// MOVED FROM CARD SYSTEM: These reactions used to be in CardSystem but were
    /// moved here because they're about hero state management, not card mechanics.
    /// Makes HeroSystem a more complete hero state manager.
    /// </remarks>
    void OnEnable()
    {
          // Listen for enemy turns to handle cards automatically
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

    /// <summary>
    /// Unsubscribe from reactions when this system stops to prevent errors
    /// </summary>
    void OnDisable()
    {
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

     /// <summary>
    /// REACTIONS
    /// Reacts to enemy turn start by discarding all cards in hand
    /// </summary>
    /// <param name="enemyTurnGA">The enemy turn action that triggered this reaction</param>
    /// <remarks>
    /// This reaction happens before the enemy turn fully begins.
    /// Automatically discards all cards in the player's hand to clear it for the next turn.
    /// Part of the turn cycle management to reset the player's hand state.
    /// </remarks>
    private void EnemyTurnPreReaction(EnemyTurnGA enemyTurnGA)
    {
        // CRITICAL: Check if this object is still valid before proceeding
        // This prevents MissingReferenceException if HeroSystem was destroyed during scene transition
        if (this == null || !this)
        {
            Debug.LogWarning("[HeroSystem] EnemyTurnPreReaction called but HeroSystem has been destroyed. Skipping reaction.");
            return;
        }
        
        // Check if ActionSystem is still valid
        if (ActionSystem.Instance == null)
        {
            Debug.LogWarning("[HeroSystem] ActionSystem.Instance is null. Cannot discard cards.");
            return;
        }
        
        DiscardAllCardsGA discardAllCardsGA = new();
        ActionSystem.Instance.AddReaction(discardAllCardsGA);
    }

    /// <summary>
    /// Reacts to enemy turn end by applying burn damage, reducing Invulnerable stacks, and drawing new hand
    /// </summary>
    /// <param name="enemyTurnGA">The enemy turn action that triggered this reaction</param>
    /// <remarks>
/// This reaction happens after the enemy turn fully completes.
    /// 
    /// <b>BURN EFFECT INTEGRATION:</b>
    /// For each hero in the party, checks for burn stacks and creates an ApplyBurnGA action
    /// to deal burn damage equal to the number of stacks. Burn damage is processed for all heroes,
    /// not just the first hero, supporting multi-hero parties. Burn stacks reduce by 1 each time, creating a countdown effect.
    /// 
    /// <b>INVULNERABLE EFFECT INTEGRATION:</b>
    /// For each hero in the party, checks for Invulnerable stacks and reduces them by 1 at the end of the enemy turn.
    /// This ensures Invulnerable status expires after a set number of turns, regardless of whether the hero was attacked.
    /// 
    /// <b>TURN CYCLE MANAGEMENT:</b>
    /// After processing status effects, draws 5 cards to give the player a fresh hand for their next turn.
    /// 
    /// <b>How to use status effects:</b>
    /// 1. Cards/perks apply burn or Invulnerable stacks using AddStatusEffectEffect with the appropriate type
    /// 2. Status effects show icons and stack counts to the player
    /// 3. Each enemy turn, this reaction triggers burn damage and Invulnerable stack reduction automatically
    /// 4. BurnSystem and InvulnerableStatusEffectSystem handle the effects and damage blocking
    /// 5. Stacks reduce by 1 each time, creating countdown effects for both statuses
    /// </remarks>
    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        // CRITICAL: Check if this object is still valid before proceeding
        // This prevents MissingReferenceException if HeroSystem was destroyed during scene transition
        if (this == null || !this)
        {
            Debug.LogWarning("[HeroSystem] EnemyTurnPostReaction called but HeroSystem has been destroyed. Skipping reaction.");
            return;
        }
        
        // Check if HeroBoardView is still valid
        if (HeroBoardView == null || !HeroBoardView)
        {
            Debug.LogWarning("[HeroSystem] HeroBoardView is null or destroyed. Cannot process enemy turn post-reaction.");
            return;
        }
        
        // Use StatusEffectTickSystem to process all enemy status effect ticks
        if (StatusEffectTickSystem.Instance != null)
        {
            StatusEffectTickSystem.Instance.TickStatusEffects(HeroBoardView.HeroViews.ConvertAll(e => (CombatantView)e));
        }
        
        // Delay card drawing until after player turn banner animation completes
        // This provides better UX by not drawing cards simultaneously with the banner
        // Only start coroutine if object is still valid
        if (this != null && this)
        {
            StartCoroutine(DelayedCardDraw());
        }
    }
    
    /// <summary>
    /// Delays card drawing until after the player turn banner animation completes
    /// </summary>
    private IEnumerator DelayedCardDraw()
    {
        // Wait for player turn banner animation to complete
        // Banner animation duration: fadeInDuration (0.4s) + holdDuration (1.0s) + slideOutDuration (0.5s) = ~1.9s
        // Add a small buffer for safety
        float bannerAnimationDuration = 2.0f;
        
        // Wait for banner to finish animating
        yield return new WaitForSeconds(bannerAnimationDuration);
        
        // CRITICAL: Check if this object is still valid before proceeding
        // This prevents MissingReferenceException if HeroSystem was destroyed during the wait
        if (this == null || !this)
        {
            Debug.LogWarning("[HeroSystem] DelayedCardDraw: HeroSystem was destroyed during wait. Aborting card draw.");
            yield break;
        }
        
        // Check if ActionSystem is still valid
        if (ActionSystem.Instance == null)
        {
            Debug.LogWarning("[HeroSystem] DelayedCardDraw: ActionSystem.Instance is null. Cannot draw cards.");
            yield break;
        }
        
        // Now draw cards after the banner has finished
        // Use Perform() instead of AddReaction() because we're outside of an active action flow
        // (the EnemyTurnGA flow has already completed by the time this coroutine finishes)
        DrawCardsGA drawCardsGA = new(5);
        ActionSystem.Instance.Perform(drawCardsGA);
    }

   
}
