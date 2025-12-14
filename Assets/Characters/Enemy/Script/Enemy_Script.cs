using UnityEditor.Build;
using UnityEngine;
using System.Collections;

public class Enemy_Script : MonoBehaviour
{
    [Header("AI Control Mode")]
    [SerializeField] private bool useRandomDefense = true;

    [Header("Audio")]
    [SerializeField] private AudioClip hurtSoundEffect;
    [SerializeField] private AudioClip blockSoundEffect;
    [SerializeField] private AudioClip dashSoundEffect;

    [Header("PlayerStats")]
    public GameObject player;
    private Player_Attack playerAttackScript;
    public LayerMask playerLayer;

    Player_Stats stats;

    [Header("Enemy Stats")]
    [SerializeField] private float enemyHealth = 10f;
    private float currentHealth;
    float nextAttackTime = 0f;
    public float attackRate = 2f;
    public float enemyWalkSpeed = 2.5f;
    public GameObject enemyhp_BarGameObject;
    [SerializeField] private Rigidbody2D rb;
    public bool enemyIsInvincible;

    [Header("Enemy Block Stats")]
    public float percentageBlocking = 0.7f;
    public bool isBlocking = false;
    private float blockStartTime = 0f;
    public float blockDuration = 0.4f; // Covers startup + damage frame

    [Header("Enemy Dodge Stats")]
    public float percentageDodging = 0.7f;
    public bool isDodging = false;
    [SerializeField] private float dashSpeed = 30f;
    public float lastDodgeTime = 0f;
    public float dodgeCooldown = 2f;

    [Header("Enemy Melee Stats")]
    public Transform enemyMeleeAttackPos;
    public float enemyMeleeAttack = 1;
    public float enemyMeleeColliderRadius = 1.93f;
    public bool meleeAttack = false;

    [Header("Enemy Range Stats")]
    public Transform enemyRangeAttackPos;
    public float percentageRangeAttack = 0.3f;
    [SerializeField] private GameObject bullet;
    public float bulletForce = 1f;
    public bool rangeAttack = false;

    private bool isFlipped = false;
    private HP_BarScript hp_BarScript;

    public bool PlayerIsAttacking => playerAttackScript != null && playerAttackScript.isAttacking;

    private void Awake()
    {
        hp_BarScript = enemyhp_BarGameObject.GetComponent<HP_BarScript>();
    }

    void Start()
    {
        currentHealth = enemyHealth;
        stats = player.gameObject.GetComponent<Player_Stats>();
        hp_BarScript.SetMaxHealth(enemyHealth);
        rb = transform.GetComponent<Rigidbody2D>();
        playerAttackScript = player.GetComponent<Player_Attack>();
    }

    public void Update()
    {
        // NEW: Check if block duration has expired
        if (isBlocking && Time.time >= blockStartTime + blockDuration)
        {
            isBlocking = false;
        }

        if (Time.time >= nextAttackTime)
        {
            if (meleeAttack)
            {
                DealMeleeDamage(enemyMeleeAttack);
                nextAttackTime = Time.time + 1f / attackRate;
            }
            if (rangeAttack)
            {
                DealShootDamage();
                nextAttackTime = Time.time + 1f / attackRate;
            }
        }
    }

    // NEW: Method to activate blocking (called from Enemy_brain)
    public void ActivateBlock()
    {
        // Always refresh the block timer, even if already blocking
        // This handles spam attacks
        isBlocking = true;
        blockStartTime = Time.time;
    }

    // NEW: Method to check if we can block
    public bool IsBlockActive()
    {
        return isBlocking && Time.time < blockStartTime + blockDuration;
    }

    public void DodgeAttack()
    {
        if (Time.time >= lastDodgeTime + dodgeCooldown && !isDodging)
        {
            StartCoroutine(DodgeDash());
            lastDodgeTime = Time.time;
        }
    }

