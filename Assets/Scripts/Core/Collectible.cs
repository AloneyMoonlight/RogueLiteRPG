using UnityEngine;

/// <summary>
/// Añade este script a cualquier objeto que el jugador pueda recoger.
/// Funciona con cualquier sprite/prefab — solo necesita un Collider2D en modo Trigger.
/// </summary>
public class Collectible : MonoBehaviour
{
    public enum CollectibleType
    {
        Coin,       // Suma moneda + puntos base
        ExtraLife,  // Cura 1 punto de vida
        ScoreGem,   // Solo suma puntos (customPoints)
    }

    [Header("Tipo")]
    [SerializeField] private CollectibleType type = CollectibleType.Coin;

    [Header("Puntos (deja 0 para usar el valor de GameStats)")]
    [SerializeField] private int customPoints = 0;

    [Header("Visual / Efectos")]
    [SerializeField] private GameObject collectEffect; // Prefab de partícula opcional

    // ── Trigger ───────────────────────────────────────────────────────────────
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        Collect(other);
    }

    private void Collect(Collider2D player)
    {
        switch (type)
        {
            case CollectibleType.Coin:
                GameStats.Instance?.AddCoin();
                if (customPoints > 0) GameStats.Instance?.AddScore(customPoints);
                break;

            case CollectibleType.ExtraLife:
                player.GetComponent<PlayerHealth>()?.Heal(1);
                if (customPoints > 0) GameStats.Instance?.AddScore(customPoints);
                break;

            case CollectibleType.ScoreGem:
                if (customPoints > 0)
                    GameStats.Instance?.AddScore(customPoints);
                else
                    GameStats.Instance?.AddCollectible();
                break;
        }

        AudioManager.Instance?.PlaySFX(AudioManager.Instance.collectSFX);

        if (collectEffect != null)
            Instantiate(collectEffect, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
