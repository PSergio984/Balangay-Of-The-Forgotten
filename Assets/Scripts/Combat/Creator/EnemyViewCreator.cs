using UnityEngine;

/* ENEMY VIEW CREATOR DOCUMENTATION
 * 
 * Purpose: Factory that creates visual representations of enemy characters
 * 
 * How it works:
 * - Creates new enemy views from a prefab template
 * - Positions enemies at specified battle locations
 * - Sets up enemies with their stats, appearance, and behavior
 * - Provides consistent enemy creation across the game
 * 
 * Integration: Used by EnemySystem and EnemyBoardView to spawn enemies in combat
 */

/// <summary>
/// Singleton factory responsible for creating enemy view instances
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Factory that creates the visual enemy characters players fight against</para>
/// 
/// <para><strong>What it does:</strong> This is the factory that creates all enemy visuals 
/// when they appear in combat. When a battle starts or new enemies spawn, this system 
/// creates them from a prefab template and sets them up with their stats, appearance, 
/// and health. It ensures all enemies are created consistently and appear in the right 
/// positions on the battlefield.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>EnemySystem or EnemyBoardView requests a new enemy to be created</item>
/// <item>This system instantiates an enemy prefab at the specified position</item>
/// <item>Sets up the enemy with data from EnemyData (health, attack, image, name)</item>
/// <item>Returns the finished enemy view ready for combat</item>
/// <item>Enemy appears on battlefield and can participate in combat</item>
/// </list>
/// 
/// <para><strong>Features:</strong></para>
/// <list type="bullet">
/// <item>Singleton pattern for easy access from anywhere</item>
/// <item>Prefab-based creation for consistency</item>
/// <item>Automatic setup with enemy data</item>
/// <item>Proper positioning and rotation</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> EnemySystem for enemy management, EnemyBoardView for positioning, EnemyData for stats</para>
/// 
/// <para><strong>How to use:</strong> Assign enemy prefab in Inspector, other systems call CreateEnemyView()</para>
/// </remarks>
public class EnemyViewCreator : Singleton<EnemyViewCreator>
{
   /// <summary>
   /// Prefab reference for the enemy view that will be instantiated
   /// </summary>
   /// <remarks>
   /// This is the template used to create all enemy visuals in the game.
   /// Should be a prefab with EnemyView component and all necessary UI elements.
   /// Assign this in the Inspector.
   /// </remarks>
   [SerializeField] private EnemyView enemyViewPrefab;

    /// <summary>
    /// Creates a new enemy view with specified data, position, and rotation
    /// </summary>
    /// <param name="enemyData">Data containing enemy stats, appearance, and behavior</param>
    /// <param name="position">Where to position the enemy on the battlefield</param>
    /// <param name="rotation">What rotation to give the enemy</param>
    /// <returns>The created and set up enemy view ready for combat</returns>
    /// <remarks>
    /// Creates a new enemy from the prefab template and sets it up with all the data 
    /// from the EnemyData asset. This includes health, attack power, visual appearance, 
    /// and name. The enemy is positioned correctly and ready to participate in combat.
    /// </remarks>
    public EnemyView CreateEnemyView(EnemyData enemyData, Vector3 position, Quaternion rotation)
    {
        // Instantiate a new enemy view from the prefab at the specified position and rotation
        EnemyView enemyView = Instantiate(enemyViewPrefab, position, rotation);
        // Set up the enemy with its data (health, attack, image, name, etc.)
        enemyView.Setup(enemyData);
        // Return the created and set up enemy view
        return enemyView;
    }
}
