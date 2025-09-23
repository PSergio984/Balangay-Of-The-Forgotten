using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a healing plan for a single target.
/// </summary>
/// <remarks>
/// <para><strong>Purpose:</strong> Used by HealGA to specify who gets healed and by how much.</para>
/// <para><strong>How it works:</strong> Each HealTarget is a tuple of (CombatantView, heal amount) for flexible multi-target healing.</para>
/// <para><strong>Examples:</strong> Heal one ally, heal all allies, heal self for lifesteal, etc.</para>
/// </remarks>
public struct HealTarget
{
	/// <summary>
	/// The combatant who will receive healing.
	/// </summary>
	public CombatantView Target;
	/// <summary>
	/// The amount of HP to restore to the target.
	/// </summary>
	public float Amount;
	/// <summary>
	/// Creates a new HealTarget for a specific combatant and heal amount.
	/// </summary>
	/// <param name="target">Combatant to heal</param>
	/// <param name="amount">Amount of HP to restore</param>
	public HealTarget(CombatantView target, float amount)
	{
		Target = target;
		Amount = amount;
	}
}

/// <summary>
/// GameAction representing a plan to heal one or more combatants.
/// </summary>
/// <remarks>
/// <para><strong>Main purpose:</strong> Separates healing intent from actual health changes, allowing perks and systems to react or modify healing.</para>
/// <para><strong>How it works:</strong> Stores a list of HealTarget (target, amount) and tracks the caster for perks and effects.</para>
/// <para><strong>Benefits:</strong></para>
/// <list type="bullet">
/// <item>Supports multi-target and area healing</item>
/// <item>Enables perks to react to healing events</item>
/// <item>Allows lifesteal, self-heal, and complex healing logic</item>
/// </list>
/// </remarks>
public class HealGA : GameAction, IHaveCaster
{
	/// <summary>
	/// List of all healing actions (target, amount). Each entry is a HealTarget specifying who gets healed and by how much.
	/// </summary>
	public List<HealTarget> HealTargets { get; set; }

	/// <summary>
	/// The combatant who initiated the healing (for perk system, lifesteal, etc.).
	/// </summary>
	public CombatantView Caster { get; set; }

	/// <summary>
	/// Creates a healing action with a safe copy of targets and caster tracking.
	/// </summary>
	/// <param name="healTargets">List of HealTarget (target, amount)</param>
	/// <param name="caster">Who is responsible for this healing</param>
	/// <remarks>
	/// Safe copy prevents outside changes to planned action. Caster parameter enables perk system to know who caused the healing.
	/// </remarks>
	public HealGA(List<HealTarget> healTargets, CombatantView caster)
	{
		HealTargets = new(healTargets);
		Caster = caster;
	}
}
