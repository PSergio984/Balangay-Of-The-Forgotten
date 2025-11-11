using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif


/* 
 * Purpose: Custom serializable field for safely referencing scenes in the Inspector
 * 
 * How it works:
 * - Stores scene asset reference and extracts scene name automatically
 * - Custom property drawer provides scene asset picker in Inspector
 * - Implicit string operator allows direct use with SceneManager.LoadScene()
 * 
 * Integration: Use instead of raw string fields to prevent typos and broken scene references
 */


/// <summary>
/// Serializable wrapper for scene references with automatic name extraction and Inspector support
/// </summary>
/// <remarks>
/// <para><strong>Why:</strong> Prevents broken scene references from typos - validates scene exists at edit time</para>
/// <para><strong>How:</strong> Stores SceneAsset reference, auto-extracts name, implicitly converts to string for LoadScene()</para>
/// <para><strong>Usage:</strong> Declare as <c>public SceneField myScene;</c> then use <c>SceneManager.LoadScene(myScene);</c></para>
/// </remarks>
[System.Serializable]
public class SceneField
{
    /// <summary>
    /// Editor-only reference to the scene asset (validated at edit time)
    /// </summary>
    [SerializeField]
    private Object m_SceneAsset;

    /// <summary>
    /// Runtime scene name extracted from the scene asset (survives builds)
    /// </summary>
    [SerializeField]
    private string m_SceneName = "";
    
    /// <summary>
    /// Gets the scene name for runtime scene loading
    /// </summary>
    public string SceneName
    {
        get { return m_SceneName; }
    }

    /// <summary>
    /// Implicit conversion to string - allows direct use with SceneManager.LoadScene()
    /// </summary>
    /// <param name="sceneField">The SceneField to convert</param>
    /// <returns>Scene name string, or empty string if null</returns>
    /// <remarks>
    /// <para><strong>Why:</strong> Enables seamless integration with existing Unity scene loading APIs</para>
    /// </remarks>
    public static implicit operator string(SceneField sceneField)
    {
        // Null-safe: returns empty string if sceneField is null
        return sceneField?.SceneName ?? "";
    }   
}


#if UNITY_EDITOR
/// <summary>
/// Custom property drawer for SceneField - provides scene asset picker in Inspector
/// </summary>
/// <remarks>
/// <para><strong>Why:</strong> Creates user-friendly scene selection UI instead of manual string entry</para>
/// <para><strong>How:</strong> Shows ObjectField for SceneAsset, automatically syncs scene name when asset changes</para>
/// </remarks>
[CustomPropertyDrawer(typeof(SceneField))]
public class SceneFieldPropertyDrawer : PropertyDrawer 
{
    /// <summary>
    /// Renders the custom Inspector GUI for SceneField properties
    /// </summary>
    /// <param name="_position">Rectangle in the Inspector where the property is drawn</param>
    /// <param name="_property">SerializedProperty being drawn (the SceneField instance)</param>
    /// <param name="_label">Label to display for this property</param>
    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        EditorGUI.BeginProperty(_position, GUIContent.none, _property);
        
        // Get references to the serialized fields inside SceneField
        SerializedProperty sceneAsset = _property.FindPropertyRelative("m_SceneAsset");
        SerializedProperty sceneName = _property.FindPropertyRelative("m_SceneName");
        
        // Draw the property label and get the remaining rect for the field
        _position = EditorGUI.PrefixLabel(_position, GUIUtility.GetControlID(FocusType.Passive), _label);
        
        if (sceneAsset != null)
        {
            // Draw ObjectField that only accepts SceneAsset references
            sceneAsset.objectReferenceValue = EditorGUI.ObjectField(_position, sceneAsset.objectReferenceValue, typeof(SceneAsset), false);

            // Automatically sync scene name when asset is assigned
            if (sceneAsset.objectReferenceValue != null)
            {
                var sceneAssetObj = sceneAsset.objectReferenceValue as SceneAsset;
                if (sceneAssetObj != null)
                {
                    // Extract scene name from the SceneAsset
                    sceneName.stringValue = sceneAssetObj.name;
                }
                else
                {
                    // Clear scene name if cast failed
                    sceneName.stringValue = "";
                }
            }
            else
            {
                // Clear scene name if asset is removed
                sceneName.stringValue = "";
            }
        }
        
        EditorGUI.EndProperty();
    }
}
#endif
