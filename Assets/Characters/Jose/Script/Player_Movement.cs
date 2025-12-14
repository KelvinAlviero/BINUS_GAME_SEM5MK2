using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    [Header("Sound Effects")]
    [SerializeField] private AudioClip dashSoundEffect;
    [SerializeField] private AudioClip jumpSoundEffect;

    [Header("Movement Settings")]
    public float walkSpeed = 8f;
    public float jumpForce = 20f;
    [SerializeField] private float jumpCost = 20f;

    [Header("Dashing Settings")]
    [SerializeField] private float dashingPower = 14f;
    [SerializeField] private float dashingTime = 0.5f;
    [SerializeField] private float dashingCooldown = 1f;
    [SerializeField] private float dashCost = 40f;

    private Vector2 dashingDirection;
    private bool isDashing;
    private bool canDash = true;

    [Header("Ground Detection")]
    public Transform groundCheck; // Drag an empty child object here
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer; // Set this to 'Ground' in Inspector

    private Rigidbody2D rb;
    private Player_Stats stats;
    private float horizontalInput;
    private float yVelocity;
    private bool isGrounded;
    private bool isFacingRight = true;
    
    [SerializeField] private Animator animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<Player_Stats>();
    }

    void Update()
    {
        FlipCharacter();
    }
    void FixedUpdate()
    {
       
        if (isDashing) return;

        CheckIfPlayerGrounded();

        HandlePlayerMovement();

        HandleJumpAnimation();
    }

    private void HandleJumpAnimation()
    {
        if (isGrounded)
        {
            animator.SetBool("IsJumping", false);
        }
        else
        {
            animator.SetBool("IsJumping", true);
        }
    }

    private void HandlePlayerMovement()
    {
        // 4. Move the Character
        // We keep the current Y velocity to not interfere with gravity
        rb.linearVelocity = new Vector2(horizontalInput * walkSpeed, rb.linearVelocity.y);

        // Animation by tracking the y velocity

        animator.SetFloat("yVelocity", rb.linearVelocity.y);

        if (rb.linearVelocityY < 0)
        {
            animator.SetBool("IsFalling", true);
        }
        else
        {
            animator.SetBool("IsFalling", false);
        }
    }

    private void CheckIfPlayerGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void FlipCharacter()
    {
        if (horizontalInput < 0f && isFacingRight || horizontalInput > 0f && !isFacingRight)
        {
            Vector3 localScale = transform.localScale;
            isFacingRight = !isFacingRight;
            localScale.x *= -1;
            transform.localScale = localScale;
        }
    }

    public void TryWalkingInput(float horizontalInputFromKeyboard)
    {
        horizontalInput = horizontalInputFromKeyboard; // Returns -1, 0, or 1
        // Animation
        if (horizontalInput > 0)
        {
            DataLogger.Instance.LogPlayerMovement("Right");
        }
        else if (horizontalInput < 0)
        {
            DataLogger.Instance.LogPlayerMovement("Left");
        }

        animator.SetBool("IsWalking", horizontalInput != 0);
    }

    public void TryJumpingInput()
    {
        if (isGrounded && stats.CanSpendStamina(jumpCost))
        {
            DataLogger.Instance.LogPlayerJump();
            AudioManager.instance.PlaySoundFXClip(jumpSoundEffect, transform, 0.5f);
            stats.DrainStamina(jumpCost);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    public void TryDashingInput()
    {
        if (canDash && stats.CanSpendStamina(dashCost))
        {
            DataLogger.Instance.LogPlayerDash();
            AudioManager.instance.PlaySoundFXClip(dashSoundEffect, transform, 0.5f);
            stats.DrainStamina(dashCost);
            StartCoroutine(Dashing());
        }
    }

    private IEnumerator Dashing()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        yield return new WaitForSeconds(dashingTime);
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }

    // Optional: Draw the ground check circle in the editor for debugging
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}