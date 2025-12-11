using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

/// <summary>
/// UI component for a single special card button.
/// Displays card info and handles click events.
/// </summary>
public class SpecialCardButtonUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Image for the card icon/sprite")]
    [SerializeField] private Image cardIcon;
    
    [Tooltip("Text for card name (optional)")]
    [SerializeField] private TextMeshProUGUI cardNameText;
    
    [Tooltip("Text for card description (optional)")]
    [SerializeField] private TextMeshProUGUI cardDescriptionText;
    
    [Tooltip("Background image for visual feedback")]
    [SerializeField] private Image backgroundImage;
    
    // Button component
    private Button button;
    
    // Canvas group for fade effects
    private CanvasGroup canvasGroup;
    
    // Current card data
    private SpecialCardData currentCard;
    
    // Click event
    public event System.Action OnCardClicked;
    
    private void Awake()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }
        
        button.onClick.AddListener(HandleClick);
        
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Try to find child components if not assigned
        if (cardIcon == null)
        {
            Transform iconTransform = transform.Find("Icon");
            if (iconTransform != null)
            {
                cardIcon = iconTransform.GetComponent<Image>();
            }
        }
        
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }
    }
    
    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
        }
    }
    
    /// <summary>
    /// Sets up the button to display a specific special card
    /// </summary>
    public void SetupCard(SpecialCardData card)
    {
        currentCard = card;
        
        if (card == null)
        {
            gameObject.SetActive(false);
            return;
        }
        
        // Set icon
        if (cardIcon != null && card.CardSprite != null)
        {
            cardIcon.sprite = card.CardSprite;
            cardIcon.enabled = true;
        }
        
        // Set name
        if (cardNameText != null)
        {
            cardNameText.text = card.CardName;
        }
        
        // Set description
        if (cardDescriptionText != null)
        {
            cardDescriptionText.text = GetEffectDescription(card);
        }
        
        // Reset visual state
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
        
        if (button != null)
        {
            button.interactable = true;
        }
        
        transform.localScale = Vector3.one;
    }
    
    /// <summary>
    /// Gets a human-readable description of the card effect
    /// </summary>
    private string GetEffectDescription(SpecialCardData card)
    {
        switch (card.EffectType)
        {
            case SpecialCardData.SpecialCardEffectType.DamageUp:
                return $"+{card.EffectPercentage}% DMG for {card.Duration} turns";
                
            case SpecialCardData.SpecialCardEffectType.DefenseUpTwoTargets:
                return $"+{card.EffectPercentage}% DEF to {card.TargetCount} allies for {card.Duration} turn";
                
            case SpecialCardData.SpecialCardEffectType.NoCooldown:
                return $"No cooldowns for {card.Duration} rounds";
                
            default:
                return "Unknown effect";
        }
    }
    
    /// <summary>
    /// Handles button click
    /// </summary>
    private void HandleClick()
    {
        OnCardClicked?.Invoke();
    }
    
    /// <summary>
    /// Plays a use animation when the card is consumed
    /// </summary>
    public void PlayUseAnimation(float duration, System.Action onComplete)
    {
        // Disable button during animation
        if (button != null)
        {
            button.interactable = false;
        }
        
        // Create animation sequence
        Sequence sequence = DOTween.Sequence();
        
        // Scale up slightly
        sequence.Append(transform.DOScale(1.2f, duration * 0.3f).SetEase(Ease.OutBack));
        
        // Flash white
        if (backgroundImage != null)
        {
            sequence.Join(backgroundImage.DOColor(Color.white, duration * 0.3f));
        }
        
        // Scale down and fade out
        sequence.Append(transform.DOScale(0f, duration * 0.5f).SetEase(Ease.InBack));
        
        if (canvasGroup != null)
        {
            sequence.Join(canvasGroup.DOFade(0f, duration * 0.5f));
        }
        
        // Call complete callback
        sequence.OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }
}
