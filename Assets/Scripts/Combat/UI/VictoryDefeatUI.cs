using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;

/// <summary>
/// Handles victory and defeat UI banners with continue button
/// Similar to CombatPhaseUI - shows banner, then continue button to return to map selection
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Displays end-of-combat results and allows player to continue</para>
/// 
/// <para><strong>Animation Flow:</strong></para>
/// <list type="bullet">
/// <item>Banner fades in from transparent at center screen</item>
/// <item>Holds for a moment</item>
/// <item>Continue button appears</item>
/// <item>Player clicks continue to return to map selection</item>
/// </list>
/// 
/// <para><strong>Usage:</strong> Call ShowVictory() or ShowDefeat() to display appropriate banner</para>
/// </remarks>
public class VictoryDefeatUI : Singleton<VictoryDefeatUI>
{
    [Header("Banner References")]
    [Tooltip("The main container panel for the victory/defeat banner")]
    [SerializeField] [Required("Banner panel is required for animations!")]
    private RectTransform bannerPanel;
    
    [Tooltip("Image for 'Victory' banner")]
    [SerializeField] [Required("Victory image is required!")]
    private Image victoryImage;
    
    [Tooltip("Image for 'Defeat' banner")]
    [SerializeField] [Required("Defeat image is required!")]
    private Image defeatImage;
    
    [Tooltip("Background image for the banner")]
    [SerializeField]
    private Image bannerBackground;
    
    [Header("Banner Sprites")]
    [Tooltip("Sprite image for 'Victory' banner")]
    [SerializeField] [Required("Victory sprite is required!")]
    private Sprite victorySprite;
    
    [Tooltip("Sprite image for 'Defeat' banner")]
    [SerializeField] [Required("Defeat sprite is required!")]
    private Sprite defeatSprite;

    [Header("Continue Button")]
    [Tooltip("The continue button that appears after the banner")]
    [SerializeField] [Required("Continue button is required!")]
    private Button continueButton;
    
    [Tooltip("Text component for the continue button")]
    [SerializeField]
    private TMP_Text continueButtonText;

    [Header("Reward System")]
    [Tooltip("Reference to the RewardChestUI component for showing rewards")]
    [SerializeField] private RewardChestUI rewardChestUI;

    [Header("Collection Systems")]
    [Tooltip("Reference to the RelicCollectionData for collecting main boss relics")]
    [SerializeField] private RelicCollectionData relicCollection;
    
    [Tooltip("Reference to the SpecialCardCollectionData for collecting mini-boss special cards")]
    [SerializeField] private SpecialCardCollectionData specialCardCollection;
    
    [Tooltip("Reference to the GameProgressData for tracking map completion")]
    [SerializeField] private GameProgressData gameProgress;
    
    [Tooltip("Reference to the LevelTransitionData to get current map ID")]
    [SerializeField] private LevelTransitionData levelTransitionData;

    [Header("Animation Settings")]
    [Tooltip("Duration for fade in animation")]
    [SerializeField] private float fadeInDuration = 0.4f;
    
    [Tooltip("Duration to hold the banner visible before showing button")]
    [SerializeField] private float holdDuration = 1.0f;
    
    [Tooltip("Duration for button fade in")]
    [SerializeField] private float buttonFadeInDuration = 0.3f;
    
    [Tooltip("Ease type for fade in")]
    [SerializeField] private Ease fadeInEase = Ease.OutQuad;
    
    [Tooltip("Ease type for button fade in")]
    [SerializeField] private Ease buttonFadeInEase = Ease.OutQuad;

    // Canvas groups for fading
    private CanvasGroup backgroundCanvasGroup;
    private CanvasGroup victoryImageCanvasGroup;
    private CanvasGroup defeatImageCanvasGroup;
    private CanvasGroup continueButtonCanvasGroup;
    private CanvasGroup bannerPanelCanvasGroup;
    
