using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Player_Attack : MonoBehaviour
{
    [Header("Attacking System")]
    [SerializeField] private AudioClip attackSoundEffect;
    public float attackRate = 2f;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private float attackDamage = 1f;
    [SerializeField] private Animator animator;
    public bool isAttacking;
    private float nextAttackTime = 0f;
    public Transform attackPoint;

    [Header("Attack Timing")]
    [SerializeField] private float attackStartupDelay = 0.2f; // Time before damage lands
    [SerializeField] private float attackDuration = 0.3f;     // Total attack animation time

    [Header("Enemy Layer")]
    public LayerMask enemyLayer;

    void Start()
    {
        if (attackPoint == null) Debug.LogWarning("attackPoint Not Found");
    }

    public void TryAttack()
    {
        if (Time.time >= nextAttackTime)
        {
            AudioManager.instance.PlaySoundFXClipWithRandomPitch(attackSoundEffect, transform, 0.5f);
            StartCoroutine(AttackCoroutine());
            nextAttackTime = Time.time + 1f / attackRate;
        }
    }

    IEnumerator AttackCoroutine()
    {
        // PHASE 1: Attack Startup (enemy can react here!)
        isAttacking = true;
        animator.SetTrigger("IsAttacking");

        Debug.Log($"[PLAYER ATTACK] Started at {Time.time} - Enemy can react now!");

        // Wait for startup delay - enemy reacts during this time
        yield return new WaitForSeconds(attackStartupDelay);

        // PHASE 2: Damage Frame (actual hit)
        Debug.Log($"[PLAYER ATTACK] Damage landing at {Time.time}!");
        AttackLogic();

        // PHASE 3: Recovery
        float remainingTime = attackDuration - attackStartupDelay;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        isAttacking = false;
        Debug.Log($"[PLAYER ATTACK] Finished at {Time.time}");
    }

    private void AttackLogic()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, new Vector3(attackRange, 3), 0, enemyLayer);

        bool hitSomething = hitEnemies.Length > 0;
        DataLogger.Instance.LogPlayerAttack(hitSomething);

        foreach (Collider2D hit in hitEnemies)
        {
            DamageEnemy(hit);
        }
    }

    private void DamageEnemy(Collider2D hitEnemy)
    {
        Debug.Log(hitEnemy.gameObject.name + " Is hit");
        hitEnemy.GetComponent<Enemy_Script>().TakeDamage(attackDamage);
    }

    private void OnDrawGizmos()
    {
        if (isAttacking && attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackPoint.position, new Vector3(attackRange, 3));
        }
    }
}