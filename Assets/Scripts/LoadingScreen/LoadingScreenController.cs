using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;
using AudioSystem;

/// <summary>
/// Professional video controller with robust error handling and smooth transitions
/// Perfect for Balangay of the Forgotten splash screens and cutscenes
/// </summary>
public class LoadingScreenController : MonoBehaviour
{
    [Header("Video Settings")]
    [SerializeField] private VideoPlayer videoPlayer;
    
    [Header("Input Settings")]
    [SerializeField] private bool skipOnClick = true;
    [SerializeField] private bool skipOnAnyKey = true;
    [SerializeField] private float minimumPlayTime = 1f; // Prevent accidental immediate skips
    
    [Header("Transition Settings")]
    [SerializeField] private SoundData MenuMusic;
    [SerializeField] private float MusicFadeTime = 2f;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    // Private variables
    private bool videoStarted = false;
    private bool videoFinished = false;
    private bool canSkip = false;
    private float videoStartTime;

    void Start()
    {
        InitializeVideo();
    }
    
    void Update()
    {
        HandleInput();
    }

    /// <summary>
    /// Initialize video player with proper error handling
    /// </summary>
    private void InitializeVideo()
    {
        if (videoPlayer == null)
        {
            LogError("VideoPlayer component not assigned!");
            LoadNextScene();
            return;
        }

        // Set up video events
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.prepareCompleted += OnVideoPrepared;
        
        // Configure video player
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = false;
        
        // Prepare and play
        if (videoPlayer.clip != null || !string.IsNullOrEmpty(videoPlayer.url))
        {
            videoPlayer.Prepare();
        }
        else
        {
            LogError("No video clip or URL assigned to VideoPlayer!");
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
        videoPlayer.Play();
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
        
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            videoPlayer.Stop();
        }
        
        LoadNextScene();
    }

    /// <summary>
    /// Load the next scene with optional fade transition
    /// </summary>
    private void LoadNextScene()
    {
        SceneController.Instance
            .NewTransition()
            .Load(SceneDatabase.Slots.Menu, SceneDatabase.Scenes.MainMenu, setActive: true)
            .Unload(SceneDatabase.Slots.LoadingScreen)
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
    public bool IsVideoPlaying => videoPlayer != null && videoPlayer.isPlaying;
    public bool IsVideoFinished => videoFinished;
    public float VideoProgress => 
        videoPlayer != null && videoPlayer.length > 0 && !double.IsNaN(videoPlayer.length) 
            ? Mathf.Clamp01((float)(videoPlayer.time / videoPlayer.length))
            : 0f;}
