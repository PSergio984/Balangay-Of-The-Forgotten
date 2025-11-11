

using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using AudioSystem;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class MapButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _mapNameText;
     
    public MapData MapData { get; set; }
    private Button _MapButton;

    private Image _MapImage;

    public Color ReturnColor { get; set; }

    [SerializeField] private SoundData CombatMusic;
    [SerializeField] private float MusicFadeTime = 2f;

    private void Awake()
    {
        _MapButton = GetComponent<Button>();
        if (_MapButton == null)
        {
            Debug.LogError($"[MapButton] Missing Button component on {gameObject.name}. Disabling script.");
            enabled = false;
            return;
        }
        _MapImage = GetComponent<Image>();
        if (_MapImage == null)
        {
            Debug.LogError($"[MapButton] Missing Image component on {gameObject.name}. Disabling script.");
            enabled = false;
            return;
        }
        ReturnColor = Color.gray;
    }

    public void Setup(MapData map, bool isUnlocked)
    {
        MapData = map;
        _mapNameText.SetText(map.MapId);
        _MapButton.interactable = isUnlocked;

        if (isUnlocked)
        {
            _MapButton.onClick.AddListener(LoadMap);
            ReturnColor = Color.white;
            _MapImage.color = ReturnColor;
        }
        else
        {
            ReturnColor = Color.gray;
            _MapImage.color = ReturnColor;
        }
    }

    public void Unlock()
    {
        _MapButton.interactable = true;
        _MapButton.onClick.AddListener(LoadMap);
        ReturnColor = Color.white;
        _MapImage.color = ReturnColor;
    }
    
    public void LoadMap()
    {
       SceneController.Instance
            .NewTransition()
            .Unload(SceneDatabase.Slots.SessionContent)
            .Load(SceneDatabase.Slots.SessionContent, SceneDatabase.Scenes.Combat, setActive: true)
            .WithMusic(CombatMusic, MusicFadeTime)
            .WithOverlay()
            .Perform();
    }

}
