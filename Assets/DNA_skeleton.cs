using UnityEngine;
using System.Collections.Generic; // Needed for Lists

public class DNA_skeleton : MonoBehaviour
{
    // This 'struct' defines what ONE row of your table looks like.
    // [System.Serializable] makes it show up in the Unity Inspector!
    [System.Serializable]
    public struct RungDefinition
{
    public string name;             
    public GameObject leftBase;     
    public GameObject rightBase;    
    
    // CHANGE 1: We now need to reference the actual SpriteRenderer, not just the GameObject
    public SpriteRenderer[] connectorRenderers; 
    
    // CHANGE 2: We need slots for the undamaged and damaged sprites
    public Sprite healthySprite; 
    public Sprite damagedSprite; 
    
    public bool isBroken; // We'll keep this flag for logic
}

    // This is the actual list you will see in the Editor
    public List<RungDefinition> dnaTable = new List<RungDefinition>();

    // New Function to visualize the damage
    public void ApplyDamageVisuals(int rowIndex)
    {
        if (rowIndex < dnaTable.Count)
        {
            // IMPORTANT: Since we are changing the struct's values (isBroken), 
            // we need to retrieve it, modify it, and put it back into the list.
            RungDefinition row = dnaTable[rowIndex]; 
            
            // Check if it's already broken to prevent re-damaging
            if (row.isBroken) return; 

            // Set the broken flag
            row.isBroken = true; 

            // Loop through all connectors in this specific row
            foreach(SpriteRenderer renderer in row.connectorRenderers)
            {
                // CHANGE: Swap the sprite to the damaged one
                if (renderer != null && row.damagedSprite != null)
                {
                    renderer.sprite = row.damagedSprite; 
                    // Optional: Change color for visual warning
                    renderer.color = Color.red; 
                }
            }
            
            // Update the list with the modified row struct
            dnaTable[rowIndex] = row; 

            Debug.Log(row.name + " is now damaged! Repair required.");
        }
    }
}