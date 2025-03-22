using UnityEngine;

public enum NPCBehaviorType
{
    Static,
    Roaming
}

public class NPCBehavior : MonoBehaviour
{
    [Header("Behavior Settings")]
    [SerializeField] protected NPCBehaviorType behaviorType = NPCBehaviorType.Static;
    [SerializeField] protected float moveSpeed = 1.5f;
    
    [Header("Components")]
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected Animator animator;
    [SerializeField] protected NPCInteraction npcInteraction;
    
    protected bool canMove = true;
    protected Vector2 movementDirection;
    protected bool isMoving = false;
    
    protected virtual void Awake()
    {
        // Get required components if not assigned
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        if (npcInteraction == null) npcInteraction = GetComponent<NPCInteraction>();
        
        // Make sure rigidbody doesn't have constraints that would prevent proper 2D movement
        if (rb != null)
        {
            // Check for constraints that would restrict Y movement
            if (rb.constraints.HasFlag(RigidbodyConstraints2D.FreezePositionY))
            {
                Debug.LogWarning($"NPC {gameObject.name} has Y-position constraint on Rigidbody2D, which will prevent vertical movement!");
                
                // Optionally, remove the constraint - uncomment if needed
                // rb.constraints &= ~RigidbodyConstraints2D.FreezePositionY;
            }
        }
    }
    
    protected virtual void Start()
    {
        // Subscribe to dialogue events to pause movement during conversations
        DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueStarted += PauseMovement;
            dialogueManager.OnDialogueEnded += ResumeMovement;
        }
    }
    
    protected virtual void OnDestroy()
    {
        // Unsubscribe from dialogue events
        DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager != null)
        {
            dialogueManager.OnDialogueStarted -= PauseMovement;
            dialogueManager.OnDialogueEnded -= ResumeMovement;
        }
    }
    
    protected virtual void Update()
    {
        // Handle animation states
        UpdateAnimations();
    }
    
    protected virtual void FixedUpdate()
    {
        // Handle movement in FixedUpdate for physics consistency
        if (canMove && behaviorType == NPCBehaviorType.Roaming && isMoving)
        {
            Move();
        }
    }
    
    protected virtual void Move()
    {
        // Base movement logic - override in child classes
        if (rb != null)
        {
            // Use velocity instead of linearVelocity to ensure proper 2D movement
            rb.linearVelocity = movementDirection * moveSpeed;
            
            // Debug movement direction
            Debug.DrawRay(transform.position, movementDirection * 0.5f, Color.red);
        }
    }
    
    protected virtual void UpdateAnimations()
    {
        // Base animation updates - override in child classes
        if (animator != null)
        {
            // Set animator parameters based on movement
            animator.SetBool("IsMoving", isMoving);
            
            // Set direction parameters if moving
            if (isMoving && movementDirection.magnitude > 0)
            {
                animator.SetFloat("Horizontal", movementDirection.x);
                animator.SetFloat("Vertical", movementDirection.y);
            }
        }
    }
    
    public virtual void PauseMovement()
    {
        canMove = false;
        isMoving = false;
        
        // Stop current movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
    
    public virtual void ResumeMovement()
    {
        canMove = true;
    }
    
    protected virtual void OnDrawGizmosSelected()
    {
        // Override in child classes to show movement areas
    }
} 