    // Original positions for resetting
    private Vector2 bannerPanelOriginalPosition;
    private Vector2 victoryImageOriginalPosition;
    private Vector2 defeatImageOriginalPosition;
    
    // Flag to prevent overlapping animations
    private bool isAnimating = false;
    
    // Current state
    private bool isShowingVictory = false;
    
    // Reward state
    private RewardData pendingRewardData = null;
    private bool hasMoreEnemies = false;
    private bool isFirstReward = true;
    private System.Action rewardCollectedCallback = null;

    private void Start()
    {
        // Ensure canvas groups exist
        if (bannerBackground != null)
        {
            backgroundCanvasGroup = bannerBackground.GetComponent<CanvasGroup>();
            if (backgroundCanvasGroup == null)
            {
                backgroundCanvasGroup = bannerBackground.gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        if (victoryImage != null)
        {
            victoryImageCanvasGroup = victoryImage.GetComponent<CanvasGroup>();
            if (victoryImageCanvasGroup == null)
            {
                victoryImageCanvasGroup = victoryImage.gameObject.AddComponent<CanvasGroup>();
            }
            victoryImageOriginalPosition = victoryImage.rectTransform.anchoredPosition;
        }
        
        if (defeatImage != null)
        {
            defeatImageCanvasGroup = defeatImage.GetComponent<CanvasGroup>();
            if (defeatImageCanvasGroup == null)
            {
                defeatImageCanvasGroup = defeatImage.gameObject.AddComponent<CanvasGroup>();
            }
            defeatImageOriginalPosition = defeatImage.rectTransform.anchoredPosition;
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
        
        // Store original positions
        if (bannerPanel != null)
        {
            bannerPanelOriginalPosition = bannerPanel.anchoredPosition;
            
            // Setup canvas group for blocking interactions
            bannerPanelCanvasGroup = bannerPanel.GetComponent<CanvasGroup>();
            if (bannerPanelCanvasGroup == null)
            {
                bannerPanelCanvasGroup = bannerPanel.gameObject.AddComponent<CanvasGroup>();
            }
            
            // Setup blocking image to prevent interactions with elements below
            Image blockingImage = bannerPanel.GetComponent<Image>();
            if (blockingImage == null)
            {
                blockingImage = bannerPanel.gameObject.AddComponent<Image>();
                blockingImage.color = new Color(0, 0, 0, 0); // Transparent but still blocks raycasts
            }
            blockingImage.raycastTarget = true; // Enable raycast blocking
        }
        
        // Hide initially
        HideBanner();
    }

    private void OnDestroy()
    {
        // Clean up button listener
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinueButtonClicked);
        }
    }

    /// <summary>
    /// Shows the "Victory" banner (final victory, no reward)
    /// </summary>
    public void ShowVictory()
    {
        isShowingVictory = true;
        pendingRewardData = null;
        hasMoreEnemies = false;
        isFirstReward = true;
        rewardCollectedCallback = null;
        
        // Reset continue button state in case it was disabled
        if (continueButton != null)
        {
            continueButton.interactable = true;
        }
        
        ShowBanner(victorySprite, true);
    }

    /// <summary>
    /// Shows the "Victory" banner with reward chest
    /// </summary>
    /// <param name="rewardData">The reward data to display (can be null if no reward)</param>
    /// <param name="hasMoreEnemies">True if there are more enemies to fight, false if this is the final victory</param>
    /// <param name="onRewardCollected">Callback when reward is collected (to continue to next enemy or final victory)</param>
    /// <param name="isFirstReward">True if this is the first enemy reward (miniboss), false if second enemy (main boss)</param>
    public void ShowVictoryWithReward(RewardData rewardData, bool hasMoreEnemies, System.Action onRewardCollected, bool isFirstReward = true)
    {
        isShowingVictory = true;
        pendingRewardData = rewardData;
        this.hasMoreEnemies = hasMoreEnemies;
        this.isFirstReward = isFirstReward;
        rewardCollectedCallback = onRewardCollected;
        ShowBanner(victorySprite, true);
    }

    /// <summary>
    /// Shows the "Defeat" banner
    /// </summary>
    public void ShowDefeat()
    {
        isShowingVictory = false;
        ShowBanner(defeatSprite, false);
    }

    /// <summary>
    /// Generic method to show victory or defeat banner
    /// </summary>
    /// <param name="bannerSprite">The sprite image to display for the banner</param>
    /// <param name="isVictory">True for victory, false for defeat</param>
    private void ShowBanner(Sprite bannerSprite, bool isVictory)
    {
        if (isAnimating)
        {
            // Kill any ongoing animations and continue
            DOTween.Kill(bannerPanel);
            if (victoryImage != null) DOTween.Kill(victoryImage.rectTransform);
            if (defeatImage != null) DOTween.Kill(defeatImage.rectTransform);
            if (backgroundCanvasGroup != null) DOTween.Kill(backgroundCanvasGroup);
            if (victoryImageCanvasGroup != null) DOTween.Kill(victoryImageCanvasGroup);
            if (defeatImageCanvasGroup != null) DOTween.Kill(defeatImageCanvasGroup);
            if (continueButtonCanvasGroup != null) DOTween.Kill(continueButtonCanvasGroup);
        }
        
        StartCoroutine(AnimateBanner(bannerSprite, isVictory));
    }

    /// <summary>
    /// Coroutine that handles the full banner animation sequence
    /// </summary>
    private IEnumerator AnimateBanner(Sprite bannerSprite, bool isVictory)
    {
        isAnimating = true;
        
        // Validate required components
        if (bannerPanel == null)
        {
            Debug.LogError("[VictoryDefeatUI] Banner panel is not assigned! Cannot animate banner.", this);
            isAnimating = false;
            yield break;
        }
        
        if (bannerSprite == null)
        {
            Debug.LogError("[VictoryDefeatUI] Banner sprite is null! Cannot show banner.", this);
            isAnimating = false;
            yield break;
        }
        
        // Select the appropriate image component
        Image activeImage = isVictory ? victoryImage : defeatImage;
        CanvasGroup activeImageCanvasGroup = isVictory ? victoryImageCanvasGroup : defeatImageCanvasGroup;
        Vector2 activeImageOriginalPos = isVictory ? victoryImageOriginalPosition : defeatImageOriginalPosition;
        
        if (activeImage == null || activeImageCanvasGroup == null)
        {
            Debug.LogError($"[VictoryDefeatUI] {(isVictory ? "Victory" : "Defeat")} image is not assigned! Cannot show banner.", this);
            isAnimating = false;
            yield break;
        }
        
        // Hide both images first, then show the correct one
        if (victoryImage != null)
        {
            victoryImage.gameObject.SetActive(isVictory);
            victoryImage.rectTransform.anchoredPosition = victoryImageOriginalPosition;
        }
        
        if (defeatImage != null)
        {
            defeatImage.gameObject.SetActive(!isVictory);
            defeatImage.rectTransform.anchoredPosition = defeatImageOriginalPosition;
        }
        
        // Setup banner content on the active image
        activeImage.sprite = bannerSprite;
        activeImage.color = Color.white;
        
        // Reset positions and make visible but transparent
        bannerPanel.anchoredPosition = bannerPanelOriginalPosition;
        activeImage.rectTransform.anchoredPosition = activeImageOriginalPos;
        
        // Set initial alpha values
        if (backgroundCanvasGroup != null)
        {
            backgroundCanvasGroup.alpha = 0f;
        }
        activeImageCanvasGroup.alpha = 0f;
        
        // Hide continue button initially
        if (continueButton != null)
        {
            continueButtonCanvasGroup.alpha = 0f;
            continueButton.interactable = false;
            continueButton.gameObject.SetActive(false);
        }
        
        // Make panel and images visible
        bannerPanel.gameObject.SetActive(true);
        activeImage.gameObject.SetActive(true);
        
        // Enable blocking when banner is shown
        if (bannerPanelCanvasGroup != null)
        {
            bannerPanelCanvasGroup.blocksRaycasts = true;
            bannerPanelCanvasGroup.interactable = true;
        }
        
        // Phase 1: Fade in at center (both background and banner image)
        Sequence fadeIn = DOTween.Sequence();
        if (backgroundCanvasGroup != null)
        {
            fadeIn.Join(backgroundCanvasGroup.DOFade(1f, fadeInDuration).SetEase(fadeInEase));
        }
        fadeIn.Join(activeImageCanvasGroup.DOFade(1f, fadeInDuration).SetEase(fadeInEase));
        yield return fadeIn.WaitForCompletion();
        
        // Phase 2: Hold
        yield return new WaitForSeconds(holdDuration);
        
        // Phase 3: Show continue button
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
            continueButton.interactable = true;
            yield return continueButtonCanvasGroup.DOFade(1f, buttonFadeInDuration).SetEase(buttonFadeInEase).WaitForCompletion();
        }
        
        // Animation complete - button is now clickable
        isAnimating = false;
    }

