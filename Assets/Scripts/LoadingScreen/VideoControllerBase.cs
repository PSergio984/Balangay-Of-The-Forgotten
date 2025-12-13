using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using AudioSystem;

/// <summary>
/// Base class for video playback controllers with robust error handling and smooth transitions
/// Shared logic for LoadingScreenController, LoreController, and CreditsController
/// </summary>
/// <remarks>
/// <para><strong>Why:</strong> Eliminates code duplication across video controllers</para>
/// <para><strong>How:</strong> Provides all video playback, input handling, and UI management logic</para>
/// <para><strong>Override:</strong> Derived classes can override LoadNextScene() to specify destination</para>
/// </remarks>
public abstract class VideoControllerBase : MonoBehaviour
{
    [Header("Loop Settings")]
    [Tooltip("If true, the last video in the sequence will loop instead of ending. Useful for screens that should not auto-advance.")]
    [SerializeField] protected bool loopLastVideo = false;
    [Header("Playback Mode")]
    [Tooltip("If true, use URL/StreamingAssets for video playback (WebGL/experimental). If false, use native VideoClip (PC/Android). Automatically set at runtime.")]
    protected bool useWebGLVideoPlayer = false;

    [Header("Video Settings")]
    [Tooltip("VideoPlayer for PC/Standalone/Android builds")]
    [SerializeField] protected VideoPlayer pcVideoPlayer;
    [Tooltip("VideoPlayer for WebGL builds")]
    [SerializeField] protected VideoPlayer webglVideoPlayer;
    
    [Header("Video Object Roots")]
    [Tooltip("Root GameObject for all PC video objects (e.g., VideoPlayer, mesh, UI)")]
    [SerializeField] protected GameObject pcVideoRoot;
    [Tooltip("Root GameObject for all WebGL video objects (e.g., VideoPlayer, mesh, UI)")]
    [SerializeField] protected GameObject webglVideoRoot;
    [Tooltip("The UI RawImage GameObject that displays the PC video RenderTexture (separate from pcVideoRoot, like MainMenuVideoController)")]
    [SerializeField] protected GameObject videoDisplayScreen;
    
    [Header("Video Sequence - PC/Standalone")]
    [Tooltip("List of VideoClips to play in sequence for PC/Standalone/Android builds")]
    [SerializeField] protected System.Collections.Generic.List<VideoClip> pcVideoClips = new System.Collections.Generic.List<VideoClip>();
    
    [Header("Video Sequence - WebGL")]
    [Tooltip("List of video filenames in StreamingAssets folder for WebGL builds (e.g., 'Video1.mp4', 'Video2.mp4')")]
    [SerializeField] protected System.Collections.Generic.List<string> webglVideoNames = new System.Collections.Generic.List<string>();
    
    [Header("Per-Video Music Settings")]
    [Tooltip("Optional background music for each video. Leave null for videos with their own audio. Must match video list length.")]
    [SerializeField] protected System.Collections.Generic.List<SoundData> videoBackgroundMusic = new System.Collections.Generic.List<SoundData>();
    [Tooltip("Fade time for music transitions between videos (seconds)")]
    [SerializeField] protected float musicFadeTime = 1.5f;
    
    [Header("Input Settings")]
    [SerializeField] protected bool skipOnClick = true;
    [SerializeField] protected bool skipOnAnyKey = true;
    [SerializeField] protected float minimumPlayTime = 1f; // Prevent accidental immediate skips
    
    [Header("Press To Continue Settings")]
    [Tooltip("GameObject that displays 'Press To Continue' text")]
    [SerializeField] protected GameObject pressToContinueObject;
    [Tooltip("Fade-in duration for Press To Continue text (seconds)")]
    [SerializeField] protected float fadeInDuration = 1.5f;
    [Tooltip("Flicker speed for Press To Continue animation (pulses per second)")]
    [SerializeField] protected float flickerSpeed = 2f;
    [Tooltip("Minimum alpha during flicker (0-1)")]
    [SerializeField] protected float flickerMinAlpha = 0.3f;
    [Tooltip("Maximum alpha during flicker (0-1)")]
    [SerializeField] protected float flickerMaxAlpha = 1f;
    
    [Header("Debug")]
    [SerializeField] protected bool showDebugLogs = true;

