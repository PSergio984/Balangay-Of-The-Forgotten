using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// System that gets everything ready when a battle starts
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Sets up all the pieces needed for combat to work</para>
/// 
/// <para><strong>What it does:</strong> This system runs once at the start of every battle 
/// to make sure everything is ready. It creates the hero, spawns enemies, sets up the 
/// card deck, gives player starting perks, and provides the starting hand. Without 
/// this, nothing else would work properly.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Unity calls Start() when the scene begins</item>
/// <item>System tells HeroSystem to create the hero with their data</item>
/// <item>System tells EnemySystem to create all enemies for this fight</item>
/// <item>System tells CardSystem to prepare the hero's deck</item>
/// <item>System gives player starting perks through PerkSystem</item>
/// <item>System draws 5 cards for the player's starting hand</item>
/// </list>
/// 
/// <para><strong>Perk system integration:</strong> Shows how any system can easily give 
/// perks to the player by creating a Perk from PerkData and adding it to PerkSystem.</para>
/// 
/// <para><strong>Works with:</strong> HeroSystem, EnemySystem, CardSystem, PerkSystem, ActionSystem</para>
/// </remarks>
public class MatchSetupSystem : MonoBehaviour
{
   [Header("Fixed Data (Hero & Perks)")]
   
   /// <summary>
   /// Information about the hero character (health, cards, etc.)
   /// </summary>
   /// <remarks>
   /// This contains all the data needed to create the hero for this battle.
   /// Includes things like max health, starting deck, and other stats.
   /// Assign a HeroData asset in the Inspector.
   /// </remarks>
   [SerializeField] private List<HeroData> heroDatas;
   
   /// <summary>
   /// Test perk to give the player at the start of battle
   /// </summary>
   /// <remarks>
   /// Example of how to give perks to the player. Any system can create a Perk 
   /// from PerkData and add it to PerkSystem to give the player new abilities.
   /// This shows the simple pattern for perk integration.
   /// </remarks>
   [SerializeField] private PerkData perkData;
   
   
   [Header("Dynamic Level Data")]
   
   /// <summary>
   /// Reference to level transition data for reading selected level info
   /// </summary>
   /// <remarks>
   /// <para><strong>Why:</strong> Reads which level the player selected from level select screen</para>
   /// <para><strong>How:</strong> Assign the same LevelTransitionData asset used by MapButton</para>
   /// </remarks>
   [Tooltip("Assign the LevelTransitionData asset - reads selected level data from level select")]
   [SerializeField] private LevelTransitionData levelTransitionData;
   
    /// <summary>
    /// Reference to the background Image in the combat scene
    /// </summary>
    /// <remarks>
    /// <para><strong>Why:</strong> Changes based on selected level's background sprite</para>
    /// <para><strong>How:</strong> Assign the background GameObject's Image component</para>
    /// </remarks>
    [Tooltip("The background Image component that displays the level's background image")]
    [SerializeField] private UnityEngine.UI.Image combatBackgroundRenderer;
   
   
   [Header("Fallback Data (For Direct Testing)")]
   
   /// <summary>
   /// Fallback list of enemies when no level is selected (for testing combat scene directly)
   /// </summary>
   /// <remarks>
   /// Each enemy in this list will be created when the battle starts.
   /// Can have multiple enemies for harder fights or just one for simple battles.
   /// Used when LevelTransitionData has no selected map (direct scene testing).
   /// </remarks>
   [Tooltip("Fallback enemies used when testing combat scene directly without going through level select")]
   [SerializeField] private List<EnemyData> fallbackEnemyDatas;
   
   /// </remarks>
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


        // --- Setup variables at top for use throughout method ---
        MapData selectedMapData = null;
        List<EnemyData> enemiesToSpawn = null;

        // Determine selected map and enemy list
        if (levelTransitionData != null && levelTransitionData.HasValidData()) {
            selectedMapData = levelTransitionData.SelectedMapData;
            if (selectedMapData != null && selectedMapData.EnemyDatas != null && selectedMapData.EnemyDatas.Count > 0) {
                Debug.Log($"[MatchSetupSystem] Using enemies from selected level: {selectedMapData.MapId}");
                enemiesToSpawn = new List<EnemyData>(selectedMapData.EnemyDatas);
            }
        }
        if (enemiesToSpawn == null && fallbackEnemyDatas != null && fallbackEnemyDatas.Count > 0) {
            Debug.Log("[MatchSetupSystem] No level selected - using fallback enemy data for testing");
            enemiesToSpawn = fallbackEnemyDatas;
        }

        if (enemiesToSpawn == null) {
            Debug.LogWarning("[MatchSetupSystem] No enemy data available! Check LevelTransitionData or fallback enemies.");
            enemiesToSpawn = new List<EnemyData>();
        }

       
        // Set the combat background from selected level (if available)
        if (combatBackgroundRenderer != null && selectedMapData != null && selectedMapData.CombatBackgroundSprite != null)
        {
            combatBackgroundRenderer.sprite = selectedMapData.CombatBackgroundSprite;
            Debug.Log($"[MatchSetupSystem] Set combat background from level: {selectedMapData.MapId}");
        }

        // Spawn all enemies for this battle
        EnemySystem.Instance.Setup(enemiesToSpawn);


        // Spawn all hero entities first (multi-hero support)
        // This ensures heroes exist before cards are set up
        HeroSystem.Instance.Setup(heroDatas);

        // Prepare the card system with all hero decks (multi-hero support)
        CardSystem.Instance.Setup(heroDatas);

        // Give the player a starting perk - shows how any system can add perks
        if (perkData == null)
        {
            Debug.LogWarning("[MatchSetupSystem] No PerkData assigned. Skipping starting perk.", this);
        }
        else
        {
            PerkSystem.Instance.AddPerk(new Perk(perkData));
        }

        // Create an action to draw 5 cards for the starting hand of the first hero
        DrawCardsGA drawCardsGA = new(5);

        // Execute the draw cards action to give the first hero their starting hand
        ActionSystem.Instance.Perform(drawCardsGA);

    }

}





