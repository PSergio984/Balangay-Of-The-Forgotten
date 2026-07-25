using UnityEngine;


public class GameAssets : MonoBehaviour
{
    private static GameAssets _i;
    private static readonly object _lock = new object();
    public static GameAssets i {
        get {
            if (_i == null)
            {
                // Try to find an existing instance in the scene
                _i = Object.FindFirstObjectByType<GameAssets>();
                if (_i == null)
                {
                    // Try to load from Resources (do not instantiate here, just log error)
                    var prefab = Resources.Load<GameObject>("GameAssets");
                    if (prefab == null)
                    {
                        Debug.LogError("[GameAssets] GameAssets prefab not found in Resources! Singleton will remain null.");
                    }
                    else
                    {
                        Debug.LogError("[GameAssets] No GameAssets instance found in scene. Please add one to your scene at startup.");
                    }
                }
            }
            return _i;
        }
    }

    private void Awake()
    {
        lock (_lock)
        {
            if (_i == null)
            {
                _i = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_i != this)
            {
                Debug.LogWarning("[GameAssets] Duplicate GameAssets instance detected. Destroying this instance.");
                Destroy(gameObject);
                return;
            }
        }
    }

    private void OnDestroy()
    {
        if (_i == this)
        {
            _i = null;
        }
    }

    [Tooltip("Assign the DamagePopUp prefab (GameObject) here")] 
    public GameObject pfDamagePopup;
}
