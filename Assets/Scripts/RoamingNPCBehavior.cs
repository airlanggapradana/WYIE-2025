using UnityEngine;
using System.Collections;

public class RoamingNPCBehavior : NPCBehavior
{
    [Header("Roaming Settings")]
    [SerializeField] private Vector2 roamingAreaSize = new Vector2(5f, 5f);
    [SerializeField] private Vector2 roamingAreaOffset = Vector2.zero;
    [SerializeField] private float waitTimeAtEdge = 2f;
    [SerializeField] private LayerMask obstacleLayer;
    
    private Vector3 startPosition;
    private Bounds roamingBounds;
    private Coroutine movementCoroutine;
    
    // States for the NPC
    private enum NPCState
    {
        MovingToEdge,
        WaitingAtEdge,
        Blocked
    }
    
    private NPCState currentState = NPCState.WaitingAtEdge;
    private Vector2 currentTarget;
    
    protected override void Start()
    {
        base.Start();
        
        // Set behavior type to roaming
        behaviorType = NPCBehaviorType.Roaming;
        
        // Store the starting position
        startPosition = transform.position;
        
        // Define roaming area
        Vector3 boundsCenter = startPosition + new Vector3(roamingAreaOffset.x, roamingAreaOffset.y, 0);
        roamingBounds = new Bounds(boundsCenter, new Vector3(roamingAreaSize.x, roamingAreaSize.y, 0));
        
        // Start the movement
        StartMovementPattern();
    }
    
    private void StartMovementPattern()
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }
        
        movementCoroutine = StartCoroutine(EdgeToEdgeMovement());
    }
    
    private IEnumerator EdgeToEdgeMovement()
    {
        while (true)
        {
            // If we can't move (e.g., during dialogue), just wait
            if (!canMove)
            {
                yield return null;
                continue;
            }
            
            switch (currentState)
            {
                case NPCState.WaitingAtEdge:
                    // Wait at the current edge
                    yield return new WaitForSeconds(waitTimeAtEdge);
                    
                    // Choose a new edge to move to
                    currentTarget = GetRandomEdgePosition();
                    Debug.Log($"{gameObject.name} moving to new edge position: {currentTarget}");
                    
                    // Start moving to the new edge
                    currentState = NPCState.MovingToEdge;
                    break;
                    
                case NPCState.MovingToEdge:
                    // Move towards the target edge
                    if (MoveTowardsTarget())
                    {
                        // We've reached the target
                        currentState = NPCState.WaitingAtEdge;
                    }
                    break;
                    
                case NPCState.Blocked:
                    // Try to find a new path
                    currentTarget = GetRandomEdgePosition();
                    Debug.Log($"{gameObject.name} was blocked, trying new target: {currentTarget}");
                    currentState = NPCState.MovingToEdge;
                    yield return new WaitForSeconds(1f); // Brief wait before trying again
                    break;
            }
            
            yield return null;
        }
    }
    
    private bool MoveTowardsTarget()
    {
        // Calculate direction to target, but zero out the y component to keep movement horizontal
        Vector2 directionToTarget = (currentTarget - (Vector2)transform.position).normalized;
        directionToTarget.y = 0; // Ensure movement is only horizontal
        
        if (directionToTarget.magnitude < 0.01f)
        {
            // If we're already aligned horizontally, just move directly left or right
            directionToTarget = new Vector2(currentTarget.x > transform.position.x ? 1 : -1, 0);
        }
        
        // Check for obstacles
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToTarget, 0.5f, obstacleLayer);
        if (hit.collider != null)
        {
            // If blocked, try to find a way around
            Debug.Log($"{gameObject.name} hit obstacle: {hit.collider.name}");
            currentState = NPCState.Blocked;
            isMoving = false;
            
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            
            return false;
        }
        
        // Set movement direction (horizontal only)
        movementDirection = directionToTarget;
        isMoving = true;
        
        // Check if we've reached the target (with some tolerance) - check only X position
        float distanceToTargetX = Mathf.Abs(transform.position.x - currentTarget.x);
        if (distanceToTargetX < 0.1f)
        {
            // We've reached the target
            isMoving = false;
            movementDirection = Vector2.zero;
            
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            
            return true;
        }
        
        return false;
    }
    
    private Vector2 GetRandomEdgePosition()
    {
        // Choose between left and right edge only (this is a 2D side-scrolling game)
        int edge = Random.Range(0, 2); // 0 = left, 1 = right
        Vector2 result = Vector2.zero;
        
        // Use current Y position to keep NPC on the same horizontal plane
        float currentY = transform.position.y;
        
        switch (edge)
        {
            case 0: // Left edge
                result = new Vector2(
                    roamingBounds.min.x,
                    currentY
                );
                break;
                
            case 1: // Right edge
                result = new Vector2(
                    roamingBounds.max.x,
                    currentY
                );
                break;
        }
        
        return result;
    }
    
    public override void PauseMovement()
    {
        base.PauseMovement();
        
        // Stop the current movement but don't stop the coroutine
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
    
    public override void ResumeMovement()
    {
        base.ResumeMovement();
        
        // When resuming movement, start from waiting state to choose a new edge
        currentState = NPCState.WaitingAtEdge;
    }
    
    protected override void OnDrawGizmosSelected()
    {
        // Draw the roaming area in the editor
        Vector3 boundsCenter = Application.isPlaying 
            ? roamingBounds.center 
            : transform.position + new Vector3(roamingAreaOffset.x, roamingAreaOffset.y, 0);
        
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.3f);
        Gizmos.DrawCube(boundsCenter, new Vector3(roamingAreaSize.x, roamingAreaSize.y, 0.1f));
        
        Gizmos.color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
        Gizmos.DrawWireCube(boundsCenter, new Vector3(roamingAreaSize.x, roamingAreaSize.y, 0.1f));
        
        // Draw current target if in play mode
        if (Application.isPlaying && currentState == NPCState.MovingToEdge)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(currentTarget, 0.2f);
            Gizmos.DrawLine(transform.position, currentTarget);
        }
    }
} 