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
    /// Attack Up effect - increases attack stat
    /// </summary>
    ATTACK_UP,

    /// <summary>
    /// Burn effect - deals damage over time
    /// </summary>
    BURN,

    /// <summary>
    /// Crit Up effect - increases critical hit chance
    /// </summary>
    CRIT_UP,

    /// <summary>
    /// Defense Down effect - decreases defense stat
    /// </summary>
    DEFENSE_DOWN,

    /// <summary>
    /// Defense Up effect - increases defense stat
    /// </summary>
    DEFENSE_UP,

    /// <summary>
    /// Dmg Up effect - increases damage dealt
    /// </summary>
    DMG_UP,

    /// <summary>
    /// Ignore Defense effect - ignores a portion of target's defense
    /// </summary>
    IGNORE_DEFENSE,

    /// <summary>
    /// Invulnerable effect - makes combatant immune to damage
    /// </summary>
    INVULNERABLE,

    /// <summary>
    /// Taunt effect - forces enemies to target this combatant
    /// </summary>
    TAUNT,

    /// <summary>
    /// Temporary HP effect - adds temporary health that can absorb damage
    /// </summary>
    TEMP_HP,
}