using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Sirenix.OdinInspector;

/// <summary>
/// Handles enemy spawn overlay animations - shows black overlay and banner image before enemy spawns
/// Shows overlay before miniboss (first enemy) and main boss (second enemy) spawns
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Creates dramatic reveal animations before important enemy encounters</para>
/// 
/// <para><strong>Animation Flow:</strong></para>
/// <list type="bullet">
/// <item>Black overlay fades in to cover the screen</item>
/// <item>Banner image (16:9) slides diagonally from bottom-left to upper-right with bounce</item>
/// <item>Holds for a moment</item>
/// <item>Overlay fades out</item>
/// </list>
/// 
/// <para><strong>Usage:</strong> Call ShowEnemySpawnOverlay() with banner sprite before spawning enemy</para>
/// </remarks>
public class EnemySpawnOverlayUI : Singleton<EnemySpawnOverlayUI>
{
    [Header("Canvas References")]
    [Tooltip("The main canvas that contains the overlay (should cover full screen)")]
    [SerializeField] [Required("Overlay canvas is required!")]
    private Canvas overlayCanvas;
    
    [Tooltip("The black background image that covers the screen")]
    [SerializeField] [Required("Black background is required!")]
    private Image blackBackground;
    
    [Tooltip("The image component that displays the banner (16:9 ratio, e.g., Mini Boss Battle, Main Boss Battle)")]
    [SerializeField] [Required("Banner image is required!")]
    private Image bannerImage;
    
    [Header("Banner Sprites")]
    [Tooltip("Sprite for 'Mini Boss Battle' banner (shown before first enemy)")]
    [SerializeField] [Required("Mini Boss banner sprite is required!")]
    private Sprite miniBossBannerSprite;
    
    [Tooltip("Sprite for 'Main Boss Battle' banner (shown before second enemy)")]
    [SerializeField] [Required("Main Boss banner sprite is required!")]
    private Sprite mainBossBannerSprite;

    [Header("Animation Settings")]
    [Tooltip("Duration for black overlay fade in")]
    [SerializeField] private float fadeInDuration = 0.5f;
    
    [Tooltip("Duration for banner diagonal slide animation")]
    [SerializeField] private float slideDuration = 0.8f;
    
    [Tooltip("Bounce back distance (in pixels) after reaching target position")]
    [SerializeField] private float bounceDistance = 50f;
    
    [Tooltip("Duration for bounce back animation")]
    [SerializeField] private float bounceDuration = 0.3f;
    
    [Tooltip("Duration to hold the banner visible before fading out")]
    [SerializeField] private float holdDuration = 1.5f;
    
    [Tooltip("Duration for fade out animation")]
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    [Tooltip("Ease type for fade in")]
    [SerializeField] private Ease fadeInEase = Ease.OutQuad;
    
    [Tooltip("Ease type for slide animation")]
    [SerializeField] private Ease slideEase = Ease.OutCubic;
    
    [Tooltip("Ease type for bounce animation")]
    [SerializeField] private Ease bounceEase = Ease.OutBounce;
    
    [Tooltip("Ease type for fade out")]
    [SerializeField] private Ease fadeOutEase = Ease.InQuad;
    
    // Canvas groups for fading
    private CanvasGroup blackBackgroundCanvasGroup;
    private CanvasGroup bannerCanvasGroup;
    
    // Original positions for resetting
    private Vector2 bannerOriginalPosition;
    
    // Screen dimensions for diagonal calculation
    private RectTransform canvasRect;
    
    // Flag to prevent overlapping animations
    private bool isAnimating = false;

