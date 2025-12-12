using UnityEngine;

/// <summary>
/// Video controller for lore cutscenes
/// Inherits all video playback functionality from VideoControllerBase
/// </summary>
/// <remarks>
/// <para><strong>Inherits:</strong> VideoControllerBase - All video playback logic is in base class</para>
/// <para><strong>Override:</strong> LoadNextScene() - Transitions to MainMenu after lore videos finish</para>
/// </remarks>
public class LoreController : VideoControllerBase
{
    /// <summary>
    /// Load the next scene - transitions to MainMenu after lore videos finish
    /// </summary>
    protected override void LoadNextScene()
    {
        SceneController.Instance
            .NewTransition()
            .Unload(SceneDatabase.Slots.SessionContent)
            .Load(SceneDatabase.Slots.SessionContent, SceneDatabase.Scenes.MapSelection, setActive: true) 
            .WithLoadingVideo("loading")
            .Perform();
    }
}
