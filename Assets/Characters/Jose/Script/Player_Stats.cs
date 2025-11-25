using UnityEngine;

public class Player_Stats : MonoBehaviour
{
    [Header("PlayerStats")]
    public float playerMaxHealth = 100f;
    public float playerMaxStamina = 100f;
    public float staminaRegenRate = 1.0f;

    [Header("References")]
    public HP_BarScript hp_BarScript;
    public Stamina_BarScript stamina_BarScript;

    private float playerCurrentHealth;
    private float playerCurrentStamina;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCurrentHealth = playerMaxHealth;
        playerCurrentStamina = playerMaxStamina;
        stamina_BarScript.SetMaxStamina(playerMaxStamina);
        hp_BarScript.SetMaxHealth(playerMaxHealth);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerCurrentStamina < playerMaxStamina)
        {
            playerCurrentStamina += staminaRegenRate * Time.deltaTime;
            playerCurrentStamina = Mathf.Clamp(playerCurrentStamina, 0f, playerMaxStamina);
            UpdateStaminaBar();
        }
    }

    public void DrainStamina(float amount)
    {
        playerCurrentStamina -= amount;
        UpdateStaminaBar();
    }

    public bool CanSpendStamina(float amount)
    {
        return playerCurrentStamina >= amount;
    }

    private void UpdateStaminaBar()
    {
        if (stamina_BarScript != null)
        {
            //float percentage = playerCurrentStamina / playerMaxStamina;
            stamina_BarScript.SetStamina(playerCurrentStamina); 
        }
    }

    public void TakeDamage(float damage)
    {
        playerCurrentHealth -= damage;
        playerMaxStamina -= damage;
        if (playerCurrentStamina > playerMaxStamina) DrainStamina(damage); 

        //playerCurrentStamina = playerMaxStamina;

        hp_BarScript.SetHealth(playerCurrentHealth);
        // temporary
        //stamina_BarScript.SetStamina(playerCurrentStamina);

        if (playerCurrentHealth <= 0)
        {
            Death();
        }
    }

    private void Death() 
    {
        Debug.Log("Player has died");
    }
}
