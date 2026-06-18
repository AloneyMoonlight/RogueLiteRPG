using System.Collections;
using UnityEngine;

/// <summary>
/// Caracol: patrulla la plataforma, persigue al jugador al detectarlo
/// y ataca al alcanzarlo con un golpe cuerpo a cuerpo.
/// Se interrumpe si el jugador le golpea durante el ataque.
///
/// ── Configuración en Inspector ──────────────────────────────────────────────
/// • AttackPoint: Transform hijo delante del caracol (donde aparece el hitbox)
/// • Ground Layer: layer del suelo (para detectar bordes durante patrulla)
/// • Player Layer: layer del jugador (para el hitbox del ataque)
/// • Enemy GameObject Layer: debe ser "Enemy" para que PlayerCombat lo detecte
///
/// ── Parámetros Animator ──────────────────────────────────────────────────────
/// Speed (float), Attack (trigger), Stun (trigger), Die (trigger)
/// </summary>
public class SnailEnemy : EnemyBase
{
    [Header("Movimiento")]
    [SerializeField] private float patrolSpeed  = 1.2f;
    [SerializeField] private float chaseSpeed   = 2.2f;
    [SerializeField] private float detectRange  = 5f;

    [Header("Ataque")]
    [SerializeField] private int       attackDamage   = 1;
    [SerializeField] private float     attackRange    = 1.2f;
    [SerializeField] private float     attackDuration = 0.7f;
    [SerializeField] private float     attackHitFrac  = 0.55f; // fracción del clip en que golpea
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float     attackRadius   = 0.5f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Patrulla")]
    [SerializeField] private LayerMask groundLayer;

    // ── Estado ────────────────────────────────────────────────────────────────
    private enum State { Patrol, Chase, Attack, Stunned }
    private State state = State.Patrol;

    private float     patrolDir = 1f;
    private Coroutine attackCoroutine;

    private static readonly int AttackHash = Animator.StringToHash("Attack");

    // ── Ciclo principal ───────────────────────────────────────────────────────
    void Update()
    {
        if (isDead || player == null) return;

        float dist = DistanceToPlayer();

        switch (state)
        {
            case State.Patrol:
                DoPatrol();
                if (dist <= detectRange)
                    state = State.Chase;
                break;

            case State.Chase:
                if (dist <= attackRange)
                    BeginAttack();
                else if (dist > detectRange * 1.5f)
                    state = State.Patrol;
                else
                    DoChase();
                break;

            case State.Attack:
            case State.Stunned:
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                break;
        }

        animator?.SetFloat(SpeedHash, Mathf.Abs(rb.linearVelocity.x));
    }

    // ── Patrulla con detección de paredes y bordes ────────────────────────────
    private void DoPatrol()
    {
        // Pared adelante
        RaycastHit2D wall = Physics2D.Raycast(
            transform.position,
            new Vector2(patrolDir, 0f),
            0.6f, groundLayer);

        // Borde: no hay suelo justo por delante-abajo
        Vector2 edgeOrigin = (Vector2)transform.position + new Vector2(patrolDir * 0.6f, -0.1f);
        RaycastHit2D edge = Physics2D.Raycast(edgeOrigin, Vector2.down, 0.5f, groundLayer);

        if (wall.collider != null || edge.collider == null)
            patrolDir = -patrolDir;

        rb.linearVelocity = new Vector2(patrolDir * patrolSpeed, rb.linearVelocity.y);
        transform.localScale = new Vector3(patrolDir, 1f, 1f);
    }

    private void DoChase()
    {
        FacePlayer();
        rb.linearVelocity = new Vector2(DirectionToPlayer() * chaseSpeed, rb.linearVelocity.y);
    }

    // ── Ataque ────────────────────────────────────────────────────────────────
    private void BeginAttack()
    {
        if (state == State.Attack) return;
        state = State.Attack;
        FacePlayer();
        attackCoroutine = StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        animator?.SetTrigger(AttackHash);

        yield return new WaitForSeconds(attackDuration * attackHitFrac);

        // Aplicar hitbox si no fue interrumpido
        if (!isDead && !isStunned && attackPoint != null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(
                attackPoint.position, attackRadius, playerLayer);
            foreach (var h in hits)
                h.GetComponent<IDamageable>()?.TakeDamage(attackDamage);
        }

        yield return new WaitForSeconds(attackDuration * (1f - attackHitFrac));

        if (!isDead && !isStunned) state = State.Chase;
    }

    // ── Interrupción (recibe daño durante el ataque) ──────────────────────────
    protected override void OnInterrupted()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }
        StartCoroutine(StunRoutine());
    }

    private IEnumerator StunRoutine()
    {
        isStunned = true;
        state     = State.Stunned;
        animator?.SetTrigger(StunHash);
        yield return new WaitForSeconds(stunDuration);
        isStunned = false;
        if (!isDead) state = State.Chase;
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}
