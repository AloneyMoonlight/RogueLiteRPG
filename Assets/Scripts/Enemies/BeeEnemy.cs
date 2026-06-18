using System.Collections;
using UnityEngine;

/// <summary>
/// Abeja: vuela rastreando al jugador y se lanza en picado para atacar.
/// Tras 2 picados queda cansada y vulnerable en el suelo.
///
/// Comportamiento:
///   Flying   → rastrea al jugador horizontalmente a flyHeight sobre el suelo
///   Targeting → lleva targetTime segundos apuntando antes de picar
///   Diving   → baja en picado a diveSpeed; golpea al jugador o choca con el suelo
///   Tired    → cae al suelo, es vulnerable durante tiredDuration
///   (repite desde Flying con diveCount = 0)
///
///   Si el jugador la golpea en CUALQUIER estado → queda aturdida (stun)
///   Si la golpean durante el picado → el picado se cancela (no suma al contador)
///
/// ── Configuración en Inspector ──────────────────────────────────────────────
/// • Rigidbody2D: Dynamic, Gravity Scale = 0
/// • Collider2D: IsTrigger = TRUE (para volar y picar sin colisión física)
/// • Ground Layer: layer del suelo/plataformas
/// • Player Layer: layer del jugador
/// • Enemy Layer: este GameObject debe estar en layer "Enemy"
///
/// ── Parámetros Animator ──────────────────────────────────────────────────────
/// Speed (float), IsTired (bool), Dive (trigger), Stun (trigger), Die (trigger)
/// </summary>
public class BeeEnemy : EnemyBase
{
    [Header("Vuelo")]
    [SerializeField] private float flyHeight   = 4f;   // altura sobre el suelo
    [SerializeField] private float flySpeed    = 3.5f; // velocidad de seguimiento
    [SerializeField] private float detectRange = 7f;   // rango para iniciar targeting

    [Header("Picado")]
    [SerializeField] private float targetTime        = 1.2f; // segundos rastreando antes de picar
    [SerializeField] private float diveSpeed         = 9f;
    [SerializeField] private float diveHitRadius     = 0.5f; // radio del hitbox durante el picado
    [SerializeField] private int   divesBeforeTired  = 2;
    [SerializeField] private float tiredDuration     = 3f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask playerLayer;

    // ── Estado ────────────────────────────────────────────────────────────────
    private enum State { Flying, Targeting, Diving, Tired, Stunned }
    private State state = State.Flying;

    private int   diveCount   = 0;
    private float targetTimer = 0f;
    private bool  divingActive = false; // flag para cortar el picado inmediatamente desde OnInterrupted
    private Coroutine activeCoroutine;

    private static readonly int IsTiredHash = Animator.StringToHash("IsTired");
    private static readonly int DiveHash    = Animator.StringToHash("Dive");

    // ── Init ──────────────────────────────────────────────────────────────────
    protected override void Awake()
    {
        base.Awake();
        if (rb != null) rb.gravityScale = 0f; // la abeja vuela, sin gravedad
    }

    // ── Update ────────────────────────────────────────────────────────────────
    void Update()
    {
        if (isDead) return;
        animator?.SetBool(IsTiredHash, state == State.Tired);
    }

    void FixedUpdate()
    {
        if (isDead || isStunned) return;

        switch (state)
        {
            case State.Flying:    UpdateFlying();    break;
            case State.Targeting: UpdateTargeting(); break;
            case State.Diving:    UpdateDiving();    break;
            case State.Tired:
                rb.linearVelocity = Vector2.zero;   break;
        }

        animator?.SetFloat(SpeedHash, rb.linearVelocity.magnitude);
    }

    // ── Vuelo (seguimiento horizontal a flyHeight) ────────────────────────────
    private void UpdateFlying()
    {
        if (player == null) return;

        Vector2 target = new Vector2(player.position.x, GetGroundY() + flyHeight);
        Vector2 delta  = target - (Vector2)transform.position;

        rb.linearVelocity = new Vector2(
            Mathf.Lerp(rb.linearVelocity.x, delta.x * flySpeed, Time.fixedDeltaTime * 4f),
            Mathf.Lerp(rb.linearVelocity.y, delta.y * flySpeed, Time.fixedDeltaTime * 4f));

        FacePlayer();

        if (DistanceToPlayer() <= detectRange)
        {
            state       = State.Targeting;
            targetTimer = 0f;
        }
    }

