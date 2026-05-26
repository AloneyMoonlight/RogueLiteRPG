using UnityEngine;

// Place this in the Game scene. It wires up persistent systems on scene load.
public class GameInitializer : MonoBehaviour
{
    void Start()
    {
        Time.timeScale = 1f;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            GameStats.Instance?.StartTracking(player.transform);

        if (AudioManager.Instance != null && AudioManager.Instance.gameMusic != null)
            AudioManager.Instance.PlayMusic(AudioManager.Instance.gameMusic);
    }
}
