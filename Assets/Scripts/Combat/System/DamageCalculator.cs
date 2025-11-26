using UnityEngine;

/// <summary>
/// Core combat calculation system for Balangay turn-based combat
/// All methods are static and use formulas from damage-calculation-instructions.md
/// </summary>
public static class DamageCalculator
{
    #region Damage Calculation

    /// <summary>
    /// Calculates skill power from base damage and stat scaling
    /// Formula: baseDamage + (AttackAmp × ATK) + (MagicAmp × MAG)
    /// </summary>
    /// <param name="baseDamage">Flat base damage of the skill</param>
    /// <param name="attackAmp">Physical damage multiplier (e.g., 1.5 = 150% ATK scaling)</param>
    /// <param name="magicAmp">Magical damage multiplier (e.g., 1.0 = 100% MAG scaling)</param>
    /// <param name="attackPower">Caster's ATK stat</param>
    /// <param name="magicPower">Caster's MAG stat</param>
    /// <returns>Calculated skill power before defense/amplification</returns>
    public static float CalculateSkillPower(float baseDamage, float attackAmp, float magicAmp, float attackPower, float magicPower)
    {
        float skillPower = baseDamage + (attackAmp * attackPower) + (magicAmp * magicPower);
        
        Debug.Log($"[DamageCalculator] SkillPower Calculation: " +
                  $"Base={baseDamage}, AtkAmp={attackAmp}, MagAmp={magicAmp}, " +
                  $"ATK={attackPower}, MAG={magicPower} → SkillPower={skillPower}");
        
        return skillPower;
    }

    /// <summary>
    /// Calculates final damage using the core combat formula
    /// Formula: [skillPower × DMG_AMP / (C × (1 + DEF_final × 0.01))] × CRIT_MULTIPLIER
    /// </summary>
    /// <param name="skillPower">Calculated skill power (from CalculateSkillPower)</param>
    /// <param name="damageAmplification">Damage amplification from buffs (1.0 = 100%)</param>
    /// <param name="coefficient">Attacker coefficient (1.0 for player, 1.5 for enemy)</param>
    /// <param name="targetDefense">Target's final defense value after all modifiers</param>
    /// <param name="critMultiplier">Critical multiplier (1.0 = no crit, 1.5 = player crit, 1.2 = enemy crit)</param>
    /// <returns>Final damage as integer (minimum 1)</returns>
    public static int CalculateFinalDamage(float skillPower, float damageAmplification, float coefficient, float targetDefense, float critMultiplier)
    {
        // Guard: Prevent division by zero for coefficient
        const float MIN_COEFFICIENT = 1e-4f;
        float safeCoefficient = coefficient;
        if (Mathf.Abs(coefficient) < MIN_COEFFICIENT)
        {
            Debug.LogWarning($"[DamageCalculator] Coefficient was zero or too small (input={coefficient}), clamped to {MIN_COEFFICIENT} to prevent division by zero.");
            safeCoefficient = MIN_COEFFICIENT;
        }
        float denominator = safeCoefficient * (1f + targetDefense * 0.01f);
        float rawDamage = (skillPower * damageAmplification) / denominator;
        float finalDamage = rawDamage * critMultiplier;
        int result = Mathf.Max(1, Mathf.RoundToInt(finalDamage));
        
        Debug.Log($"[DamageCalculator] FinalDamage Calculation: " +
                  $"SkillPower={skillPower}, Amp={damageAmplification}, Coeff={safeCoefficient}, " +
                  $"Def={targetDefense}, Crit={critMultiplier} → RawDmg={rawDamage:F2}, FinalDmg={result}");
        
        return result;
    }

