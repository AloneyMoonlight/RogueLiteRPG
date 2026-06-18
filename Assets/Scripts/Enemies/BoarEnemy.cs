using System.Collections;
using UnityEngine;

/// <summary>
/// Jabalí: cuando detecta al jugador hace un windup y carga a alta velocidad.
///
/// Comportamiento:
///   • Jugador en rango → pausa (windup telegráfico) → carga
///   • Colisión con jugador → daño + deceleración suave durante ~1s
///   • Colisión con pared   → parada INSTANTÁNEA
///   • Golpeado durante la carga → interrumpido + stun
///
/// ── Configuración en Inspector ──────────────────────────────────────────────
/// • Enemy GameObject Layer: debe ser "Enemy"
/// • El Rigidbody2D debe ser Dynamic, Collision Detection: Continuous
///
/// ── Parámetros Animator ──────────────────────────────────────────────────────
/// Speed (float), Windup (trigger), IsCharging (bool), Stun (trigger), Die (trigger)
/// </summary>
public class BoarEnemy : EnemyBase
{
    [Header("Detección")]
    [SerializeField] private float detectRange = 7f;

    [Header("Carga")]
    [SerializeField] private float windupDuration   = 0.6f;  // pausa antes de cargar
    [SerializeField] private float chargeSpeed      = 9f;
    [SerializeField] private float recoveryDuration = 1f;    // deceleración tras golpear al jugador
    [SerializeField] private float chargeCooldown   = 2f;    // espera antes de poder cargar de nuevo

    // ── Estado ────────────────────────────────────────────────────────────────
    private enum State { Idle, Windup, Charging, Recovering, Cooldown, Stunned }
    private State state = State.Idle;

    private float     chargeDir    = 1f;
    private bool      hasHitPlayer = false;
    private Coroutine activeCoroutine;

    private static readonly int WindupHash    = Animator.StringToHash("Windup");
    private static readonly int IsChargingHash = Animator.StringToHash("IsCharging");

    // ── Update ────────────────────────────────────────────────────────────────
    void Update()
    {
        if (isDead || isStunned || player == null) return;

        animator?.SetBool(IsChargingHash, state == State.Charging);
        animator?.SetFloat(SpeedHash, Mathf.Abs(rb.linearVelocity.x));

        if (state == State.Idle && DistanceToPlayer() <= detectRange)
        {
            FacePlayer();
            chargeDir = DirectionToPlayer();
            activeCoroutine = StartCoroutine(ChargeSequence());
        }
    }

    void FixedUpdate()
    {
        if (isDead || isStunned) return;

        // Solo durante la carga aplicamos la velocidad; Recovery la gestiona su coroutine
        if (state == State.Charging)
            rb.linearVelocity = new Vector2(chargeDir * chargeSpeed, rb.linearVelocity.y);
    }

    // ── Secuencia de carga ────────────────────────────────────────────────────
    private IEnumerator ChargeSequence()
    {
        // 1. Windup: pausa telegráfica antes de cargar
        state = State.Windup;
        rb.linearVelocity = Vector2.zero;
        animator?.SetTrigger(WindupHash);
        yield return new WaitForSeconds(windupDuration);

        if (isDead || isStunned) yield break;

        // 2. Carga
        state        = State.Charging;
        hasHitPlayer = false;
    }

    // ── Colisiones ────────────────────────────────────────────────────────────
    void OnCollisionEnter2D(Collision2D col)
    {
        if (isDead || state != State.Charging) return;

        // ── Jugador ──────────────────────────────────────────────────────────
        if (col.gameObject.CompareTag("Player"))
        {
            if (!hasHitPlayer)
            {
                hasHitPlayer = true;
                state        = State.Recovering;          // cambia estado YA (evita race con FixedUpdate)
                col.gameObject.GetComponent<IDamageable>()?.TakeDamage(contactDamage);
                activeCoroutine = StartCoroutine(RecoverAfterHit());
            }
            return;
        }

        // ── Pared (normal horizontal) → parada instantánea ───────────────────
        foreach (ContactPoint2D contact in col.contacts)
        {
            if (Mathf.Abs(contact.normal.x) >= 0.7f)
            {
                state             = State.Cooldown;
                rb.linearVelocity = Vector2.zero;
                activeCoroutine   = StartCoroutine(CooldownRoutine());
                return;
            }
        }
    }

    // ── Recuperación tras golpear jugador (deceleración suave ~1s) ────────────
    private IEnumerator RecoverAfterHit()
    {
        float elapsed    = 0f;
        float startSpeed = Mathf.Abs(rb.linearVelocity.x);

        while (elapsed < recoveryDuration)
        {
            float spd = Mathf.Lerp(startSpeed, 0f, elapsed / recoveryDuration);
            rb.linearVelocity = new Vector2(chargeDir * spd, rb.linearVelocity.y);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        if (!isDead && !isStunned)
            activeCoroutine = StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        state = State.Cooldown;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        yield return new WaitForSeconds(chargeCooldown);
        if (!isDead && !isStunned) state = State.Idle;
    }

    // ── Interrupción (golpeado durante la carga) ──────────────────────────────
    protected override void OnInterrupted()
    {
        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(StunRoutine());
    }

    private IEnumerator StunRoutine()
    {
        isStunned         = true;
        state             = State.Stunned;
        rb.linearVelocity = Vector2.zero;
        animator?.SetTrigger(StunHash);

        yield return new WaitForSeconds(stunDuration);

        isStunned = false;
        if (!isDead) activeCoroutine = StartCoroutine(CooldownRoutine());
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}
