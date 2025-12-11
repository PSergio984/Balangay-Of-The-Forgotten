using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// Handles reward chest display, animations, and interactions
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Manages the reward chest UI with glow, shake, opening, and reward display</para>
/// 
/// <para><strong>Animation Flow:</strong></para>
/// <list type="bullet">
/// <item>Chest appears with glow behind it</item>
/// <item>Player can click chest or anywhere on screen</item>
/// <item>Chest shakes when clicked</item>
/// <item>Chest sprite transitions from closed to open</item>
/// <item>Chest disappears/gets smaller</item>
/// <item>Reward item appears</item>
/// <item>Continue button appears</item>
/// </list>
/// 
/// <para><strong>Usage:</strong> Called by VictoryDefeatUI to show reward chest during victory</para>
/// </remarks>
public class RewardChestUI : Singleton<RewardChestUI>
{
    [Header("Chest References")]
    [Tooltip("The chest image component")]
    [SerializeField] private Image chestImage;
    
    [Tooltip("The glow image component behind the chest")]
    [SerializeField] private Image glowImage;
    
    [Header("Reward Item Images")]
    [Tooltip("The reward item image component for the first enemy (miniboss)")]
    [SerializeField] private Image firstRewardItemImage;
    
    [Tooltip("The reward item image component for the second enemy (main boss)")]
    [SerializeField] private Image secondRewardItemImage;
    
    [Header("Reward Header")]
    [Tooltip("The reward header image component showing the reward sprite")]
    [SerializeField] private Image rewardHeaderImage;
    
    [Header("Continue Button")]
    [Tooltip("The continue button that appears after the reward item is shown")]
    [SerializeField] private Button continueButton;
    
    [Tooltip("The container panel for all reward elements")]
    [SerializeField] private RectTransform rewardPanel;
    
    // Canvas group for blocking interactions
    private CanvasGroup rewardPanelCanvasGroup;

    [Header("Animation Settings")]
    [Tooltip("Duration for chest fade in")]
    [SerializeField] private float chestFadeInDuration = 0.5f;
    
    [Tooltip("Duration for glow pulse animation")]
    [SerializeField] private float glowPulseDuration = 1.0f;
    
    [Tooltip("Shake intensity when chest is clicked")]
    [SerializeField] private float shakeIntensity = 10f;
    
    [Tooltip("Shake duration when chest is clicked")]
    [SerializeField] private float shakeDuration = 0.3f;
    
    [Tooltip("Duration for chest sprite transition (closed to open)")]
    [SerializeField] private float chestTransitionDuration = 0.5f;
    
    [Tooltip("Duration for chest scale down/disappear")]
    [SerializeField] private float chestDisappearDuration = 0.5f;
    
    [Tooltip("Duration for reward item fade in")]
    [SerializeField] private float rewardFadeInDuration = 0.5f;
    
    [Tooltip("Scale factor for chest when disappearing (0.3 = 30% size)")]
    [SerializeField] private float disappearScale = 0.3f;

    // Canvas groups for fading
    private CanvasGroup chestCanvasGroup;
    private CanvasGroup glowCanvasGroup;
    private CanvasGroup firstRewardItemCanvasGroup;
    private CanvasGroup secondRewardItemCanvasGroup;
    private CanvasGroup rewardHeaderCanvasGroup;
    private CanvasGroup continueButtonCanvasGroup;
    
    // Original positions and scales
    private Vector3 chestOriginalPosition;
    private Vector3 chestOriginalScale;
    
    // State tracking
    private bool isAnimating = false;
    private bool chestOpened = false;
    private bool continueClicked = false;
    private System.Action onCompleteCallback;
    private RewardData currentRewardData;

