using UnityEngine;
using UnityEngine.UI;

public class SurgeryManager : MonoBehaviour
{
    public enum SurgeryPhase { Extraction, Insertion, Complete }
    public SurgeryPhase currentPhase = SurgeryPhase.Extraction;

    [Header("References")]
    public GameObject microscopePanel; // The whole canvas/panel
    public Transform socketLocation;   // The center point
    public GameObject brokenPiece;     // The object currently in the hole
    
    [Header("Phase 2 Spawns")]
    public GameObject replacementPrefabA; // Option 1
    public GameObject replacementPrefabB; // Option 2
    public Transform spawnPointA; // Left tray
    public Transform spawnPointB; // Right tray

    [Header("Extraction Settings")]
    public float extractionDistance = 300f; // How far away to pull the trash (in pixels/units)

    void Update()
    {
        // PHASE 1: EXTRACTION CHECK
        if (currentPhase == SurgeryPhase.Extraction && brokenPiece != null)
        {
            // Calculate distance between the Trash and the Socket
            float dist = Vector2.Distance(brokenPiece.transform.position, socketLocation.position);
            
            if (dist > extractionDistance)
            {
                FinishExtraction();
            }
        }
    }

    // Called when trash is far enough away
    void FinishExtraction()
    {
        Debug.Log("Extraction Complete! Spawning replacements...");
        currentPhase = SurgeryPhase.Insertion;

        // 1. Delete the trash (or make it float away)
        Destroy(brokenPiece);

        // 2. Spawn the two options
        GameObject optA = Instantiate(replacementPrefabA, spawnPointA.position, Quaternion.identity, microscopePanel.transform);
        GameObject optB = Instantiate(replacementPrefabB, spawnPointB.position, Quaternion.identity, microscopePanel.transform);
        
        // Ensure they are flagged as replacements
        optA.GetComponent<BioDraggable>().isReplacement = true;
        optB.GetComponent<BioDraggable>().isReplacement = true;

        // Reset Physics Z-Position just in case
        ResetZ(optA);
        ResetZ(optB);
    }

    void ResetZ(GameObject obj)
    {
        Vector3 pos = obj.transform.position;
        pos.z = 0;
        obj.transform.position = pos;
    }

    // Called by the Socket when a replacement is dropped in
    public void CompleteSurgery(BioDraggable finalPiece)
    {
        if (currentPhase == SurgeryPhase.Complete) return;

        currentPhase = SurgeryPhase.Complete;
        Debug.Log("SURGERY SUCCESSFUL!");

        // 1. Snap the piece into the center
        finalPiece.transform.position = socketLocation.position;
        finalPiece.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        finalPiece.enabled = false; // Disable dragging

        // 2. Close the Minigame after a delay
        Invoke("CloseMicroscope", 1.5f);
    }

    void CloseMicroscope()
    {
        // Hide the panel
        microscopePanel.SetActive(false);
        
        // Inform the Main DNA Manager that we won!
        FindObjectOfType<DNA_skeleton>().RepairCurrentTarget(); // We need to write this function next!
    }
}