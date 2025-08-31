using UnityEngine;

public class PerformEffectGA : GameAction
{
    
    public Effects Effect { get; set; }

    public PerformEffectGA(Effects effect)
    {
            Effect = effect;
    }
}
