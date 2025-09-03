using UnityEngine;

/* HERO SYSTEM DOCUMENTATION
 * 
 * Purpose: Manages the player's hero character and their stats
 * 
 * How it works:
 * - Holds reference to the hero's visual display
 * - Sets up the hero with their starting stats and appearance
 * - Provides access to hero information for other systems
 * - Manages hero state during combat
 * 
 * Integration: Works with HeroView for display and other systems for hero interactions
 */

/// <summary>
/// System that manages the player's hero character and their information
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Controls the player's hero character and their stats</para>
/// 
/// <para><strong>What it does:</strong> This system manages everything about the player's 
/// hero character. It sets up the hero's starting health, appearance, and other stats. 
/// Other parts of the game can use this system to check hero information or affect 
/// the hero during combat.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Game starts and hero data gets loaded</item>
/// <item>System sets up the hero with their stats and appearance</item>
/// <item>HeroView displays the hero to the player</item>
/// <item>Other systems can access hero info through this system</item>
/// </list>
/// 
/// <para><strong>Needs:</strong> HeroView component for displaying the hero, HeroData for stats</para>
/// 
/// <para><strong>Works with:</strong> HeroView for display, combat systems for health/damage</para>
/// 
/// <para><strong>How to use:</strong> Put this on a GameObject and assign the HeroView in Inspector</para>
/// </remarks>
public class HeroSystem : Singleton<HeroSystem>
{
    /// <summary>
    /// The visual display component that shows the hero to players
    /// </summary>
    /// <remarks>
    /// This property holds the HeroView component that displays the hero character.
    /// Other systems can access this to affect the hero's appearance or get hero info.
    /// Assign a HeroView GameObject in the Inspector.
    /// </remarks>
    [field: SerializeField] public HeroView HeroView { get; private set; }

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

    public void Setup(HeroData heroData)
        {
            // Tell the hero view to set up the hero's appearance and stats
            HeroView.Setup(heroData);
        }
    void OnEnable()
    {
          // Listen for enemy turns to handle cards automatically
    ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPreReaction, ReactionTiming.PRE);
    ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

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
        DiscardAllCardsGA discardAllCardsGA = new();
        ActionSystem.Instance.AddReaction(discardAllCardsGA);
    }

    /// <summary>
    /// Reacts to enemy turn end by drawing a new hand of cards
    /// </summary>
    /// <param name="enemyTurnGA">The enemy turn action that triggered this reaction</param>
    /// <remarks>
    /// This reaction happens after the enemy turn fully completes.
    /// Automatically draws 5 cards to give the player a fresh hand for their next turn.
    /// Part of the turn cycle management to prepare the player for their turn.
    /// </remarks>
    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        int burnStacks = HeroView.GetStatusEffectStacks(StatusEffectType.BURN);
        if (burnStacks > 0)
        {
            ApplyBurnGA applyBurnGA = new(burnStacks, HeroView);
            ActionSystem.Instance.AddReaction(applyBurnGA);
        }
          DrawCardsGA drawCardsGA = new(5);
        ActionSystem.Instance.AddReaction(drawCardsGA);
      
    }

   
}
