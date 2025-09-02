using System.Collections.Generic;
using UnityEngine;

/* MATCH SETUP SYSTEM DOCUMENTATION
 * 
 * Purpose: Gets everything ready at the start of a battle
 * 
 * How it works:
 * - Sets up the hero with their stats and deck
 * - Creates all the enemies for this fight
 * - Prepares the card system with the hero's deck
 * - Draws the starting hand of cards for the player
 * 
 * Integration: First system to run, works with all other combat systems
 */

/// <summary>
/// System that gets everything ready when a battle starts
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Sets up all the pieces needed for combat to work</para>
/// 
/// <para><strong>What it does:</strong> This system runs once at the start of every battle 
/// to make sure everything is ready. It creates the hero, spawns enemies, sets up the 
/// card deck, and gives the player their starting hand. Without this, nothing else 
/// would work properly.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Unity calls Start() when the scene begins</item>
/// <item>System tells HeroSystem to create the hero with their data</item>
/// <item>System tells EnemySystem to create all enemies for this fight</item>
/// <item>System tells CardSystem to prepare the hero's deck</item>
/// <item>System draws 5 cards for the player's starting hand</item>
/// </list>
/// 
/// <para><strong>Needs:</strong> HeroData for hero setup, EnemyData list for enemies</para>
/// 
/// <para><strong>Works with:</strong> HeroSystem, EnemySystem, CardSystem, ActionSystem</para>
/// 
/// <para><strong>How to use:</strong> Put this on a GameObject and assign hero/enemy data in Inspector</para>
/// </remarks>
public class MatchSetupSystem : MonoBehaviour
{
   /// <summary>
   /// Information about the hero character (health, cards, etc.)
   /// </summary>
   /// <remarks>
   /// This contains all the data needed to create the hero for this battle.
   /// Includes things like max health, starting deck, and other stats.
   /// Assign a HeroData asset in the Inspector.
   /// </remarks>
   [SerializeField] private HeroData heroData;
   
   /// <summary>
   /// List of all enemies that will appear in this battle
   /// </summary>
   /// <remarks>
   /// Each enemy in this list will be created when the battle starts.
   /// Can have multiple enemies for harder fights or just one for simple battles.
   /// Assign EnemyData assets in the Inspector.
   /// </remarks>
   [SerializeField] private List<EnemyData> enemyDatas;

    /// <summary>
    /// Sets up everything needed for combat when the battle scene starts
    /// </summary>
    /// <remarks>
    /// Unity calls this automatically when the GameObject becomes active.
    /// This is where all the combat systems get initialized in the right order.
    /// Must happen before any other combat actions can work.
    /// </remarks>
private void Start()
    {
        // Create the hero character using the assigned hero data
        HeroSystem.Instance.Setup(heroData);
        // Create all enemy characters using the assigned enemy data list
        EnemySystem.Instance.Setup(enemyDatas);
        // Prepare the card system with the hero's deck of cards
        CardSystem.Instance.Setup(heroData.Deck);
        // Create an action to draw 5 cards for the player's starting hand
        DrawCardsGA drawCardsGA = new(5);
        // Execute the draw cards action to give the player their starting hand
        ActionSystem.Instance.Perform(drawCardsGA);
    }
}
