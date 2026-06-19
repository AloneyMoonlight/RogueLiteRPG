using UnityEngine;
using TMPro;

/// <summary>
/// NPC con dialogo. Coloca este script en el GameObject del NPC, que debe
/// tener un Collider2D en modo Trigger (la zona donde el jugador puede hablar).
///
/// Funcionamiento:
///   • El jugador entra en la zona  -> aparece la pista "Pulsa E"
///   • Pulsa E (interactKey)        -> muestra la 1a linea del dialogo
///   • Pulsa E de nuevo             -> avanza linea a linea
///   • Al terminar                  -> cierra el panel y HasInteracted = true
///
/// El borde de salida de la aldea (LevelExit) consulta HasInteracted para
/// dejar pasar al jugador solo despues de escuchar la historia.
///
/// ── Wiring en Inspector ──────────────────────────────────────────────────────
/// • Dialogue Panel : el GameObject del panel de dialogo (se activa/desactiva)
/// • Dialogue Text  : el TMP_Text donde se escribe cada linea
/// • Interact Hint  : (opcional) GameObject "Pulsa E" que flota sobre el NPC
/// • Lines          : las lineas de la historia (una por entrada)
/// </summary>
public class DialogueNPC : MonoBehaviour
{
    [Header("Interaccion")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    [Header("UI de dialogo")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text   dialogueText;
    [SerializeField] private GameObject interactHint;   // opcional "Pulsa E"

    [Header("Lineas del dialogo")]
    [TextArea(2, 4)]
    [SerializeField] private string[] lines = new string[]
    {
        "Oh, gracias a dios has llegado...",
        "¿Eres tu la aventurera que contrate, cierto? ¡Genial!",
        "Necesito que te encargues de todos los monstruos que han estado acechandonos estos ultimos dias.",
        "Gracias a ellos todos los habitantes del pueblo estan abarrotados dentro de sus hogares y tienen miedo a salir.",
        "Asi que... ¡Lo dejo en tus manos!"
    };

    [Header("Congelar al jugador durante el dialogo")]
    [SerializeField] private bool freezePlayer = true;

    /// <summary>El borde de salida consulta esto para dejar pasar al jugador.</summary>
    public bool HasInteracted { get; private set; }

    // ── Estado interno ──────────────────────────────────────────────────────────
    private bool           playerInRange;
    private bool           dialogueOpen;
    private int            lineIndex;
    private PlayerMovement playerMovement;

    void Start()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (interactHint  != null) interactHint.SetActive(false);
    }

    void Update()
    {
        if (!playerInRange) return;

        if (Input.GetKeyDown(interactKey))
        {
            if (!dialogueOpen) OpenDialogue();
            else               NextLine();
        }
    }

    // ── Apertura / avance ───────────────────────────────────────────────────────
    private void OpenDialogue()
    {
        dialogueOpen = true;
        lineIndex    = 0;

        if (interactHint  != null) interactHint.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(true);

        ShowCurrentLine();

        if (freezePlayer && playerMovement != null)
            playerMovement.enabled = false;
    }

    private void NextLine()
    {
        lineIndex++;
        if (lineIndex >= lines.Length)
            CloseDialogue();
        else
            ShowCurrentLine();
    }

    private void ShowCurrentLine()
    {
        if (dialogueText != null && lineIndex < lines.Length)
            dialogueText.text = lines[lineIndex];
    }

    private void CloseDialogue()
    {
        dialogueOpen  = false;
        HasInteracted = true;

        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (freezePlayer && playerMovement != null)
            playerMovement.enabled = true;

        // Tras hablar, vuelve a mostrar la pista si el jugador sigue cerca
        // (por si quiere releer; al estar HasInteracted ya puede salir igual).
    }

    // ── Deteccion del jugador ────────────────────────────────────────────────────
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange  = true;
        playerMovement = other.GetComponent<PlayerMovement>();
        if (interactHint != null && !dialogueOpen) interactHint.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        if (interactHint != null) interactHint.SetActive(false);
        if (dialogueOpen) CloseDialogue();
    }
}
