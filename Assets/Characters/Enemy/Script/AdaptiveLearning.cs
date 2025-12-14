using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ADAPTIVE LEARNING SYSTEM FOR RESEARCH
/// 
/// This system makes the Neural Network AI learn during combat by:
/// 1. Tracking player behavior patterns (jump attacks, blocking, aggression)
/// 2. Monitoring success/failure of AI defensive/offensive actions
/// 3. Adapting neural network weights based on what works
/// 4. Logging all adaptations for research analysis
/// 
/// This creates TRUE adaptive AI that forces players to evolve their strategy
/// </summary>
public class AdaptiveLearning : MonoBehaviour
{
    [Header("References")]
    public NeuralNetwork neuralNet;
    public Enemy_Script enemyBody;
    public GameObject player;

    [Header("Learning Settings")]
    [Tooltip("How often to analyze patterns and adapt (seconds)")]
    public float adaptationInterval = 10f;

    [Tooltip("How much to adjust weights each adaptation (0.1 = 10% change)")]
    public float learningRate = 0.15f;

    [Tooltip("Minimum sample size before adapting")]
    public int minSampleSize = 5;

    [Header("Pattern Tracking")]
    private float lastAdaptationTime = 0f;

    // Player behavior tracking
    private int totalPlayerAttacks = 0;
    private int jumpAttacks = 0;
    private int dashAttacks = 0;
    private int groundAttacks = 0;
    private int playerBlocks = 0;
    private int playerDodges = 0;

    // AI action tracking
    private int blockAttempts = 0;
    private int blockSuccesses = 0;
    private int blockFailures = 0;

    private int dodgeAttempts = 0;
    private int dodgeSuccesses = 0;
    private int dodgeFailures = 0;

    private int meleeAttempts = 0;
    private int meleeHits = 0;
    private int meleeMisses = 0;

    private int rangedAttempts = 0;
    private int rangedHits = 0;
    private int rangedMisses = 0;

    // State tracking
    private bool lastPlayerAttacking = false;
    private bool lastPlayerJumping = false;
    private bool lastPlayerDashing = false;
    private bool lastAIBlocking = false;
    private bool lastAIDodging = false;

    private float lastHealthCheck = 0f;
    private float damageTakenSinceLastCheck = 0f;

    [Header("Adaptation State")]
    public bool hasAdapted = false;
    public int adaptationCount = 0;
    public List<string> adaptationHistory = new List<string>();

    void Start()
    {
        // AGGRESSIVE TESTING SETTINGS - You'll DEFINITELY notice this!
        adaptationInterval = 5f;      // Adapt every 5 seconds (was 10)
        learningRate = 0.4f;          // BIG weight changes (was 0.15)
        minSampleSize = 3;            // Only need 3 attacks (was 5)

        lastAdaptationTime = Time.time;
        lastHealthCheck = enemyBody.GetHealthPercentage();

        Debug.Log("<color=cyan>╔════════════════════════════════════════╗</color>");
        Debug.Log("<color=cyan>║   ADAPTIVE AI - TESTING MODE          ║</color>");
        Debug.Log("<color=cyan>║   Interval: 5s | Rate: 0.4 | Min: 3   ║</color>");
        Debug.Log("<color=cyan>╚════════════════════════════════════════╝</color>");
    }

    void Update()
    {
        TrackPlayerBehavior();
        TrackAIPerformance();

        // Adapt periodically
        if (Time.time - lastAdaptationTime >= adaptationInterval)
        {
            // DEBUG: Always print what we've tracked so far
            Debug.Log($"<color=yellow>--- PATTERN ANALYSIS ({Time.time:F1}s) ---</color>");
            Debug.Log($"Total Attacks: {totalPlayerAttacks}");
            Debug.Log($"Jump Attacks: {jumpAttacks} ({(totalPlayerAttacks > 0 ? (float)jumpAttacks / totalPlayerAttacks * 100 : 0):F1}%)");
            Debug.Log($"Dash Attacks: {dashAttacks} ({(totalPlayerAttacks > 0 ? (float)dashAttacks / totalPlayerAttacks * 100 : 0):F1}%)");
            Debug.Log($"Ground Attacks: {groundAttacks} ({(totalPlayerAttacks > 0 ? (float)groundAttacks / totalPlayerAttacks * 100 : 0):F1}%)");
            Debug.Log($"Block Attempts: {blockAttempts} | Success: {blockSuccesses} | Fail: {blockFailures}");
            Debug.Log($"Dodge Attempts: {dodgeAttempts} | Success: {dodgeSuccesses}");
            Debug.Log($"<color=yellow>---------------------------------------</color>");

            AdaptToPlayerBehavior();
            lastAdaptationTime = Time.time;
        }
    }

