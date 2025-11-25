using UnityEngine;
using UnityEngine.UI;

public class Stamina_BarScript : MonoBehaviour
{
    public Slider staminaBarSlider;
    public Slider staminaBarUISlider;
    public void SetMaxStamina(float stamina)
    {
        staminaBarSlider.maxValue = stamina;
        staminaBarSlider.value = stamina;
        // code bellow is temporary
        staminaBarUISlider.maxValue = stamina;
        staminaBarUISlider.value = stamina;
    }

    public void SetStamina(float stamina)
    {
        staminaBarSlider.value = stamina;
        // code bellow is temporary
        staminaBarUISlider.value = stamina;
    }
    
}
