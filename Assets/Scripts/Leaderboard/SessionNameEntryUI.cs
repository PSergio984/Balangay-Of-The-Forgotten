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
    /// Resolves the GameProgressData instance at runtime.
    /// Prefers the singleton; falls back to the serialized field so a
    /// misconfigured prefab still works in the editor.
    /// </summary>
    private GameProgressData ResolveData()
    {
        return GameProgressData.Instance != null ? GameProgressData.Instance : gameProgressData;
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
            // Always start with an empty input. The previous player name must never be
            // pre-filled here — that was leaking previous progress into a "New Player" reset.
            nameInputField.text = string.Empty;
            nameInputField.ActivateInputField();
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(nameInputField.gameObject);
            }
        }

        // Apply current play-button interactivity based on the (now empty) field state.
        if (playButton != null)
        {
            playButton.interactable = nameInputField != null && !string.IsNullOrWhiteSpace(nameInputField.text);
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
        // Gate the play button. A player name is required before starting,
        // so the button must be unclickable while the field is blank.
        if (playButton != null)
        {
            playButton.interactable = !string.IsNullOrWhiteSpace(value);
        }
    }

    private void OnPlayClicked()
    {
        string rawName = nameInputField != null ? nameInputField.text : string.Empty;
        if (string.IsNullOrWhiteSpace(rawName))
        {
            Debug.LogWarning("[SessionNameEntryUI] Player name is required to start. Ignoring empty submission.");
            return;
        }

        var resolvedData = ResolveData();
        if (resolvedData != null)
        {
            resolvedData.SetPlayerName(rawName);
        }

        Hide();
    }
}
