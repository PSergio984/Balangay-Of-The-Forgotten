using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    [Header("Character Selection Data")]
    [Tooltip("Reference to the ScriptableObject that stores selected heroes and presets from character selection.")]
    [SerializeField] private CharacterTransitionData characterTransitionData;
   
   /// <summary>
   /// Information about the hero character (health, cards, etc.)
   /// </summary>
   /// <remarks>
   /// This contains all the data needed to create the hero for this battle.
   /// Includes things like max health, starting deck, and other stats.
   /// Assign a HeroData asset in the Inspector.
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
    [SerializeField] private Image combatBackgroundRenderer;

    [Tooltip("The background Image component that displays the level's map image")]
    [SerializeField] private Image mapIcon;
   
   
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

   /// <summary>
   /// Fallback reward data for the first enemy (miniboss) when using fallback enemies
   /// </summary>
   /// <remarks>
   /// Used for testing reward system when no map is selected.
   /// Assign reward data here to test rewards with fallback enemies.
   /// </remarks>
   [Tooltip("Reward data for first enemy (miniboss) when using fallback enemies for testing")]
   [SerializeField] private RewardData fallbackMinibossReward;

   /// <summary>
   /// Fallback reward data for the second enemy (main boss) when using fallback enemies
   /// </summary>
   /// <remarks>
   /// Used for testing reward system when no map is selected.
   /// Assign reward data here to test rewards with fallback enemies.
   /// </remarks>
   [Tooltip("Reward data for second enemy (main boss) when using fallback enemies for testing")]
   [SerializeField] private RewardData fallbackMainBossReward;

   [SerializeField] private Animator roleTurnAnimator;
   
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
        // Try to find CharacterTransitionData if not assigned
        if (characterTransitionData == null)
        {
            Debug.LogWarning("[MatchSetupSystem] CharacterTransitionData not assigned in Inspector. Attempting to find asset automatically...");
            
            // Try to load from Resources first
            characterTransitionData = Resources.Load<CharacterTransitionData>("CharacterTransitionData");
            
            // If not in Resources, try to find it in the project (Editor only)
            if (characterTransitionData == null)
            {
                #if UNITY_EDITOR
                // Use UnityEditor API to find the asset
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:CharacterTransitionData");
                if (guids.Length > 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    characterTransitionData = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterTransitionData>(path);
                    Debug.Log($"[MatchSetupSystem] Found CharacterTransitionData at: {path}");
                }
                #endif
            }
            
            if (characterTransitionData == null)
            {
                Debug.LogError("[MatchSetupSystem] CharacterTransitionData could not be found! Please assign it in the Inspector or create it in Resources/CharacterTransitionData.asset");
            }
            else
            {
                Debug.Log("[MatchSetupSystem] CharacterTransitionData found and assigned automatically.");
            }
        }
        
        // If characterTransitionData has valid data, use it to populate heroDatas
        if (characterTransitionData != null && characterTransitionData.HasValidData())
        {
            Debug.Log("[MatchSetupSystem] Reading character data from CharacterTransitionData...");
            var slots = characterTransitionData.GetCompleteSlots();
            heroDatas = new List<HeroData>();
            foreach (var slot in slots)
            {
                if (slot.Hero != null)
                {
                    // Optionally, you can also store the preset somewhere if needed for later
                    heroDatas.Add(slot.Hero);
                    Debug.Log($"[MatchSetupSystem] Added hero to setup: {slot.Hero.HeroName} (Preset: {slot.SelectedPreset?.PresetName ?? "None"})");
                }
            }
            Debug.Log($"[MatchSetupSystem] Loaded {heroDatas.Count} heroes from CharacterTransitionData");
            // NOTE: We do NOT clear characterTransitionData here so it persists between maps
            // The data will persist across multiple combat scenes, allowing the same heroes to be used
            // Data is only cleared when starting a new game session (in MainMenu.StartSession)
        }
        else if (characterTransitionData != null)
        {
            Debug.LogWarning("[MatchSetupSystem] CharacterTransitionData exists but has no valid data. Using fallback heroDatas from Inspector.");
        }
        else
        {
            Debug.LogWarning("[MatchSetupSystem] CharacterTransitionData is null. Using fallback heroDatas from Inspector.");
        }
        
        // Start the setup sequence as a coroutine to handle async hero spawning
        StartCoroutine(SetupSequence());
    }
    
    /// <summary>
    /// Coroutine that handles the full setup sequence in the correct order
    /// </summary>
    /// <remarks>
    /// Sequence:
    /// 1. Reset combat state from any previous encounter
    /// 2. Setup heroes and wait for spawn animations to complete
    /// 3. Setup cards, perks, etc.
    /// 4. Setup enemies (triggers overlay and battle start sequence)
    /// </remarks>
    private IEnumerator SetupSequence()
    {
        // STEP 0: Reset combat state from any previous encounter
        // This ensures a clean slate for the new fight (fixes bug where attack doesn't work after returning from boss)
        if (ActionSystem.Instance != null)
        {
            ActionSystem.Instance.ResetCombatState();
            Debug.Log("[MatchSetupSystem] ActionSystem combat state reset for new encounter");
        }


        // --- Setup variables at top for use throughout method ---
        MapData selectedMapData = null;
        List<EnemyData> enemiesToSpawn = null;

        // Determine selected map and enemy list
        if (levelTransitionData != null && levelTransitionData.HasValidData()) {
            selectedMapData = levelTransitionData.SelectedMapData;
            if (selectedMapData != null && selectedMapData.EnemyDatas != null && selectedMapData.EnemyDatas.Count > 0) {
                Debug.Log($"[MatchSetupSystem] Using enemies from selected level: {selectedMapData.MapId}");
                enemiesToSpawn = new List<EnemyData>(selectedMapData.EnemyDatas);
            }        }
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

        // Set the map icon from selected level (if available)
        if (mapIcon != null && selectedMapData != null && selectedMapData.CombatMapIcon != null)
        {
            mapIcon.sprite = selectedMapData.CombatMapIcon;
            Debug.Log($"[MatchSetupSystem] Set combat map icon from level: {selectedMapData.MapId}");
        }

        // Defensive: Ensure heroDatas is not null or empty before any use
        if (heroDatas == null || heroDatas.Count == 0)
        {
            Debug.LogWarning("[MatchSetupSystem] heroDatas is null or empty. Cannot setup heroes, cards, or animator override.", this);
            yield break;
        }

        // STEP 1: Spawn all hero entities first (multi-hero support)
        // This ensures heroes exist before cards are set up
        HeroSystem.Instance.Setup(heroDatas);
        
        // Wait for all hero spawn animations to complete
        // Animation timing: Each hero has staggered spawn with delay
        // Last hero spawns at: spawnDelay * (heroCount - 1) + slideInDuration
        // Default values: spawnDelay = 0.2s, slideInDuration = 0.6s
        // For safety, calculate: (0.2 * (count-1)) + 0.6 + buffer
        float heroSpawnDelay = 0.2f; // Default from HeroBoardView
        float heroSlideInDuration = 0.6f; // Default from HeroBoardView
        float totalHeroSpawnTime = (heroSpawnDelay * (heroDatas.Count - 1)) + heroSlideInDuration + 0.2f; // Add buffer
        yield return new WaitForSeconds(totalHeroSpawnTime);
        
        Debug.Log($"[MatchSetupSystem] All heroes spawned. Waiting {totalHeroSpawnTime}s for animations.");

        // STEP 2: Prepare the card system with all hero decks (multi-hero support)
        CardSystem.Instance.Setup(heroDatas);

        if (heroDatas[0] != null)
        {
            SetAnimatorOverride(heroDatas[0].TurnProfileOverride);
        }

        // Give the player a starting perk - shows how any system can add perks
        if (perkData == null)
        {
            Debug.LogWarning("[MatchSetupSystem] No PerkData assigned. Skipping starting perk.", this);
        }
        else
        {
            PerkSystem.Instance.AddPerk(new Perk(perkData));
        }

        // STEP 3: Initialize sequential enemy spawning (only spawns first enemy, rest spawn on defeat)
        // SEQUENTIAL MODE: Enemies appear one at a time. When defeated, the next spawns automatically.
        // This triggers the enemy spawn overlay and then the battle start sequence
        // Pass map data for reward access, or fallback reward data if using fallback enemies
        MapData rewardMapData = selectedMapData;
        if (rewardMapData == null && (fallbackMinibossReward != null || fallbackMainBossReward != null))
        {
            // Create a temporary MapData-like structure for fallback rewards
            // We'll pass the fallback rewards directly to EnemySystem instead
            rewardMapData = null; // Keep as null, we'll handle fallback rewards separately
        }
        EnemySystem.Instance.Setup(enemiesToSpawn, rewardMapData, fallbackMinibossReward, fallbackMainBossReward);

        // STEP 4: Delay card drawing until after Battle Start and Player Turn banners complete
        // Sequence: Enemy Overlay -> Battle Start -> Player Turn -> Draw Cards (not simultaneously)
        StartCoroutine(DelayedInitialCardDraw());
    }
    
    /// <summary>
    /// Delays initial card drawing until after the player turn banner animation completes
    /// </summary>
    private IEnumerator DelayedInitialCardDraw()
    {
        // Calculate actual banner sequence timing from CombatPhaseManager.ShowBattleStartSequence:
        // - Battle Start delay: 0.5s (battleStartDelay)
        // - Wait after Battle Start call: 2.5s (this wait starts when Battle Start begins animating)
        // - Player Turn animation: fadeIn (0.4s) + hold (1.0s) + slideOut (0.5s) = 1.9s
        // Total: 0.5 + 2.5 + 1.9 = 4.9s
        // Add small buffer for safety: 5.0s
        float totalBannerDuration = 5.0f;
        
        yield return new WaitForSeconds(totalBannerDuration);
        
        // Now draw cards after both banners have finished
        DrawCardsGA drawCardsGA = new(5);
        ActionSystem.Instance.Perform(drawCardsGA);
    }

    /// <summary>
    /// Assigns a new AnimatorOverrideController instance to the Animator at runtime.
    /// </summary>
    /// <param name="overrideController">The override controller to assign (from HeroData)</param>
    public void SetAnimatorOverride(AnimatorOverrideController overrideController)
    {
        if (roleTurnAnimator != null && overrideController != null)
        {
            roleTurnAnimator.runtimeAnimatorController = overrideController;
            Debug.Log($"[MatchSetupSystem] AnimatorOverrideController set at runtime: {overrideController.name} (Base: {overrideController.runtimeAnimatorController?.name})", roleTurnAnimator);
            // Force rebind to ensure Animator uses the new override controller
            roleTurnAnimator.Rebind();
        }
        else
        {
            if (roleTurnAnimator == null)
            {
                Debug.LogWarning($"[MatchSetupSystem] Cannot assign override: Animator reference is null on {gameObject.name}", this);
            }
            if (overrideController == null)
            {
                Debug.LogWarning($"[MatchSetupSystem] Cannot assign override: OverrideController is null on {gameObject.name}", this);
            }
        }
    }
}





