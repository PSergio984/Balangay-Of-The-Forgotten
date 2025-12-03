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
    
    [Header("Video Sequence - PC/Standalone")]
    [Tooltip("List of VideoClips to play in sequence for PC/Standalone/Android builds")]
    [SerializeField] private System.Collections.Generic.List<VideoClip> pcVideoClips = new System.Collections.Generic.List<VideoClip>();
    
    [Header("Video Sequence - WebGL")]
    [Tooltip("List of video filenames in StreamingAssets folder for WebGL builds (e.g., 'Video1.mp4', 'Video2.mp4')")]
    [SerializeField] private System.Collections.Generic.List<string> webglVideoNames = new System.Collections.Generic.List<string>();
    
    [Header("Per-Video Music Settings")]
    [Tooltip("Optional background music for each video. Leave null for videos with their own audio. Must match video list length.")]
    [SerializeField] private System.Collections.Generic.List<SoundData> videoBackgroundMusic = new System.Collections.Generic.List<SoundData>();
    [Tooltip("Fade time for music transitions between videos (seconds)")]
    [SerializeField] private float musicFadeTime = 1.5f;
    
    [Header("Input Settings")]
    [SerializeField] private bool skipOnClick = true;
    [SerializeField] private bool skipOnAnyKey = true;
    [SerializeField] private float minimumPlayTime = 1f; // Prevent accidental immediate skips
    
    [Header("Press To Continue Settings")]
    [Tooltip("GameObject that displays 'Press To Continue' text")]
    [SerializeField] private GameObject pressToContinueObject;
    [Tooltip("Fade-in duration for Press To Continue text (seconds)")]
    [SerializeField] private float fadeInDuration = 1.5f;
    [Tooltip("Flicker speed for Press To Continue animation (pulses per second)")]
    [SerializeField] private float flickerSpeed = 2f;
    [Tooltip("Minimum alpha during flicker (0-1)")]
    [SerializeField] private float flickerMinAlpha = 0.3f;
    [Tooltip("Maximum alpha during flicker (0-1)")]
    [SerializeField] private float flickerMaxAlpha = 1f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    // Private variables
    private bool videoStarted = false;
    private bool videoFinished = false;
    private bool canSkip = false;
    private float videoStartTime;
    private int currentVideoIndex = 0;
    private bool allVideosFinished = false;
    private bool isMusicPlayingForCurrentVideo = false;
    private CanvasGroup pressToContinueCanvasGroup;
    private Coroutine flickerCoroutine;

     void Awake()
    {
#if UNITY_WEBGL
    useWebGLVideoPlayer = true;
#endif
    }
    
    void Start()
    {
        // Initialize Press To Continue
        if (pressToContinueObject != null)
        {
            // Get the CanvasGroup from the parent canvas for proper layering
            Transform canvasParent = pressToContinueObject.transform.parent;
            if (canvasParent != null)
            {
                pressToContinueCanvasGroup = canvasParent.GetComponent<CanvasGroup>();
            }
            
            // Fallback to the text object itself if parent doesn't have CanvasGroup
            if (pressToContinueCanvasGroup == null)
            {
                pressToContinueCanvasGroup = pressToContinueObject.GetComponent<CanvasGroup>();
            }
            
            if (pressToContinueCanvasGroup != null)
            {
                pressToContinueCanvasGroup.alpha = 0f; // Start invisible
            }
            pressToContinueObject.SetActive(false); // Hide initially
        }
        
        // Enable only the relevant video root
        if (pcVideoRoot != null) pcVideoRoot.SetActive(!useWebGLVideoPlayer);
        if (webglVideoRoot != null) webglVideoRoot.SetActive(useWebGLVideoPlayer);

        // Validate video lists
        if (useWebGLVideoPlayer && (webglVideoNames == null || webglVideoNames.Count == 0))
        {
            LogError("WebGL video list is empty! Skipping to next scene.");
            LoadNextScene();
            return;
        }
        else if (!useWebGLVideoPlayer && (pcVideoClips == null || pcVideoClips.Count == 0))
        {
            LogError("PC video clips list is empty! Skipping to next scene.");
            LoadNextScene();
            return;
        }

        // Start playing first video
        currentVideoIndex = 0;
        PlayCurrentVideo();
    }
    
    void Update()
    {
        HandleInput();
    }

    void OnDisable()
    {
        // Called BEFORE OnDestroy when scene is unloaded
        // Stop videos immediately to prevent overlap with next scene
        if (pcVideoPlayer != null && pcVideoPlayer.isPlaying)
        {
            pcVideoPlayer.Stop();
        }
        if (webglVideoPlayer != null && webglVideoPlayer.isPlaying)
        {
            webglVideoPlayer.Stop();
        }
        
        // Hide video roots immediately
        if (pcVideoRoot != null)
        {
            pcVideoRoot.SetActive(false);
        }
        if (webglVideoRoot != null)
        {
            webglVideoRoot.SetActive(false);
        }
        
        // Stop Press To Continue animations
        HidePressToContinue();
    }

    void OnDestroy()
    {
        // Stop Press To Continue animations
        HidePressToContinue();
        
        // Stop any playing music
        //StopMusicForCurrentVideo();
        
        // Stop and cleanup PC VideoPlayer
        if (pcVideoPlayer != null)
        {
            if (pcVideoPlayer.isPlaying)
            {
                pcVideoPlayer.Stop();
            }
            pcVideoPlayer.loopPointReached -= OnVideoFinished;
            pcVideoPlayer.errorReceived -= OnVideoError;
            pcVideoPlayer.prepareCompleted -= OnVideoPrepared;
        }
        
        // Stop and cleanup WebGL VideoPlayer
        if (webglVideoPlayer != null)
        {
            if (webglVideoPlayer.isPlaying)
            {
                webglVideoPlayer.Stop();
            }
            webglVideoPlayer.loopPointReached -= OnVideoFinished;
            webglVideoPlayer.errorReceived -= OnVideoError;
            webglVideoPlayer.prepareCompleted -= OnVideoPrepared;
        }
        
        // Hide/disable video roots to prevent visual artifacts
        if (pcVideoRoot != null)
        {
            pcVideoRoot.SetActive(false);
        }
        if (webglVideoRoot != null)
        {
            webglVideoRoot.SetActive(false);
        }
    }

    /// <summary>
    /// Play the current video in the sequence
    /// </summary>
    private void PlayCurrentVideo()
    {
        // Reset per-video state
        videoStarted = false;
        videoFinished = false;
        canSkip = false;

        LogDebug($"Playing video {currentVideoIndex + 1} of {GetTotalVideoCount()}");

        if (useWebGLVideoPlayer)
        {
            InitializeWebGLVideo();
        }
        else
        {
            InitializeVideo();
        }
    }

    /// <summary>
    /// Get total number of videos in the current sequence
    /// </summary>
    private int GetTotalVideoCount()
    {
        return useWebGLVideoPlayer ? webglVideoNames.Count : pcVideoClips.Count;
    }

    /// <summary>
    /// Initialize video player with proper error handling
    /// </summary>
    private void InitializeVideo()
    {
        if (pcVideoPlayer == null)
        {
            LogError("PC VideoPlayer component not assigned!");
            AdvanceToNextVideo();
            return;
        }

        if (currentVideoIndex >= pcVideoClips.Count)
        {
            LogError($"Video index {currentVideoIndex} out of range!");
            AdvanceToNextVideo();
            return;
        }

        VideoClip clip = pcVideoClips[currentVideoIndex];
        if (clip == null)
        {
            LogError($"VideoClip at index {currentVideoIndex} is null!");
            AdvanceToNextVideo();
            return;
        }

        // Set up video events
        pcVideoPlayer.loopPointReached -= OnVideoFinished;
        pcVideoPlayer.errorReceived -= OnVideoError;
        pcVideoPlayer.prepareCompleted -= OnVideoPrepared;
        
        pcVideoPlayer.loopPointReached += OnVideoFinished;
        pcVideoPlayer.errorReceived += OnVideoError;
        pcVideoPlayer.prepareCompleted += OnVideoPrepared;

        // Configure video player
        pcVideoPlayer.source = VideoSource.VideoClip;
        pcVideoPlayer.clip = clip;
        pcVideoPlayer.playOnAwake = false;
        pcVideoPlayer.isLooping = false;
        
        // Prepare and play
        pcVideoPlayer.Prepare();
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
            // Check if we're on the last video
            bool isLastVideo = (currentVideoIndex >= GetTotalVideoCount() - 1);
            
            if (isLastVideo)
            {
                LogDebug("Last video - Loading next scene");
                videoFinished = true;
                LoadNextScene();
            }
            else
            {
                LogDebug("Video skipped by user input");
                SkipVideo();
            }
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
        
        // Play background music if configured for this video
        PlayMusicForCurrentVideo();
        
        // Check if this is the last video
        bool isLastVideo = (currentVideoIndex >= GetTotalVideoCount() - 1);
        if (isLastVideo)
        {
            LogDebug("Last video detected - showing Press To Continue");
            StartCoroutine(ShowPressToContinue());
        }
        
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
        
        LogDebug($"Video {currentVideoIndex + 1} finished naturally");
        videoFinished = true;
        AdvanceToNextVideo();
    }

    /// <summary>
    /// Called when video encounters an error
    /// </summary>
    private void OnVideoError(VideoPlayer vp, string message)
    {
        LogError($"Video {currentVideoIndex + 1} error: {message}");
        videoFinished = true;
        AdvanceToNextVideo();
    }

    /// <summary>
    /// Skip current video manually (called by input or external systems)
    /// </summary>
    public void SkipVideo()
    {
        if (videoFinished) return;
        
        LogDebug($"Video {currentVideoIndex + 1} skipped by user");
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
        
        AdvanceToNextVideo();
    }

    /// <summary>
    /// Advance to the next video in the sequence or transition to next scene
    /// </summary>
    private void AdvanceToNextVideo()
    {
        // Stop current video's music before advancing
        //StopMusicForCurrentVideo();
        
        currentVideoIndex++;

        if (currentVideoIndex >= GetTotalVideoCount())
        {
            // Loop the last video
            currentVideoIndex = GetTotalVideoCount() - 1;
            LogDebug("Last video finished, looping last video");
            PlayCurrentVideo();
        }
        else
        {
            LogDebug($"Advancing to video {currentVideoIndex + 1} of {GetTotalVideoCount()}");
            PlayCurrentVideo();
        }
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
        LogDebug("Initializing WebGL Video Player");
        if (webglVideoPlayer == null)
        {
            LogError("WebGL VideoPlayer component not assigned!");
            AdvanceToNextVideo();
            return;
        }

        if (currentVideoIndex >= webglVideoNames.Count)
        {
            LogError($"Video index {currentVideoIndex} out of range!");
            AdvanceToNextVideo();
            return;
        }

        string videoName = webglVideoNames[currentVideoIndex];
        if (string.IsNullOrEmpty(videoName))
        {
            LogError($"Video name at index {currentVideoIndex} is null or empty!");
            AdvanceToNextVideo();
            return;
        }

        // Set up video events
        webglVideoPlayer.loopPointReached -= OnVideoFinished;
        webglVideoPlayer.errorReceived -= OnVideoError;
        webglVideoPlayer.prepareCompleted -= OnVideoPrepared;
        
        webglVideoPlayer.loopPointReached += OnVideoFinished;
        webglVideoPlayer.errorReceived += OnVideoError;
        webglVideoPlayer.prepareCompleted += OnVideoPrepared;

        // Configure video player
        webglVideoPlayer.playOnAwake = false;
        webglVideoPlayer.isLooping = false;

        // Robust, platform-specific path handling for video URL
#if UNITY_WEBGL
        // For WebGL, Application.streamingAssetsPath is a URL; just append the file name
        string videoPath = $"{Application.streamingAssetsPath}/{videoName}";
#else
        // For file system platforms, use Path.Combine for safety
        string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoName);
