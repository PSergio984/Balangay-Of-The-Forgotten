using System.Collections.Generic;
using UnityEngine;

/* PERK SYSTEM DOCUMENTATION
 * 
 * How it works:
 * - Keeps track of all active perks the player currently has
 * - Handles adding new perks and removing old ones
 * - Makes sure perks start working when added and stop working when removed
 * - Simple manager that lets other parts of the game work with perks easily
 * 
 * Design reasoning:
 * - Central place to manage all player perks without scattered perk code everywhere
 * - Simple add/remove interface makes it easy for other systems to give perks to player
 * - Each perk manages itself, so this system just needs to track them and handle lifecycle
 * - Singleton pattern means any part of the game can easily access the perk system
 * 
 * Integration: Used by MatchSetupSystem to give starting perks, can be used by other systems to grant perks
 */

/// <summary>
/// System that manages all the perks the player currently has active
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Central manager for the player's collection of perks</para>
/// 
/// <para><strong>What it does:</strong> This system keeps track of all the perks the 
/// player has and makes sure they work properly. When a perk is added, this system 
/// tells it to start working. When a perk is removed, this system tells it to stop 
/// working. It's like a simple list manager with proper cleanup.</para>
/// 
/// <para><strong>How it works:</strong></para>
/// <list type="bullet">
/// <item>Other systems create Perk objects and add them here</item>
/// <item>When added, system calls perk.OnAdd() to make it start listening</item>
/// <item>Perk automatically triggers during gameplay when conditions are met</item>
/// <item>When removed, system calls perk.OnRemove() to clean up properly</item>
/// <item>Maintains list of active perks for other systems to reference</item>
/// </list>
/// 
/// <para><strong>Examples of use:</strong></para>
/// <list type="bullet">
/// <item>MatchSetupSystem gives player starting perks for the battle</item>
/// <item>Reward system could add perks when player wins fights</item>
/// <item>Item system could add perks from equipped gear</item>
/// <item>Level up system could add perks when player gains levels</item>
/// </list>
/// 
/// <para><strong>Works with:</strong> Any system that wants to give or remove perks from the player</para>
/// </remarks>
public class PerkSystem : Singleton<PerkSystem>
{
    /// <summary>
    /// List of all perks the player currently has active
    /// </summary>
    /// <remarks>
    /// Contains all the working perks that are currently listening for events.
    /// Each perk in this list will automatically trigger when its conditions are met.
    /// </remarks>
    private readonly List<Perk> perks = new();

    /// <summary>
    /// Adds a new perk to the player and makes it start working
    /// </summary>
    /// <param name="perk">The perk to add to the player</param>
    /// <remarks>
    /// Call this method to give the player a new perk. The perk will be added to 
    /// the active list and told to start listening for its trigger events. From 
    /// this point on, the perk will automatically activate when appropriate.
    /// </remarks>
    public void AddPerk(Perk perk)
    {
        // Add the perk to our list of active perks
        perks.Add(perk);
        // Tell the perk to start working (start listening for events)
        perk.OnAdd();
    }

    /// <summary>
    /// Removes a perk from the player and makes it stop working
    /// </summary>
    /// <param name="perk">The perk to remove from the player</param>
    /// <remarks>
    /// Call this method to take away a perk from the player. The perk will be 
    /// removed from the active list and told to stop listening for events. 
    /// This ensures proper cleanup and prevents unwanted perk triggers.
    /// </remarks>
    public void RemovePerk(Perk perk)
    {
        // Remove the perk from our list of active perks
        perks.Remove(perk);
        // Tell the perk to stop working (stop listening for events)
        perk.OnRemove();
    }
}
