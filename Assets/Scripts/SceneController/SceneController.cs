using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages loading and unloading multiple scenes with smooth transitions.
/// Uses a slot-based system where each scene is assigned to a named slot for easy management.
/// Supports fade transitions, resource cleanup, and prevents concurrent scene changes.
/// Perfect for multi-scene architectures (UI layer, gameplay layer, background layer, etc).
/// </summary>
/// <example>
/// // Basic scene transition with fade
/// SceneController.Instance.NewTransition()
///     .Load("gameplay", "Level_01", setActive: true)
///     .Load("ui", "GameplayUI")
///     .WithOverlay()
///     .Perform();
/// 
/// // Replace one scene with another
/// SceneController.Instance.NewTransition()
///     .Unload("gameplay")
///     .Load("gameplay", "Level_02", setActive: true)
///     .WithClearUnusedAssets()
///     .WithOverlay()
///     .Perform();
/// </example>
public class SceneController : MonoBehaviour
{
    #region Singleton
    /// <summary>
    /// The single instance of SceneController available globally.
    /// Access this from anywhere to trigger scene transitions.
    /// </summary>
    public static SceneController Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    #endregion

    /// <summary>
    /// The UI overlay used for fade in/out transitions between scenes.
    /// Assign this in the inspector - typically a black image that covers the screen.
    /// </summary>
    [SerializeField] private LoadingOverlay loadingOverlay;

    /// <summary>
    /// Tracks which scene is currently loaded in each slot.
    /// Key = slot name (like "gameplay", "ui", "background")
    /// Value = actual scene name (like "Level_01", "MainMenu")
    /// </summary>
    private Dictionary<string, string> loadedSceneBySlot = new();

    /// <summary>
    /// Prevents multiple scene transitions from happening at the same time.
    /// New transition requests are rejected while one is in progress.
    /// </summary>
    private bool isBusy = false;


    /// <summary>
    /// Creates a new scene transition plan that you can configure with a builder pattern.
    /// Chain methods together to define what scenes to load/unload, then call Perform().
    /// </summary>
    /// <returns>A new transition plan ready to be configured.</returns>
    /// <example>
    /// SceneController.Instance.NewTransition()
    ///     .Load("gameplay", "BossArena")
    ///     .Unload("shop")
    ///     .WithOverlay()
    ///     .Perform();
    /// </example>
    public SceneTransitionPlan NewTransition()
    {
        return new SceneTransitionPlan();
    }

    /// <summary>
    /// Executes a scene transition plan.
    /// Don't call this directly - use plan.Perform() instead.
    /// Prevents overlapping transitions by checking the busy flag.
    /// </summary>
    /// <param name="plan">The configured transition plan to execute.</param>
    /// <returns>The coroutine handling the transition, or null if busy.</returns>
    private Coroutine ExecutePlan(SceneTransitionPlan plan)
    {
        if (isBusy)
        {
            Debug.LogWarning("Scene change already in progress.");
            return null;
        }
        isBusy = true;
        return StartCoroutine(ChangeSceneRoutine(plan));
    }

    /// <summary>
    /// The main coroutine that handles the scene transition workflow.
    /// Steps: Fade in → Unload old scenes → Clean assets (optional) → Load new scenes → Fade out (optional).
    /// Automatically sets the busy flag back to false when complete.
    /// </summary>
    /// <param name="plan">The transition plan containing all load/unload instructions.</param>
    private IEnumerator ChangeSceneRoutine(SceneTransitionPlan plan)
    {
        // Fade to black
        yield return loadingOverlay.FadeInBlack();
        yield return new WaitForSeconds(0.5f);

        // Unload old scenes
        foreach (var slotKey in plan.ScenesToUnload)
        {
            yield return UnloadSceneRoutine(slotKey);
        }

        // Optional: Clean up memory
        if (plan.ClearUnusedAssets) yield return CleanupUnusedAssetsRoutine();

        // Load new scenes
        foreach (var kvp in plan.ScenesToLoad)
        {
            // If slot already has a scene, unload it first
            if (!loadedSceneBySlot.ContainsKey(kvp.Key))
            {
                yield return UnloadSceneRoutine(kvp.Key);
            }
            yield return LoadAdditiveRoutine(kvp.Key, kvp.Value, plan.ActiveSceneName == kvp.Value);
        }
        
        // Optional: Fade back to gameplay
        if (plan.Overlay)
        {
            yield return loadingOverlay.FadeOutBlack();
        }
        isBusy = false;
    }

    /// <summary>
    /// Loads a scene additively (on top of existing scenes) into a named slot.
    /// Uses allowSceneActivation to ensure smooth loading without hiccups.
    /// Optionally sets the loaded scene as the active scene for lighting/physics.
    /// </summary>
    /// <param name="slotKey">The slot name to assign this scene to (for later reference).</param>
    /// <param name="sceneName">The actual Unity scene name to load.</param>
    /// <param name="setActive">If true, makes this the active scene (affects lighting, physics context).</param>
    private IEnumerator LoadAdditiveRoutine(string slotKey, string sceneName, bool setActive)
    {
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (loadOp == null) yield break;
        
        // Wait until scene is almost ready (90%)
        loadOp.allowSceneActivation = false;
        while (loadOp.progress < 0.9f)
        {
            yield return null;
        }

        // Allow scene to activate and finish loading
        loadOp.allowSceneActivation = true;
        while (!loadOp.isDone)
        {
            yield return null;
        }
        
        // Set as active scene if requested
        if (setActive)
        {
            Scene newScene = SceneManager.GetSceneByName(sceneName);
            if (newScene.IsValid() && newScene.isLoaded)
            {
                SceneManager.SetActiveScene(newScene);
            }
        }

        // Track which scene is in this slot
        loadedSceneBySlot[slotKey] = sceneName;
    }

