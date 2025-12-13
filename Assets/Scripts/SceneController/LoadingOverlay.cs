using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// Loading overlay with optional video playback during scene transitions.
/// Supports map-specific loading videos (WebGL and PC) with fade transitions.
/// 
/// IMPORTANT: This creates its OWN RenderTexture at runtime to avoid conflicts
/// with scene video controllers (MainMenuVideoController, LoadingScreenController, etc.)
/// that share a different RenderTexture.
/// </summary>
public class LoadingOverlay : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image overlayImage;  // The Image component that shows the overlay color
    [SerializeField] private float fadeInTime = 0.5f;
    [SerializeField] private float fadeOutTime = 0.8f;  // Slightly longer for smoother video-to-scene transition
    
    [Header("Canvas Settings")]
    [Tooltip("Canvas component - will be auto-found if not assigned")]
    [SerializeField] private Canvas canvas;
    [Tooltip("Sorting order for the overlay canvas (higher = on top of everything)")]
    [SerializeField] private int overlaySortOrder = 9999;
    
    [Header("Video Settings")]
    [Tooltip("Root GameObject containing the WebGL video player setup")]
    [SerializeField] private GameObject webglVideoRoot;
    [Tooltip("Root GameObject containing the PC video player setup")]
    [SerializeField] private GameObject pcVideoRoot;
    [Tooltip("VideoPlayer for WebGL builds")]
    [SerializeField] private VideoPlayer webglVideoPlayer;
    [Tooltip("VideoPlayer for PC/Standalone builds")]
    [SerializeField] private VideoPlayer pcVideoPlayer;
    [Tooltip("RawImage that displays the video (optional - used for UI-based video display)")]
    [SerializeField] private RawImage videoDisplay;
    [Tooltip("MeshRenderer that displays the video (optional - used for 3D mesh video display like VideoCube)")]
    [SerializeField] private Renderer videoMeshRenderer;
    [Tooltip("Default loading video filename in StreamingAssets (e.g., 'loading.mp4')")]
    [SerializeField] private string defaultLoadingVideo = "loading.mp4";
    [Tooltip("Enable video playback during loading transitions")]
    [SerializeField] private bool useVideoLoading = true;
    
    [Header("RenderTexture Settings")]
    [Tooltip("Width of the private RenderTexture for loading videos")]
    [SerializeField] private int renderTextureWidth = 1920;
    [Tooltip("Height of the private RenderTexture for loading videos")]
    [SerializeField] private int renderTextureHeight = 1080;
    
    [Header("UI Video Display (Auto-Created)")]
    [Tooltip("If true, creates a fullscreen RawImage for video display (recommended for Screen Space - Overlay canvas)")]
    [SerializeField] private bool useUIVideoDisplay = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    // Platform detection
    private bool useWebGL = false;
    
    // PRIVATE RenderTexture - created at runtime, NOT shared with scene video controllers
    private RenderTexture privateRenderTexture;
    
    // Dynamically created UI video display (for Screen Space - Overlay canvas)
    private GameObject uiVideoDisplayObject;
    private RawImage uiVideoRawImage;
    
    // Current video state
    private bool isVideoPlaying = false;
    private bool videoFinished = false;

    private void Awake()
    {
#if UNITY_WEBGL
        useWebGL = true;
#endif
        // Find canvas and ensure it's on top of everything
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = GetComponent<Canvas>();
            }
        }
        
        // Set high sorting order to ensure overlay is ALWAYS on top of scene content
        if (canvas != null)
        {
            canvas.sortingOrder = overlaySortOrder;
            LogDebug($"Canvas sorting order set to {overlaySortOrder}");
        }
        else
        {
            LogDebug("WARNING: No Canvas found - overlay may not display correctly!");
        }
        
        // Auto-find Image component if not assigned
        if (overlayImage == null)
        {
            overlayImage = GetComponentInChildren<Image>();
            if (overlayImage == null && canvasGroup != null)
            {
                overlayImage = canvasGroup.GetComponent<Image>();
            }
            if (overlayImage != null)
            {
                LogDebug($"Auto-found overlay Image: {overlayImage.gameObject.name}");
            }
        }
        
        // Start with overlay invisible
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        
        // Ensure overlay starts black
        if (overlayImage != null)
        {
            overlayImage.color = Color.black;
        }
        
        // Hide video elements initially
        if (webglVideoRoot != null) webglVideoRoot.SetActive(false);
        if (pcVideoRoot != null) pcVideoRoot.SetActive(false);
        
        // Auto-find VideoPlayer components from roots if not assigned
        if (webglVideoPlayer == null && webglVideoRoot != null)
        {
            webglVideoPlayer = webglVideoRoot.GetComponentInChildren<VideoPlayer>(true);
        }
        if (pcVideoPlayer == null && pcVideoRoot != null)
        {
            pcVideoPlayer = pcVideoRoot.GetComponentInChildren<VideoPlayer>(true);
        }
        
        // Auto-find RawImage for video display if not assigned
        if (videoDisplay == null)
        {
            // Try to find RawImage in video roots
            if (webglVideoRoot != null)
            {
                videoDisplay = webglVideoRoot.GetComponentInChildren<RawImage>(true);
            }
            if (videoDisplay == null && pcVideoRoot != null)
            {
                videoDisplay = pcVideoRoot.GetComponentInChildren<RawImage>(true);
            }
        }
        
        // Auto-find MeshRenderer for 3D video display (like VideoCube) if not assigned
        if (videoMeshRenderer == null)
        {
            // Try to find MeshRenderer in video roots (skip the VideoPlayer's own renderer if any)
            if (webglVideoRoot != null)
            {
                // Find all renderers and pick the one that's not on the VideoPlayer object
                Renderer[] renderers = webglVideoRoot.GetComponentsInChildren<Renderer>(true);
                LogDebug($"Found {renderers.Length} renderers in WEBGL video root");
                foreach (var r in renderers)
                {
                    if (r.GetComponent<VideoPlayer>() == null)
                    {
                        videoMeshRenderer = r;
                        LogDebug($"Auto-found video mesh renderer: {r.gameObject.name}");
                        break;
                    }
                }
                
                if (videoMeshRenderer == null)
                {
                    LogDebug("WARNING: No suitable mesh renderer found in WEBGL video root!");
                }
            }
        }
        
        // Create UI-based video display for Screen Space - Overlay canvas
        // This is more reliable than 3D mesh which doesn't render properly in overlay mode
        if (useUIVideoDisplay)
        {
            CreateUIVideoDisplay();
        }
        
        // Create our own PRIVATE RenderTexture - this is key to avoiding conflicts!
        CreatePrivateRenderTexture();
    }
    
    /// <summary>
    /// Creates a fullscreen RawImage for video display.
    /// This works properly with Screen Space - Overlay canvas, unlike 3D meshes.
    /// </summary>
    private void CreateUIVideoDisplay()
    {
        // Check if we already have one
        if (uiVideoDisplayObject != null)
        {
            return;
        }
        
        // Create a new GameObject with RawImage
        uiVideoDisplayObject = new GameObject("UIVideoDisplay");
        
        // Parent to the CanvasGroup child (if available) so the video is affected by alpha fade
        // The canvasGroup reference points to a CanvasGroup component on a child GameObject
        // We need to parent our UIVideoDisplay to that same GameObject so it fades with everything else
        Transform parentTransform = transform;
        if (canvasGroup != null)
        {
            // Parent to the same object that has the CanvasGroup component
            parentTransform = canvasGroup.transform;
            LogDebug($"Parenting UIVideoDisplay to CanvasGroup container: {canvasGroup.gameObject.name}");
        }
        
        uiVideoDisplayObject.transform.SetParent(parentTransform, false);
        
        // Make it the first sibling so it renders behind other UI elements (like loading text)
        // but still shows the video as the background
        uiVideoDisplayObject.transform.SetAsFirstSibling();
        
        // Add RectTransform and stretch to fill the overlay
        RectTransform rt = uiVideoDisplayObject.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
        
        // Add RawImage for video display
        uiVideoRawImage = uiVideoDisplayObject.AddComponent<RawImage>();
        uiVideoRawImage.color = Color.white;
        
        // Set UI layer
        uiVideoDisplayObject.layer = 5; // UI layer
        
        // Start hidden
        uiVideoDisplayObject.SetActive(false);
        
        // Use this as the primary video display
        videoDisplay = uiVideoRawImage;
        
        LogDebug("Created UI video display (RawImage) for fullscreen video overlay");
    }
    
    /// <summary>
    /// Creates a private RenderTexture for the loading overlay video.
    /// This is separate from scene video controllers to avoid conflicts.
    /// </summary>
    private void CreatePrivateRenderTexture()
    {
        // Release any existing private texture
        if (privateRenderTexture != null)
        {
            privateRenderTexture.Release();
            Destroy(privateRenderTexture);
        }
        
        // Create new RenderTexture at runtime
        privateRenderTexture = new RenderTexture(renderTextureWidth, renderTextureHeight, 0, RenderTextureFormat.ARGB32);
        privateRenderTexture.name = "LoadingOverlay_PrivateRT";
        privateRenderTexture.Create();
        
        // Clear it to black
        ClearRenderTexture(privateRenderTexture);
        
        // Assign to video players
        if (webglVideoPlayer != null)
        {
            webglVideoPlayer.targetTexture = privateRenderTexture;
        }
        if (pcVideoPlayer != null)
        {
            pcVideoPlayer.targetTexture = privateRenderTexture;
        }
        
        // Assign to RawImage display (UI-based)
        if (videoDisplay != null)
        {
            videoDisplay.texture = privateRenderTexture;
        }
        
        // Assign to MeshRenderer display (3D mesh like VideoCube)
        if (videoMeshRenderer != null)
        {
            // Use sharedMaterial to avoid creating material instances during edit mode
            // But use material at runtime to avoid modifying the shared asset
            Material mat = Application.isPlaying ? videoMeshRenderer.material : videoMeshRenderer.sharedMaterial;
            if (mat != null)
            {
                mat.mainTexture = privateRenderTexture;
                LogDebug($"Assigned private RenderTexture to mesh renderer material: {videoMeshRenderer.gameObject.name}");
            }
        }
        
        LogDebug($"Created private RenderTexture: {renderTextureWidth}x{renderTextureHeight}");
    }

    /// <summary>
    /// Fades in to black/loading screen (hides game content)
    /// </summary>
    public IEnumerator FadeInBlack()
    {
        LogDebug("Fading in to black...");
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
        yield return FadeTo(1f, fadeInTime);
        LogDebug("Fade in complete");
    }

    /// <summary>
    /// Fades out from black/loading screen (reveals game content)
    /// </summary>
    public IEnumerator FadeOutBlack()
    {
        LogDebug("Fading out from black...");
        // Stop any playing video before fading out
        StopVideo();
        yield return FadeTo(0f, fadeOutTime);
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        LogDebug("Fade out complete");
    }
    
    /// <summary>
    /// Fades in to white (for credits transition)
    /// </summary>
    public IEnumerator FadeInWhite()
    {
        LogDebug("Fading in to white...");
        if (overlayImage != null)
        {
            overlayImage.color = Color.white;
        }
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
        yield return FadeTo(1f, fadeInTime);
        LogDebug("Fade in to white complete");
    }
    
    /// <summary>
    /// Fades out from white to black (for credits transition)
    /// First fades out white, then changes to black and fades in
    /// </summary>
    public IEnumerator FadeOutWhiteToBlack()
    {
        LogDebug("Fading out from white to black...");
        
        // First fade out white
        yield return FadeTo(0f, fadeOutTime);
        
        // Change color to black while invisible
        if (overlayImage != null)
        {
            overlayImage.color = Color.black;
        }
        
        // Now fade in black
        yield return FadeTo(1f, fadeInTime);
        
        LogDebug("Fade out white to black complete");
    }
    
    /// <summary>
    /// Fades in with video playback. Uses map-specific video if mapId is provided.
    /// Video filename format: "{mapId}.mp4" (e.g., "loadingApolaki.mp4", "loadingMayari.mp4")
    /// Falls back to default loading video if map-specific video not found.
    /// The video starts playing first, then fades in smoothly for a polished transition.
    /// </summary>
    /// <param name="mapId">Optional video filename (without .mp4 extension) for map-specific loading video</param>
    public IEnumerator FadeInWithVideo(string mapId = null)
    {
        if (!useVideoLoading)
        {
            yield return FadeInBlack();
            yield break;
        }
        
        // Determine video filename
        string videoName = defaultLoadingVideo;
        if (!string.IsNullOrEmpty(mapId))
        {
            // Use the provided video name directly (e.g., "loadingMayari" -> "loadingMayari.mp4")
            videoName = $"{mapId}.mp4";
            LogDebug($"Attempting to use map-specific loading video: {videoName}");
        }
        
        // Prepare for fade in - start invisible
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
        
        // Start video playback FIRST (while still invisible)
        // This allows the video to start and we fade in on top of it
        yield return StartVideo(videoName);
        
        // Wait a frame for video to start rendering
        yield return null;
        
        // Now smoothly fade in the video overlay
        LogDebug("Video started, now fading in...");
        yield return FadeTo(1f, fadeInTime);
        LogDebug("Fade in with video complete");
    }
    
    /// <summary>
    /// Waits for the loading video to finish playing.
    /// Call this BEFORE loading scenes to ensure the video plays completely
    /// while the old scene is still visible, then load the new scene.
    /// </summary>
    /// <param name="earlyFinishBuffer">Seconds before video ends to return (allows scene loading to start slightly early)</param>
    public IEnumerator WaitForVideoToFinish(float earlyFinishBuffer = 0.5f)
    {
        if (!useVideoLoading || !isVideoPlaying)
        {
            LogDebug("No video playing, skipping wait");
            yield break;
        }
        
        float maxWaitTime = 60f; // Safety timeout
        float elapsed = 0f;
        
        LogDebug("Waiting for loading video to finish before scene load...");
        
        // Wait for video to finish (videoFinished flag set by loopPointReached)
        while (isVideoPlaying && !videoFinished && elapsed < maxWaitTime)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (elapsed >= maxWaitTime)
        {
            LogDebug("Video wait timeout reached");
        }
        else
        {
            LogDebug($"Video finished after {elapsed:F1} seconds, proceeding with scene load");
        }
    }
    
    /// <summary>
    /// Waits for video to finish then fades out smoothly.
    /// The video will play once (not looped) and transition happens after it ends.
    /// Includes a smooth fade transition to prevent abrupt cuts.
    /// NOTE: If using WaitForVideoToFinish() before scene loading, the video 
    /// should already be finished when this is called - fade out immediately.
    /// </summary>
    /// <param name="minimumDisplayTime">Minimum time to show loading screen even if video ends early (only applies if video still playing)</param>
    public IEnumerator FadeOutAfterVideo(float minimumDisplayTime = 2f)
    {
        // If video is still playing (fallback case - WaitForVideoToFinish wasn't called), wait for it
        if (isVideoPlaying && !videoFinished)
        {
            float elapsed = 0f;
            float maxWaitTime = 30f; // Safety timeout
            
            LogDebug("FadeOutAfterVideo: Video still playing, waiting...");
            
            while (isVideoPlaying && !videoFinished && elapsed < maxWaitTime)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            if (elapsed >= maxWaitTime)
            {
                LogDebug("Video wait timeout reached, proceeding with fade out");
            }
            else
            {
                LogDebug($"Video finished after {elapsed:F1} seconds");
            }
            
            // Ensure minimum display time only if we had to wait for video here
            if (elapsed < minimumDisplayTime)
            {
                yield return new WaitForSeconds(minimumDisplayTime - elapsed);
            }
        }
        else
        {
            // Video already finished (WaitForVideoToFinish was used) - fade out immediately
            // No extra wait needed since we already waited for video in PHASE 4.5
            LogDebug("Video already finished, starting smooth fade out transition");
        }
        
        // Smooth fade out transition (video fades out with the overlay)
        // This prevents the abrupt cut when the video ends
        LogDebug("Starting smooth fade out...");
        yield return FadeTo(0f, fadeOutTime);
        
        // Stop video AFTER fade completes (video is no longer visible anyway)
        StopVideo();
        
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        
        LogDebug("Fade out complete");
    }

    private IEnumerator StartVideo(string videoName)
    {
        LogDebug($"Starting loading video: {videoName}");
        
        // Clear our private RenderTexture before showing video
        if (privateRenderTexture != null)
        {
            ClearRenderTexture(privateRenderTexture);
        }
        
        VideoPlayer activePlayer = useWebGL ? webglVideoPlayer : pcVideoPlayer;
        GameObject activeRoot = useWebGL ? webglVideoRoot : pcVideoRoot;
        
        if (activePlayer == null)
        {
            LogDebug("Video player not configured, skipping video playback");
            yield break;
        }
        
        // Ensure video player uses our private RenderTexture (not the shared one)
        activePlayer.targetTexture = privateRenderTexture;
        
        // Show UI video display (fullscreen RawImage) - this works with Screen Space - Overlay
        if (useUIVideoDisplay && uiVideoDisplayObject != null)
        {
            uiVideoDisplayObject.SetActive(true);
            // Ensure the RawImage has our RenderTexture
            if (uiVideoRawImage != null && uiVideoRawImage.texture != privateRenderTexture)
            {
                uiVideoRawImage.texture = privateRenderTexture;
            }
            LogDebug("Showing UI video display (RawImage)");
        }
        
        // Show the video root (for the VideoPlayer component, even if we don't use its mesh)
        if (activeRoot != null)
        {
            activeRoot.SetActive(true);
        }
        
        // For 3D mesh display (backup, but UI display is preferred)
        if (!useUIVideoDisplay && videoMeshRenderer != null && privateRenderTexture != null)
        {
            Material mat = videoMeshRenderer.material;
            if (mat != null && mat.mainTexture != privateRenderTexture)
            {
                mat.mainTexture = privateRenderTexture;
                LogDebug($"Re-assigned RenderTexture to mesh material: {videoMeshRenderer.gameObject.name}");
            }
        }
        
        // Configure video player
        activePlayer.loopPointReached -= OnVideoFinished;
        activePlayer.loopPointReached += OnVideoFinished;
        activePlayer.errorReceived -= OnVideoError;
        activePlayer.errorReceived += OnVideoError;
        
        // Set video source
        string videoPath;
        if (useWebGL)
        {
            // For WebGL, Application.streamingAssetsPath is a URL; just append the file name
            videoPath = $"{Application.streamingAssetsPath}/{videoName}";
            activePlayer.source = VideoSource.Url;
            activePlayer.url = videoPath;
        }
        else
        {
            // For desktop/standalone builds, use file:// protocol for file paths
            // Unity's VideoPlayer on desktop can handle file:// URLs or direct file paths
            videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoName);
            // Convert to file:// URL format for better compatibility across platforms
            if (!videoPath.StartsWith("file://") && !videoPath.StartsWith("http://") && !videoPath.StartsWith("https://"))
            {
                videoPath = "file://" + videoPath;
            }
            activePlayer.source = VideoSource.Url;
            activePlayer.url = videoPath;
        }
        activePlayer.isLooping = false; // Play once, then transition
        activePlayer.playOnAwake = false;
        
        // Prepare video
        isVideoPlaying = false;
        videoFinished = false;
        
        activePlayer.Prepare();
        
        // Wait for preparation (with timeout)
        float prepareTimeout = 5f;
        float prepareElapsed = 0f;
        while (!activePlayer.isPrepared && prepareElapsed < prepareTimeout)
        {
            prepareElapsed += Time.deltaTime;
            yield return null;
        }
        
        if (!activePlayer.isPrepared)
        {
            LogDebug($"Video preparation timed out, trying default video");
            
            // Try default video as fallback
            if (videoName != defaultLoadingVideo)
            {
                string fallbackPath;
                if (useWebGL)
                {
                    fallbackPath = $"{Application.streamingAssetsPath}/{defaultLoadingVideo}";
                }
                else
                {
                    fallbackPath = System.IO.Path.Combine(Application.streamingAssetsPath, defaultLoadingVideo);
                    if (!fallbackPath.StartsWith("file://") && !fallbackPath.StartsWith("http://") && !fallbackPath.StartsWith("https://"))
                    {
                        fallbackPath = "file://" + fallbackPath;
                    }
                }
                activePlayer.url = fallbackPath;
                activePlayer.Prepare();
                
                prepareElapsed = 0f;
                while (!activePlayer.isPrepared && prepareElapsed < prepareTimeout)
                {
                    prepareElapsed += Time.deltaTime;
                    yield return null;
                }
            }
            
            if (!activePlayer.isPrepared)
            {
                LogDebug("Failed to prepare any video, hiding video player");
                if (activeRoot != null) activeRoot.SetActive(false);
                if (uiVideoDisplayObject != null) uiVideoDisplayObject.SetActive(false);
                yield break;
            }
        }
        
        // Start playback
        activePlayer.Play();
        isVideoPlaying = true;
        LogDebug("Loading video started");
    }
    
    private void StopVideo()
    {
        if (!isVideoPlaying) return;
        
        LogDebug("Stopping loading video");
        
        VideoPlayer activePlayer = useWebGL ? webglVideoPlayer : pcVideoPlayer;
        GameObject activeRoot = useWebGL ? webglVideoRoot : pcVideoRoot;
        
        if (activePlayer != null)
        {
            activePlayer.Stop();
            activePlayer.loopPointReached -= OnVideoFinished;
            activePlayer.errorReceived -= OnVideoError;
        }
        
        // Clear our private RenderTexture to prevent stale frames
        if (privateRenderTexture != null)
        {
            ClearRenderTexture(privateRenderTexture);
        }
        
        // Hide UI video display
        if (uiVideoDisplayObject != null)
        {
            uiVideoDisplayObject.SetActive(false);
        }
        
        // Hide video root
        if (activeRoot != null)
        {
            activeRoot.SetActive(false);
        }
        
        isVideoPlaying = false;
        videoFinished = false;
    }
    
    private void OnVideoFinished(VideoPlayer vp)
    {
        // For loading videos, we loop, so this won't trigger unless we disable looping
        videoFinished = true;
        LogDebug("Loading video finished");
    }
    
    private void OnVideoError(VideoPlayer vp, string message)
    {
        LogDebug($"Video error: {message}");
        videoFinished = true;
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (canvasGroup == null)
        {
            LogDebug("ERROR: CanvasGroup is null, cannot fade!");
            yield break;
        }
        
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;
    }
    
    /// <summary>
    /// Clears a RenderTexture to black to prevent showing stale video frames.
    /// </summary>
    private void ClearRenderTexture(RenderTexture rt)
    {
        if (rt == null) return;
        
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;
        GL.Clear(true, true, Color.black);
        RenderTexture.active = prev;
        
        LogDebug("RenderTexture cleared");
    }
    
    private void OnDisable()
    {
        StopVideo();
    }
    
    private void OnDestroy()
    {
        StopVideo();
        
        // Clean up dynamically created UI video display
        if (uiVideoDisplayObject != null)
        {
            Destroy(uiVideoDisplayObject);
            uiVideoDisplayObject = null;
            uiVideoRawImage = null;
        }
        
        // Clean up our private RenderTexture
        if (privateRenderTexture != null)
        {
            privateRenderTexture.Release();
            Destroy(privateRenderTexture);
            privateRenderTexture = null;
            LogDebug("Private RenderTexture destroyed");
        }
    }
    
    private void LogDebug(string message)
    {
        if (showDebugLogs)
            Debug.Log($"[LoadingOverlay] {message}");
    }
}