    // Protected variables (accessible to derived classes)
    protected bool videoStarted = false;
    protected bool videoFinished = false;
    protected bool canSkip = false;
    protected float videoStartTime;
    protected int currentVideoIndex = 0;
    protected bool allVideosFinished = false;
    protected bool isMusicPlayingForCurrentVideo = false;
    protected CanvasGroup pressToContinueCanvasGroup;
    protected Coroutine flickerCoroutine;
    
    // Cached RenderTexture for clearing
    protected RenderTexture webglRenderTexture;
    
    // Cached display components for PC video
    protected UnityEngine.UI.RawImage pcVideoRawImage;
    protected Renderer pcVideoMeshRenderer;

    protected virtual void Awake()
    {
#if UNITY_WEBGL
        useWebGLVideoPlayer = true;
#endif
        // CRITICAL: Hide all video roots immediately in Awake to prevent overlap
        // This runs before ANY rendering happens, ensuring no stale video frames appear
        if (pcVideoRoot != null) pcVideoRoot.SetActive(false);
        if (webglVideoRoot != null) webglVideoRoot.SetActive(false);
        // Also hide the video display screen (RawImage) to prevent showing stale RenderTexture content
        if (videoDisplayScreen != null) videoDisplayScreen.SetActive(false);
        
        // Fix video flipping issue for WebGL: Use multiple methods to ensure video displays correctly
        // Method 1: RawImage UVRect flip (most reliable for UI-based video display)
        // Method 2: Material texture scale flip (for 3D mesh display)
        // Method 3: Transform scale (backup method)
        if (useWebGLVideoPlayer && webglVideoRoot != null)
        {
            // METHOD 1: Fix RawImage UVRect to flip horizontally
            // This is the most reliable method for UI-based video display
            UnityEngine.UI.RawImage rawImage = webglVideoRoot.GetComponentInChildren<UnityEngine.UI.RawImage>(true);
            if (rawImage != null)
            {
                Rect currentUVRect = rawImage.uvRect;
                // Flip horizontally: width = -1, x = 1 (mirrors the texture)
                if (currentUVRect.width > 0) // Only fix if not already flipped
                {
                    rawImage.uvRect = new Rect(1f, currentUVRect.y, -1f, currentUVRect.height);
                    LogDebug($"Fixed WebGL RawImage UVRect to flip horizontally: {rawImage.uvRect}");
                }
                else
                {
                    LogDebug($"WebGL RawImage UVRect already flipped: {currentUVRect}");
                }
            }
            
            // METHOD 2: Fix Material texture scale for 3D mesh renderers
            Renderer[] renderers = webglVideoRoot.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                if (renderer.GetComponent<VideoPlayer>() != null) continue; // Skip VideoPlayer's own renderer
                
                Material mat = renderer.material;
                if (mat != null && mat.HasProperty("_MainTex"))
                {
                    Vector2 currentScale = mat.GetTextureScale("_MainTex");
                    // Flip horizontally: x scale = -1, x offset = 1
                    if (currentScale.x > 0) // Only fix if not already flipped
                    {
                        mat.SetTextureScale("_MainTex", new Vector2(-1f, currentScale.y));
                        mat.SetTextureOffset("_MainTex", new Vector2(1f, mat.GetTextureOffset("_MainTex").y));
                        LogDebug($"Fixed WebGL Material '{mat.name}' texture scale to flip horizontally on '{renderer.gameObject.name}'");
                    }
                }
            }
            
            // METHOD 3: Fix Transform scale (backup method for 3D meshes)
            // Fix VideoPlayer GameObject
            if (webglVideoPlayer != null)
            {
                Transform videoPlayerTransform = webglVideoPlayer.transform;
                Vector3 currentScale = videoPlayerTransform.localScale;
                if (Mathf.Abs(currentScale.z - (-1f)) > 0.01f)
                {
                    videoPlayerTransform.localScale = new Vector3(currentScale.x, currentScale.y, -1f);
                    LogDebug($"Fixed WebGL VideoPlayer GameObject Z scale from {currentScale.z} to -1");
                }
            }
            
            // Fix VideoCube mesh - THE KEY FIX: X scale must be NEGATIVE to flip horizontally!
            // LoadingScreen scene has VideoCube X scale = -6.399383 (negative = correct, no flip)
            // Credits/Lore scenes have VideoCube X scale = positive (causes horizontal flip)
            Transform videoCube = webglVideoRoot.transform.Find("VideoCube");
            if (videoCube == null)
            {
                foreach (Transform child in webglVideoRoot.transform)
                {
                    if (child.name.Contains("VideoCube") || child.name.Contains("Cube") || child.name.Contains("Mesh"))
                    {
                        videoCube = child;
                        break;
                    }
                }
            }
            
            if (videoCube != null)
            {
                Vector3 cubeScale = videoCube.localScale;
                LogDebug($"WebGL VideoCube '{videoCube.name}' current scale: {cubeScale}");
                
                // CRITICAL FIX: X scale must be NEGATIVE to prevent horizontal flipping
                // This matches the LoadingScreen scene which works correctly
                Vector3 fixedScale = cubeScale;
                bool needsFix = false;
                
                if (cubeScale.x > 0) // X scale is positive (causes flip)
                {
                    fixedScale.x = -Mathf.Abs(cubeScale.x); // Make it negative
                    needsFix = true;
                    LogDebug($"Fixing VideoCube X scale from {cubeScale.x} to {fixedScale.x} (negative = no flip)");
                }
                
                // Also ensure Z scale is -1 (for depth)
                if (Mathf.Abs(cubeScale.z - (-1f)) > 0.01f)
                {
                    fixedScale.z = -1f;
                    needsFix = true;
                    LogDebug($"Fixing VideoCube Z scale from {cubeScale.z} to -1");
                }
                
                if (needsFix)
                {
                    videoCube.localScale = fixedScale;
                    LogDebug($"Fixed WebGL VideoCube '{videoCube.name}' scale to: {fixedScale}");
                }
                else
                {
                    LogDebug($"WebGL VideoCube scale already correct: {cubeScale}");
                }
            }
        }
        
