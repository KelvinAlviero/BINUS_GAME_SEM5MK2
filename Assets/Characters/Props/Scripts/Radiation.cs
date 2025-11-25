using UnityEngine;

public class Radiation : MonoBehaviour
{
    [Header("Settings")]
    public float checkInterval = 2.0f; 
    public float maxDamage = 50f;
    public float radiationFalloff = 1.0f;

    private CircleCollider2D radiationZone;
    private Transform playerTransform;
    private Player_Stats playerStatsScript;
    private float timer = 0f;
    private bool isPlayerInside = false; 

    // Pulls Circle collider from Unity
    void Start()
    {
        radiationZone = GetComponent<CircleCollider2D>();
        radiationZone.isTrigger = true; 
    }

    void Update()
    {
        
        timer += Time.deltaTime;

        // 2. Checker
        if (timer >= checkInterval)
        {
            ThreeSecondCheck();
            timer = 0f; 
        }
    }

    void ThreeSecondCheck()
    {
        
        if (isPlayerInside)
        {
            CalculateDamage();
        }
    }

    void CalculateDamage()
    {
        float range = radiationZone.radius;
        
        // Center of the circle
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        // Calculator
        float damagePercent = 1.0f - (distance / range);
        damagePercent = Mathf.Clamp(damagePercent, 0f, radiationFalloff);

        float finalDamage = maxDamage * damagePercent;

        TakeRadiationDamage(finalDamage);

        Debug.Log($"-- {gameObject.name}: {finalDamage:F1} (Dist: {distance:F1}m) --");
    }


    void TakeRadiationDamage(float finalDamage)
    {
        if (playerStatsScript != null)
        {
            playerStatsScript.TakeDamage(finalDamage);
        }
        return;
    }

    // Trigger logic
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"You are taking radiation damage from {gameObject.name}");
            isPlayerInside = true;
            playerTransform = other.transform;
            playerStatsScript = other.GetComponentInParent<Player_Stats>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"You are outside {gameObject.name} range");
            isPlayerInside = false;
            playerTransform = null;
            playerStatsScript = null;
        }
    }
}