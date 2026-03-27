using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    // Variable to hold the target (player) that the camera will follow
    public Transform Player;
    public Vector3 offset; // Offset from the player position
    public float smoothSpeed = 0.125f; // Speed of the camera movement
    
    void start()
    {
        // Initialize the offset based on the initial position of the camera and the player
        offset = transform.position - Player.position;
    }
    void LateUpdate()
    {
        // Desired position of the camera based on the player's position and the offset
        Vector3 desiredPosition = Player.position + offset;
        // Smoothly interpolate between the current position and the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        // Update the camera's position to the smoothed position
        transform.position = smoothedPosition;
    }
}
