using UnityEngine;

public class Radiation : MonoBehaviour
{
    [Header("Settings")]
    public float checkInterval = 2.0f; 
    public float maxDamage = 50f;
    public float radiationFalloff = 1.0f;

    [Header("Snap Settings")]
    [Tooltip("Minimum damage required to risk snapping a DNA strand")]
    public float snapThreshold = 5.0f; 

    [Header("Double Strand Settings")]
    public float doubleStrandThreshold = 40.0f; // High damage requirement
    [Range(0, 1)] public float doubleStrandChance = 0.3f; // 30% chance if threshold met
    
    [Tooltip("0 = 0% chance, 1 = 100% chance to snap if threshold met")]
    [Range(0, 1)] public float maxsnapChance = 1.0f; 

    // References
    private CircleCollider2D radiationZone;
    private Transform playerTransform;
    private Player_Stats playerStatsScript;
    
    // NEW: Reference to your DNA Visuals
    private DNA_skeleton dnaManager; 

    private float timer = 0f;
    private bool isPlayerInside = false; 

    void Start()
    {
        radiationZone = GetComponent<CircleCollider2D>();
        radiationZone.isTrigger = true; 
        
        // NEW: Find the DNA Manager automatically in the scene!
        dnaManager = FindObjectOfType<DNA_skeleton>();

        if (dnaManager == null)
        {
            Debug.LogWarning("Radiation Source couldn't find the DNAStructure script! Visuals won't break.");
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

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
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        // Calculate Damage falloff (Option B: Alpha style)
        float intensity = 1.0f - (distance / range);
        intensity = Mathf.Clamp(intensity, 0f, radiationFalloff);

        // 2. Calculate Raw Damage
        float finalDamage = maxDamage * intensity;

        // 1. Deal HP Damage
        TakeRadiationDamage(finalDamage);

        if (dnaManager != null)
        {
            // 1. CHECK FOR DOUBLE STRAND BREAK (Critical Damage)
            if (finalDamage >= doubleStrandThreshold)
            {
                // Roll for Critical Break
                if (Random.value < doubleStrandChance)
                {
                    dnaManager.DamageDoubleStrand();
                    Debug.Log($"<color=purple>RADIATION SPIKE!</color> Damage {finalDamage:F1} caused DSB!");
                    return; // Return so we don't ALSO do a single break on the same tick
                }
            }

            // 2. CHECK FOR SINGLE STRAND BREAK (Standard Damage)
            // Only runs if we didn't just trigger a Double Break
            if (finalDamage > snapThreshold)
            {
                // Scale chance by intensity as before
                float currentSnapChance = intensity * maxsnapChance;
                
                if (Random.value < currentSnapChance)
                {
                    dnaManager.DamageRandomStrand();
                }
            }
        }

        Debug.Log($"-- {gameObject.name}: {finalDamage:F1} Damage --");
    }

    void TakeRadiationDamage(float finalDamage)
    {
        if (playerStatsScript != null)
        {
            playerStatsScript.TakeDamage(finalDamage);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Debug.Log($"Entered Radiation: {gameObject.name}");
            isPlayerInside = true;
            playerTransform = other.transform;
            
            // If the script is on a child/parent, use:
            playerStatsScript = other.GetComponentInParent<Player_Stats>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Debug.Log($"Safe from: {gameObject.name}");
            isPlayerInside = false;
            playerTransform = null;
            playerStatsScript = null;
        }
    }
}