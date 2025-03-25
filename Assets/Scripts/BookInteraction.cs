using UnityEngine;

public class BookInteraction : MonoBehaviour
{
    [Header("Book Settings")]
    [SerializeField] private BookData bookData;
    [SerializeField] private string bookName = "Book";
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private KeyCode interactionKey = KeyCode.Return; // Enter key
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private LayerMask playerLayer;
    
    [Header("UI References")]
    [SerializeField] private GameObject interactionPrompt;
    
    private bool playerInRange = false;
    private BookManager bookManager;
    
    private void Start()
    {
        // Get reference to book manager
        bookManager = FindObjectOfType<BookManager>();
        
        if (bookManager == null)
        {
            Debug.LogError("No BookManager found in scene! Please add one.");
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
            // Only show the prompt if not already reading a book
            bool canShow = playerInRange && (bookManager == null || !bookManager.IsBookOpen);
            interactionPrompt.SetActive(canShow);
            
            if (canShow && interactionPrompt.GetComponent<InteractionPrompt>() != null)
            {
                interactionPrompt.GetComponent<InteractionPrompt>().SetPromptText($"Press [Enter] to Read {bookName}");
                interactionPrompt.GetComponent<InteractionPrompt>().SetTarget(transform);
            }
        }
        
        // Handle player interaction
        if (playerInRange && Input.GetKeyDown(interactionKey) && (bookManager == null || !bookManager.IsBookOpen))
        {
            OpenBook();
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
    
    private void OpenBook()
    {
        if (bookManager != null && bookData != null)
        {
            bookManager.OpenBook(bookData);
        }
        else
        {
            Debug.LogWarning("Cannot open book: BookManager or BookData is missing.");
        }
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