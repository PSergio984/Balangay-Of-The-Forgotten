using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pre-session name entry panel displayed on the Main Menu overlay.
/// Prompts the player to enter their name before starting a session/game.
/// </summary>
public class SessionNameEntryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button playButton;
    [SerializeField] private GameProgressData gameProgressData;

    [Header("Behavior")]
    [Tooltip("If true, automatically opens panel on Start if no session player name is set.")]
    [SerializeField] private bool autoShowIfNoName = true;

    private void Awake()
    {
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayClicked);
        }

        if (nameInputField != null)
        {
            nameInputField.characterLimit = 20;
            nameInputField.onValueChanged.AddListener(OnInputChanged);
        }
    }

    private void Start()
    {
        // Always start hidden; Show() will activate when needed
        gameObject.SetActive(false);

        if (autoShowIfNoName && gameProgressData != null && !gameProgressData.HasPlayerName)
        {
            Show();
        }
    }

    private void OnDestroy()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveListener(OnPlayClicked);
        }

        if (nameInputField != null)
        {
            nameInputField.onValueChanged.RemoveListener(OnInputChanged);
        }
    }

    /// <summary>
    /// Displays the session name entry panel.
    /// </summary>
    public void Show()
    {
        // Activate this root GameObject (it starts inactive in the scene)
        gameObject.SetActive(true);

        if (nameInputField != null)
        {
            string current = (gameProgressData != null && gameProgressData.HasPlayerName)
                ? gameProgressData.PlayerName
                : string.Empty;
            nameInputField.text = current;
            nameInputField.ActivateInputField();
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(nameInputField.gameObject);
            }
        }
    }

    /// <summary>
    /// Hides the session name entry panel.
    /// </summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnInputChanged(string value)
    {
        // Allowed to be blank (defaults to Anonymous)
    }

    private void OnPlayClicked()
    {
        string rawName = nameInputField != null ? nameInputField.text : string.Empty;
        if (gameProgressData != null)
        {
            gameProgressData.SetPlayerName(rawName);
        }

        Hide();
    }
}
