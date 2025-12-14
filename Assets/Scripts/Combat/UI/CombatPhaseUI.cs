using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;

/// <summary>
/// Handles combat phase UI animations (Battle Start, Player Turn, Enemy Turn)
/// Inspired by Slay the Spire - fade in center, then slide out to left
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Shows animated banners for combat phase transitions</para>
/// 
/// <para><strong>Animation Flow:</strong></para>
/// <list type="bullet">
/// <item>Banner fades in from transparent at center screen</item>
/// <item>Holds for a moment</item>
/// <item>Slides out to the left while fading out</item>
/// </list>
/// 
/// <para><strong>Usage:</strong> Call ShowBattleStart(), ShowPlayerTurn(), or ShowEnemyTurn() to display banners</para>
/// </remarks>
public class CombatPhaseUI : Singleton<CombatPhaseUI>
{
    [Header("Banner References")]
    [Tooltip("The main container panel for the phase banner")]
    [SerializeField] [Required("Banner panel is required for animations!")]
    private RectTransform bannerPanel;
    
    [Tooltip("Image for 'Battle Start' banner (square aspect ratio)")]
    [SerializeField] [Required("Battle Start image is required!")]
    private Image battleStartImage;
    
    [Tooltip("Image for 'Player Turn' and 'Enemy Turn' banners (rectangular aspect ratio)")]
    [SerializeField] [Required("Turn banner image is required!")]
    private Image turnBannerImage;
    
    [Tooltip("The subtitle text for turn numbers (e.g., '1st turn', '2nd turn')")]
    [SerializeField]
    private TMP_Text subtitleText;
    
    [Tooltip("Background image for the banner")]
    [SerializeField]
    private Image bannerBackground;
    
    [Tooltip("Optional decorative elements (swords, dividers, etc.)")]
    [SerializeField]
    private CanvasGroup decorationsGroup;
    
    [Header("Banner Sprites")]
    [Tooltip("Sprite image for 'Battle Start' banner")]
    [SerializeField] [Required("Battle Start sprite is required!")]
    private Sprite battleStartSprite;
    
    [Tooltip("Sprite image for 'Player Turn' banner")]
    [SerializeField] [Required("Player Turn sprite is required!")]
    private Sprite playerTurnSprite;
    
    [Tooltip("Sprite image for 'Enemy Turn' banner")]
    [SerializeField] [Required("Enemy Turn sprite is required!")]
    private Sprite enemyTurnSprite;

    [Header("Animation Settings")]
    [Tooltip("Duration for fade in animation")]
    [SerializeField] private float fadeInDuration = 0.4f;
    
    [Tooltip("Duration to hold the banner visible")]
    [SerializeField] private float holdDuration = 1.0f;
    
    [Tooltip("Duration for slide out animation")]
    [SerializeField] private float slideOutDuration = 0.5f;
    
    [Tooltip("How far left to slide when exiting (in pixels)")]
    [SerializeField] private float slideOutDistance = 500f;
    
    [Tooltip("Ease type for fade in")]
    [SerializeField] private Ease fadeInEase = Ease.OutQuad;
    
    [Tooltip("Ease type for slide out")]
    [SerializeField] private Ease slideOutEase = Ease.InQuad;


    // Canvas group for fading the background (stays in place)
    private CanvasGroup backgroundCanvasGroup;
    
    // Original positions for resetting
    private Vector2 bannerPanelOriginalPosition;
    private Vector2 battleStartImageOriginalPosition;
    private Vector2 turnBannerImageOriginalPosition;
    private Vector2 subtitleTextOriginalPosition;
    
    // Track current turn number
    private int currentTurnNumber = 0;
    
    // Flag to prevent overlapping animations
    private bool isAnimating = false;

