using UnityEngine;

public class DeathZone : MonoBehaviour
{
    [Header("Fall Settings")]
    [SerializeField] private float fallDamage = 10f;
    [SerializeField] private bool instantKill = false;
    [SerializeField] private bool resetPositionOnly = false;

    [Header("Effects")]
    [SerializeField] private GameObject fallEffect;

    private void OnTriggerEnter(Collider other)
    {
        HandleFall(other.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleFall(other.gameObject);
    }

    private void HandleFall(GameObject entity)
    {
        // Only process player falls
        if (entity.CompareTag("Player"))
        {
            Debug.Log("Player fell into death zone");

            // Spawn effect if available
            if (fallEffect != null)
            {
                Instantiate(fallEffect, entity.transform.position, Quaternion.identity);
            }

            // Get health system
            HealthSystem healthSystem = entity.GetComponent<HealthSystem>();

            if (healthSystem != null)
            {
                if (instantKill)
                {
                    // Apply lethal damage
                    healthSystem.TakeDamage(healthSystem.GetMaxHealth() * 2);
                }
                else if (!resetPositionOnly)
                {
                    // Apply configured fall damage
                    healthSystem.TakeDamage(fallDamage);
                }
            }

            // If the player isn't dead from the fall damage,
            // or if we're just resetting position, respawn immediately
            if (resetPositionOnly || (healthSystem != null && !healthSystem.IsDead()))
            {
                // Respawn player at last checkpoint
                if (CheckpointManager.Instance != null)
                {
                    entity.transform.position = CheckpointManager.Instance.GetRespawnPosition();

                    // Reset velocity
                    Rigidbody rb = entity.GetComponent<Rigidbody>();
                    if (rb != null) rb.linearVelocity = Vector3.zero;

                    Rigidbody2D rb2d = entity.GetComponent<Rigidbody2D>();
                    if (rb2d != null) rb2d.linearVelocity = Vector2.zero;

                    Debug.Log("Repositioned player to checkpoint without death");
                }
            }
        }
        else if (entity.CompareTag("Enemy") || entity.CompareTag("Boss"))
        {
            // Optionally handle enemies falling
            HealthSystem enemyHealth = entity.GetComponent<HealthSystem>();
            if (enemyHealth != null && instantKill)
            {
                enemyHealth.TakeDamage(enemyHealth.GetMaxHealth() * 2);
            }
        }
    }
}