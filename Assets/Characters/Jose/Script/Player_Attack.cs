using Unity.VisualScripting;
using UnityEngine;

public class Player_Attack : MonoBehaviour
{
    [Header("Attacking System")]
    public float attackRate = 2f;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private Animator animator;
    private bool isAttacking = false;
    private float nextAttackTime = 0f;
    public Transform attackPoint;

    [Header("Enemy Layer")]
    public LayerMask enemyLayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (attackPoint == null) Debug.LogWarning("attackPoint Not Found");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TryAttack()
    {
        isAttacking = false;
        if (Time.time >= nextAttackTime)
        {            
            isAttacking = true;
            AttackLogic();
            nextAttackTime = Time.time + 1f / attackRate;   
        }
    }

    private void AttackLogic()
    {   
        animator.SetTrigger("IsAttacking");
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackPoint.position, new Vector3(attackRange, 3), 0,enemyLayer);  


        foreach(Collider2D hit in hitEnemies)
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
        if (isAttacking && attackPoint != null) // Only during attack
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(attackPoint.position, new Vector3(attackRange, 3));
        }
    }

}
