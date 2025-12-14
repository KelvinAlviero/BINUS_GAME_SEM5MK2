using UnityEngine;
using UnityEngine.UI;

public class HP_BarScript : MonoBehaviour
{
    public Slider hpBarSlider;
    public Slider hpBarUISlider;
    public void SetMaxHealth(float health)
    {
        hpBarSlider.maxValue = health;
        hpBarSlider.value = health;
        // code bellow is temporary
        //hpBarUISlider.maxValue = health;
        //hpBarUISlider.value = health;
    }

    public void SetHealth(float health) 
    {
        hpBarSlider.value = health;
        // code bellow is temporary
        //hpBarUISlider.value = health;
    }
}
