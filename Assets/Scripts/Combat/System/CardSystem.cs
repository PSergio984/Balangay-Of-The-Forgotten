using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using AudioSystem;

/* CARD SYSTEM DESIGN
 *
 * How it works:
 * - Each hero has their own deck, hand, and discard pile (no cross-hero card access)
 * - Handles drawing cards from the correct hero's deck to their hand with animations
 * - Manages card playing including manual targeting and regular effects
 * - Processes card-related game actions and reactions
 * - Controls card lifecycle from deck through hand to discard pile, all per-hero
 * - Routes manual target effects separately from auto-target effects
 *
 * Design reasoning:
 * - Ensures strict per-hero deck/hand/discard logic for party-based gameplay
 * - Separates manual targeting from auto-targeting for cleaner effect processing
 * - Manual target effects get the player-selected target directly
 * - Other effects still use their own target modes for flexibility
 * - Same card playing flow handles both targeting types seamlessly
 *
 * Integration: Works with ActionSystem, HandView, EffectSystem, ManualTargetingSystem, and other card components
 */

/// <summary>
/// Core system that manages all card functionality including per-hero deck, hand, and card actions
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Controls all card-related gameplay mechanics and state for each hero independently</para>
///
/// <para><strong>What it does:</strong> This system handles everything related to cards for each hero -
/// drawing from the correct hero's deck, managing their hand, playing cards, discarding, and shuffling.
/// It also handles card animations and integrates with the action system to process card-related actions.</para>
///
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Sets up a separate deck, hand, and discard pile for each hero at game start</item>
/// <item>Draws cards from the correct hero's deck to their hand when requested</item>
/// <item>Handles card playing with effects and stamina costs, always using the correct hero's hand</item>
/// <item>Manages discarding and deck refilling per-hero (no cross-hero card movement)</item>
/// <item>Responds to enemy turns by discarding/drawing cards for the correct hero</item>
/// </list>
///
/// <para><strong>Needs:</strong> HandView for card display, ActionSystem for processing, card prefabs</para>
///
/// <para><strong>Works with:</strong> ActionSystem, HandView, EffectSystem, StaminaSystem</para>
///
/// <para><strong>How to use:</strong> Set up references in Inspector, system handles card management automatically</para>
/// </remarks>
public class CardSystem : Singleton<CardSystem>
{
    [SerializeField] private HandView handView;
    [SerializeField] private Transform drawPilePoint;
    [SerializeField] private Transform discardPilePoint;

    // Each hero has their own deck, hand, and discard pile
    private List<List<Card>> drawPiles = new();
    private List<List<Card>> discardPiles = new();
    private List<List<Card>> hands = new();

    /// <summary>
    /// Stores original position of current hero at the start of their turn (for returning after turn ends)
    /// </summary>
    private Dictionary<int, Vector3> heroTurnStartPositions = new Dictionary<int, Vector3>();

    [Header("🎵 Card Audio")]
    [SerializeField] private SoundData cardDiscardSound;

    [Header("Hero Movement Settings")]
    [Tooltip("How far heroes move forward (right) when their turn starts")]
    [SerializeField] private float heroTurnMoveDistance = 0.3f;
    [Tooltip("How long it takes for heroes to move forward at turn start")]
    [SerializeField] private float heroTurnMoveDuration = 0.2f;
    [Tooltip("How far heroes move forward (right) when attacking with a card")]
    [SerializeField] private float heroAttackMoveDistance = 0.5f;
    [Tooltip("How long it takes for heroes to move forward during attack")]
    [SerializeField] private float heroAttackMoveForwardDuration = 0.15f;
    [Tooltip("How long it takes for heroes to move back after attack")]
    [SerializeField] private float heroAttackMoveBackDuration = 0.2f;

    // Only for UI/display, not for logic
    private int activeHeroIndex = 0;
    public int ActiveHeroIndex => activeHeroIndex;
    public int HeroCount => drawPiles.Count;

    private SoundBuilder soundBuilder;

    [SerializeField] private Animator roleTurnAnimator;

