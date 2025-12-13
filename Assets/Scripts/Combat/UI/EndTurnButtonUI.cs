using UnityEngine;
using AudioSystem;
/* END TURN BUTTON UI DOCUMENTATION
 *
 * Purpose: Button that lets players end their current hero's turn and cycle through the party, then start the enemy turn
 *
 * How it works:
 * - Player clicks the button when they're done with their current hero's turn
 * - Discards the current hero's hand, then advances to the next hero (if any)
 * - Draws a new hand for the next hero, or starts the enemy turn if all heroes have acted
 * - Simple UI button that triggers the party turn cycle and enemy phase
 * - Essential for party-based, turn-based gameplay flow
 *
 * Integration: Works with ActionSystem to trigger turn changes, part of UI system
 */

/// <summary>
/// UI button that allows players to end the current hero's turn, cycle through the party, and start the enemy turn
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Provides a way for players to finish the current hero's turn and manage party turn cycling</para>
///
/// <para><strong>What it does:</strong> This is the "End Turn" button that players click when
/// they're done playing cards and taking actions with the current hero. When clicked, it discards the current hero's hand,
/// advances to the next hero (if any), draws a new hand for them, or starts the enemy turn if all heroes have acted.
/// It's essential for the party-based, turn-based flow of the game.</para>
///
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Player clicks the button (Unity calls OnClick automatically)</item>
/// <item>Discards the current hero's hand</item>
/// <item>If more heroes remain, advances to the next hero and draws their hand</item>
/// <item>If all heroes have acted, starts the enemy turn and resets to the first hero</item>
/// <item>ActionSystem handles the turn transition and enemy actions</item>
/// </list>
///
/// <para><strong>Example:</strong> Player plays some cards with hero 1, clicks "End Turn" → hero 2's turn starts, draws new hand → ... → after last hero, enemies act, then cycle repeats.</para>
///
/// <para><strong>Works with:</strong> ActionSystem for turn processing, Unity UI for button functionality</para>
///
/// <para><strong>How to use:</strong> Attach to a UI Button, set OnClick() as the button's click event</para>
/// </remarks>
public class EndTurnButtonUI : MonoBehaviour
{
    /// <summary>
    /// Called when the player clicks the end turn button
    /// </summary>
    /// <remarks>
    /// Unity calls this automatically when the button is clicked (if set up in Inspector).
    /// Creates an enemy turn action and sends it to the action system to start the 
    /// enemy phase. This is how players pass control to the enemies when they're 
    /// finished with their turn.
    /// </remarks>
    
    [SerializeField] private SoundData mapSelectionMusic;
    [SerializeField] private float MusicFadeTime = 2f;
    
    [Header("Loading Overlay")]
    [Tooltip("Optional loading video ID. If empty, scene will load without loading overlay")]
    [SerializeField] private string loadingVideoId = "loading";
    
    [Header("Turn Management")]
    [Tooltip("If true, players can play multiple cards per turn. If false, automatically advances to next hero after playing one card.")]
    [SerializeField] private bool canPlayMultipleCards = true;
    
    /// <summary>
    /// Static reference to the EndTurnButtonUI instance (for accessing settings)
    /// </summary>
    private static EndTurnButtonUI instance;
    
    /// <summary>
    /// Whether players can play multiple cards per turn
    /// </summary>
    /// <remarks>
    /// If false, after playing any card, automatically discards hand and advances to next hero (or enemy turn).
    /// If true, players can play multiple cards and must manually click "End Turn" button.
    /// </remarks>
    public bool CanPlayMultipleCards => canPlayMultipleCards;
    
    /// <summary>
    /// Gets the EndTurnButtonUI instance from the scene
    /// </summary>
    public static EndTurnButtonUI Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<EndTurnButtonUI>();
            }
            return instance;
        }
    }
    
    private void Awake()
    {
        // Set instance reference if not already set
        if (instance == null)
        {
            instance = this;
        }
    }

    public void OnClick()
    {
        Debug.Log("[EndTurnButtonUI] End Turn button clicked.");
        AdvanceToNextHero();
    }
    
    /// <summary>
    /// Advances to the next hero (or enemy turn if all heroes have acted)
    /// </summary>
    /// <remarks>
    /// Discards current hero's hand, then either advances to next hero or starts enemy turn.
    /// Can be called manually or automatically when canPlayMultipleCards is false.
    /// </remarks>
    public void AdvanceToNextHero()
    {
        int heroCount = CurrentHeroUtil.GetHeroCount();
        int currentHeroIndex = CurrentHeroUtil.CurrentHeroIndex;
        Debug.Log($"[EndTurnButtonUI] Advancing turn. CurrentHeroIndex: {currentHeroIndex} / {heroCount - 1}");

        // Always discard current hero's hand before advancing
        ActionSystem.Instance.Perform(new DiscardAllCardsGA(), () =>
        {
            // If all heroes have acted, start enemy turn and reset to first hero
            if (currentHeroIndex >= heroCount - 1)
            {
                Debug.Log("[EndTurnButtonUI] All heroes finished, starting enemy turn.");
                CurrentHeroUtil.CurrentHeroIndex = 0;
                EnemyTurnGA enemyTurnGA = new();
                ActionSystem.Instance.Perform(enemyTurnGA);
            }
            else
            {
                // Advance to next hero
                CurrentHeroUtil.CurrentHeroIndex++;

                Debug.Log($"[EndTurnButtonUI] Next hero: {CurrentHeroUtil.CurrentHeroIndex}");
                // Draw new hand for the next hero using CardSystem performer
                ActionSystem.Instance.Perform(new DrawCardsGA(5));
            }
        });
    }

    public void goBackToMainMenu()
    {
        var transition = SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Session, SceneDatabase.Scenes.MapSelection, setActive: true)
            .Unload(SceneDatabase.Slots.SessionContent);
        
        // Only add loading video if provided
        if (!string.IsNullOrEmpty(loadingVideoId))
        {
            transition = transition.WithLoadingVideo(loadingVideoId);
        }
        
        // Play main menu music near the end of transition (music fades in as transition completes)
        if (mapSelectionMusic != null)
        {
            transition = transition.WithMusic(mapSelectionMusic, MusicFadeTime);
        }
        
        transition
            .WithPauseMusic(9)
            .Perform();
    }
}
