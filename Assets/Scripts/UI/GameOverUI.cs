using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverUI : MonoBehaviour
{
    [Header("Nombre de escenas")]
    [SerializeField] private string gameSceneName     = "Game";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Estadisticas (arrastra TextMeshPro)")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text enemiesKilledText;
    [SerializeField] private TMP_Text distanceTraveledText;
    [SerializeField] private TMP_Text timeSurvivedText;

    [Header("Botones")]
    [SerializeField] private Button retryButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    void Start()
    {
        Time.timeScale = 1f;

        if (retryButton != null)    retryButton.onClick.AddListener(OnRetry);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);
        if (quitButton != null)     quitButton.onClick.AddListener(OnQuit);

        DisplayStats();

        if (AudioManager.Instance != null && AudioManager.Instance.gameOverMusic != null)
            AudioManager.Instance.PlayMusic(AudioManager.Instance.gameOverMusic);
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
            timeSurvivedText.text = string.Format("Tiempo: {0:D2}:{1:D2}", min, sec);
        }
    }

    private void OnRetry()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.buttonClickSFX);
        GameStats.Instance?.ResetStats();
        SceneManager.LoadScene(gameSceneName);
    }
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