    private void Start()
    {
        // Cache the sound builder for playing sounds and performance
        if (SoundManager.Instance != null)
        {
            soundBuilder = SoundManager.Instance.CreateSoundBuilder();
        }
        else
        {
            Debug.LogWarning("[CardSystem] SoundManager not available. Card sounds will be disabled.");
        }
    }

    // Set active hero for UI, but do NOT use for logic
    public void SetActiveHero(int heroIndex)
    {
        if (heroIndex >= 0 && heroIndex < drawPiles.Count)
        {
            activeHeroIndex = heroIndex;
            // Optionally update UI/handView here
        }
    }

    /// <summary>
    /// Sets up event handlers when this system becomes active
    /// </summary>
    /// <remarks>
    /// Unity calls this automatically when the GameObject goes from inactive to active.
    /// Registers this system to handle card-related actions and enemy turn reactions.
    /// </remarks>
    void OnEnable()
    {
        // Register performers for card-related actions
        ActionSystem.AttachPerformer<DrawCardsGA>(DrawCardsPerformer);
        ActionSystem.AttachPerformer<DiscardAllCardsGA>(DiscardAllCardsPerformer);
        ActionSystem.AttachPerformer<PlayCardsGA>(PlayCardPerformer);
    
    }

    /// <summary>
    /// Cleans up event handlers when this system becomes inactive
    /// </summary>
    /// <remarks>
    /// Unity calls this automatically when the GameObject goes from active to inactive.
    /// Unregisters all handlers to prevent memory leaks and system errors.
    /// </remarks>
    void OnDisable()
    {
        ActionSystem.DetachPerformer<DrawCardsGA>();
        ActionSystem.DetachPerformer<DiscardAllCardsGA>();
        ActionSystem.DetachPerformer<PlayCardsGA>();
    
    }
    // Performers

    /// <summary>
    /// Initializes the card deck from a list of card data assets
    /// </summary>
    /// <param name="deckData">List of CardData assets that define the deck contents</param>
    /// <remarks>
    /// This method sets up the initial deck by creating Card instances from CardData assets.
    /// Each CardData gets converted into a playable Card and added to the draw pile.
    /// Call this once at game start to prepare the deck for play.
    /// </remarks>
    [Header("Special Card Persistence")]
    [SerializeField] private SpecialCardCollectionData specialCardCollection;

    // Setup for multiple heroes: each gets their own deck/hand/discard
    public void Setup(List<HeroData> heroDatas)
    {
        drawPiles.Clear();
        discardPiles.Clear();
        hands.Clear();

        if (specialCardCollection == null)
        {
            specialCardCollection = Resources.Load<SpecialCardCollectionData>("Special Card Collection");
        }
        if (specialCardCollection != null)
        {
            specialCardCollection.Load();
        }

        for (int i = 0; i < heroDatas.Count; i++)
        {
            var deck = new List<Card>();
            foreach (var cardData in heroDatas[i].Deck)
            {
                // Skip null card data entries (defensive check for missing asset references)
                if (cardData == null)
                {
                    Debug.LogWarning($"[CardSystem] Hero '{heroDatas[i].HeroName}' has a null CardData entry in their deck. Skipping this card.");
                    continue;
                }
                deck.Add(new Card(cardData));
            }

            // Special cards are NOT added to the deck. They are granted as extra
            // hand slots at draw time (see DrawCardsPerformer) so they never enter
            // the draw/discard piles and can never be recycled back into the deck.

            drawPiles.Add(deck);
            discardPiles.Add(new List<Card>());
            hands.Add(new List<Card>());
        }
        activeHeroIndex = 0;
    }

