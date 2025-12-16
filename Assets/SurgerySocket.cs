using UnityEngine;

public class SurgerySocket : MonoBehaviour
{
    public SurgeryManager manager; // Reference to the mmain script

    // Trigger logic for insertion 
    void OnTriggerEnter2D(Collider2D other)
    {
        // We only care about this if we are in the Insertion Phase
        if (manager.currentPhase == SurgeryManager.SurgeryPhase.Insertion)
        {
            // Check if the object is a "Replacement"
            BioDraggable piece = other.GetComponent<BioDraggable>();
            
            if (piece != null && piece.isReplacement)
            {
                // Surgery complete
                manager.CompleteSurgery(piece);
            }
        }
    }
}