    /// <summary>
    /// LEGACY METHOD - Calculates final damage using the provided DND-like formula
    /// </summary>
    /// <param name="baseDamage">Base damage of the card</param>
    /// <param name="dmgAmp">Amplification multiplier (DMG_AMP, e.g. 1.2 for +20%)</param>
    /// <param name="coefficient">Player/Boss coefficient (C: 1 for Player→Boss, 1.5 for Boss→Player)</param>
    /// <param name="defFinal">Final defense value (DEF_final)</param>
    /// <param name="critMultiplier">Critical multiplier (CRITMUL: 1.5 or 1.2)</param>
    /// <returns>Final integer damage value</returns>
    [System.Obsolete("Use CalculateSkillPower + CalculateFinalDamage for better control")]
    public static int CalculateDamage(float baseDamage, float dmgAmp, float coefficient, float defFinal, float critMultiplier)
    {
        // DMG = [DMG_skill * DMG_AMP / (C * (1 + DEF_final * 0.01)) ] * CRITMUL
        float denominator = coefficient * (1f + defFinal * 0.01f);
        float raw = (baseDamage * dmgAmp) / denominator;
        float finalDmg = raw * critMultiplier;
        int result = Mathf.Max(1, Mathf.RoundToInt(finalDmg)); // Minimum 1 damage for consistency
        Debug.Log($"[DamageCalculator] LEGACY CalculateDamage: " +
                  $"Base={baseDamage}, Amp={dmgAmp}, Coeff={coefficient}, " +
                  $"Def={defFinal}, Crit={critMultiplier} → Result={result} (min 1 enforced)");
        return result;
    }

    #endregion

    #region Defense Calculation

    /// <summary>
    /// Applies defense buff to base defense
    /// Formula: DEF × (1 + buff_percent)
    /// </summary>
    /// <param name="baseDef">Base defense value</param>
    /// <param name="buffPercent">Buff percentage (0.3 = +30% DEF)</param>
    /// <returns>Buffed defense value</returns>
    public static float ApplyDefenseBuff(float baseDef, float buffPercent)
    {
        float result = baseDef * (1f + buffPercent);
        
        Debug.Log($"[DamageCalculator] DefenseBuff: " +
                  $"Base={baseDef}, Buff%={buffPercent * 100}% → Result={result}");
        
        return result;
    }

    /// <summary>
    /// Applies defense reduction debuff
    /// Formula: DEF × (1 - reduction_percent)
    /// </summary>
    /// <param name="currentDef">Current defense value</param>
    /// <param name="reductionPercent">Reduction percentage (0.1 = -10% DEF)</param>
    /// <returns>Reduced defense value</returns>
    public static float ApplyDefenseReduction(float currentDef, float reductionPercent)
    {
        float result = currentDef * (1f - reductionPercent);
        
        Debug.Log($"[DamageCalculator] DefenseReduction: " +
                  $"Current={currentDef}, Reduction%={reductionPercent * 100}% → Result={result}");
        
        return result;
    }

    /// <summary>
    /// Applies defense ignore from attacker
    /// Formula: DEF × (1 - ignore_percent)
    /// </summary>
    /// <param name="currentDef">Current defense value</param>
    /// <param name="ignorePercent">Ignore percentage (1.0 = ignore 100% DEF)</param>
    /// <returns>Defense after ignore effect</returns>
    public static float ApplyDefenseIgnore(float currentDef, float ignorePercent)
    {
        float result = currentDef * (1f - ignorePercent);
        
        Debug.Log($"[DamageCalculator] DefenseIgnore: " +
                  $"Current={currentDef}, Ignore%={ignorePercent * 100}% → Result={result}");
        
        return result;
    }

