using System.Collections;
using UnityEngine;

/// <summary>
/// Manages combat phase transitions and UI display
/// Hooks into the Action System to show banners at appropriate times
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Coordinates combat phase UI with game flow</para>
/// 
/// <para><strong>Responsibilities:</strong></para>
/// <list type="bullet">
/// <item>Shows "Battle Start" banner at combat initialization</item>
/// <item>Shows "Player Turn" banner when player turn begins</item>
/// <item>Shows "Enemy Turn" banner when enemy turn begins</item>
/// <item>Coordinates spawn animations with phase transitions</item>
/// </list>
/// 
/// <para><strong>Integration:</strong> Works with ActionSystem reactions and CombatPhaseUI</para>
/// </remarks>
public class CombatPhaseManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Reference to the CombatPhaseUI component")]
    [SerializeField] private CombatPhaseUI combatPhaseUI;

    [Header("Timing")]
    [Tooltip("Delay before showing Battle Start banner after scene loads")]
    [SerializeField] private float battleStartDelay = 0.5f;
    
    [Tooltip("Delay before showing Player Turn banner")]
    [SerializeField] private float playerTurnDelay = 0.2f;

    [Header("Settings")]
    [Tooltip("If true, shows Battle Start banner on scene start")]
    [SerializeField] private bool showBattleStartOnLoad = true;

    private bool battleStartShown = false;
    private int turnCount = 0;

    // WORKAROUND: Due to a bug in ActionSystem.UnsubscribeReaction() where it creates a new wrapper
    // that doesn't match the original, we cannot reliably unsubscribe. Instead, we manually check
    // if this component is enabled before processing reactions to prevent stale reactions.
    private bool isActiveAndEnabled = false;

    private void OnEnable()
    {
        isActiveAndEnabled = true;
        
        // Subscribe to enemy turn actions to show Enemy Turn banner
        // NOTE: These subscriptions cannot be properly unsubscribed due to ActionSystem bug
        // We use the isActiveAndEnabled flag to prevent stale reactions
        ActionSystem.SubscribeReaction<EnemyTurnGA>(OnEnemyTurnStart, ReactionTiming.PRE);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(OnEnemyTurnEnd, ReactionTiming.POST);
    }

    private void OnDisable()
    {
        isActiveAndEnabled = false;
        
        // NOTE: UnsubscribeReaction doesn't work due to ActionSystem bug (creates new wrapper closure)
        // Leaving these calls here for documentation, but they have no effect
        // The isActiveAndEnabled flag prevents stale reactions from executing
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(OnEnemyTurnStart, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(OnEnemyTurnEnd, ReactionTiming.POST);
    }

    private void Start()
    {
        // Show Battle Start banner after a small delay
        if (showBattleStartOnLoad)
        {
            StartCoroutine(ShowBattleStartSequence());
        }
    }

    /// <summary>
    /// Shows Battle Start banner and initial Player Turn
    /// </summary>
    private IEnumerator ShowBattleStartSequence()
    {
        yield return new WaitForSeconds(battleStartDelay);
        
        if (combatPhaseUI != null)
        {
            combatPhaseUI.ShowBattleStart();
            battleStartShown = true;
            
            // Wait for Battle Start animation to complete before showing Player Turn
            yield return new WaitForSeconds(2.5f);
            
            // Show first Player Turn
            combatPhaseUI.ShowPlayerTurn();
            turnCount = 1;
        }
    }

    /// <summary>
    /// Called when enemy turn begins
    /// </summary>
    private void OnEnemyTurnStart(EnemyTurnGA enemyTurnGA)
    {
        // Guard against stale reactions after component is disabled
        if (!isActiveAndEnabled || combatPhaseUI == null)
            return;
            
        combatPhaseUI.ShowEnemyTurn();
    }

    /// <summary>
    /// Called when enemy turn ends (player turn begins)
    /// </summary>
    private void OnEnemyTurnEnd(EnemyTurnGA enemyTurnGA)
    {
        // Guard against stale reactions after component is disabled
        if (!isActiveAndEnabled || combatPhaseUI == null)
            return;
            
        turnCount++;
        StartCoroutine(ShowPlayerTurnDelayed());
    }

    /// <summary>
    /// Shows Player Turn banner with a small delay for pacing
    /// </summary>
    private IEnumerator ShowPlayerTurnDelayed()
    {
        yield return new WaitForSeconds(playerTurnDelay);
        combatPhaseUI.ShowPlayerTurn();
    }

    /// <summary>
    /// Resets the combat phase manager for a new battle
    /// </summary>
    public void ResetForNewBattle()
    {
        battleStartShown = false;
        turnCount = 0;
        if (combatPhaseUI != null)
        {
            combatPhaseUI.ResetTurnCounter();
        }
    }

    /// <summary>
    /// Gets the current turn number
    /// </summary>
    public int CurrentTurn => turnCount;
}

