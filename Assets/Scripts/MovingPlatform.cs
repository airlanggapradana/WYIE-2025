using UnityEngine;
using System.Collections.Generic;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private List<Transform> waypoints = new List<Transform>();
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waitTime = 0.5f;
    [SerializeField] private bool cyclic = true;
    [SerializeField] private bool startAtRandomWaypoint = false;
    
    private int currentWaypointIndex = 0;
    private float waitCounter = 0f;
    private bool isWaiting = false;
    private bool movingForward = true;
    
    private void Start()
    {
        if (waypoints.Count == 0)
        {
            Debug.LogError("No waypoints assigned to moving platform: " + gameObject.name);
            enabled = false;
            return;
        }
        
        // Optionally start at a random waypoint
        if (startAtRandomWaypoint && waypoints.Count > 1)
        {
            currentWaypointIndex = Random.Range(0, waypoints.Count);
            transform.position = waypoints[currentWaypointIndex].position;
        }
        else
        {
            // Start at first waypoint
            transform.position = waypoints[0].position;
        }
    }
    
    private void Update()
    {
        if (waypoints.Count < 2)
            return;
            
        // Handle waiting at waypoints
        if (isWaiting)
        {
            waitCounter -= Time.deltaTime;
            if (waitCounter <= 0f)
            {
                isWaiting = false;
            }
            return;
        }
        
        // Calculate direction to the next waypoint
        Vector3 targetPosition = waypoints[currentWaypointIndex].position;
        Vector3 moveDirection = (targetPosition - transform.position).normalized;
        
        // Move towards the waypoint
        transform.position = Vector3.MoveTowards(
            transform.position, 
            targetPosition, 
            moveSpeed * Time.deltaTime
        );
        
        // Check if we've reached the waypoint
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            // Start waiting
            waitCounter = waitTime;
            isWaiting = true;
            
            // Calculate next waypoint
            if (cyclic)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
            }
            else
            {
                // Ping-pong between waypoints
                if (currentWaypointIndex >= waypoints.Count - 1)
                {
                    movingForward = false;
                }
                else if (currentWaypointIndex <= 0)
                {
                    movingForward = true;
                }
                
                currentWaypointIndex += movingForward ? 1 : -1;
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        if (waypoints.Count < 2)
            return;
            
        // Draw platform path in editor
        Gizmos.color = Color.yellow;
        
        for (int i = 0; i < waypoints.Count; i++)
        {
            if (waypoints[i] != null)
            {
                // Draw waypoint
                Gizmos.DrawSphere(waypoints[i].position, 0.2f);
                
                // Draw line to next waypoint
                if (i < waypoints.Count - 1 && waypoints[i+1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i+1].position);
                }
            }
        }
        
        // If cyclic, connect last and first waypoints
        if (cyclic && waypoints.Count > 1 && waypoints[0] != null && waypoints[waypoints.Count-1] != null)
        {
            Gizmos.DrawLine(waypoints[waypoints.Count-1].position, waypoints[0].position);
        }
    }
} 