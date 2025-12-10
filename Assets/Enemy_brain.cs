using UnityEngine;

public class Enemy_brain : MonoBehaviour
{
    [Header("Connections")]
    public Enemy_Script body;      // Drag the Enemy_Script here
    public PlayerProfiler spotter; // Drag the Player here
    public Transform playerTransform;

    [Header("Network Settings")]
    // Assuming you have a class named NeuralNetwork
    public NeuralNetwork neuralNet; 
    public float[] inputNodes;

    void Start()
    {
        // Initialize your network (Input Layer = 6, Hidden = 8, Output = 3)
        // Adjust these numbers based on your actual network structure
        neuralNet = new NeuralNetwork(new int[] { 6, 8, 3 }); 
        inputNodes = new float[6];
    }

    void Update()
    {
        // 1. GATHER INPUTS (The Sniper Scope)
        // ------------------------------------
        
        // Input 0: Relative Distance X (Normalized)
        float dist = (playerTransform.position.x - transform.position.x);
        inputNodes[0] = Mathf.Clamp(dist / 10f, -1f, 1f); 

        // Input 1: My Health %
        // (You might need to make currentHealth public in Enemy_Script)
        // inputNodes[1] = body.currentHealth / body.enemyHealth; 

        // Input 2: My Ammo Status
        inputNodes[2] = body.enemyRangeAmmo > 0 ? 1f : 0f;

        // Input 3: PLAYER HABITS (From the Spotter!)
        inputNodes[3] = spotter.jumpFrequency; 
        
        // Input 4: PLAYER AGGRESSION (From the Spotter!)
        inputNodes[4] = spotter.aggressionScore;

        // Input 5: Is Player close?
        inputNodes[5] = Mathf.Abs(dist) < 2.0f ? 1f : 0f;


        // 2. FEED FORWARD (Pull the Trigger)
        // ----------------------------------
        float[] outputs = neuralNet.FeedForward(inputNodes);


        // 3. EXECUTE OUTPUTS (Control the Body)
        // -------------------------------------
        
        // Output 0: Movement (Negative = Left, Positive = Right)
        if (Mathf.Abs(outputs[0]) > 0.1f) // Deadzone
        {
            body.MoveEnemy(outputs[0]); 
        }

        // Output 1: Melee Attack (Threshold)
        if (outputs[1] > 0.7f) 
        {
            body.AttemptMeleeAttack();
        }

        // Output 2: Ranged Attack (Threshold)
        if (outputs[2] > 0.7f)
        {
            body.AttemptRangeAttack();
        }
    }
}