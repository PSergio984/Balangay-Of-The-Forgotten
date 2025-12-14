using UnityEngine;
using System.Collections.Generic;

/* CHARACTER TRANSITION DATA
 * 
 * Purpose: Persists character selection data between scenes (character select → combat)
 * 
 * How it works:
 * - ScriptableObjects naturally persist data across scenes without DontDestroyOnLoad
 * - CharacterSelectionManager writes the selected HeroData before scene transition
 * - MatchSetupSystem reads the selected HeroData when combat scene loads
 * 
 * Integration: Bridge between CharacterSelectionManager (character select) and MatchSetupSystem (combat)
 */

/// <summary>
/// ScriptableObject mailbox for passing selected HeroData between scenes.
/// Used to bridge character selection and combat scene setup.
/// <br/><br/>
/// <b>IMPORTANT (Editor Only):</b>
/// <para>
/// <b>Editor State Pollution Warning:</b> Because ScriptableObjects persist their field values in the Unity Editor after exiting Play mode, any runtime data (such as SelectedHeroes) will remain set until explicitly cleared. This can cause stale or unexpected data to appear in subsequent Play sessions or scene loads during Editor testing.
/// </para>
/// <para>
/// <b>Best Practice:</b> Always call <see cref="Clear"/> on this ScriptableObject after reading its data, or during scene teardown (e.g., in OnDisable or OnDestroy) when running in the Editor. This ensures that runtime fields are reset and prevents accidental carryover of data between Play sessions.
/// </para>
/// <para>
/// <b>Example:</b>
/// <code>
/// #if UNITY_EDITOR
///     characterTransitionData.Clear();
/// #endif
/// </code>
/// </para>
/// <br/>
/// <b>What:</b> Acts as a "mailbox" - character select writes, combat scene reads
/// </summary>
[CreateAssetMenu(fileName = "CharacterTransitionData", menuName = "Data/Character Transition Data")]
public class CharacterTransitionData : ScriptableObject
{
    /// <summary>
    /// The 4 character slots with hero and preset selections (set by character selection UI before scene load)
    /// </summary>
    /// <remarks>
    /// <para><strong>Set by:</strong> Character selection UI when player clicks "Save"</para>
    /// <para><strong>Read by:</strong> MatchSetupSystem.Start() when combat scene initializes</para>
    /// <para><strong>Order matters:</strong> Slot index determines spawn order and attack order in combat</para>
    /// </remarks>
    [Header("Runtime Data (Set by Character Selection)")]
    [Tooltip("The 4 character slots - order determines spawn/attack order in combat")]

    public CharacterSlotData[] CharacterSlots = new CharacterSlotData[4];

    private void OnEnable()
    {
        // Ensure array is correct length and all slots are non-null, but do NOT clear data automatically
        if (CharacterSlots == null || CharacterSlots.Length != 4)
            CharacterSlots = new CharacterSlotData[4];
        for (int i = 0; i < 4; i++)
        {
            if (CharacterSlots[i] == null)
                CharacterSlots[i] = new CharacterSlotData(i);
        }
    }

    /// <summary>
    /// Clears all slot data (call after reading or when returning to main menu)
    /// </summary>
    public void Clear()
    {
        if (CharacterSlots != null)
        {
            for (int i = 0; i < CharacterSlots.Length; i++)
            {
                CharacterSlots[i] = new CharacterSlotData(i);
            }
        }
    }

    /// <summary>
    /// Checks if valid character data is available for combat setup
    /// </summary>
    /// <returns>True if at least one slot has a complete hero+preset selection</returns>
    public bool HasValidData()
    {
        if (CharacterSlots == null || CharacterSlots.Length == 0) return false;
        
        // Check if at least one slot is complete
        foreach (var slot in CharacterSlots)
        {
            if (slot != null && slot.IsComplete)
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Gets all complete slots in order
    /// </summary>
    public CharacterSlotData[] GetCompleteSlots()
    {
        var completeSlots = new List<CharacterSlotData>();
        
        if (CharacterSlots != null)
        {
            foreach (var slot in CharacterSlots)
            {
                if (slot != null && slot.IsComplete)
                {
                    completeSlots.Add(slot);
                }
            }
        }
        
        return completeSlots.ToArray();
    }
}
