using AudioSystem;
using UnityEngine;

public class CoreManager : MonoBehaviour
{
    [SerializeField] private SoundData MainMenuMusic;
    [SerializeField] private float MusicFadeTime = 2f;
    void Start()
    {
        // Core Setup for the game
        // Load everything Like Audio Managers, Save System, ...
        SceneController.Instance
        .NewTransition()
        .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu)
        .WithMusic(MainMenuMusic,MusicFadeTime)
        .WithOverlay()
        .Perform();
    }
}
