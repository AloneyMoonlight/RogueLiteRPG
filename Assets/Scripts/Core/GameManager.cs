using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, Paused, GameOver, Win }
    public GameState CurrentState { get; private set; }

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string gameScene = "Game";
    [SerializeField] private string gameOverScene = "GameOver";
    [SerializeField] private string winScene = "Win";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        CurrentState = GameState.Playing;
    }

    // ── Escucha cada carga de escena ──────────────────────────────────────────
    void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == gameScene)
        {
            // Siempre que se cargue la escena Game (primera vez, Play Again, etc.)
            // reseteamos el estado para que la pausa y el input funcionen bien.
            CurrentState   = GameState.Playing;
            Time.timeScale = 1f;

            // La música del juego la forzamos aquí como respaldo: aunque
            // GameInitializer falle o no exista, la música siempre arranca.
            AudioManager.Instance?.PlayMusic(AudioManager.Instance.gameMusic);
        }
    }

    void Update()
    {
        if (CurrentState == GameState.Playing && Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void TogglePause()
    {
        if (CurrentState == GameState.Paused) ResumeGame();
        else if (CurrentState == GameState.Playing) PauseGame();
    }

    public void PauseGame()
    {
        CurrentState = GameState.Paused;
        Time.timeScale = 0f;
        UIManager.Instance?.ShowPauseMenu(true);
    }

    public void ResumeGame()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        UIManager.Instance?.ShowPauseMenu(false);
    }

    public void GameOver()
    {
        if (CurrentState == GameState.GameOver) return;
        CurrentState = GameState.GameOver;
        GameStats.Instance?.StopTracking();
        StartCoroutine(LoadSceneRealtime(gameOverScene, 1.5f));
    }

    public void Win()
    {
        if (CurrentState == GameState.Win) return;
        CurrentState = GameState.Win;
        GameStats.Instance?.StopTracking();
        StartCoroutine(LoadSceneRealtime(winScene, 1f));
    }

    public void StartGame()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        GameStats.Instance?.ResetStats();
        SceneManager.LoadScene(gameScene);
    }

    public void RestartGame()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        GameStats.Instance?.ResetStats();
        SceneManager.LoadScene(gameScene);
    }

    public void GoToMainMenu()
    {
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private System.Collections.IEnumerator LoadSceneRealtime(string sceneName, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        SceneManager.LoadScene(sceneName);
    }
}
