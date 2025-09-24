using System.Collections.Generic;
using UnityEngine;

/*
 * STATUS EFFECT TICK SYSTEM DOCUMENTATION
 *
 * Purpose:
 *   Centralizes per-turn status effect ticking for all combatants (heroes and enemies).
 *   Removes duplicate logic from HeroSystem and EnemySystem, making status effect ticking scalable and maintainable.
 *
 * How it works:
 *   - At the end of each turn, this system is called to process all status effects that need to "tick" (e.g., burn, invulnerable).
 *   - Loops through all combatants and applies per-turn logic for each status effect type.
 *   - For burn: applies burn damage and reduces burn stacks by 1.
 *   - For invulnerable: reduces invulnerable stacks by 1, regardless of attacks.
 *   - Easily extensible: add new status effect ticking logic in one place as new effects are added.
 *
 * Design reasoning:
 *   - Keeps per-turn status effect logic out of individual systems (HeroSystem, EnemySystem), reducing code duplication.
 *   - Supports any number of combatants and status effects, making future expansion easy.
 *   - Follows the single responsibility principle: this system only handles ticking, not effect application or event listening.
 *
 * Integration:
 *   - Called by HeroSystem and EnemySystem at the end of each turn.
 *   - Uses the Singleton pattern for global access: StatusEffectTickSystem.Instance.TickStatusEffects(...)
 *   - Accepts a list of CombatantView (heroes and enemies) to process.
 *   - Triggers GameActions (e.g., ApplyBurnGA) for effects that require action system integration.
 *
 * Extending:
 *   - To add a new status effect (e.g., poison, regeneration), add its ticking logic to TickStatusEffects().
 *   - For complex effects, consider extracting per-effect logic into helper methods or classes.
 *   - All per-turn status effect logic is centralized here for maintainability.
 *
 * Example usage:
 *   StatusEffectTickSystem.Instance.TickStatusEffects(allCombatants);
 *
 * See also:
 *   - HeroSystem.cs (calls this for heroes)
 *   - EnemySystem.cs (calls this for enemies)
 *   - CombatantView.cs (base class for all combatants)
 *   - ApplyBurnGA.cs (burn damage action)
 *   - StatusEffectType.cs (enum of all status effects)
 */
/// <summary>
/// Central system to process per-turn status effect ticking for all combatants (heroes and enemies)
/// </summary>
public class StatusEffectTickSystem : Singleton<StatusEffectTickSystem>
{
	/// <summary>
	/// Call this at the end of each turn to process status effect ticks for all combatants
	/// </summary>
	/// <param name="combatants">List of all combatants (heroes and enemies)</param>
	public void TickStatusEffects(List<CombatantView> combatants)
	{
		foreach (var combatant in combatants)
		{
			// Burn: apply burn damage and reduce stack
			int burnStacks = combatant.GetStatusEffectStacks(StatusEffectType.BURN);
			if (burnStacks > 0)
			{
				ApplyBurnGA applyBurnGA = new(burnStacks, combatant);
				ActionSystem.Instance.AddReaction(applyBurnGA);
			}
			// Invulnerable: reduce stack by 1
			int invulStacks = combatant.GetStatusEffectStacks(StatusEffectType.INVULNERABLE);
			if (invulStacks > 0)
			{
				combatant.RemoveStatusEffect(StatusEffectType.INVULNERABLE, 1);
			}
			// Add more status effect ticking logic here as needed
		}
	}
}
