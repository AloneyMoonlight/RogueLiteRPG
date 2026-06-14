using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;

    [Header("Attack Movement")]
    [Tooltip("Qué fracción de moveSpeed avanza el personaje al atacar (0 = estático, 0.15 = paso adelante)")]
    [SerializeField] private float attackMoveMultiplier = 0.15f;

    [Header("Dash")]
    [SerializeField] private bool  dashEnabled  = true;
    [SerializeField] private float dashSpeed    = 15f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Physics Feel")]
    [SerializeField] private float fallMultiplier    = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float     groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Coyote Time & Jump Buffer")]
    [SerializeField] private float coyoteTime    = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    // ── Referencias ───────────────────────────────────────────────────────────
    private Rigidbody2D  rb;
    private Animator     anim;
    private PlayerHealth health;
    private PlayerCombat combat;   // ← para consultar IsAttacking

    // ── Estado ───────────────────────────────────────────────────────────────
    private float move;
    private bool  isGrounded;
    private bool  hasAirJump;

    private float coyoteTimer;
    private float jumpBufferTimer;

    private bool  isDashing;
    private float lastDashTime = -999f;

    // ── Init ──────────────────────────────────────────────────────────────────
    void Start()
    {
        rb     = GetComponent<Rigidbody2D>();
        anim   = GetComponent<Animator>();
        health = GetComponent<PlayerHealth>();
        combat = GetComponent<PlayerCombat>();
    }

    // ── Update ────────────────────────────────────────────────────────────────
    void Update()
    {
        if (health != null && health.IsDead) return;
        if (isDashing) return;

        move = Input.GetAxisRaw("Horizontal");

        // Solo cambia de dirección si no está atacando
        bool attacking = combat != null && combat.IsAttacking;
        if (!attacking) HandleFlip();

        HandleJump();
        HandleDash();
        UpdateAnimator();
    }

    // ── FixedUpdate ───────────────────────────────────────────────────────────
    void FixedUpdate()
    {
        if (isDashing) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        bool attacking = combat != null && combat.IsAttacking;

        if (attacking)
        {
            // Pequeño paso adelante en la dirección que mira (efecto típico de ataque)
            float facingDir = transform.localScale.x;
            rb.linearVelocity = new Vector2(facingDir * moveSpeed * attackMoveMultiplier,
                                            rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y);
        }

        ApplyFallMultiplier();
    }

    // ── Flip ──────────────────────────────────────────────────────────────────
    private void HandleFlip()
    {
        if (move != 0)
            transform.localScale = new Vector3(Mathf.Sign(move), 1, 1);
    }

    // ── Jump ──────────────────────────────────────────────────────────────────
    private void HandleJump()
    {
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            hasAirJump  = true;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
            jumpBufferTimer = jumpBufferTime;
        else
            jumpBufferTimer -= Time.deltaTime;

        if (jumpBufferTimer > 0f)
        {
            if (coyoteTimer > 0f)
            {
                Jump();
                coyoteTimer     = 0f;
                jumpBufferTimer = 0f;
            }
            else if (hasAirJump)
            {
                Jump();
                hasAirJump      = false;
                jumpBufferTimer = 0f;
            }
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.jumpSFX);
    }

    // ── Dash ──────────────────────────────────────────────────────────────────
    private void HandleDash()
    {
        if (!dashEnabled) return;
        if (!Input.GetKeyDown(KeyCode.LeftShift)) return;
        if (Time.time < lastDashTime + dashCooldown) return;

        float dir = move != 0 ? Mathf.Sign(move) : transform.localScale.x;
        StartCoroutine(DashCoroutine(dir));
    }

    private IEnumerator DashCoroutine(float direction)
    {
        isDashing    = true;
        lastDashTime = Time.time;

        float originalGravity = rb.gravityScale;
        rb.gravityScale  = 0f;
        rb.linearVelocity = new Vector2(direction * dashSpeed, 0f);

        anim?.SetTrigger("Dash");
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.dashSFX);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing       = false;
    }

    // ── Fall multiplier ───────────────────────────────────────────────────────
    private void ApplyFallMultiplier()
    {
        if (rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        else if (rb.linearVelocity.y > 0 && !Input.GetButton("Jump"))
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
    }

    // ── Animator ──────────────────────────────────────────────────────────────
    private void UpdateAnimator()
    {
        if (anim == null) return;
        anim.SetFloat("Speed",           Mathf.Abs(move));
        anim.SetBool ("isGrounded",      isGrounded);
        anim.SetFloat("VerticalVelocity", rb.linearVelocity.y);
    }

    // ── Trigger: caída al vacío ───────────────────────────────────────────────
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Deep"))
        {
            if (health != null) health.InstantKill();
            else GameManager.Instance?.GameOver();
        }
    }
}
