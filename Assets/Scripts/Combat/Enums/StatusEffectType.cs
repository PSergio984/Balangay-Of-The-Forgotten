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
 * 
 * Categories:
 * - Defensive: ARMOR, DEFENSE_UP, INVULNERABLE, SHIELD, TEMP_HP
 * - Offensive Buffs: ATTACK_UP, CRIT_UP, DMG_UP, IGNORE_DEFENSE, HIT_UP, RAGE, FOCUSED, BLESSED
 * - Debuffs: BURN, DEFENSE_DOWN, BONECRACKED, MOONFALL, DEVOURED
 * - Control: STUN, TAUNT, RESTING, CHARGING
 */

/// <summary>
/// All the different types of status effects that can affect combatants
/// </summary>
public enum StatusEffectType
{
    // ===== DEFENSIVE EFFECTS =====
    
    /// <summary>
    /// Armor effect - reduces incoming damage (consumed on hit)
    /// </summary>
    ARMOR,
    
    /// <summary>
    /// Defense Up effect - increases defense stat by percentage
    /// </summary>
    DEFENSE_UP,
    
    /// <summary>
    /// Invulnerable effect - makes combatant immune to all damage
    /// </summary>
    INVULNERABLE,
    
    /// <summary>
    /// Shield effect - temporary HP with duration that absorbs damage
    /// Used by: Fortify, Guardian's Oath
    /// </summary>
    SHIELD,
    
    /// <summary>
    /// Temporary HP effect - adds temporary health that can absorb damage
    /// </summary>
    TEMP_HP,
    
    // ===== OFFENSIVE BUFF EFFECTS =====
    
    /// <summary>
    /// Attack Up effect - increases attack stat by percentage
    /// Used by: Daybreak Fury (+40% ATK)
    /// </summary>
    ATTACK_UP,
    
    /// <summary>
    /// Crit Up effect - increases critical hit chance
    /// Used by: Solar Flare Slash (+55% crit)
    /// </summary>
    CRIT_UP,
    
    /// <summary>
    /// Dmg Up effect - increases all damage dealt by percentage
    /// Used by: Dungeon Buff (+15% DMG), Rage (+50% DMG)
    /// </summary>
    DMG_UP,
    
    /// <summary>
    /// Ignore Defense effect - attacks ignore a portion of target's defense
    /// Used by: Berserk State (20%), Focus Aim (20%), Daybreak Fury (20%)
    /// </summary>
    IGNORE_DEFENSE,
    
    /// <summary>
    /// Hit Up effect - increases accuracy/hit chance
    /// Used by: Berserk State (+20% hit), Focus Aim (+30% hit)
    /// </summary>
    HIT_UP,
    
    /// <summary>
    /// Rage effect - combo buff: +50% DMG, +20% def ignore, +20% hit
    /// Used by: Berserk State (3 turns, requires HP ≤50%)
    /// </summary>
    RAGE,
    
    /// <summary>
    /// Focused effect - combo buff: +30% hit, +20% def ignore
    /// Used by: Focus Aim (2 turns)
    /// </summary>
    FOCUSED,
    
    /// <summary>
    /// Blessed effect - +20% damage buff to ally
    /// Used by: Blessing (2 turns)
    /// </summary>
    BLESSED,
    
    // ===== DEBUFF EFFECTS =====
    
    /// <summary>
    /// Burn effect - deals damage over time each turn
    /// Stacks represent damage per turn
    /// </summary>
    BURN,
    
    /// <summary>
    /// Defense Down effect - decreases defense stat by percentage
    /// </summary>
    DEFENSE_DOWN,
    
    /// <summary>
    /// Bonecracked effect - reduces target DEF by 10% for 1 turn
    /// Used by: Heavy Attack (50% chance)
    /// </summary>
    BONECRACKED,
    
    /// <summary>
    /// Moonfall effect - reduces target DEF by 20% for 2 turns
    /// Used by: Moonfall Spear (Mayari)
    /// </summary>
    MOONFALL,
    
    /// <summary>
    /// Devoured effect - DoT debuff that deals fixed damage per turn
    /// Used by: Lunar Devour (Bakunawa) - 60 HP per turn for 2 turns
    /// </summary>
    DEVOURED,
    
    // ===== CONTROL EFFECTS =====
    
    /// <summary>
    /// Stun effect - target skips their turn
    /// Used by: Skyhammer (70%), Thunderous Decree (50%), Radiant Charge (30%)
    /// </summary>
    STUN,
    
    /// <summary>
    /// Taunt effect - forces enemies to target this combatant
    /// Used by: Taunt (2 turns)
    /// </summary>
    TAUNT,
    
    /// <summary>
    /// Resting effect - combatant is resting and cannot act
    /// Used by: Celestial Judgement (Bathala rests 1 turn after)
    /// </summary>
    RESTING,
    
    /// <summary>
    /// Charging effect - combatant is preparing a powerful attack
    /// Used by: Shadow Dive (Bakunawa) - next attack deals double damage
    /// </summary>
    CHARGING,
}