    /// <summary>
    /// Called when continue button is clicked
    /// Either shows reward chest or transitions to map selection
    /// </summary>
    private void OnContinueButtonClicked()
    {
        if (continueButton != null)
        {
            continueButton.interactable = false; // Prevent multiple clicks
        }
        
        // If there's a reward to show, display it
        if (pendingRewardData != null && rewardChestUI != null)
        {
            // Hide banner first
            HideBanner();
            
            // Show reward chest with appropriate reward type
            rewardChestUI.ShowReward(pendingRewardData, OnRewardChestComplete, isFirstReward);
        }
        else if (pendingRewardData == null && rewardCollectedCallback == null)
        {
            // No reward and no callback = final victory, go to map selection
            TransitionToMapSelection();
        }
        else
        {
            // Edge case: reward data is null but callback exists (shouldn't happen, but handle gracefully)
            Debug.LogWarning("[VictoryDefeatUI] Continue clicked but reward state is inconsistent. Clearing state and going to map selection.", this);
            pendingRewardData = null;
            rewardCollectedCallback = null;
            TransitionToMapSelection();
        }
    }

    /// <summary>
    /// Called when reward chest collection is complete
    /// </summary>
    private void OnRewardChestComplete()
    {
        // Hide reward UI
        if (rewardChestUI != null)
        {
            rewardChestUI.HideReward();
        }
        
        // Capture reward data BEFORE clearing state
        RewardData collectedReward = pendingRewardData;
        bool wasFirstReward = isFirstReward;
        bool hadMoreEnemies = hasMoreEnemies;
        
        // Process reward collection based on type
        ProcessRewardCollection(collectedReward, wasFirstReward);
        
        // If this was the final enemy (no more enemies), mark map as complete
        if (!hadMoreEnemies)
        {
            MarkCurrentMapComplete();
        }
        
        // Store callback before clearing state
        System.Action callback = rewardCollectedCallback;
        
        // Clear reward state immediately to prevent double-triggering
        pendingRewardData = null;
        rewardCollectedCallback = null;
        hasMoreEnemies = false;
        isFirstReward = true;
        
        // Call the callback (which will either spawn next enemy or trigger final victory)
        callback?.Invoke();
    }
    
