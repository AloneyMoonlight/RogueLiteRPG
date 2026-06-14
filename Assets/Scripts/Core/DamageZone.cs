using UnityEngine;

/// <summary>
/// Añade este script a cualquier zona/objeto que deba dañar al jugador al tocarlo.
/// Funciona tanto con colliders sólidos (isTrigger=false, jugador choca)
/// como con triggers (isTrigger=true, jugador pasa por dentro).
///
/// Uso recomendado:
///   • Pinchos de suelo  → collider sólido + este script (OnCollisionEnter2D)
///   • Bola de pinchos   → collider sólido + este script (OnCollisionEnter2D)
///   • Zona de lava/gas  → collider trigger + este script (OnTriggerEnter2D)
/// </summary>
public class DamageZone : MonoBehaviour
{
    [Header("Daño")]
    [Tooltip("Puntos de vida que quita al jugador en cada contacto")]
    [SerializeField] private int damage = 1;

    [Header("Muerte instantanea (opcional)")]
    [Tooltip("Si está activo, mata al jugador de un golpe ignorando la armadura/invencibilidad")]
    [SerializeField] private bool instantKill = false;

    // ── Colisión sólida (isTrigger = false) ──────────────────────────────────
    void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.gameObject.CompareTag("Player")) return;
        ApplyDamage(col.gameObject);
    }

    // ── Trigger (isTrigger = true) ────────────────────────────────────────────
    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;
        ApplyDamage(col.gameObject);
    }

    // ── Lógica común ──────────────────────────────────────────────────────────
    private void ApplyDamage(GameObject player)
    {
        if (instantKill)
        {
            // Mata al jugador directamente (útil para fosos o trampas letales)
            player.GetComponent<PlayerHealth>()?.InstantKill();
        }
        else
        {
            // Daño normal — respeta la invencibilidad del PlayerHealth
            player.GetComponent<IDamageable>()?.TakeDamage(damage);
        }
    }
}
