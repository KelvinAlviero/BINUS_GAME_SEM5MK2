using UnityEngine;

public class Beta_Radiation : MonoBehaviour
{
    [Header("Settings")]
    public float checkInterval = 2.0f; 
    public float maxDamage = 50f;

    private CircleCollider2D radiationZone;
    private Transform playerTransform;
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
            //Does math when inside
            Debug.Log($"Inside!!");
            CalculateDamage();
        }
        else
        {

            Debug.Log($"BetaRads: Not inside");
        }
    }

    void CalculateDamage()
    {
        float range = radiationZone.radius;
        
        // Center of the circle
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        // Calculator
        float damagePercent = 1.0f - (distance / range);
        damagePercent = Mathf.Clamp(damagePercent, 0f, 1.0f);

        float finalDamage = maxDamage * damagePercent;

        Debug.Log($"-- BetaDamage: {finalDamage:F1} (Dist: {distance:F1}m) --");
    }

    // Trigger logic
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            playerTransform = other.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            playerTransform = null;
        }
    }
}