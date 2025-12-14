using UnityEngine;

public class Player_Stats : MonoBehaviour
{
    [Header("PlayerStats")]
    [SerializeField] private float playerMaxHealth = 100f;
    [SerializeField] private float playerMaxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 1.0f;
    [SerializeField] private float staminaBlockDrain = 30f;

    [Header("References")]
    public GameObject hp_BarGameObject;
    public GameObject stamina_BarGameObject;
    private Stamina_BarScript stamina_BarScript;
    private HP_BarScript hp_BarScript;
    [SerializeField] private AudioClip hurtSoundEffect;
    [SerializeField] private AudioClip blockSoundEffect;
    public bool isBlocking = false;

    private float _playerCurrentHealth;
    private float _playerCurrentStamina;

    // GETTERS AND SETTERS

    // Current Health
    public float playerCurrentHealth
    {
        get { return _playerCurrentHealth; }
        set
        {
            _playerCurrentHealth = Mathf.Clamp(value, 0f, playerMaxHealth);
            if (hp_BarScript != null)
            {
                hp_BarScript.SetHealth(_playerCurrentHealth);
            }
        }
    }

    // Max Health
    public float playerMaxHealthValue
    {
        get { return playerMaxHealth; }
        set
        {
            playerMaxHealth = Mathf.Max(0f, value);
            if (hp_BarScript != null)
            {
                hp_BarScript.SetMaxHealth(playerMaxHealth);
            }
        }
    }

    // Current Stamina
    public float playerCurrentStamina
    {
        get { return _playerCurrentStamina; }
        set
        {
            _playerCurrentStamina = Mathf.Clamp(value, 0f, playerMaxStamina);
            UpdateStaminaBar();
        }
    }

    // Max Stamina
    public float playerMaxStaminaValue
    {
        get { return playerMaxStamina; }
        set
        {
            playerMaxStamina = Mathf.Max(0f, value);
            if (stamina_BarScript != null)
            {
                stamina_BarScript.SetMaxStamina(playerMaxStamina);
            }
        }
    }

    void Awake()
    {
        stamina_BarScript = stamina_BarGameObject.GetComponent<Stamina_BarScript>();
        hp_BarScript = hp_BarGameObject.GetComponent<HP_BarScript>();
    }

    void Start()
    {
        playerCurrentHealth = playerMaxHealth;
        playerCurrentStamina = playerMaxStamina;
        stamina_BarScript.SetMaxStamina(playerMaxStamina);
        hp_BarScript.SetMaxHealth(playerMaxHealth);
    }

    void Update()
    {
        if (playerCurrentStamina < playerMaxStamina)
        {
            playerCurrentStamina += staminaRegenRate * Time.deltaTime;
        }
    }

    public void DrainStamina(float amount)
    {
        playerCurrentStamina -= amount;
    }

    public bool CanSpendStamina(float amount)
    {
        return playerCurrentStamina >= amount;
    }

    private void UpdateStaminaBar()
    {
        if (stamina_BarScript != null)
        {
            stamina_BarScript.SetStamina(_playerCurrentStamina);
        }
    }

    public void TakeDamage(float damage)
    {
        if (CheckIfBlocking())
        {
            AudioManager.instance.PlaySoundFXClipWithRandomPitch(blockSoundEffect, transform, 0.5f);
            DataLogger.Instance.LogPlayerDamage(damage, true);
            return;
        }

        Hitstop.instance.Stop(0.1f);
        AudioManager.instance.PlaySoundFXClipWithRandomPitch(hurtSoundEffect, transform, 0.5f);
        DataLogger.Instance.LogPlayerDamage(damage, false);
        
        playerCurrentHealth -= damage;
        playerMaxStaminaValue -= damage; // Using the setter to update max stamina

        if (playerCurrentStamina > playerMaxStamina)
        {
            DrainStamina(damage);
        }

        if (playerCurrentHealth <= 0)
        {
            DataLogger.Instance.LogPlayerDeath();
            Death();
        }
    }

    private bool CheckIfBlocking()
    {
        if (isBlocking)
        {
            if (playerCurrentStamina >= staminaBlockDrain)
            {
                DrainStamina(staminaBlockDrain);
                return true;
            }
            else
            {
                Debug.Log("Not enough stamina to block!");
                return false;
            }
        }
        return false;
    }

    private void Death()
    {
        Debug.Log("Player has died");
    }
}