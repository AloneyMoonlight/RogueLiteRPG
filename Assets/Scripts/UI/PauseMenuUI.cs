using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Nombre de escena")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [SerializeField] private Button resumeButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitButton;

    void Start()
    {
        if (resumeButton != null)   resumeButton.onClick.AddListener(OnResume);
        if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenu);
        if (quitButton != null)     quitButton.onClick.AddListener(OnQuit);
    }

    private void OnResume()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.buttonClickSFX);
        Time.timeScale = 1f;
        // Intentamos usar GameManager si existe, si no desactivamos el panel directamente
        if (GameManager.Instance != null)
            GameManager.Instance.ResumeGame();
        else
            gameObject.SetActive(false);
    }

    private void OnMainMenu()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.buttonClickSFX);
        Time.timeScale = 1f;
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
