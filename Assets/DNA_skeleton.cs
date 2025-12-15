using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DNA_skeleton : MonoBehaviour
{
    // Changed to CLASS so we can edit it easily in the list
    [System.Serializable]
    public class RungDefinition
    {
        public string name;             // e.g., "Row 1"
        public GameObject leftBase;     // e.g., Adenine
        public GameObject rightBase;    // e.g., Thymine

        // CHANGE: Now we look for UI Images, not SpriteRenderers
        public Image[] connectors;
        // The Sprites
        public Sprite healthySprite;
        public Sprite damagedSprite;
    }

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

        // 1. Pick a Random Row
        int randomIndex = Random.Range(0, dnaTable.Count);
        RungDefinition row = dnaTable[randomIndex];

        // Safety Check: Does this row have connectors?
        if (row.connectors == null || row.connectors.Length == 0)
        {
            Debug.LogWarning($"Tried to damage {row.name}, but it has no connectors!");
            return;
        }

        // 2. Pick a Random Connector within that row (The "Single Strand" logic)
        int randomConnectorIndex = Random.Range(0, row.connectors.Length);
        Image targetStrand = row.connectors[randomConnectorIndex];

        // 3. Apply the Damage Visuals ONLY to that one strand
        if (targetStrand != null && row.damagedSprite != null)
        {
            // Check if it's already damaged to avoid redundant logs
            if (targetStrand.sprite == row.damagedSprite)
            {
                Debug.Log($"<color=orange>Refused:</color> {row.name} strand {randomConnectorIndex} is already broken!");
                return;
            }

            targetStrand.sprite = row.damagedSprite;
            targetStrand.color = Color.white; // Ensure it's visible

            Debug.Log($"<color=red>SNAP!</color> {row.name} :: Connector {randomConnectorIndex} broke.");
        }
    }

    // --- TEST BUTTON ---
    // Right-click the component and select "TEST: Random Damage"
    [ContextMenu("TEST: Random Damage")]
    public void TestRandomDamage()
    {
        DamageRandomStrand();
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