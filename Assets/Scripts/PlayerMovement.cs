using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private bool controlsEnabled = true;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Ledge Grab Settings")]
    [SerializeField] private Transform ledgeCheck;
    [SerializeField] private Vector2 ledgeCheckSize = new Vector2(0.5f, 0.1f);
    [SerializeField] private float ledgeGrabOffset = 0.2f;
    [SerializeField] private float ledgePullUpForce = 8f;
    [SerializeField] private float ledgeGrabDuration = 0.5f;
    
    [Header("Climbing Settings")]
    [SerializeField] private float wallCheckDistance = 0.2f;
    [SerializeField] private float climbInputHoldTime = 0.2f;
    [SerializeField] private float climbTransitionSpeed = 5f;
    
    public Rigidbody2D rb;
    Animator animator;
    private bool isGrounded;
    private float horizontalInput;
    private bool canJump = true;
    private float jumpCooldown = 0.2f;
    private float jumpCooldownTimer = 0f;
    
    // Ledge grab variables
    private bool isGrabbingLedge = false;
    private bool canGrabLedge = true;
    private Vector2 ledgePosition;
    private float ledgeGrabTimer = 0f;
    
    // Platform sticking variables
    private Transform currentPlatform;
    private Vector3 lastPlatformPosition;
    private bool isOnMovingPlatform = false;
    
    // Climbing variables
    private bool isNearClimbableWall = false;
    private bool isClimbing = false;
    private ClimbableSurface currentClimbableSurface;
    private float climbInputHoldTimer = 0f;
    private float climbTransitionTimer = 0f;
    private Vector3 targetClimbPosition;
    private bool isTransitioningToClimb = false;
    
    // Player state
    private enum PlayerState { Normal, LedgeGrab, PullingUp, Climbing, ClimbTransition }
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
        
        // Create ledge check if not assigned
        if (ledgeCheck == null)
        {
            GameObject check = new GameObject("LedgeCheck");
            check.transform.parent = transform;
            check.transform.localPosition = new Vector3(0.5f, 0.5f, 0);
            ledgeCheck = check.transform;
            Debug.Log("Ledge Check created. Position it at the character's reach point.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case PlayerState.Normal:
                NormalStateUpdate();
                break;
            case PlayerState.LedgeGrab:
                LedgeGrabStateUpdate();
                break;
            case PlayerState.PullingUp:
                PullingUpStateUpdate();
                break;
            case PlayerState.Climbing:
                ClimbingStateUpdate();
                break;
            case PlayerState.ClimbTransition:
                ClimbTransitionUpdate();
                break;
        }
    }
    
    void NormalStateUpdate()
    {
        // If controls are disabled, don't process input
        if (!controlsEnabled) return;
        
        // Get input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        
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
        
        // Check for climbable walls
        CheckForClimbableWalls();
        
        // Handle climbing input
        if (isNearClimbableWall && Input.GetButton("Jump"))
        {
            climbInputHoldTimer += Time.deltaTime;
            if (climbInputHoldTimer >= climbInputHoldTime && !isClimbing)
            {
                StartClimbing();
            }
        }
        else
        {
            climbInputHoldTimer = 0f;
        }
        
        // Only process jump when not in dialogue
        bool inDialogue = dialogueManager != null && dialogueManager.IsDialogueActive;
        
        // Jump input - only process if not in dialogue
        if (!inDialogue && Input.GetButtonDown("Jump") && isGrounded && canJump)
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
        
        // Check for ledge grab if falling, pressing jump, and not grounded
        // Only process if not in dialogue
        if (!inDialogue && !isGrounded && rb.linearVelocity.y < 0 && Input.GetButton("Jump") && canGrabLedge)
        {
            CheckForLedge();
        }
    }
    
    void CheckForClimbableWalls()
    {
        // Check both left and right sides for climbable surfaces
        Vector2[] checkDirections = new Vector2[] { Vector2.right, Vector2.left };
        bool foundClimbable = false;
        
        foreach (Vector2 direction in checkDirections)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, wallCheckDistance, groundLayer);
            if (hit.collider != null)
            {
                ClimbableSurface climbable = hit.collider.GetComponent<ClimbableSurface>();
                if (climbable != null && climbable.IsClimbable)
                {
                    isNearClimbableWall = true;
                    currentClimbableSurface = climbable;
                    foundClimbable = true;
                    break;
                }
            }
        }
        
        if (!foundClimbable)
        {
            isNearClimbableWall = false;
            currentClimbableSurface = null;
        }
    }
    
    void StartClimbing()
    {
        if (currentClimbableSurface == null) return;
        
        // Calculate target position for climbing
        float direction = Mathf.Sign(transform.localScale.x);
        targetClimbPosition = transform.position + new Vector3(
            currentClimbableSurface.GrabOffset * direction,
            0,
            0
        );
        
        // Start transition to climbing
        currentState = PlayerState.ClimbTransition;
        climbTransitionTimer = currentClimbableSurface.GrabTransitionTime;
        isTransitioningToClimb = true;
    }
    
    void ClimbTransitionUpdate()
    {
        if (isTransitioningToClimb)
        {
            // Smoothly move to climbing position
            transform.position = Vector3.Lerp(
                transform.position,
                targetClimbPosition,
                climbTransitionSpeed * Time.deltaTime
            );
            
            climbTransitionTimer -= Time.deltaTime;
            if (climbTransitionTimer <= 0)
            {
                isTransitioningToClimb = false;
                currentState = PlayerState.Climbing;
                isClimbing = true;
                rb.gravityScale = 0;
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
    
    void ClimbingStateUpdate()
    {
        // Check if dialogue is active
        bool inDialogue = dialogueManager != null && dialogueManager.IsDialogueActive;
        
        if (currentClimbableSurface == null)
        {
            ExitClimbing();
            return;
        }
        
        // Handle vertical movement while climbing (only if not in dialogue)
        float verticalInput = inDialogue ? 0 : Input.GetAxisRaw("Vertical");
        rb.linearVelocity = new Vector2(0, verticalInput * currentClimbableSurface.ClimbSpeed);
        
        // Exit climbing if player jumps or moves away from wall (only if not in dialogue)
        if (!inDialogue && (Input.GetButtonDown("Jump") || !isNearClimbableWall))
        {
            ExitClimbing();
        }
    }
    
    void ExitClimbing()
    {
        currentState = PlayerState.Normal;
        isClimbing = false;
        rb.gravityScale = 1;
        rb.linearVelocity = Vector2.zero;
        currentClimbableSurface = null;
        isNearClimbableWall = false;
    }
    
    void LedgeGrabStateUpdate()
    {
        // Check if dialogue is active
        bool inDialogue = dialogueManager != null && dialogueManager.IsDialogueActive;
        
        // Disable horizontal movement and gravity while grabbing ledge
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;
        
        // While grabbing ledge, position the player correctly
        transform.position = new Vector3(ledgePosition.x - ledgeGrabOffset * transform.localScale.x, 
                                        ledgePosition.y - ledgeGrabOffset, 
                                        transform.position.z);
        
        // Press jump to pull up (only if not in dialogue)
        if (!inDialogue && Input.GetButtonDown("Jump"))
        {
            StartPullUp();
        }
        
        // Let go if player presses down (only if not in dialogue)
        if (!inDialogue && Input.GetAxisRaw("Vertical") < -0.5f)
        {
            ExitLedgeGrab();
        }
    }
    
    void PullingUpStateUpdate()
    {
        ledgeGrabTimer -= Time.deltaTime;
        
        if (ledgeGrabTimer <= 0)
        {
            // Finish pull up
            currentState = PlayerState.Normal;
            rb.gravityScale = 1;
            rb.linearVelocity = Vector2.zero;
            transform.position = new Vector3(ledgePosition.x, ledgePosition.y + 0.5f, transform.position.z);
        }
    }
    
    void FixedUpdate()
    {
        // Handle movement in FixedUpdate for consistent physics
        if (currentState == PlayerState.Normal)
        {
            Move();
        }
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
    
    void CheckForLedge()
    {
        // Adjust raycast direction based on player's facing direction
        Vector2 direction = new Vector2(transform.localScale.x, 0);
        Vector2 startPosition = ledgeCheck.position;
        
        // Cast a box to detect ledges
        RaycastHit2D hit = Physics2D.BoxCast(startPosition, ledgeCheckSize, 0f, direction, 0.1f, groundLayer);
        
        if (hit.collider != null)
        {
            // We found a ledge!
            ledgePosition = hit.point;
            StartLedgeGrab();
        }
    }
    
    void StartLedgeGrab()
    {
        isGrabbingLedge = true;
        canGrabLedge = false;
        currentState = PlayerState.LedgeGrab;
    }
    
    void ExitLedgeGrab()
    {
        isGrabbingLedge = false;
        currentState = PlayerState.Normal;
        rb.gravityScale = 1;
        
        // Add a small cooldown before allowing to grab again
        Invoke("ResetLedgeGrab", 0.5f);
    }
    
    void ResetLedgeGrab()
    {
        canGrabLedge = true;
    }
    
    void StartPullUp()
    {
        currentState = PlayerState.PullingUp;
        ledgeGrabTimer = ledgeGrabDuration;
        rb.linearVelocity = new Vector2(0, ledgePullUpForce);
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
        
        if (ledgeCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(ledgeCheck.position, ledgeCheckSize);
        }
        
        // Draw wall check rays
        Gizmos.color = isNearClimbableWall ? Color.green : Color.yellow;
        Gizmos.DrawRay(transform.position, Vector2.right * wallCheckDistance);
        Gizmos.DrawRay(transform.position, Vector2.left * wallCheckDistance);
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
}
