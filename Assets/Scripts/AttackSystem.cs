using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class AttackSystem : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float baseDamage = 10f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private bool debugAttacks = true;
    
    [Header("Attack Types")]
    [SerializeField] private bool enableSpecialAttack = false;
    [SerializeField] private float specialAttackDamage = 25f;
    [SerializeField] private float specialAttackCooldown = 5f;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject attackEffectPrefab;
    [SerializeField] private Transform attackPoint;
    
    [Header("Critical Hit Settings")]
    [SerializeField] private float criticalHitChance = 0.05f; // 5% base chance
    [SerializeField] private float criticalHitMultiplier = 2f; // Double damage on crit
    
    // Events
    public UnityEvent OnAttack;
    public UnityEvent OnSpecialAttack;
    public UnityEvent<float> OnDamageDealt;
    
    private bool canAttack = true;
    private bool canSpecialAttack = true;
    private Animator animator;
    
    private void Awake()
    {
        animator = GetComponent<Animator>();
        
        // Create attack point if none exists
        if (attackPoint == null)
        {
            GameObject attackPointObj = new GameObject("AttackPoint");
            attackPointObj.transform.parent = transform;
            attackPointObj.transform.localPosition = Vector3.forward + Vector3.right * 0.5f;
            attackPoint = attackPointObj.transform;
            Debug.Log("Attack Point created at runtime. Position it appropriately in the editor.");
        }
        
        // Debug info
        if (debugAttacks)
        {
            Debug.Log($"AttackSystem initialized: Attack Range = {attackRange}, Target Layers = {targetLayers.value}");
            if (attackPoint != null)
            {
                Debug.Log($"Attack Point position: {attackPoint.position}, Attack Point local position: {attackPoint.localPosition}");
            }
            else
            {
                Debug.LogError("Attack Point is still null after Awake!");
            }
        }
        
        // Initialize events
        if (OnAttack == null) OnAttack = new UnityEvent();
        if (OnSpecialAttack == null) OnSpecialAttack = new UnityEvent();
        if (OnDamageDealt == null) OnDamageDealt = new UnityEvent<float>();
    }
    
    /// <summary>
    /// Perform a basic attack if not on cooldown
    /// </summary>
    public void Attack()
    {
        if (!canAttack) return;
        
        // Start attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // Play attack sound or effect
        if (attackEffectPrefab != null && attackPoint != null)
        {
            Instantiate(attackEffectPrefab, attackPoint.position, attackPoint.rotation);
        }
        
        // Detect hits
        DetectAndDamageTargets(baseDamage);
        
        // Put attack on cooldown
        StartCoroutine(AttackCooldown());
    }
    
    /// <summary>
    /// Perform a special attack if available
    /// </summary>
    public void SpecialAttack()
    {
        if (!enableSpecialAttack || !canSpecialAttack) return;
        
        // Start special attack animation
        if (animator != null)
        {
            animator.SetTrigger("SpecialAttack");
        }
        
        // Play special attack effect
        if (attackEffectPrefab != null && attackPoint != null)
        {
            GameObject effect = Instantiate(attackEffectPrefab, attackPoint.position, attackPoint.rotation);
            effect.transform.localScale *= 1.5f; // Make the effect bigger for special attacks
        }
        
        // Apply damage with guaranteed critical hit
        DetectAndDamageTargets(specialAttackDamage, true, true);
        
        // Put special attack on cooldown
        StartCoroutine(SpecialAttackCooldown());
    }
    
    /// <summary>
    /// Detect targets in range and apply damage
    /// </summary>
    private void DetectAndDamageTargets(float damage, bool ignoreDefense = false, bool guaranteedCritical = false)
    {
        if (attackPoint == null)
        {
            Debug.LogError("AttackSystem: Cannot attack - attackPoint is null!");
            return;
        }
        
        if (debugAttacks)
        {
            Debug.Log("======= ATTACK DETECTION =======");
            Debug.Log($"Attack point: {attackPoint.position}, Attack range: {attackRange}");
            
            // List all layers in the target mask
            string layerNames = "";
            for (int i = 0; i < 32; i++)
            {
                if ((targetLayers.value & (1 << i)) != 0)
                {
                    layerNames += LayerMask.LayerToName(i) + ", ";
                }
            }
            Debug.Log($"Target layers: {layerNames}");
        }
        
        // Create a sphere cast to find targets
        Collider2D[] hitTargets = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, targetLayers);

        bool hitAnyTarget = false;
        
        if (debugAttacks)
        {
            Debug.Log($"Found {hitTargets.Length} targets with OverlapCircleAll");
            
            // Show all nearby colliders for debugging
            Collider2D[] allColliders = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);
            Debug.Log($"Total colliders nearby (any layer): {allColliders.Length}");
            foreach (Collider2D col in allColliders)
            {
                Debug.Log($"Nearby object: {col.gameObject.name}, Layer: {LayerMask.LayerToName(col.gameObject.layer)} ({col.gameObject.layer})");
            }
        }
        
        // Apply damage to each target found
        foreach (Collider2D target in hitTargets)
        {
            // Get target's health system
            HealthSystem targetHealth = target.GetComponent<HealthSystem>();
            
            if (targetHealth != null)
            {
                // Calculate distance to confirm we're in range
                float distanceToTarget = Vector2.Distance(attackPoint.position, target.transform.position);
                
                // Only damage if we're actually within range (double check)
                if (distanceToTarget <= attackRange)
                {
                    // Apply damage
                    float actualDamage = targetHealth.TakeDamage(damage, ignoreDefense, guaranteedCritical);
                    
                    Debug.Log($"Hit {target.name} for {actualDamage} damage. Distance: {distanceToTarget:F2}");
                    hitAnyTarget = true;
                }
                else
                {
                    Debug.Log($"Target {target.name} detected but out of range. Distance: {distanceToTarget:F2}, Max Range: {attackRange:F2}");
                }
            }
            else 
            {
                Debug.Log($"No HealthSystem on target {target.name}");
            }
        }
        
        if (!hitAnyTarget)
        {
            Debug.Log($"No targets hit within range of {attackRange:F2}");
        }
    }
    
    /// <summary>
    /// Cooldown coroutine for basic attacks
    /// </summary>
    private IEnumerator AttackCooldown()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }
    
    /// <summary>
    /// Cooldown coroutine for special attacks
    /// </summary>
    private IEnumerator SpecialAttackCooldown()
    {
        canSpecialAttack = false;
        yield return new WaitForSeconds(specialAttackCooldown);
        canSpecialAttack = true;
    }
    
    /// <summary>
    /// Visual debug to show attack range
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            // Draw attack range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
            
            // Draw a filled sphere to make it more visible
            Gizmos.color = new Color(1, 0, 0, 0.2f); // Semi-transparent red
            Gizmos.DrawSphere(attackPoint.position, attackRange);
        }
        else
        {
            // Draw from transform if attackPoint is null
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + Vector3.forward + Vector3.right * 0.5f, attackRange);
        }
    }
    
    // Public getters
    public bool CanAttack() => canAttack;
    public bool CanSpecialAttack() => enableSpecialAttack && canSpecialAttack;
    public float GetBaseDamage() => baseDamage;
    public Transform GetAttackPoint() => attackPoint;
    public float GetAttackRange() => attackRange;
    
    /// <summary>
    /// Get current critical hit chance
    /// </summary>
    public float GetCriticalHitChance()
    {
        return criticalHitChance;
    }
    
    /// <summary>
    /// Set critical hit chance (used for level progression)
    /// </summary>
    public void SetCriticalHitChance(float newChance)
    {
        criticalHitChance = Mathf.Clamp01(newChance);
    }

    /// <summary>
    /// Set the base damage value (used for level progression)
    /// </summary>
    public void SetBaseDamage(float newDamage)
    {
        baseDamage = newDamage;
        // Also scale special attack damage proportionally
        specialAttackDamage = baseDamage * 2f;
    }

    public void SetDamage(float newDamage)
    {
        baseDamage = newDamage;
    }
} 