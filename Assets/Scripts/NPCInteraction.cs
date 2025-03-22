using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    [Header("NPC Settings")]
    [SerializeField] private string npcName = "NPC";
    [SerializeField] private DialogueData dialogueData;
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private LayerMask playerLayer;
    
    [Header("UI References")]
    [SerializeField] private GameObject interactionPrompt;
    
    private bool playerInRange = false;
    private DialogueManager dialogueManager;
    private NPCBehavior npcBehavior;
    
    private void Awake()
    {
        // Get reference to the NPC behavior if present
        npcBehavior = GetComponent<NPCBehavior>();
    }
    
    private void Start()
    {
        // Get reference to dialogue manager
        dialogueManager = FindObjectOfType<DialogueManager>();
        
        if (dialogueManager == null)
        {
            Debug.LogError("No DialogueManager found in scene! Please add one.");
        }
        
        // Set interaction point to this transform if not assigned
        if (interactionPoint == null)
        {
            interactionPoint = transform;
        }
        
        // Hide interaction prompt initially
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }
    
    private void Update()
    {
        // Check if player is in range
        CheckPlayerInRange();
        
        // Show interaction prompt if player is in range
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(playerInRange);
        }
        
        // Handle player interaction
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            TriggerDialogue();
        }
    }
    
    private void CheckPlayerInRange()
    {
        // Check for player in interaction radius
        Collider2D playerCollider = Physics2D.OverlapCircle(
            interactionPoint.position,
            interactionRadius,
            playerLayer
        );
        
        playerInRange = playerCollider != null;
    }
    
    private void TriggerDialogue()
    {
        if (dialogueManager != null && dialogueData != null)
        {
            // Optional: Make NPC face the player before starting dialogue
            if (npcBehavior != null)
            {
                // The NPCBehavior's PauseMovement method will be called via event
                // when dialogue starts, so we don't need to call it directly here
            }
            
            dialogueManager.StartDialogue(dialogueData);
        }
        else
        {
            Debug.LogWarning("Cannot start dialogue: DialogueManager or DialogueData is missing.");
        }
    }
    
    public string GetNPCName()
    {
        return npcName;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (interactionPoint == null)
        {
            interactionPoint = transform;
        }
        
        // Draw interaction radius
        Gizmos.color = playerInRange ? Color.green : Color.yellow;
        Gizmos.DrawWireSphere(interactionPoint.position, interactionRadius);
    }
} 