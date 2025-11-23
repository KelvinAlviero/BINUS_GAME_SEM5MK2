using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float jumpForce = 20f;

    [Header("Dashing Settings")]
    [SerializeField] private float dashingPower = 14f;
    [SerializeField] private float dashingTime = 0.5f;
    [SerializeField] private float dashingCooldown = 1f;
    
    private Vector2 dashingDirection;
    private bool isDashing;
    private bool canDash = true;

    [Header("Ground Detection")]
    public Transform groundCheck; // Drag an empty child object here
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer; // Set this to 'Ground' in Inspector

    private Rigidbody2D rb;
    private float horizontalInput;
    private float yVelocity;
    private bool isGrounded;
    private bool isFacingRight = true;
    
    [SerializeField] private Animator animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 1. Capture Input
        horizontalInput = Input.GetAxisRaw("Horizontal"); // Returns -1, 0, or 1

        // 2. Check for Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // 3. Check for dashing

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dashing());
        }

        FlipCharacter();

        //Animation - Walking
        animator.SetBool("IsWalking", horizontalInput != 0);
        
        //Animation - Jump
        if (!isGrounded)
        {
            animator.SetBool("IsJumping",true);
            animator.SetFloat("yVelocity", rb.linearVelocity.y);
        }
        else
        {
            animator.SetBool("IsJumping", false);
        }
    }
    void FixedUpdate()
    {
        if (isDashing) return;


        // 3. Check if on Ground
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 4. Move the Character
        // We keep the current Y velocity to not interfere with gravity
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
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