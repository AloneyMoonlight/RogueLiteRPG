using UnityEngine;

/// <summary>
/// Trigger manual de victoria: al tocarlo el jugador, muestra la pantalla de
/// Victoria. Alternativa a LevelManager cuando prefieres ganar al LLEGAR a un
/// punto (meta/bandera) en vez de limpiar todo el mapa.
///
/// Coloca el script en un GameObject con Collider2D en modo Trigger.
///   • Next Scene Name : siguiente nivel tras la victoria
///   • Is Final Level  : marca el ultimo mapa -> "Fin del juego"
/// </summary>
public class WinZone : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "nivel_2";
    [SerializeField] private bool   isFinalLevel  = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            GameManager.Instance?.LevelComplete(nextSceneName, isFinalLevel);
    }
}
