using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Borde de salida de un mapa que NO usa puntuacion (la aldea).
/// Coloca este script en un GameObject con Collider2D en modo Trigger al final
/// del mapa. Cuando el jugador entra, carga directamente la siguiente escena.
///
/// Si se asigna "Required NPC", el borde solo funciona despues de que el jugador
/// haya hablado con ese NPC (su HasInteracted == true). Mientras no haya hablado,
/// puede mostrarse una pista opcional.
///
/// ── Wiring en Inspector ──────────────────────────────────────────────────────
/// • Next Scene Name : escena a cargar (ej. "nivel_1")
/// • Required NPC     : (opcional) el DialogueNPC con el que hay que hablar antes
/// • Locked Hint      : (opcional) cartel "Habla con el aldeano primero"
/// </summary>
public class LevelExit : MonoBehaviour
{
    [Header("Destino")]
    [SerializeField] private string nextSceneName = "nivel_1";

    [Header("Requisito (opcional)")]
    [Tooltip("Si se asigna, hay que hablar con este NPC antes de poder pasar")]
    [SerializeField] private DialogueNPC requiredNPC;
    [SerializeField] private GameObject  lockedHint;

    private bool playerInside;

    void Start()
    {
        if (lockedHint != null) lockedHint.SetActive(false);
    }

    void Update()
    {
        // Si el jugador se queda en el borde esperando, en cuanto cumpla el
        // requisito (acabe de hablar con el NPC) pasa automaticamente.
        if (playerInside && CanExit())
            LoadNext();
    }

    private bool CanExit()
    {
        return requiredNPC == null || requiredNPC.HasInteracted;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;

        if (CanExit())
            LoadNext();
        else if (lockedHint != null)
            lockedHint.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        if (lockedHint != null) lockedHint.SetActive(false);
    }

    private void LoadNext()
    {
        if (string.IsNullOrEmpty(nextSceneName)) return;
        SceneManager.LoadScene(nextSceneName);
    }
}
