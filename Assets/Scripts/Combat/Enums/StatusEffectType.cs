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
 * CONSOLIDATED DESIGN:
 * - Generic effects (DMG_UP, DEFENSE_DOWN) can be used with different stack values
 * - Combo buffs (RAGE, FOCUSED) combine multiple effects for specific abilities
 * - Removed redundant effects (BLESSED = DMG_UP, BONECRACKED/MOONFALL = DEFENSE_DOWN)
 * 
 * Integration:
 * - StatusEffectsUI uses these to track which effects to display
 * - Game systems use these to identify what effects are active on combatants
 * - Easy to extend by adding new effect types to this enum
 * 
 * Categories:
 * - Defensive: ARMOR, DEFENSE_UP, INVULNERABLE, SHIELD, TEMP_HP
 * - Offensive Buffs: ATTACK_UP, CRIT_UP, DMG_UP, RAGE, FOCUSED
 * - Debuffs: BURN, DEFENSE_DOWN, DEVOURED
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
    /// Used by: Last Stand (+50% defense), Daragang Magayon Buff (+25% defense)
    /// Stacks represent percentage increase (e.g., 50 = +50% defense)
    /// </summary>
    DEFENSE_UP,
    
    /// <summary>
    /// Invulnerable effect - makes combatant immune to all damage
    /// Used by: Tide of Night (Mayari) - invulnerable for 1 turn
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
    /// Used by: Daybreak Fury (+40% ATK), Apolaki enrage
    /// Stacks represent percentage increase (e.g., 40 = +40% ATK)
    /// </summary>
    ATTACK_UP,
    
    /// <summary>
    /// Crit Up effect - increases critical hit chance
    /// Used by: Lunar Strike (+55% crit)
    /// Stacks represent percentage increase (e.g., 55 = +55% crit chance)
    /// </summary>
    CRIT_UP,
    
    /// <summary>
    /// Damage Up effect - increases all damage dealt by percentage
    /// Used by: 
    /// - Dagát ng Kabisayaan Buff (+15% DMG)
    /// - Blessing (+20% DMG for 2 turns)
    /// - Generic damage buffs
    /// Stacks represent percentage increase (e.g., 20 = +20% damage)
    /// NOTE: This replaces BLESSED (which was just +20% DMG)
    /// </summary>
    DMG_UP,
    
    /// <summary>
    /// Rage effect - combo buff: +50% DMG, +20% def ignore, +20% hit
    /// Used by: Berserk State (3 turns, requires HP ≤50%)
    /// This is a special combo buff handled by RageStatusEffectSystem
    /// </summary>
    RAGE,
    
    /// <summary>
    /// Focused effect - combo buff: +30% hit, +20% def ignore
    /// Used by: Focus Aim (2 turns)
    /// This is a special combo buff that combines accuracy and defense penetration
    /// </summary>
    FOCUSED,
    
    /// <summary>
    /// Defense Ignore effect - attacks ignore a percentage of enemy defense
    /// Used by: Daybreak Fury (Apolaki) - ignores 20% DEF for 1 turn
    /// Stacks represent duration in turns
    /// </summary>
    DEFENSE_IGNORE,
    
    // ===== DEBUFF EFFECTS =====
    
    /// <summary>
    /// Burn effect - deals damage over time each turn
    /// Stacks represent damage per turn
    /// </summary>
    BURN,
    
    /// <summary>
    /// Defense Down effect - decreases target defense by percentage
    /// Used by:
    /// - Heavy Attack: -10% DEF for 1 turn (Bonecracked)
    /// - Moonfall Spear: -20% DEF for 2 turns (Moonfall)
    /// - Serpent's Coil: -15% DEF for 2 turns
    /// Stacks represent percentage decrease (e.g., 10 = -10% defense)
    /// NOTE: This consolidates BONECRACKED, MOONFALL, and generic defense debuffs
    /// </summary>
    DEFENSE_DOWN,
    
    /// <summary>
    /// Attack Down effect - decreases target attack by percentage
    /// Used by: Weaken, Enfeeble abilities
    /// Stacks represent percentage decrease (e.g., 30 = -30% attack)
    /// </summary>
    ATTACK_DOWN,
    
    /// <summary>
    /// Crit Down effect - decreases target critical hit chance
    /// Used by: Dull Senses, Blind abilities
    /// Stacks represent percentage decrease (e.g., 30 = -30% crit chance)
    /// </summary>
    CRIT_DOWN,
    
    /// <summary>
    /// Devoured effect - DoT debuff that deals fixed damage per turn
    /// Used by: Lunar Devour (Bakunawa) - 60 HP per turn for 2 turns
    /// Stacks represent damage per turn
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
    
    // ===== SPECIAL CARD EFFECTS =====
    
    /// <summary>
    /// No Cooldown effect - all skills have no cooldown for the duration
    /// Used by: Bundok Pulag Mini-Boss Buff (4 rounds for all players)
    /// Stacks represent duration in rounds
    /// </summary>
    NO_COOLDOWN,
}