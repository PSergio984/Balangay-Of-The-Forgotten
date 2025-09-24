using UnityEngine;

using System.Collections.Generic;

/// <summary>
/// System that enforces taunt targeting: if any hero has Taunt status, enemies must target that hero.
/// </summary>
public class TauntStatusEffectSystem : MonoBehaviour
{
	private void OnEnable()
	{
		// Subscribe to enemy attack targeting event (e.g., before AttackHeroGA is processed)
		ActionSystem.SubscribeReaction<AttackHeroGA>(OnEnemyAttackTargeting, ReactionTiming.PRE);
        // Subscribe to PerformEffectGA for auto-targeted enemy moves (PRE)
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
		// Get all heroes
		var heroes = HeroSystem.Instance.HeroViews;
		// Find all heroes with Taunt status
		List<HeroView> tauntingHeroes = new();
		foreach (var hero in heroes)
		{
			if (hero.GetStatusEffectStacks(StatusEffectType.TAUNT) > 0)
			{
				tauntingHeroes.Add(hero);
			}
		}
		// If any hero has Taunt, force targeting to one of them (random if multiple)
		if (tauntingHeroes.Count > 0)
		{
			var forcedTarget = tauntingHeroes[Random.Range(0, tauntingHeroes.Count)];
			attackHeroGA.Targets = new List<HeroView> { forcedTarget };
		}
		// Otherwise, default targeting remains
	}

	/// <summary>
	/// Intercepts PerformEffectGA for single-target random attacks and enforces taunt
	/// </summary>
	/// <param name="performEffectGA">The effect action about to be processed</param>
	private void OnPerformEffectPre(PerformEffectGA performEffectGA)
	{
		// Only enforce for enemy-originated effects (skip hero cards)
		// You may need to add more robust checks if PerformEffectGA is used for both sides

		if (performEffectGA.Effect == null || performEffectGA.Targets == null || performEffectGA.Targets.Count != 1)
			return;

		// Check if the effect's TargetMode is RandomTargetTM
		var effect = performEffectGA.Effect;
		var targetModeProp = effect.GetType().GetProperty("TargetMode");
		if (targetModeProp == null)
			return;
		var targetMode = targetModeProp.GetValue(effect, null);
		if (targetMode == null || targetMode.GetType().Name != "RandomTargetTM")
			return;

		// Find all heroes with Taunt status
		var heroes = HeroSystem.Instance.HeroViews;
		List<HeroView> tauntingHeroes = new();
		foreach (var hero in heroes)
		{
			if (hero.GetStatusEffectStacks(StatusEffectType.TAUNT) > 0)
			{
				tauntingHeroes.Add(hero);
			}
		}
		// If any hero has Taunt, force targeting to one of them (random if multiple)
		if (tauntingHeroes.Count > 0)
		{
			var forcedTarget = tauntingHeroes[Random.Range(0, tauntingHeroes.Count)];
			performEffectGA.Targets = new List<CombatantView> { forcedTarget };
		}
		// Otherwise, default targeting remains
	}
}
