using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private bool controlsEnabled = true;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    public Rigidbody2D rb;
    Animator animator;
    private bool isGrounded;
    private float horizontalInput;
    private bool canJump = true;
    private float jumpCooldown = 0.2f;
    private float jumpCooldownTimer = 0f;

    // Platform sticking variables
    private Transform currentPlatform;
    private Vector3 lastPlatformPosition;
    private bool isOnMovingPlatform = false;

    // Player state
    private enum PlayerState { Normal }
    private PlayerState currentState = PlayerState.Normal;

    private DialogueManager dialogueManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Find dialogue manager
        dialogueManager = FindObjectOfType<DialogueManager>();

        // Create ground check if not assigned
        if (groundCheck == null)
        {
            GameObject check = new GameObject("GroundCheck");
            check.transform.parent = transform;
            check.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = check.transform;
            Debug.Log("Ground Check created. Position it at the character's feet.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        NormalStateUpdate();
    }

    void NormalStateUpdate()
    {
        // If controls are disabled, don't process input
        if (!controlsEnabled) return;

        // Get input from keyboard or mobile input system
        horizontalInput = GetHorizontalInput();

        // Check if player is grounded
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Handle jump cooldown
        if (!canJump)
        {
            jumpCooldownTimer -= Time.deltaTime;
            if (jumpCooldownTimer <= 0f && isGrounded)
            {
                canJump = true;
            }
        }

        // Platform sticking
        HandlePlatformSticking();

        // Only process jump when not in dialogue
        bool inDialogue = dialogueManager != null && dialogueManager.IsDialogueActive;

        // Jump input - only process if not in dialogue
        if (!inDialogue && GetJumpButtonDown() && isGrounded && canJump)
        {
            Jump();
            canJump = false;
            jumpCooldownTimer = jumpCooldown;

            // Detach from platform when jumping
            if (isOnMovingPlatform)
            {
                transform.parent = null;
                isOnMovingPlatform = false;
            }
        }
    }

    void FixedUpdate()
    {
        // Handle movement in FixedUpdate for consistent physics
        Move();
    }

    void Move()
    {
        // If controls are disabled, stop moving
        if (!controlsEnabled)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // Calculate velocity
        Vector2 velocity = rb.linearVelocity;
        velocity.x = horizontalInput * moveSpeed;
        animator.SetFloat("XVel", Mathf.Abs(rb.linearVelocityX));
        animator.SetFloat("YVel", rb.linearVelocity.y);

        // Apply velocity
        rb.linearVelocity = velocity;

        // Flip character based on direction (assuming sprite faces right by default)
        if (horizontalInput != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(horizontalInput), 1f, 1f);
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        animator.SetBool("IsJumping", !isGrounded);
    }

    void HandlePlatformSticking()
    {
        if (isGrounded)
        {
            // Check if we're standing on a moving platform
            RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, 0.1f, groundLayer);

            if (hit.collider != null && hit.collider.gameObject.GetComponent<MovingPlatform>() != null)
            {
                // We're on a moving platform
                if (!isOnMovingPlatform || currentPlatform != hit.transform)
                {
                    // First time on this platform or new platform
                    transform.parent = hit.transform;
                    currentPlatform = hit.transform;
                    isOnMovingPlatform = true;
                    lastPlatformPosition = currentPlatform.position;
                }
            }
            else if (isOnMovingPlatform)
            {
                // Left the platform
                transform.parent = null;
                isOnMovingPlatform = false;
                currentPlatform = null;
            }
        }

        // Update platform position if on a platform
        if (isOnMovingPlatform && currentPlatform != null)
        {
            lastPlatformPosition = currentPlatform.position;
        }
    }

    // Visualize the ground check in the editor
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    /// <summary>
    /// Enables or disables player controls
    /// </summary>
    /// <param name="enabled">Whether controls should be enabled</param>
    public void SetControlsEnabled(bool enabled)
    {
        controlsEnabled = enabled;

        // If disabling controls, reset input and stop horizontal movement
        if (!enabled)
        {
            horizontalInput = 0;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    // Public methods for mobile controls

    /// <summary>
    /// Move the player right (for mobile button)
    /// </summary>
    public void MoveRight()
    {
        Debug.Log($"MoveRight called - controlsEnabled: {controlsEnabled}, currentState: {currentState}");
        if (controlsEnabled && currentState == PlayerState.Normal)
        {
            Debug.Log("MoveRight executed successfully");
            horizontalInput = 1f;
            transform.localScale = new Vector3(1f, 1f, 1f);
            rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
        }
    }

    /// <summary>
    /// Move the player left (for mobile button)
    /// </summary>
    public void MoveLeft()
    {
        if (controlsEnabled && currentState == PlayerState.Normal)
        {
            horizontalInput = -1f;
            transform.localScale = new Vector3(-1f, 1f, 1f);
            rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);
        }
    }

    /// <summary>
    /// Stop horizontal movement (for mobile button)
    /// </summary>
    public void StopMoving()
    {
        horizontalInput = 0f;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    /// <summary>
    /// Make the player jump (for mobile button)
    /// </summary>
    public void MobileJump()
    {
        // Only process jump when not in dialogue and player is grounded
        bool inDialogue = dialogueManager != null && dialogueManager.IsDialogueActive;

        if (!inDialogue && isGrounded && canJump && controlsEnabled)
        {
            Jump();
            canJump = false;
            jumpCooldownTimer = jumpCooldown;

            // Detach from platform when jumping
            if (isOnMovingPlatform)
            {
                transform.parent = null;
                isOnMovingPlatform = false;
            }
        }
    }

    // Helper methods to get input (either from keyboard or mobile buttons)
    private float GetHorizontalInput()
    {
        // Get keyboard input first
        float keyboardInput = Input.GetAxisRaw("Horizontal");

        // If no keyboard input, check mobile input
        if (keyboardInput == 0)
        {
            return MobileControllerUI.CustomInput.HorizontalInput;
        }

        return keyboardInput;
    }

    private bool GetJumpButtonDown()
    {
        // Check keyboard first
        bool keyboardJump = Input.GetButtonDown("Jump");

        // Check mobile input
        bool mobileJump = MobileControllerUI.CustomInput.JumpButtonDown;

        // Return true if either input is detected
        return keyboardJump || mobileJump;
    }

    private bool GetJumpButton()
    {
        // Check keyboard first
        bool keyboardJump = Input.GetButton("Jump");

        // Check mobile input
        bool mobileJump = MobileControllerUI.CustomInput.JumpButton;

        // Return true if either input is detected
        return keyboardJump || mobileJump;
    }
}
