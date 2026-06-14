using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Daño")]
    [SerializeField] private int attack1Damage = 1;
    [SerializeField] private int attack2Damage = 2;

    [Header("Hitbox")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.5f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Timing del combo")]
    [Tooltip("Duración del clip Attack1 en segundos (ver en Project → Animation clip → Inspector)")]
    [SerializeField] private float attack1Duration = 0.4f;
    [Tooltip("Duración del clip Attack2 en segundos")]
    [SerializeField] private float attack2Duration = 0.5f;
    [Tooltip("En qué fracción de Attack1 conecta el golpe (0-1). Ej: 0.25 = al 25% de la anim")]
    [SerializeField] private float attack1HitMoment = 0.25f;
    [Tooltip("En qué fracción de Attack2 conecta el golpe")]
    [SerializeField] private float attack2HitMoment = 0.25f;

    // ── Estado público (PlayerMovement lo consulta) ─────────────────────────
    public bool IsAttacking { get; private set; }

    // ── Estado interno ──────────────────────────────────────────────────────
    private bool comboQueued = false;

    private Animator     animator;
    private PlayerHealth health;

    // Hash del parámetro del Animator (más eficiente que strings)
    private static readonly int ComboStepHash = Animator.StringToHash("ComboStep");

    // ── Init ────────────────────────────────────────────────────────────────
    void Start()
    {
        animator = GetComponent<Animator>();
        health   = GetComponent<PlayerHealth>();
    }

    // ── Input ───────────────────────────────────────────────────────────────
    void Update()
    {
        if (health != null && health.IsDead) return;

        if (Input.GetButtonDown("Fire1"))
        {
            if (!IsAttacking)
            {
                StartCoroutine(Attack1Routine());
            }
            else if (!comboQueued)
            {
                // ► Clave del fix: ComboStep = 2 INMEDIATAMENTE
                // El animator puede ver el nuevo valor y hacer la transición
                // en cuanto llegue al Exit Time configurado (0.5 recomendado).
                comboQueued = true;
                animator?.SetInteger(ComboStepHash, 2);
            }
        }
    }

    // ── Attack 1 ────────────────────────────────────────────────────────────
    private IEnumerator Attack1Routine()
    {
        IsAttacking  = true;
        comboQueued  = false;

        animator?.SetInteger(ComboStepHash, 1);
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.attackSFX);

        // Espera hasta el frame de impacto y aplica hitbox
        yield return new WaitForSeconds(attack1Duration * attack1HitMoment);
        ApplyHitbox(attack1Damage);

        // Espera el resto de la animación
        yield return new WaitForSeconds(attack1Duration * (1f - attack1HitMoment));

        if (comboQueued)
            yield return Attack2Routine();
        else
            ResetCombo();
    }

    // ── Attack 2 (cadena) ───────────────────────────────────────────────────
    private IEnumerator Attack2Routine()
    {
        // ComboStep ya fue puesto a 2 cuando el jugador presionó el botón.
        // Solo gestionamos el hitbox y el timing aquí.
        comboQueued = false;

        AudioManager.Instance?.PlaySFX(AudioManager.Instance.attackSFX);

        yield return new WaitForSeconds(attack2Duration * attack2HitMoment);
        ApplyHitbox(attack2Damage);

        yield return new WaitForSeconds(attack2Duration * (1f - attack2HitMoment));

        ResetCombo();
    }

    // ── Reset ────────────────────────────────────────────────────────────────
    private void ResetCombo()
    {
        IsAttacking  = false;
        comboQueued  = false;
        animator?.SetInteger(ComboStepHash, 0);
    }

    // ── Hitbox ───────────────────────────────────────────────────────────────
    private void ApplyHitbox(int damage)
    {
        if (attackPoint == null) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            damageable?.TakeDamage(damage);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
