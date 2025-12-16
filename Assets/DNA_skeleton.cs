using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum DNABaseType 
{ 
    Adenine,  // Stamina
    Thymine,  // HP
    Guanine,  // Jump
    Cytosine  // Attack
}

public class DNA_skeleton : MonoBehaviour
{

    // Changed to CLASS so we can edit it easily in the list
    [System.Serializable]
    public class RungDefinition
    {
        public string name;             // e.g., "Row 1"
        public GameObject leftBase;     // e.g., Adenine
        public GameObject rightBase;    // e.g., Thymine

        [Header("Base Genetics")]
        public DNABaseType leftBaseType; 
        public GameObject leftBaseObj; // The GameObject for the Left Icon
        
        public DNABaseType rightBaseType;
        public GameObject rightBaseObj; // The GameObject for the Right Icon
        // CHANGE: Now we look for UI Images, not SpriteRenderers
        public Image[] connectors;
        // The Sprites
        public Sprite healthySprite;
        public Sprite damagedSprite;
        public Sprite greyedOutSprite; // dead sprite
        public Sprite deadBaseSprite;

        public bool isDead = false;
    }

    [Header("Repair System")]
    public GameObject repairButtonPrefab; // Drag your Button Prefab here!
    public Transform repairButtonContainer; // Assign your Canvas/Panel here

    public List<RungDefinition> dnaTable = new List<RungDefinition>();

    void Start()
    {
        // --- DIAGNOSTIC CHECK (Runs automatically on Play) ---
        RunSystemCheck();
    }

    void RunSystemCheck()
    {
        Debug.Log($"<color=cyan>--- STARTING DNA INTEGRITY CHECK ---</color>");
        
        for (int i = 0; i < dnaTable.Count; i++)
        {
            RungDefinition row = dnaTable[i];

            // 1. Check Name
            string rowName = string.IsNullOrEmpty(row.name) ? $"Row {i}" : row.name;

            // 2. Check Bases
            if (row.leftBase == null || row.rightBase == null)
            {
                Debug.LogError($"[ERROR] {rowName} is missing a Left or Right Base GameObject!");
            }

            // 3. Check Connectors
            if (row.connectors == null || row.connectors.Length == 0)
            {
                Debug.LogError($"[ERROR] {rowName} has NO connectors assigned!");
            }
            else
            {
                // Check if any connector in the list is empty
                for (int c = 0; c < row.connectors.Length; c++)
                {
                    if (row.connectors[c] == null)
                        Debug.LogError($"[ERROR] {rowName}: Connector element {c} is empty/null!");
                }
            }

            // 4. Check Sprites
            if (row.healthySprite == null || row.damagedSprite == null)
            {
                Debug.LogWarning($"[WARNING] {rowName} is missing a Healthy or Damaged sprite. Visuals won't swap.");
            }

            // If we get here safely
            Debug.Log($"[OK] {rowName}: Connected {row.leftBase?.name} to {row.rightBase?.name} with {row.connectors?.Length} connectors.");
        }
        
        Debug.Log($"<color=cyan>--- CHECK COMPLETE ---</color>");
    }

    // --- THE ACTION FUNCTIONS ---

    public void DamageRandomStrand()
    {
        if (dnaTable.Count == 0) return;

        int rowIndex = Random.Range(0, dnaTable.Count);
        RungDefinition row = dnaTable[rowIndex];

        if (row.isDead) return;
        if (row.connectors == null || row.connectors.Length == 0) return;

        // Pick a specific connector to break
        int connectorIndex = Random.Range(0, row.connectors.Length);
        Image targetStrand = row.connectors[connectorIndex];

        // Only proceed if it's currently healthy
        if (targetStrand != null && targetStrand.sprite == row.healthySprite)
        {
            // Visual snap
            targetStrand.sprite = row.damagedSprite;
            targetStrand.color = Color.white;

            // --- THE FIX: CALCULATE PROXIMITY ---
            // We verify which "half" of the bridge this connector belongs to.
            
            bool isLeftVictim = false;

            if (row.connectors.Length == 1)
            {
                // If there is only 1 connector, it touches both sides.
                // We fall back to random (or you can default to Left if you prefer)
                isLeftVictim = (Random.value > 0.5f);
            }
            else
            {
                // If we have 2+ connectors (e.g., Index 0 and 1)
                // Length is 2. Midpoint is 1.0.
                // Index 0 is < 1.0 (Left). Index 1 is >= 1.0 (Right).
                float midpoint = row.connectors.Length / 2f;
                
                isLeftVictim = (connectorIndex < midpoint);
            }

            DNABaseType victimType = isLeftVictim ? row.leftBaseType : row.rightBaseType;
            GameObject victimObj = isLeftVictim ? row.leftBaseObj : row.rightBaseObj;

            // Get the image component
            Image baseImage = victimObj.GetComponent<Image>();
            
            if (baseImage == null)
            {
                Debug.LogError($"[Error] No Image component found on {victimObj.name}!");
                return;
            }

            Debug.Log($"<color=red>SNAP!</color> Connector {connectorIndex} broke. Nearest Base: {victimType}");

            // Spawn the repair button
            SpawnRepairButton(targetStrand, baseImage, row, victimType);
        }
    }

