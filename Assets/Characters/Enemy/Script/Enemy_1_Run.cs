using UnityEngine;

// ============================================================================
// PREDICTABLE FSM AI - FOR RESEARCH COMPARISON
// ============================================================================
// This is the TRADITIONAL AI that follows fixed, learnable patterns.
// Players should be able to learn and exploit these patterns over time.
//
// ATTACK PATTERN:
//   - Shoots 3 times (keeps distance)
//   - Melees 2 times (rushes in)
//   - Repeats forever
//
// DEFENSE PATTERNS:
//   - DODGE: Every 3rd player attack when HP < 70%
//   - BLOCK: Every 2nd player attack when HP < 50%
//
// PREDICTABILITY: All behaviors are deterministic and pattern-based.
// Players can learn: "He dodges every 3rd hit, so I should feint first!"
// ============================================================================

public class Enemy_1_Run : StateMachineBehaviour
{
    GameObject player;
    Enemy_Script enemyScript;
    Rigidbody2D rb;

    public float meleeAttackRange = 3f;
    public float rangeAttackRange = 10f;

    // *** PREDICTABLE PATTERN SYSTEM ***
    // ATTACK: Shoot 3 times → Melee 2 times → Repeat
    // DODGE: Every 3rd player attack when HP < 70%
    // BLOCK: Every 2nd player attack when HP < 50%
    private enum AttackPattern { Ranged, Melee }
    private AttackPattern currentPattern = AttackPattern.Ranged;

    private int rangedAttackCount = 0;
    private int meleeAttackCount = 0;

    // Pattern: Shoot 3 times → Melee 2 times → Repeat
    private const int MAX_RANGED_ATTACKS = 3;
    private const int MAX_MELEE_ATTACKS = 2;

    private float lastAttackTime = 0f;
    private float attackCooldown = 1.5f; // Time between pattern attacks

    // *** PREDICTABLE BLOCKING ***
    // Blocks on every 2nd player attack when health < 50%
    private int playerAttacksSeen = 0;
    private bool lastPlayerAttackState = false;

    // *** PREDICTABLE DODGING ***
    // Dodges every 3rd player attack when health < 70%
    private int playerAttacksForDodge = 0;

    // Movement logging
    private float lastMovementLogTime = 0f;
    private float movementLogCooldown = 0.3f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player");
        rb = animator.GetComponent<Rigidbody2D>();
        enemyScript = animator.GetComponent<Enemy_Script>();

        // Start with ranged pattern
        currentPattern = AttackPattern.Ranged;
        rangedAttackCount = 0;
        meleeAttackCount = 0;
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

        // *** PREDICTABLE BLOCKING & DODGING ***
        CheckForPredictableDefense();

