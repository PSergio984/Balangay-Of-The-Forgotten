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

    [Header("🎵 Card Audio")]
    [SerializeField] private SoundData cardDiscardSound;

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
    // Setup for multiple heroes: each gets their own deck/hand/discard
    public void Setup(List<HeroData> heroDatas)
    {
        drawPiles.Clear();
        discardPiles.Clear();
        hands.Clear();
        for (int i = 0; i < heroDatas.Count; i++)
        {
            var deck = new List<Card>();
            foreach (var cardData in heroDatas[i].Deck)
            {
                deck.Add(new Card(cardData));
            }
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
        var discardPile = discardPiles[heroIndex];
        hand.Remove(playCardsGA.Card);
        CardView cardView = handView.RemoveCard(playCardsGA.Card);
        yield return DiscardCard(cardView, heroIndex);
        // SpendStaminaGA spendStaminaGA = new (playCardsGA.Card.Stamina);
        // ActionSystem.Instance.AddReaction(spendStaminaGA);

        if (playCardsGA.Card.ManualTargetEffect != null)
        {
            PerformEffectGA performEffectGA = new(playCardsGA.Card.ManualTargetEffect, new() { playCardsGA.ManualTarget });
            ActionSystem.Instance.AddReaction(performEffectGA);
        }

        foreach (var effectWrapper in playCardsGA.Card.OtherEffects)
        {
            List<CombatantView> targets = effectWrapper.targetMode.GetTargets();
            PerformEffectGA performEffectGA = new(effectWrapper.effects,targets);
            ActionSystem.Instance.AddReaction(performEffectGA);
        }
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
        discardPile.Add(cardView.Card);
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
}
