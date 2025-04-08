using UnityEngine;
using System.Collections;

public class FallDetector : MonoBehaviour
{
    [Header("Fall Detection Settings")]
    [SerializeField] private bool detectByYPosition = true;
    [SerializeField] private float minimumYPosition = -10f;

    [SerializeField] private bool detectByFallTime = false;
    [SerializeField] private float maxFallTime = 3f;

    [Header("Fall Damage Settings")]
    [SerializeField] private float fallDamage = 10f;
    [SerializeField] private bool instantKill = false;
    [SerializeField] private bool respawnWithoutDamage = false;

    // References
    private Rigidbody rb;
    private Rigidbody2D rb2D;
    private HealthSystem healthSystem;

    // Fall tracking
    private bool isFalling = false;
    private float fallTimer = 0f;
    private float verticalVelocity = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb2D = GetComponent<Rigidbody2D>();
        healthSystem = GetComponent<HealthSystem>();
    }

    private void Update()
    {
        // Position-based detection
        if (detectByYPosition && transform.position.y < minimumYPosition)
        {
            HandleFall();
            return;
        }

        // Fall time detection
        if (detectByFallTime)
        {
            CheckFallState();

            if (isFalling)
            {
                fallTimer += Time.deltaTime;

                // If falling for too long
                if (fallTimer > maxFallTime)
                {
                    HandleFall();
                }
            }
        }
    }

    private void CheckFallState()
    {
        // Check if player is falling
        if (rb != null)
        {
            verticalVelocity = rb.linearVelocity.y;
        }
        else if (rb2D != null)
        {
            verticalVelocity = rb2D.linearVelocity.y;
        }

        // If falling and not previously falling, start fall timer
        if (verticalVelocity < -0.1f && !isFalling)
        {
            isFalling = true;
            fallTimer = 0f;
        }

        // If not falling anymore, reset
        if (verticalVelocity >= -0.1f && isFalling)
        {
            isFalling = false;
            fallTimer = 0f;
        }
    }

    private void HandleFall()
    {
        Debug.Log("Player fall detected");

        // Apply fall damage if needed
        if (healthSystem != null && !respawnWithoutDamage)
        {
            if (instantKill)
            {
                healthSystem.TakeDamage(healthSystem.GetMaxHealth() * 2);
            }
            else
            {
                healthSystem.TakeDamage(fallDamage);
            }
        }

        // If player isn't dead from the fall damage,
        // or if we're not applying damage, respawn immediately
        if (respawnWithoutDamage || (healthSystem != null && !healthSystem.IsDead()))
        {
            // Respawn player at last checkpoint
            if (CheckpointManager.Instance != null)
            {
                // Reset player
                RespawnAtCheckpoint();
            }
        }

        // Reset fall tracking
        isFalling = false;
        fallTimer = 0f;
    }

    /// <summary>
    /// Respawn the player at the last checkpoint
    /// </summary>
    private void RespawnAtCheckpoint()
    {
        if (CheckpointManager.Instance == null) return;

        // Get respawn position
        Vector3 respawnPosition = CheckpointManager.Instance.GetRespawnPosition();

        // Reset player position
        transform.position = respawnPosition;

        // Reset velocity
        if (rb != null) rb.linearVelocity = Vector3.zero;
        if (rb2D != null) rb2D.linearVelocity = Vector2.zero;

        Debug.Log("Repositioned player to checkpoint after fall");
    }
}