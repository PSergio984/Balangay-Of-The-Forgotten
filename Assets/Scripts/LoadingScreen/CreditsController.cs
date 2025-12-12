using UnityEngine;

/// <summary>
/// Video controller for credits sequence
/// Inherits all video playback functionality from VideoControllerBase
/// </summary>
/// <remarks>
/// <para><strong>Inherits:</strong> VideoControllerBase - All video playback logic is in base class</para>
/// <para><strong>Override:</strong> LoadNextScene() - Transitions to MainMenu after credits finish</para>
/// </remarks>
public class CreditsController : VideoControllerBase
{
    protected override void Awake()
    {
        base.Awake();
        // Credits videos should not loop the last video
        loopLastVideo = false;
    }
    /// <summary>
    /// Load the next scene - transitions to MainMenu after credits finish
    /// </summary>
    protected override void LoadNextScene()
    {
        SceneController.Instance
            .NewTransition()
            .Unload(SceneDatabase.Slots.LoadingScreen)
            .Load(SceneDatabase.Slots.MainMenu, SceneDatabase.Scenes.MainMenu, setActive: true) 
            .Perform();
    }
}
