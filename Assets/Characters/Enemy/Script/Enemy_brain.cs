using UnityEngine;

public class Enemy_brain : MonoBehaviour
{
    [Header("Connections")]
    public Enemy_Script body;
    public GameObject player;
    public PlayerProfiler spotter;

    private Player_Stats playerStats;
    private Rigidbody2D playerRb;
    private Rigidbody2D enemyRb;

    [Header("Neural Network")]
    public NeuralNetwork neuralNet;
    private float[] inputNodes;

    [Header("Decision Settings")]
    public float decisionThreshold = 0.4f;
    public float detectionRange = 15f;
    public float meleeRange = 3f;
    public float rangedRange = 10f;
    public float decisionCooldown = 0.5f;
    public float confidenceMargin = 0.15f;

    // Decision State
    private int currentAction = -1;
    private float currentConfidence = 0f;
    private float lastDecisionTime = 0f;

    // *** NEW: Movement logging cooldown to prevent spam ***
    private float lastMovementLogTime = 0f;
    private float movementLogCooldown = 0.3f; // Log movement every 0.3 seconds

    [Header("Debug")]
    public bool showDebugLogs = false;

    void Start()
    {
        neuralNet = new NeuralNetwork(14, 8, 6);
        inputNodes = new float[14];

        enemyRb = GetComponent<Rigidbody2D>();

        if (player != null)
        {
            playerStats = player.GetComponent<Player_Stats>();
            playerRb = player.GetComponent<Rigidbody2D>();
        }
        else
        {
            Debug.LogError("Player not assigned to Enemy_brain!");
        }
    }

    void Update()
    {
        if (player == null) return;

        body.LookAtPlayer();

        GatherInputs();
        float[] decisions = neuralNet.FeedForward(inputNodes);

        // CRITICAL: Activate block IMMEDIATELY when player starts attacking
        if (body.PlayerIsAttacking && !body.isBlocking && !body.isDodging)
        {
            float distToPlayer = Vector2.Distance(transform.position, player.transform.position);

            if (distToPlayer <= meleeRange * 1.5f)
            {
                float blockThreshold = decisionThreshold * 0.5f;

                if (decisions[2] >= blockThreshold)
                {
                    body.ActivateBlock();
                    if (showDebugLogs)
                    {
                        Debug.Log($"<color=cyan>[PREDICTIVE BLOCK] Activated at {Time.time} (confidence: {decisions[2]:F3}, threshold: {blockThreshold:F3})</color>");
                    }
                }
                else if (showDebugLogs)
                {
                    Debug.Log($"<color=yellow>[BLOCK FAILED] Output too low: {decisions[2]:F3} < {blockThreshold:F3}</color>");
                }
            }
        }

        ExecuteDecisions(decisions);

        if (showDebugLogs)
        {
            DebugDecisions(decisions);
        }
    }

    private void GatherInputs()
    {
        Vector2 enemyPos = transform.position;
        Vector2 playerPos = player.transform.position;

        float distX = playerPos.x - enemyPos.x;
        inputNodes[0] = Mathf.Clamp(distX / detectionRange, -1f, 1f);

        float distY = playerPos.y - enemyPos.y;
        inputNodes[1] = Mathf.Clamp(distY / detectionRange, -1f, 1f);

        float wallCheckDistance = 2f;
        Vector2 direction = (distX > 0) ? Vector2.right : Vector2.left;
        RaycastHit2D wallCheck = Physics2D.Raycast(enemyPos, direction, wallCheckDistance, LayerMask.GetMask("Ground"));
        inputNodes[2] = wallCheck.collider != null ? 1f : 0f;

        if (playerRb != null)
        {
            inputNodes[3] = Mathf.Clamp(playerRb.linearVelocity.x / 10f, -1f, 1f);
        }
        else
        {
            inputNodes[3] = 0f;
        }

        if (playerStats != null)
        {
            inputNodes[4] = playerStats.playerCurrentHealth / playerStats.playerMaxHealthValue;
        }
        else
        {
            inputNodes[4] = 1f;
        }

        if (playerStats != null)
        {
            inputNodes[5] = playerStats.playerCurrentStamina / playerStats.playerMaxStaminaValue;
        }
        else
        {
            inputNodes[5] = 1f;
        }

        inputNodes[6] = body.GetHealthPercentage();
        inputNodes[7] = body.meleeAttack ? 1f : 0f;
        inputNodes[8] = body.rangeAttack ? 1f : 0f;
        inputNodes[9] = body.PlayerIsAttacking ? 1f : 0f;

        if (playerStats != null)
        {
            inputNodes[10] = playerStats.isBlocking ? 1f : 0f;
        }
        else
        {
            inputNodes[10] = 0f;
        }

        inputNodes[11] = (Mathf.Abs(playerRb.linearVelocity.y) > 0.1f) ? 1f : 0f;
        inputNodes[12] = body.isDodging ? 1f : 0f;

        float actualDist = Vector2.Distance(enemyPos, playerPos);
        inputNodes[13] = Mathf.Clamp(actualDist / detectionRange, 0f, 1f);
    }

