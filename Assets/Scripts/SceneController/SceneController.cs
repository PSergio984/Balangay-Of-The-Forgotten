using System.Collections;
using System.Collections.Generic;
using AudioSystem;
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
    
    #region Manual Music Control (Option B)
    
    /// <summary>
    /// Manually pause music for transition (Option B - Manual Control).
    /// Call this before starting a scene transition, then call ResumeMusicAfterTransition() after.
    /// </summary>
    /// <param name="fadeTime">Fade out time for music pause (uses MusicManager default if not specified)</param>
    public static void PauseMusicForTransition(float fadeTime = -1f)
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.PauseMusicForTransition(fadeTime);
        }
    }
    
    /// <summary>
    /// Manually resume music after transition (Option B - Manual Control).
    /// Call this after a scene transition completes to resume the paused music.
    /// </summary>
    /// <param name="fadeTime">Fade in time for music resume (uses MusicManager default if not specified)</param>
    public static void ResumeMusicAfterTransition(float fadeTime = -1f)
    {
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.ResumePausedMusic(fadeTime);
        }
    }
    
    /// <summary>
    /// Check if music is currently paused and waiting to resume.
    /// </summary>
    /// <returns>True if music is paused, false otherwise</returns>
    public static bool IsMusicPaused()
    {
        return MusicManager.Instance != null && MusicManager.Instance.IsPaused;
    }
    
    #endregion

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

    private IEnumerator ChangeSceneRoutine(SceneTransitionPlan plan)
    {
        Debug.Log($"[Frame {Time.frameCount}] ===== TRANSITION START =====");

        // PHASE 1: Change music (if new music specified)
        if (plan.TransitionMusic != null && MusicManager.Instance != null)
        {
            Debug.Log($"[Frame {Time.frameCount}] PHASE 1: Starting music fade");
            MusicManager.Instance.PlayMusic(plan.TransitionMusic, plan.MusicFadeTime);
        }

        // PHASE 2: Fade to black/white (with optional video) and pause music if requested
        if(plan.Overlay && loadingOverlay != null)
        {
            // Pause music simultaneously with overlay fade in
            if (plan.PauseMusicDuringTransition && MusicManager.Instance != null)
            {
                Debug.Log($"[Frame {Time.frameCount}] PHASE 2: Pausing music for transition");
                MusicManager.Instance.PauseMusicForTransition(plan.MusicFadeTime);
            }
            
            if (plan.UseWhiteFade)
            {
                Debug.Log($"[Frame {Time.frameCount}] PHASE 2: Starting fade in to white");
                yield return loadingOverlay.FadeInWhite();
                yield return new WaitForSeconds(0.5f);
            }
            else if (plan.UseVideoLoading)
            {
                Debug.Log($"[Frame {Time.frameCount}] PHASE 2: Starting fade to black with video");
                // Use video loading screen
                yield return loadingOverlay.FadeInWithVideo(plan.LoadingMapId);
            }
            else
            {
                Debug.Log($"[Frame {Time.frameCount}] PHASE 2: Starting fade to black");
                // Standard black fade
                yield return loadingOverlay.FadeInBlack();
            }
            yield return new WaitForSeconds(1f);
        }
        else if (plan.PauseMusicDuringTransition && MusicManager.Instance != null)
        {
            // If no overlay, still pause music
            Debug.Log($"[Frame {Time.frameCount}] PHASE 2: Pausing music for transition (no overlay)");
            MusicManager.Instance.PauseMusicForTransition(plan.MusicFadeTime);
        }

        // PHASE 3: Unload old scenes
        Debug.Log($"[Frame {Time.frameCount}] PHASE 3: Unloading {plan.ScenesToUnload.Count} scenes");
        foreach (var slotKey in plan.ScenesToUnload)
        {
            Debug.Log($"[Frame {Time.frameCount}] Unloading slot: {slotKey}");
            yield return UnloadSceneRoutine(slotKey);
        }

        // PHASE 4: Memory cleanup
        if (plan.ClearUnusedAssets)
        {
            Debug.Log($"[Frame {Time.frameCount}] PHASE 4: Cleaning unused assets");
            yield return CleanupUnusedAssetsRoutine();
        }

        // PHASE 4.5: Wait for loading video to finish BEFORE loading new scenes
        // This ensures the video plays completely while old content is gone
        // and new scenes are loaded only after the video finishes
        if (plan.UseVideoLoading && plan.Overlay && loadingOverlay != null)
        {
            Debug.Log($"[Frame {Time.frameCount}] PHASE 4.5: Waiting for loading video to finish...");
            yield return loadingOverlay.WaitForVideoToFinish();
        }

        // PHASE 5: Load new scenes (now happens AFTER video finishes)
        Debug.Log($"[Frame {Time.frameCount}] PHASE 5: Loading {plan.ScenesToLoad.Count} scenes");
        foreach (var kvp in plan.ScenesToLoad)
        {
            Debug.Log($"[Frame {Time.frameCount}] Loading {kvp.Value} into slot {kvp.Key}");
            if (loadedSceneBySlot.ContainsKey(kvp.Key))
            {
                yield return UnloadSceneRoutine(kvp.Key);
            }
            yield return LoadAdditiveRoutine(kvp.Key, kvp.Value, plan.ActiveSceneName == kvp.Value);
        }
        
        // PHASE 6: Fade from black/white
        if (plan.Overlay && loadingOverlay != null)
        {
            if (plan.UseWhiteFade)
            {
                Debug.Log($"[Frame {Time.frameCount}] PHASE 6: Starting fade out white to black");
                yield return loadingOverlay.FadeOutWhiteToBlack();
                Debug.Log($"[Frame {Time.frameCount}] PHASE 6: Fade white to black COMPLETE");
            }
            else if (plan.UseVideoLoading)
            {
                Debug.Log($"[Frame {Time.frameCount}] PHASE 6: Waiting for loading video to finish...");
                // Wait for video to finish, then fade out
                // minimumDisplayTime ensures scene has time to initialize
                yield return loadingOverlay.FadeOutAfterVideo(1f);
                Debug.Log($"[Frame {Time.frameCount}] PHASE 6: Fade from black COMPLETE");
            }
            else
            {
                Debug.Log($"[Frame {Time.frameCount}] PHASE 6: Starting fade from black");
                // Standard fade out
                yield return loadingOverlay.FadeOutBlack();
                Debug.Log($"[Frame {Time.frameCount}] PHASE 6: Fade from black COMPLETE");
            }
        }
        
        // PHASE 7: Resume paused music (after fade out completes)
        if (plan.PauseMusicDuringTransition && MusicManager.Instance != null)
        {
            Debug.Log($"[Frame {Time.frameCount}] PHASE 7: Resuming paused music");
            MusicManager.Instance.ResumePausedMusic(plan.MusicFadeTime);
        }
        
        Debug.Log($"[Frame {Time.frameCount}] ===== TRANSITION COMPLETE =====");
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
        /// Music to play during this transition (plays immediately when transition starts)
        /// </summary>
        public SoundData TransitionMusic { get; private set; }        
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
        /// Whether to use video loading screen during transition.
        /// Requires Overlay to be true.
        /// </summary>
        public bool UseVideoLoading { get; private set; } = false;
        
        /// <summary>
        /// Whether to use white fade transition (fade in to white, then fade out to black).
        /// Used for credits scene transitions.
        /// </summary>
        public bool UseWhiteFade { get; private set; } = false;
        
        /// <summary>
        /// Map ID for map-specific loading video (e.g., "Apolaki" → "loadingApolaki.mp4").
        /// If null or empty, uses default loading video.
        /// </summary>
        public string LoadingMapId { get; private set; } = null;
        
        /// <summary>
        /// Duration of music fade in/out during transition.
        /// </summary>
        public float MusicFadeTime { get; private set; } = 2f;
        
        /// <summary>
        /// Whether to pause current music during transition and resume it after.
        /// Music stops when overlay fades in (PHASE 2) and resumes when overlay fades out (PHASE 6).
        /// </summary>
        public bool PauseMusicDuringTransition { get; private set; } = false;

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
        /// Enables video loading screen during transition.
        /// Uses map-specific video if mapId provided (e.g., "Apolaki" → "loadingApolaki.mp4").
        /// Automatically enables Overlay if not already enabled.
        /// </summary>
        /// <param name="mapId">Optional map ID for map-specific loading video</param>
        /// <returns>This plan for method chaining.</returns>
        public SceneTransitionPlan WithLoadingVideo(string mapId = null)
        {
            Overlay = true;
            UseVideoLoading = true;
            LoadingMapId = mapId;
            return this;
        }
        
        /// <summary>
        /// Enables white fade transition (fade in to white, then fade out to black).
        /// Used for credits scene transitions.
        /// Automatically enables Overlay if not already enabled.
        /// </summary>
        /// <returns>This plan for method chaining.</returns>
        public SceneTransitionPlan WithWhiteFade()
        {
            Overlay = true;
            UseWhiteFade = true;
            return this;
        }

        /// <summary>
        /// Sets music with custom fade time
        /// </summary>
        public SceneTransitionPlan WithMusic(SoundData music, float fadeTime = 2f)
        {
            TransitionMusic = music;
            MusicFadeTime = fadeTime;
            return this;
        }
        
        /// <summary>
        /// Pauses current music during transition and resumes it after.
        /// Music stops when overlay fades in and resumes when overlay fades out.
        /// Use this when you want the same music to continue playing in the next scene.
        /// </summary>
        /// <param name="fadeTime">Fade time for pause/resume (uses MusicFadeTime if not specified)</param>
        /// <returns>This plan for method chaining.</returns>
        public SceneTransitionPlan WithPauseMusic(float fadeTime = -1f)
        {
            PauseMusicDuringTransition = true;
            if (fadeTime > 0)
            {
                MusicFadeTime = fadeTime;
            }
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