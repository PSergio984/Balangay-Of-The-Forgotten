using UnityEngine;

/// <summary>
/// Utility for testing dialogue triggers from the Inspector context menu.
/// Attach this to any GameObject in your scene and assign the relevant DialogueTrigger references.
/// </summary>
public class DialogueTestTools : MonoBehaviour
{
    [Header("Dialogue Triggers to Test")]
    public DialogueTrigger dialogueManagerTrigger;
    public DialogueTrigger dialogueIntroTrigger;
    public DialogueTrigger dialoguePostVictoryTrigger;
    public DialogueTrigger dialogueEndingTrigger;

    [ContextMenu("Test DialogueManager Dialogue")]
    public void TestDialogueManager()
    {
        if (dialogueManagerTrigger != null)
            dialogueManagerTrigger.TriggerDialogue();
        else
            Debug.LogWarning("DialogueManager DialogueTrigger not assigned.");
    }

    [ContextMenu("Test DialogueIntro Dialogue")]
    public void TestDialogueIntro()
    {
        if (dialogueIntroTrigger != null)
            dialogueIntroTrigger.TriggerDialogue();
        else
            Debug.LogWarning("DialogueIntro DialogueTrigger not assigned.");
    }

    [ContextMenu("Test DialoguePostVictory Dialogue")]
    public void TestDialoguePostVictory()
    {
        if (dialoguePostVictoryTrigger != null)
            dialoguePostVictoryTrigger.TriggerDialogue();
        else
            Debug.LogWarning("DialoguePostVictory DialogueTrigger not assigned.");
    }

    [ContextMenu("Test DialogueEnding Dialogue")]
    public void TestDialogueEnding()
    {
        if (dialogueEndingTrigger != null)
            dialogueEndingTrigger.TriggerDialogue();
        else
            Debug.LogWarning("DialogueEnding DialogueTrigger not assigned.");
    }
}