    private void ExecuteDecisions(float[] outputs)
    {
        if (body.isDodging)
        {
            return;
        }

        float distToPlayer = Vector2.Distance(transform.position, player.transform.position);

        // Find best action
        int bestAction = 0;
        float bestConfidence = outputs[0];

        for (int i = 1; i < outputs.Length; i++)
        {
            if (outputs[i] > bestConfidence)
            {
                bestConfidence = outputs[i];
                bestAction = i;
            }
        }

        // Decision stability check
        bool shouldSwitchDecision = ShouldSwitchDecision(bestAction, bestConfidence, distToPlayer);

        if (shouldSwitchDecision)
        {
            currentAction = bestAction;
            currentConfidence = bestConfidence;
            lastDecisionTime = Time.time;

            if (showDebugLogs)
            {
                string[] actionNames = { "WalkRight", "WalkLeft", "Block", "Dodge", "Melee", "Ranged" };
                Debug.Log($"[SWITCH] to {actionNames[bestAction]} (conf: {bestConfidence:F2}, dist: {distToPlayer:F2})");
            }
        }

        // Execute current action
        ExecuteAction(currentAction, distToPlayer);
    }

    // *** UPDATED: Added logging for all decision switches ***
    private bool ShouldSwitchDecision(int newAction, float newConfidence, float distToPlayer)
    {
        // First decision
        if (currentAction == -1)
        {
            if (newConfidence >= decisionThreshold)
            {
                // Log the first decision
                string[] actionNames = { "WalkRight", "WalkLeft", "Block", "Dodge", "Melee", "Ranged" };
                DataLogger.Instance.LogAIDecision(actionNames[newAction], newConfidence, distToPlayer);
                return true;
            }
            return false;
        }

        // CRITICAL: Force block if player is attacking and we're close
        if (newAction == 2 && body.PlayerIsAttacking && distToPlayer <= meleeRange && newConfidence >= decisionThreshold)
        {
            DataLogger.Instance.LogAIDecision("Block", newConfidence, distToPlayer);
            return true;
        }

        // CRITICAL: Force melee if very close
        if (newAction == 4 && distToPlayer <= meleeRange && newConfidence >= decisionThreshold)
        {
            DataLogger.Instance.LogAIDecision("Melee", newConfidence, distToPlayer);
            return true;
        }

        // CRITICAL: Force ranged if at good range
        if (newAction == 5 && distToPlayer > meleeRange && distToPlayer <= rangedRange && newConfidence >= decisionThreshold)
        {
            DataLogger.Instance.LogAIDecision("Ranged", newConfidence, distToPlayer);
            return true;
        }

        // Dodge should interrupt anything
        if (newAction == 3 && body.PlayerIsAttacking && newConfidence >= decisionThreshold)
        {
            DataLogger.Instance.LogAIDecision("Dodge", newConfidence, distToPlayer);
            return true;
        }

        // Cooldown not elapsed - keep current action
        if (Time.time - lastDecisionTime < decisionCooldown)
        {
            return false;
        }

        // Same action - update confidence but don't "switch"
        if (newAction == currentAction)
        {
            currentConfidence = newConfidence;
            return false;
        }

        // Significantly better action
        if (newConfidence >= decisionThreshold && newConfidence > currentConfidence + confidenceMargin)
        {
            string[] actionNames = { "WalkRight", "WalkLeft", "Block", "Dodge", "Melee", "Ranged" };
            DataLogger.Instance.LogAIDecision(actionNames[newAction], newConfidence, distToPlayer);
            return true;
        }

        return false;
    }

