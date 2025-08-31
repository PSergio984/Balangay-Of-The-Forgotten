using UnityEngine;
using System.Collections;
public class EnemySystem : MonoBehaviour
{
    //performers
    void OnEnable()
    {
        ActionSystem.AttachPerformer<EnemyTurnGA>(EnemyTurnPerformer);
    }
    void OnDisable()
    {
        ActionSystem.DetachPerformer<EnemyTurnGA>();
    }
   // This class will manage enemy behavior and actions
    private IEnumerator EnemyTurnPerformer(EnemyTurnGA enemyTurnGA)
    {
        Debug.Log("Performing enemy turn actions");
        yield return new WaitForSeconds(2);
        Debug.Log("Enemy turn actions complete");
    }
}
