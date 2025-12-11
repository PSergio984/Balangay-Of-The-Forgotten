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
    public Image characterIcon;
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI dialogueArea;
 
    [Header("Image Overlay")]
    [Tooltip("Image component for displaying overlay images during dialogue")]
    [SerializeField] private Image overlayImage;
    
    [Tooltip("Canvas group for fading the overlay")]
    [SerializeField] private CanvasGroup overlayCanvasGroup;
    
    [Tooltip("Container panel for the overlay (for activation)")]
    [SerializeField] private GameObject overlayPanel;
    
    private Queue<DialogueLine> lines;
    
    public bool isDialogueActive = false;
 
    public float typingSpeed = 0.2f;
 
    public Animator animator;
    
    // Current dialogue reference
    private Dialogue currentDialogue;
    
    // Event for dialogue completion
    public System.Action OnDialogueComplete;
 
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
 
        lines = new Queue<DialogueLine>();
        
        // Hide overlay initially
        if (overlayPanel != null)
        {
            overlayPanel.SetActive(false);
        }
    }
 
    public void StartDialogue(Dialogue dialogue)
    {
        isDialogueActive = true;
        currentDialogue = dialogue;
 
        animator.Play("show");
 
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
 
        characterIcon.sprite = currentLine.character.icon;
        characterName.text = currentLine.character.name;
        
        // Handle overlay image
        HandleOverlayImage(currentLine);
 
        StopAllCoroutines();
 
        StartCoroutine(TypeSentence(currentLine));
    }
    
    /// <summary>
    /// Handles showing/hiding overlay image for a dialogue line
    /// </summary>
    private void HandleOverlayImage(DialogueLine line)
    {
        if (line.overlayImage != null)
        {
            ShowOverlay(line.overlayImage, line.overlayFadeDuration);
            
            // If duration is set, auto-hide after duration
            if (line.overlayDuration > 0)
            {
                StartCoroutine(AutoHideOverlay(line.overlayDuration, line.overlayFadeDuration));
            }
        }
        else
        {
            // No overlay for this line, hide if visible
            HideOverlay(0.2f);
        }
    }
    
    /// <summary>
    /// Shows an overlay image with fade in
    /// </summary>
    private void ShowOverlay(Sprite sprite, float fadeDuration)
    {
        if (overlayImage == null || overlayPanel == null)
        {
            return;
        }
        
        overlayImage.sprite = sprite;
        overlayPanel.SetActive(true);
        
        if (overlayCanvasGroup != null)
        {
            overlayCanvasGroup.alpha = 0f;
            overlayCanvasGroup.DOFade(1f, fadeDuration);
        }
    }
    
    /// <summary>
    /// Hides the overlay with fade out
    /// </summary>
    private void HideOverlay(float fadeDuration)
    {
        if (overlayPanel == null || !overlayPanel.activeSelf)
        {
            return;
        }
        
        if (overlayCanvasGroup != null)
        {
            overlayCanvasGroup.DOFade(0f, fadeDuration).OnComplete(() =>
            {
                overlayPanel.SetActive(false);
            });
        }
        else
        {
            overlayPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Auto-hides overlay after a duration
    /// </summary>
    private IEnumerator AutoHideOverlay(float delay, float fadeDuration)
    {
        yield return new WaitForSeconds(delay);
        HideOverlay(fadeDuration);
    }
 
    IEnumerator TypeSentence(DialogueLine dialogueLine)
    {
        dialogueArea.text = "";
        foreach (char letter in dialogueLine.line.ToCharArray())
        {
            dialogueArea.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }
 
    void EndDialogue()
    {
        isDialogueActive = false;
        
        // Hide overlay
        HideOverlay(0.3f);
        
        animator.Play("hide");
        
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
                Debug.Log("[DialogueManager] Post-final-boss dialogue completed.");
                // Could trigger credits or ending sequence
                break;
                
            case DialogueType.Normal:
            default:
                // No special handling
                break;
        }
    }
    
    /// <summary>
    /// Immediately displays an overlay image (for scripted sequences)
    /// </summary>
    public void ShowImageOverlay(Sprite image, float fadeDuration = 0.3f)
    {
        ShowOverlay(image, fadeDuration);
    }
    
    /// <summary>
    /// Hides the current overlay image
    /// </summary>
    public void HideImageOverlay(float fadeDuration = 0.3f)
    {
        HideOverlay(fadeDuration);
    }
}