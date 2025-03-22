using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using TMPro;
public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private float defense = 0f;
    
    [Header("Combat Settings")]
    [SerializeField] private float criticalHitChance = 0.1f; // 10% chance
    [SerializeField] private float criticalHitMultiplier = 2f; // Double damage on critical
    
    [Header("Regeneration Settings")]
    [SerializeField] private bool enableNaturalRegeneration = false;
    [SerializeField] private float regenerationRate = 5f; // Health per second
    [SerializeField] private float regenerationDelay = 5f; // Seconds after damage before regeneration starts
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private Transform damageTextSpawnPoint;
    [SerializeField] private GameObject healthBar;
    
    [Header("Events")]
    public UnityEvent OnDamaged;
    public UnityEvent OnHealed;
    public UnityEvent OnDeath;
    
    private bool isRegenerating = false;
    private float lastDamageTime;
    private Coroutine regenerationCoroutine;
    private bool isDead = false;
    
    // Component references
    private Animator animator;
    private Transform healthSystemPanelTransform; // Add cached reference
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    
    private void Start()
    {
        // Initialize health to max at start
        currentHealth = maxHealth;
        lastDamageTime = -regenerationDelay; // Allow immediate regeneration if enabled
        
        // Cache HealthSystemPanel reference
        GameObject healthSystemPanel = GameObject.Find("HealthSystemPanel");
        if (healthSystemPanel != null)
        {
            healthSystemPanelTransform = healthSystemPanel.transform;
        }
        
        Debug.Log("start: " + currentHealth);
        // Start regeneration if enabled
        if (enableNaturalRegeneration)
        {
            regenerationCoroutine = StartCoroutine(RegenerateHealth());
        }
        
        // Ensure damage text spawn point exists
        if (damageTextSpawnPoint == null)
        {
            damageTextSpawnPoint = transform;
        }
        
        UpdateHealthBar();
    }
    
    /// <summary>
    /// Applies damage to this character
    /// </summary>
    /// <param name="baseDamage">The base damage amount before modifiers</param>
    /// <param name="ignoreDefense">Whether to ignore defense calculation</param>
    /// <param name="guaranteedCritical">Force a critical hit</param>
    /// <returns>The actual damage dealt</returns>
    public float TakeDamage(float baseDamage, bool ignoreDefense = false, bool guaranteedCritical = false)
    {
        if (isDead) return 0f;
        
        // Calculate if this is a critical hit
        bool isCritical = guaranteedCritical || Random.value <= criticalHitChance;
        float damageMultiplier = isCritical ? criticalHitMultiplier : 1f;
        
        // Apply defense reduction if not ignored
        float effectiveDamage = baseDamage * damageMultiplier;
        if (!ignoreDefense)
        {
            effectiveDamage = Mathf.Max(1f, effectiveDamage - defense);
        }
        
        // Apply damage
        currentHealth = Mathf.Max(0f, currentHealth - effectiveDamage);
        lastDamageTime = Time.time;
        
        // Display damage text
        if (damageTextPrefab != null)
        {
            ShowDamageText(effectiveDamage, isCritical);
        }
        Debug.Log("CurrentHealth: " + currentHealth);
        Debug.Log("get damage text: " + damageTextPrefab);
        Debug.Log("get damage text spawn point: " + damageTextSpawnPoint);
        // Update health bar
        UpdateHealthBar();
        
        // Trigger damage event
        OnDamaged?.Invoke();
        
        // Apply animation effects
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
        
        // Check for death
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
        
        return effectiveDamage;
    }
    
    /// <summary>
    /// Heals the character by the specified amount
    /// </summary>
    /// <param name="healAmount">Amount to heal</param>
    /// <returns>The actual amount healed</returns>
    public float Heal(float healAmount)
    {
        if (isDead) return 0f;
        
        float previousHealth = currentHealth;
        currentHealth = Mathf.Min(maxHealth, currentHealth + healAmount);
        float actualHealAmount = currentHealth - previousHealth;
        
        // Show heal text with positive number
        if (damageTextPrefab != null && actualHealAmount > 0)
        {
            ShowHealText(actualHealAmount);
        }
        
        // Update health bar
        UpdateHealthBar();
        
        // Trigger heal event
        if (actualHealAmount > 0)
        {
            OnHealed?.Invoke();
        }
        
        return actualHealAmount;
    }
    
    /// <summary>
    /// Use a healing item to recover health
    /// </summary>
    /// <param name="healAmount">Amount to heal</param>
    public void UseHealingItem(float healAmount)
    {
        Heal(healAmount);
    }
    
    /// <summary>
    /// Apply a temporary defense buff
    /// </summary>
    /// <param name="buffAmount">Amount to increase defense</param>
    /// <param name="duration">Duration of the buff in seconds</param>
    public void ApplyDefenseBuff(float buffAmount, float duration)
    {
        StartCoroutine(DefenseBuffCoroutine(buffAmount, duration));
    }
    
    private IEnumerator DefenseBuffCoroutine(float buffAmount, float duration)
    {
        defense += buffAmount;
        yield return new WaitForSeconds(duration);
        defense -= buffAmount;
    }
    
    private void Die()
    {
        isDead = true;
        
        // Stop regeneration
        if (regenerationCoroutine != null)
        {
            StopCoroutine(regenerationCoroutine);
        }
        
        // Play death animation
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }
        
        // Trigger death event
        OnDeath?.Invoke();
        
        // Determine game flow based on who died
        if (CompareTag("Player"))
        {
            // Player died - trigger game over
            Debug.Log("Player died - Game Over");
            
            // Add delay before game over screen
            StartCoroutine(DelayedGameOver());
        }
        else if (CompareTag("Boss"))
        {
            // Boss died - player wins
            Debug.Log("Boss defeated - Victory!");
            Debug.Log("Object: " + gameObject + " currentHealth: " + currentHealth);
            
            // Add victory logic
            StartCoroutine(DelayedVictory());
        }
    }
    
    private IEnumerator DelayedGameOver()
    {
        // Wait for death animation
        yield return new WaitForSeconds(2f);
        
        // Trigger game over screen using GameManager
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.GameOver();
        }
        else
        {
            Debug.LogWarning("No GameManager found. Cannot trigger Game Over screen.");
        }
    }
    
    private IEnumerator DelayedVictory()
    {
        // Wait for death animation
        yield return new WaitForSeconds(2f);
        
        // Trigger victory sequence using GameManager
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            gameManager.Victory();
        }
        else
        {
            Debug.LogWarning("No GameManager found. Cannot trigger Victory screen.");
        }
    }
    
    private IEnumerator RegenerateHealth()
    {
        while (true)
        {
            // Wait until enough time has passed since last damage
            yield return new WaitUntil(() => Time.time >= lastDamageTime + regenerationDelay);
            
            // Only regenerate if not at max health
            if (currentHealth < maxHealth)
            {
                Heal(regenerationRate * Time.deltaTime);
            }
            
            yield return null;
        }
    }
    
    private void ShowDamageText(float damage, bool isCritical)
    {
        if (damageTextPrefab != null && damageTextSpawnPoint != null)
        {
            // Instantiate text prefab
            GameObject damageTextObj = Instantiate(damageTextPrefab, damageTextSpawnPoint.position, Quaternion.identity);
            
            // Use cached reference instead of GameObject.Find at runtime
            if (healthSystemPanelTransform != null)
            {
                damageTextObj.transform.SetParent(healthSystemPanelTransform, false);
            }
            else
            {
                // Try to find it once if we don't have it yet
                GameObject healthSystemPanel = GameObject.Find("HealthSystemPanel");
                if (healthSystemPanel != null)
                {
                    healthSystemPanelTransform = healthSystemPanel.transform;
                    damageTextObj.transform.SetParent(healthSystemPanelTransform, false);
                }
                else
                {
                    Debug.LogWarning("HealthSystemPanel not found for damage text.");
                    // Leave it unparented or set world canvas as parent
                    damageTextObj.transform.SetParent(null);
                }
            }

            // Get text component and set value
            TextMeshProUGUI damageText = damageTextObj.GetComponentInChildren<TextMeshProUGUI>();

            if (damageText != null)
            {
                // Format with color based on critical
                string text = Mathf.RoundToInt(damage).ToString();
                if (isCritical)
                {
                    damageText.text = "<color=red>CRIT! " + text + "</color>";
                    damageText.fontSize *= 1.2f;
                }
                else
                {
                    damageText.text = text;
                }
            }
            
            // Destroy after animation
            Destroy(damageTextObj, 1f);
        }
    }
    
    private void ShowHealText(float healAmount)
    {
        if (damageTextPrefab != null && damageTextSpawnPoint != null)
        {
            // Instantiate text prefab
            GameObject healTextObj = Instantiate(damageTextPrefab, damageTextSpawnPoint.position, Quaternion.identity);
            
            // Use cached reference instead of GameObject.Find at runtime
            if (healthSystemPanelTransform != null)
            {
                healTextObj.transform.SetParent(healthSystemPanelTransform, false);
            }
            else
            {
                // Try to find it once if we don't have it yet
                GameObject healthSystemPanel = GameObject.Find("HealthSystemPanel");
                if (healthSystemPanel != null)
                {
                    healthSystemPanelTransform = healthSystemPanel.transform;
                    healTextObj.transform.SetParent(healthSystemPanelTransform, false);
                }
                else
                {
                    Debug.LogWarning("HealthSystemPanel not found for heal text.");
                    healTextObj.transform.SetParent(null);
                }
            }
            
            // Get text component and set value
            TextMeshProUGUI healText = healTextObj.GetComponentInChildren<TextMeshProUGUI>();
            if (healText != null)
            {
                // Format with color for healing
                healText.text = "<color=green>+" + Mathf.RoundToInt(healAmount).ToString() + "</color>";
            }
            
            // Destroy after animation
            Destroy(healTextObj, 1f);
        }
    }
    
    private void UpdateHealthBar()
    {
        Debug.Log("UpdateHealthBar: " + healthBar);
        if (healthBar != null)
        {
            // Get slider component
            UnityEngine.UI.Slider healthSlider = healthBar.GetComponent<UnityEngine.UI.Slider>();
            Debug.Log("Object: " + healthBar + " currentHealth: " + currentHealth);
            if (healthSlider != null)
            {
                healthSlider.value = currentHealth / maxHealth;
            }
        }
    }
    
    // Public getters
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
    public float GetHealthPercentage() => currentHealth / maxHealth;
    public bool IsDead() => isDead;
    
    // For editor/debug visibility
    private void OnGUI()
    {
        // Uncomment for debug display
        
            GUI.Label(new Rect(40, 40, 200, 20), gameObject.name + " Health: " + currentHealth + "/" + maxHealth);
   
    }
} 