using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Paneles")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("Vida")]
    [SerializeField] private HealthUI healthUI;

    [Header("Puntuacion (solo muestra el numero)")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text killsText;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        ShowPauseMenu(false);
        UpdateScore(0);
        UpdateKills(0);
    }

    void Update()
    {
        if (GameStats.Instance != null)
        {
            UpdateScore(GameStats.Instance.Score);
            UpdateKills(GameStats.Instance.EnemiesKilled);
        }
    }

    public void ShowPauseMenu(bool show)
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(show);
        if (hudPanel != null)       hudPanel.SetActive(!show);
    }

    public void UpdateHealth(int current, int max) => healthUI?.UpdateHealth(current, max);

    // Solo el número, el texto estático lo tienes en un Text separado en la escena
    public void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = score.ToString();
    }

    public void UpdateKills(int kills)
    {
        if (killsText != null) killsText.text = kills.ToString();
    }
}
