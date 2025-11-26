using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using AudioSystem;

/// <summary>
/// Professional video controller with robust error handling and smooth transitions
/// Perfect for Balangay of the Forgotten splash screens and cutscenes
/// </summary>
public class LoadingScreenController : MonoBehaviour
{
    [Header("Playback Mode")]
    [Tooltip("If true, use URL/StreamingAssets for video playback (WebGL/experimental). If false, use native VideoClip (PC/Android). Automatically set at runtime.")]
    private bool useWebGLVideoPlayer = false;

    [Header("Video Settings")]
    [Tooltip("VideoPlayer for PC/Standalone/Android builds")]
    [SerializeField] private VideoPlayer pcVideoPlayer;
    [Tooltip("VideoPlayer for WebGL builds")]
    [SerializeField] private VideoPlayer webglVideoPlayer;
    [Header("Video Object Roots")]
    [Tooltip("Root GameObject for all PC video objects (e.g., VideoPlayer, mesh, UI)")]
    [SerializeField] private GameObject pcVideoRoot;
    [Tooltip("Root GameObject for all WebGL video objects (e.g., VideoPlayer, mesh, UI)")]
    [SerializeField] private GameObject webglVideoRoot;
    
    [Header("Input Settings")]
    [SerializeField] private bool skipOnClick = true;
    [SerializeField] private bool skipOnAnyKey = true;
    [SerializeField] private float minimumPlayTime = 1f; // Prevent accidental immediate skips
    
    [Header("Transition Settings")]
    [SerializeField] private SoundData MenuMusic;
    [SerializeField] private float MusicFadeTime = 2f;

    /// <summary>
    /// The filename of the video to play, located in the StreamingAssets folder (e.g., "Rp_Visuals_Splash.mp4").
    /// </summary>
    [Tooltip("Filename of the video in StreamingAssets (e.g., 'Rp_Visuals_Splash.mp4')")]
    [SerializeField] private string VideoName;



    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    // Private variables
    private bool videoStarted = false;
    private bool videoFinished = false;
    private bool canSkip = false;
    private float videoStartTime;

     void Awake()
    {
#if UNITY_WEBGL
    useWebGLVideoPlayer = true;
#endif
    }
    
    void Start()
    {
        // Enable only the relevant video root
        if (pcVideoRoot != null) pcVideoRoot.SetActive(!useWebGLVideoPlayer);
        if (webglVideoRoot != null) webglVideoRoot.SetActive(useWebGLVideoPlayer);

        // Branch video playback method based on the selected mode
        if (useWebGLVideoPlayer)
        {
            // Use URL/StreamingAssets-based playback (intended for WebGL or testing)
            InitializeWebGLVideo();
        }
        else
        {
            // Use native VideoClip-based playback (PC/Android)
            InitializeVideo();
        }
    }
    
    void Update()
    {
        HandleInput();
    }

    void OnDestroy()
    {
        // Unsubscribe PC VideoPlayer events
        if (pcVideoPlayer != null)
        {
            pcVideoPlayer.loopPointReached -= OnVideoFinished;
            pcVideoPlayer.errorReceived -= OnVideoError;
            pcVideoPlayer.prepareCompleted -= OnVideoPrepared;
        }
        // Unsubscribe WebGL VideoPlayer events
        if (webglVideoPlayer != null)
        {
            webglVideoPlayer.loopPointReached -= OnVideoFinished;
            webglVideoPlayer.errorReceived -= OnVideoError;
            webglVideoPlayer.prepareCompleted -= OnVideoPrepared;
        }
    }

    /// <summary>
    /// Initialize video player with proper error handling
    /// </summary>
    private void InitializeVideo()
    {
        if (pcVideoPlayer == null)
        {
            LogError("PC VideoPlayer component not assigned!");
            LoadNextScene();
            return;
        }

        // Set up video events
        pcVideoPlayer.loopPointReached += OnVideoFinished;
        pcVideoPlayer.errorReceived += OnVideoError;
        pcVideoPlayer.prepareCompleted += OnVideoPrepared;

        // Configure video player
        pcVideoPlayer.source = VideoSource.VideoClip;
        pcVideoPlayer.playOnAwake = false;
        pcVideoPlayer.isLooping = false;
        // Prepare and play
        if (pcVideoPlayer.clip != null)
        {
            pcVideoPlayer.Prepare();
        }
        else
        {
            LogError("No VideoClip assigned to PC VideoPlayer!");
            LoadNextScene();
        }
    }

