using UnityEngine;

public class NeuralNetwork
{
    // Layer sizes
    private int inputSize;
    private int hiddenSize;
    private int outputSize;

    // Weights: connections between layers
    // weights_input_hidden[i][j] = connection from input i to hidden j
    private float[][] weights_input_hidden;
    private float[][] weights_hidden_output;

    // Biases: adjustment values for each neuron
    private float[] biases_hidden;
    private float[] biases_output;

    // Constructor: Set up the network structure
    public NeuralNetwork(int inputNodes, int hiddenNodes, int outputNodes)
    {
        inputSize = inputNodes;
        hiddenSize = hiddenNodes;
        outputSize = outputNodes;

        // Initialize weight arrays
        weights_input_hidden = new float[inputSize][];
        for (int i = 0; i < inputSize; i++)
        {
            weights_input_hidden[i] = new float[hiddenSize];
        }

        weights_hidden_output = new float[hiddenSize][];
        for (int i = 0; i < hiddenSize; i++)
        {
            weights_hidden_output[i] = new float[outputSize];
        }

        // Initialize bias arrays
        biases_hidden = new float[hiddenSize];
        biases_output = new float[outputSize];

        // Set initial smart weights
        InitializeSmartWeights();
    }

    // FEEDFORWARD: This is where the "thinking" happens
    // Takes inputs → processes through network → returns outputs
    public float[] FeedForward(float[] inputs)
    {
        // Validate input
        if (inputs.Length != inputSize)
        {
            Debug.LogError($"Wrong input size! Expected {inputSize}, got {inputs.Length}");
            return new float[outputSize];
        }

        // STEP 1: Calculate Hidden Layer
        float[] hidden = new float[hiddenSize];
        for (int h = 0; h < hiddenSize; h++)
        {
            float sum = biases_hidden[h]; // Start with bias

            // Add weighted inputs
            for (int i = 0; i < inputSize; i++)
            {
                sum += inputs[i] * weights_input_hidden[i][h];
            }

            // Apply activation function (ReLU - Rectified Linear Unit)
            hidden[h] = ReLU(sum);
        }

        // STEP 2: Calculate Output Layer
        float[] outputs = new float[outputSize];
        for (int o = 0; o < outputSize; o++)
        {
            float sum = biases_output[o]; // Start with bias

            // Add weighted hidden values
            for (int h = 0; h < hiddenSize; h++)
            {
                sum += hidden[h] * weights_hidden_output[h][o];
            }

            // Apply activation function (Sigmoid for outputs)
            outputs[o] = Sigmoid(sum);
        }

        return outputs;
    }

    // ACTIVATION FUNCTIONS
    // These help the network learn non-linear patterns

    // ReLU: Returns 0 for negative, keeps positive values
    // Good for hidden layers - fast and effective
    private float ReLU(float x)
    {
        return Mathf.Max(0, x);
    }

    // Sigmoid: Squashes values between 0 and 1
    // Good for outputs - gives us probabilities
    private float Sigmoid(float x)
    {
        return 1.0f / (1.0f + Mathf.Exp(-x));
    }

    // INITIALIZE SMART WEIGHTS
    // Hand-tuned to make the enemy behave intelligently
    private void InitializeSmartWeights()
    {
        // For now, use small random weights
        // We'll tune these based on your game behavior
        System.Random rand = new System.Random();

        // Input to Hidden weights
        for (int i = 0; i < inputSize; i++)
        {
            for (int h = 0; h < hiddenSize; h++)
            {
                // Random values between -0.5 and 0.5
                weights_input_hidden[i][h] = (float)(rand.NextDouble() - 0.5);
            }
        }

        // Hidden to Output weights
        for (int h = 0; h < hiddenSize; h++)
        {
            for (int o = 0; o < outputSize; o++)
            {
                weights_hidden_output[h][o] = (float)(rand.NextDouble() - 0.5);
            }
        }

        // Biases
        for (int h = 0; h < hiddenSize; h++)
        {
            biases_hidden[h] = (float)(rand.NextDouble() - 0.5);
        }

        for (int o = 0; o < outputSize; o++)
        {
            biases_output[o] = (float)(rand.NextDouble() - 0.5);
        }

        // We'll manually tune important connections next
        TuneWeightsForGameplay();
    }

