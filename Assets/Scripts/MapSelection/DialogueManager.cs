using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using DG.Tweening;
 
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("Dialogue UI")]
    [Tooltip("Main dialogue panel that contains all dialogue UI elements (for slide animation)")]
    public RectTransform dialoguePanel;
    public Image characterIcon;
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI dialogueArea;
    
    [Header("Continue Button")]
    [Tooltip("Continue button that appears after typing completes")]
    public GameObject continueButton;

    [Header("Animated Portrait")] 
    [Tooltip("Parent transform for animated portrait prefab instance")] 
    public Transform animatedPortraitParent;
    private GameObject currentAnimatedPortraitInstance;
    
    [Header("Glow Component")]
    [Tooltip("Glow GameObject that should be hidden when there's no overlay image")]
    public GameObject glowComponent;

    [Header("Animation Settings")]
    [Tooltip("Duration for dialogue panel slide animation")]
    [SerializeField] private float dialogueSlideDuration = 0.5f;
    
    [Tooltip("Ease type for dialogue panel animation")]
    [SerializeField] private Ease dialogueSlideEase = Ease.OutBack;
    
    [Tooltip("Duration for individual UI elements (portrait, name, text) slide-up animation")]
    [SerializeField] private float elementSlideDuration = 0.4f;
    
    [Tooltip("Ease type for element slide animation")]
    [SerializeField] private Ease elementSlideEase = Ease.OutCubic;

    // Currently active overlay GameObject
    private GameObject currentOverlayObject;
    
    // Store original scale of overlay objects
    private Dictionary<GameObject, Vector3> overlayOriginalScales = new Dictionary<GameObject, Vector3>();
    
    // Track typing completion
    private bool isTypingComplete = false;

    private Queue<DialogueLine> lines;

    public bool isDialogueActive = false;

    public float typingSpeed = 0.2f;

    // Store initial dialogue panel position
    private Vector2 dialoguePanelInitialPosition;
    private bool dialoguePanelPositionStored = false;
    
    // Store initial positions for UI elements
    private Vector2 characterIconInitialPos;
    private Vector2 characterNameInitialPos;
    private Vector2 dialogueAreaInitialPos;
    private bool elementPositionsStored = false;

    // Current dialogue reference
    private Dialogue currentDialogue;

    // Event for dialogue completion
    public System.Action OnDialogueComplete;
 
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
 
        lines = new Queue<DialogueLine>();
        
        // Store initial dialogue panel position for slide animation
        if (dialoguePanel != null && !dialoguePanelPositionStored)
        {
            dialoguePanelInitialPosition = dialoguePanel.anchoredPosition;
            dialoguePanelPositionStored = true;
            // Start hidden below screen
            dialoguePanel.gameObject.SetActive(false);
        }
        
        // Store initial positions for UI elements
        if (!elementPositionsStored)
        {
            if (characterIcon != null)
            {
                RectTransform iconRect = characterIcon.GetComponent<RectTransform>();
                if (iconRect != null)
                    characterIconInitialPos = iconRect.anchoredPosition;
            }
            if (characterName != null)
            {
                RectTransform nameRect = characterName.GetComponent<RectTransform>();
                if (nameRect != null)
                    characterNameInitialPos = nameRect.anchoredPosition;
            }
            if (dialogueArea != null)
            {
                RectTransform areaRect = dialogueArea.GetComponent<RectTransform>();
                if (areaRect != null)
                    dialogueAreaInitialPos = areaRect.anchoredPosition;
            }
            elementPositionsStored = true;
        }
        
        // Hide continue button initially
        if (continueButton != null)
        {
            continueButton.SetActive(false);
        }
        
        // Hide glow initially
        if (glowComponent != null)
        {
            glowComponent.SetActive(false);
        }
    }
 
    public void StartDialogue(Dialogue dialogue)
    {
        isDialogueActive = true;
        currentDialogue = dialogue;

        // Animate dialogue panel sliding up from bottom
        ShowDialoguePanel();

        lines.Clear();

        foreach (DialogueLine dialogueLine in dialogue.dialogueLines)
        {
            lines.Enqueue(dialogueLine);
        }

        DisplayNextDialogueLine();
    }
    
    /// <summary>
    /// Starts dialogue with a completion callback
    /// </summary>
    public void StartDialogue(Dialogue dialogue, System.Action onComplete)
    {
        OnDialogueComplete = onComplete;
        StartDialogue(dialogue);
    }
 
    public void DisplayNextDialogueLine()
    {
        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine currentLine = lines.Dequeue();

        // Hide continue button while typing
        if (continueButton != null)
        {
            continueButton.SetActive(false);
        }
        isTypingComplete = false;

        // Handle animated portrait or static icon with slide-up animation
        SetupCharacterPortrait(currentLine.character);
        
        // Animate character name sliding up
        AnimateCharacterName(currentLine.character.name);

        // Handle overlay image (scale animation, not slide)
        HandleOverlayImage(currentLine);

        StopAllCoroutines();

        // Start typing with custom speed if set
        float speed = currentLine.typingSpeed > 0 ? currentLine.typingSpeed : typingSpeed;
        StartCoroutine(TypeSentence(currentLine, speed));
    }
    
    /// <summary>
    /// Called by continue button to advance to next dialogue line
    /// </summary>
    public void OnContinueButtonClicked()
    {
        if (isTypingComplete)
        {
            DisplayNextDialogueLine();
        }
    }

    /// <summary>
    /// Sets up the character portrait: animated prefab if present, else static icon
    /// Animates sliding up from bottom
    /// </summary>
    private void SetupCharacterPortrait(DialogueCharacter character)
    {
        // Remove previous animated portrait if any
        if (currentAnimatedPortraitInstance != null)
        {
            Destroy(currentAnimatedPortraitInstance);
            currentAnimatedPortraitInstance = null;
        }

        if (character.animatedPortraitPrefab != null && animatedPortraitParent != null)
        {
            // Hide static icon
            if (characterIcon != null)
                characterIcon.gameObject.SetActive(false);

            // Instantiate animated portrait prefab
            currentAnimatedPortraitInstance = Instantiate(character.animatedPortraitPrefab, animatedPortraitParent);
            currentAnimatedPortraitInstance.transform.localPosition = Vector3.zero;
            currentAnimatedPortraitInstance.transform.localRotation = Quaternion.identity;
            currentAnimatedPortraitInstance.transform.localScale = Vector3.one;
            
            // Animate slide up from bottom
            RectTransform portraitRect = currentAnimatedPortraitInstance.GetComponent<RectTransform>();
            if (portraitRect == null)
            {
                // If no RectTransform, try to get from parent or create one
                portraitRect = currentAnimatedPortraitInstance.AddComponent<RectTransform>();
            }
            if (portraitRect != null)
            {
                Vector2 startPos = portraitRect.anchoredPosition;
                startPos.y -= 200f; // Start below
                portraitRect.anchoredPosition = startPos;
                portraitRect.DOAnchorPos(Vector2.zero, elementSlideDuration).SetEase(elementSlideEase);
            }
        }
        else
        {
            // Show static icon with slide-up animation
            if (characterIcon != null)
            {
                characterIcon.sprite = character.icon;
                characterIcon.gameObject.SetActive(true);
                
                RectTransform iconRect = characterIcon.GetComponent<RectTransform>();
                if (iconRect != null)
                {
                    Vector2 startPos = characterIconInitialPos;
                    startPos.y -= 200f; // Start below
                    iconRect.anchoredPosition = startPos;
                    iconRect.DOAnchorPos(characterIconInitialPos, elementSlideDuration).SetEase(elementSlideEase);
                }
            }
        }
    }
    
    /// <summary>
    /// Animates character name sliding up from bottom
    /// </summary>
    private void AnimateCharacterName(string name)
    {
        if (characterName != null)
        {
            characterName.text = name;
            
            RectTransform nameRect = characterName.GetComponent<RectTransform>();
            if (nameRect != null)
            {
                Vector2 startPos = characterNameInitialPos;
                startPos.y -= 150f; // Start below
                nameRect.anchoredPosition = startPos;
                nameRect.DOAnchorPos(characterNameInitialPos, elementSlideDuration).SetEase(elementSlideEase);
            }
        }
    }
    
    /// <summary>
    /// Handles showing/hiding overlay image for a dialogue line
    /// Uses pre-placed GameObjects from hierarchy
    /// </summary>
    private void HandleOverlayImage(DialogueLine line)
    {
        // Hide previous overlay if any
        if (currentOverlayObject != null)
        {
            HideOverlayObject(currentOverlayObject, 0.2f);
            currentOverlayObject = null;
        }

        // Show overlay if assigned
        if (line.overlayImageObject != null)
        {
            currentOverlayObject = line.overlayImageObject;
            ShowOverlayObject(currentOverlayObject, line.overlayFadeDuration);
            
            // Show glow component when overlay is present
            if (glowComponent != null)
            {
                glowComponent.SetActive(true);
            }

            // If duration is set, auto-hide after duration
            if (line.overlayDuration > 0)
            {
                StartCoroutine(AutoHideOverlayObject(currentOverlayObject, line.overlayDuration, line.overlayFadeDuration));
            }
        }
        else
        {
            // Hide glow component when no overlay
            if (glowComponent != null)
            {
                glowComponent.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Shows an overlay GameObject with scale animation (0 to original scale)
    /// Preserves the scale you set in the Unity scene
    /// </summary>
    private void ShowOverlayObject(GameObject overlayObj, float duration)
    {
        if (overlayObj == null)
            return;

        // Store original scale if not already stored
        if (!overlayOriginalScales.ContainsKey(overlayObj))
        {
            overlayOriginalScales[overlayObj] = overlayObj.transform.localScale;
        }
        
        Vector3 targetScale = overlayOriginalScales[overlayObj];
        
        overlayObj.SetActive(true);
        
        // Scale from 0 to original scale
        overlayObj.transform.localScale = Vector3.zero;
        overlayObj.transform.DOScale(targetScale, duration).SetEase(Ease.OutBack);
    }
    
    /// <summary>
    /// Hides an overlay GameObject with scale animation (original scale to 0)
    /// </summary>
    private void HideOverlayObject(GameObject overlayObj, float duration)
    {
        if (overlayObj == null || !overlayObj.activeSelf)
            return;

        // Get original scale if stored, otherwise use current scale
        Vector3 originalScale = overlayOriginalScales.ContainsKey(overlayObj) 
            ? overlayOriginalScales[overlayObj] 
            : overlayObj.transform.localScale;

        // Scale down to 0
        overlayObj.transform.DOScale(Vector3.zero, duration)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                if (overlayObj != null)
                {
                    overlayObj.SetActive(false);
                    // Restore original scale for next time
                    if (overlayOriginalScales.ContainsKey(overlayObj))
                    {
                        overlayObj.transform.localScale = overlayOriginalScales[overlayObj];
                    }
                }
            });
    }
    
    /// <summary>
    /// Auto-hides overlay after a duration
    /// </summary>
    private IEnumerator AutoHideOverlayObject(GameObject overlayObj, float delay, float fadeDuration)
    {
        yield return new WaitForSeconds(delay);
        HideOverlayObject(overlayObj, fadeDuration);
    }
 
    IEnumerator TypeSentence(DialogueLine dialogueLine, float speed)
    {
        // Animate dialogue area sliding up from bottom
        if (dialogueArea != null)
        {
            RectTransform areaRect = dialogueArea.GetComponent<RectTransform>();
            if (areaRect != null)
            {
                Vector2 startPos = dialogueAreaInitialPos;
                startPos.y -= 100f; // Start below
                areaRect.anchoredPosition = startPos;
                areaRect.DOAnchorPos(dialogueAreaInitialPos, elementSlideDuration).SetEase(elementSlideEase);
            }
        }
        
        dialogueArea.text = "";
        foreach (char letter in dialogueLine.line.ToCharArray())
        {
            dialogueArea.text += letter;
            yield return new WaitForSeconds(speed);
        }
        
        // Typing complete - show continue button
        isTypingComplete = true;
        if (continueButton != null)
        {
            continueButton.SetActive(true);
        }
    }
 
    void EndDialogue()
    {
        isDialogueActive = false;
        isTypingComplete = false;

        // Hide continue button
        if (continueButton != null)
        {
            continueButton.SetActive(false);
        }

        // Hide overlay if active
        if (currentOverlayObject != null)
        {
            HideOverlayObject(currentOverlayObject, 0.3f);
            currentOverlayObject = null;
        }
        
        // Hide glow component
        if (glowComponent != null)
        {
            glowComponent.SetActive(false);
        }

        // Remove animated portrait if any
        if (currentAnimatedPortraitInstance != null)
        {
            Destroy(currentAnimatedPortraitInstance);
            currentAnimatedPortraitInstance = null;
        }

        // Animate dialogue panel sliding down
        HideDialoguePanel();

        // Handle dialogue type specific behavior
        if (currentDialogue != null)
        {
            HandleDialogueTypeCompletion(currentDialogue.dialogueType);
        }

        // Invoke completion callback
        OnDialogueComplete?.Invoke();
        OnDialogueComplete = null;

        currentDialogue = null;
    }
    
    /// <summary>
    /// Handles specific behaviors based on dialogue type
    /// </summary>
    private void HandleDialogueTypeCompletion(DialogueType type)
    {
        switch (type)
        {
            case DialogueType.IntroScene:
                Debug.Log("[DialogueManager] Intro dialogue completed.");
                // Could trigger scene transition here
                break;
                
            case DialogueType.PostVictory:
                Debug.Log("[DialogueManager] Post-victory dialogue completed.");
                // Could trigger return to map selection
                break;
                
            case DialogueType.PostFinalBoss:
                Debug.Log("[DialogueManager] Post-final-boss dialogue completed. Loading credits scene...");
                // Navigate to credits scene using SceneController
                LoadCreditsScene();
                break;
                
            case DialogueType.Normal:
            default:
                // No special handling
                break;
        }
    }
    
    /// <summary>
    /// Immediately displays an overlay GameObject (for scripted sequences)
    /// </summary>
    public void ShowImageOverlay(GameObject overlayObj, float fadeDuration = 0.3f)
    {
        if (currentOverlayObject != null)
        {
            HideOverlayObject(currentOverlayObject, 0.2f);
        }
        currentOverlayObject = overlayObj;
        ShowOverlayObject(overlayObj, fadeDuration);
    }
    
    /// <summary>
    /// Hides the current overlay GameObject
    /// </summary>
    public void HideImageOverlay(float fadeDuration = 0.3f)
    {
        if (currentOverlayObject != null)
        {
            HideOverlayObject(currentOverlayObject, fadeDuration);
            currentOverlayObject = null;
        }
    }
    
    /// <summary>
    /// Shows the dialogue panel with slide-up animation
    /// Uses the manually set position you configured in Unity
    /// </summary>
    private void ShowDialoguePanel()
    {
        if (dialoguePanel == null)
        {
            Debug.LogWarning("[DialogueManager] Dialogue panel not assigned. Please assign it in the Inspector.");
            return;
        }

        // Store the target position (where you manually positioned it)
        if (!dialoguePanelPositionStored)
        {
            dialoguePanelInitialPosition = dialoguePanel.anchoredPosition;
            dialoguePanelPositionStored = true;
        }

        // Calculate start position below screen (move down by panel height + some padding)
        Vector2 startPosition = dialoguePanelInitialPosition;
        float panelHeight = dialoguePanel.rect.height;
        startPosition.y = dialoguePanelInitialPosition.y - panelHeight - 100f; // Start below screen

        // Set initial position and activate
        dialoguePanel.anchoredPosition = startPosition;
        dialoguePanel.gameObject.SetActive(true);

        // Animate slide up to your manually set position
        dialoguePanel.DOAnchorPos(dialoguePanelInitialPosition, dialogueSlideDuration)
            .SetEase(dialogueSlideEase);
    }
    
    /// <summary>
    /// Hides the dialogue panel with slide-down animation
    /// </summary>
    private void HideDialoguePanel()
    {
        if (dialoguePanel == null)
        {
            return;
        }

        // Calculate end position below screen
        Vector2 endPosition = dialoguePanelInitialPosition;
        float panelHeight = dialoguePanel.rect.height;
        endPosition.y = dialoguePanelInitialPosition.y - panelHeight - 100f;

        // Animate slide down
        dialoguePanel.DOAnchorPos(endPosition, dialogueSlideDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                dialoguePanel.gameObject.SetActive(false);
            });
    }
    
    /// <summary>
    /// Loads the credits scene with white-to-black fade transition
    /// </summary>
    private void LoadCreditsScene()
    {
        if (SceneController.Instance == null)
        {
            Debug.LogError("[DialogueManager] SceneController.Instance is null! Cannot load credits scene.");
            return;
        }
        
        // Use SceneController to transition to credits with white fade
        SceneController.Instance
            .NewTransition()
            .Unload(SceneDatabase.Slots.SessionContent)
            .Unload(SceneDatabase.Slots.Session)
            .Load(SceneDatabase.Slots.LoadingScreen, SceneDatabase.Scenes.Credits, setActive: true)
            .WithWhiteFade()
            .Perform();
    }
}