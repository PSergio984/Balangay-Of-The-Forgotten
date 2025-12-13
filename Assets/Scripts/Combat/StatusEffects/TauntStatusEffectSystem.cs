using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// System that enforces taunt targeting: if any hero has Taunt status, enemies must target that hero.
/// </summary>
public class TauntStatusEffectSystem : MonoBehaviour
{
	private void OnEnable()
	{
		// Subscribe to enemy attack targeting event (e.g., before AttackHeroGA is processed)
		ActionSystem.SubscribeReaction<AttackHeroGA>(OnEnemyAttackTargeting, ReactionTiming.PRE);
        // Subscribe to PerformEffectGA for enemy moves that target heroes (PRE)
        ActionSystem.SubscribeReaction<PerformEffectGA>(OnPerformEffectPre, ReactionTiming.PRE);
	}

	private void OnDisable()
	{
		ActionSystem.UnsubscribeReaction<AttackHeroGA>(OnEnemyAttackTargeting, ReactionTiming.PRE);
        ActionSystem.UnsubscribeReaction<PerformEffectGA>(OnPerformEffectPre, ReactionTiming.PRE);
	}

	/// <summary>
	/// Forces enemy to target a hero with Taunt if any exist
	/// </summary>
	/// <param name="attackHeroGA">The attack action about to be processed</param>
	private void OnEnemyAttackTargeting(AttackHeroGA attackHeroGA)
	{
		// Get all living heroes with Taunt status
		var heroes = HeroSystem.Instance?.HeroViews;
		if (heroes == null || heroes.Count == 0)
			return;

		List<HeroView> tauntingHeroes = new();
		foreach (var hero in heroes)
		{
			if (hero != null && !hero.IsDead && hero.GetStatusEffectStacks(StatusEffectType.TAUNT) > 0)
			{
				tauntingHeroes.Add(hero);
			}
		}
		
		// If any hero has Taunt, force targeting to one of them (random if multiple)
		if (tauntingHeroes.Count > 0)
		{
			var forcedTarget = tauntingHeroes[Random.Range(0, tauntingHeroes.Count)];
			attackHeroGA.Targets = new List<HeroView> { forcedTarget };
			Debug.Log($"[TauntSystem] Redirecting AttackHeroGA to taunting hero: {forcedTarget.name}");
		}
	}

	/// <summary>
	/// Intercepts PerformEffectGA for enemy moves that target heroes and enforces taunt
	/// </summary>
	/// <param name="performEffectGA">The effect action about to be processed</param>
	private void OnPerformEffectPre(PerformEffectGA performEffectGA)
	{
		// Only enforce for enemy-originated effects (skip hero cards)
		if (performEffectGA.Caster == null || !(performEffectGA.Caster is EnemyView))
			return;

		// Must have targets to redirect
		if (performEffectGA.Targets == null || performEffectGA.Targets.Count == 0)
			return;

		// Check if any targets are heroes (enemies can target themselves with buffs)
		bool hasHeroTargets = performEffectGA.Targets.Any(t => t is HeroView);
		if (!hasHeroTargets)
			return;

		// Get all living heroes with Taunt status
		var heroes = HeroSystem.Instance?.HeroViews;
		if (heroes == null || heroes.Count == 0)
			return;

		List<HeroView> tauntingHeroes = new();
		foreach (var hero in heroes)
		{
			if (hero != null && !hero.IsDead && hero.GetStatusEffectStacks(StatusEffectType.TAUNT) > 0)
			{
				tauntingHeroes.Add(hero);
			}
		}

		// If any hero has Taunt, redirect all hero targets to taunting heroes
		if (tauntingHeroes.Count > 0)
		{
			// Create new target list, replacing hero targets with taunting heroes
			List<CombatantView> newTargets = new();
			foreach (var target in performEffectGA.Targets)
			{
				if (target is HeroView)
				{
					// Replace hero target with a random taunting hero
					var forcedTarget = tauntingHeroes[Random.Range(0, tauntingHeroes.Count)];
					if (!newTargets.Contains(forcedTarget))
					{
						newTargets.Add(forcedTarget);
					}
				}
				else
				{
					// Keep non-hero targets (enemies can still target themselves)
					newTargets.Add(target);
				}
			}
			performEffectGA.Targets = newTargets;
			Debug.Log($"[TauntSystem] Redirecting PerformEffectGA from {performEffectGA.Caster.name} to taunting heroes");
		}
	}
}