    private void ExecuteAction(int action, float distToPlayer)
    {
        if (action == -1)
        {
            if (distToPlayer > meleeRange)
            {
                MoveTowardPlayer();
            }
            else
            {
                StopMovement();
            }
            body.meleeAttack = false;
            body.rangeAttack = false;
            return;
        }

        switch (action)
        {
            case 0: // Walk Right
                MoveEnemy(1);
                body.meleeAttack = false;
                body.rangeAttack = false;
                break;

            case 1: // Walk Left
                MoveEnemy(-1);
                body.meleeAttack = false;
                body.rangeAttack = false;
                break;

            case 2: // Block
                if (body.PlayerIsAttacking)
                {
                    body.ActivateBlock();
                    StopMovement();
                    body.meleeAttack = false;
                    body.rangeAttack = false;

                    if (showDebugLogs)
                    {
                        Debug.Log($"<color=cyan>[NN BLOCKING] Player attack detected at {Time.time}!</color>");
                    }
                }
                else
                {
                    currentAction = -1;
                    if (showDebugLogs)
                    {
                        Debug.Log("[BLOCK CANCELLED] Player not attacking");
                    }
                }
                break;

            case 3: // Dodge
                if (Time.time >= body.lastDodgeTime + body.dodgeCooldown)
                {
                    body.DodgeAttack();
                    currentAction = -1;

                    if (showDebugLogs)
                    {
                        Debug.Log($"<color=green>[NN DODGING] Executing dodge at {Time.time}!</color>");
                    }
                }
                break;

            case 4: // Melee Attack
                if (distToPlayer <= meleeRange)
                {
                    body.meleeAttack = true;
                    body.rangeAttack = false;
                    StopMovement();

                    if (showDebugLogs && !body.meleeAttack)
                    {
                        Debug.Log($"<color=yellow>[NN MELEE] Attacking at distance {distToPlayer:F2}</color>");
                    }
                }
                else
                {
                    body.meleeAttack = false;
                    body.rangeAttack = false;
                    MoveTowardPlayer();
                }
                break;

            case 5: // Ranged Attack
                body.rangeAttack = true;
                body.meleeAttack = false;

                if (showDebugLogs)
                {
                    Debug.Log($"<color=orange>[NN RANGED] Firing at distance {distToPlayer:F2}</color>");
                }

                if (distToPlayer < rangedRange * 0.6f)
                {
                    MoveAwayFromPlayer();
                }
                else if (distToPlayer > rangedRange)
                {
                    MoveTowardPlayer();
                }
                else
                {
                    StopMovement();
                }
                break;
        }
    }

    // *** UPDATED: Added logging with cooldown ***
    private void MoveTowardPlayer()
    {
        if (enemyRb != null && player != null)
        {
            Vector2 playerPos = player.transform.position;
            Vector2 enemyPos = transform.position;
            float direction = (playerPos.x > enemyPos.x) ? 1 : -1;

            // Log movement with cooldown to prevent spam
            if (Time.time - lastMovementLogTime >= movementLogCooldown)
            {
                float distance = Vector2.Distance(enemyPos, playerPos);
                DataLogger.Instance.LogAIMovement("TowardPlayer", distance);
                lastMovementLogTime = Time.time;
            }

            enemyRb.linearVelocity = new Vector2(direction * body.enemyWalkSpeed, enemyRb.linearVelocity.y);
        }
    }

