using UnityEngine;

public static class DamageCalculator
{
    /// <summary>
    /// Calculates the final damage using the provided DND-like formula.
    /// </summary>
    /// <param name="baseDamage">Base damage of the card</param>
    /// <param name="dmgAmp">Amplification multiplier (DMG_AMP, e.g. 1.2 for +20%)</param>
    /// <param name="coefficient">Player/Boss coefficient (C: 1 for Player→Boss, 1.5 for Boss→Player)</param>
    /// <param name="defFinal">Final defense value (DEF_final)</param>
    /// <param name="critMultiplier">Critical multiplier (CRITMUL: 1.5 or 1.2)</param>
    /// <returns>Final integer damage value</returns>
    public static int CalculateDamage(float baseDamage, float dmgAmp, float coefficient, float defFinal, float critMultiplier)
    {
        // DMG = [DMG_skill * DMG_AMP / (C * (1 + DEF_final * 0.01)) ] * CRITMUL
        float denominator = coefficient * (1f + defFinal * 0.01f);
        float raw = (baseDamage * dmgAmp) / denominator;
        float finalDmg = raw * critMultiplier;
        return Mathf.Max(Mathf.RoundToInt(finalDmg), 0);
    }

    /// <summary>
    /// Calculates if an attack hits, based on accuracy and a d20 roll.
    /// </summary>
    /// <param name="accuracy">Chance to hit (0-1, e.g. 0.75 for 75%)</param>
   /// <returns>True if hit, false if miss</returns>
    public static bool CalculateAccuracy(float accuracy)
    {
        // Simulate d20 roll: 1 = miss, 20 = crit, else check accuracy
        int roll = Random.Range(1, 21); // 1-20 inclusive
        if (roll == 1) return false; // automatic miss
        if (roll == 20) return true; // automatic hit/crit (handle crit elsewhere)
        // Otherwise, hit if roll is within accuracy threshold
        float threshold = accuracy * 20f;
        return roll <= threshold;
    }

    /// <summary>
    /// Calculates if an attack is a critical hit, based on crit chance and a d20 roll.
    /// </summary>
    /// <param name="critChance">Chance to crit (0-1, e.g. 0.25 for 25%)</param>
    /// <returns>True if crit, false otherwise</returns>
    public static bool CalculateCrit(float critChance)
    {
        // Simulate d20 roll: 20 = crit, else check crit chance
        int roll = Random.Range(1, 21);
        if (roll == 20) return true; // automatic crit
        // Otherwise, crit if roll is within crit threshold
        float threshold = critChance * 20f;
        return roll <= threshold;
    }

}