    /// <summary>
    /// Processes reward collection by adding to appropriate collection (special card or relic)
    /// </summary>
    /// <param name="rewardData">The reward data from the chest</param>
    /// <param name="isMiniBossReward">True if this is a mini-boss reward (special card), false for main boss (relic)</param>
    private void ProcessRewardCollection(RewardData rewardData, bool isMiniBossReward)
    {
        if (rewardData == null)
        {
            Debug.LogWarning("[VictoryDefeatUI] Cannot process null reward data.");
            return;
        }
        
        if (isMiniBossReward)
        {
            // Mini-boss reward: collect special card
            SpecialCardData specialCard = rewardData.SpecialCardReward;
            if (specialCard != null && specialCardCollection != null)
            {
                specialCardCollection.AddSpecialCard(specialCard);
                Debug.Log($"[VictoryDefeatUI] Collected special card: {specialCard.CardName}");
            }
            else if (specialCard != null)
            {
                Debug.LogWarning("[VictoryDefeatUI] SpecialCardCollectionData not assigned, cannot save special card!");
            }
        }
        else
        {
            // Main boss reward: collect relic
            RelicData relic = rewardData.AssociatedRelic;
            if (relic != null && relicCollection != null)
            {
                relicCollection.AddRelic(relic);
                Debug.Log($"[VictoryDefeatUI] Collected relic: {relic.RelicName}");
            }
            else if (relic != null)
            {
                Debug.LogWarning("[VictoryDefeatUI] RelicCollectionData not assigned, cannot save relic!");
            }
        }
    }
    
