using UnityEngine;

/* INCREASE STATS GA DOCUMENTATION
 * 
 * Purpose: Action that boosts a character's attack and defense stats
 * 
 * How it works:
 * - Contains amounts to increase attack and defense by
 * - Will target a specific minion/character when implemented
 * - Used for buff effects, temporary stat boosts, and enhancement abilities
 * - Currently in development (missing target and GameAction inheritance)
 * 
 * Integration: Will work with combat systems for stat modifications when complete
 * 
 * NOTE: This class currently inherits from MonoBehaviour instead of GameAction,
 * and is missing the target minion. Needs to be updated to inherit from GameAction
 * and include target specification when minion system is implemented.
 */

/// <summary>
/// Game action that increases a character's attack and defense stats
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Action for buffing characters with stat increases</para>
/// 
/// <para><strong>What it does:</strong> This action will increase a character's attack 
/// and defense stats by specified amounts. It's used for buff spells, equipment effects, 
/// or temporary enhancements that make characters stronger in combat. When fully 
/// implemented, it will target specific minions or characters.</para>
/// 
/// <para><strong>How it works (when complete):</strong></para>
/// <list type="bullet">
/// <item>Something creates this action with stat increase amounts and target</item>
/// <item>ActionSystem processes the stat increase action</item>
/// <item>Target character's attack increases by AttackIncreaseAmount</item>
/// <item>Target character's defense increases by DefenseIncreaseAmount</item>
/// <item>Character becomes stronger for the rest of combat</item>
/// </list>
/// 
/// <para><strong>Examples:</strong></para>
/// <list type="bullet">
/// <item>Buff spell: "Give a minion +2 attack and +1 defense"</item>
/// <item>Equipment: "Equipped character gains +3 attack and +2 defense"</item>
/// <item>Ability: "All friendly minions get +1/+1"</item>
/// <item>Temporary boost: "Until end of turn, gain +4 attack"</item>
/// </list>
/// 
/// <para><strong>Development Notes:</strong></para>
/// <list type="bullet">
/// <item>Currently inherits from MonoBehaviour - should inherit from GameAction</item>
/// <item>Missing target minion field - needs to be added when minion system is ready</item>
/// <item>Constructor works but class needs structural updates</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> Will work with minion system and stat management when implemented</para>
/// 
/// <para><strong>How to use:</strong> Create with attack and defense increase amounts, add target when available</para>
/// </remarks>
public class IncreaseStatsGA : MonoBehaviour  // TODO: Should inherit from GameAction instead
{
    // TODO: Uncomment and implement when minion system is ready
    // public Minion TargetMinion;
    
    /// <summary>
    /// How much to increase the target's attack power
    /// </summary>
    /// <remarks>
    /// This amount will be added to the target character's attack stat.
    /// Positive values increase attack, making them deal more damage.
    /// </remarks>
    public int AttackIncreaseAmount;
    
    /// <summary>
    /// How much to increase the target's defense/health
    /// </summary>
    /// <remarks>
    /// This amount will be added to the target character's defense stat.
    /// Positive values increase defense, making them harder to kill.
    /// </remarks>
    public int DefenseIncreaseAmount;
    
    /// <summary>
    /// Creates a new stat increase action with specified amounts
    /// </summary>
    /// <param name="attackIncreaseAmount">How much to boost attack by</param>
    /// <param name="defenseIncreaseAmount">How much to boost defense by</param>
    /// <remarks>
    /// Constructor for creating stat boost actions. Currently missing target specification
    /// which will be added when the minion system is implemented.
    /// Comment says: "balik nalang yung minion target minion pag iiimplement na"
    /// (bring back the minion target when implementing)
    /// </remarks>
    //balik nalang yung minion target minion pag iiimplement na
    public IncreaseStatsGA(int attackIncreaseAmount, int defenseIncreaseAmount)
    {
        // Store how much to increase attack by
        AttackIncreaseAmount = attackIncreaseAmount;
        // Store how much to increase defense by
        DefenseIncreaseAmount = defenseIncreaseAmount;
    }
}
