using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [SerializeField] private bool isActive = false;
    [SerializeField] private int checkpointID = 0;
    [SerializeField] private GameObject activationEffect;

    [Header("Debug")]
    [SerializeField] private bool drawGizmo = true;
    [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0f, 0.3f);
    [SerializeField] private Vector3 gizmoSize = new Vector3(1f, 2f, 1f);

    // Trigger when player enters the checkpoint
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isActive)
        {
            ActivateCheckpoint();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isActive)
        {
            ActivateCheckpoint();
        }
    }

    // Activate checkpoint and register with CheckpointManager
    private void ActivateCheckpoint()
    {
        // Check if CheckpointManager exists before activating
        if (CheckpointManager.Instance != null)
        {
            // Activate this checkpoint and deactivate others
            isActive = true;
            CheckpointManager.Instance.SetActiveCheckpoint(this);

            // Spawn activation effect if available
            if (activationEffect != null)
            {
                Instantiate(activationEffect, transform.position, Quaternion.identity);
            }

            Debug.Log($"Checkpoint {checkpointID} activated");
        }
        else
        {
            Debug.LogWarning("No CheckpointManager found in scene!");
        }
    }

    public void SetActive(bool active)
    {
        isActive = active;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public int GetID()
    {
        return checkpointID;
    }

    // Draw checkpoint in scene view for easier level design
    private void OnDrawGizmos()
    {
        if (!drawGizmo) return;

        Gizmos.color = isActive ? Color.green : gizmoColor;
        Gizmos.DrawCube(transform.position, gizmoSize);

        // Draw ID text
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, $"Checkpoint {checkpointID}");
    }
}