using System.Collections.Generic;
using UnityEngine;

/* PERK DOCUMENTATION
 * 
 * How it works:
 * - Turns perk data files into working perks that can trigger during gameplay
 * - Listens for specific events and automatically activates when conditions are met
 * - Handles its own setup when added and cleanup when removed
 * - Creates game actions when it triggers to actually do something useful
 * 
 * Design reasoning:
 * - Separates perk data (what it is) from perk behavior (what it does)
 * - Self-managing design means perks handle their own event listening
 * - Flexible targeting allows perks to affect different characters based on situation
 * - Works with existing action system so perks feel like natural part of the game
 * 
 * Integration: Created from PerkData, managed by PerkSystem, triggers create actions for other systems
 */

/// <summary>
/// A working perk that can automatically trigger during gameplay
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Turns perk design files into actual working perks</para>
/// 
/// <para><strong>What it does:</strong> This class takes perk data from files and makes 
/// it actually work in the game. It listens for specific events (like enemy attacks) 
/// and automatically triggers its effect when the right thing happens. Each perk 
/// manages itself - it knows when to start listening, when to stop, and what to 
/// do when it activates.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Created from a PerkData file that defines what the perk does</item>
/// <item>When added to player, starts listening for its trigger condition</item>
/// <item>When the right event happens, checks if it should actually trigger</item>
/// <item>If yes, figures out who to target and creates an action to do its effect</item>
/// <item>Action gets processed by the game like any other action</item>
/// </list>
/// 
/// <para><strong>Examples:</strong></para>
/// <list type="bullet">
/// <item>Damage reduction perk that reduces incoming attack damage</item>
/// <item>Healing perk that restores health when you get hurt</item>
/// <item>Card draw perk that gives you extra cards when enemies attack</item>
/// <item>Counter-attack perk that damages enemies when they hit you</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> Created by PerkSystem, uses PerkData for setup, creates actions for game systems</para>
/// </remarks>
public class Perk
{
    /// <summary>
    /// The visual icon for this perk that players see
    /// </summary>
    /// <remarks>
    /// Gets the perk image from the data file to show in UI.
    /// Helps players recognize which perks they have active.
    /// </remarks>
    public Sprite Image => data.Image;
    
    /// <summary>
    /// The original design file that defines this perk
    /// </summary>
    /// <remarks>
    /// Stores the perk data that was used to create this working perk.
    /// Contains all the settings like what triggers it and what effect it has.
    /// </remarks>
    private readonly PerkData data;
    
    /// <summary>
    /// The condition that decides when this perk should trigger
    /// </summary>
    /// <remarks>
    /// Handles listening for the right events and checking if conditions are met.
    /// Different perks have different conditions like "when attacked" or "when playing cards".
    /// </remarks>
    private readonly PerkCondition condition;
    
    /// <summary>
    /// The effect that happens when this perk triggers
    /// </summary>
    /// <remarks>
    /// The actual thing the perk does when it activates.
    /// Could be damage, healing, card draw, or any other game effect.
    /// </remarks>
    private readonly AutoTargetEffect effect;

    /// <summary>
    /// Creates a working perk from perk data
    /// </summary>
    /// <param name="perkData">The perk design file to turn into a working perk</param>
    /// <remarks>
    /// Takes perk data and extracts the pieces needed to make it work in game.
    /// Separates the condition (when to trigger) from the effect (what to do).
    /// </remarks>
    public Perk(PerkData perkData)
    {
        // Store the original design data
        data = perkData;
        // Get the trigger condition from the data
        condition = data.PerkCondition;
        // Get the effect from the data
        effect = data.AutoTargetEffect;
    }

    /// <summary>
    /// Starts this perk working when it gets added to the player
    /// </summary>
    /// <remarks>
    /// Called by PerkSystem when the perk is added to the player.
    /// Makes the perk start listening for its trigger events.
    /// From this point on, the perk will automatically activate when conditions are met.
    /// </remarks>
    public void OnAdd()
    {
        // Tell the condition to start listening for its events and call our Reaction method when they happen
        condition.SubscribeCondition(Reaction);
    }

    /// <summary>
    /// Stops this perk working when it gets removed from the player
    /// </summary>
    /// <remarks>
    /// Called by PerkSystem when the perk is removed from the player.
    /// Makes the perk stop listening for events to prevent unwanted triggers.
    /// Cleans up properly so the perk doesn't interfere after removal.
    /// </remarks>
    public void OnRemove()
    {
        // Tell the condition to stop listening for events
        condition.UnsubscribeCondition(Reaction);
    }

    /// <summary>
    /// The method that gets called when the perk's condition is met
    /// </summary>
    /// <param name="gameAction">The action that triggered this perk</param>
    /// <remarks>
    /// This is the heart of the perk system. When the right event happens, this method
    /// figures out who should be affected and creates an action to do the perk's effect.
    /// Handles different targeting modes based on the perk's settings.
    /// </remarks>
    private void Reaction(GameAction gameAction)
    {
        // First check if the extra conditions are met (like minimum damage, etc.)
        if (condition.SubConditionIsMet(gameAction))
        {
            // Create a list to hold who this perk should affect
            List<CombatantView> targets = new();
            
            // Check if this perk should target whoever caused the triggering action
            if (data.UseActionCasterAsTarget && gameAction is IHaveCaster haveCaster)
            {
                // Target the one who caused the action (like the attacking enemy)
                targets.Add(haveCaster.Caster);
            }
            
            // Check if this perk should use automatic targeting
            if (data.UseAutoTarget)
            {
                // Add targets from the effect's built-in targeting (like "all enemies")
                targets.AddRange(effect.targetMode.GetTargets());
            }
            
            // Create the actual action that will do the perk's effect
            GameAction perkEffectAction = effect.effects.GetGameAction(targets, HeroSystem.Instance.HeroView);
            // Add the action to the game to be processed
            ActionSystem.Instance.AddReaction(perkEffectAction);
        }
    }
}
