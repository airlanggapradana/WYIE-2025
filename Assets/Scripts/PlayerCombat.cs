using UnityEngine;
using System.Collections.Generic;

public class PlayerCombat : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private KeyCode attackKey = KeyCode.F;
    [SerializeField] private KeyCode specialAttackKey = KeyCode.E;
    [SerializeField] private KeyCode useHealthItemKey = KeyCode.Q;

    [Header("Combat")]
    [SerializeField] private LayerMask enemyLayers; // Add this to detect enemies
    [SerializeField] private float attackRangeCheck = 2.0f; // Range to check for enemies
    [SerializeField] private bool bypassRangeCheck = false; // Add a bypass option for testing
    [SerializeField] private bool debugAttackRange = true; // Enable detailed debug logs

    [Header("Inventory")]
    [SerializeField] private int maxHealthItems = 5;
    [SerializeField] private float healthItemAmount = 20f;

    // Component references
    private HealthSystem healthSystem;
    private AttackSystem attackSystem;
    private PlayerMovement playerMovement;

    // Inventory tracking
    private int currentHealthItems = 0;

    private void Awake()
    {
        // Get required components
        healthSystem = GetComponent<HealthSystem>();
        attackSystem = GetComponent<AttackSystem>();
        playerMovement = GetComponent<PlayerMovement>();

        // Error checking
        if (healthSystem == null)
        {
            Debug.LogError("PlayerCombat requires a HealthSystem component!");
        }

        if (attackSystem == null)
        {
            Debug.LogError("PlayerCombat requires an AttackSystem component!");
        }
    }

    private void Start()
    {
        // Start with some health items
        currentHealthItems = Mathf.Max(2, maxHealthItems / 2);
    }

    private void Update()
    {
        // Only process input if not in dialogue and controls are enabled
        bool controlsEnabled = IsPlayerControlsEnabled();
        if (!controlsEnabled) return;

        // Check attack point position relative to player
        // if (attackSystem != null && debugAttackRange)
        // {
        //     Transform attackPoint = attackSystem.GetAttackPoint();
        //     if (attackPoint != null)
        //     {
        //         Debug.Log($"Player position: {transform.position}, Attack point position: {attackPoint.position}, Local offset: {attackPoint.localPosition}");
        //     }
        // }

        // Basic attack - check both keyboard and mobile input
        if ((Input.GetKeyDown(attackKey) || MobileControllerUI.CustomInput.AttackButtonDown) && attackSystem != null)
        {
            if (bypassRangeCheck || IsEnemyInAttackRange())
            {
                attackSystem.Attack();
            }
            else
            {
                Debug.Log("No enemy in attack range");
                // Optionally show a UI message to the player
            }
        }

        // Special attack - check both keyboard and mobile input
        if ((Input.GetKeyDown(specialAttackKey) || MobileControllerUI.CustomInput.SpecialAttackButtonDown) && attackSystem != null)
        {
            if (bypassRangeCheck || IsEnemyInAttackRange())
            {
                attackSystem.SpecialAttack();
            }
            else
            {
                Debug.Log("No enemy in attack range for special attack");
                // Optionally show a UI message to the player
            }
        }

        // Use health item - check both keyboard and mobile input
        if ((Input.GetKeyDown(useHealthItemKey) || MobileControllerUI.CustomInput.HealthItemButtonDown) && healthSystem != null)
        {
            UseHealthItem();
        }
    }

    /// <summary>
    /// Check if player controls are currently enabled
    /// </summary>
    private bool IsPlayerControlsEnabled()
    {
        // Check if dialogue is active
        DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
        if (dialogueManager != null && dialogueManager.IsDialogueActive)
        {
            return false;
        }

        // Check if player movement controls are enabled
        if (playerMovement != null)
        {
            // Use reflection to access private field (or you could expose this through a getter)
            System.Reflection.FieldInfo field = typeof(PlayerMovement).GetField("controlsEnabled",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                bool controlsEnabled = (bool)field.GetValue(playerMovement);
                return controlsEnabled;
            }
        }

        return true;
    }

    /// <summary>
    /// Check if there are any enemies within attack range
    /// </summary>
    private bool IsEnemyInAttackRange()
    {
        // Check if we have an attack system with a valid attack point
        Transform attackPoint = null;
        float range = attackRangeCheck;

        if (debugAttackRange)
        {
            Debug.Log("======= ATTACK RANGE CHECK =======");
            Debug.Log($"Default range: {range}");
            Debug.Log($"Enemy Layers mask: {enemyLayers.value}");
        }

        // Try to get attack range from the attack system if available
        if (attackSystem != null)
        {
            // First try to use the public getters (preferred method)
            attackPoint = attackSystem.GetAttackPoint();

            // Use the attack system's range if available
            range = attackSystem.GetAttackRange();

            if (debugAttackRange)
            {
                Debug.Log($"Got attack point: {(attackPoint != null ? "Success" : "NULL")}");
                Debug.Log($"Got attack range: {range}");
            }

            // Fallback to reflection only if the getters failed
            if (attackPoint == null)
            {
                if (debugAttackRange) Debug.Log("Getter failed, trying reflection as backup...");

                // Use reflection to get the private attackPoint field
                System.Reflection.FieldInfo attackPointField = typeof(AttackSystem).GetField("attackPoint",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (attackPointField != null)
                {
                    attackPoint = (Transform)attackPointField.GetValue(attackSystem);
                    if (debugAttackRange)
                    {
                        Debug.Log($"Got attack point from reflection: {(attackPoint != null ? "Success" : "NULL")}");
                    }
                }
                else if (debugAttackRange)
                {
                    Debug.Log("Failed to get attackPoint field via reflection");
                }
            }
        }

        // If we couldn't get attack point, use the player's position
        Vector2 checkPosition = (attackPoint != null) ? attackPoint.position : transform.position;

        if (debugAttackRange)
        {
            Debug.Log($"Check position: {checkPosition}, Range: {range}");
        }

        // Check for enemies in range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(checkPosition, range, enemyLayers);

        if (debugAttackRange)
        {
            Debug.Log($"Found {hitEnemies.Length} potential targets in range");

            // List all layers in the enemy mask for debugging
            string layerNames = "";
            for (int i = 0; i < 32; i++)
            {
                if ((enemyLayers.value & (1 << i)) != 0)
                {
                    layerNames += LayerMask.LayerToName(i) + ", ";
                }
            }
            Debug.Log($"Enemy layers in mask: {layerNames}");

            // List nearby colliders even if they aren't in the enemy layer
            Collider2D[] allNearbyColliders = Physics2D.OverlapCircleAll(checkPosition, range);
            Debug.Log($"Total colliders nearby (any layer): {allNearbyColliders.Length}");
            foreach (Collider2D col in allNearbyColliders)
            {
                Debug.Log($"Nearby object: {col.gameObject.name}, Layer: {LayerMask.LayerToName(col.gameObject.layer)} ({col.gameObject.layer})");
            }
        }

        foreach (Collider2D target in hitEnemies)
        {
            // Calculate distance to confirm we're in range
            float distanceToTarget = Vector2.Distance(attackPoint.position, target.transform.position);

            Debug.Log($"-----DISTANCE DEBUG-----");
            Debug.Log($"Attack Point Position: {attackPoint.position}");
            Debug.Log($"Target Position: {target.transform.position}");
            Debug.Log($"Target Name: {target.gameObject.name}");
            Debug.Log($"Raw Distance: {distanceToTarget}");
            Debug.Log($"Attack Range: {range}");
            Debug.Log($"------------------------");

            // Return true if at least one enemy is in range
            Debug.DrawLine(attackPoint.position, target.transform.position, Color.red, 1.0f);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Visualize the attack range for debugging
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Draw the attack range check circle
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRangeCheck);
    }

    /// <summary>
    /// Use a health item if available
    /// </summary>
    public void UseHealthItem()
    {
        if (currentHealthItems <= 0 || healthSystem == null)
        {
            Debug.Log("No health items available");
            return;
        }

        // Only use if health is not full
        if (healthSystem.GetCurrentHealth() < healthSystem.GetMaxHealth())
        {
            healthSystem.Heal(healthItemAmount);
            currentHealthItems--;

            Debug.Log($"Used health item. Remaining: {currentHealthItems}");
        }
        else
        {
            Debug.Log("Health is already full");
        }
    }

    /// <summary>
    /// Add health items to inventory
    /// </summary>
    public void AddHealthItems(int amount)
    {
        currentHealthItems = Mathf.Min(currentHealthItems + amount, maxHealthItems);
        Debug.Log($"Added {amount} health items. Total: {currentHealthItems}");
    }

    /// <summary>
    /// Get current health item count
    /// </summary>
    public int GetHealthItemCount()
    {
        return currentHealthItems;
    }

    /// <summary>
    /// OnGUI is used here for a simple HUD display
    /// In a real game, you'd want to use proper UI elements
    /// </summary>
    private void OnGUI()
    {
        // if (healthSystem == null) return;

        // // Create a basic HUD
        // GUI.Box(new Rect(10, 10, 200, 25), "Health: " + Mathf.RoundToInt(healthSystem.GetCurrentHealth()) +
        //                                     " / " + Mathf.RoundToInt(healthSystem.GetMaxHealth()));

        // GUI.Box(new Rect(10, 40, 200, 25), "Health Items: " + currentHealthItems +
        //                                     " (Press " + useHealthItemKey.ToString() + " to use)");
    }
}