    /// <summary>
    /// Processes drawing the specified number of cards from deck to hand
    /// </summary>
    /// <param name="drawCardsGA">Action containing the number of cards to draw</param>
    /// <returns>IEnumerator for coroutine execution with card drawing animations</returns>
    /// <remarks>
    /// This method handles drawing cards with deck management. If there aren't enough
    /// cards in the draw pile, it refills the deck from discards and continues drawing.
    /// Each card draw includes animation as cards move from deck to hand.
    /// </remarks>
    private IEnumerator DrawCardsPerformer(DrawCardsGA drawCardsGA)
    {
        int heroIndex = CurrentHeroUtil.CurrentHeroIndex;
        Debug.Log($"[CardSystem] Drawing cards for hero index: {heroIndex}");

        // Move current hero slightly to the right to show they're preparing to act
        HeroView currentHero = CurrentHeroUtil.GetCurrentHero();
        if (currentHero != null)
        {
            // Store original position before moving
            heroTurnStartPositions[heroIndex] = currentHero.transform.position;
            // Move right (positive X direction)
            currentHero.transform.DOMoveX(currentHero.transform.position.x + heroTurnMoveDistance, heroTurnMoveDuration);
            // Wait for movement to complete
            yield return new WaitForSeconds(heroTurnMoveDuration);
        }

        // Update the turn profile animator to match the current hero
        if (HeroSystem.Instance == null)
        {
            Debug.LogWarning("HeroSystem.Instance is null. Skipping GetHeroTurnProfileOverride.");
        }
        else
        {
            var turnProfileOverride = HeroSystem.Instance.GetHeroTurnProfileOverride(heroIndex);
            if (turnProfileOverride != null)
            {
                SetAnimatorOverride(turnProfileOverride);
            }
        }

        var drawPile = drawPiles[heroIndex];
        var discardPile = discardPiles[heroIndex];
        var hand = hands[heroIndex];
        int totalToDraw = drawCardsGA.Amount;
        for (int i = 0; i < totalToDraw; i++)
        {
            // If deck is empty, try to refill from discard
            if (drawPile.Count == 0 && discardPile.Count > 0)
            {
                RefillDeck(heroIndex);
            }
            yield return DrawCard(heroIndex);
        }

        // Grant the current hero's assigned special cards as extra, guaranteed hand slots.
        // Specials are one-shot: they persist here until played (consumed from the collection),
        // never enter the draw/discard piles, and are re-added on the next turn draw if unplayed.
        if (specialCardCollection != null && currentHero != null && currentHero.HeroData != null)
        {
            string heroName = currentHero.HeroData.HeroName;
            var assignedSpecials = specialCardCollection.GetSpecialCardsForHero(heroName);
            foreach (var assignedSpecial in assignedSpecials)
            {
                if (assignedSpecial == null || !specialCardCollection.HasCard(assignedSpecial.CardId)) continue;
                CardData playableData = assignedSpecial.GetOrCreatePlayableCardData();
                if (playableData == null) continue;
                if (hand.Exists(c => c.Data == playableData)) continue; // already present in hand

                var specialCard = new Card(playableData);
                hand.Add(specialCard);
                CardView cardView = CardViewCreator.Instance.CreateCardView(specialCard, drawPilePoint.position, drawPilePoint.rotation);
                yield return handView.AddCard(cardView);
                Debug.Log($"[CardSystem] Granted special card '{assignedSpecial.CardName}' as an extra hand slot for hero '{heroName}'.");
            }
        }
    }

    /// <summary>
    /// Processes discarding all cards currently in the player's hand
    /// </summary>
    /// <param name="discardAllCardsGA">Action that triggers discarding all hand cards</param>
    /// <returns>IEnumerator for coroutine execution with discard animations</returns>
    /// <remarks>
    /// This method removes all cards from the hand and moves them to the discard pile.
    /// Each card gets animated as it moves from hand to discard pile.
    /// Typically called at the end of the player's turn or when enemy turn starts.
    /// </remarks>
    private IEnumerator DiscardAllCardsPerformer(DiscardAllCardsGA discardAllCardsGA)
    {
        int heroIndex = CurrentHeroUtil.CurrentHeroIndex;
        
        // Return current hero to their original position (they moved right at turn start)
        if (heroTurnStartPositions.ContainsKey(heroIndex))
        {
            HeroView currentHero = CurrentHeroUtil.GetCurrentHero();
            if (currentHero != null)
            {
                float returnDuration = 0.2f;
                // Return to the exact original position stored at turn start
                currentHero.transform.DOMove(heroTurnStartPositions[heroIndex], returnDuration);
                // Remove from dictionary after returning
                heroTurnStartPositions.Remove(heroIndex);
            }
        }
        
        var hand = hands[heroIndex];
        // Copy to avoid modifying collection during iteration
        var handCopy = new List<Card>(hand);

        foreach (var card in handCopy)
        {
            CardView cardView = handView.RemoveCard(card);
            yield return DiscardCard(cardView, heroIndex);
        }

        hand.Clear();
    }
    