    private void Start()
    {
        // Ensure canvas groups exist
        if (blackBackground != null)
        {
            blackBackgroundCanvasGroup = blackBackground.GetComponent<CanvasGroup>();
            if (blackBackgroundCanvasGroup == null)
            {
                blackBackgroundCanvasGroup = blackBackground.gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        if (bannerImage != null)
        {
            bannerCanvasGroup = bannerImage.GetComponent<CanvasGroup>();
            if (bannerCanvasGroup == null)
            {
                bannerCanvasGroup = bannerImage.gameObject.AddComponent<CanvasGroup>();
            }
            
            // Store original position
            bannerOriginalPosition = bannerImage.rectTransform.anchoredPosition;
        }
        
        // Get canvas rect for screen calculations
        if (overlayCanvas != null)
        {
            canvasRect = overlayCanvas.GetComponent<RectTransform>();
        }
        
        // Hide initially
        HideOverlay();
    }

    /// <summary>
    /// Shows the enemy spawn overlay with the appropriate banner sprite
    /// </summary>
    /// <param name="isMainBoss">True for Main Boss banner, false for Mini Boss banner</param>
    public void ShowEnemySpawnOverlay(bool isMainBoss)
    {
        if (isAnimating)
        {
            // Kill any ongoing animations and continue
            DOTween.Kill(blackBackgroundCanvasGroup);
            DOTween.Kill(bannerCanvasGroup);
            DOTween.Kill(bannerImage.rectTransform);
        }
        
        // Select the appropriate banner sprite
        Sprite bannerSprite = isMainBoss ? mainBossBannerSprite : miniBossBannerSprite;
        
        if (bannerSprite == null)
        {
            Debug.LogWarning($"[EnemySpawnOverlayUI] {(isMainBoss ? "Main Boss" : "Mini Boss")} banner sprite is null! Cannot show overlay.", this);
            return;
        }
        
        StartCoroutine(AnimateEnemySpawnOverlay(bannerSprite));
    }

    /// <summary>
    /// Coroutine that handles the full overlay animation sequence
    /// </summary>
    private IEnumerator AnimateEnemySpawnOverlay(Sprite bannerSprite)
    {
        isAnimating = true;
        
        // Validate required components
        if (blackBackground == null || bannerImage == null || canvasRect == null)
        {
            Debug.LogError("[EnemySpawnOverlayUI] Required components are missing! Cannot animate overlay.", this);
            isAnimating = false;
            yield break;
        }
        
        // Set banner sprite
        bannerImage.sprite = bannerSprite;
        bannerImage.color = Color.white;
        
        // Calculate diagonal positions
        // Start: bottom-left (off-screen)
        // End: upper-right (center of screen)
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;
        
        Vector2 startPosition = new Vector2(-canvasWidth * 0.6f, -canvasHeight * 0.6f);
        Vector2 targetPosition = Vector2.zero; // Center of screen
        Vector2 bouncePosition = targetPosition + new Vector2(bounceDistance * 0.7f, bounceDistance * 0.7f);
        
        // Reset positions and alpha
        blackBackgroundCanvasGroup.alpha = 0f;
        bannerCanvasGroup.alpha = 0f;
        bannerImage.rectTransform.anchoredPosition = startPosition;
        
        // Make overlay visible
        overlayCanvas.gameObject.SetActive(true);
        blackBackground.gameObject.SetActive(true);
        bannerImage.gameObject.SetActive(true);
        
        // Phase 1: Fade in black background
        yield return blackBackgroundCanvasGroup.DOFade(1f, fadeInDuration).SetEase(fadeInEase).WaitForCompletion();
        
        // Phase 2: Slide banner diagonally from bottom-left to upper-right
        Sequence slideSequence = DOTween.Sequence();
        slideSequence.Append(bannerImage.rectTransform.DOAnchorPos(targetPosition, slideDuration).SetEase(slideEase));
        slideSequence.Join(bannerCanvasGroup.DOFade(1f, slideDuration).SetEase(fadeInEase));
        yield return slideSequence.WaitForCompletion();
        
        // Phase 3: Bounce back effect
        yield return bannerImage.rectTransform.DOAnchorPos(bouncePosition, bounceDuration).SetEase(bounceEase).WaitForCompletion();
        yield return bannerImage.rectTransform.DOAnchorPos(targetPosition, bounceDuration * 0.5f).SetEase(Ease.OutQuad).WaitForCompletion();
        
        // Phase 4: Hold
        yield return new WaitForSeconds(holdDuration);
        
        // Phase 5: Fade out everything
        Sequence fadeOutSequence = DOTween.Sequence();
        fadeOutSequence.Join(blackBackgroundCanvasGroup.DOFade(0f, fadeOutDuration).SetEase(fadeOutEase));
        fadeOutSequence.Join(bannerCanvasGroup.DOFade(0f, fadeOutDuration).SetEase(fadeOutEase));
        yield return fadeOutSequence.WaitForCompletion();
        
        // Hide and reset
        HideOverlay();
        isAnimating = false;
    }

    /// <summary>
    /// Immediately hides the overlay
    /// </summary>
    private void HideOverlay()
    {
        if (overlayCanvas != null)
        {
            // Reset alpha values
            if (blackBackgroundCanvasGroup != null)
            {
                blackBackgroundCanvasGroup.alpha = 0f;
            }
            
            if (bannerCanvasGroup != null)
            {
                bannerCanvasGroup.alpha = 0f;
            }
            
            // Reset banner position
            if (bannerImage != null)
            {
                bannerImage.rectTransform.anchoredPosition = bannerOriginalPosition;
            }
            
            // Hide overlay
            overlayCanvas.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Returns true if overlay animation is currently playing
    /// </summary>
    public bool IsAnimating => isAnimating;
}