    /// <summary>
    /// Calculates final defense after all modifiers in correct order:
    /// 1. Apply buffs (multiplicative)
    /// 2. Apply reductions (multiplicative)
    /// 3. Apply ignore effects (multiplicative)
    /// </summary>
    /// <param name="baseDef">Base defense value</param>
    /// <param name="buffPercents">Array of buff percentages</param>
    /// <param name="reductionPercents">Array of reduction percentages</param>
    /// <param name="ignorePercents">Array of ignore percentages</param>
    /// <returns>Final defense value (minimum 0)</returns>
    public static float CalculateFinalDefense(float baseDef, float[] buffPercents, float[] reductionPercents, float[] ignorePercents)
    {
        float def = baseDef;
        
        Debug.Log($"[DamageCalculator] FinalDefense Calculation Start: BaseDef={baseDef}");
        
        // Step 1: Apply all buffs (multiplicative)
        foreach (float buff in buffPercents)
        {
            def *= (1f + buff);
            Debug.Log($"  → Applied Buff +{buff * 100}%: DEF={def}");
        }
        
        // Step 2: Apply all reductions (multiplicative)
        foreach (float reduction in reductionPercents)
        {
            def *= (1f - reduction);
            Debug.Log($"  → Applied Reduction -{reduction * 100}%: DEF={def}");
        }
        
        // Step 3: Apply all ignore effects (multiplicative)
        foreach (float ignore in ignorePercents)
        {
            def *= (1f - ignore);
            Debug.Log($"  → Applied Ignore -{ignore * 100}%: DEF={def}");
        }
        
        float result = Mathf.Max(0f, def);
        
        Debug.Log($"[DamageCalculator] FinalDefense Result: {result} (cannot go below 0)");
        
        return result;
    }

    #endregion

    #region Critical Hit System

    /// <summary>
    /// Determines if an attack is a critical hit using d20 roll system
    /// Formula: Roll d20 → if 20 = auto crit, else check if roll ≤ (critChance × 20)
    /// </summary>
    /// <param name="critChance">Critical chance (0-1, e.g., 0.25 = 25%)</param>
    /// <returns>True if critical hit</returns>
    public static bool IsCriticalHit(float critChance)
    {
        int roll = Random.Range(1, 21); // Roll d20 (1-20)
        bool isCrit = false;
        
        if (roll == 20)
        {
            isCrit = true;
            Debug.Log($"[DamageCalculator] CritCheck: Roll={roll} → AUTOMATIC CRIT!");
        }
        else
        {
            float threshold = critChance * 20f;
            isCrit = roll <= threshold;
            Debug.Log($"[DamageCalculator] CritCheck: Roll={roll}, Threshold={threshold:F2} ({critChance * 100}% chance) → {(isCrit ? "CRIT!" : "Normal")}");
        }
        
        return isCrit;
    }

    /// <summary>
    /// Gets the critical multiplier based on attacker type
    /// Player: 1.5× damage | Enemy: 1.2× damage
    /// </summary>
    /// <param name="isPlayer">True if player is attacking, false if enemy</param>
    /// <returns>Critical multiplier (1.5 for player, 1.2 for enemy)</returns>
    public static float GetCriticalMultiplier(bool isPlayer)
    {
        float multiplier = isPlayer ? 1.5f : 1.2f;
        
        Debug.Log($"[DamageCalculator] CritMultiplier: Attacker={(isPlayer ? "Player" : "Enemy")} → {multiplier}×");
        
        return multiplier;
    }

    /// <summary>
    /// LEGACY METHOD - Calculates if an attack is a critical hit, based on crit chance and a d20 roll
    /// </summary>
    /// <param name="critChance">Chance to crit (0-1, e.g. 0.25 for 25%)</param>
    /// <returns>True if crit, false otherwise</returns>
    [System.Obsolete("Use IsCriticalHit instead for consistency")]
    public static bool CalculateCrit(float critChance)
    {
        // Simulate d20 roll: 20 = crit, else check crit chance
        int roll = Random.Range(1, 21);
        if (roll == 20) return true; // automatic crit
        // Otherwise, crit if roll is within crit threshold
        float threshold = critChance * 20f;
        return roll <= threshold;
    }

    #endregion

    #region Accuracy System