    /// <summary>
    /// Discards all cards for all heroes (used when enemy dies to prevent card hovering)
    /// </summary>
    /// <returns>Coroutine that completes when all cards are discarded</returns>
    public IEnumerator DiscardAllCardsForAllHeroes()
    {
        int heroCount = CurrentHeroUtil.GetHeroCount();
        
        // Discard cards for each hero
        for (int heroIndex = 0; heroIndex < heroCount; heroIndex++)
        {
            if (heroIndex >= hands.Count) continue;
            
            var hand = hands[heroIndex];
            // Copy to avoid modifying collection during iteration
            var handCopy = new List<Card>(hand);
            
            foreach (var card in handCopy)
            {
                CardView cardView = handView.RemoveCard(card);
                if (cardView != null)
                {
                    yield return DiscardCard(cardView, heroIndex);
                }
            }
            
            hand.Clear();
        }
        
        Debug.Log($"[CardSystem] Discarded all cards for all {heroCount} heroes");
    }

    /// <summary>
    /// Handles the complete process of playing a card including effects and targeting
    /// </summary>
    /// <param name="playCardsGA">The action containing the card to be played</param>
    /// <returns>IEnumerator for coroutine execution</returns>
    /// <remarks>
    /// This method processes the full card playing sequence: removes card from hand,
    /// spends stamina, and executes effects with proper targeting. Manual target effects
    /// use the player-selected target, while other effects use their own target modes.
    /// </remarks>
    
