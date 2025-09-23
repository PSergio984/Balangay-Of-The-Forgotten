using UnityEngine;


/// <summary>
/// Utility for managing and accessing the current active hero in the party.
/// Centralizes logic for determining which hero is "current" for targeting, effects, etc.
/// </summary>
public static class CurrentHeroUtil
{
    // Maximum number of heroes allowed (hard limit)
    private const int MaxHeroLimit = 4;

    // Index of the current hero in the party (default 0)
    private static int currentHeroIndex = 0;

    /// <summary>
    /// Gets or sets the current hero index (0-based)
    /// </summary>
    public static int CurrentHeroIndex
    {
        get => currentHeroIndex;
        set => currentHeroIndex = value;
    }

    /// <summary>
    /// Returns the number of heroes currently in the party (capped at max limit)
    /// </summary>
    public static int GetHeroCount()
    {
        var heroes = HeroSystem.Instance.HeroViews;
        if (heroes == null) return 0;
        return Mathf.Min(heroes.Count, MaxHeroLimit);
    }

    /// <summary>
    /// Advances to the next hero, wrapping to 0 if at the end or exceeding max limit.
    /// </summary>
    public static void NextHero()
    {
        int heroCount = GetHeroCount();
        if (heroCount == 0) return;
        currentHeroIndex = (currentHeroIndex + 1) % heroCount;
    }

    /// <summary>
    /// Returns the current active HeroView, or null if not available
    /// </summary>
    public static HeroView GetCurrentHero()
    {
        var heroes = HeroSystem.Instance.HeroViews;
        int heroCount = GetHeroCount();
        if (heroes == null || heroCount == 0) return null;
        if (currentHeroIndex < 0 || currentHeroIndex >= heroCount) return null;
        return heroes[currentHeroIndex];
    }
}
