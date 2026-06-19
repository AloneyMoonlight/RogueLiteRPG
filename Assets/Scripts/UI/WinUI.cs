using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Pantalla de Victoria. Tiene dos modos segun GameManager:
///   • Nivel intermedio  -> Titulo "¡VICTORIA!"   + boton CONTINUAR (siguiente nivel)
///   • Nivel final        -> Titulo "¡FIN DEL JUEGO!" + sin Continuar (solo Menu)
/// </summary>
public class WinUI : MonoBehaviour
{
    [Header("Nombre de escenas")]
    [Tooltip("Fallback si GameManager no indica el siguiente nivel")]
    [SerializeField] private string fallbackNextScene = "nivel_1";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Titulo")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private string   victoryTitle  = "¡VICTORIA!";
    [SerializeField] private string   gameEndTitle  = "¡FIN DEL JUEGO!";

    [Header("Estadisticas (arrastra TextMeshPro)")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text enemiesKilledText;
    [SerializeField] private TMP_Text distanceTraveledText;
    [SerializeField] private TMP_Text timeSurvivedText;

    [Header("Botones")]
    [Tooltip("Avanza al siguiente nivel. Se oculta en el nivel final.")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    private bool   isGameComplete;
    private string nextScene;

    void Start()
    {
        Time.timeScale = 1f;

        // ── Leer el estado de progresion del GameManager ──────────────────────
        if (GameManager.Instance != null)
        {
            isGameComplete = GameManager.Instance.IsGameComplete;
            nextScene      = GameManager.Instance.NextSceneToLoad;
        }
        if (string.IsNullOrEmpty(nextScene)) nextScene = fallbackNextScene;

        // ── Titulo + visibilidad del boton Continuar ──────────────────────────
        if (titleText != null)
            titleText.text = isGameComplete ? gameEndTitle : victoryTitle;

        if (continueButton != null)
            continueButton.gameObject.SetActive(!isGameComplete);

        // ── Listeners ─────────────────────────────────────────────────────────
        if (continueButton != null) continueButton.onClick.AddListener(OnContinue);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);
        if (quitButton != null)     quitButton.onClick.AddListener(OnQuit);

        DisplayStats();

        if (AudioManager.Instance != null && AudioManager.Instance.winMusic != null)
            AudioManager.Instance.PlayMusic(AudioManager.Instance.winMusic);
    }

    private void DisplayStats()
    {
        GameStats s = GameStats.Instance;
        if (s == null) return;

        if (scoreText != null)
            scoreText.text = "Puntuacion: " + s.Score;
        if (enemiesKilledText != null)
            enemiesKilledText.text = "Enemigos eliminados: " + s.EnemiesKilled;
        if (distanceTraveledText != null)
            distanceTraveledText.text = "Distancia: " + Mathf.RoundToInt(s.DistanceTraveled) + " m";
        if (timeSurvivedText != null)
        {
            int min = (int)(s.TimeAlive / 60);
            int sec = (int)(s.TimeAlive % 60);
            timeSurvivedText.text = string.Format("Tiempo total: {0:D2}:{1:D2}", min, sec);
        }
    }

    // ── Continuar al siguiente nivel (NO resetea stats: se acumulan) ───────────
    private void OnContinue()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.buttonClickSFX);
        SceneManager.LoadScene(nextScene);
    }

    // ── Volver al menu (el New Game posterior reseteara las stats) ─────────────
    private void OnMainMenu()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.buttonClickSFX);
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnQuit()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.buttonClickSFX);
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
