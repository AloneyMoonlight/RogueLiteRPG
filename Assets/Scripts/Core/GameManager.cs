using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Playing, Paused, GameOver, Win }
    public GameState CurrentState { get; private set; }

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [Tooltip("Primera escena al pulsar Play (la aldea con el NPC)")]
    [SerializeField] private string firstScene    = "Aldea";
    [SerializeField] private string gameOverScene = "GameOver";
    [SerializeField] private string winScene      = "Win";

    [Header("Escenas jugables")]
    [Tooltip("Todas las escenas donde se juega (aldea + niveles). En ellas se " +
             "reinicia el estado a Playing y suena la musica de juego.")]
    [SerializeField] private string[] gameplayScenes = { "Aldea", "nivel_1", "nivel_2", "nivel_3" };

    // ── Estado de progresion entre niveles (persistente) ──────────────────────
    /// <summary>Escena que cargara el boton "Continuar" de la pantalla de victoria.</summary>
    public string NextSceneToLoad { get; private set; }
    /// <summary>Si es true, la pantalla de victoria muestra "Fin del juego".</summary>
    public bool   IsGameComplete  { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        transform.SetParent(null);        // DontDestroyOnLoad requiere objeto raiz
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
        if (IsGameplayScene(scene.name))
        {
            // Cualquier escena jugable (primera vez, siguiente nivel, retry...):
            // reseteamos el estado para que pausa e input funcionen bien.
            CurrentState   = GameState.Playing;
            Time.timeScale = 1f;

            // Musica de juego como respaldo robusto (aunque GameInitializer falle).
            AudioManager.Instance?.PlayMusic(AudioManager.Instance.gameMusic);
        }
    }

    private bool IsGameplayScene(string name)
    {
        foreach (string s in gameplayScenes)
            if (s == name) return true;
        return false;
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

    // ── Nivel completado (todos los enemigos muertos + coleccionables) ─────────
    /// <summary>
    /// Lo llama LevelManager cuando se limpia un mapa.
    /// nextScene = a donde ir tras la victoria; isFinal = ultimo mapa del juego.
    /// </summary>
    public void LevelComplete(string nextScene, bool isFinal)
    {
        if (CurrentState == GameState.Win) return;
        CurrentState    = GameState.Win;
        NextSceneToLoad = nextScene;
        IsGameComplete  = isFinal;
        GameStats.Instance?.StopTracking();
        StartCoroutine(LoadSceneRealtime(winScene, 1f));
    }

    // Compatibilidad con WinZone / codigo antiguo: victoria simple sin progresion
    public void Win()
    {
        LevelComplete(firstScene, true);
    }

    public void StartGame()
    {
        CurrentState   = GameState.Playing;
        Time.timeScale = 1f;
        IsGameComplete = false;
        GameStats.Instance?.ResetStats();
        SceneManager.LoadScene(firstScene);
    }

    public void GoToMainMenu()
    {
        CurrentState   = GameState.Playing;
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