        // Cache and clear the WebGL RenderTexture to prevent showing previous scene's video
        if (webglVideoPlayer != null && webglVideoPlayer.targetTexture != null)
        {
            webglRenderTexture = webglVideoPlayer.targetTexture;
            ClearRenderTexture(webglRenderTexture);
        }
        
        // Find and cache PC video display components
        if (!useWebGLVideoPlayer)
        {
            // First, try to find RawImage in the separate videoDisplayScreen GameObject (like MainMenuVideoController)
            if (videoDisplayScreen != null)
            {
                pcVideoRawImage = videoDisplayScreen.GetComponent<UnityEngine.UI.RawImage>();
                if (pcVideoRawImage == null)
                {
                    pcVideoRawImage = videoDisplayScreen.GetComponentInChildren<UnityEngine.UI.RawImage>(true);
                }
                if (pcVideoRawImage != null)
                {
                    LogDebug($"Found PC video RawImage in videoDisplayScreen: {pcVideoRawImage.gameObject.name}");
                }
            }
            
            // Fallback: Find RawImage in pcVideoRoot if videoDisplayScreen doesn't have one
            if (pcVideoRawImage == null && pcVideoRoot != null)
            {
                pcVideoRawImage = pcVideoRoot.GetComponentInChildren<UnityEngine.UI.RawImage>(true);
                if (pcVideoRawImage != null)
                {
                    LogDebug($"Found PC video RawImage in pcVideoRoot: {pcVideoRawImage.gameObject.name}");
                }
            }
            
            // Find MeshRenderer for 3D mesh video display (like VideoCube) - only if no RawImage found
            if (pcVideoRawImage == null && pcVideoRoot != null)
            {
                Renderer[] renderers = pcVideoRoot.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer renderer in renderers)
                {
                    if (renderer.GetComponent<VideoPlayer>() == null) // Skip VideoPlayer's own renderer
                    {
                        pcVideoMeshRenderer = renderer;
                        LogDebug($"Found PC video MeshRenderer: {renderer.gameObject.name}");
                        break;
                    }
                }
            }
        }
    }
    
    protected virtual void Start()
    {
        // Initialize Press To Continue (only if configured - specific to LoadingScreenController)
        InitializePressToContinue();
        
        // CRITICAL: Reset and configure video players BEFORE activating roots
        // This prevents VideoPlayer from trying to play with wrong settings
        if (!useWebGLVideoPlayer && pcVideoPlayer != null)
        {
            // Stop any playback that might have started
            if (pcVideoPlayer.isPlaying)
            {
                pcVideoPlayer.Stop();
            }
            // Clear any existing clip assignment (prevents "Missing" clip issues)
            pcVideoPlayer.clip = null;
            // Ensure playOnAwake is false (set early to prevent auto-play)
            pcVideoPlayer.playOnAwake = false;
            // Reset source to VideoClip (in case prefab had it set differently)
            pcVideoPlayer.source = VideoSource.VideoClip;
            
            // CRITICAL: Preserve the RenderTexture from prefab - don't clear it!
            // The RenderTexture must be set in the prefab for video to be visible
            if (pcVideoPlayer.targetTexture == null)
            {
                LogError("PC VideoPlayer targetTexture is null! Video will not be visible. Ensure RenderTexture is assigned in prefab.");
            }
            else
            {
                LogDebug($"PC VideoPlayer RenderTexture preserved: {pcVideoPlayer.targetTexture.name}");
            }
        }
        else if (useWebGLVideoPlayer && webglVideoPlayer != null)
        {
            // Stop any playback that might have started
            if (webglVideoPlayer.isPlaying)
            {
                webglVideoPlayer.Stop();
            }
            // Clear any existing URL (prevents conflicts)
            webglVideoPlayer.url = "";
            // Ensure playOnAwake is false
            webglVideoPlayer.playOnAwake = false;
            // Reset source to Url
            webglVideoPlayer.source = VideoSource.Url;
        }
        
        // Enable only the relevant video root (AFTER configuring the player)
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
    
    protected virtual void Update()
    {
        HandleInput();
    }

    protected virtual void OnDisable()
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
        
        // CRITICAL: Clear the RenderTexture to prevent showing stale video frames in next scene
        if (webglRenderTexture != null)
        {
            ClearRenderTexture(webglRenderTexture);
        }
        else if (webglVideoPlayer != null && webglVideoPlayer.targetTexture != null)
        {
            ClearRenderTexture(webglVideoPlayer.targetTexture);
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
        // Also hide the video display screen (RawImage) to prevent showing stale RenderTexture content
        if (videoDisplayScreen != null)
        {
            videoDisplayScreen.SetActive(false);
        }
        
        // Stop Press To Continue animations
        HidePressToContinue();
    }

    protected virtual void OnDestroy()
    {
        // Stop Press To Continue animations
        HidePressToContinue();
        
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
        // Also hide the video display screen
        if (videoDisplayScreen != null)
        {
            videoDisplayScreen.SetActive(false);
        }
    }

    /// <summary>
    /// Play the current video in the sequence
    /// </summary>
    protected virtual void PlayCurrentVideo()
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
    protected virtual int GetTotalVideoCount()
    {
        return useWebGLVideoPlayer ? webglVideoNames.Count : pcVideoClips.Count;
    }

    /// <summary>
    /// Initialize video player with proper error handling
    /// </summary>
    protected virtual void InitializeVideo()
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

        // CRITICAL: Stop any existing playback and clear state before reconfiguring
        if (pcVideoPlayer.isPlaying)
        {
            pcVideoPlayer.Stop();
        }
        
        // Unsubscribe from events first to prevent duplicate subscriptions
        pcVideoPlayer.loopPointReached -= OnVideoFinished;
        pcVideoPlayer.errorReceived -= OnVideoError;
        pcVideoPlayer.prepareCompleted -= OnVideoPrepared;
        
        // Configure video player settings BEFORE assigning clip
        pcVideoPlayer.source = VideoSource.VideoClip;
        pcVideoPlayer.playOnAwake = false;
        pcVideoPlayer.isLooping = false;
        
        // CRITICAL: Ensure RenderTexture is set and connected to display
        // The prefab should have a RenderTexture assigned, but we verify it here
        if (pcVideoPlayer.targetTexture == null)
        {
            LogError("PC VideoPlayer targetTexture is null! Video will not be visible. Check prefab configuration.");
        }
        else
        {
            LogDebug($"PC VideoPlayer using RenderTexture: {pcVideoPlayer.targetTexture.name}");
            
            // Connect RenderTexture to RawImage display (UI-based)
            // Note: We connect it here, but activate the display screen in OnVideoPrepared (like MainMenuVideoController)
            if (pcVideoRawImage != null)
            {
                pcVideoRawImage.texture = pcVideoPlayer.targetTexture;
                LogDebug($"Connected RenderTexture to PC RawImage: {pcVideoRawImage.gameObject.name}");
            }
            // Connect RenderTexture to MeshRenderer display (3D mesh)
            else if (pcVideoMeshRenderer != null)
            {
                Material mat = pcVideoMeshRenderer.material;
                if (mat != null)
                {
                    mat.mainTexture = pcVideoPlayer.targetTexture;
                    LogDebug($"Connected RenderTexture to PC MeshRenderer: {pcVideoMeshRenderer.gameObject.name}");
                }
            }
            else
            {
                LogError("No display component found for PC video! Video will play but not be visible. Assign videoDisplayScreen GameObject with RawImage component (like MainMenuVideoController).");
            }
        }
        
        // Now assign the clip
        pcVideoPlayer.clip = clip;
        
        // Subscribe to events AFTER configuration
        pcVideoPlayer.loopPointReached += OnVideoFinished;
        pcVideoPlayer.errorReceived += OnVideoError;
        pcVideoPlayer.prepareCompleted += OnVideoPrepared;
        
        // Prepare and play
        pcVideoPlayer.Prepare();
    }

    /// <summary>
    /// Handle all input for skipping video
    /// </summary>
    protected virtual void HandleInput()
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
    protected virtual void OnVideoPrepared(VideoPlayer vp)
    {
        LogDebug("Video prepared, starting playback");
        
        // Show the video display screen now that video is ready to play
        // This prevents showing stale RenderTexture content from previous scenes (like MainMenuVideoController)
        if (!useWebGLVideoPlayer && videoDisplayScreen != null)
        {
            videoDisplayScreen.SetActive(true);
            LogDebug("Activated videoDisplayScreen for PC video");
        }
        
        // Validate that the VideoPlayer is still valid and matches our expected player
        if (useWebGLVideoPlayer)
        {
            if (webglVideoPlayer != null && webglVideoPlayer == vp)
            {
                webglVideoPlayer.Play();
            }
            else
            {
                LogError("WebGL VideoPlayer mismatch or null in OnVideoPrepared!");
                return;
            }
        }
        else
        {
            if (pcVideoPlayer != null && pcVideoPlayer == vp)
            {
                pcVideoPlayer.Play();
            }
            else
            {
                LogError("PC VideoPlayer mismatch or null in OnVideoPrepared!");
                return;
            }
        }
        
        videoStarted = true;
        videoStartTime = Time.time;
        
        // Play background music if configured for this video
        PlayMusicForCurrentVideo();
        
        // Check if this is the last video
        bool isLastVideo = (currentVideoIndex >= GetTotalVideoCount() - 1);
        if (isLastVideo)
        {
            if (loopLastVideo && vp != null)
            {
                vp.isLooping = true;
                LogDebug("Last video detected - looping enabled");
            }
            else
            {
                if (vp != null) vp.isLooping = false;
                LogDebug("Last video detected - showing Press To Continue");
                StartCoroutine(ShowPressToContinue());
            }
        }
        
        // Allow skipping after minimum time
        StartCoroutine(EnableSkipAfterDelay());
    }

    /// <summary>
    /// Enable skipping after minimum play time to prevent accidental skips
    /// </summary>
    protected virtual IEnumerator EnableSkipAfterDelay()
    {
        yield return new WaitForSeconds(minimumPlayTime);
        canSkip = true;
        LogDebug("Video skipping now enabled");
    }

    /// <summary>
    /// Called when video reaches the end naturally
    /// </summary>
    protected virtual void OnVideoFinished(VideoPlayer vp)
    {
        if (videoFinished) return; // Prevent double-calling
        
        LogDebug($"Video {currentVideoIndex + 1} finished naturally");
        videoFinished = true;
        AdvanceToNextVideo();
    }

    /// <summary>
    /// Called when video encounters an error
    /// </summary>
    protected virtual void OnVideoError(VideoPlayer vp, string message)
    {
        LogError($"Video {currentVideoIndex + 1} error: {message}");
        videoFinished = true;
        AdvanceToNextVideo();
    }

    /// <summary>
    /// Skip current video manually (called by input or external systems)
    /// </summary>
    public virtual void SkipVideo()
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
    protected virtual void AdvanceToNextVideo()
    {
        currentVideoIndex++;

        if (currentVideoIndex >= GetTotalVideoCount())
        {
            // All videos finished - check if we should loop or transition
            if (loopLastVideo)
            {
                // Loop the last video
                currentVideoIndex = GetTotalVideoCount() - 1;
                LogDebug("Last video finished, looping last video");
                PlayCurrentVideo();
            }
            else
            {
                // All videos finished - transition to next scene
                LogDebug("All videos finished - transitioning to next scene");
                allVideosFinished = true;
                LoadNextScene();
            }
        }
        else
        {
            LogDebug($"Advancing to video {currentVideoIndex + 1} of {GetTotalVideoCount()}");
            PlayCurrentVideo();
        }
    }

    /// <summary>
    /// Initialize URL/StreamingAssets-based video playback (WebGL/experimental)
    /// </summary>
    protected virtual void InitializeWebGLVideo()
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

        // CRITICAL: Stop any existing playback and clear state before reconfiguring
        if (webglVideoPlayer.isPlaying)
        {
            webglVideoPlayer.Stop();
        }
        
        // Unsubscribe from events first to prevent duplicate subscriptions
        webglVideoPlayer.loopPointReached -= OnVideoFinished;
        webglVideoPlayer.errorReceived -= OnVideoError;
        webglVideoPlayer.prepareCompleted -= OnVideoPrepared;

        // Configure video player settings BEFORE setting URL
        webglVideoPlayer.source = VideoSource.Url;
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

        // Subscribe to events AFTER configuration
        webglVideoPlayer.loopPointReached += OnVideoFinished;
        webglVideoPlayer.errorReceived += OnVideoError;
        webglVideoPlayer.prepareCompleted += OnVideoPrepared;

        // Prepare and play
        webglVideoPlayer.Prepare();
    }

    /// <summary>
    /// Play background music for the current video if configured
    /// </summary>
    protected virtual void PlayMusicForCurrentVideo()
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
    protected virtual void StopMusicForCurrentVideo()
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
    /// Load the next scene - override this in derived classes to specify destination
    /// </summary>
    protected abstract void LoadNextScene();

    /// <summary>
    /// Initialize Press To Continue UI (optional - only used by LoadingScreenController)
    /// </summary>
    protected virtual void InitializePressToContinue()
    {
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
    }

    /// <summary>
    /// Show Press To Continue with fade-in animation followed by flicker effect
    /// Override in derived classes if Press To Continue is not needed
    /// </summary>
    protected virtual IEnumerator ShowPressToContinue()
    {
        if (pressToContinueObject == null || pressToContinueCanvasGroup == null)
        {
            // Not an error - Press To Continue is optional
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
    protected virtual IEnumerator FlickerPressToContinue()
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
    protected virtual void HidePressToContinue()
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

    /// <summary>
    /// Clears a RenderTexture to black to prevent showing stale video frames.
    /// This is critical for preventing video overlap during scene transitions.
    /// </summary>
    /// <param name="rt">The RenderTexture to clear</param>
    protected virtual void ClearRenderTexture(RenderTexture rt)
    {
        if (rt == null) return;
        
        // Store the current active RenderTexture
        RenderTexture prev = RenderTexture.active;
        
        // Set the target RenderTexture as active and clear it to black
        RenderTexture.active = rt;
        GL.Clear(true, true, Color.black);
        
        // Restore the previous active RenderTexture
        RenderTexture.active = prev;
        
        LogDebug("RenderTexture cleared to black");
    }

    // Debug logging methods
    protected virtual void LogDebug(string message)
    {
        if (showDebugLogs)
            Debug.Log($"[{GetType().Name}] {message}");
    }

    protected virtual void LogError(string message)
    {
        Debug.LogError($"[{GetType().Name}] {message}");
    }

    // Public properties for external control
    public virtual bool IsVideoPlaying
    {
        get
        {
            return useWebGLVideoPlayer
                ? webglVideoPlayer != null && webglVideoPlayer.isPlaying
                : pcVideoPlayer != null && pcVideoPlayer.isPlaying;
        }
    }
    
    public virtual bool IsVideoFinished => videoFinished;
    
    public virtual float VideoProgress
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
    
    public virtual int CurrentVideoIndex => currentVideoIndex;
    public virtual int TotalVideoCount => GetTotalVideoCount();
    public virtual bool AllVideosFinished => allVideosFinished;
}