    // ========================================================================
    // PATTERN TRACKING
    // ========================================================================

    private void TrackPlayerBehavior()
    {
        if (player == null) return;

        Player_Attack playerAttack = player.GetComponent<Player_Attack>();
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        Player_Stats playerStats = player.GetComponent<Player_Stats>();

        bool playerAttacking = playerAttack != null && playerAttack.isAttacking;
        bool playerJumping = playerRb != null && Mathf.Abs(playerRb.linearVelocity.y) > 0.1f;
        bool playerDashing = playerRb != null && Mathf.Abs(playerRb.linearVelocity.x) > 8f;

        // Detect attack START (rising edge)
        if (playerAttacking && !lastPlayerAttacking)
        {
            totalPlayerAttacks++;

            // Categorize attack type
            if (playerJumping)
            {
                jumpAttacks++;
                DataLogger.Instance.LogPlayerAction("JumpAttack", Vector2.Distance(transform.position, player.transform.position));
            }
            else if (playerDashing)
            {
                dashAttacks++;
                DataLogger.Instance.LogPlayerAction("DashAttack", Vector2.Distance(transform.position, player.transform.position));
            }
            else
            {
                groundAttacks++;
                DataLogger.Instance.LogPlayerAction("GroundAttack", Vector2.Distance(transform.position, player.transform.position));
            }
        }

        // Track player blocking
        if (playerStats != null && playerStats.isBlocking)
        {
            playerBlocks++;
        }

        lastPlayerAttacking = playerAttacking;
        lastPlayerJumping = playerJumping;
        lastPlayerDashing = playerDashing;
    }

    private void TrackAIPerformance()
    {
        // Track blocking performance
        bool currentlyBlocking = enemyBody.isBlocking;
        if (currentlyBlocking && !lastAIBlocking)
        {
            blockAttempts++;
        }
        lastAIBlocking = currentlyBlocking;

        // Track dodging
        bool currentlyDodging = enemyBody.isDodging;
        if (currentlyDodging && !lastAIDodging)
        {
            dodgeAttempts++;
        }
        lastAIDodging = currentlyDodging;

        // Track damage taken (indicates defensive failure)
        float currentHealth = enemyBody.GetHealthPercentage();
        if (currentHealth < lastHealthCheck)
        {
            float damageTaken = lastHealthCheck - currentHealth;
            damageTakenSinceLastCheck += damageTaken;

            // If we were blocking/dodging and still took damage = failure
            if (lastAIBlocking)
            {
                blockFailures++;
            }
            else if (lastAIDodging)
            {
                dodgeFailures++;
            }
        }
        else if (lastPlayerAttacking && (lastAIBlocking || lastAIDodging))
        {
            // Successfully defended
            if (lastAIBlocking)
            {
                blockSuccesses++;
            }
            else if (lastAIDodging)
            {
                dodgeSuccesses++;
            }
        }

        lastHealthCheck = currentHealth;
    }

    // Called from Enemy_Script when attacks hit/miss
    public void OnMeleeAttempt(bool hit)
    {
        meleeAttempts++;
        if (hit)
        {
            meleeHits++;
        }
        else
        {
            meleeMisses++;
        }
    }

    public void OnRangedAttempt(bool hit)
    {
        rangedAttempts++;
        if (hit)
        {
            rangedHits++;
        }
        else
        {
            rangedMisses++;
        }
    }

    // ========================================================================
    // ADAPTIVE LEARNING
    // ========================================================================