    /// <summary>
    /// Handle all input for skipping video
    /// </summary>
    private void HandleInput()
    {
        if (!canSkip || videoFinished) return;

        bool shouldSkip = false;

        // Mouse click (and touch, if enabled)
        if (skipOnClick)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                shouldSkip = true;
            }
            // Touch is treated like a click, but only if skipOnClick is enabled
            else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                shouldSkip = true;
            }
        }
        // Keyboard/key input (mutually exclusive with click/touch)
        else if (skipOnAnyKey && Input.anyKeyDown)
        {
            shouldSkip = true;
        }

        if (shouldSkip)
        {
            LogDebug("Video skipped by user input");
            SkipVideo();
        }
    }

    /// <summary>
    /// Called when video is prepared and ready to play
    /// </summary>
    private void OnVideoPrepared(VideoPlayer vp)
    {
        LogDebug("Video prepared, starting playback");
        if (useWebGLVideoPlayer)
        {
            if (webglVideoPlayer != null) webglVideoPlayer.Play();
        }
        else
        {
            if (pcVideoPlayer != null) pcVideoPlayer.Play();
        }
        videoStarted = true;
        videoStartTime = Time.time;
        // Allow skipping after minimum time
        StartCoroutine(EnableSkipAfterDelay());
    }

    /// <summary>
    /// Enable skipping after minimum play time to prevent accidental skips
    /// </summary>
    private IEnumerator EnableSkipAfterDelay()
    {
        yield return new WaitForSeconds(minimumPlayTime);
        canSkip = true;
        LogDebug("Video skipping now enabled");
    }

    /// <summary>
    /// Called when video reaches the end naturally
    /// </summary>
    private void OnVideoFinished(VideoPlayer vp)
    {
        if (videoFinished) return; // Prevent double-calling
        
        LogDebug("Video finished naturally");
        videoFinished = true;
        LoadNextScene();
    }

    /// <summary>
    /// Called when video encounters an error
    /// </summary>
    private void OnVideoError(VideoPlayer vp, string message)
    {
        LogError($"Video error: {message}");
        LoadNextScene(); // Gracefully continue to next scene on error
    }

    /// <summary>
    /// Skip video manually (called by input or external systems)
    /// </summary>
    public void SkipVideo()
    {
        if (videoFinished) return;
        
        videoFinished = true;
        
        if (useWebGLVideoPlayer)
        {
            if (webglVideoPlayer != null && webglVideoPlayer.isPlaying)
                webglVideoPlayer.Stop();
        }
        else
        {
            if (pcVideoPlayer != null && pcVideoPlayer.isPlaying)
                pcVideoPlayer.Stop();
        }
        
        LoadNextScene();
    }

    public void PlayVideoWeb(){
        // This method is now called automatically if useWebGLVideoPlayer is true
        // Kept for compatibility/testing
        InitializeWebGLVideo();
    }

    /// <summary>
    /// Initialize URL/StreamingAssets-based video playback (WebGL/experimental)
    /// </summary>
    private void InitializeWebGLVideo()
    {
        Debug.Log("Initializing WebGL Video Player");
        if (webglVideoPlayer == null)
        {
            LogError("WebGL VideoPlayer component not assigned!");
            LoadNextScene();
            return;
        }

        // Set up video events
        webglVideoPlayer.loopPointReached += OnVideoFinished;
        webglVideoPlayer.errorReceived += OnVideoError;
        webglVideoPlayer.prepareCompleted += OnVideoPrepared;

        // Configure video player
        webglVideoPlayer.playOnAwake = false;
        webglVideoPlayer.isLooping = false;


        // IMPORTANT: Validate VideoName before constructing the path.
        // If VideoName is null or empty, log an error and skip video loading to prevent invalid path issues.
        if (string.IsNullOrEmpty(VideoName))
        {
            LogError("VideoName is not set! Cannot construct video path.");
            LoadNextScene();
            return;
        }

        // Robust, platform-specific path handling for video URL
#if UNITY_WEBGL
        // For WebGL, Application.streamingAssetsPath is a URL; just append the file name
        string videoPath = $"{Application.streamingAssetsPath}/{VideoName}";
        // No need to trim slashes; Unity handles this for WebGL.
#else
        // For file system platforms, use Path.Combine for safety
        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, VideoName);
#endif
        LogDebug($"[WebGLVideo] Using video URL: {videoPath}");
        webglVideoPlayer.url = videoPath;

        // Prepare and play
        if (!string.IsNullOrEmpty(webglVideoPlayer.url))
        {
            webglVideoPlayer.Prepare();
        }
        else
        {
            LogError("No video URL assigned to WebGL VideoPlayer!");
            LoadNextScene();
        }
    }

    /// <summary>
    /// Load the next scene with optional fade transition
    /// </summary>
    private void LoadNextScene()
    {        SceneController.Instance
            .NewTransition()
            .Unload(SceneDatabase.Slots.LoadingScreen)
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu, setActive: true) 
            .WithOverlay()
            .WithMusic(MenuMusic, MusicFadeTime)
            .Perform();
    }

    // Debug logging methods
    private void LogDebug(string message)
    {
        if (showDebugLogs)
            Debug.Log($"[VideoController] {message}");
    }

    private void LogError(string message)
    {
        Debug.LogError($"[VideoController] {message}");
    }

    // Public methods for external control
    public bool IsVideoPlaying
    {
        get
        {
            // Use a ternary operator for clarity: check the correct VideoPlayer based on platform mode.
            return useWebGLVideoPlayer
                ? webglVideoPlayer != null && webglVideoPlayer.isPlaying
                : pcVideoPlayer != null && pcVideoPlayer.isPlaying;
        }
    }
    public bool IsVideoFinished => videoFinished;
    public float VideoProgress
    {
        get
        {
            if (useWebGLVideoPlayer && webglVideoPlayer != null && webglVideoPlayer.length > 0 && !double.IsNaN(webglVideoPlayer.length))
                return Mathf.Clamp01((float)(webglVideoPlayer.time / webglVideoPlayer.length));
            else if (!useWebGLVideoPlayer && pcVideoPlayer != null && pcVideoPlayer.length > 0 && !double.IsNaN(pcVideoPlayer.length))
                return Mathf.Clamp01((float)(pcVideoPlayer.time / pcVideoPlayer.length));
            else
                return 0f;
        }
    }
}
