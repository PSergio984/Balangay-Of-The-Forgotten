using UnityEngine;

public class MapSelector : MonoBehaviour
{
    public void StartCombat()
    {
        SceneController.Instance
            .NewTransition()
            .Unload(SceneDatabase.Slots.SessionContent)
            .Load(SceneDatabase.Slots.SessionContent, SceneDatabase.Scenes.Combat, setActive: true)
            .WithOverlay()
            .Perform();
    }

    
}
