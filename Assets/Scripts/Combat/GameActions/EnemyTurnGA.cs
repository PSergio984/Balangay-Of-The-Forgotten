using UnityEngine;

/* ENEMY TURN GAME ACTION DOCUMENTATION
 * 
 * Purpose: Represents the start of the enemy turn phase in combat
 * 
 * How it works:
 * - This action signals that it's now the enemies' turn to act
 * - EnemySystem processes this and makes all enemies perform their actions
 * - Triggers enemy AI behavior like attacks and abilities
 * 
 * Integration: Works with EnemySystem and ActionSystem for turn management
 */

/// <summary>
/// Game action that starts the enemy turn phase in combat
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Signals that it's time for enemies to take their turn</para>
/// 
/// <para><strong>What it does:</strong> This action represents the beginning of the 
/// enemy turn phase. When this action happens, all enemies on the board will 
/// perform their actions like attacking the player or using abilities.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Player ends their turn</item>
/// <item>EnemyTurnGA action gets created and sent to ActionSystem</item>
/// <item>EnemySystem receives this action</item>
/// <item>EnemySystem makes all enemies perform their actions</item>
/// <item>Turn switches back to player after all enemies are done</item>
/// </list>
/// 
/// <para><strong>Needs:</strong> EnemySystem must be in the scene to handle enemy turns</para>
/// 
/// <para><strong>Works with:</strong> EnemySystem to control enemy behavior, ActionSystem for turn flow</para>
/// 
/// <para><strong>How to use:</strong> Create this action when the player's turn ends and enemies should act</para>
/// </remarks>
public class EnemyTurnGA : GameAction
{
    /// <summary>
    /// Creates a new enemy turn action
    /// </summary>
    /// <remarks>
    /// Simple constructor that creates an action to start the enemy turn.
    /// No parameters needed since all enemies automatically act during their turn.
    /// </remarks>
    public EnemyTurnGA()
    {
        // No setup needed - this action just signals enemy turn start
    }
}