    // *** UPDATED: Added logging with cooldown ***
    private void MoveAwayFromPlayer()
    {
        if (enemyRb != null && player != null)
        {
            Vector2 playerPos = player.transform.position;
            Vector2 enemyPos = transform.position;
            float direction = (enemyPos.x > playerPos.x) ? 1 : -1;

            // Log movement with cooldown to prevent spam
            if (Time.time - lastMovementLogTime >= movementLogCooldown)
            {
                float distance = Vector2.Distance(enemyPos, playerPos);
                DataLogger.Instance.LogAIMovement("AwayFromPlayer", distance);
                lastMovementLogTime = Time.time;
            }

            enemyRb.linearVelocity = new Vector2(direction * body.enemyWalkSpeed, enemyRb.linearVelocity.y);
        }
    }

    // *** UPDATED: Added logging with cooldown ***
    private void StopMovement()
    {
        if (enemyRb != null)
        {
            // Log movement with cooldown to prevent spam
            if (player != null && Time.time - lastMovementLogTime >= movementLogCooldown)
            {
                float distance = Vector2.Distance(transform.position, player.transform.position);
                DataLogger.Instance.LogAIMovement("Stop", distance);
                lastMovementLogTime = Time.time;
            }

            enemyRb.linearVelocity = new Vector2(0, enemyRb.linearVelocity.y);
        }
    }

    // *** UPDATED: Added logging with cooldown ***
    private void MoveEnemy(float direction)
    {
        if (enemyRb != null)
        {
            // Log movement with cooldown to prevent spam
            if (player != null && Time.time - lastMovementLogTime >= movementLogCooldown)
            {
                float distance = Vector2.Distance(transform.position, player.transform.position);
                string movementType = direction > 0 ? "Right" : "Left";
                DataLogger.Instance.LogAIMovement(movementType, distance);
                lastMovementLogTime = Time.time;
            }

            enemyRb.linearVelocity = new Vector2(direction * body.enemyWalkSpeed, enemyRb.linearVelocity.y);
        }
    }

    private void DebugDecisions(float[] outputs)
    {
        string[] actionNames = { "WalkRight", "WalkLeft", "Block", "Dodge", "Melee", "Ranged" };
        float distToPlayer = Vector2.Distance(transform.position, player.transform.position);
        bool playerBlocking = (playerStats != null && playerStats.isBlocking);
        bool playerAttacking = body.PlayerIsAttacking;

        string debug = $"=== NEURAL NETWORK DEBUG ===\n";
        debug += $"Distance: {distToPlayer:F2}m | Player Attacking: {playerAttacking} | Player Blocking: {playerBlocking}\n";
        debug += $"Enemy State: Block={body.isBlocking} | Dodge={body.isDodging} | Invincible={body.enemyIsInvincible}\n";
        debug += $"CURRENT ACTION: {(currentAction >= 0 ? actionNames[currentAction] : "None")} (Confidence: {currentConfidence:F2})\n";
        debug += $"Time Since Decision: {Time.time - lastDecisionTime:F2}s | Cooldown: {decisionCooldown}s\n";
        debug += "\nOUTPUTS:\n";

        for (int i = 0; i < outputs.Length; i++)
        {
            string status = "";
            if (outputs[i] >= decisionThreshold)
            {
                status = " [ABOVE THRESHOLD]";
            }
            if (i == currentAction)
            {
                status += " <-- EXECUTING";
            }
            debug += $"  [{i}] {actionNames[i]}: {outputs[i]:F3}{status}\n";
        }

        int bestIdx = 0;
        float bestVal = outputs[0];
        for (int i = 1; i < outputs.Length; i++)
        {
            if (outputs[i] > bestVal)
            {
                bestVal = outputs[i];
                bestIdx = i;
            }
        }

        debug += $"\nBEST ACTION: {actionNames[bestIdx]} ({bestVal:F3})";
        debug += $"\n\nATTACK STATUS:\n";
        debug += $"  Melee Flag: {body.meleeAttack} (In Range: {distToPlayer <= meleeRange})\n";
        debug += $"  Ranged Flag: {body.rangeAttack} (In Range: {distToPlayer <= rangedRange})\n";

        Debug.Log(debug);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, rangedRange);
    }
}