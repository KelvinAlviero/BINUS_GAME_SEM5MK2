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

        // SNAP LOGIC  ---
        
        // Only run the risk if damage is high enough to matter
        if (finalDamage > snapThreshold)
        {
            // Calculate the specific chance for THIS moment
            // Example: If Intensity is 0.8 (Close), and MaxChance is 1.0
            // Then current chance is 80%.
            float currentSnapChance = intensity * maxsnapChance;
            // Roll the dice (0.0 to 1.0)
            float diceRoll = Random.value;
            // Debug to see the math in action
            // Debug.Log($"Risk: {currentSnapChance*100:F0}% (Rolled: {diceRoll:F2})");
            if (diceRoll < currentSnapChance)
            {
                if (dnaManager != null)
                {
                    dnaManager.DamageRandomStrand();
                    Debug.Log($"<color=red>SNAP!</color> Distance: {distance:F1}m | Chance was: {currentSnapChance*100:F0}%");
                }
            }
        }
        Debug.Log($"-- {gameObject.name}: {finalDamage:F1} (Dist: {distance:F1}m) --");
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