    private IEnumerator PlayCardPerformer(PlayCardsGA playCardsGA)
    {
        int heroIndex = CurrentHeroUtil.CurrentHeroIndex;
        var hand = hands[heroIndex];
        var currentHero = CurrentHeroUtil.GetCurrentHero();
        
        // Check if card has conditional effects that will fail BEFORE removing from hand
        // If condition fails, card stays in hand and play is cancelled
        bool conditionFailed = CheckIfConditionalEffectsFail(playCardsGA.Card, playCardsGA.ManualTarget, currentHero);
        
        if (conditionFailed)
        {
            Debug.Log($"[CardSystem] Card '{playCardsGA.Card.Title}' condition not met - card remains in hand");
            
            // Card stays in hand - ensure it's properly positioned in hand view
            // If it was dragged, this will animate it back to its hand position
            yield return handView.UpdateCardPositions(0.3f);
            
            yield break; // Don't play the card, don't remove from hand, don't discard
        }
        
        // Condition passed - proceed with normal play
        // Remove card from hand and get the card view
        hand.Remove(playCardsGA.Card);
        CardView cardView = handView.RemoveCard(playCardsGA.Card);

        // Check if this played card was an assigned special card, and consume it from persistent storage if so
        if (specialCardCollection == null)
        {
            Debug.LogWarning("[CardSystem] Played a special card but SpecialCardCollectionData is null - cannot consume. Check inspector wiring or Resources path.");
        }
        else if (currentHero == null || currentHero.HeroData == null)
        {
            Debug.LogWarning($"[CardSystem] Played special card '{playCardsGA.Card?.Title}' but currentHero{(currentHero == null ? " is null" : ".HeroData is null")} - cannot resolve hero name for consume.");
        }
        else
        {
            string heroName = currentHero.HeroData.HeroName;
            var assignedSpecials = specialCardCollection.GetSpecialCardsForHero(heroName);
            string playedTitle = playCardsGA.Card?.Title;
            CardData playedData = playCardsGA.Card?.Data;
            Debug.Log($"[CardSystem] SpecialConsumeCheck: hero='{heroName}' playedTitle='{playedTitle}' playedData='{playedData?.name}' assignedSpecials={assignedSpecials.Count} collection={specialCardCollection.name}");

            bool consumedAny = false;
            foreach (var assignedSpecial in assignedSpecials)
            {
                if (assignedSpecial == null) continue;

                // Defensive match: (1) stable title match, (2) CardData reference equality
                // against the representation asset, (3) CardData reference equality against
                // the playable card data (runtime-synthesized).
                bool matchByTitle = !string.IsNullOrEmpty(playedTitle) && playedTitle == assignedSpecial.CardName;
                bool matchByRepresentation = playedData != null && assignedSpecial.CardDataRepresentation != null && playedData == assignedSpecial.CardDataRepresentation;
                bool matchByPlayable = playedData != null && playedData == assignedSpecial.GetOrCreatePlayableCardData();

                bool isMatchingSpecial = matchByTitle || matchByRepresentation || matchByPlayable;

                if (isMatchingSpecial)
                {
                    bool consumed = specialCardCollection.ConsumeSpecialCardForHero(heroName, assignedSpecial);
                    consumedAny |= consumed;
                    Debug.Log($"[CardSystem] Consumed special card '{assignedSpecial.CardName}' from hero '{heroName}' upon playing. (ConsumeSpecialCardForHero returned {consumed})");
                    break;
                }
            }
            Debug.Log($"[CardSystem] SpecialConsumeCheck done. consumedAny={consumedAny}. Remaining assignments for hero '{heroName}': {specialCardCollection.GetSpecialCardsForHero(heroName).Count}");
        }
        
        // Play card sound effect if available
        if (playCardsGA.Card.SoundData != null && soundBuilder != null && currentHero != null)
        {
            soundBuilder.WithPosition(currentHero.transform.position).Play(playCardsGA.Card.SoundData);
        }
        
        // Normal flow: discard the card
        yield return DiscardCard(cardView, heroIndex);
        // SpendStaminaGA spendStaminaGA = new (playCardsGA.Card.Stamina);
        // ActionSystem.Instance.AddReaction(spendStaminaGA);

        // Determine animation type based on card effects (attack for damage, cast for magic/buffs)
        CombatantAnimState animState = DetermineCardAnimationType(playCardsGA.Card);
        
        // Move hero forward (right) when attacking with a card, then return
        Vector3 originalHeroPos = currentHero != null ? currentHero.transform.position : Vector3.zero;
        bool movedHero = false;
        if (currentHero != null && animState == CombatantAnimState.Attack)
        {
            // Store original position for return
            if (!heroTurnStartPositions.ContainsKey(heroIndex))
            {
                heroTurnStartPositions[heroIndex] = originalHeroPos;
            }
            
            // Move hero forward (right/positive X direction) when attacking
            movedHero = true;
            Tween attackMoveTween = currentHero.transform.DOMoveX(originalHeroPos.x + heroAttackMoveDistance, heroAttackMoveForwardDuration);
            yield return attackMoveTween.WaitForCompletion();
        }
        
        // Play the hero animation and wait for it to complete
        if (currentHero != null)
        {
            yield return currentHero.PlayAnimationAndWait(animState, returnToIdle: true);
        }
        
        // Return hero to original position after attack
        if (movedHero && currentHero != null && heroTurnStartPositions.ContainsKey(heroIndex))
        {
            Vector3 returnPos = heroTurnStartPositions[heroIndex];
            Tween returnTween = currentHero.transform.DOMoveX(returnPos.x, heroAttackMoveBackDuration);
            yield return returnTween.WaitForCompletion();
            heroTurnStartPositions.Remove(heroIndex); // Clean up
        }

        // Start hit tracking for this move sequence (allows subsequent effects to check if damage hit)
        if (currentHero != null)
        {
            HitTargetTracker.StartMoveSequence(currentHero);
        }

        if (playCardsGA.Card.ManualTargetEffect != null)
        {
            PerformEffectGA performEffectGA = new(playCardsGA.Card.ManualTargetEffect, new() { playCardsGA.ManualTarget }, currentHero);
            ActionSystem.Instance.AddReaction(performEffectGA);
        }

        foreach (var effectWrapper in playCardsGA.Card.OtherEffects)
        {
            List<CombatantView> targets = effectWrapper.targetMode.GetTargets();
            PerformEffectGA performEffectGA = new(effectWrapper.effects, targets, currentHero);
            ActionSystem.Instance.AddReaction(performEffectGA);
        }
        
        // If canPlayMultipleCards is false, automatically advance to next hero after playing a card
        var endTurnButton = EndTurnButtonUI.Instance;
        if (endTurnButton != null && !endTurnButton.CanPlayMultipleCards)
        {
            // Start a coroutine to wait for card effects to complete, then auto-advance
            StartCoroutine(AutoAdvanceAfterCardPlay());
        }
    }
    
