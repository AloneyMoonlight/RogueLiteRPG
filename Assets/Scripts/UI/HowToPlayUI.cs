using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Pantalla "Como jugar". Solo gestiona el boton de volver al menu.
/// El contenido (imagenes de los controles) lo colocas tu en la escena.
///
/// ── Wiring en Inspector ──────────────────────────────────────────────────────
/// • Back Button : boton para volver al menu principal
/// </summary>
public class HowToPlayUI : MonoBehaviour
{
    [Header("Escena")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("UI")]
    [SerializeField] private Button backButton;

    void Start()
    {
        Time.timeScale = 1f;
        if (backButton != null) backButton.onClick.AddListener(OnBack);
    }

    private void OnBack()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.buttonClickSFX);
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
