using UnityEngine;

public class GameStats : MonoBehaviour
{
    public static GameStats Instance { get; private set; }

    // ── Estadísticas ────────────────────────────────────────────────────────
    public int   EnemiesKilled    { get; private set; }
    public float DistanceTraveled { get; private set; }
    public float TimeAlive        { get; private set; }
    public int   CoinsCollected   { get; private set; }
    public int   Score            { get; private set; }

    // ── Puntos por acción ────────────────────────────────────────────────────
    [Header("Puntos")]
    [SerializeField] private int pointsPerKill        = 100;
    [SerializeField] private int pointsPerCoin        = 10;
    [SerializeField] private int pointsPerCollectible = 50;

    // ── Tracking ─────────────────────────────────────────────────────────────
    private bool      isTracking;
    private Transform playerTransform;
    private float     lastPlayerX;

    // ── Singleton ────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (!isTracking || playerTransform == null) return;

        TimeAlive += Time.deltaTime;

        float currentX = playerTransform.position.x;
        float delta    = currentX - lastPlayerX;
        if (delta > 0) DistanceTraveled += delta;
        lastPlayerX = currentX;
    }

    // ── Control de tracking ──────────────────────────────────────────────────
    public void StartTracking(Transform player)
    {
        playerTransform = player;
        lastPlayerX     = player.position.x;
        isTracking      = true;
    }

    public void StopTracking() => isTracking = false;

    // ── Acciones que suman puntos ─────────────────────────────────────────────
    public void AddKill()
    {
        EnemiesKilled++;
        Score += pointsPerKill;
    }

    public void AddCoin()
    {
        CoinsCollected++;
        Score += pointsPerCoin;
    }

    public void AddCollectible()
    {
        Score += pointsPerCollectible;
    }

    /// <summary>Suma puntos arbitrarios (para ítems especiales, etc.)</summary>
    public void AddScore(int points)
    {
        Score += Mathf.Max(0, points);
    }

    // ── Reset ────────────────────────────────────────────────────────────────
    public void ResetStats()
    {
        EnemiesKilled    = 0;
        DistanceTraveled = 0f;
        TimeAlive        = 0f;
        CoinsCollected   = 0;
        Score            = 0;
        isTracking       = false;
        playerTransform  = null;
    }
}
