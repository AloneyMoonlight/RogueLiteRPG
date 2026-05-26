using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    // ── Barra de relleno (usa Healthbarr_inside.png) ─────────────────────────
    [Header("Barra de vida")]
    [Tooltip("Arrastra la imagen INTERIOR (Healthbarr_inside). Image Type debe ser 'Filled'")]
    [SerializeField] private Image healthFillImage;

    // ── Corazones (alternativa) ───────────────────────────────────────────────
    [Header("Corazones (opcional, alternativa a la barra)")]
    [SerializeField] private Image[] hearts;
    [SerializeField] private Sprite fullHeart;
    [SerializeField] private Sprite emptyHeart;

    public void UpdateHealth(int current, int max)
    {
        // ── Barra de relleno ─────────────────────────────────────────────────
        // fillAmount va de 0.0 (vacía) a 1.0 (llena)
        if (healthFillImage != null)
            healthFillImage.fillAmount = max > 0 ? (float)current / max : 0f;

        // ── Corazones ────────────────────────────────────────────────────────
        if (hearts != null && hearts.Length > 0 && fullHeart != null && emptyHeart != null)
        {
            for (int i = 0; i < hearts.Length; i++)
            {
                if (hearts[i] == null) continue;
                hearts[i].sprite = i < current ? fullHeart : emptyHeart;
            }
        }
    }
}
