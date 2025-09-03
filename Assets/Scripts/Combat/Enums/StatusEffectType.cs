/*
 * STATUS EFFECT TYPE DOCUMENTATION
 * 
 * How it works:
 * This enum lists all the different types of status effects that can affect combatants.
 * Each type represents a different ongoing effect like armor protection or burn damage.
 * Used to identify which status effect we're dealing with in the UI and game systems.
 * 
 * Design reasoning:
 * Using an enum makes it easy to add new status effects and keeps everything organized.
 * The UI system can use these types to know which sprite to show for each effect.
 * Game systems can check for specific effect types when applying rules and interactions.
 * 
 * Integration:
 * - StatusEffectsUI uses these to track which effects to display
 * - Game systems use these to identify what effects are active on combatants
 * - Easy to extend by adding new effect types to this enum
 */

/// <summary>
/// All the different types of status effects that can affect combatants
/// </summary>
public enum StatusEffectType
{
    /// <summary>
    /// Armor effect - reduces incoming damage
    /// </summary>
    ARMOR,
    
    /// <summary>
    /// Burn effect - deals damage over time
    /// </summary>
    BURN
}