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

    public void OnClick()
    {
        Debug.Log("[EndTurnButtonUI] End Turn button clicked.");
        int heroCount = CurrentHeroUtil.GetHeroCount();
        int currentHeroIndex = CurrentHeroUtil.CurrentHeroIndex;
        Debug.Log($"[EndTurnButtonUI] CurrentHeroIndex: {currentHeroIndex} / {heroCount - 1}");

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
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.MainMenu, SceneDatabase.Scenes.MainMenu, setActive: true)
            .Unload(SceneDatabase.Slots.SessionContent)
            .WithOverlay()
            .WithMusic(mapSelectionMusic, MusicFadeTime)
            .WithClearUnusedAssets()
            .Perform();
    }
}
