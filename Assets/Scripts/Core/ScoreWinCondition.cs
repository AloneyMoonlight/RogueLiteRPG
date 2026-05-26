using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Comprueba el Score de GameStats cada frame.
/// Cuando supera el umbral, activa la condición de victoria.
///
/// Coloca este script en un GameObject vacío de la escena Game (ej. "Managers").
/// Para el test usa scoreThreshold = 60.
/// Para la win real del nivel usa WinZone en el final del mapa.
/// </summary>
public class ScoreWinCondition : MonoBehaviour
{
    [Header("Umbral de puntuacion")]
    [Tooltip("Puntuacion que hay que alcanzar para ganar (60 para el test)")]
    [SerializeField] private int scoreThreshold = 60;

    [Header("Escena de destino")]
    [SerializeField] private string winSceneName = "Win";

    [Header("Debug")]
    [Tooltip("Muestra en pantalla la puntuacion actual vs umbral")]
    [SerializeField] private bool showDebugGUI = false;

    private bool winTriggered = false;

    void Update()
    {
        if (winTriggered) return;
        if (GameStats.Instance == null) return;

        if (GameStats.Instance.Score >= scoreThreshold)
            TriggerWin();
    }

    private void TriggerWin()
    {
        if (winTriggered) return;
        winTriggered = true;

        Debug.Log($"[ScoreWinCondition] Victoria! Score {GameStats.Instance.Score} >= {scoreThreshold}");

        if (GameManager.Instance != null)
            GameManager.Instance.Win();
        else
        {
            // Fallback si GameManager no existe
            GameStats.Instance?.StopTracking();
            SceneManager.LoadScene(winSceneName);
        }
    }

    // ── Gizmo de debug opcional ───────────────────────────────────────────────
    void OnGUI()
    {
        if (!showDebugGUI) return;
        int current = GameStats.Instance != null ? GameStats.Instance.Score : 0;
        GUI.Label(new Rect(10, 10, 300, 25),
            $"Score: {current} / {scoreThreshold} para ganar");
    }
}
