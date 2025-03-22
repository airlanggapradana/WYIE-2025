using UnityEngine;

public class ClimbableSurface : MonoBehaviour
{
    [Header("Climbing Settings")]
    [SerializeField] private bool isClimbable = true;
    [SerializeField] private float climbSpeed = 3f;
    [SerializeField] private float grabOffset = 0.2f;
    [SerializeField] private float grabTransitionTime = 0.2f;
    
    [Header("Visual Feedback")]
    [SerializeField] private Color gizmoColor = new Color(0.2f, 0.8f, 0.2f, 0.5f);
    
    private void OnDrawGizmos()
    {
        if (!isClimbable) return;
        
        // Draw a visual indicator for climbable surfaces
        Gizmos.color = gizmoColor;
        Vector3 size = GetComponent<Collider2D>()?.bounds.size ?? Vector3.one;
        Gizmos.DrawCube(transform.position, size);
    }
    
    public bool IsClimbable => isClimbable;
    public float ClimbSpeed => climbSpeed;
    public float GrabOffset => grabOffset;
    public float GrabTransitionTime => grabTransitionTime;
} 