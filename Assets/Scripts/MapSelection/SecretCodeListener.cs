using UnityEngine;
using System.Collections;

/// <summary>
/// Listens for a secret code (e.g. "pass") and triggers UnlockAllLevels on MapSelectManager.
/// Attach this to your Map Selection menu root.
/// </summary>
public class SecretCodeListener : MonoBehaviour
{
    [Tooltip("Reference to the MapSelectManager in the scene.")]
    [SerializeField] private MapSelectManager mapSelectManager;

    [Tooltip("The secret code to trigger the unlock.")]
    [SerializeField] private string secretCode = "janrel";

    private string _inputBuffer = "";
    private float _resetTime = 2f; // seconds to reset buffer if typing is too slow
    private float _lastInputTime = 0f;

    private Coroutine _inputCoroutine;

    private void Awake()
    {
        // Validate required reference and disable if missing
        if (mapSelectManager == null)
        {
            Debug.LogError("[SecretCodeListener] MapSelectManager reference is missing! Disabling component.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (_inputCoroutine == null)
            _inputCoroutine = StartCoroutine(InputPollingCoroutine());
    }

    private void OnDisable()
    {
        if (_inputCoroutine != null)
        {
            StopCoroutine(_inputCoroutine);
            _inputCoroutine = null;
        }
    }

    /// <summary>
    /// Polls for input at a reduced frequency to minimize overhead.
    /// </summary>
    private IEnumerator InputPollingCoroutine()
    {
        const float pollInterval = 0.07f; // ~14x/sec, fast enough for typing
        while (true)
        {
            if (!string.IsNullOrEmpty(secretCode) && mapSelectManager != null)
            {
                foreach (char c in Input.inputString)
                {
                    if (Time.time - _lastInputTime > _resetTime)
                        _inputBuffer = "";
                    _lastInputTime = Time.time;

                    // Only accept letters, ignore others
                    if (char.IsLetter(c))
                    {
                        _inputBuffer += char.ToLowerInvariant(c);
                        if (_inputBuffer.Length > secretCode.Length)
                        {
                            _inputBuffer = _inputBuffer.Substring(_inputBuffer.Length - secretCode.Length);
                        }                        if (_inputBuffer == secretCode.ToLowerInvariant())
                        {
                            Debug.Log("[SecretCodeListener] Secret code entered! Unlocking all levels.");
                            mapSelectManager.UnlockAllLevels();
                            _inputBuffer = "";
                        }
                    }
                }
            }
            yield return new WaitForSeconds(pollInterval);
        }
    }
}
