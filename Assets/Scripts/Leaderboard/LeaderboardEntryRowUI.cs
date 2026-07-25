using TMPro;
using UnityEngine;

/// <summary>
/// Controller for a single leaderboard entry row UI prefab.
/// </summary>
public class LeaderboardEntryRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text clearTimeText;

    /// <summary>
    /// Populates rank, player name, and clear time text components.
    /// </summary>
    /// <param name="rank">1-based rank index.</param>
    /// <param name="playerName">Player name.</param>
    /// <param name="clearTimeSeconds">Clear time in seconds.</param>
    public void Populate(int rank, string playerName, float clearTimeSeconds)
    {
        if (rankText != null)
        {
            rankText.text = $"#{rank}";
        }

        if (playerNameText != null)
        {
            playerNameText.text = playerName;
        }

        if (clearTimeText != null)
        {
            clearTimeText.text = FormatTime(clearTimeSeconds);
        }
    }

    /// <summary>
    /// Formats seconds into "m:ss" string format (e.g. 94.3s -> "1:34").
    /// </summary>
    /// <param name="seconds">Total duration in seconds.</param>
    /// <returns>Formatted time string.</returns>
    public static string FormatTime(float seconds)
    {
        if (seconds < 0f) seconds = 0f;
        int m = (int)(seconds / 60f);
        int s = (int)(seconds % 60f);
        return $"{m}:{s:D2}";
    }
}