    private void OnDestroy()
    {
        // Clean up button listener
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueButtonClicked);
        }
    }

    protected override void Awake()
    {
        base.Awake(); // Initialize Singleton
        
        // Ensure canvas groups exist
        if (chestImage != null)
        {
            chestCanvasGroup = chestImage.GetComponent<CanvasGroup>();
            if (chestCanvasGroup == null)
            {
                chestCanvasGroup = chestImage.gameObject.AddComponent<CanvasGroup>();
            }
            chestOriginalPosition = chestImage.rectTransform.anchoredPosition;
            chestOriginalScale = chestImage.rectTransform.localScale;
        }
        
        if (glowImage != null)
        {
            glowCanvasGroup = glowImage.GetComponent<CanvasGroup>();
            if (glowCanvasGroup == null)
            {
                glowCanvasGroup = glowImage.gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        if (firstRewardItemImage != null)
        {
            firstRewardItemCanvasGroup = firstRewardItemImage.GetComponent<CanvasGroup>();
            if (firstRewardItemCanvasGroup == null)
            {
                firstRewardItemCanvasGroup = firstRewardItemImage.gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        if (secondRewardItemImage != null)
        {
            secondRewardItemCanvasGroup = secondRewardItemImage.GetComponent<CanvasGroup>();
            if (secondRewardItemCanvasGroup == null)
            {
                secondRewardItemCanvasGroup = secondRewardItemImage.gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        if (rewardHeaderImage != null)
        {
            rewardHeaderCanvasGroup = rewardHeaderImage.GetComponent<CanvasGroup>();
            if (rewardHeaderCanvasGroup == null)
            {
                rewardHeaderCanvasGroup = rewardHeaderImage.gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        if (continueButton != null)
        {
            continueButtonCanvasGroup = continueButton.GetComponent<CanvasGroup>();
            if (continueButtonCanvasGroup == null)
            {
                continueButtonCanvasGroup = continueButton.gameObject.AddComponent<CanvasGroup>();
            }
            
            // Setup button click handler
            continueButton.onClick.AddListener(OnContinueButtonClicked);
        }
        
        // Setup click detection on entire screen
        SetupClickDetection();
        
        // Setup blocking for reward panel
        if (rewardPanel != null)
        {
            rewardPanelCanvasGroup = rewardPanel.GetComponent<CanvasGroup>();
            if (rewardPanelCanvasGroup == null)
            {
                rewardPanelCanvasGroup = rewardPanel.gameObject.AddComponent<CanvasGroup>();
            }
            
            // Ensure the panel has an image for raycast blocking
            Image panelImage = rewardPanel.GetComponent<Image>();
            if (panelImage == null)
            {
                panelImage = rewardPanel.gameObject.AddComponent<Image>();
                panelImage.color = new Color(0, 0, 0, 0); // Transparent but blocks raycasts
            }
            panelImage.raycastTarget = true;
        }
        
        // Hide initially
        HideReward();
    }

    /// <summary>
    /// Sets up click detection for the entire screen
    /// </summary>
    private void SetupClickDetection()
    {
        // Add EventTrigger to reward panel to detect clicks anywhere
        if (rewardPanel != null)
        {
            EventTrigger trigger = rewardPanel.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                trigger = rewardPanel.gameObject.AddComponent<EventTrigger>();
            }
            
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((data) => { OnScreenClicked(); });
            trigger.triggers.Add(entry);
        }
        
        // Also allow clicking directly on chest
        if (chestImage != null)
        {
            Button chestButton = chestImage.GetComponent<Button>();
            if (chestButton == null)
            {
                chestButton = chestImage.gameObject.AddComponent<Button>();
            }
            chestButton.onClick.AddListener(OnChestClicked);
        }
    }

    /// <summary>
    /// Shows the reward chest with the given reward data
    /// </summary>
    /// <param name="rewardData">The reward data containing chest sprites and reward item</param>
    /// <param name="onComplete">Callback when reward collection is complete</param>
    /// <param name="isFirstReward">True if this is the first enemy reward (miniboss), false if second enemy (main boss)</param>
    public void ShowReward(RewardData rewardData, System.Action onComplete, bool isFirstReward = true)
    {
        if (isAnimating)
        {
            Debug.LogWarning("[RewardChestUI] Already animating! Ignoring ShowReward call.", this);
            return;
        }
        
        if (rewardData == null)
        {
            Debug.LogWarning("[RewardChestUI] RewardData is null! Cannot show reward.", this);
            onComplete?.Invoke();
            return;
        }
        
        onCompleteCallback = onComplete;
        chestOpened = false;
        currentRewardData = rewardData;
        
        // Setup sprites
        if (chestImage != null && rewardData.ClosedChestSprite != null)
        {
            chestImage.sprite = rewardData.ClosedChestSprite;
        }
        
        if (glowImage != null && rewardData.GlowSprite != null)
        {
            glowImage.sprite = rewardData.GlowSprite;
        }
        
        // Setup reward item image based on which reward type
        Image activeRewardItemImage = isFirstReward ? firstRewardItemImage : secondRewardItemImage;
        if (activeRewardItemImage != null && rewardData.RewardItemSprite != null)
        {
            activeRewardItemImage.sprite = rewardData.RewardItemSprite;
        }
        
        // Hide the inactive reward item image
        if (isFirstReward && secondRewardItemImage != null)
        {
            secondRewardItemImage.gameObject.SetActive(false);
        }
        else if (!isFirstReward && firstRewardItemImage != null)
        {
            firstRewardItemImage.gameObject.SetActive(false);
        }
        
        // Start animation sequence
        StartCoroutine(ShowRewardSequence(isFirstReward));
    }

    /// <summary>
    /// Coroutine that handles the full reward display sequence
    /// </summary>
    /// <param name="isFirstReward">True if this is the first enemy reward, false if second</param>
    private IEnumerator ShowRewardSequence(bool isFirstReward)
    {
        isAnimating = true;
        
        // Reset positions and scales
        if (chestImage != null)
        {
            chestImage.rectTransform.anchoredPosition = chestOriginalPosition;
            chestImage.rectTransform.localScale = chestOriginalScale;
        }
        
        // Make panel visible and enable blocking
        if (rewardPanel != null)
        {
            rewardPanel.gameObject.SetActive(true);
            if (rewardPanelCanvasGroup != null)
            {
                rewardPanelCanvasGroup.blocksRaycasts = true;
                rewardPanelCanvasGroup.interactable = true;
            }
        }
        
        // Get the active reward item image and canvas group
        Image activeRewardItemImage = isFirstReward ? firstRewardItemImage : secondRewardItemImage;
        CanvasGroup activeRewardItemCanvasGroup = isFirstReward ? firstRewardItemCanvasGroup : secondRewardItemCanvasGroup;
        
        // Set initial alpha values
        if (chestCanvasGroup != null) chestCanvasGroup.alpha = 0f;
        if (glowCanvasGroup != null) glowCanvasGroup.alpha = 0f;
        if (activeRewardItemCanvasGroup != null) activeRewardItemCanvasGroup.alpha = 0f;
        if (rewardHeaderCanvasGroup != null) rewardHeaderCanvasGroup.alpha = 0f;
        if (continueButtonCanvasGroup != null) continueButtonCanvasGroup.alpha = 0f;
        
        // Show chest, glow, and reward header
        if (chestImage != null) chestImage.gameObject.SetActive(true);
        if (glowImage != null) glowImage.gameObject.SetActive(true);
        if (rewardHeaderImage != null) rewardHeaderImage.gameObject.SetActive(true);
        if (activeRewardItemImage != null) activeRewardItemImage.gameObject.SetActive(false);
        
        // Hide continue button initially
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
            continueButton.interactable = false;
        }
        
        continueClicked = false;
        
        // Fade in chest, glow, and reward header
        Sequence fadeIn = DOTween.Sequence();
        if (glowCanvasGroup != null)
        {
            fadeIn.Join(glowCanvasGroup.DOFade(1f, chestFadeInDuration));
        }
        if (chestCanvasGroup != null)
        {
            fadeIn.Join(chestCanvasGroup.DOFade(1f, chestFadeInDuration));
        }
        if (rewardHeaderCanvasGroup != null)
        {
            fadeIn.Join(rewardHeaderCanvasGroup.DOFade(1f, chestFadeInDuration));
        }
        yield return fadeIn.WaitForCompletion();
        
        // Start glow pulse animation
        if (glowImage != null && glowCanvasGroup != null)
        {
            glowCanvasGroup.DOFade(0.5f, glowPulseDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
        
        // Wait for player to click (chestOpened will be set to true when clicked)
        while (!chestOpened)
        {
            yield return null;
        }
        
        // Stop glow pulse
        if (glowCanvasGroup != null)
        {
            DOTween.Kill(glowCanvasGroup);
        }
        
        // Shake chest
        if (chestImage != null)
        {
            yield return chestImage.rectTransform.DOShakePosition(shakeDuration, shakeIntensity, 10, 90f, false, true)
                .WaitForCompletion();
        }
        
        // Transition chest sprite from closed to open
        if (chestImage != null && currentRewardData != null && currentRewardData.OpenChestSprite != null)
        {
            // Fade out closed chest
            if (chestCanvasGroup != null)
            {
                yield return chestCanvasGroup.DOFade(0f, chestTransitionDuration * 0.5f).WaitForCompletion();
            }
            
            // Change to open chest sprite
            chestImage.sprite = currentRewardData.OpenChestSprite;
            
            // Fade in open chest
            if (chestCanvasGroup != null)
            {
                yield return chestCanvasGroup.DOFade(1f, chestTransitionDuration * 0.5f).WaitForCompletion();
            }
        }
        else
        {
            // No open chest sprite, just wait
            yield return new WaitForSeconds(chestTransitionDuration);
        }
        
        // Chest disappears/gets smaller
        if (chestImage != null)
        {
            Sequence disappear = DOTween.Sequence();
            disappear.Join(chestImage.rectTransform.DOScale(disappearScale, chestDisappearDuration));
            if (chestCanvasGroup != null)
            {
                disappear.Join(chestCanvasGroup.DOFade(0f, chestDisappearDuration));
            }
            yield return disappear.WaitForCompletion();
            
            chestImage.gameObject.SetActive(false);
        }
        
        // Hide glow (but keep reward header visible)
        if (glowImage != null)
        {
            if (glowCanvasGroup != null)
            {
                yield return glowCanvasGroup.DOFade(0f, 0.3f).WaitForCompletion();
            }
            glowImage.gameObject.SetActive(false);
        }
        
        // Keep reward header visible - don't hide it
        
        // Show reward item
        if (activeRewardItemImage != null)
        {
            activeRewardItemImage.gameObject.SetActive(true);
            if (activeRewardItemCanvasGroup != null)
            {
                yield return activeRewardItemCanvasGroup.DOFade(1f, rewardFadeInDuration).WaitForCompletion();
            }
            
            // Hold reward item visible for a moment
            yield return new WaitForSeconds(1.0f);
        }
        
        // Show continue button
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
            continueButton.interactable = true;
            if (continueButtonCanvasGroup != null)
            {
                yield return continueButtonCanvasGroup.DOFade(1f, 0.3f).WaitForCompletion();
            }
        }
        
        // Wait for continue button to be clicked
        while (!continueClicked)
        {
            yield return null;
        }
        
        // Complete
        isAnimating = false;
        onCompleteCallback?.Invoke();
    }

    /// <summary>
    /// Called when chest is clicked directly
    /// </summary>
    private void OnChestClicked()
    {
        if (!isAnimating || chestOpened) return;
        chestOpened = true;
    }

    /// <summary>
    /// Called when screen is clicked anywhere
    /// </summary>
    private void OnScreenClicked()
    {
        if (!isAnimating || chestOpened) return;
        chestOpened = true;
    }

    /// <summary>
    /// Called when continue button is clicked
    /// </summary>
    private void OnContinueButtonClicked()
    {
        if (continueButton != null)
        {
            continueButton.interactable = false; // Prevent multiple clicks
        }
        continueClicked = true;
    }
    
    /// <summary>
    /// Returns true if the reward UI is currently showing
    /// </summary>
    public bool IsShowing => isAnimating || (rewardPanel != null && rewardPanel.gameObject.activeSelf);

    /// <summary>
    /// Immediately hides the reward UI
    /// </summary>
    public void HideReward()
    {
        if (rewardPanel != null)
        {
            // Disable blocking when hidden
            if (rewardPanelCanvasGroup != null)
            {
                rewardPanelCanvasGroup.blocksRaycasts = false;
                rewardPanelCanvasGroup.interactable = false;
            }
            
            rewardPanel.gameObject.SetActive(false);
        }
        
        if (chestImage != null) chestImage.gameObject.SetActive(false);
        if (glowImage != null) glowImage.gameObject.SetActive(false);
        if (firstRewardItemImage != null) firstRewardItemImage.gameObject.SetActive(false);
        if (secondRewardItemImage != null) secondRewardItemImage.gameObject.SetActive(false);
        if (rewardHeaderImage != null) rewardHeaderImage.gameObject.SetActive(false);
        if (continueButton != null)
        {
            if (continueButtonCanvasGroup != null) continueButtonCanvasGroup.alpha = 0f;
            continueButton.interactable = false;
            continueButton.gameObject.SetActive(false);
        }
        
        // Kill any ongoing animations
        if (chestCanvasGroup != null) DOTween.Kill(chestCanvasGroup);
        if (glowCanvasGroup != null) DOTween.Kill(glowCanvasGroup);
        if (firstRewardItemCanvasGroup != null) DOTween.Kill(firstRewardItemCanvasGroup);
        if (secondRewardItemCanvasGroup != null) DOTween.Kill(secondRewardItemCanvasGroup);
        if (rewardHeaderCanvasGroup != null) DOTween.Kill(rewardHeaderCanvasGroup);
        if (continueButtonCanvasGroup != null) DOTween.Kill(continueButtonCanvasGroup);
        if (chestImage != null) DOTween.Kill(chestImage.rectTransform);
        
        isAnimating = false;
        chestOpened = false;
        continueClicked = false;
    }

}

