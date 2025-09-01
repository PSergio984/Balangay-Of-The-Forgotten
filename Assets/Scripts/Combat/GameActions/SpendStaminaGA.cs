using UnityEngine;

public class SpendStaminaGA : GameAction
{
  public int Amount { get; set; }
   public SpendStaminaGA(int amount)
   {
       Amount = amount;
   }
}
