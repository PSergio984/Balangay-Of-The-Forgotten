using UnityEngine;
using AudioSystem;

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
    [Header("Music Settings")]
    [Tooltip("Music to play when transitioning to map selection")]
    [SerializeField] private SoundData mapSelectionMusic;
    
    [Tooltip("Fade time for music transition")]
    [SerializeField] private float MusicFadeTime = 2f;
    
    protected override void Awake()
    {
        base.Awake();
        // Lore videos should not loop the last video
        loopLastVideo = false;
    }
    /// <summary>
    /// Load the next scene - transitions to MainMenu after lore videos finish
    /// </summary>
    protected override void LoadNextScene()
    {
        // Stop music entirely before proceeding with the transition
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.StopMusic(MusicFadeTime);
        }
        
        SceneController.Instance
            .NewTransition()
            .Unload(SceneDatabase.Slots.SessionContent)
            .Load(SceneDatabase.Slots.SessionContent, SceneDatabase.Scenes.MapSelection, setActive: true)
            .WithMusic(mapSelectionMusic, MusicFadeTime)
            .Perform();
    }
}