    /// <summary>
    /// Unloads a scene from a named slot.
    /// Looks up which scene is currently in that slot and unloads it.
    /// Does nothing if the slot is empty or doesn't exist.
    /// </summary>
    /// <param name="slotKey">The slot to unload (like "gameplay", "ui", etc).</param>
    private IEnumerator UnloadSceneRoutine(string slotKey)
    {
        // Check if this slot has a scene
        if (!loadedSceneBySlot.TryGetValue(slotKey, out string sceneName)) yield break;
        if (string.IsNullOrEmpty(sceneName)) yield break;
        
        // Unload the scene
        AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneName);
        if (unloadOp != null)
        {
            while (!unloadOp.isDone)
            {
                yield return null;
            }
        }
        
        // Remove from tracking
        loadedSceneBySlot.Remove(slotKey);
    }

    /// <summary>
    /// Cleans up unused assets from memory (textures, audio, prefabs not currently in use).
    /// This is like forcing garbage collection but for Unity assets.
    /// Use this between major scene changes to free up memory (like going from level to level).
    /// Can cause a noticeable pause, so only use when necessary.
    /// </summary>
    private IEnumerator CleanupUnusedAssetsRoutine()
    {
        AsyncOperation cleanupOp = Resources.UnloadUnusedAssets();
        while (!cleanupOp.isDone)
        {
            yield return null;
        }
    }

    /// <summary>
    /// A builder pattern class for creating scene transitions.
    /// Chain methods together to define your transition, then call Perform() to execute it.
    /// Keeps track of what scenes to load/unload and transition settings.
    /// </summary>
    /// <example>
    /// var transition = SceneController.Instance.NewTransition()
    ///     .Unload("mainmenu")
    ///     .Load("gameplay", "Level_01", setActive: true)
    ///     .Load("ui", "GameplayUI")
    ///     .WithOverlay()
    ///     .WithClearUnusedAssets()
    ///     .Perform();
    /// </example>
    public class SceneTransitionPlan
    {
        /// <summary>
        /// Dictionary of scenes to load.
        /// Key = slot name, Value = scene name to load into that slot.
        /// </summary>
        public Dictionary<string, string> ScenesToLoad { get; } = new();
        
        /// <summary>
        /// List of slots to unload before loading new scenes.
        /// </summary>
        public List<string> ScenesToUnload { get; } = new();
        
        /// <summary>
        /// Which scene should become the active scene (for lighting/physics).
        /// Empty string means don't change the active scene.
        /// </summary>
        public string ActiveSceneName { get; private set; } = "";
        
        /// <summary>
        /// Whether to run asset cleanup after unloading scenes.
        /// Frees memory but adds a small delay.
        /// </summary>
        public bool ClearUnusedAssets { get; private set; } = false;
        
        /// <summary>
        /// Whether to fade the screen in and out during transition.
        /// If false, transition happens instantly (but scenes still load properly).
        /// </summary>
        public bool Overlay { get; private set; } = false;

        /// <summary>
        /// Adds a scene to load into a specific slot.
        /// If a scene is already in that slot, it gets replaced.
        /// Optionally makes this the active scene for lighting and physics.
        /// </summary>
        /// <param name="slotKey">The slot name (like "gameplay", "ui", "background").</param>
        /// <param name="sceneName">The Unity scene name to load.</param>
        /// <param name="setActive">If true, this becomes the active scene.</param>
        /// <returns>This plan for method chaining.</returns>
        public SceneTransitionPlan Load(string slotKey, string sceneName, bool setActive = false)
        {
            ScenesToLoad[slotKey] = sceneName;
            if (setActive) ActiveSceneName = sceneName;
            return this;
        }

        /// <summary>
        /// Marks a slot to be unloaded during the transition.
        /// The scene currently in that slot will be removed.
        /// Safe to call even if the slot is empty.
        /// </summary>
        /// <param name="slotKey">The slot to unload.</param>
        /// <returns>This plan for method chaining.</returns>
        public SceneTransitionPlan Unload(string slotKey)
        {
            ScenesToUnload.Add(slotKey);
            return this;
        }

        /// <summary>
        /// Enables the fade in/out overlay during transition.
        /// Without this, scenes change instantly (useful for background scene swaps).
        /// With this, player sees a smooth fade to black and back.
        /// </summary>
        /// <returns>This plan for method chaining.</returns>
        public SceneTransitionPlan WithOverlay()
        {
            Overlay = true;
            return this;
        }

        /// <summary>
        /// Enables memory cleanup after unloading scenes.
        /// Unloads unused assets from memory (textures, audio, etc).
        /// Adds a small delay but frees up memory - good for level-to-level transitions.
        /// </summary>
        /// <returns>This plan for method chaining.</returns>
        public SceneTransitionPlan WithClearUnusedAssets()
        {
            ClearUnusedAssets = true;
            return this;
        }

        /// <summary>
        /// Executes this transition plan.
        /// Starts the scene loading/unloading process with all configured settings.
        /// Returns null if a scene change is already in progress.
        /// </summary>
        /// <returns>The coroutine handling the transition, or null if controller is busy.</returns>
        public Coroutine Perform()
        {
            return SceneController.Instance.ExecutePlan(this);
        }

    }


}