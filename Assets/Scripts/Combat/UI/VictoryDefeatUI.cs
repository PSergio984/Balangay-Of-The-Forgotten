using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using Sirenix.OdinInspector;
using AudioSystem;

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

    [Header("Leaderboard")]
    [Tooltip("Name entry popup — shown after victory, before continue button")]
    [SerializeField] private NameEntryUI nameEntryUI;

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

    [Header("Music Settings")]
    [Tooltip("Music to play when transitioning to map selection/main menu")]
    [SerializeField] private SoundData mapSelectionMusic;
    
    [Tooltip("Fade time for music transition")]
    [SerializeField] private float MusicFadeTime = 2f;

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
        
        RecordCombatClearTime();

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

        if (!hasMoreEnemies)
        {
            RecordCombatClearTime();
        }

        ShowBanner(victorySprite, true);
    }

    private void RecordCombatClearTime()
    {
        if (CombatTimer.Instance != null)
        {
            CombatTimer.Instance.StopTimer();
        }

        if (levelTransitionData == null)
        {
            Debug.LogError("[VictoryDefeatUI] levelTransitionData is null. Cannot record clear time.", this);
            return;
        }

        if (CombatTimer.Instance != null)
        {
            levelTransitionData.ClearTimeSeconds = CombatTimer.Instance.ElapsedSeconds;
            Debug.Log($"[VictoryDefeatUI] Recorded combat clear time: {CombatTimer.Instance.ElapsedSeconds}s");
        }
        else
        {
            levelTransitionData.ClearTimeSeconds = 0f;
        }
    }

    /// <summary>
    /// Shows the "Defeat" banner
    /// </summary>
    public void ShowDefeat()
    {
        isShowingVictory = false;
        pendingRewardData = null;
        hasMoreEnemies = false;
        isFirstReward = true;
        rewardCollectedCallback = null;
        
        // Stop combat timer before showing defeat UI (matching victory behavior)
        if (CombatTimer.Instance != null)
        {
            CombatTimer.Instance.StopTimer();
        }
        
        // Reset continue button state in case it was disabled
        if (continueButton != null)
        {
            continueButton.interactable = true;
        }
        
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
        
        // Phase 3: Show NameEntryUI prompt (on final victory) and enable Continue button
        if (isVictory && !hasMoreEnemies)
        {
            if (nameEntryUI != null)
            {
                string mapId = (levelTransitionData != null && levelTransitionData.SelectedMapData != null)
                    ? levelTransitionData.SelectedMapData.MapId
                    : string.Empty;
                float clearTime = levelTransitionData != null ? levelTransitionData.ClearTimeSeconds : 0f;

                nameEntryUI.Show(
                    mapId: mapId,
                    clearTimeSeconds: clearTime,
                    onComplete: () => StartCoroutine(EnableContinueButtonRoutine())
                );
            }
            else
            {
                Debug.LogWarning("[VictoryDefeatUI] nameEntryUI is not assigned! Skipping name entry and enabling continue button directly.", this);
                yield return EnableContinueButtonRoutine();
            }
        }
        else
        {
            yield return EnableContinueButtonRoutine();
        }
        
        // Animation complete - button is now clickable (if no enemies are active)
        isAnimating = false;
    }

    private IEnumerator EnableContinueButtonRoutine()
    {
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
            
            // CRITICAL: Check if enemies are still active before enabling button
            // Exception: On defeat, always enable the button (player has lost, let them continue)
            bool canProceed = true;
            if (isShowingVictory && EnemySystem.Instance != null && EnemySystem.Instance.EnemyViews != null)
            {
                int activeEnemies = EnemySystem.Instance.EnemyViews.Count;
                if (activeEnemies > 0)
                {
                    Debug.LogWarning($"[VictoryDefeatUI] Cannot enable continue button! {activeEnemies} enemy(ies) still active on board.");
                    canProceed = false;
                }
            }
            
            continueButton.interactable = canProceed;
            if (continueButtonCanvasGroup != null)
            {
                yield return continueButtonCanvasGroup.DOFade(1f, buttonFadeInDuration).SetEase(buttonFadeInEase).WaitForCompletion();
            }
        }
    }

    /// <summary>
    /// Called when continue button is clicked
    /// Either shows reward chest or transitions to map selection
    /// </summary>
    private void OnContinueButtonClicked()
    {
        // Guard 1: If RewardChestUI is currently active, ignore VictoryDefeatUI continue clicks
        if (rewardChestUI != null && rewardChestUI.IsShowing)
        {
            Debug.Log("[VictoryDefeatUI] Ignoring continue click because RewardChestUI is currently active.");
            return;
        }

        // Guard 2: If the Victory/Defeat banner panel is hidden, ignore continue clicks
        if (bannerPanel != null && !bannerPanel.gameObject.activeSelf)
        {
            Debug.Log("[VictoryDefeatUI] Ignoring continue click because banner panel is hidden.");
            return;
        }

        if (continueButton != null)
        {
            continueButton.interactable = false; // Prevent multiple clicks
        }
        
        // CRITICAL SAFETY CHECK: Verify no enemies are active before proceeding (only on Victory)
        if (isShowingVictory && EnemySystem.Instance != null && EnemySystem.Instance.EnemyViews != null)
        {
            int activeEnemies = EnemySystem.Instance.EnemyViews.Count;
            if (activeEnemies > 0)
            {
                Debug.LogWarning($"[VictoryDefeatUI] CRITICAL: Cannot proceed! {activeEnemies} enemy(ies) still active on board. Re-enabling continue button.");
                if (continueButton != null)
                {
                    continueButton.interactable = true; // Re-enable button
                }
                return;
            }
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
            // BUT: Double-check that there are truly no enemies before transitioning
            bool hasEnemiesRemaining = false;
            if (EnemySystem.Instance != null)
            {
                bool hasEnemiesInQueue = EnemySystem.Instance.HasRemainingEnemies;
                int activeEnemiesOnBoard = (EnemySystem.Instance.EnemyViews != null) ? EnemySystem.Instance.EnemyViews.Count : 0;
                hasEnemiesRemaining = hasEnemiesInQueue || activeEnemiesOnBoard > 0;
                
                if (isShowingVictory && hasEnemiesRemaining)
                {
                    Debug.LogWarning($"[VictoryDefeatUI] CRITICAL: Attempted final victory transition but enemies still exist! Queue: {hasEnemiesInQueue}, Active: {activeEnemiesOnBoard}. Aborting transition.");
                    if (continueButton != null)
                    {
                        continueButton.interactable = true; // Re-enable button
                    }
                    return;
                }
            }
            
            // Safe to transition - all enemies defeated
            TransitionToMapSelection();
        }
        else if (pendingRewardData == null && rewardCollectedCallback != null)
        {
            // No reward data but callback exists = more enemies to spawn
            // This should NOT transition - instead call the callback to spawn next enemy
            Debug.Log("[VictoryDefeatUI] No reward data but callback exists - spawning next enemy instead of transitioning");
            
            // Store callback before clearing
            System.Action callback = rewardCollectedCallback;
            
            // Clear state
            rewardCollectedCallback = null;
            hasMoreEnemies = false;
            isFirstReward = true;
            isShowingVictory = false; // Reset flag to allow card interactions again
            HideBanner(); // Ensure banner is fully hidden
            
            // Call callback to spawn next enemy (NOT transition!)
            callback?.Invoke();
        }
        else
        {
            // Edge case: reward data exists but no rewardChestUI (or reward object missing)
            Debug.LogWarning("[VictoryDefeatUI] Continue clicked but reward state is inconsistent. Reward data exists but no rewardChestUI.", this);
            
            System.Action callback = rewardCollectedCallback;
            rewardCollectedCallback = null;
            pendingRewardData = null;
            hasMoreEnemies = false;
            isFirstReward = true;
            isShowingVictory = false;
            HideBanner();
            
            if (callback != null)
            {
                Debug.Log("[VictoryDefeatUI] Invoking stored callback to ensure next enemy encounter spawns.");
                callback.Invoke();
            }
            else
            {
                TransitionToMapSelection();
            }
        }
    }

    /// <summary>
    /// Called when reward chest collection is complete
    /// </summary>
    private void OnRewardChestComplete()
    {
        Debug.Log($"[VictoryDefeatUI] OnRewardChestComplete called. PendingReward: {(pendingRewardData != null ? pendingRewardData.name : "NULL")}, IsFirstReward: {isFirstReward}");
        
        // Safety check: Log if enemies are still present, but allow progression since enemy was defeated
        if (EnemySystem.Instance != null && EnemySystem.Instance.EnemyViews != null)
        {
            int activeEnemies = EnemySystem.Instance.EnemyViews.Count;
            if (activeEnemies > 0)
            {
                Debug.LogWarning($"[VictoryDefeatUI] Notice: {activeEnemies} enemy view(s) still in list during reward collection completion.");
            }
        }
        
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
        
        // CRITICAL: Double-check that there are no enemies before clearing state
        // If callback exists, it means more enemies should spawn - don't clear it yet
        bool actuallyHasMoreEnemies = hadMoreEnemies;
        if (EnemySystem.Instance != null)
        {
            // Check queue and active enemies
            bool hasEnemiesInQueue = EnemySystem.Instance.HasRemainingEnemies;
            int activeEnemiesOnBoard = (EnemySystem.Instance.EnemyViews != null) ? EnemySystem.Instance.EnemyViews.Count : 0;
            actuallyHasMoreEnemies = hasEnemiesInQueue || activeEnemiesOnBoard > 0;
            
            if (actuallyHasMoreEnemies && callback == null)
            {
                Debug.LogWarning($"[VictoryDefeatUI] CRITICAL: More enemies detected (queue: {hasEnemiesInQueue}, active: {activeEnemiesOnBoard}) but no callback! This should not happen.");
            }
        }
        
        // Clear reward state immediately to prevent double-triggering
        pendingRewardData = null;
        rewardCollectedCallback = null;
        hasMoreEnemies = false;
        isFirstReward = true;
        
        // CRITICAL: Reset victory flag when continuing to fight next enemy
        // If callback exists, we're continuing combat - reset the flag so cards can be used again
        if (callback != null)
        {
            isShowingVictory = false; // Reset flag to allow card interactions again
            HideBanner(); // Ensure banner is fully hidden
            Debug.Log("[VictoryDefeatUI] Reset victory flag - continuing to fight next enemy");
        }
        
        // Call the callback (which will either spawn next enemy or trigger final victory)
        // Only call if callback exists - if it doesn't exist and there are more enemies, something is wrong
        if (callback != null)
        {
            Debug.Log($"[VictoryDefeatUI] Calling reward collected callback to spawn next enemy or trigger final victory");
            callback.Invoke();
        }
        else if (actuallyHasMoreEnemies)
        {
            Debug.LogError($"[VictoryDefeatUI] CRITICAL: More enemies exist but no callback to spawn them! This is a bug.");
            // Even if callback is null, if we're continuing combat, reset the flag
            isShowingVictory = false;
            HideBanner();
        }
    }
    
    /// <summary>
    /// Processes reward collection by adding to appropriate collection (special card or relic)
    /// </summary>
    /// <param name="rewardData">The reward data from the chest</param>
    /// <param name="isMiniBossReward">True if this is a mini-boss reward (special card), false for main boss (relic)</param>
    private void ProcessRewardCollection(RewardData rewardData, bool isMiniBossReward)
    {
        Debug.Log($"[VictoryDefeatUI] ProcessRewardCollection called. RewardData: {(rewardData != null ? rewardData.name : "NULL")}, IsMiniBoss: {isMiniBossReward}");
        
        if (rewardData == null)
        {
            Debug.LogWarning("[VictoryDefeatUI] Cannot process null reward data.");
            return;
        }
        
        if (isMiniBossReward)
        {
            // Mini-boss reward: collect special card
            SpecialCardData specialCard = rewardData.SpecialCardReward;
            Debug.Log($"[VictoryDefeatUI] Processing mini-boss reward. SpecialCard: {(specialCard != null ? specialCard.name : "NULL")}, Collection: {(specialCardCollection != null ? "ASSIGNED" : "NULL")}");
            
            if (specialCard != null && specialCardCollection != null)
            {
                // Gather active hero names from HeroSystem
                List<string> activeHeroNames = new List<string>();
                if (HeroSystem.Instance != null && HeroSystem.Instance.HeroViews != null)
                {
                    foreach (var heroView in HeroSystem.Instance.HeroViews)
                    {
                        if (heroView != null && heroView.HeroData != null && !string.IsNullOrEmpty(heroView.HeroData.HeroName))
                        {
                            activeHeroNames.Add(heroView.HeroData.HeroName);
                        }
                    }
                }
                
                bool added = false;
                if (activeHeroNames.Count > 0)
                {
                    added = specialCardCollection.AddSpecialCardToRandomHero(specialCard, activeHeroNames);
                }
                else
                {
                    Debug.LogWarning($"[VictoryDefeatUI] Could not assign special card '{specialCard.CardName}': No active hero views found in HeroSystem.");
                }
                
                Debug.Log($"[VictoryDefeatUI] Collected special card: {specialCard.CardName} (Assigned/Added: {added})");
                
                // Notify SpecialCardPanelUI to refresh if card was successfully added
                if (added)
                {
                    NotifySpecialCardPanelUI();
                }
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
            Debug.Log($"[VictoryDefeatUI] Processing main boss reward. Relic: {(relic != null ? relic.name : "NULL")}, Collection: {(relicCollection != null ? "ASSIGNED" : "NULL")}");
            
            if (relic != null && relicCollection != null)
            {
                bool added = relicCollection.AddRelic(relic);
                Debug.Log($"[VictoryDefeatUI] Collected relic: {relic.RelicName} (Added: {added})");
                
                // Notify RelicDisplayUI to refresh if relic was successfully added
                if (added)
                {
                    NotifyRelicDisplayUI();
                }
            }
            else
            {
                if (relic == null)
                {
                    Debug.LogWarning("[VictoryDefeatUI] RewardData.AssociatedRelic is NULL! Cannot collect relic.");
                }
                if (relicCollection == null)
                {
                    Debug.LogWarning("[VictoryDefeatUI] RelicCollectionData not assigned, cannot save relic!");
                }
            }
        }
    }
    
    /// <summary>
    /// Notifies RelicDisplayUI to refresh its display when a new relic is collected
    /// </summary>
    private void NotifyRelicDisplayUI()
    {
        // Find RelicDisplayUI in the scene and notify it
        RelicDisplayUI relicDisplay = Object.FindFirstObjectByType<RelicDisplayUI>();
        if (relicDisplay != null)
        {
            relicDisplay.OnRelicCollected();
            Debug.Log("[VictoryDefeatUI] Notified RelicDisplayUI to refresh display.");
        }
        else
        {
            Debug.LogWarning("[VictoryDefeatUI] RelicDisplayUI not found in scene! Relic display may not update.");
        }
    }
    
    /// <summary>
    /// Notifies SpecialCardPanelUI to refresh its display when a new special card is collected
    /// </summary>
    private void NotifySpecialCardPanelUI()
    {
        // Find SpecialCardPanelUI in the scene and notify it
        SpecialCardPanelUI specialCardPanel = Object.FindFirstObjectByType<SpecialCardPanelUI>();
        if (specialCardPanel != null)
        {
            specialCardPanel.RefreshDisplay();
            Debug.Log("[VictoryDefeatUI] Notified SpecialCardPanelUI to refresh display.");
        }
        else
        {
            Debug.LogWarning("[VictoryDefeatUI] SpecialCardPanelUI not found in scene! Special card display may not update.");
        }
    }
    
    /// <summary>
    /// Marks the current map as complete in the game progress
    /// </summary>
    private void MarkCurrentMapComplete()
    {
        MarkCurrentMapCompleteInternal();
    }
    
    /// <summary>
    /// Internal method to mark map complete (can be called from EnemySystem)
    /// </summary>
    public void MarkCurrentMapCompleteInternal()
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
        Debug.Log($"[VictoryDefeatUI] Attempting to mark map complete. MapId: '{mapId}'");
        
        // Verify mapId matches expected format (with spaces as they appear in MapData)
        bool isValidMapId = mapId == GameProgressData.MAP_ID_DAGAT || 
                           mapId == GameProgressData.MAP_ID_DARAGANG || 
                           mapId == GameProgressData.MAP_ID_BUNDOK || 
                           mapId == GameProgressData.MAP_ID_KALUWALHATIAN;
        
        if (!isValidMapId)
        {
            Debug.LogWarning($"[VictoryDefeatUI] MapId '{mapId}' does not match expected format! Expected: '{GameProgressData.MAP_ID_DAGAT}', '{GameProgressData.MAP_ID_DARAGANG}', '{GameProgressData.MAP_ID_BUNDOK}', or '{GameProgressData.MAP_ID_KALUWALHATIAN}'");
        }
        
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
    /// Checks if the game is completed (Kaluwalhatian defeated)
    /// </summary>
    public bool IsGameCompleted()
    {
        if (gameProgress == null)
        {
            return false;
        }
        return gameProgress.IsGameCompleted;
    }

    /// <summary>
    /// Transitions back to map selection scene
    /// </summary>
    private void TransitionToMapSelection()
    {
        // CRITICAL SAFETY CHECK: Verify no enemies are active before transitioning (only on victory)
        if (isShowingVictory && EnemySystem.Instance != null && EnemySystem.Instance.EnemyViews != null)
        {
            int activeEnemies = EnemySystem.Instance.EnemyViews.Count;
            if (activeEnemies > 0)
            {
                Debug.LogWarning($"[VictoryDefeatUI] CRITICAL: Cannot transition! {activeEnemies} enemy(ies) still active on board. Aborting transition.");
                // Re-enable continue button so player can try again
                if (continueButton != null)
                {
                    continueButton.interactable = true;
                }
                return;
            }
        }
        
        // Transition back to map selection using SceneController
        if (SceneController.Instance != null)
        {
            Debug.Log("[VictoryDefeatUI] Transitioning to map selection scene");
            
            // Mark that we should trigger post-combat dialogue when MapSelection loads
            MapSelectManager2.MarkShouldTriggerPostCombatDialogue();
            
            var transition = SceneController.Instance
                .NewTransition()
                .Unload(SceneDatabase.Slots.SessionContent)
                .Load(SceneDatabase.Slots.SessionContent, SceneDatabase.Scenes.MapSelection, setActive: true)
                .WithLoadingVideo("loading");
            
            // Play map selection music with smooth fade
            if (mapSelectionMusic != null)
            {
                transition = transition.WithMusic(mapSelectionMusic, MusicFadeTime);
            }
            
            transition.Perform();
        }
        else
        {
            Debug.LogWarning("[VictoryDefeatUI] SceneController.Instance is null! Cannot transition to map selection. (This is expected during testing phase)", this);
            // During testing, just log a warning instead of error
        }
    }

    /// <summary>
    /// Immediately hides the banner
    /// </summary>
    private void HideBanner()
    {
        // Reset animation flag to ensure clean state
        isAnimating = false;
        
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
    
    /// <summary>
    /// Returns true if victory or defeat banner is currently showing
    /// </summary>
    /// <remarks>
    /// Used by other systems to check if combat has ended and prevent further actions
    /// </remarks>
    public bool IsShowingResult => isShowingVictory || (bannerPanel != null && bannerPanel.gameObject.activeSelf);
}

