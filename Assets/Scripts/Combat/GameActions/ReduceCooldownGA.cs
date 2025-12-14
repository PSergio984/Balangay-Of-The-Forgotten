using UnityEngine;

/// <summary>
/// Game action that reduces cooldown for all cards at the start of player turn
/// </summary>
public class ReduceCooldownGA : GameAction
{
    /// <summary>
    /// The hero index whose cards should have cooldown reduced. -1 means all heroes.
    /// </summary>
    public int HeroIndex { get; private set; }
    
    /// <summary>
    /// Creates a new reduce cooldown action
    /// </summary>
    /// <param name="heroIndex">The index of the hero, or -1 for all heroes</param>
    public ReduceCooldownGA(int heroIndex = -1)
    {
        HeroIndex = heroIndex;
    }
}
