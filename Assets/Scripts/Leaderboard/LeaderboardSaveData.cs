using System;
using System.Collections.Generic;

/// <summary>
/// Serializable container class representing the full saved leaderboard state.
/// </summary>
[Serializable]
public class LeaderboardSaveData
{
    /// <summary>
    /// List of leaderboard entries stored in persistence.
    /// </summary>
    public List<LeaderboardEntry> Entries = new List<LeaderboardEntry>();
}
