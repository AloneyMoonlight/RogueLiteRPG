using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Vida")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Invencibilidad tras recibir daño")]
    [SerializeField] private float invincibilityDuration = 1.5f;
    [SerializeField] private float flashInterval = 0.1f;
    private bool isInvincible;

    [Header("Eventos")]
    [Tooltip("Se dispara cuando la vida cambia. Útil para actualizar la UI de vida.")]
    public UnityEvent<int, int> onHealthChanged;  // (currentHP, maxHP)
    [Tooltip("Se dispara al morir. Añade aquí efectos extra desde el Inspector (shake, fade, partículas...).")]
    public UnityEvent onDeath;

    public int  CurrentHealth => currentHealth;
    public int  MaxHealth     => maxHealth;
    public bool IsDead        { get; private set; }

    private SpriteRenderer spriteRenderer;
    private Animator       animator;
    private PlayerMovement movement;
    private PlayerCombat   combat;
    private Rigidbody2D    rb;

    void Start()
    {
        currentHealth  = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator       = GetComponent<Animator>();
        movement       = GetComponent<PlayerMovement>();
        combat         = GetComponent<PlayerCombat>();
        rb             = GetComponent<Rigidbody2D>();

        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // ── Daño ──────────────────────────────────────────────────────────────────
    public void TakeDamage(int amount)
    {
        if (isInvincible || IsDead) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        onHealthChanged?.Invoke(currentHealth, maxHealth);
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.hurtSFX);

        if (currentHealth <= 0)
            Die();
        else
            StartCoroutine(InvincibilityCoroutine());
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void InstantKill()
    {
        if (IsDead) return;
        currentHealth = 0;
        Die();
    }

    // ── Muerte ────────────────────────────────────────────────────────────────
    private void Die()
    {
        if (IsDead) return;   // guarda contra doble llamada
        IsDead = true;

        // 1. Cortar movimiento y combate
        if (movement != null) movement.enabled = false;
        if (combat   != null) combat.enabled   = false;

        // 2. Congelar física (el personaje queda donde cayó)
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType       = RigidbodyType2D.Kinematic;
        }

        // 3. Deshabilitar colliders → enemigos no siguen interactuando con el cadáver
        foreach (Collider2D col in GetComponents<Collider2D>())
            col.enabled = false;

        // 4. Animación y sonido
        animator?.SetTrigger("Die");
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.deathSFX);

        // 5. Dispara el evento → aquí van los efectos extra del Inspector
        onDeath?.Invoke();

        // 6. Carga Game Over tras la animación
        StartCoroutine(TriggerGameOver());
    }

    private IEnumerator TriggerGameOver()
    {
        yield return new WaitForSecondsRealtime(1.5f);

        if (GameManager.Instance != null)
            GameManager.Instance.GameOver();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameOver");
    }

    // ── Invencibilidad (parpadeo) ─────────────────────────────────────────────
    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        float elapsed = 0f;
        while (elapsed < invincibilityDuration)
        {
            if (spriteRenderer != null)
                spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        isInvincible = false;
    }
}
