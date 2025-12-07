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
    
    [Tooltip("The main title text (e.g., 'Battle Start', 'Player Turn')")]
    [SerializeField] [Required("Title text is required to display phase names!")]
    private TMP_Text titleText;
    
    [Tooltip("The subtitle text (e.g., '1st turn', 'Enemy Turn')")]
    [SerializeField]
    private TMP_Text subtitleText;
    
    [Tooltip("Background image for the banner")]
    [SerializeField]
    private Image bannerBackground;
    
    [Tooltip("Optional decorative elements (swords, dividers, etc.)")]
    [SerializeField]
    private CanvasGroup decorationsGroup;

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

    [Header("Colors")]
    [Tooltip("Color for Player Turn text")]
    [SerializeField] private Color playerTurnColor = new Color(1f, 0.85f, 0.4f, 1f); // Gold
    
    [Tooltip("Color for Enemy Turn text")]
    [SerializeField] private Color enemyTurnColor = new Color(0.8f, 0.3f, 0.3f, 1f); // Red
    
    [Tooltip("Color for Battle Start text")]
    [SerializeField] private Color battleStartColor = Color.white;

    // Canvas group for fading the entire banner
    private CanvasGroup bannerCanvasGroup;
    
    // Original position for resetting
    private Vector2 originalPosition;
    
    // Track current turn number
    private int currentTurnNumber = 0;
    
    // Flag to prevent overlapping animations
    private bool isAnimating = false;

    private void Start()
    {
        // Ensure we have a canvas group for fading
        bannerCanvasGroup = bannerPanel.GetComponent<CanvasGroup>();
        if (bannerCanvasGroup == null)
        {
            bannerCanvasGroup = bannerPanel.gameObject.AddComponent<CanvasGroup>();
        }
        
        // Store original position
        originalPosition = bannerPanel.anchoredPosition;
        
        // Hide initially
        HideBanner();
    }

    /// <summary>
    /// Shows the "Battle Start" banner at the beginning of combat
    /// </summary>
    public void ShowBattleStart()
    {
        currentTurnNumber = 0;
        ShowBanner("Battle Start", "", battleStartColor);
    }

    /// <summary>
    /// Shows the "Player Turn" banner with turn number
    /// </summary>
    public void ShowPlayerTurn()
    {
        currentTurnNumber++;
        string ordinal = GetOrdinal(currentTurnNumber);
        ShowBanner("Player Turn", $"{ordinal} turn", playerTurnColor);
    }

    /// <summary>
    /// Shows the "Enemy Turn" banner
    /// </summary>
    public void ShowEnemyTurn()
    {
        ShowBanner("Enemy Turn", "", enemyTurnColor);
    }

    /// <summary>
    /// Generic method to show any banner with custom text
    /// </summary>
    public void ShowBanner(string title, string subtitle, Color titleColor)
    {
        if (isAnimating)
        {
            // Kill any ongoing animation and continue
            DOTween.Kill(bannerPanel);
            DOTween.Kill(bannerCanvasGroup);
        }
        
        StartCoroutine(AnimateBanner(title, subtitle, titleColor));
    }

    /// <summary>
    /// Coroutine that handles the full banner animation sequence
    /// </summary>
    private IEnumerator AnimateBanner(string title, string subtitle, Color titleColor)
    {
        isAnimating = true;
        
        // Validate required components
        if (titleText == null)
        {
            Debug.LogError("[CombatPhaseUI] Title text is not assigned! Cannot show banner.", this);
            isAnimating = false;
            yield break;
        }
        
        if (bannerPanel == null)
        {
            Debug.LogError("[CombatPhaseUI] Banner panel is not assigned! Cannot animate banner.", this);
            isAnimating = false;
            yield break;
        }
        
        // Setup banner content
        titleText.text = title;
        titleText.color = titleColor;
        
        if (subtitleText != null)
        {
            subtitleText.text = subtitle;
            subtitleText.gameObject.SetActive(!string.IsNullOrEmpty(subtitle));
        }
        
        // Reset position and make visible but transparent
        bannerPanel.anchoredPosition = originalPosition;
        bannerCanvasGroup.alpha = 0f;
        bannerPanel.gameObject.SetActive(true);
        
        // Phase 1: Fade in at center
        Tween fadeIn = bannerCanvasGroup.DOFade(1f, fadeInDuration).SetEase(fadeInEase);
        yield return fadeIn.WaitForCompletion();
        
        // Phase 2: Hold
        yield return new WaitForSeconds(holdDuration);
        
        // Phase 3: Slide out to left while fading
        Sequence slideOut = DOTween.Sequence();
        slideOut.Append(bannerPanel.DOAnchorPosX(originalPosition.x - slideOutDistance, slideOutDuration).SetEase(slideOutEase));
        slideOut.Join(bannerCanvasGroup.DOFade(0f, slideOutDuration).SetEase(Ease.InQuad));
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
            bannerCanvasGroup.alpha = 0f;
            bannerPanel.anchoredPosition = originalPosition;
            bannerPanel.gameObject.SetActive(false);
        }
    }

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