    // TUNE WEIGHTS FOR GAMEPLAY
    // Manually set important weights based on game logic
    // Replace the TuneWeightsForGameplay() method in NeuralNetwork.cs
    private void TuneWeightsForGameplay()
    {
        // ==================================================================
        // NEURAL NETWORK WEIGHTS FOR ENEMY AI
        // Network: 14 inputs, 8 hidden nodes, 6 outputs
        // ==================================================================

        // Clear all weights first
        for (int i = 0; i < weights_input_hidden.Length; i++)
        {
            for (int j = 0; j < weights_input_hidden[i].Length; j++)
            {
                weights_input_hidden[i][j] = 0f;
            }
        }

        for (int i = 0; i < weights_hidden_output.Length; i++)
        {
            for (int j = 0; j < weights_hidden_output[i].Length; j++)
            {
                weights_hidden_output[i][j] = 0f;
            }
        }

        // ==================== INPUT MAPPING ====================
        // [0]  = normalized horizontal distance (player.x - enemy.x)
        // [1]  = normalized vertical distance (player.y - enemy.y)
        // [2]  = wall in front (1 if wall, 0 if not)
        // [3]  = player horizontal velocity (normalized)
        // [4]  = player health percentage
        // [5]  = player stamina percentage
        // [6]  = ENEMY health percentage (THIS IS KEY!)
        // [7]  = enemy melee attack flag
        // [8]  = enemy ranged attack flag
        // [9]  = player attacking flag
        // [10] = player blocking flag
        // [11] = player jumping flag (vertical velocity > 0.1)
        // [12] = enemy dodging flag
        // [13] = normalized distance to player (0-1)
        // =======================================================

        // === HIDDEN NODE 0: MOVEMENT (Horizontal positioning) ===
        weights_input_hidden[0][0] = 2.5f;    // Horizontal distance -> move
        weights_input_hidden[2][0] = -3.0f;   // Wall in front -> stop moving forward
        weights_input_hidden[13][0] = -1.0f;  // Close distance -> less movement

        // === HIDDEN NODE 1: DEFENSIVE PRESSURE ===
        weights_input_hidden[9][1] = 2.0f;    // Player attacking -> defensive pressure
        weights_input_hidden[13][1] = 1.5f;   // Close distance -> more pressure
        weights_input_hidden[4][1] = -1.0f;   // Player low health -> less pressure needed

        // === HIDDEN NODE 2: HEALTH-BASED DEFENSE (Blocks more when low health) ===
        // KEY: Negative weight from health means more defense when health is LOW
        weights_input_hidden[6][2] = -2.5f;   // ENEMY health -> defense (negative = more when low)
        weights_input_hidden[9][2] = 2.0f;    // Player attacking -> trigger defense
        weights_input_hidden[13][2] = 1.0f;   // Close distance -> more defense

        // === HIDDEN NODE 3: HEALTH-BASED DODGE ===
        weights_input_hidden[6][3] = -2.0f;   // ENEMY health -> dodge (negative = more when low)
        weights_input_hidden[9][3] = 1.5f;    // Player attacking -> dodge
        weights_input_hidden[6][3] = 1.0f;    // Heavy attack -> dodge (if you add heavy attack detection)

        // === HIDDEN NODE 4: MELEE AGGRESSION (More aggressive when healthy) ===
        weights_input_hidden[6][4] = 2.0f;    // ENEMY health -> melee (positive = more when healthy)
        weights_input_hidden[13][4] = 3.0f;   // Close distance -> melee
        weights_input_hidden[10][4] = -1.5f;  // Player blocking -> less melee

        // === HIDDEN NODE 5: RANGED & PURSUIT (For fleeing enemies) ===
        // This node activates when player is jumping/retreating
        weights_input_hidden[11][5] = 2.5f;   // Player jumping -> use ranged!
        weights_input_hidden[3][5] = 1.5f;    // Player moving away (negative velocity) -> ranged
        weights_input_hidden[13][5] = 2.0f;   // Medium distance -> ranged
        weights_input_hidden[10][5] = 1.0f;   // Player blocking -> use ranged

        // === HIDDEN NODE 6: AGGRESSION CONTROL ===
        weights_input_hidden[6][6] = 2.5f;    // ENEMY health -> aggression (positive = more when healthy)
        weights_input_hidden[4][6] = -1.0f;   // Player low health -> finish them!
        weights_input_hidden[5][6] = 0.8f;    // Player low stamina -> be aggressive

        // === HIDDEN NODE 7: URGENCY (When enemy is low health) ===
        weights_input_hidden[6][7] = -3.0f;   // ENEMY low health -> urgent mode
        weights_input_hidden[4][7] = 1.5f;    // Player also low health -> desperate

        // =============== OUTPUT LAYER CONNECTIONS ===============

        // Output 0: WalkRight
        weights_hidden_output[0][0] = 3.0f;   // Movement -> WalkRight

        // Output 1: WalkLeft  
        weights_hidden_output[0][1] = -3.0f;  // Movement -> WalkLeft (inverse)

        // Output 2: Block
        weights_hidden_output[1][2] = 2.0f;   // Defensive pressure -> Block
        weights_hidden_output[2][2] = 3.0f;   // Health-based defense -> Block
        weights_hidden_output[7][2] = 2.0f;   // Urgency -> Block when low health

        // Output 3: Dodge
        weights_hidden_output[1][3] = 1.5f;   // Defensive pressure -> Dodge
        weights_hidden_output[3][3] = 2.5f;   // Health-based dodge -> Dodge
        weights_hidden_output[7][3] = 1.5f;   // Urgency -> Dodge when low health

        // Output 4: Melee Attack
        weights_hidden_output[4][4] = 3.5f;   // Melee aggression -> Melee
        weights_hidden_output[6][4] = 1.5f;   // Aggression control -> Melee
        weights_hidden_output[7][4] = -1.0f;  // Urgency -> Less melee when desperate

        // Output 5: Ranged Attack
        weights_hidden_output[5][5] = 4.0f;   // Ranged & pursuit -> Ranged (STRONG for fleeing)
        weights_hidden_output[6][5] = 1.0f;   // Aggression control -> Ranged
        weights_hidden_output[7][5] = 1.5f;   // Urgency -> More ranged when desperate

        // ==================== BIASES ====================

        // Hidden layer biases
        biases_hidden[0] = -0.3f;  // Movement (slight negative)
        biases_hidden[1] = -1.0f;  // Defensive pressure (needs strong signal)
        biases_hidden[2] = -0.8f;  // Health defense (activates below 50% health)
        biases_hidden[3] = -1.0f;  // Dodge (needs strong signal)
        biases_hidden[4] = -1.5f;  // Melee (needs close range to activate)
        biases_hidden[5] = -0.5f;  // Ranged (easier to activate)
        biases_hidden[6] = 0.5f;   // Aggression (positive bias = aggressive by default)
        biases_hidden[7] = -2.0f;  // Urgency (only activates when VERY low health)

        // Output layer biases
        biases_output[0] = 0.0f;   // WalkRight (neutral)
        biases_output[1] = 0.0f;   // WalkLeft (neutral)
        biases_output[2] = -0.5f;  // Block (slightly negative = don't block randomly)
        biases_output[3] = -1.0f;  // Dodge (negative = don't dodge without reason)
        biases_output[4] = -1.5f;  // Melee (negative = don't spam attacks)
        biases_output[5] = -0.8f;  // Ranged (slightly negative)

        Debug.Log("Neural network weights tuned for adaptive gameplay!");
    }