    /// <summary>
    /// Coroutine that waits for card effects to complete, then automatically advances to next hero
    /// </summary>
    /// <remarks>
    /// Used when canPlayMultipleCards is false to automatically discard and advance after playing a card.
    /// Waits for the ActionSystem to finish processing all card effects before advancing.
    /// </remarks>
    private IEnumerator AutoAdvanceAfterCardPlay()
    {
        Debug.Log("[CardSystem] Auto-advance coroutine started (canPlayMultipleCards = false)");
        
        // Wait a frame to ensure all reactions are queued
        yield return null;
        
        // Wait for ActionSystem to finish processing all reactions from the card
        Debug.Log("[CardSystem] Waiting for ActionSystem to finish processing card effects...");
        yield return new WaitUntil(() => !ActionSystem.Instance.isPerforming);
        
        // Small additional wait to ensure animations/effects are fully settled
        yield return new WaitForSeconds(0.2f);
        
        // CRITICAL: Check if victory/defeat is showing before auto-advancing
        // If combat has ended, don't advance turn or draw cards
        if (VictoryDefeatUI.Instance != null && VictoryDefeatUI.Instance.IsShowingResult)
        {
            Debug.Log("[CardSystem] Victory/Defeat banner is showing. Aborting auto-advance to prevent drawing cards during victory/defeat.");
            yield break;
        }
        
        var endTurnButton = EndTurnButtonUI.Instance;
        if (endTurnButton != null)
        {
            Debug.Log("[CardSystem] Card effects complete. Auto-advancing turn - discarding current hero's hand and drawing next hero's hand");
            endTurnButton.AdvanceToNextHero();
        }
        else
        {
            Debug.LogWarning("[CardSystem] EndTurnButtonUI.Instance is null! Cannot auto-advance turn.");
        }
    }
    
    /// <summary>
    /// Checks if any conditional effects on the card will fail
    /// </summary>
    /// <param name="card">The card to check</param>
    /// <param name="manualTarget">The manually selected target (if any)</param>
    /// <param name="caster">The caster of the card</param>
    /// <returns>True if any conditional effect fails, false if all pass or no conditional effects</returns>
    private bool CheckIfConditionalEffectsFail(Card card, CombatantView manualTarget, CombatantView caster)
    {
        // Check manual target effect if it's a conditional effect
        if (card.ManualTargetEffect is ConditionalEffect conditionalManual)
        {
            List<CombatantView> targets = manualTarget != null ? new List<CombatantView> { manualTarget } : new List<CombatantView>();
            if (!conditionalManual.IsConditionMet(targets, caster))
            {
                Debug.Log($"[CardSystem] Manual target conditional effect failed for card '{card.Title}'");
                return true; // Condition failed
            }
        }
        
        // Check other effects for conditional effects
        // Note: We need to get targets using the target mode, same as when playing the card
        if (card.OtherEffects != null)
        {
            foreach (var effectWrapper in card.OtherEffects)
            {
                if (effectWrapper.effects is ConditionalEffect conditionalOther)
                {
                    // Get targets using the same target mode that would be used when playing
                    List<CombatantView> targets = effectWrapper.targetMode != null ? effectWrapper.targetMode.GetTargets() : new List<CombatantView>();
                    if (!conditionalOther.IsConditionMet(targets, caster))
                    {
                        Debug.Log($"[CardSystem] Other effect conditional effect failed for card '{card.Title}'");
                        return true; // Condition failed
                    }
                }
            }
        }
        
        return false; // All conditions passed or no conditional effects
    }
    
