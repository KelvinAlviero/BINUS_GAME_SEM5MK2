using UnityEngine;

public class Enemy_1_Run : StateMachineBehaviour
{
    Transform player;
    Enemy_Script enemyScript;
    Rigidbody2D rb;
    public float meleeAttackRange = 3f;
    public float rangeAttackRange = 10f;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = animator.GetComponent<Rigidbody2D>();
        enemyScript = animator.GetComponent<Enemy_Script>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Vector2 enemyPos = rb.position;
        Vector2 playerPos = player.position;
        Vector2 target = new Vector2(playerPos.x, enemyPos.y);
        Vector2 newPos = Vector2.MoveTowards(enemyPos, target, enemyScript.enemyWalkSpeed * Time.fixedDeltaTime);
        float distanceToPlayer = Vector2.Distance(playerPos, enemyPos);

        enemyScript.LookAtPlayer();

        if (enemyScript.enemyRangeAmmo > 0 && distanceToPlayer <= rangeAttackRange)
        {
            // Keep distance - don't move closer
            enemyScript.rangeAttack = true;
            enemyScript.meleeAttack = false;
            MoveAwayFromPlayer(distanceToPlayer, enemyPos, playerPos);
        }
        else if (enemyScript.enemyRangeAmmo <= 0 && distanceToPlayer <= meleeAttackRange)
        {
            enemyScript.meleeAttack = true;
            enemyScript.rangeAttack = false;
        }
        else
        {
            enemyScript.meleeAttack = false;
            enemyScript.rangeAttack = false;
            rb.MovePosition(newPos); // Chase player
        }

    }


    private void MoveAwayFromPlayer(float distanceToPlayer, Vector2 enemyPos, Vector2 playerPos)
    {
        float retreatThreshold = 0.7f;
        if (distanceToPlayer < rangeAttackRange * retreatThreshold) 
        {
            Vector2 awayDirection = (enemyPos - playerPos).normalized;
            Vector2 retreatPos = rb.position + awayDirection * enemyScript.enemyWalkSpeed * Time.fixedDeltaTime;
            rb.MovePosition(retreatPos);
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // animator.resettrigger("attacK");
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