    public void AdjustBehaviorBasedOnHealth(float currentHealthPercent)
    {
        // This function can be called to dynamically adjust weights based on health
        // For example, make the AI smarter when low on health

        if (currentHealthPercent < 0.3f)
        {
            // When very low health, increase defensive tendencies
            weights_hidden_output[2][2] = 4.0f;  // Stronger blocking
            weights_hidden_output[3][3] = 3.0f;  // Stronger dodging

            // Reduce risky attacks
            weights_hidden_output[4][4] = 2.0f;  // Weaker melee
            weights_hidden_output[5][5] = 3.0f;  // Still decent ranged

            // Increase survival instinct
            biases_hidden[7] = 0.0f;  // Urgency node always active
        }
        else if (currentHealthPercent < 0.5f)
        {
            // When below 50% health, start being more defensive
            weights_hidden_output[2][2] = 3.5f;  // Increased blocking
            weights_hidden_output[3][3] = 2.5f;  // Increased dodging
        }
        else
        {
            // Above 50% health - normal/aggressive behavior
            weights_hidden_output[2][2] = 3.0f;  // Normal blocking
            weights_hidden_output[3][3] = 2.0f;  // Normal dodging
            weights_hidden_output[4][4] = 3.5f;  // Strong melee
            weights_hidden_output[5][5] = 4.0f;  // Strong ranged
        }
    }

    // SAVE/LOAD WEIGHTS (For future pre-training option)
    public void SaveWeights(string filename)
    {
        // TODO: Implement JSON serialization
        Debug.Log("Saving weights to: " + filename);
    }

    public void LoadWeights(string filename)
    {
        // TODO: Implement JSON deserialization
        Debug.Log("Loading weights from: " + filename);
    }
}