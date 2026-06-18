using System.Collections;
using UnityEngine;

/// <summary>
/// Clase base para todos los enemigos.
/// Gestiona: vida, daño recibido, flash de daño, muerte y referencia al jugador.
/// Cada enemigo hijo implementa OnInterrupted() para cancelar su ataque en curso.
/// </summary>
public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    [Header("Vida")]
    [SerializeField] protected int maxHealth = 3;

    [Header("Daño al jugador")]
    [SerializeField] protected int contactDamage = 1;

    [Header("Stun al recibir daño")]
    [SerializeField] protected float stunDuration = 0.5f;

    // ── Estado ───────────────────────────────────────────────────────────────
    protected int  currentHealth;
    protected bool isDead;
    protected bool isStunned;

    // ── Referencias ──────────────────────────────────────────────────────────
    protected Transform      player;
    protected Rigidbody2D    rb;
    protected Animator       animator;
    protected SpriteRenderer spriteRenderer;

    // ── Hashes (eficiencia en Animator) ──────────────────────────────────────
    protected static readonly int SpeedHash = Animator.StringToHash("Speed");
    protected static readonly int DieHash   = Animator.StringToHash("Die");
    protected static readonly int StunHash  = Animator.StringToHash("Stun");

    // ── Init ──────────────────────────────────────────────────────────────────
    protected virtual void Awake()
    {
        rb             = GetComponent<Rigidbody2D>();
        animator       = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    // ── IDamageable ──────────────────────────────────────────────────────────
    public virtual void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
            Die();
        else
            OnInterrupted();
    }

    // Cada hijo cancela su propio ataque aquí
    protected abstract void OnInterrupted();

    // ── Muerte ───────────────────────────────────────────────────────────────
    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        GameStats.Instance?.AddKill();

        rb.linearVelocity = Vector2.zero;
        rb.bodyType        = RigidbodyType2D.Kinematic;

        foreach (Collider2D col in GetComponents<Collider2D>())
            col.enabled = false;

        animator?.SetTrigger(DieHash);
        StartCoroutine(DestroyAfterDeath());
    }

    private IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }

    // ── Flash rojo al recibir daño ────────────────────────────────────────────
    private IEnumerator DamageFlash()
    {
        if (spriteRenderer == null) yield break;
        Color original = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        if (spriteRenderer != null) spriteRenderer.color = original;
    }

    // ── Utilidades ───────────────────────────────────────────────────────────
    protected void FacePlayer()
    {
        if (player == null) return;
        float dir = player.position.x - transform.position.x;
        if (Mathf.Abs(dir) > 0.05f)
            transform.localScale = new Vector3(Mathf.Sign(dir), 1f, 1f);
    }

    protected float DistanceToPlayer() =>
        player != null ? Vector2.Distance(transform.position, player.position) : float.MaxValue;

    protected float DirectionToPlayer() =>
        player != null ? Mathf.Sign(player.position.x - transform.position.x) : 1f;
}
