using UnityEngine;

public class HealthItem : MonoBehaviour
{
    public enum ItemType
    {
        SmallHeal,
        MediumHeal,
        LargeHeal,
        DefenseBuff,
        MaxHealthIncrease
    }
    
    [Header("Item Settings")]
    [SerializeField] private ItemType itemType = ItemType.SmallHeal;
    [SerializeField] private float effectValue = 20f; // Health restored or buff amount
    [SerializeField] private float buffDuration = 10f; // For temporary buffs (in seconds)
    
    [Header("Visual Settings")]
    [SerializeField] private GameObject pickupEffectPrefab;
    [SerializeField] private float rotationSpeed = 50f;
    [SerializeField] private float bobSpeed = 1f;
    [SerializeField] private float bobHeight = 0.2f;
    
    [Header("Behavior")]
    [SerializeField] private bool destroyOnPickup = true;
    
    private Vector3 startPosition;
    private float bobTimer = 0f;
    
    private void Start()
    {
        startPosition = transform.position;
    }
    
    private void Update()
    {
        // Rotate the item
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        // Bob the item up and down
        bobTimer += Time.deltaTime * bobSpeed;
        transform.position = startPosition + Vector3.up * Mathf.Sin(bobTimer) * bobHeight;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to the player
        if (other.CompareTag("Player"))
        {
            // Get the player's health system
            HealthSystem playerHealth = other.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                // Apply the health item effect
                ApplyEffect(playerHealth);
                
                // Play pickup effect
                if (pickupEffectPrefab != null)
                {
                    Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
                }
                
                // Destroy the item if set to do so
                if (destroyOnPickup)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
    
    // Also support 2D colliders with OnTriggerEnter2D
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collider belongs to the player
        if (other.CompareTag("Player"))
        {
            // Get the player's health system
            HealthSystem playerHealth = other.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                // Apply the health item effect
                ApplyEffect(playerHealth);
                
                // Play pickup effect
                if (pickupEffectPrefab != null)
                {
                    Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
                }
                
                // Destroy the item if set to do so
                if (destroyOnPickup)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
    
    /// <summary>
    /// Apply the item's effect to the target's health system
    /// </summary>
    private void ApplyEffect(HealthSystem targetHealth)
    {
        switch (itemType)
        {
            case ItemType.SmallHeal:
            case ItemType.MediumHeal:
            case ItemType.LargeHeal:
                // Apply healing
                targetHealth.Heal(effectValue);
                Debug.Log($"Player healed for {effectValue} health");
                break;
                
            case ItemType.DefenseBuff:
                // Apply defense buff
                targetHealth.ApplyDefenseBuff(effectValue, buffDuration);
                Debug.Log($"Player received defense buff of {effectValue} for {buffDuration} seconds");
                break;
                
            case ItemType.MaxHealthIncrease:
                // This would require a new method in HealthSystem to increase max health
                Debug.Log("Max health increase not implemented yet");
                break;
        }
    }
} 