    /// <summary>
    /// Marks the current map as complete in the game progress
    /// </summary>
    private void MarkCurrentMapComplete()
    {
        if (gameProgress == null)
        {
            Debug.LogWarning("[VictoryDefeatUI] GameProgressData not assigned, cannot track map completion!");
            return;
        }
        
        if (levelTransitionData == null || levelTransitionData.SelectedMapData == null)
        {
            Debug.LogWarning("[VictoryDefeatUI] LevelTransitionData or SelectedMapData is null, cannot determine current map!");
            return;
        }
        
        string mapId = levelTransitionData.SelectedMapData.MapId;
        if (gameProgress.MarkMapComplete(mapId))
        {
            Debug.Log($"[VictoryDefeatUI] Map '{mapId}' marked as complete!");
            
            // Check if Kaluwalhatian was just unlocked
            if (gameProgress.CheckKaluwalhatianUnlock())
            {
                Debug.Log("[VictoryDefeatUI] Kaluwalhatian has been unlocked!");
            }
            
            // Check if game is now complete (Kaluwalhatian beaten)
            if (gameProgress.IsGameCompleted)
            {
                Debug.Log("[VictoryDefeatUI] Game complete! Player has beaten Kaluwalhatian!");
            }
        }
    }

    /// <summary>
    /// Transitions back to map selection scene
    /// </summary>
    private void TransitionToMapSelection()
    {
        // Transition back to map selection using SceneController
        if (SceneController.Instance != null)
        {
            SceneController.Instance
                .NewTransition()
                .Load(SceneDatabase.Slots.Session, SceneDatabase.Scenes.MapSelection, setActive: true)
                .WithLoadingVideo("loading")
                .WithPauseMusic(9)
                .Perform();
        }
        else
        {
            Debug.LogError("[VictoryDefeatUI] SceneController.Instance is null! Cannot transition to map selection.", this);
        }
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
            
            // Reset image positions and alpha
            if (victoryImage != null)
            {
                victoryImage.rectTransform.anchoredPosition = victoryImageOriginalPosition;
                if (victoryImageCanvasGroup != null) victoryImageCanvasGroup.alpha = 0f;
            }
            
            if (defeatImage != null)
            {
                defeatImage.rectTransform.anchoredPosition = defeatImageOriginalPosition;
                if (defeatImageCanvasGroup != null) defeatImageCanvasGroup.alpha = 0f;
            }
            
            // Hide continue button
            if (continueButton != null)
            {
                if (continueButtonCanvasGroup != null) continueButtonCanvasGroup.alpha = 0f;
                continueButton.interactable = false;
                continueButton.gameObject.SetActive(false);
            }
            
            // Disable blocking when banner is hidden
            if (bannerPanelCanvasGroup != null)
            {
                bannerPanelCanvasGroup.blocksRaycasts = false;
                bannerPanelCanvasGroup.interactable = false;
            }
            
            // Hide panel
            bannerPanel.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Returns true if a banner animation is currently playing
    /// </summary>
    public bool IsAnimating => isAnimating;
}

