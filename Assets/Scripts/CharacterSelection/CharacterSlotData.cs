using UnityEngine;
using System;

/* CHARACTER SLOT DATA
 * 
 * Purpose: Tracks a single character slot's selection (hero + build preset)
 * 
 * How it works:
 * - Each of the 4 slots has its own CharacterSlotData
 * - Slot order determines spawn/attack order in combat
 * - Stores both the hero and their selected build preset
 * - IMPORTANT: Selected preset provides FIXED stats that completely override hero base stats
 * - Gets passed to MatchSetupSystem for spawning
 * 
 * Design Decision: Presets use fixed stats (not modifiers) for:
 *   - Clarity: Each preset shows exact final stats, no mental math required
 *   - Balance: Designers control exact stat values per build
 *   - Simplicity: No edge cases with negative modifiers or stat floors
 * 
 * Integration: Created by main selection UI, stored in CharacterTransitionData, read by MatchSetupSystem
 */

/// <summary>
/// Data for a single character slot selection
/// </summary>
[Serializable]
public class CharacterSlotData
{
    /// <summary>
    /// The hero assigned to this slot (can be null if slot is empty)
    /// </summary>
    public HeroData Hero;
    
    /// <summary>
    /// The selected build preset for this hero (null if not selected yet)
    /// </summary>
    public CharacterBuildPreset SelectedPreset;
    
    /// <summary>
    /// The slot index (0-3) determining spawn/attack order. Value is validated in constructor.
    /// </summary>
    private int _slotIndex;
    public int SlotIndex => _slotIndex;
    
    /// <summary>
    /// Whether this slot has a valid hero and preset selection
    /// </summary>
    public bool IsComplete => Hero != null && SelectedPreset != null;
    
    /// <summary>
    /// Display name for this slot selection
    /// </summary>
    public string DisplayName
    {
        get
        {
            if (Hero == null) return "Empty Slot";
            string heroName = string.IsNullOrWhiteSpace(Hero.HeroName) ? "Unnamed Hero" : Hero.HeroName;
            if (SelectedPreset == null)
                return $"{heroName} (No Build)";
            string presetName = string.IsNullOrWhiteSpace(SelectedPreset.PresetName) ? "Unnamed Preset" : SelectedPreset.PresetName;
            return $"{heroName} - {presetName}";
        }
    }
    
    public CharacterSlotData(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex > 3)
            throw new ArgumentOutOfRangeException(nameof(slotIndex), slotIndex, "SlotIndex must be in the range 0..3. Provided value: " + slotIndex);
        _slotIndex = slotIndex;
        Hero = null;
        SelectedPreset = null;
    }
    
    /// <summary>
    /// Gets the final stats from the selected preset (overrides hero base stats completely).
    /// Returns a named tuple for IntelliSense support.
    /// Design: Presets provide fixed stats, not modifiers. Hero base stats are ignored when preset is selected.
    /// </summary>
    public (int Health, float Attack, float Magic, float Defense) GetFinalStats()
    {
        if (Hero == null || SelectedPreset == null)
        {
            return (Health: 0, Attack: 0, Magic: 0, Defense: 0);
        }

        // Preset now provides fixed stats (overrides hero base stats)
        var (health, attack, magic, defense) = SelectedPreset.GetPresetStats();
        return (Health: health, Attack: attack, Magic: magic, Defense: defense);
    }
}