    void SpawnRepairButton(Image connector, Image baseTarget, RungDefinition row, DNABaseType victim)
    {
        if (repairButtonPrefab == null) return;

        GameObject btnObj = Instantiate(repairButtonPrefab, repairButtonContainer);
        
        btnObj.transform.SetAsLastSibling();
        
        // Position button over the CONNECTOR (so player knows where to click)
        btnObj.transform.position = connector.transform.position;

        RepairNode node = btnObj.GetComponent<RepairNode>();
        if (node != null)
        {
            // PASS THE BASE IMAGE, NOT THE CONNECTOR IMAGE for the "Death" target
            node.Initialize(victim, connector, baseTarget, row.healthySprite, row.deadBaseSprite);
        }
    }


    // --- DOUBLE STRAND BREAK LOGIC ---
    public void DamageDoubleStrand()
    {
        if (dnaTable.Count == 0) return;

        // 1. Pick a Random Row
        int randomIndex = Random.Range(0, dnaTable.Count);
        RungDefinition row = dnaTable[randomIndex];

        // Safety: We need at least 2 connectors to have a "Double" break next to each other
        if (row.connectors == null || row.connectors.Length < 2)
        {
            // Fallback: If there's only 1 connector, just break that one
            Debug.LogWarning($"Row {row.name} only has 1 connector, doing single break instead.");
            DamageRandomStrand(); 
            return;
        }

        // 2. Logic: Pick a start index that allows for a neighbor
        // If length is 2, we must pick index 0 (so we break 0 and 1).
        // If length is 3, we can pick 0 (0,1) or 1 (1,2).
        int maxStartIndex = row.connectors.Length - 1; 
        int firstIndex = Random.Range(0, maxStartIndex); // Random 0 to Length-2
        int secondIndex = firstIndex + 1; // The neighbor "right next to it"

        // 3. Apply Damage to BOTH
        Image connectorA = row.connectors[firstIndex];
        Image connectorB = row.connectors[secondIndex];

        bool snappedSomething = false;

        // Break A
        if (connectorA != null && row.damagedSprite != null)
        {
            connectorA.sprite = row.damagedSprite;
            connectorA.color = Color.white;
            snappedSomething = true;
        }

        // Break B (The neighbor)
        if (connectorB != null && row.damagedSprite != null)
        {
            connectorB.sprite = row.damagedSprite;
            connectorB.color = Color.white;
            snappedSomething = true;
        }

        if (snappedSomething)
        {
            Debug.Log($"<color=purple>CRITICAL:</color> Double Strand Break on {row.name}! (Connectors {firstIndex} & {secondIndex})");
        }
    }
    
    // --- TEST BUTTON ---
    // Right-click the component and select "TEST: Random Damage"
    [ContextMenu("TEST: Random Damage")]
    public void TestRandomDamage()
    {
        DamageRandomStrand();
    }
    
    // Test Button
    [ContextMenu("TEST: Double Strand Break")]
    public void TestDSB()
    {
        DamageDoubleStrand();
    }

    // Helper to reset everything (Optional, handy for testing)
    [ContextMenu("TEST: Reset All")]
    public void ResetAll()
    {
        foreach (var row in dnaTable)
        {
            foreach (var connector in row.connectors)
            {
                if(connector != null && row.healthySprite != null)
                    connector.sprite = row.healthySprite;
            }
        }
        Debug.Log("DNA Repaired.");
    }
}