    private void AdaptToPlayerBehavior()
    {
        if (neuralNet == null || totalPlayerAttacks < minSampleSize)
        {
            return;
        }

        Debug.Log($"<color=cyan>=== ADAPTIVE LEARNING CYCLE #{adaptationCount + 1} ===</color>");

        bool adapted = false;

        // Calculate player pattern frequencies
        float jumpAttackRate = (float)jumpAttacks / totalPlayerAttacks;
        float dashAttackRate = (float)dashAttacks / totalPlayerAttacks;
        float groundAttackRate = (float)groundAttacks / totalPlayerAttacks;

        // === ADAPTATION 1: Counter Jump Attacks ===
        if (jumpAttackRate > 0.35f) // Player jump attacks > 35% of the time
        {
            AdaptToDodgeJumpAttacks(jumpAttackRate);
            adapted = true;
        }

        // === ADAPTATION 2: Counter Dash Attacks ===
        if (dashAttackRate > 0.3f) // Player dash attacks > 30% of the time
        {
            AdaptToDodgeDashAttacks(dashAttackRate);
            adapted = true;
        }

        // === ADAPTATION 3: Improve Blocking if Failing ===
        if (blockAttempts > 5)
        {
            float blockSuccessRate = (float)blockSuccesses / blockAttempts;
            if (blockSuccessRate < 0.4f) // Less than 40% success
            {
                ImproveBlockTiming();
                adapted = true;
            }
        }

        // === ADAPTATION 4: If Player Blocks Often, Use Ranged ===
        if (playerBlocks > totalPlayerAttacks * 0.3f) // Player blocks > 30%
        {
            AdaptToRangedStrategy();
            adapted = true;
        }

        // === ADAPTATION 5: If Melee Failing, Switch to Ranged ===
        if (meleeAttempts > 5)
        {
            float meleeSuccessRate = (float)meleeHits / meleeAttempts;
            if (meleeSuccessRate < 0.3f) // Less than 30% hit rate
            {
                AdaptToPreferRanged();
                adapted = true;
            }
        }

        // === ADAPTATION 6: Adjust Aggression Based on Damage ===
        if (damageTakenSinceLastCheck > 0.15f) // Lost > 15% health
        {
            AdaptToMoreDefensive();
            adapted = true;
        }

        if (adapted)
        {
            hasAdapted = true;
            adaptationCount++;

            // Update health-based behavior
            neuralNet.AdjustBehaviorBasedOnHealth(enemyBody.GetHealthPercentage());

            Debug.Log($"<color=cyan>Adaptation #{adaptationCount} complete. Total patterns learned: {adaptationHistory.Count}</color>");
        }
        else
        {
            Debug.Log("<color=yellow>No significant patterns detected. Maintaining current behavior.</color>");
        }

        // Reset damage counter
        damageTakenSinceLastCheck = 0f;
    }

    // ========================================================================
    // SPECIFIC ADAPTATIONS
    // ========================================================================

    private void AdaptToDodgeJumpAttacks(float jumpRate)
    {
        // Increase dodge response to jumping players
        // This makes the AI dodge more when player is airborne

        // Strengthen the connection: player jumping → dodge node
        neuralNet.weights_input_hidden[11][3] += learningRate * 2f;

        // Increase dodge priority when player jumps
        neuralNet.weights_hidden_output[3][3] += learningRate;

        string adaptation = $"Increased dodge response to jump attacks (Player jump attack rate: {jumpRate * 100:F0}%)";
        adaptationHistory.Add(adaptation);

        float distance = Vector2.Distance(transform.position, player.transform.position);
        DataLogger.Instance.LogAdaptation("DodgeJumpAttacks", jumpRate, distance);

        Debug.Log($"<color=green>[ADAPTED] {adaptation}</color>");
    }

    private void AdaptToDodgeDashAttacks(float dashRate)
    {
        // Player dashes a lot, so increase dodge sensitivity to fast movement

        neuralNet.weights_input_hidden[3][3] += learningRate * 1.5f; // Player velocity → dodge
        neuralNet.biases_hidden[3] -= learningRate * 0.5f; // Make dodge node easier to activate

        string adaptation = $"Increased dodge sensitivity to dash attacks (Dash rate: {dashRate * 100:F0}%)";
        adaptationHistory.Add(adaptation);

        float distance = Vector2.Distance(transform.position, player.transform.position);
        DataLogger.Instance.LogAdaptation("DodgeDashAttacks", dashRate, distance);

        Debug.Log($"<color=green>[ADAPTED] {adaptation}</color>");
    }

    private void ImproveBlockTiming()
    {
        // Blocking is failing, make it more responsive

        float blockSuccessRate = (float)blockSuccesses / blockAttempts;

        // Strengthen block response to attacks
        neuralNet.weights_input_hidden[9][2] += learningRate; // Player attacking → block node
        neuralNet.biases_output[2] -= learningRate * 0.3f; // Make block easier to trigger

        string adaptation = $"Improved block timing (Success rate was: {blockSuccessRate * 100:F0}%)";
        adaptationHistory.Add(adaptation);

        float distance = Vector2.Distance(transform.position, player.transform.position);
        DataLogger.Instance.LogAdaptation("ImproveBlocking", blockSuccessRate, distance);

        Debug.Log($"<color=green>[ADAPTED] {adaptation}</color>");
    }