    /// <summary>
    /// Determines if an attack hits the target using d20 roll system
    /// Formula: Roll d20 → if 1 = auto miss, if 20 = auto hit, else check if roll ≤ (accuracy × 20)
    /// </summary>
    /// <param name="accuracy">Accuracy (0-1, e.g., 0.75 = 75% hit chance)</param>
    /// <returns>True if attack hits</returns>
    public static bool IsHit(float accuracy)
    {
        int roll = Random.Range(1, 21); // Roll d20 (1-20)
        bool isHit = false;
        
        if (roll == 1)
        {
            isHit = false;
            Debug.Log($"[DamageCalculator] AccuracyCheck: Roll={roll} → AUTOMATIC MISS!");
        }
        else if (roll == 20)
        {
            isHit = true;
            Debug.Log($"[DamageCalculator] AccuracyCheck: Roll={roll} → AUTOMATIC HIT!");
        }
        else
        {
            float threshold = accuracy * 20f;
            isHit = roll <= threshold;
            Debug.Log($"[DamageCalculator] AccuracyCheck: Roll={roll}, Threshold={threshold:F2} ({accuracy * 100}% accuracy) → {(isHit ? "HIT" : "MISS")}");
        }
        
        return isHit;
    }

    /// <summary>
    /// LEGACY METHOD - Calculates if an attack hits, based on accuracy and a d20 roll
    /// </summary>
    /// <param name="accuracy">Chance to hit (0-1, e.g. 0.75 for 75%)</param>
    /// <returns>True if hit, false if miss</returns>
    [System.Obsolete("Use IsHit instead for consistency")]
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

    #endregion

    #region Status Effect Modifiers

    /// <summary>
    /// Applies all damage amplification buffs (multiplicative stacking)
    /// Formula: base_damage × buff1 × buff2 × buff3 × ...
    /// </summary>
    /// <param name="baseDamage">Base damage value</param>
    /// <param name="multipliers">Array of damage multipliers (1.2 = +20% damage)</param>
    /// <returns>Amplified damage value</returns>
    public static float ApplyDamageAmplification(float baseDamage, params float[] multipliers)
    {
        float result = baseDamage;
        
        Debug.Log($"[DamageCalculator] DamageAmplification Start: BaseDmg={baseDamage}");
        
        foreach (float mult in multipliers)
        {
            result *= mult;
            Debug.Log($"  → Applied Multiplier {mult}× ({(mult - 1f) * 100:+0;-0}%): Damage={result}");
        }
        
        Debug.Log($"[DamageCalculator] DamageAmplification Result: {result}");
        
        return result;
    }

    /// <summary>
    /// Calculates damage over time from caster's magic power
    /// Formula: caster.MAG × dot_percent
    /// </summary>
    /// <param name="casterMagic">Caster's MAG stat</param>
    /// <param name="dotPercent">DoT percentage (0.2 = 20% MAG per turn)</param>
    /// <returns>DoT damage per turn</returns>
    public static int CalculateDoTDamage(float casterMagic, float dotPercent)
    {
        int result = Mathf.RoundToInt(casterMagic * dotPercent);
        
        Debug.Log($"[DamageCalculator] DoTDamage: " +
                  $"CasterMAG={casterMagic}, DoT%={dotPercent * 100}% → Damage/Turn={result}");
        
        return result;
    }

    /// <summary>
    /// Calculates damage based on target's current HP percentage
    /// Formula: target.currentHP × damage_percent
    /// </summary>
    /// <param name="targetCurrentHP">Target's current HP</param>
    /// <param name="damagePercent">Damage percentage (0.3 = 30% current HP)</param>
    /// <returns>Calculated HP damage (minimum 1)</returns>
    public static int CalculateCurrentHPDamage(int targetCurrentHP, float damagePercent)
    {
        int result = Mathf.Max(1, Mathf.RoundToInt(targetCurrentHP * damagePercent));
        
        Debug.Log($"[DamageCalculator] CurrentHPDamage: " +
                  $"TargetHP={targetCurrentHP}, Damage%={damagePercent * 100}% → Damage={result}");
        
        return result;
    }

    #endregion
}