        // *** EXECUTE PREDICTABLE ATTACK PATTERN ***
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            ExecuteAttackPattern(distanceToPlayer, enemyPos, playerPos);
        }
        else
        {
            // Between attacks, chase or maintain distance
            MaintainCombatPosition(distanceToPlayer, enemyPos, playerPos);
        }
    }

    private void ExecuteAttackPattern(float distanceToPlayer, Vector2 enemyPos, Vector2 playerPos)
    {
        switch (currentPattern)
        {
            case AttackPattern.Ranged:
                if (distanceToPlayer <= rangeAttackRange)
                {
                    // Execute ranged attack
                    enemyScript.rangeAttack = true;
                    enemyScript.meleeAttack = false;
                    rangedAttackCount++;
                    lastAttackTime = Time.time;

                    // Log decision
                    DataLogger.Instance.LogAIDecision("Ranged", 1.0f, distanceToPlayer);

                    // Switch to melee after 3 ranged attacks
                    if (rangedAttackCount >= MAX_RANGED_ATTACKS)
                    {
                        currentPattern = AttackPattern.Melee;
                        rangedAttackCount = 0;
                        Debug.Log("[FSM Pattern] Switching to MELEE pattern");
                    }

                    // Move away from player when using ranged
                    MoveAwayFromPlayer(distanceToPlayer, enemyPos, playerPos);
                }
                else
                {
                    // Too far, move closer
                    MoveEnemyToPlayer(playerPos, enemyPos);
                }
                break;

            case AttackPattern.Melee:
                if (distanceToPlayer <= meleeAttackRange)
                {
                    // Execute melee attack
                    enemyScript.meleeAttack = true;
                    enemyScript.rangeAttack = false;
                    meleeAttackCount++;
                    lastAttackTime = Time.time;

                    // Log decision
                    DataLogger.Instance.LogAIDecision("Melee", 1.0f, distanceToPlayer);

                    // Switch to ranged after 2 melee attacks
                    if (meleeAttackCount >= MAX_MELEE_ATTACKS)
                    {
                        currentPattern = AttackPattern.Ranged;
                        meleeAttackCount = 0;
                        Debug.Log("[FSM Pattern] Switching to RANGED pattern");
                    }

                    StopEnemyMovement();
                }
                else
                {
                    // Too far, chase player
                    enemyScript.meleeAttack = false;
                    enemyScript.rangeAttack = false;
                    MoveEnemyToPlayer(playerPos, enemyPos);
                }
                break;
        }
    }

    // *** PREDICTABLE DEFENSE: Both blocking and dodging patterns ***
    private void CheckForPredictableDefense()
    {
        bool playerIsAttacking = enemyScript.PlayerIsAttacking;

        // Detect attack START (rising edge)
        if (playerIsAttacking && !lastPlayerAttackState)
        {
            playerAttacksSeen++;
            playerAttacksForDodge++;

            float healthPercent = enemyScript.GetHealthPercentage();
            float distance = Vector2.Distance(player.transform.position, rb.position);

            // === DODGE PATTERN: Every 3rd attack when health < 70% ===
            // Dodging has priority and happens more frequently at higher health
            if (healthPercent < 0.7f && playerAttacksForDodge % 3 == 0)
            {
                // Check if dodge is off cooldown
                if (Time.time >= enemyScript.lastDodgeTime + enemyScript.dodgeCooldown)
                {
                    enemyScript.DodgeAttack();
                    playerAttacksForDodge = 0; // Reset dodge counter after successful dodge
                    Debug.Log($"[FSM Pattern] Predictable DODGE triggered (attack #{playerAttacksSeen}, HP: {healthPercent * 100:F0}%)");

                    // Don't block if we dodged
                    lastPlayerAttackState = playerIsAttacking;
                    return;
                }
            }

            // === BLOCK PATTERN: Every 2nd attack when health < 50% ===
            // Only block if we didn't dodge
            if (healthPercent < 0.5f && playerAttacksSeen % 2 == 0)
            {
                enemyScript.ActivateBlock();
                Debug.Log($"[FSM Pattern] Predictable BLOCK triggered (attack #{playerAttacksSeen}, HP: {healthPercent * 100:F0}%)");
            }
        }

        lastPlayerAttackState = playerIsAttacking;
    }

    private void MaintainCombatPosition(float distanceToPlayer, Vector2 enemyPos, Vector2 playerPos)
    {
        enemyScript.meleeAttack = false;
        enemyScript.rangeAttack = false;

        if (currentPattern == AttackPattern.Ranged)
        {
            // Maintain medium distance for ranged
            if (distanceToPlayer < rangeAttackRange * 0.7f)
            {
                MoveAwayFromPlayer(distanceToPlayer, enemyPos, playerPos);
            }
            else if (distanceToPlayer > rangeAttackRange)
            {
                MoveEnemyToPlayer(playerPos, enemyPos);
            }
            else
            {
                StopEnemyMovement();
            }
        }
        else
        {
            // Chase for melee
            if (distanceToPlayer > meleeAttackRange)
            {
                MoveEnemyToPlayer(playerPos, enemyPos);
            }
            else
            {
                StopEnemyMovement();
            }
        }
    }

    private void StopEnemyMovement()
    {
        if (Time.time - lastMovementLogTime >= movementLogCooldown)
        {
            float distance = Vector2.Distance(player.transform.position, rb.position);
            DataLogger.Instance.LogAIMovement("Stop", distance);
            lastMovementLogTime = Time.time;
        }

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    private void MoveEnemyToPlayer(Vector2 playerPos, Vector2 enemyPos)
    {
        if (Time.time - lastMovementLogTime >= movementLogCooldown)
        {
            float distance = Vector2.Distance(playerPos, enemyPos);
            DataLogger.Instance.LogAIMovement("TowardPlayer", distance);
            lastMovementLogTime = Time.time;
        }

        float direction = (playerPos.x > enemyPos.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(direction * enemyScript.enemyWalkSpeed, rb.linearVelocity.y);
    }

    private void MoveAwayFromPlayer(float distanceToPlayer, Vector2 enemyPos, Vector2 playerPos)
    {
        if (Time.time - lastMovementLogTime >= movementLogCooldown)
        {
            DataLogger.Instance.LogAIMovement("AwayFromPlayer", distanceToPlayer);
            lastMovementLogTime = Time.time;
        }

        float direction = (enemyPos.x > playerPos.x) ? 1 : -1;
        rb.linearVelocity = new Vector2(direction * enemyScript.enemyWalkSpeed, rb.linearVelocity.y);
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        enemyScript.meleeAttack = false;
        enemyScript.rangeAttack = false;
    }
}