using UnityEngine;


/// <summary>
/// Utility for managing and accessing the current active hero in the party.
/// Centralizes logic for determining which hero is "current" for targeting, effects, etc.
/// </summary>
public static class CurrentHeroUtil
{
    // -------------------------------------------------------------------------
    // Maximum number of heroes allowed in the party (hard limit for party size)
    // -------------------------------------------------------------------------
    private const int MaxHeroLimit = 4;

    // -------------------------------------------------------------------------
    // Index of the current hero in the party (0-based, default is 0)
    // This tracks which hero is currently active for turn-based actions.
    // -------------------------------------------------------------------------
    private static int currentHeroIndex = 0;

    /// <summary>
    /// Gets or sets the current hero index (0-based).
    /// Use this property to change which hero is currently active.
    /// </summary>
    public static int CurrentHeroIndex
    {
        get => currentHeroIndex; // Returns the index of the current hero
        set => currentHeroIndex = value; // Sets the index of the current hero
    }

    /// <summary>
    /// Returns the number of heroes currently in the party (capped at max limit).
    /// This is used to ensure party logic does not exceed the allowed hero count.
    /// </summary>
    public static int GetHeroCount()
    {
        // Get the list of hero views from the HeroSystem singleton
        var heroes = HeroSystem.Instance.HeroViews;
        // If the hero list is null, return 0 (no heroes in party)
        if (heroes == null) return 0;
        // Return the number of heroes, but do not exceed MaxHeroLimit
        return Mathf.Min(heroes.Count, MaxHeroLimit);
    }

    /// <summary>
    /// Advances to the next hero in the party.
    /// Wraps around to 0 if at the end or if the index exceeds the hero count.
    /// This is typically called at the end of a hero's turn to cycle to the next hero.
    /// </summary>
    public static void NextHero()
    {
        // Get the current number of heroes in the party
        int heroCount = GetHeroCount();
        // If there are no heroes, do nothing
        if (heroCount == 0) return;
        // Advance the current hero index, wrapping to 0 if at the end
        currentHeroIndex = (currentHeroIndex + 1) % heroCount;
    }

    /// <summary>
    /// Returns the current active HeroView, or null if not available.
    /// Use this to get the hero object for the current turn or action.
    /// </summary>
    public static HeroView GetCurrentHero()
    {
        // Get the list of hero views from the HeroSystem singleton
        var heroes = HeroSystem.Instance.HeroViews;
        // Get the current number of heroes in the party
        int heroCount = GetHeroCount();
        // If there are no heroes or the list is null, return null
        if (heroes == null || heroCount == 0) return null;
        // If the current hero index is out of bounds, return null
        if (currentHeroIndex < 0 || currentHeroIndex >= heroCount) return null;
        // Return the HeroView at the current hero index
        return heroes[currentHeroIndex];
    }
}
