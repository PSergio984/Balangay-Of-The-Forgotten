using UnityEditor.Rendering;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // Static property to access the singleton instance from anywhere in the code
    public static T Instance { get; private set; }

    // Called when the GameObject is first created - sets up the singleton instance
    protected virtual void Awake()
    {
        // Check if an instance already exists to prevent duplicates
        if (Instance != null)
        {
            // Destroy this GameObject if another instance already exists
            Destroy(gameObject);
            return;
        }
        else
        {
            // Set this object as the singleton instance
            Instance = this as T;

        }

    }
    
    // Called when the application is quitting - cleanup the singleton
    protected virtual void OnApplicationQuit()
    {
        // Clear the instance reference
        Instance = null;
        // Destroy the GameObject to ensure proper cleanup
        Destroy(gameObject);
    }
    
    // Nested abstract class for persistent singletons that survive scene changes
    public abstract class PersistentSingleton<T> : Singleton<T> where T : MonoBehaviour
    {
        // Override Awake to add persistence across scene loads
        protected override void Awake()
        {
            // Call the base Awake method to set up singleton behavior
            base.Awake();
            // Mark this GameObject to not be destroyed when loading new scenes
            DontDestroyOnLoad(gameObject);
        }
    }
}

