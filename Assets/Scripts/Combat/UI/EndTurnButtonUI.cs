using UnityEngine;

/* END TURN BUTTON UI DOCUMENTATION
 * 
 * Purpose: Button that lets players end their turn and start the enemy turn
 * 
 * How it works:
 * - Player clicks the button when they're done with their turn
 * - Creates an EnemyTurnGA action to start enemy phase
 * - Simple UI button that triggers the turn transition
 * - Essential for turn-based gameplay flow
 * 
 * Integration: Works with ActionSystem to trigger turn changes, part of UI system
 */

/// <summary>
/// UI button that allows players to end their turn and start the enemy turn
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Provides a way for players to finish their turn when ready</para>
/// 
/// <para><strong>What it does:</strong> This is the "End Turn" button that players click when 
/// they're done playing cards and taking actions. When clicked, it creates an enemy turn 
/// action and sends it to the action system, which starts the enemy phase of combat. 
/// It's essential for the turn-based flow of the game.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Player clicks the button (Unity calls OnClick automatically)</item>
/// <item>Creates a new EnemyTurnGA action</item>
/// <item>Sends the action to ActionSystem for processing</item>
/// <item>ActionSystem handles the turn transition and enemy actions</item>
/// <item>Turn control passes from player to enemies</item>
/// </list>
/// 
/// <para><strong>Example:</strong> Player plays some cards, then clicks "End Turn" → 
/// enemies take their actions → turn returns to player with full stamina.</para>
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
    public void OnClick()
    {
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
}