    IEnumerator DodgeDash()
    {
        isDodging = true;
        enemyIsInvincible = true;
        float dashDuration = 0.5f;
        float elapsed = 0f;

        Vector2 dashDirection = (transform.position.x > player.transform.position.x) ? Vector2.right : Vector2.left;

        float checkDistance = 2f;
        RaycastHit2D wallCheck = Physics2D.Raycast(transform.position, dashDirection, checkDistance, LayerMask.GetMask("Wall"));
        
        AudioManager.instance.PlaySoundFXClipWithRandomPitch(dashSoundEffect, transform, 0.5f); // play sfx
        
        if (wallCheck.collider != null)
        {
            dashDirection = -dashDirection;
            Debug.Log("Wall detected! Dashing opposite direction");
        }

        while (elapsed < dashDuration)
        {
            rb.linearVelocity = new Vector2(dashDirection.x * dashSpeed, rb.linearVelocity.y);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        yield return new WaitForSeconds(0.2f);

        isDodging = false;
        enemyIsInvincible = false;
    }

    private void DealShootDamage()
    {
        GameObject currentBullet = Instantiate(bullet, enemyRangeAttackPos.position, Quaternion.identity);
        Rigidbody2D bulletRigidBody = currentBullet.GetComponent<Rigidbody2D>();
        Vector2 direction = player.transform.position - currentBullet.transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        currentBullet.transform.rotation = Quaternion.Euler(0, 0, angle);

        bulletRigidBody.AddForce(direction.normalized * bulletForce, ForceMode2D.Impulse);
    }

    private void DealMeleeDamage(float meleeDamage)
    {
        MeleeDamageCollider(meleeDamage);
    }

    private void MeleeDamageCollider(float meleeDamage)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(enemyMeleeAttackPos.position, new Vector3(enemyMeleeColliderRadius, 3), 0, playerLayer);

        foreach (Collider2D hit in hitEnemies)
        {
            stats.TakeDamage(meleeDamage);
        }
    }

    public void TakeDamage(float damage)
    {
        // Check invincibility FIRST
        if (enemyIsInvincible || isDodging)
        {
            Debug.Log("<color=green>[DAMAGE AVOIDED] Enemy is invincible/dodging!</color>");
            return;
        }

        // === NEURAL NETWORK DEFENSE MODE ===
        if (!useRandomDefense)
        {
            // Check if block is ACTIVE (not just the flag)
            if (IsBlockActive())
            {
                Debug.Log("<color=cyan>[BLOCKED] Damage reduced by 75%!</color>");
                BlockDamage(damage);
            }
            else
            {
                Debug.Log("<color=red>[FULL DAMAGE] No active defense</color>");
                TakeFullDamage(damage);
            }
        }
        // === RANDOM DEFENSE MODE ===
        else
        {
            if (CheckForRandomDodging())
            {
                return;
            }

            if (Random.value <= percentageBlocking && !isDodging)
            {
                BlockDamage(damage);
            }
            else
            {
                TakeFullDamage(damage);
            }
        }

        if (currentHealth <= 0)
        {
            Death();
        }
    }

    private bool CheckForRandomDodging()
    {
        if (PlayerIsAttacking &&
        !isDodging &&
        Time.time >= lastDodgeTime + dodgeCooldown)
        {
            float distanceToPlayer = Vector2.Distance(player.transform.position, transform.position);
            if (distanceToPlayer <= 10f && Random.value <= percentageDodging)
            {
                enemyIsInvincible = true;
                DodgeAttack();
                return true;
            }
        }
        return false;
    }

    private void BlockDamage(float damage)
    {
        AudioManager.instance.PlaySoundFXClipWithRandomPitch(blockSoundEffect, transform, 0.5f);
        Hitstop.instance.Stop(0.1f);
        float blockedDamage = (damage * 0.25f);
        Debug.Log($"<color=yellow>[BLOCK SUCCESS] Enemy blocked! Took {blockedDamage} instead of {damage}</color>");
        currentHealth -= blockedDamage;
        hp_BarScript.SetHealth(currentHealth);
    }

    private void TakeFullDamage(float damage)
    {
        AudioManager.instance.PlaySoundFXClipWithRandomPitch(hurtSoundEffect, transform, 0.5f);
        Hitstop.instance.Stop(0.1f);
        Debug.Log($"[FULL DAMAGE] Enemy took {damage} damage");
        currentHealth -= damage;
        hp_BarScript.SetHealth(currentHealth);
    }

    public IEnumerator BlockingCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        isBlocking = false;
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

    public float GetHealthPercentage()
    {
        return currentHealth / enemyHealth;
    }

    private void OnDrawGizmos()
    {
        if (meleeAttack && enemyMeleeAttackPos != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(enemyMeleeAttackPos.position, new Vector3(enemyMeleeColliderRadius, 3));
        }
    }
}