using AudioSystem;
using UnityEngine;

public class CoreManager : MonoBehaviour
{
    void Start()
    {
        // Core Setup for the game
        // Load everything Like Audio Managers, Save System, ...
        SceneController.Instance
        .NewTransition()
        .Load(SceneDatabase.Slots.LoadingScreen, SceneDatabase.Scenes.LoadingScreen, setActive: true)
        .Perform();
    }
}
