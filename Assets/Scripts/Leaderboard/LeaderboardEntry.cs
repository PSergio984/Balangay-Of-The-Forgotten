using System;

/// <summary>
/// Serializable data class representing a single player's leaderboard record for a map clear.
/// </summary>
[Serializable]
public class LeaderboardEntry
{
    /// <summary>
    /// Player display name (max 20 characters; "Anonymous" if skipped).
    /// </summary>
    public string PlayerName;

    /// <summary>
    /// Map ID matching GameProgressData map ID constants.
    /// </summary>
    public string MapId;

    /// <summary>
    /// Clear time in seconds (e.g., 94.3f).
    /// </summary>
    public float ClearTime;

    /// <summary>
    /// Timestamp of entry creation in ISO-8601 UTC format (e.g., "2026-07-25T07:30:00Z").
    /// </summary>
    public string DateTimeUtc;
}
