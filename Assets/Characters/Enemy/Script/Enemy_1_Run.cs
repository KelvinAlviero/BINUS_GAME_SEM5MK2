using UnityEngine;

public class Enemy_1_Run : StateMachineBehaviour
{
    GameObject player;
    Enemy_Script enemyScript;
    Rigidbody2D rb;

    public float meleeAttackRange = 3f;
    public float rangeAttackRange = 10f;

    // Decision variables
    private bool useRangedAttack;
    private float nextDecisionTime = 0f;
    private float decisionCooldown = 2f; // Make new decision every 2 seconds

    // *** NEW: Movement logging cooldown to prevent spam ***
    private float lastMovementLogTime = 0f;
    private float movementLogCooldown = 0.3f; // Log movement every 0.3 seconds

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player");
        rb = animator.GetComponent<Rigidbody2D>();
        enemyScript = animator.GetComponent<Enemy_Script>();

        // Make initial decision
        MakeAttackDecision();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Vector2 enemyPos = rb.position;
        Vector2 playerPos = player.transform.position;
        float distanceToPlayer = Vector2.Distance(playerPos, enemyPos);

        enemyScript.LookAtPlayer();

        // Handle dodging
        if (enemyScript.isDodging)
        {
            enemyScript.meleeAttack = false;
            enemyScript.rangeAttack = false;
            return;
        }

        // Make new attack decision periodically
        if (Time.time >= nextDecisionTime)
        {
            MakeAttackDecision();
            nextDecisionTime = Time.time + decisionCooldown;
        }

        // Execute based on the DECIDED attack type
        if (useRangedAttack && distanceToPlayer <= rangeAttackRange)
        {
            // Ranged attack mode
            enemyScript.rangeAttack = true;
            enemyScript.meleeAttack = false;
            MoveAwayFromPlayer(distanceToPlayer, enemyPos, playerPos);
        }
        else if (!useRangedAttack && distanceToPlayer <= meleeAttackRange)
        {
            // Melee attack mode
            enemyScript.meleeAttack = true;
            enemyScript.rangeAttack = false;
            StopEnemyMovement(); // Stop when in melee range
        }
        else
        {
            // Chase mode
            enemyScript.meleeAttack = false;
            enemyScript.rangeAttack = false;
            MoveEnemyToPlayer(playerPos, enemyPos);
        }
    }

    // *** UPDATED: Added logging ***
    private void MakeAttackDecision()
    {
        useRangedAttack = (Random.value <= enemyScript.percentageRangeAttack);

        // Log the decision made by the state machine
        float distanceToPlayer = Vector2.Distance(player.transform.position, rb.position);
        string attackType = useRangedAttack ? "Ranged" : "Melee";
        float confidence = useRangedAttack ? enemyScript.percentageRangeAttack : (1f - enemyScript.percentageRangeAttack);

        DataLogger.Instance.LogAIDecision(attackType, confidence, distanceToPlayer);
    }

    // *** UPDATED: Added logging ***
    private void StopEnemyMovement()
    {
        // Log movement with cooldown to prevent spam
        if (Time.time - lastMovementLogTime >= movementLogCooldown)
        {
            float distance = Vector2.Distance(player.transform.position, rb.position);
            DataLogger.Instance.LogAIMovement("Stop", distance);
            lastMovementLogTime = Time.time;
        }

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    // *** UPDATED: Added logging ***
    private void MoveEnemyToPlayer(Vector2 playerPos, Vector2 enemyPos)
    {
        // Log movement with cooldown to prevent spam
        if (Time.time - lastMovementLogTime >= movementLogCooldown)
        {
            float distance = Vector2.Distance(playerPos, enemyPos);
            DataLogger.Instance.LogAIMovement("TowardPlayer", distance);
            lastMovementLogTime = Time.time;
        }

        float direction = (playerPos.x > enemyPos.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(direction * enemyScript.enemyWalkSpeed, rb.linearVelocity.y);
    }

    // *** UPDATED: Added logging ***
    private void MoveAwayFromPlayer(float distanceToPlayer, Vector2 enemyPos, Vector2 playerPos)
    {
        float retreatThreshold = 0.7f;
        if (distanceToPlayer < rangeAttackRange * retreatThreshold)
        {
            // Log movement with cooldown to prevent spam
            if (Time.time - lastMovementLogTime >= movementLogCooldown)
            {
                DataLogger.Instance.LogAIMovement("AwayFromPlayer", distanceToPlayer);
                lastMovementLogTime = Time.time;
            }

            float direction = (enemyPos.x > playerPos.x) ? 1 : -1;
            rb.linearVelocity = new Vector2(direction * enemyScript.enemyWalkSpeed, rb.linearVelocity.y);
        }
        else
        {
            StopEnemyMovement(); // This will log "Stop" with cooldown
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

    }
}