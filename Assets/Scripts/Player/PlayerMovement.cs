using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Variables for movement
    public float moveSpeed = 5f; // Speed of the player movement
    private Rigidbody2D rb; // Reference to the Rigidbody2D component for physics-based movement
    private float move; // Variable to store horizontal input

        
    private bool isGrounded; // Reference to the IsGrounded component to check if the player is on the ground
    public Transform groundCheck; // Transform used to check if the player is grounded
    public float groundCheckRadius = 0.1f; // Radius for checking if the player is grounded
    public LayerMask groundLayer; // Layer mask to specify what is considered ground
    public float jumpForce = 10f; // Force applied when the player jumps

    private Animator anim; // Reference to the Animator component for handling animations

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Get the Rigidbody2D component attached to the player
        anim = GetComponent<Animator>(); // Get the Animator component attached to the player
    }

    // Update is called once per frame
    private bool hasAirJump; // Track if the player has used their air jump

    void Update()
    {
        move = Input.GetAxisRaw("Horizontal"); // Get horizontal input (A/D or Left/Right arrow keys)
        rb.linearVelocity = new Vector2(move * moveSpeed, rb.linearVelocity.y); // Set the horizontal velocity based on input and maintain the current vertical velocity
        
        // Flip the player's sprite based on the direction of movement
        if (move != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(move), 1, 1); // Flip the player's sprite based on the direction of movement
        }

        // Check for jump input
        if (Input.GetButtonDown("Jump"))
        {
            // If the player is grounded, allow them to jump. If they are in the air and have an air jump available, allow them to jump again.
            if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); // Apply vertical force for ground jump
                hasAirJump = true; // Allow one air jump
            }
            else if (hasAirJump)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); // Apply vertical force for air jump
                hasAirJump = false; // Consume the air jump
            }
        }
        // Update the animator parameters based on movement and grounded state
        anim.SetFloat("Speed", Mathf.Abs(move)); // Set the "Speed" parameter to the absolute value of horizontal movement for running animation
        anim.SetBool("isGrounded", isGrounded); // Set the "isGrounded" parameter to true or false based on whether the player is on the ground for jumping/falling animations
        anim.SetFloat("VerticalVelocity", rb.linearVelocity.y); // Set the "VerticalVelocity" parameter to the current vertical velocity for more accurate jumping/falling animations
    }

    // FixedUpdate is called at a fixed interval and is used for physics updates
    void FixedUpdate()
    {
        // Check if the player is grounded by checking for collisions with the ground layer
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }
}