    private void AdaptToRangedStrategy()
    {
        // Player blocks often, so use more ranged attacks to bypass blocks

        float blockRate = (float)playerBlocks / (totalPlayerAttacks + 1);

        neuralNet.weights_hidden_output[5][5] += learningRate * 0.8f; // Increase ranged attack
        neuralNet.weights_hidden_output[4][4] -= learningRate * 0.5f; // Decrease melee

        string adaptation = $"Switched to ranged strategy (Player blocks {blockRate * 100:F0}% of time)";
        adaptationHistory.Add(adaptation);

        float distance = Vector2.Distance(transform.position, player.transform.position);
        DataLogger.Instance.LogAdaptation("PreferRanged", blockRate, distance);

        Debug.Log($"<color=green>[ADAPTED] {adaptation}</color>");
    }

    private void AdaptToPreferRanged()
    {
        // Melee isn't working, prefer ranged

        float meleeSuccessRate = (float)meleeHits / meleeAttempts;

        neuralNet.weights_hidden_output[5][5] += learningRate;
        neuralNet.weights_hidden_output[4][4] -= learningRate * 0.5f;

        string adaptation = $"Prioritized ranged over melee (Melee hit rate: {meleeSuccessRate * 100:F0}%)";
        adaptationHistory.Add(adaptation);

        float distance = Vector2.Distance(transform.position, player.transform.position);
        DataLogger.Instance.LogAdaptation("MeleeToRanged", meleeSuccessRate, distance);

        Debug.Log($"<color=green>[ADAPTED] {adaptation}</color>");
    }

    private void AdaptToMoreDefensive()
    {
        // Taking too much damage, be more defensive

        neuralNet.weights_hidden_output[2][2] += learningRate * 0.5f; // Increase blocking
        neuralNet.weights_hidden_output[3][3] += learningRate * 0.8f; // Increase dodging
        neuralNet.weights_hidden_output[4][4] -= learningRate * 0.3f; // Decrease melee aggression

        string adaptation = $"Adopted defensive strategy (Took {damageTakenSinceLastCheck * 100:F0}% damage)";
        adaptationHistory.Add(adaptation);

        float distance = Vector2.Distance(transform.position, player.transform.position);
        DataLogger.Instance.LogAdaptation("MoreDefensive", damageTakenSinceLastCheck, distance);

        Debug.Log($"<color=green>[ADAPTED] {adaptation}</color>");
    }

    // ========================================================================
    // DEBUG & ANALYTICS
    // ========================================================================

    public void PrintAnalytics()
    {
        Debug.Log($@"
=== ADAPTIVE AI ANALYTICS ===
Adaptations Made: {adaptationCount}
Has Adapted: {hasAdapted}

PLAYER PATTERNS:
Total Attacks: {totalPlayerAttacks}
- Jump Attacks: {jumpAttacks} ({(totalPlayerAttacks > 0 ? (float)jumpAttacks / totalPlayerAttacks * 100 : 0):F1}%)
- Dash Attacks: {dashAttacks} ({(totalPlayerAttacks > 0 ? (float)dashAttacks / totalPlayerAttacks * 100 : 0):F1}%)
- Ground Attacks: {groundAttacks} ({(totalPlayerAttacks > 0 ? (float)groundAttacks / totalPlayerAttacks * 100 : 0):F1}%)
Player Blocks: {playerBlocks}

AI PERFORMANCE:
Blocks: {blockSuccesses}/{blockAttempts} ({(blockAttempts > 0 ? (float)blockSuccesses / blockAttempts * 100 : 0):F1}% success)
Dodges: {dodgeSuccesses}/{dodgeAttempts} ({(dodgeAttempts > 0 ? (float)dodgeSuccesses / dodgeAttempts * 100 : 0):F1}% success)
Melee: {meleeHits}/{meleeAttempts} ({(meleeAttempts > 0 ? (float)meleeHits / meleeAttempts * 100 : 0):F1}% hit rate)
Ranged: {rangedHits}/{rangedAttempts} ({(rangedAttempts > 0 ? (float)rangedHits / rangedAttempts * 100 : 0):F1}% hit rate)

ADAPTATION HISTORY:
{string.Join("\n", adaptationHistory)}
");
    }

    void OnDestroy()
    {
        // Print final analytics when AI dies
        PrintAnalytics();
    }
}