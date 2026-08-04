using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Editor tools for testing combat systems during play mode
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Provides quick access to test functions via Unity's Tools menu</para>
/// <para><strong>Usage:</strong> Tools → Combat Test → Kill Current Enemy (only works in Play Mode)</para>
/// </remarks>
public class CombatTestTools
{
    [MenuItem("Tools/Combat Test/Clear All Save Data", false, 0)]
    public static void ClearAllSaveData()
    {
        // 1. Clear Game Progress Data (PlayerPrefs)
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[CombatTestTools] Cleared all PlayerPrefs data.");

        // 2. Clear Leaderboard Data (JSON file)
        string leaderboardPath = System.IO.Path.Combine(Application.persistentDataPath, "leaderboard.json");
        if (System.IO.File.Exists(leaderboardPath))
        {
            System.IO.File.Delete(leaderboardPath);
            Debug.Log($"[CombatTestTools] Deleted leaderboard file at: {leaderboardPath}");
        }
        
        EditorUtility.DisplayDialog(
            "Save Data Cleared", 
            "Successfully cleared all Game Progress (PlayerPrefs) and Leaderboard data.", 
            "OK"
        );
    }
    /// <summary>
    /// TEST FUNCTION: Kills the current enemy for testing reward system
    /// Only available in Play Mode
    /// </summary>
    [MenuItem("Tools/Combat Test/Kill Current Enemy", false, 1)]
    public static void KillCurrentEnemy()
    {
        // Check if we're in play mode
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "Not in Play Mode",
                "This function only works during Play Mode.\n\nPlease enter Play Mode first.",
                "OK"
            );
            return;
        }

        // Check if EnemySystem exists
        if (EnemySystem.Instance == null)
        {
            EditorUtility.DisplayDialog(
                "EnemySystem Not Found",
                "EnemySystem.Instance is null!\n\nMake sure you're in a combat scene with EnemySystem set up.",
                "OK"
            );
            Debug.LogWarning("[CombatTestTools] EnemySystem.Instance is null! Cannot kill enemy.");
            return;
        }

        // Call the test function
        EnemySystem.Instance.KillCurrentEnemy();
        Debug.Log("[CombatTestTools] Called KillCurrentEnemy() - Check console for results.");
    }

    /// <summary>
    /// Validates if the menu item should be enabled
    /// </summary>
    [MenuItem("Tools/Combat Test/Kill Current Enemy", true)]
    public static bool ValidateKillCurrentEnemy()
    {
        // Only enable in play mode
        return Application.isPlaying;
    }

    /// <summary>
    /// TEST FUNCTION: Kills all heroes and triggers defeat banner
    /// Only available in Play Mode
    /// </summary>
    [MenuItem("Tools/Combat Test/Trigger Defeat (Kill All Heroes)", false, 2)]
    public static void TriggerDefeat()
    {
        // Check if we're in play mode
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "Not in Play Mode",
                "This function only works during Play Mode.\n\nPlease enter Play Mode first.",
                "OK"
            );
            return;
        }

        // Check if HeroSystem exists
        if (HeroSystem.Instance == null)
        {
            EditorUtility.DisplayDialog(
                "HeroSystem Not Found",
                "HeroSystem.Instance is null!\n\nMake sure you're in a combat scene with HeroSystem set up.",
                "OK"
            );
            Debug.LogWarning("[CombatTestTools] HeroSystem.Instance is null! Cannot trigger defeat.");
            return;
        }

        // Get all heroes
        var heroes = HeroSystem.Instance.HeroViews;
        if (heroes == null || heroes.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "No Heroes Found",
                "No heroes are available in the scene!\n\nMake sure heroes are spawned.",
                "OK"
            );
            Debug.LogWarning("[CombatTestTools] No heroes found! Cannot trigger defeat.");
            return;
        }

        Debug.Log($"[CombatTestTools] TEST: Triggering defeat by killing {heroes.Count} hero(es)...");

        // Deal lethal damage to all heroes
        // Use a very high damage value to ensure they die
        float lethalDamage = 99999f;
        
        // Convert HeroViews to CombatantViews for DealDamageGA
        var combatantTargets = new System.Collections.Generic.List<CombatantView>();
        foreach (var hero in heroes)
        {
            if (hero != null)
            {
                combatantTargets.Add(hero);
            }
        }

        if (combatantTargets.Count == 0)
        {
            Debug.LogWarning("[CombatTestTools] No valid hero targets found!");
            return;
        }

        // Create damage action to kill all heroes
        // Use null caster since this is a test function
        DealDamageGA killAllHeroesGA = new DealDamageGA(lethalDamage, combatantTargets, null);
        
        // Perform the damage action through ActionSystem
        // This will trigger death animations and automatically check for defeat
        ActionSystem.Instance.Perform(killAllHeroesGA);
        
        Debug.Log("[CombatTestTools] Called DealDamageGA on all heroes - Defeat should trigger automatically when all heroes die.");
    }

    /// <summary>
    /// Validates if the defeat menu item should be enabled
    /// </summary>
    [MenuItem("Tools/Combat Test/Trigger Defeat (Kill All Heroes)", true)]
    public static bool ValidateTriggerDefeat()
    {
        // Only enable in play mode
        return Application.isPlaying;
    }

    // ========== STATUS EFFECT TEST FUNCTIONS ==========

    /// <summary>
    /// Helper method to get current hero and show error if not available
    /// </summary>
    private static HeroView GetCurrentHeroForTest()
    {
        if (HeroSystem.Instance == null)
        {
            EditorUtility.DisplayDialog(
                "HeroSystem Not Found",
                "HeroSystem.Instance is null!\n\nMake sure you're in a combat scene with HeroSystem set up.",
                "OK"
            );
            Debug.LogWarning("[CombatTestTools] HeroSystem.Instance is null!");
            return null;
        }

        var currentHero = CurrentHeroUtil.GetCurrentHero();
        if (currentHero == null)
        {
            EditorUtility.DisplayDialog(
                "No Current Hero",
                "Could not get current hero!\n\nMake sure a hero turn is active.",
                "OK"
            );
            Debug.LogWarning("[CombatTestTools] CurrentHeroUtil.GetCurrentHero() returned null!");
            return null;
        }

        return currentHero;
    }

    /// <summary>
    /// TEST FUNCTION: Apply Bonecracked (DEFENSE_DOWN 10% for 1 turn) to current hero
    /// </summary>
    [MenuItem("Tools/Combat Test/Apply Status Effects/Apply Bonecracked", false, 10)]
    public static void ApplyBonecracked()
    {
        if (!Application.isPlaying) { EditorUtility.DisplayDialog("Not in Play Mode", "This function only works during Play Mode.", "OK"); return; }

        var hero = GetCurrentHeroForTest();
        if (hero == null) return;

        var targets = new List<CombatantView> { hero };
        var action = new ApplyDefenseDownGA(targets, 10, 1, "Bonecracked");
        ActionSystem.Instance.Perform(action);
        Debug.Log($"[CombatTestTools] Applied Bonecracked to {hero.name}");
    }

    [MenuItem("Tools/Combat Test/Apply Status Effects/Apply Bonecracked", true)]
    public static bool ValidateApplyBonecracked() => Application.isPlaying;

    /// <summary>
    /// TEST FUNCTION: Apply Rage (+50% DMG, +20% def ignore, +20% hit for 3 turns) to current hero
    /// </summary>
    [MenuItem("Tools/Combat Test/Apply Status Effects/Apply Rage", false, 11)]
    public static void ApplyRage()
    {
        if (!Application.isPlaying) { EditorUtility.DisplayDialog("Not in Play Mode", "This function only works during Play Mode.", "OK"); return; }

        var hero = GetCurrentHeroForTest();
        if (hero == null) return;

        RageStatusEffectSystem.ApplyRage(hero, 3);
        Debug.Log($"[CombatTestTools] Applied Rage to {hero.name}");
    }

    [MenuItem("Tools/Combat Test/Apply Status Effects/Apply Rage", true)]
    public static bool ValidateApplyRage() => Application.isPlaying;

    /// <summary>
    /// TEST FUNCTION: Apply On Guard (+40% defense for 3 turns) to current hero
    /// </summary>
    [MenuItem("Tools/Combat Test/Apply Status Effects/Apply On Guard", false, 12)]
    public static void ApplyOnGuard()
    {
        if (!Application.isPlaying) { EditorUtility.DisplayDialog("Not in Play Mode", "This function only works during Play Mode.", "OK"); return; }

        var hero = GetCurrentHeroForTest();
        if (hero == null) return;

        var targets = new List<CombatantView> { hero };
        var action = new ApplyDefenseUpGA(targets, 40, 3, "On Guard");
        ActionSystem.Instance.Perform(action);
        Debug.Log($"[CombatTestTools] Applied On Guard to {hero.name}");
    }

    [MenuItem("Tools/Combat Test/Apply Status Effects/Apply On Guard", true)]
    public static bool ValidateApplyOnGuard() => Application.isPlaying;

    /// <summary>
    /// TEST FUNCTION: Apply Blessing (+20% dmg buff for 2 turns) to current hero
    /// </summary>
    [MenuItem("Tools/Combat Test/Apply Status Effects/Apply Blessing", false, 13)]
    public static void ApplyBlessing()
    {
        if (!Application.isPlaying) { EditorUtility.DisplayDialog("Not in Play Mode", "This function only works during Play Mode.", "OK"); return; }

        var hero = GetCurrentHeroForTest();
        if (hero == null) return;

        // For DMG_UP, stacks represent percentage. Use AddStatusEffectGA with custom name via direct call
        var targets = new List<CombatantView> { hero };
        hero.AddStatusEffect(StatusEffectType.DMG_UP, 20, "Blessing");
        Debug.Log($"[CombatTestTools] Applied Blessing to {hero.name}");
    }

    [MenuItem("Tools/Combat Test/Apply Status Effects/Apply Blessing", true)]
    public static bool ValidateApplyBlessing() => Application.isPlaying;

    /// <summary>
    /// TEST FUNCTION: Apply Focused Aim (+30% hit, +20% def ignore for 2 turns) to current hero
    /// </summary>
    [MenuItem("Tools/Combat Test/Apply Status Effects/Apply Focused Aim", false, 14)]
    public static void ApplyFocusedAim()
    {
        if (!Application.isPlaying) { EditorUtility.DisplayDialog("Not in Play Mode", "This function only works during Play Mode.", "OK"); return; }

        var hero = GetCurrentHeroForTest();
        if (hero == null) return;

        var targets = new List<CombatantView> { hero };
        var action = new AddStatusEffectGA(StatusEffectType.FOCUSED, 2, targets);
        ActionSystem.Instance.Perform(action);
        Debug.Log($"[CombatTestTools] Applied Focused Aim to {hero.name}");
    }

    [MenuItem("Tools/Combat Test/Apply Status Effects/Apply Focused Aim", true)]
    public static bool ValidateApplyFocusedAim() => Application.isPlaying;

    /// <summary>
    /// TEST FUNCTION: Apply Moonfall (DEFENSE_DOWN 20% for 2 turns) to current hero
    /// </summary>
    [MenuItem("Tools/Combat Test/Apply Status Effects/Apply Moonfall", false, 15)]
    public static void ApplyMoonfall()
    {
        if (!Application.isPlaying) { EditorUtility.DisplayDialog("Not in Play Mode", "This function only works during Play Mode.", "OK"); return; }

        var hero = GetCurrentHeroForTest();
        if (hero == null) return;

        var targets = new List<CombatantView> { hero };
        var action = new ApplyDefenseDownGA(targets, 20, 2, "Moonfall");
        ActionSystem.Instance.Perform(action);
        Debug.Log($"[CombatTestTools] Applied Moonfall to {hero.name}");
    }

    [MenuItem("Tools/Combat Test/Apply Status Effects/Apply Moonfall", true)]
    public static bool ValidateApplyMoonfall() => Application.isPlaying;

    /// <summary>
    /// TEST FUNCTION: Apply Invulnerable (100% damage reduction) to current hero
    /// </summary>
    [MenuItem("Tools/Combat Test/Apply Status Effects/Apply Invulnerable", false, 16)]
    public static void ApplyInvulnerable()
    {
        if (!Application.isPlaying) { EditorUtility.DisplayDialog("Not in Play Mode", "This function only works during Play Mode.", "OK"); return; }

        var hero = GetCurrentHeroForTest();
        if (hero == null) return;

        // Invulnerable uses stacks to represent duration (typically 1-2 turns for testing)
        var targets = new List<CombatantView> { hero };
        var action = new AddStatusEffectGA(StatusEffectType.INVULNERABLE, 3, targets);
        ActionSystem.Instance.Perform(action);
        Debug.Log($"[CombatTestTools] Applied Invulnerable (3 stacks) to {hero.name}");
    }

    [MenuItem("Tools/Combat Test/Apply Status Effects/Apply Invulnerable", true)]
    public static bool ValidateApplyInvulnerable() => Application.isPlaying;

    /// <summary>
    /// TEST FUNCTION: Apply Stun (cannot attack) to current hero
    /// </summary>
    [MenuItem("Tools/Combat Test/Apply Status Effects/Apply Stun", false, 17)]
    public static void ApplyStun()
    {
        if (!Application.isPlaying) { EditorUtility.DisplayDialog("Not in Play Mode", "This function only works during Play Mode.", "OK"); return; }

        var hero = GetCurrentHeroForTest();
        if (hero == null) return;

        // Stun uses stacks to represent duration
        var targets = new List<CombatantView> { hero };
        var action = new AddStatusEffectGA(StatusEffectType.STUN, 2, targets);
        ActionSystem.Instance.Perform(action);
        Debug.Log($"[CombatTestTools] Applied Stun (2 stacks) to {hero.name}");
    }

    [MenuItem("Tools/Combat Test/Apply Status Effects/Apply Stun", true)]
    public static bool ValidateApplyStun() => Application.isPlaying;

    /// <summary>
    /// TEST FUNCTION: Apply Devoured (60HP fixed damage per turn for 2 turns) to current hero
    /// </summary>
    [MenuItem("Tools/Combat Test/Apply Status Effects/Apply Devoured", false, 18)]
    public static void ApplyDevoured()
    {
        if (!Application.isPlaying) { EditorUtility.DisplayDialog("Not in Play Mode", "This function only works during Play Mode.", "OK"); return; }

        var hero = GetCurrentHeroForTest();
        if (hero == null) return;

        var targets = new List<CombatantView> { hero };
        // Fixed damage 60HP, 0% MAG, 2 turns duration, null caster for test
        var action = new ApplyDevouredGA(targets, 0, 60, 2, null);
        ActionSystem.Instance.Perform(action);
        Debug.Log($"[CombatTestTools] Applied Devoured (60HP per turn for 2 turns) to {hero.name}");
    }

    [MenuItem("Tools/Combat Test/Apply Status Effects/Apply Devoured", true)]
    public static bool ValidateApplyDevoured() => Application.isPlaying;

    /// <summary>
    /// TEST FUNCTION: Apply Bind (DEFENSE_DOWN 15% for 2 turns) to current hero
    /// </summary>
    [MenuItem("Tools/Combat Test/Apply Status Effects/Apply Bind", false, 19)]
    public static void ApplyBind()
    {
        if (!Application.isPlaying) { EditorUtility.DisplayDialog("Not in Play Mode", "This function only works during Play Mode.", "OK"); return; }

        var hero = GetCurrentHeroForTest();
        if (hero == null) return;

        var targets = new List<CombatantView> { hero };
        var action = new ApplyDefenseDownGA(targets, 15, 2, "Bind");
        ActionSystem.Instance.Perform(action);
        Debug.Log($"[CombatTestTools] Applied Bind to {hero.name}");
    }

    [MenuItem("Tools/Combat Test/Apply Status Effects/Apply Bind", true)]
    public static bool ValidateApplyBind() => Application.isPlaying;

    /// <summary>
    /// TEST FUNCTION: Apply Taunt (enemies target you for 2 turns) to current hero
    /// </summary>
    [MenuItem("Tools/Combat Test/Apply Status Effects/Apply Taunt", false, 20)]
    public static void ApplyTaunt()
    {
        if (!Application.isPlaying) { EditorUtility.DisplayDialog("Not in Play Mode", "This function only works during Play Mode.", "OK"); return; }

        var hero = GetCurrentHeroForTest();
        if (hero == null) return;

        // Taunt uses stacks to represent duration
        var targets = new List<CombatantView> { hero };
        var action = new AddStatusEffectGA(StatusEffectType.TAUNT, 2, targets);
        ActionSystem.Instance.Perform(action);
        Debug.Log($"[CombatTestTools] Applied Taunt (2 stacks) to {hero.name}");
    }

    [MenuItem("Tools/Combat Test/Apply Status Effects/Apply Taunt", true)]
    public static bool ValidateApplyTaunt() => Application.isPlaying;

    // ========== DAMAGE TEST FUNCTIONS ==========

    /// <summary>
    /// TEST FUNCTION: Deal 1000 damage to the current enemy for testing
    /// </summary>
    [MenuItem("Tools/Combat Test/Deal Damage/Deal 1000 Damage to Enemy", false, 1)]
    public static void Deal1000DamageToEnemy()
    {
        // Check if we're in play mode
        if (!Application.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "Not in Play Mode",
                "This function only works during Play Mode.\n\nPlease enter Play Mode first.",
                "OK"
            );
            return;
        }

        // Check if EnemySystem exists
        if (EnemySystem.Instance == null)
        {
            EditorUtility.DisplayDialog(
                "EnemySystem Not Found",
                "EnemySystem.Instance is null!\n\nMake sure you're in a combat scene with EnemySystem set up.",
                "OK"
            );
            Debug.LogWarning("[CombatTestTools] EnemySystem.Instance is null! Cannot deal damage to enemy.");
            return;
        }

        // Get enemies directly from EnemySystem (which accesses EnemyBoardView)
        var enemyViews = EnemySystem.Instance.EnemyViews;
        if (enemyViews == null || enemyViews.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "No Enemy Found",
                "No enemies are available in the scene!\n\nMake sure an enemy is spawned.",
                "OK"
            );
            Debug.LogWarning("[CombatTestTools] No enemies found! Cannot deal damage.");
            return;
        }

        // Get the first active enemy
        var enemy = enemyViews[0];
        if (enemy == null)
        {
            EditorUtility.DisplayDialog(
                "Enemy is Null",
                "The enemy reference is null!\n\nMake sure the enemy is properly spawned.",
                "OK"
            );
            Debug.LogWarning("[CombatTestTools] Enemy is null! Cannot deal damage.");
            return;
        }

        // Create damage action to deal 1000 damage
        var targets = new List<CombatantView> { enemy };
        var damageAction = new DealDamageGA(1000f, targets, null);
        
        // Perform the damage action through ActionSystem
        ActionSystem.Instance.Perform(damageAction);
        
        Debug.Log($"[CombatTestTools] Dealt 1000 damage to {enemy.name}");
    }

    /// <summary>
    /// Validates if the menu item should be enabled
    /// </summary>
    [MenuItem("Tools/Combat Test/Deal Damage/Deal 1000 Damage to Enemy", true)]
    public static bool ValidateDeal1000DamageToEnemy()
    {
        // Only enable in play mode
        return Application.isPlaying;
    }

    // ========== SPECIAL CARD TEST FUNCTIONS ==========

    /// <summary>
    /// TEST FUNCTION: Gives the player all 3 special cards (Agos, Balaraw, Kalasag)
    /// for manual testing. Assigns them to active heroes when in Play Mode so they
    /// appear in each hero's hand during combat; otherwise adds them to the
    /// collection (persists via PlayerPrefs).
    /// </summary>
    [MenuItem("Tools/Combat Test/Special Cards/Give All Special Cards (Agos, Balaraw, Kalasag)", false, 30)]
    public static void GiveAllSpecialCards()
    {
        var cards = new List<SpecialCardData>();
        var agos = AssetDatabase.LoadAssetAtPath<SpecialCardData>("Assets/Data/Rewards/Agos.asset");
        var balaraw = AssetDatabase.LoadAssetAtPath<SpecialCardData>("Assets/Data/Rewards/Balaraw.asset");
        var kalasag = AssetDatabase.LoadAssetAtPath<SpecialCardData>("Assets/Data/Rewards/Kalasag.asset");
        if (agos != null) cards.Add(agos);
        if (balaraw != null) cards.Add(balaraw);
        if (kalasag != null) cards.Add(kalasag);

        GiveSpecialCards("Special Cards Given", "Agos, Balaraw, Kalasag", cards);
    }

    /// <summary>
    /// TEST FUNCTION: Gives the player only the Agos special card
    /// </summary>
    [MenuItem("Tools/Combat Test/Special Cards/Add Agos Only", false, 32)]
    public static void AddAgosOnly()
    {
        var agos = AssetDatabase.LoadAssetAtPath<SpecialCardData>("Assets/Data/Rewards/Agos.asset");
        GiveSpecialCards("Agos Given", "Agos", new List<SpecialCardData> { agos });
    }

    /// <summary>
    /// TEST FUNCTION: Gives the player only the Balaraw special card
    /// </summary>
    [MenuItem("Tools/Combat Test/Special Cards/Add Balaraw Only", false, 33)]
    public static void AddBalarawOnly()
    {
        var balaraw = AssetDatabase.LoadAssetAtPath<SpecialCardData>("Assets/Data/Rewards/Balaraw.asset");
        GiveSpecialCards("Balaraw Given", "Balaraw", new List<SpecialCardData> { balaraw });
    }

    /// <summary>
    /// TEST FUNCTION: Gives the player only the Kalasag special card
    /// </summary>
    [MenuItem("Tools/Combat Test/Special Cards/Add Kalasag Only", false, 34)]
    public static void AddKalasagOnly()
    {
        var kalasag = AssetDatabase.LoadAssetAtPath<SpecialCardData>("Assets/Data/Rewards/Kalasag.asset");
        GiveSpecialCards("Kalasag Given", "Kalasag", new List<SpecialCardData> { kalasag });
    }

    /// <summary>
    /// Shared implementation for granting special cards: assigns to active heroes
    /// in Play Mode (so cards enter their hands), otherwise adds them to the collection.
    /// </summary>
    private static void GiveSpecialCards(string dialogTitle, string cardNames, List<SpecialCardData> cards)
    {
        // Load the persistent collection (same path CardSystem uses)
        var collection = Resources.Load<SpecialCardCollectionData>("Special Card Collection");
        if (collection == null)
        {
            EditorUtility.DisplayDialog(
                "Collection Not Found",
                "SpecialCardCollectionData asset not found in Resources.\n\nExpected at: Assets/Data/Data Persistence/Resources/Special Card Collection.asset",
                "OK"
            );
            Debug.LogWarning("[CombatTestTools] SpecialCardCollectionData not found in Resources!");
            return;
        }

        var validCards = new List<SpecialCardData>();
        foreach (var card in cards)
        {
            if (card != null) validCards.Add(card);
        }

        if (validCards.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "Cards Not Found",
                $"Could not load special card asset(s) from Assets/Data/Rewards/ ({cardNames}).",
                "OK"
            );
            Debug.LogWarning($"[CombatTestTools] No special card assets found for: {cardNames}");
            return;
        }

        // Gather active hero names (assigns card to a hero so it enters their hand)
        List<string> activeHeroNames = new List<string>();
        if (Application.isPlaying && HeroSystem.Instance != null && HeroSystem.Instance.HeroViews != null)
        {
            foreach (var heroView in HeroSystem.Instance.HeroViews)
            {
                if (heroView != null && heroView.HeroData != null && !string.IsNullOrEmpty(heroView.HeroData.HeroName))
                {
                    activeHeroNames.Add(heroView.HeroData.HeroName);
                }
            }
        }

        collection.Load();

        int assignedCount = 0;
        int alreadyOwnedCount = 0;
        foreach (var card in validCards)
        {
            if (collection.HasCard(card.CardId))
            {
                alreadyOwnedCount++;
                Debug.Log($"[CombatTestTools] Special card '{card.CardName}' already in collection. Skipping.");
                continue;
            }

            bool added;
            if (Application.isPlaying && activeHeroNames.Count > 0)
            {
                added = collection.AddSpecialCardToRandomHero(card, activeHeroNames);
            }
            else
            {
                // No heroes (Edit Mode) - just collect the card so it persists
                added = collection.AddSpecialCard(card);
                if (added && !Application.isPlaying)
                {
                    Debug.Log($"[CombatTestTools] Added special card '{card.CardName}' to collection (Edit Mode - no hero assignment). Run in Play Mode to assign to heroes.");
                }
            }

            if (added) assignedCount++;
        }

        // Refresh the in-scene panel if it exists (Play Mode)
        if (Application.isPlaying)
        {
            var panel = Object.FindFirstObjectByType<SpecialCardPanelUI>();
            if (panel != null)
            {
                panel.RefreshDisplay();
            }
        }

        Debug.Log($"[CombatTestTools] Special cards given: {assignedCount} added, {alreadyOwnedCount} already owned.");

        EditorUtility.DisplayDialog(
            dialogTitle,
            $"Successfully gave {assignedCount} special card(s) to the collection.\n\n{(alreadyOwnedCount > 0 ? $"{alreadyOwnedCount} card(s) were already owned.\n\n" : "")}" +
            (Application.isPlaying && activeHeroNames.Count > 0
                ? "Assigned to active heroes - they should appear in the special card panel / hero hands."
                : "Collected only (no active heroes). Enter Play Mode with heroes present to have them assigned."),
            "OK"
        );
    }

    /// <summary>
    /// TEST FUNCTION: Clears all special cards from the collection
    /// </summary>
    [MenuItem("Tools/Combat Test/Special Cards/Clear Special Cards", false, 31)]
    public static void ClearSpecialCards()
    {
        var collection = Resources.Load<SpecialCardCollectionData>("Special Card Collection");
        if (collection == null)
        {
            EditorUtility.DisplayDialog(
                "Collection Not Found",
                "SpecialCardCollectionData asset not found in Resources.\n\nExpected at: Assets/Data/Data Persistence/Resources/Special Card Collection.asset",
                "OK"
            );
            Debug.LogWarning("[CombatTestTools] SpecialCardCollectionData not found in Resources!");
            return;
        }

        collection.Load();
        collection.Clear();

        if (Application.isPlaying)
        {
            var panel = Object.FindFirstObjectByType<SpecialCardPanelUI>();
            if (panel != null)
            {
                panel.RefreshDisplay();
            }
        }

        Debug.Log("[CombatTestTools] Cleared all special cards from collection.");
        EditorUtility.DisplayDialog("Special Cards Cleared", "All special cards removed from the collection.", "OK");
    }
}

