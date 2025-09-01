using System;
using System.Collections;
using UnityEngine;

public class StaminaSystem : Singleton<StaminaSystem>
{
    [SerializeField] private StaminaUI StaminaUI;
    private const int MAX_STAMINA = 99;
    private int currentStamina = MAX_STAMINA;

    void OnEnable()
    {
        ActionSystem.AttachPerformer<SpendStaminaGA>(SpendStaminaPerformer);
        ActionSystem.AttachPerformer<RefillStaminaGA>(RefillStaminaPerformer);
        ActionSystem.SubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }
    void OnDisable()
    {
        ActionSystem.DetachPerformer<SpendStaminaGA>();
        ActionSystem.DetachPerformer<RefillStaminaGA>();
        ActionSystem.UnsubscribeReaction<EnemyTurnGA>(EnemyTurnPostReaction, ReactionTiming.POST);
    }

    public bool HasEnoughStamina(int stamina)
    {
        return currentStamina >= stamina;
    }

    private IEnumerator SpendStaminaPerformer(SpendStaminaGA spendStaminaGA)
    {
        currentStamina -= spendStaminaGA.Amount;
        StaminaUI.UpdateStaminaText(currentStamina);
        yield return null;
    }

    private IEnumerator RefillStaminaPerformer(RefillStaminaGA refillStaminaGA)
    {
        currentStamina = MAX_STAMINA;
        StaminaUI.UpdateStaminaText(currentStamina);
        yield return null;
    }

    private void EnemyTurnPostReaction(EnemyTurnGA enemyTurnGA)
    {
        RefillStaminaGA refillStaminaGA = new();
        ActionSystem.Instance.AddReaction(refillStaminaGA);
    }
}