    /// <summary>
    /// Returns a card to the deck instead of discarding it
    /// Used when conditional effects fail - card goes back to deck to be drawn again
    /// </summary>
    /// <param name="cardView">The visual card to return to deck</param>
    /// <param name="heroIndex">The hero whose deck to return the card to</param>
    /// <returns>IEnumerator for coroutine execution during return animation</returns>
    private IEnumerator ReturnCardToDeck(CardView cardView, int heroIndex)
    {
        var drawPile = drawPiles[heroIndex];
        
        // Add card back to the bottom of the draw pile (or top - your choice)
        // Adding to the end means it will be drawn later
        drawPile.Add(cardView.Card);
        
        // Animate card moving back to draw pile position
        cardView.transform.DOScale(Vector3.zero, 0.15f);
        Tween tween = cardView.transform.DOMove(drawPilePoint.position, 0.15f);
        yield return tween.WaitForCompletion();
        
        // Clean up the card view
        if (cardView != null && cardView.gameObject != null)
        {
            cardView.transform.DOKill();
            DOTween.Kill(cardView);
            Destroy(cardView.gameObject);
        }
    }
    
    /// <summary>
    /// Determines what animation type a card should trigger based on its effects
    /// </summary>
    /// <param name="card">The card being played</param>
    /// <returns>The appropriate animation state for the card type</returns>
    private CombatantAnimState DetermineCardAnimationType(Card card)
    {
        // Check if the card has damage effects (indicates attack)
        if (card.ManualTargetEffect != null)
        {
            // If targeting enemies with damage, use attack animation
            if (card.ManualTargetEffect is DealDamageEffect)
            {
                return CombatantAnimState.Attack;
            }
        }
        
        // Check other effects for damage
        foreach (var effectWrapper in card.OtherEffects)
        {
            if (effectWrapper.effects is DealDamageEffect)
            {
                return CombatantAnimState.Attack;
            }
        }
        
        // Default to cast animation for non-damage cards (buffs, heals, etc.)
        return CombatantAnimState.Cast;
    }

    
   

  /// <summary>
  /// Draws a single card from the deck and adds it to the hand with animation
  /// </summary>
  /// <returns>IEnumerator for coroutine execution during card draw animation</returns>
  /// <remarks>
  /// This method handles the complete process of drawing one card: removes it from
  /// the draw pile, adds it to the hand, creates the visual card, and animates it
  /// moving from the draw pile position to the hand layout.
  /// </remarks>
    private IEnumerator DrawCard(int heroIndex)
    {
        var drawPile = drawPiles[heroIndex];
        var hand = hands[heroIndex];
        Card card = null;

        if (drawPile.Count > 0)
        {
            card = drawPile[0];
            drawPile.RemoveAt(0);
        }
        if (card == null)
        {
            Debug.LogWarning($"[CardSystem] Tried to draw a card for hero {heroIndex}, but deck and discard are empty.");
            yield break;
        }

        hand.Add(card);
        CardView cardView = CardViewCreator.Instance.CreateCardView(card, drawPilePoint.position, drawPilePoint.rotation);
        yield return handView.AddCard(cardView);
    }

    /// <summary>
    /// Refills the draw pile by moving all discarded cards back to it
    /// </summary>
    /// <remarks>
    /// This method is called when the draw pile is empty but more cards need to be drawn.
    /// Moves all cards from the discard pile back to the draw pile and clears the discard pile.
    /// Represents shuffling the discards back into the deck for continued play.
    /// </remarks>
    private void RefillDeck(int heroIndex)
    {
        var drawPile = drawPiles[heroIndex];
        var discardPile = discardPiles[heroIndex];
        drawPile.AddRange(discardPile);
        discardPile.Clear();
    }

