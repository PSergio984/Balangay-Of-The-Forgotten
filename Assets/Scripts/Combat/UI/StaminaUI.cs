using TMPro;
using UnityEngine;

public class StaminaUI : MonoBehaviour
{
    [SerializeField] private TMP_Text stamina;

    public void UpdateStaminaText(int currentStamina)
    {
        stamina.text = currentStamina.ToString();
    }
}
