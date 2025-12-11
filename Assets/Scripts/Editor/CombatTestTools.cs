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
}

