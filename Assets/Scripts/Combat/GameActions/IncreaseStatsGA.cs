using UnityEngine;

public class IncreaseStatsGA : MonoBehaviour
{
    // public Minion TargetMinion;
    public int AttackIncreaseAmount;
    public int DefenseIncreaseAmount;
    //balik nalang yung minion target minion pag iiimplement na
    public IncreaseStatsGA(int attackIncreaseAmount, int defenseIncreaseAmount)
    {
        AttackIncreaseAmount = attackIncreaseAmount;
        DefenseIncreaseAmount = defenseIncreaseAmount;
    }
}
