using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Header("Nombres de escenas")]
    [Tooltip("Primera escena al pulsar Play: la aldea con el NPC y la historia")]
    [SerializeField] private string gameSceneName = "Aldea";
    [SerializeField] private string creditsSceneName = "Creditos";

    [Header("Paneles")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Botones principales")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;

    [Header("Botones de settings")]
    [SerializeField] private Button backButton;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    void Start()
    {
        Time.timeScale = 1f;

        // --- Botones ---
        if (playButton != null)     playButton.onClick.AddListener(OnPlay);
        if (settingsButton != null) settingsButton.onClick.AddListener(OnSettings);
        if (creditsButton != null)  creditsButton.onClick.AddListener(OnCredits);
        if (quitButton != null)     quitButton.onClick.AddListener(OnQuit);
        if (backButton != null)     backButton.onClick.AddListener(OnBack);

        // --- Sliders de volumen ---
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = 0.5f;
            musicVolumeSlider.onValueChanged.AddListener(v => AudioManager.Instance?.SetMusicVolume(v));
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = 1f;
            sfxVolumeSlider.onValueChanged.AddListener(v => AudioManager.Instance?.SetSFXVolume(v));
        }

        // --- Estado inicial de paneles ---
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainPanel != null)     mainPanel.SetActive(true);

        // --- Música del menú ---
        if (AudioManager.Instance != null && AudioManager.Instance.menuMusic != null)
            AudioManager.Instance.PlayMusic(AudioManager.Instance.menuMusic);
    }

    // ── Botón Play ──────────────────────────────────────────────────────────
    private void OnPlay()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.buttonClickSFX);

        // Reseteamos stats antes de cargar la partida
        GameStats.Instance?.ResetStats();

        // Cargamos directamente; no depende de que GameManager exista
        SceneManager.LoadScene(gameSceneName);
    }

    // ── Botón Settings ──────────────────────────────────────────────────────
    private void OnSettings()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.buttonClickSFX);
        if (mainPanel != null)     mainPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    // ── Botón Créditos ────────────────────────────────────────────────────────
    private void OnCredits()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.buttonClickSFX);
        SceneManager.LoadScene(creditsSceneName);
    }

    // ── Botón Back (desde Settings) ─────────────────────────────────────────
    private void OnBack()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.buttonClickSFX);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainPanel != null)     mainPanel.SetActive(true);
    }

    // ── Botón Quit ──────────────────────────────────────────────────────────
    private void OnQuit()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.buttonClickSFX);
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
