using AudioSystem;
using UnityEngine;


public class MapSelector : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private SoundData CombatMusic;
    [SerializeField] private SoundData MenuMusic;
    [SerializeField] private float MusicFadeTime = 2f;

    [Header("Level Data")]
    [Tooltip("Assign the LevelTransitionData asset used for passing map data to combat scene.")]
    [SerializeField] private LevelTransitionData levelTransitionData;


    public void StartCombat()
    {

        if (levelTransitionData == null)
        {
            Debug.LogError($"[MapSelector] LevelTransitionData not assigned on {gameObject.name}. Combat scene won't receive map data. Aborting transition.", this);
            return;
        }
        if (levelTransitionData.SelectedMapData == null)
        {
            Debug.LogError($"[MapSelector] No MapData selected in LevelTransitionData on {gameObject.name}. You must select a map before starting combat. Aborting transition.", this);
            return;
        }

        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.SessionContent, SceneDatabase.Scenes.Combat, setActive: true)
            .WithOverlay()
            .WithPauseMusic()
            .Perform();
    }

    public void ReturnMainMenu()
    {
        SceneController.Instance
            .NewTransition()
            .Unload(SceneDatabase.Slots.SessionContent)
            .Unload(SceneDatabase.Slots.Session)
            .Load(SceneDatabase.Slots.MainMenu, SceneDatabase.Scenes.MainMenu, setActive: true)
            .WithMusic(MenuMusic, MusicFadeTime)
            .WithOverlay()
            .Perform();
    }

    public void GoToCredits(){
         SceneController.Instance
            .NewTransition()
            .Unload(SceneDatabase.Slots.SessionContent)
            .Unload(SceneDatabase.Slots.Session)
            .Load(SceneDatabase.Slots.MainMenu, SceneDatabase.Scenes.MainMenu, setActive: true)
            .WithMusic(MenuMusic, MusicFadeTime)
            .WithOverlay()
            .Perform();
    }

}
