using System.Collections.Generic;
using UnityEngine;
 
[System.Serializable]
public class DialogueCharacter
{
    public string name;
    public Sprite icon;

    [Header("Animated Portrait (Optional)")]
    [Tooltip("Prefab with Animator/Animation for animated portrait. If set, will be used instead of static icon.")]
    public GameObject animatedPortraitPrefab;
}
 
[System.Serializable]
public class DialogueLine
{
    public DialogueCharacter character;
    [TextArea(3, 10)]
    public string line;
    
    [Header("Typing Settings")]
    [Tooltip("Typing speed for this line (seconds per character). If 0, uses DialogueManager default.")]
    public float typingSpeed = 0f;
    
    [Header("Image Overlay (Optional)")]
    [Tooltip("Reference to the overlay GameObject in hierarchy. You manually place and size this in Unity.")]
    public GameObject overlayImageObject;
    
    [Tooltip("Duration to display the overlay image (0 = until next line)")]
    public float overlayDuration = 0f;
    
    [Tooltip("Scale animation duration for the overlay")]
    public float overlayFadeDuration = 0.3f;
}
 
[System.Serializable]
public class Dialogue
{
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
    
    [Header("Dialogue Settings")]
    [Tooltip("Optional callback event name to invoke when dialogue ends")]
    public string onCompleteEventName;
    
    [Tooltip("Type of dialogue for triggering specific behaviors")]
    public DialogueType dialogueType = DialogueType.Normal;
}

/// <summary>
/// Types of dialogue for special handling
/// </summary>
public enum DialogueType
{
    Normal,
    IntroScene,
    PostVictory,
    PostFinalBoss
}
 
public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;
 
    public void TriggerDialogue()
    {
        DialogueManager.Instance.StartDialogue(dialogue);
    }
 
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Player")
        {
            TriggerDialogue();
        }
    }
}