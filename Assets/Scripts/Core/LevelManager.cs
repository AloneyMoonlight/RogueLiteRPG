using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Coloca este script en un GameObject vacio (ej. "LevelManager") en cada
/// escena de NIVEL con enemigos (nivel_1, nivel_2, nivel_3).
///
/// Detecta cuando el mapa esta limpio: no quedan coleccionables NI enemigos
/// vivos. En ese momento dispara la pantalla de Victoria.
///   • Si isFinalLevel = false  -> Victoria con boton "Continuar" -> nextScene
///   • Si isFinalLevel = true   -> pantalla "Fin del juego"
///
/// La aldea NO lleva este script (se sale por el borde tras hablar con el NPC).
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Progresion")]
    [Tooltip("Escena del siguiente nivel (ignorado si es el nivel final)")]
    [SerializeField] private string nextSceneName = "nivel_2";
    [Tooltip("Marca esto en el ULTIMO nivel -> mostrara 'Fin del juego'")]
    [SerializeField] private bool   isFinalLevel  = false;

    [Header("Condiciones de victoria")]
    [SerializeField] private bool requireCollectibles = true;
    [SerializeField] private bool requireEnemiesDead  = true;

    [Header("Avanzado")]
    [Tooltip("Cada cuanto comprueba el estado del nivel (segundos)")]
    [SerializeField] private float checkInterval = 0.5f;

    // ── Estado ────────────────────────────────────────────────────────────────
    private bool  armed;       // true cuando ha existido al menos 1 objetivo
    private bool  completed;
    private float timer;

    void Update()
    {
        if (completed) return;

        timer += Time.deltaTime;
        if (timer < checkInterval) return;
        timer = 0f;

        int remaining = CountRemaining();

        // Solo "armamos" el nivel cuando aparece contenido (soporta spawners).
        if (remaining > 0) armed = true;

        if (armed && remaining == 0)
            Complete();
    }

    private int CountRemaining()
    {
        int total = 0;

        if (requireCollectibles)
            total += FindObjectsByType<Collectible>(FindObjectsSortMode.None).Length;

        if (requireEnemiesDead)
        {
            foreach (EnemyBase e in FindObjectsByType<EnemyBase>(FindObjectsSortMode.None))
                if (!e.IsDead) total++;
        }

        return total;
    }

    private void Complete()
    {
        completed = true;
        Debug.Log($"[LevelManager] Nivel completado. Final={isFinalLevel}, siguiente={nextSceneName}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LevelComplete(nextSceneName, isFinalLevel);
        }
        else
        {
            // Fallback sin GameManager
            GameStats.Instance?.StopTracking();
            SceneManager.LoadScene("Win");
        }
    }
}
