namespace AudioSystem {
    using UnityEngine;
    
    /// <summary>
    /// A base class that ensures only one instance of a component exists across all scenes.
    /// Perfect for managers that need to survive scene loads (like audio, game state, etc).
    /// Inherit from this to make any MonoBehaviour persistent and globally accessible.
    /// </summary>
    /// <typeparam name="T">The type of component this singleton manages (usually your own class).</typeparam>
    /// <remarks>
    /// <para><strong>⚠️ CRITICAL SETUP REQUIREMENT:</strong></para>
    /// <para>PersistentSingletons MUST be on their own dedicated GameObject, NOT attached to:</para>
    /// <list type="bullet">
    /// <item>Cameras (will move camera to DontDestroyOnLoad, breaking scene cameras)</item>
    /// <item>Lights (will persist lights across scenes incorrectly)</item>
    /// <item>EventSystems (will cause UI input issues across scenes)</item>
    /// <item>Any other scene-critical components</item>
    /// </list>
    /// <para>The singleton will automatically unparent itself and move to DontDestroyOnLoad,
    /// which will affect ALL components on the same GameObject!</para>
    /// </remarks>
    /// <example>
    /// // ✅ CORRECT: SoundManager on its own GameObject
    /// public class SoundManager : PersistentSingleton&lt;SoundManager&gt; {
    ///     // Your code here
    /// }
    /// // Access from anywhere: SoundManager.Instance.PlaySound();
    /// 
    /// // ❌ WRONG: Don't attach to Camera, Light, or other scene objects!
    /// </example>
    public class PersistentSingleton<T> : MonoBehaviour where T : Component {
        
        /// <summary>
        /// If true, removes this object from its parent on Awake.
        /// This prevents the object from being destroyed when its parent is destroyed.
        /// Usually you want this set to true.
        /// </summary>
        public bool AutoUnparentOnAwake = true;
        
        /// <summary>
        /// The single instance of this component that exists in the game.
        /// Don't access this directly - use the Instance property instead.
        /// </summary>
        protected static T instance;
        
        /// <summary>
        /// Checks if an instance already exists without creating one.
        /// Use this to avoid accidentally creating the singleton.
        /// </summary>
        /// <returns>True if the instance exists, false otherwise.</returns>
        public static bool HasInstance => instance != null;
        
        /// <summary>
        /// Gets the instance if it exists, or returns null without creating one.
        /// Useful when you're not sure if the singleton should exist yet.
        /// </summary>
        /// <returns>The existing instance, or null if none exists.</returns>
        public static T TryGetInstance() => HasInstance ? instance : null;
        
        /// <summary>
        /// Gets the single instance of this component.
        /// If no instance exists, it will find one in the scene or create a new GameObject with the component (play mode only).
        /// Returns null in edit mode to prevent unintended GameObject creation.
        /// </summary>
        /// <returns>The singleton instance.</returns>
        /// <example>
        /// SoundManager.Instance.PlaySound(mySound);
        /// GameManager.Instance.AddScore(100);
        /// </example>
        public static T Instance {
            get {
                if (instance == null) {
                    // Don't auto-create in edit mode
                    if (!Application.isPlaying) return null;
                    
                    // Try to find an existing instance in the scene
                    instance = FindAnyObjectByType<T>();
                    if (instance == null) {
                        // No instance found, create a new one
                        var go = new GameObject(typeof(T).Name + " Auto-Generated");
                        instance = go.AddComponent<T>();
                    }
                }
                return instance;
            }
        }        
        /// <summary>
        /// Unity's Awake method. Sets up the singleton when the component wakes up.
        /// IMPORTANT: If you override Awake in your class, make sure to call base.Awake()
        /// or your singleton won't work properly.
        /// </summary>
        /// <example>
        /// protected override void Awake() {
        ///     base.Awake(); // ALWAYS call this first!
        ///     // Your initialization code here
        /// }
        /// </example>
        protected virtual void Awake() {
            InitializeSingleton();
        }
        
        /// <summary>
        /// Handles the singleton setup logic.
        /// Makes sure only one instance exists and marks it to persist across scenes.
        /// Destroys any duplicate instances automatically.
        /// </summary>
        protected virtual void InitializeSingleton() {
            // Don't run in edit mode
            if (!Application.isPlaying) return;
            
            // Safety check: Prevent unparenting if this GameObject has critical scene components
            // like Camera, Light, or EventSystem that should stay with their scene
            if (AutoUnparentOnAwake) {
                // Check if this GameObject has components that should remain in their scene
                bool hasCriticalComponents = GetComponent<Camera>() != null 
                    || GetComponent<Light>() != null 
                    || GetComponent<UnityEngine.EventSystems.EventSystem>() != null;
                
                if (hasCriticalComponents) {
                    Debug.LogError($"[PersistentSingleton] {typeof(T).Name} is attached to a GameObject with critical scene components (Camera/Light/EventSystem). " +
                                   $"PersistentSingletons should be on their own dedicated GameObjects to prevent unintended scene destruction. " +
                                   $"Please move {typeof(T).Name} to a separate GameObject.", this);
                    // Don't unparent or persist this GameObject
                    return;
                }
                
                transform.SetParent(null);
            }
            
            // Set up the singleton instance
            if (instance == null) {
                instance = this as T;
                DontDestroyOnLoad(gameObject); // Survives scene loads
            }
            else {
                // Another instance already exists, destroy this duplicate
                if (instance != this) {
                    Destroy(gameObject);
                }
            }
        }
    }
}