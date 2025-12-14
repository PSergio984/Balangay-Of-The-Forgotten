using UnityEngine;

/// <summary>
/// Professional video controller with robust error handling and smooth transitions
/// Perfect for Balangay of the Forgotten splash screens and cutscenes
/// </summary>
/// <remarks>
/// <para><strong>Inherits:</strong> VideoControllerBase - All video playback logic is in base class</para>
/// <para><strong>Override:</strong> LoadNextScene() - Transitions to MainMenu after videos finish</para>
/// </remarks>
public class LoadingScreenController : VideoControllerBase
{
    /// <summary>
    /// Load the next scene - transitions to MainMenu after loading screen videos
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
