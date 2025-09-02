using System.Collections.Generic;
using UnityEngine;

public class RandomTargetTM : TargetMode
{
   public override List<CombatantView> GetTargets()
   {
       CombatantView target = EnemySystem.Instance.EnemyViews[Random.Range(0, EnemySystem.Instance.EnemyViews.Count)];
       return new() { target };
   }
}
