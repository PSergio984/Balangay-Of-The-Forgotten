using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Effect that applies team-wide Special Card buffs (DamageUp, DefenseUp, NoCooldown) based on SpecialCardData
/// </summary>
[Serializable]
public class SpecialCardEffect : Effects
{
    [SerializeField] private SpecialCardData specialCardData;

    public SpecialCardData SpecialCardData => specialCardData;

    public SpecialCardEffect() { }

    public SpecialCardEffect(SpecialCardData cardData)
    {
        specialCardData = cardData;
    }

    public override GameAction GetGameAction(List<CombatantView> targets, CombatantView caster)
    {
        if (specialCardData == null)
        {
            Debug.LogWarning("[SpecialCardEffect] SpecialCardData is null!");
            return null;
        }

        // Target all living player heroes for team-wide effect
        List<CombatantView> allHeroes = new List<CombatantView>();
        if (HeroSystem.Instance != null && HeroSystem.Instance.HeroViews != null)
        {
            allHeroes.AddRange(HeroSystem.Instance.HeroViews.Where(h => h != null && !h.IsDead));
        }

        if (allHeroes.Count == 0 && targets != null)
        {
            allHeroes = targets;
        }

        switch (specialCardData.EffectType)
        {
            case SpecialCardData.SpecialCardEffectType.DamageUp:
                return new ApplyDamageUpGA(allHeroes, specialCardData.EffectPercentage, specialCardData.Duration, specialCardData.CardName);

            case SpecialCardData.SpecialCardEffectType.DefenseUp:
                return new ApplyDefenseUpGA(allHeroes, specialCardData.EffectPercentage, specialCardData.Duration, specialCardData.CardName);

            case SpecialCardData.SpecialCardEffectType.NoCooldown:
                return new ApplyNoCooldownGA(allHeroes, specialCardData.Duration);

            default:
                Debug.LogWarning($"[SpecialCardEffect] Unsupported effect type: {specialCardData.EffectType}");
                return null;
        }
    }
}
