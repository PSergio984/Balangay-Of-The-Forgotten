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

    private int turnCount = 0;
    
    // Static instance for easy access
    private static CombatPhaseManager instance;
    public static CombatPhaseManager Instance => instance;

    // WORKAROUND: Due to a bug in ActionSystem.UnsubscribeReaction() where it creates a new wrapper
    // that doesn't match the original, we cannot reliably unsubscribe. Instead, we manually check
    // if this component is enabled before processing reactions to prevent stale reactions.
    // We also track whether we've already subscribed to prevent duplicate callbacks on re-enable.
    private bool isComponentActive = false;
    private bool hasSubscribedToReactions = false;

    private void Awake()
    {
        instance = this;
    }
    
    private void OnEnable()
    {
        isComponentActive = true;
        
        // Subscribe to enemy turn actions to show Enemy Turn banner
        // NOTE: These subscriptions cannot be properly unsubscribed due to ActionSystem bug
        // We use isComponentActive flag to prevent stale reactions, and hasSubscribedToReactions
        // to prevent duplicate subscriptions on re-enable.
        if (!hasSubscribedToReactions)
        {
            ActionSystem.SubscribeReaction<EnemyTurnGA>(OnEnemyTurnStart, ReactionTiming.PRE);
            ActionSystem.SubscribeReaction<EnemyTurnGA>(OnEnemyTurnEnd, ReactionTiming.POST);
            hasSubscribedToReactions = true;
        }
    }

    private void OnDisable()
    {
        isComponentActive = false;
        
        // NOTE: UnsubscribeReaction doesn't work due to ActionSystem bug (creates new wrapper closure)
        // Leaving these calls here for documentation, but they have no effect
        // The isComponentActive flag prevents stale reactions from executing
        // We do NOT reset hasSubscribedToReactions here since UnsubscribeReaction is ineffective
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(OnEnemyTurnStart, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(OnEnemyTurnEnd, ReactionTiming.POST);
    }

    private void Start()
    {
        // Show Battle Start banner after enemy spawn overlay completes (if first enemy)
        if (showBattleStartOnLoad)
        {
            StartCoroutine(ShowBattleStartSequence());
        }
    }

    /// <summary>
    /// Shows Battle Start banner and initial Player Turn
    /// Waits for first enemy spawn overlay to complete before showing Battle Start
    /// </summary>
    private IEnumerator ShowBattleStartSequence()
    {
        // Wait for first enemy spawn overlay to complete (if it's showing)
        // The overlay shows before the first enemy spawns (miniboss)
        if (EnemySpawnOverlayUI.Instance != null)
        {
            // First, wait for the overlay to start animating (in case EnemySystem hasn't started it yet)
            // Poll until overlay begins animating
            int waitCount = 0;
            while (!EnemySpawnOverlayUI.Instance.IsAnimating && waitCount < 100) // Max 10 seconds wait
            {
                yield return new WaitForSeconds(0.1f);
                waitCount++;
            }
            
            // Now wait for overlay animation to complete
            // Poll IsAnimating property until overlay finishes
            while (EnemySpawnOverlayUI.Instance.IsAnimating)
            {
                yield return new WaitForSeconds(0.1f); // Check every 0.1 seconds
            }
            
            // Add a small buffer after overlay completes
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            // Fallback: if overlay system doesn't exist, use original delay
            yield return new WaitForSeconds(battleStartDelay);
        }
        
        if (combatPhaseUI != null)
        {
            combatPhaseUI.ShowBattleStart();
            
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
        if (!isComponentActive || combatPhaseUI == null)
            return;
            
        combatPhaseUI.ShowEnemyTurn();
    }

    /// <summary>
    /// Called when enemy turn ends (player turn begins)
    /// </summary>
    private void OnEnemyTurnEnd(EnemyTurnGA enemyTurnGA)
    {
        // Guard against stale reactions after component is disabled
        if (!isComponentActive || combatPhaseUI == null)
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
        
        // Re-check component activity and object validity after delay
        if (!isComponentActive || combatPhaseUI == null)
            yield break;
            
        combatPhaseUI.ShowPlayerTurn();
    }

    /// <summary>
    /// Resets the combat phase manager for a new battle
    /// </summary>
    public void ResetForNewBattle()
    {
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
    
    /// <summary>
    /// Sets the turn count (used when resetting for new enemy encounter)
    /// </summary>
    public void SetTurnCount(int count)
    {
        turnCount = count;
    }
}

