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

    private void Awake()
    {
        Debug.Log("[SecretCodeListener] Awake called");
        // Validate required reference and disable if missing
        if (mapSelectManager == null)
        {
            Debug.LogError("[SecretCodeListener] MapSelectManager reference is missing! Disabling component.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (string.IsNullOrEmpty(secretCode) || mapSelectManager == null)
            return;

        foreach (char c in Input.inputString)
        {
            Debug.Log($"[SecretCodeListener] Key pressed: {c}");
            if (Time.time - _lastInputTime > _resetTime)
                _inputBuffer = "";
            _lastInputTime = Time.time;

            // Only accept letters, ignore others
            if (char.IsLetter(c))
            {
                _inputBuffer += char.ToLowerInvariant(c);
                Debug.Log($"[SecretCodeListener] Buffer: {_inputBuffer}");
                if (_inputBuffer.Length > secretCode.Length)
                {
                    _inputBuffer = _inputBuffer.Substring(_inputBuffer.Length - secretCode.Length);
                }
                if (_inputBuffer == secretCode.ToLowerInvariant())
                {
                    Debug.Log("[SecretCodeListener] Secret code entered! Unlocking all levels.");
                    mapSelectManager.UnlockAllLevelsExample();
                    _inputBuffer = "";
                }
            }
        }
    }
}
