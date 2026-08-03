using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Victory screen popup UI component for taking player name input and submitting clear time to LeaderboardManager.
/// </summary>
public class NameEntryUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private GameProgressData gameProgressData;
    [SerializeField] private bool autoSubmitSilent = true;

    private string _mapId;
    private float _clearTimeSeconds;
    private Action _onComplete;

    private void Awake()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmit);
        }

        if (skipButton != null)
        {
            skipButton.onClick.AddListener(OnSkip);
        }
    }

    /// <summary>
    /// Displays the name entry popup or silently auto-submits using the session player name.
    /// </summary>
    /// <param name="mapId">Target map ID cleared.</param>
    /// <param name="clearTimeSeconds">Clear time in seconds.</param>
    /// <param name="onComplete">Callback invoked after submission or skip.</param>
    public void Show(string mapId, float clearTimeSeconds, Action onComplete)
    {
        _mapId = mapId;
        _clearTimeSeconds = clearTimeSeconds;
        _onComplete = onComplete;

        if (autoSubmitSilent)
        {
            string sessionName = "Anonymous";
            if (gameProgressData != null && gameProgressData.HasPlayerName)
            {
                sessionName = gameProgressData.PlayerName;
            }

            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.SubmitEntry(sessionName, _mapId, _clearTimeSeconds);
            }
            else
            {
                Debug.LogWarning("[NameEntryUI] LeaderboardManager.Instance is null. Leaderboard entry was not submitted.", this);
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            _onComplete?.Invoke();
            return;
        }

        if (nameInputField != null)
        {
            nameInputField.text = string.Empty;
            nameInputField.characterLimit = 20;
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        if (nameInputField != null)
        {
            nameInputField.ActivateInputField();
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(nameInputField.gameObject);
            }
        }
    }

    /// <summary>
    /// Hides the name entry panel.
    /// </summary>
    public void Hide()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    /// <summary>
    /// Programmatically triggers the skip flow, exercising the same production path
    /// as clicking the Skip button.
    /// </summary>
    public void TriggerSkip()
    {
        OnSkip();
    }

    private void OnSubmit()
    {
        string rawName = nameInputField != null ? nameInputField.text : string.Empty;
        string trimmedName = string.IsNullOrWhiteSpace(rawName) ? "Anonymous" : rawName.Trim();

        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.SubmitEntry(trimmedName, _mapId, _clearTimeSeconds);
        }
        else
        {
            Debug.LogWarning("[NameEntryUI] LeaderboardManager.Instance is null. Leaderboard entry was not submitted.", this);
        }

        Hide();
        _onComplete?.Invoke();
    }

    private void OnSkip()
    {
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.SubmitEntry("Anonymous", _mapId, _clearTimeSeconds);
        }
        else
        {
            Debug.LogWarning("[NameEntryUI] LeaderboardManager.Instance is null. Leaderboard entry was not submitted.", this);
        }

        Hide();
        _onComplete?.Invoke();
    }
}