    /// <summary>
    /// Discards a single card with animation and destroys its visual representation
    /// </summary>
    /// <param name="cardView">The visual card to be discarded</param>
    /// <returns>IEnumerator for coroutine execution during discard animation</returns>
    /// <remarks>
    /// This method handles the complete discard process: adds the card to discard pile,
    /// animates it scaling down and moving to discard position, then destroys the GameObject.
    /// Used when cards are played or when hand is cleared.
    /// </remarks>
    // Overload to specify heroIndex
    private IEnumerator DiscardCard(CardView cardView, int heroIndex)
    {
        var discardPile = discardPiles[heroIndex];
        // Special cards never enter the discard pile, so RefillDeck can never recycle them.
        // An unplayed special is simply dropped (and re-granted from the collection on the next draw).
        bool isSpecial = cardView != null && cardView.Card != null &&
                         specialCardCollection != null && specialCardCollection.IsSpecialCardData(cardView.Card.Data);
        if (!isSpecial)
        {
            discardPile.Add(cardView.Card);
        }
        cardView.transform.DOScale(Vector3.zero, 0.15f);
        Tween tween = cardView.transform.DOMove(discardPilePoint.position, 0.15f);
        yield return tween.WaitForCompletion();
        if (cardView != null && cardView.gameObject != null)
        {
            soundBuilder
            .Play(cardDiscardSound);
            
            cardView.transform.DOKill();
            DOTween.Kill(cardView);
            Destroy(cardView.gameObject);
        }
    }
    /// <summary>
    /// Assigns a new AnimatorOverrideController instance to the Animator at runtime.
    /// </summary>
    /// <param name="overrideController">The override controller to assign (from HeroData)</param>
    public void SetAnimatorOverride(AnimatorOverrideController overrideController)
    {
        if (roleTurnAnimator != null && overrideController != null)
        {
            roleTurnAnimator.runtimeAnimatorController = overrideController;
            Debug.Log($"[CardSystem] AnimatorOverrideController set at runtime: {overrideController.name} (Base: {overrideController.runtimeAnimatorController?.name})", roleTurnAnimator);
            // Force rebind to ensure Animator uses the new override controller
            roleTurnAnimator.Rebind();
        }
        else
        {
            if (roleTurnAnimator == null)
            {
                Debug.LogWarning($"[CardSystem] Cannot assign override: Animator reference is null on {gameObject.name}", this);
            }
            if (overrideController == null)
            {
                Debug.LogWarning($"[CardSystem] Cannot assign override: OverrideController is null on {gameObject.name}", this);
            }
        }
    }
    
    /// <summary>
    /// Gets all cards for a specific hero or all heroes for cooldown processing
    /// </summary>
    /// <param name="heroIndex">The hero index, or -1 for all heroes</param>
    /// <returns>List of all cards across draw pile, hand, and discard pile</returns>
    /// <remarks>
    /// Used by CooldownSystem to reduce cooldowns on all cards regardless of which pile they're in.
    /// Cooldown persists on the Card object itself, not based on where it is located.
    /// </remarks>
    public List<Card> GetAllCardsForHero(int heroIndex)
    {
        List<Card> allCards = new List<Card>();
        
        if (heroIndex == -1)
        {
            // Get cards for all heroes
            for (int i = 0; i < drawPiles.Count; i++)
            {
                allCards.AddRange(drawPiles[i]);
                allCards.AddRange(hands[i]);
                allCards.AddRange(discardPiles[i]);
            }
        }
        else if (heroIndex >= 0 && heroIndex < drawPiles.Count)
        {
            // Get cards for specific hero
            allCards.AddRange(drawPiles[heroIndex]);
            allCards.AddRange(hands[heroIndex]);
            allCards.AddRange(discardPiles[heroIndex]);
        }
        else
        {
            Debug.LogWarning($"[CardSystem] Invalid heroIndex: {heroIndex}");
        }
        
        return allCards;
    }
    
    /// <summary>
    /// Checks if a card is currently in the hand for the specified hero
    /// </summary>
    /// <param name="card">The card to check</param>
    /// <param name="heroIndex">The hero index to check, or -1 for current hero</param>
    /// <returns>True if the card is in hand, false otherwise</returns>
    public bool IsCardInHand(Card card, int heroIndex = -1)
    {
        if (heroIndex == -1)
        {
            heroIndex = CurrentHeroUtil.CurrentHeroIndex;
        }
        
        if (heroIndex < 0 || heroIndex >= hands.Count)
        {
            return false;
        }
        
        return hands[heroIndex].Contains(card);
    }
}