#endif
        LogDebug($"[WebGLVideo] Using video URL: {videoPath}");
        webglVideoPlayer.url = videoPath;

        // Prepare and play
        webglVideoPlayer.Prepare();
    }

    /// <summary>
    /// Play background music for the current video if configured
    /// </summary>
    private void PlayMusicForCurrentVideo()
    {
        // Check if we have a music entry for this video index
        if (videoBackgroundMusic == null || currentVideoIndex >= videoBackgroundMusic.Count)
        {
            LogDebug($"No background music configured for video {currentVideoIndex + 1}");
            return;
        }

        SoundData musicData = videoBackgroundMusic[currentVideoIndex];
        
        // Null entry means this video has its own audio
        if (musicData == null)
        {
            LogDebug($"Video {currentVideoIndex + 1} uses its own audio (no background music)");
            return;
        }

        // Play music with crossfade
        if (MusicManager.Instance != null)
        {
            LogDebug($"Playing background music for video {currentVideoIndex + 1}: {musicData.clip.name}");
            MusicManager.Instance.PlayMusic(musicData, musicFadeTime);
            isMusicPlayingForCurrentVideo = true;
        }
        else
        {
            LogError("MusicManager.Instance is null! Cannot play background music.");
        }
    }

    /// <summary>
    /// Stop background music for the current video
    /// </summary>
    private void StopMusicForCurrentVideo()
    {
        if (!isMusicPlayingForCurrentVideo) return;

        if (MusicManager.Instance != null)
        {
            LogDebug($"Stopping background music for video {currentVideoIndex + 1}");
            MusicManager.Instance.StopMusic(musicFadeTime);
            isMusicPlayingForCurrentVideo = false;
        }
    }

    /// <summary>
    /// Load the next scene with optional fade transition
    /// </summary>
    private void LoadNextScene()
    {
        // Ensure music is stopped before scene transition
        //StopMusicForCurrentVideo();
        
        SceneController.Instance
            .NewTransition()
            .Unload(SceneDatabase.Slots.LoadingScreen)
            .Load(SceneDatabase.Slots.MainMenu, SceneDatabase.Scenes.MainMenu, setActive: true) 
            .WithOverlay()
            .Perform();
    }

    /// <summary>
    /// Show Press To Continue with fade-in animation followed by flicker effect
    /// </summary>
    private IEnumerator ShowPressToContinue()
    {
        if (pressToContinueObject == null || pressToContinueCanvasGroup == null)
        {
            LogError("Press To Continue object or CanvasGroup is missing!");
            yield break;
        }

        // Activate the object
        pressToContinueObject.SetActive(true);
        pressToContinueCanvasGroup.alpha = 0f;

        // Fade in from 0 to 1
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            pressToContinueCanvasGroup.alpha = alpha;
            yield return null;
        }

        // Ensure it's fully visible
        pressToContinueCanvasGroup.alpha = 1f;

        // Start flicker animation
        flickerCoroutine = StartCoroutine(FlickerPressToContinue());
    }

    /// <summary>
    /// Gaming-standard flicker/pulse effect for Press To Continue text
    /// </summary>
    private IEnumerator FlickerPressToContinue()
    {
        if (pressToContinueCanvasGroup == null) yield break;

        while (true)
        {
            // Pulse using sine wave for smooth, professional animation
            float time = Time.time * flickerSpeed * Mathf.PI; // Convert to radians
            float alpha = Mathf.Lerp(flickerMinAlpha, flickerMaxAlpha, (Mathf.Sin(time) + 1f) * 0.5f);
            pressToContinueCanvasGroup.alpha = alpha;
            yield return null;
        }
    }

    /// <summary>
    /// Stop and hide Press To Continue
    /// </summary>
    private void HidePressToContinue()
    {
        if (flickerCoroutine != null)
        {
            StopCoroutine(flickerCoroutine);
            flickerCoroutine = null;
        }

        if (pressToContinueObject != null)
        {
            pressToContinueObject.SetActive(false);
        }

        if (pressToContinueCanvasGroup != null)
        {
            pressToContinueCanvasGroup.alpha = 0f;
        }
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
    
    public int CurrentVideoIndex => currentVideoIndex;
    public int TotalVideoCount => GetTotalVideoCount();
    public bool AllVideosFinished => allVideosFinished;
}
