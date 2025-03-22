using UnityEngine;

public class StaticNPCBehavior : NPCBehavior
{
    [Header("Static NPC Settings")]
    [SerializeField] private bool facePlayer = true;
    [SerializeField] private float playerDetectionRadius = 3f;
    [SerializeField] private LayerMask playerLayer;
    
    private Transform player;
    
    protected override void Start()
    {
        base.Start();
        
        // Set behavior type to static
        behaviorType = NPCBehaviorType.Static;
        
        // Find player reference
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    protected override void Update()
    {
        base.Update();
        
        // If configured to face player and player is in range, face them
        if (facePlayer && player != null)
        {
            CheckAndFacePlayer();
        }
    }
    
    private void CheckAndFacePlayer()
    {
        // Calculate distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Only face player if they're within detection radius
        if (distanceToPlayer <= playerDetectionRadius)
        {
            // Calculate direction to player
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            
            // Set the facing direction but don't move
            if (Mathf.Abs(directionToPlayer.x) > Mathf.Abs(directionToPlayer.y))
            {
                // Facing left or right
                movementDirection = new Vector2(Mathf.Sign(directionToPlayer.x), 0);
            }
            else
            {
                // Facing up or down
                movementDirection = new Vector2(0, Mathf.Sign(directionToPlayer.y));
            }
            
            // Update animation states but don't actually move
            if (animator != null)
            {
                animator.SetFloat("Horizontal", movementDirection.x);
                animator.SetFloat("Vertical", movementDirection.y);
            }
        }
    }
    
    protected override void OnDrawGizmosSelected()
    {
        if (facePlayer)
        {
            // Draw player detection radius
            Gizmos.color = new Color(0.2f, 0.2f, 0.8f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);
        }
    }
} 