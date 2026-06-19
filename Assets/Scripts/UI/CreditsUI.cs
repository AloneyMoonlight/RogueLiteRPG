using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Pantalla de creditos. Muestra el texto de creditos y un boton para volver
/// al menu principal. El texto se rellena por codigo (editable en el Inspector).
///
/// ── Wiring en Inspector ──────────────────────────────────────────────────────
/// • Credits Text : el TMP_Text donde se muestran los creditos
/// • Back Button  : boton para volver al menu
/// </summary>
public class CreditsUI : MonoBehaviour
{
    [Header("Escena")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("UI")]
    [SerializeField] private TMP_Text creditsText;
    [SerializeField] private Button   backButton;

    [Header("Contenido")]
    [TextArea(8, 20)]
    [SerializeField] private string credits =
        "<b>CREDITOS</b>\n\n" +
        "<b>Musica</b>\n" +
        "Wuthering Waves\n\n" +
        "<b>Assets</b>\n" +
        "Village Props - Cainos\n" +
        "cainos.itch.io/pixel-art-platformer-village-props\n\n" +
        "Warrior Animation Set - Clembod\n" +
        "clembod.itch.io/warrior-free-animation-set\n\n" +
        "UI - Caz Pixel Keyboard - Cazwolf\n" +
        "cazwolf.itch.io/caz-pixel-keyboard\n\n" +
        "Imagenes generadas con IA (ChatGPT)\n\n" +
        "<b>Creado por</b>\n" +
        "Yehicofs Araya  -  Joaquin Carvajal  -  Alejandro Espinosa";

    void Start()
    {
        Time.timeScale = 1f;

        if (creditsText != null) creditsText.text = credits;
        if (backButton  != null) backButton.onClick.AddListener(OnBack);
    }

    private void OnBack()
    {
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.buttonClickSFX);
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
