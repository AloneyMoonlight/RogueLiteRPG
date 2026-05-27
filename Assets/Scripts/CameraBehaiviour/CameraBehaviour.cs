using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    [Header("Seguimiento")]
    [SerializeField] private Transform player;
    [SerializeField] private float followSmoothTime = 0.12f;   // qué tan rápido sigue al jugador

    [Header("Lookahead (camara que mira hacia donde vas)")]
    [Tooltip("Cuántas unidades se desplaza la cámara en la dirección que mira el jugador")]
    [SerializeField] private float lookAheadDistance = 1.5f;
    [Tooltip("Tiempo de suavizado del lookahead. Mayor = más lento y suave (recomendado 0.4-0.8)")]
    [SerializeField] private float lookAheadSmoothTime = 0.6f;
    [Tooltip("Velocidad mínima del jugador para activar el lookahead. Evita que la cámara reaccione a micro-movimientos")]
    [SerializeField] private float lookAheadVelocityThreshold = 1.5f;

    [Header("Camera Bounds (límites del nivel)")]
    [SerializeField] private bool  useBounds = false;
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 200f;
    [SerializeField] private float minY = -5f;
    [SerializeField] private float maxY = 30f;

    // ── Estado interno ────────────────────────────────────────────────────────
    private Vector3 followVelocity  = Vector3.zero;
    private float   lookAheadCurrent  = 0f;
    private float   lookAheadVelocity = 0f;
    private Vector3 offset;

    private Rigidbody2D playerRb;

    void Start()
    {
        if (player == null) return;

        offset   = transform.position - player.position;
        playerRb = player.GetComponent<Rigidbody2D>();

        // NOTA: el zoom (orthographic size) se controla directamente en el
        // componente Camera del Inspector. NO lo toques con Scale.
        // Valores típicos: 5 (lejano) → 4 (normal) → 3 (cercano) → 2 (muy cerca)
    }

    void LateUpdate()
    {
        if (player == null) return;

        // ── Dirección del jugador ─────────────────────────────────────────────
        // Solo cambia el objetivo del lookahead cuando el jugador se mueve
        // con suficiente velocidad. Así la cámara no reacciona a micro-giros
        // al aterrizar, cambiar dirección rápido o estar casi parado.
        float facingDir = player.localScale.x; // 1 = derecha, -1 = izquierda
        if (playerRb != null && Mathf.Abs(playerRb.linearVelocity.x) >= lookAheadVelocityThreshold)
            facingDir = Mathf.Sign(playerRb.linearVelocity.x);

        // ── Lookahead suavizado ───────────────────────────────────────────────
        float targetLookAhead = facingDir * lookAheadDistance;
        lookAheadCurrent = Mathf.SmoothDamp(
            lookAheadCurrent,
            targetLookAhead,
            ref lookAheadVelocity,
            lookAheadSmoothTime
        );

        // ── Posición objetivo ─────────────────────────────────────────────────
        Vector3 desired = player.position + offset + new Vector3(lookAheadCurrent, 0f, 0f);

        if (useBounds)
        {
            desired.x = Mathf.Clamp(desired.x, minX, maxX);
            desired.y = Mathf.Clamp(desired.y, minY, maxY);
        }

        // ── Smooth follow ─────────────────────────────────────────────────────
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desired,
            ref followVelocity,
            followSmoothTime
        );
    }
}
