using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor utility to automatically fix missing PresetSelectionUI references
/// </summary>
public class FixPresetSelectionUIReferences : EditorWindow
{
    [MenuItem("Tools/Fix PresetSelectionUI References")]
    public static void FixReferences()
    {
        // Find PresetSelectionUI in the scene
        PresetSelectionUI presetUI = GameObject.FindObjectOfType<PresetSelectionUI>();
        if (presetUI == null)
        {
            Debug.LogError("[FixReferences] PresetSelectionUI not found in scene!");
            EditorUtility.DisplayDialog("Error", "PresetSelectionUI not found in the current scene!", "OK");
            return;
        }

        // Find CardPresetManager in the scene
        CardPresetManager cardPresetManager = GameObject.FindObjectOfType<CardPresetManager>();
        if (cardPresetManager == null)
        {
            Debug.LogError("[FixReferences] CardPresetManager not found in scene!");
            EditorUtility.DisplayDialog("Error", "CardPresetManager not found in the current scene!", "OK");
            return;
        }

        // Use SerializedObject to properly set the reference
        SerializedObject serializedUI = new SerializedObject(presetUI);
        SerializedProperty cardPresetManagerProperty = serializedUI.FindProperty("cardPresetManager");
        
        if (cardPresetManagerProperty != null)
        {
            cardPresetManagerProperty.objectReferenceValue = cardPresetManager;
            serializedUI.ApplyModifiedProperties();
            
            // Mark scene as dirty so changes are saved
            EditorUtility.SetDirty(presetUI);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            
            Debug.Log($"[FixReferences] Successfully assigned CardPresetManager to PresetSelectionUI!");
            EditorUtility.DisplayDialog("Success", "CardPresetManager reference has been assigned to PresetSelectionUI!\n\nThe preset count should now update correctly.", "OK");
        }
        else
        {
            Debug.LogError("[FixReferences] Could not find cardPresetManager property!");
            EditorUtility.DisplayDialog("Error", "Could not find cardPresetManager property in PresetSelectionUI!", "OK");
        }
    }
}
