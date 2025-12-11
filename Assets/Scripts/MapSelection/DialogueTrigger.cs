using System.Collections.Generic;
using UnityEngine;
 
[System.Serializable]
public class DialogueCharacter
{
    public string name;
    public Sprite icon;
}
 
[System.Serializable]
public class DialogueLine
{
    public DialogueCharacter character;
    [TextArea(3, 10)]
    public string line;
    
    [Header("Image Overlay (Optional)")]
    [Tooltip("Optional image to display as an overlay during this dialogue line")]
    public Sprite overlayImage;
    
    [Tooltip("Duration to display the overlay image (0 = until next line)")]
    public float overlayDuration = 0f;
    
    [Tooltip("Fade in/out duration for the overlay")]
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