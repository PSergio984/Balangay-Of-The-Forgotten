using UnityEngine;


/* LEVEL TRANSITION DATA
 * 
 * Purpose: Persists level selection data between scenes (level select → combat)
 * 
 * How it works:
 * - ScriptableObjects naturally persist data across scenes without DontDestroyOnLoad
 * - MapButton writes the selected MapData before scene transition
 * - MatchSetupSystem reads the selected MapData when combat scene loads
 * 
 * Integration: Bridge between MapButton (level select) and MatchSetupSystem (combat)
 */


/// <summary>
/// ScriptableObject mailbox for passing selected MapData between scenes.
/// Used to bridge level selection and combat scene setup.
/// <br/><br/>
/// <b>IMPORTANT (Editor Only):</b>
/// <para>
/// <b>Editor State Pollution Warning:</b> Because ScriptableObjects persist their field values in the Unity Editor after exiting Play mode, any runtime data (such as SelectedMapData) will remain set until explicitly cleared. This can cause stale or unexpected data to appear in subsequent Play sessions or scene loads during Editor testing.
/// </para>
/// <para>
/// <b>Best Practice:</b> Always call <see cref="Clear"/> on this ScriptableObject after reading its data, or during scene teardown (e.g., in OnDisable or OnDestroy) when running in the Editor. This ensures that runtime fields are reset and prevents accidental carryover of data between Play sessions.
/// </para>
/// <para>
/// <b>Example:</b>
/// <code>
/// #if UNITY_EDITOR
///     levelTransitionData.Clear();
/// #endif
/// </code>
/// </para>
/// <br/>
/// <b>What:</b> Acts as a "mailbox" - level select writes, combat scene reads
/// </summary>
[CreateAssetMenu(fileName = "LevelTransitionData", menuName = "Data/Level Transition Data")]
public class LevelTransitionData : ScriptableObject
{
    /// <summary>
    /// The currently selected map data (set by MapButton before scene load)
    /// </summary>
    /// <remarks>
    /// <para><strong>Set by:</strong> MapButton.LoadMap() before transitioning to combat</para>
    /// <para><strong>Read by:</strong> MatchSetupSystem.Start() when combat scene initializes</para>
    /// </remarks>
    [Header("Runtime Data (Set by Level Select)")]
    [Tooltip("The map data for the currently selected level - set automatically when player selects a level")]
    public MapData SelectedMapData;

    /// <summary>
    /// Clear time in seconds recorded from CombatTimer when a map is cleared.
    /// Written by VictoryDefeatUI.RecordCombatClearTime before any scene transition.
    /// </summary>
    [Header("Runtime Data (Set by VictoryDefeatUI)")]
    [Tooltip("Elapsed combat clear time in seconds — set when victory condition is reached")]
    public float ClearTimeSeconds;

    /// <summary>
    /// Clears the selected map data and runtime clear-time (call after reading or when returning to main menu)
    /// </summary>
    public void Clear()
    {
        SelectedMapData = null;
        ClearTimeSeconds = 0f;
    }


    /// <summary>
    /// Checks if valid map data is available for combat setup
    /// </summary>
    /// <returns>True if SelectedMapData is assigned and has valid enemy data</returns>
    public bool HasValidData()
    {
        return SelectedMapData != null && 
               SelectedMapData.EnemyDatas != null && 
               SelectedMapData.EnemyDatas.Count > 0;
    }
}
