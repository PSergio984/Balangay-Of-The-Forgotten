using UnityEngine;

/// <summary>
/// Component responsible for tracking elapsed combat duration during a combat match.
/// Inherits from Singleton&lt;CombatTimer&gt;.
/// </summary>
public class CombatTimer : Singleton<CombatTimer>
{
    /// <summary>
    /// Total elapsed combat duration in seconds.
    /// </summary>
    public float ElapsedSeconds { get; private set; }

    /// <summary>
    /// Whether the timer is currently actively accumulating elapsed time.
    /// </summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// Starts or resumes the combat timer.
    /// </summary>
    public void StartTimer()
    {
        IsRunning = true;
    }

    /// <summary>
    /// Stops / pauses the combat timer.
    /// </summary>
    public void StopTimer()
    {
        IsRunning = false;
    }

    /// <summary>
    /// Resets the elapsed combat time to zero and stops the timer.
    /// </summary>
    public void ResetTimer()
    {
        ElapsedSeconds = 0f;
        IsRunning = false;
    }

    private void Update()
    {
        if (IsRunning)
        {
            ElapsedSeconds += Time.deltaTime;
        }
    }
}
