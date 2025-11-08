using AudioSystem;
using UnityEngine;

public class MapSelector : MonoBehaviour
{

    [SerializeField] private SoundData CombatMusic;
    [SerializeField] private SoundData MenuMusic;
    [SerializeField] private float MusicFadeTime = 2f;

    public void StartCombat()
    {
        SceneController.Instance
            .NewTransition()
            .Unload(SceneDatabase.Slots.SessionContent)
            .Load(SceneDatabase.Slots.SessionContent, SceneDatabase.Scenes.Combat, setActive: true)
            .WithMusic(CombatMusic, MusicFadeTime)
            .WithOverlay()
            .Perform();
    }

    public void ReturnMainMenu()
    {
        SceneController.Instance
            .NewTransition()
            .Unload(SceneDatabase.Slots.SessionContent)
            .Unload(SceneDatabase.Slots.Session)
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu, setActive: true)
            .WithMusic(MenuMusic, MusicFadeTime)
            .WithOverlay()
            .Perform();
    }

}