    private void Start()
    {
        // Ensure we have a canvas group for fading the background
        if (bannerBackground != null)
        {
            backgroundCanvasGroup = bannerBackground.GetComponent<CanvasGroup>();
            if (backgroundCanvasGroup == null)
        {
                backgroundCanvasGroup = bannerBackground.gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        // Store original positions
        bannerPanelOriginalPosition = bannerPanel.anchoredPosition;
        
        if (battleStartImage != null)
        {
            battleStartImageOriginalPosition = battleStartImage.rectTransform.anchoredPosition;
        }
        
        if (turnBannerImage != null)
        {
            turnBannerImageOriginalPosition = turnBannerImage.rectTransform.anchoredPosition;
        }
        
        if (subtitleText != null)
        {
            subtitleTextOriginalPosition = subtitleText.rectTransform.anchoredPosition;
        }
        
        // Hide initially
        HideBanner();
    }

    /// <summary>
    /// Shows the "Battle Start" banner at the beginning of combat
    /// </summary>
    public void ShowBattleStart()
    {
        currentTurnNumber = 0;
        ShowBanner(battleStartSprite, "", true);
    }

    /// <summary>
    /// Shows the "Player Turn" banner with turn number
    /// </summary>
    public void ShowPlayerTurn()
    {
        currentTurnNumber++;
        string ordinal = GetOrdinal(currentTurnNumber);
        ShowBanner(playerTurnSprite, $"{ordinal} turn", false);
    }

    /// <summary>
    /// Shows the "Enemy Turn" banner
    /// </summary>
    public void ShowEnemyTurn()
    {
        ShowBanner(enemyTurnSprite, "", false);
    }

    /// <summary>
    /// Generic method to show any banner with image and optional subtitle text
    /// </summary>
    /// <param name="bannerSprite">The sprite image to display for the banner</param>
    /// <param name="subtitle">Optional subtitle text (e.g., turn number)</param>
    /// <param name="isBattleStart">True if this is Battle Start (uses square image), false for turn banners (uses rectangular image)</param>
    public void ShowBanner(Sprite bannerSprite, string subtitle, bool isBattleStart)
    {
        if (isAnimating)
        {
            // Kill any ongoing animations and continue
            DOTween.Kill(bannerPanel);
            if (battleStartImage != null) DOTween.Kill(battleStartImage.rectTransform);
            if (turnBannerImage != null) DOTween.Kill(turnBannerImage.rectTransform);
            if (subtitleText != null) DOTween.Kill(subtitleText.rectTransform);
            if (backgroundCanvasGroup != null) DOTween.Kill(backgroundCanvasGroup);
        }
        
        StartCoroutine(AnimateBanner(bannerSprite, subtitle, isBattleStart));
    }

    /// <summary>
    /// Coroutine that handles the full banner animation sequence
    /// </summary>
    private IEnumerator AnimateBanner(Sprite bannerSprite, string subtitle, bool isBattleStart)
    {
        isAnimating = true;
        
        // Validate required components
        if (bannerPanel == null)
        {
            Debug.LogError("[CombatPhaseUI] Banner panel is not assigned! Cannot animate banner.", this);
            isAnimating = false;
            yield break;
        }
        
        if (bannerSprite == null)
        {
            Debug.LogError("[CombatPhaseUI] Banner sprite is null! Cannot show banner.", this);
            isAnimating = false;
            yield break;
        }
        
        // Select the appropriate image component based on banner type
        Image activeImage = isBattleStart ? battleStartImage : turnBannerImage;
        RectTransform activeImageRect = activeImage != null ? activeImage.rectTransform : null;
        Vector2 activeImageOriginalPos = isBattleStart ? battleStartImageOriginalPosition : turnBannerImageOriginalPosition;
        
        if (activeImage == null || activeImageRect == null)
        {
            Debug.LogError($"[CombatPhaseUI] {(isBattleStart ? "Battle Start" : "Turn Banner")} image is not assigned! Cannot show banner.", this);
            isAnimating = false;
            yield break;
        }
        
        // Hide both images first, then show the correct one
        if (battleStartImage != null)
        {
            battleStartImage.gameObject.SetActive(isBattleStart);
            battleStartImage.rectTransform.anchoredPosition = battleStartImageOriginalPosition;
        }
        
        if (turnBannerImage != null)
        {
            turnBannerImage.gameObject.SetActive(!isBattleStart);
            turnBannerImage.rectTransform.anchoredPosition = turnBannerImageOriginalPosition;
        }
        
        // Setup banner content on the active image
        activeImage.sprite = bannerSprite;
        activeImage.color = Color.white; // Use sprite's original colors
        
        if (subtitleText != null)
        {
            subtitleText.text = subtitle;
            subtitleText.gameObject.SetActive(!string.IsNullOrEmpty(subtitle));
        }
        
        // Reset positions and make visible but transparent
        bannerPanel.anchoredPosition = bannerPanelOriginalPosition;
        activeImageRect.anchoredPosition = activeImageOriginalPos;
        
        // Reset subtitle text position
        if (subtitleText != null)
        {
            subtitleText.rectTransform.anchoredPosition = subtitleTextOriginalPosition;
        }
        
        // Set initial alpha values
        if (backgroundCanvasGroup != null)
        {
            backgroundCanvasGroup.alpha = 0f;
        }
        
        // Create canvas group for the active image if it doesn't exist
        CanvasGroup activeImageCanvasGroup = activeImage.GetComponent<CanvasGroup>();
        if (activeImageCanvasGroup == null)
        {
            activeImageCanvasGroup = activeImage.gameObject.AddComponent<CanvasGroup>();
        }
        activeImageCanvasGroup.alpha = 0f;
        
        // Create canvas group for subtitle text if it doesn't exist
        CanvasGroup subtitleCanvasGroup = null;
        if (subtitleText != null)
        {
            subtitleCanvasGroup = subtitleText.GetComponent<CanvasGroup>();
            if (subtitleCanvasGroup == null)
            {
                subtitleCanvasGroup = subtitleText.gameObject.AddComponent<CanvasGroup>();
            }
            subtitleCanvasGroup.alpha = 0f;
        }
        
        // Make panel and images visible
        bannerPanel.gameObject.SetActive(true);
        activeImage.gameObject.SetActive(true);
        
        // Phase 1: Fade in at center (both background, banner image, and subtitle text)
        Sequence fadeIn = DOTween.Sequence();
        if (backgroundCanvasGroup != null)
        {
            fadeIn.Join(backgroundCanvasGroup.DOFade(1f, fadeInDuration).SetEase(fadeInEase));
        }
        fadeIn.Join(activeImageCanvasGroup.DOFade(1f, fadeInDuration).SetEase(fadeInEase));
        if (subtitleCanvasGroup != null)
        {
            fadeIn.Join(subtitleCanvasGroup.DOFade(1f, fadeInDuration).SetEase(fadeInEase));
        }
        yield return fadeIn.WaitForCompletion();
        
        // Phase 2: Hold
        yield return new WaitForSeconds(holdDuration);
        
        // Phase 3: Slide out banner image and subtitle text to left while fading, background only fades
        Sequence slideOut = DOTween.Sequence();
        
        // Banner image slides out and fades
        slideOut.Append(activeImageRect.DOAnchorPosX(activeImageOriginalPos.x - slideOutDistance, slideOutDuration).SetEase(slideOutEase));
        slideOut.Join(activeImageCanvasGroup.DOFade(0f, slideOutDuration).SetEase(Ease.InQuad));
        
        // Subtitle text slides out with the banner image and fades
        if (subtitleText != null && subtitleCanvasGroup != null)
        {
            slideOut.Join(subtitleText.rectTransform.DOAnchorPosX(subtitleTextOriginalPosition.x - slideOutDistance, slideOutDuration).SetEase(slideOutEase));
            slideOut.Join(subtitleCanvasGroup.DOFade(0f, slideOutDuration).SetEase(Ease.InQuad));
        }
        
        // Background only fades (stays in place)
        if (backgroundCanvasGroup != null)
        {
            slideOut.Join(backgroundCanvasGroup.DOFade(0f, slideOutDuration).SetEase(Ease.InQuad));
        }
        
        yield return slideOut.WaitForCompletion();
        
        // Hide and reset
        HideBanner();
        isAnimating = false;
    }

    /// <summary>
    /// Immediately hides the banner
    /// </summary>
    private void HideBanner()
    {
        if (bannerPanel != null)
        {
            // Reset background
            if (backgroundCanvasGroup != null)
            {
                backgroundCanvasGroup.alpha = 0f;
            }
            
            // Reset banner panel position
            bannerPanel.anchoredPosition = bannerPanelOriginalPosition;
            
            // Reset image positions
            if (battleStartImage != null)
            {
                battleStartImage.rectTransform.anchoredPosition = battleStartImageOriginalPosition;
                CanvasGroup cg = battleStartImage.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 0f;
            }
            
            if (turnBannerImage != null)
            {
                turnBannerImage.rectTransform.anchoredPosition = turnBannerImageOriginalPosition;
                CanvasGroup cg = turnBannerImage.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 0f;
            }
            
            // Reset subtitle text position
            if (subtitleText != null)
            {
                subtitleText.rectTransform.anchoredPosition = subtitleTextOriginalPosition;
                CanvasGroup cg = subtitleText.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 0f;
            }
            
            // Hide panel
            bannerPanel.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Returns true if a banner animation is currently playing
    /// </summary>
    public bool IsAnimating => isAnimating;

    /// <summary>
    /// Gets the ordinal suffix for a number (1st, 2nd, 3rd, etc.)
    /// </summary>
    private string GetOrdinal(int number)
    {
        if (number <= 0) return number.ToString();
        
        int lastTwoDigits = number % 100;
        if (lastTwoDigits >= 11 && lastTwoDigits <= 13)
        {
            return $"{number}th";
        }
        
        return (number % 10) switch
        {
            1 => $"{number}st",
            2 => $"{number}nd",
            3 => $"{number}rd",
            _ => $"{number}th"
        };
    }

    /// <summary>
    /// Resets turn counter (call when starting a new battle)
    /// </summary>
    public void ResetTurnCounter()
    {
        currentTurnNumber = 0;
    }
}

