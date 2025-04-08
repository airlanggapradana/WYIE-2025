using UnityEngine;

public class LifePickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private int livesToAdd = 1;
    [SerializeField] private bool destroyOnPickup = true;
    [SerializeField] private GameObject pickupEffect;

    [Header("Animation")]
    [SerializeField] private float bobHeight = 0.5f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float rotationSpeed = 90f;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        // Simple bobbing animation
        float newY = startPos.y + (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Rotation animation
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CollectLife();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CollectLife();
        }
    }

    private void CollectLife()
    {
        // Add lives to player via checkpoint manager
        if (CheckpointManager.Instance != null)
        {
            for (int i = 0; i < livesToAdd; i++)
            {
                CheckpointManager.Instance.AddLife();
            }

            // Play pickup effect
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }

            // Destroy the pickup or deactivate it
            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("No CheckpointManager found for life pickup!");
        }
    }
}