    // ── Targeting (apunta al jugador antes de picar) ──────────────────────────
    private void UpdateTargeting()
    {
        if (player == null) return;

        // Sigue al jugador más despacio mientras apunta
        Vector2 target = new Vector2(player.position.x, GetGroundY() + flyHeight);
        Vector2 delta  = target - (Vector2)transform.position;

        rb.linearVelocity = new Vector2(
            Mathf.Lerp(rb.linearVelocity.x, delta.x * flySpeed * 0.4f, Time.fixedDeltaTime * 4f),
            Mathf.Lerp(rb.linearVelocity.y, delta.y * flySpeed * 0.4f, Time.fixedDeltaTime * 4f));

        FacePlayer();
        targetTimer += Time.fixedDeltaTime;

        if (DistanceToPlayer() > detectRange * 1.5f)
        {
            state = State.Flying; // jugador muy lejos, volver a volar
            return;
        }

        if (targetTimer >= targetTime)
            StartDive();
    }

    // ── Inicio del picado ────────────────────────────────────────────────────
    private void StartDive()
    {
        state        = State.Diving;
        divingActive = true;
        rb.linearVelocity = new Vector2(0f, -diveSpeed);
        animator?.SetTrigger(DiveHash);
    }

    // ── Picado (baja recto, busca jugador y suelo) ────────────────────────────
    private void UpdateDiving()
    {
        if (!divingActive) return;

        rb.linearVelocity = new Vector2(0f, -diveSpeed);

        // Comprobar si golpea al jugador (OverlapCircle es más fiable que trigger en kinematic)
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, diveHitRadius, playerLayer);
        if (hits.Length > 0)
        {
            hits[0].GetComponent<IDamageable>()?.TakeDamage(contactDamage);
            EndDiveFromHit(countDive: true);
            return;
        }

        // Comprobar si llega al suelo (sin haber golpeado al jugador)
        RaycastHit2D ground = Physics2D.Raycast(
            transform.position, Vector2.down, 0.35f, groundLayer);
        if (ground.collider != null)
            EndDiveFromHit(countDive: true);
    }

    private void EndDiveFromHit(bool countDive)
    {
        if (!divingActive) return;
        divingActive = false;

        if (countDive) diveCount++;
        rb.linearVelocity = Vector2.zero;

        if (activeCoroutine != null) StopCoroutine(activeCoroutine);
        activeCoroutine = StartCoroutine(AfterDiveRoutine());
    }

    private IEnumerator AfterDiveRoutine()
    {
        state = State.Flying; // temporal hasta decidir qué sigue
        yield return new WaitForSeconds(0.25f);

        if (isDead) yield break;

        if (diveCount >= divesBeforeTired)
            activeCoroutine = StartCoroutine(TiredRoutine());
        else
        {
            // Repetir desde targeting
            state       = State.Targeting;
            targetTimer = 0f;
        }
    }

    // ── Cansancio (vulnerable en el suelo) ───────────────────────────────────
    private IEnumerator TiredRoutine()
    {
        state = State.Tired;
        rb.linearVelocity = Vector2.zero;

        // Posicionar en el suelo mediante raycast
        RaycastHit2D groundHit = Physics2D.Raycast(
            transform.position, Vector2.down, 25f, groundLayer);
        if (groundHit.collider != null)
            transform.position = groundHit.point + Vector2.up * 0.3f;

        yield return new WaitForSeconds(tiredDuration);

        if (isDead) yield break;

        // Levantarse y volver a volar
        diveCount = 0;
        state     = State.Flying;
    }

    // ── Interrupción (jugador golpea a la abeja) ──────────────────────────────
    protected override void OnInterrupted()
    {
        divingActive = false; // cancela el picado inmediatamente (antes del coroutine)

        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }

        activeCoroutine = StartCoroutine(StunRoutine());
    }

    private IEnumerator StunRoutine()
    {
        isStunned         = true;
        state             = State.Stunned;
        rb.linearVelocity = Vector2.zero;
        animator?.SetTrigger(StunHash);

        // Cae al suelo durante el stun
        RaycastHit2D groundHit = Physics2D.Raycast(
            transform.position, Vector2.down, 25f, groundLayer);
        if (groundHit.collider != null)
            transform.position = groundHit.point + Vector2.up * 0.3f;

        yield return new WaitForSeconds(stunDuration);

        isStunned = false;
        if (!isDead) state = State.Flying;
    }

    // ── Utilidad: altura del suelo bajo la abeja ──────────────────────────────
    private float GetGroundY()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 30f, groundLayer);
        return hit.collider != null ? hit.point.y : (transform.position.y - flyHeight);
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, diveHitRadius);
        // Altura de vuelo
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * flyHeight);
    }
}
