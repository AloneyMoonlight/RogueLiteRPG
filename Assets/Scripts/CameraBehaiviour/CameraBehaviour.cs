using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    // Variable to hold the target (player) that the camera will follow
    [SerializeField] private Transform Player;
    [SerializeField] private Vector3 offset; // Offset from the player position
    [SerializeField] private float smoothSpeed = 0.125f; // Speed of the camera movement

    private Vector3 velocity = Vector3.zero; // For SmoothDamp

    void Start()
    {
        // Initialize the offset based on the initial position of the camera and the player
        offset = transform.position - Player.position;
    }

    void LateUpdate()
    {
        // Desired position of the camera based on the player's position and the offset
        Vector3 desiredPosition = Player.position + offset;
        // Smoothly interpolate between the current position and the desired position using SmoothDamp for better smoothing
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);
        // Update the camera's position to the smoothed position
        transform.position = smoothedPosition;
    }
}
