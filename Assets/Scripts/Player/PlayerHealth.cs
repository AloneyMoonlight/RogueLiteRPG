using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Invincibility Frames")]
    [SerializeField] private float invincibilityDuration = 1.5f;
    [SerializeField] private float flashInterval = 0.1f;
    private bool isInvincible;

    [Header("Events")]
    public UnityEvent<int, int> onHealthChanged; // (current, max)
    public UnityEvent onDeath;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead { get; private set; }

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private PlayerMovement movement;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        movement = GetComponent<PlayerMovement>();
        onHealthChanged?.Invoke(currentHealth, maxHealth);
    }

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

    private void Die()
    {
        IsDead = true;
        if (movement != null) movement.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        animator?.SetTrigger("Die");
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.deathSFX);
        onDeath?.Invoke();

        StartCoroutine(TriggerGameOver());
    }

    private IEnumerator TriggerGameOver()
    {
        // Wait for death animation before loading game over scene
        yield return new WaitForSecondsRealtime(1.5f);
        GameManager.Instance?.GameOver();
    }

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
