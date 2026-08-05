using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles Arcade-style "New Player" reset functionality on Main Menu.
/// Resets session progress & name, while preserving leaderboard persistent data.
/// Prompts for confirmation before resetting.
/// </summary>
public class NewPlayerButtonUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button newPlayerButton;
    [SerializeField] private GameObject confirmationPanelRoot;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;
    [SerializeField] private GameProgressData gameProgressData;
    [SerializeField] private SessionNameEntryUI sessionNameEntryUI;

    private void OnEnable()
    {
        if (newPlayerButton != null)
        {
            newPlayerButton.onClick.AddListener(OnNewPlayerClicked);
        }

        if (confirmYesButton != null)
        {
            confirmYesButton.onClick.AddListener(OnConfirmReset);
        }

        if (confirmNoButton != null)
        {
            confirmNoButton.onClick.AddListener(OnCancelReset);
        }
    }

    private void OnDisable()
    {
        if (newPlayerButton != null)
        {
            newPlayerButton.onClick.RemoveListener(OnNewPlayerClicked);
        }

        if (confirmYesButton != null)
        {
            confirmYesButton.onClick.RemoveListener(OnConfirmReset);
        }

        if (confirmNoButton != null)
        {
            confirmNoButton.onClick.RemoveListener(OnCancelReset);
        }
    }

    private void OnNewPlayerClicked()
    {
        if (confirmationPanelRoot != null)
        {
            confirmationPanelRoot.SetActive(true);
        }
        else
        {
            OnConfirmReset();
        }
    }

    private void OnConfirmReset()
    {
        // Prefer the runtime singleton so the reset always lands on the same
        // GameProgressData instance the name entry panel reads. If both components
        // were wired to different asset instances, the reset would be invisible to
        // the panel and the previous player name would leak back in.
        var resolvedData = GameProgressData.Instance != null ? GameProgressData.Instance : gameProgressData;
        if (resolvedData != null)
        {
            resolvedData.ResetProgress();
        }

        if (confirmationPanelRoot != null)
        {
            confirmationPanelRoot.SetActive(false);
        }

        if (sessionNameEntryUI != null)
        {
            sessionNameEntryUI.Show();
        }
    }

    private void OnCancelReset()
    {
        if (confirmationPanelRoot != null)
        {
            confirmationPanelRoot.SetActive(false);
        }
    }
}
