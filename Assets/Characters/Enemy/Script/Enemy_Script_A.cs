using UnityEngine;

public class Enemy_Script_A : MonoBehaviour
{
    [Header("PlayerStats")]
    public GameObject player;
    public LayerMask playerLayer;
    Player_Stats stats;

    [Header("Enemy Stats")]
    [SerializeField] private int enemyHealth = 10;
    private int currentHealth;
    float nextAttackTime = 0f;
    public float attackRate = 2f;
    public float enemyWalkSpeed = 2.5f;

    [Header("Enemy Melee Stats")]
    public Transform enemyMeleeAttackPos;
    public float enemyMeleeAttack = 1;
    public float enemyMeleeColliderRadius = 1.93f;
    public bool meleeAttack = false;

    [Header("Enemy Range Stats")]
    public Transform enemyRangeAttackPos;
    public int enemyRangeAmmo = 3;
    public GameObject bullet;
    public float bulletForce = 1f;
    private int timesAttack =0;
    public bool rangeAttack = false;

    private bool isFlipped = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = enemyHealth;
        stats = player.gameObject.GetComponent<Player_Stats>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= nextAttackTime)
        {
            if (meleeAttack)
            {
                DealMeleeDamage(enemyMeleeAttack);
                nextAttackTime = Time.time + 1f / attackRate;
            }
            if (rangeAttack && enemyRangeAmmo > 0)
            {
                DealShootDamage();
                timesAttack = 0;
                nextAttackTime = Time.time + 1f / attackRate;
            }

        }
    }

    private void DealShootDamage()
    {
        GameObject currentBullet = Instantiate(bullet, enemyRangeAttackPos.position, Quaternion.identity);
        Rigidbody2D bulletRigidBody = currentBullet.GetComponent<Rigidbody2D>();
        Vector2 direction = player.transform.position - currentBullet.transform.position;

        // Calculate the angle and apply rotation
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        currentBullet.transform.rotation = Quaternion.Euler(0, 0, angle);

        bulletRigidBody.AddForce(direction.normalized * bulletForce, ForceMode2D.Impulse);
        enemyRangeAmmo--;
    }

    private void DealMeleeDamage(float meleeDamage)
    {
        MeleeDamageCollider(meleeDamage);
        timesAttack++;

        // After attacking 3 times enemy go into range mode
        if (timesAttack > 2)
        {
            enemyRangeAmmo = 3;
        }
    }

    private void MeleeDamageCollider(float meleeDamage)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(enemyMeleeAttackPos.position, new Vector3(enemyMeleeColliderRadius, 3), 0, playerLayer);

        foreach (Collider2D hit in hitEnemies)
        {
            stats.TakeDamage(meleeDamage);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Death();
        }
    }

    private void Death()
    {
        Destroy(gameObject);
    }

    public void LookAtPlayer()
    {
        Vector3 flipped = transform.localScale;
        flipped.x *= -1;

        if (transform.position.x > player.transform.position.x && isFlipped)
        {
            transform.localScale = flipped;
            isFlipped = false;
        }
        else if (transform.position.x < player.transform.position.x && !isFlipped)
        {
            transform.localScale = flipped;
            isFlipped = true;
        }
    }


    //Debugging

    private void OnDrawGizmos()
    {
        if (meleeAttack && enemyMeleeAttackPos != null) // Only during attack
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(enemyMeleeAttackPos.position, new Vector3(enemyMeleeColliderRadius, 3));
        }
    }
}
