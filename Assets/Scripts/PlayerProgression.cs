using UnityEngine;
using UnityEngine.Events;

public class PlayerProgression : MonoBehaviour
{
    [Header("Experience Settings")]
    [SerializeField] private int baseExperienceRequirement = 100; // Base XP needed for first level up
    [SerializeField] private float experienceScaleFactor = 1.5f; // How much XP requirements scale per level
    [SerializeField] private int experiencePerBoss = 50; // Base XP gained from defeating a boss
    
    [Header("Damage Settings")]
    [SerializeField] private float baseDamageIncrease = 5f; // Flat damage increase per level
    [SerializeField] private float damagePercentIncrease = 0.1f; // 10% damage increase per level
    [SerializeField] private bool usePercentageBased = true; // Toggle between flat or percentage-based increases
    
    [Header("Critical Hit Settings")]
    [SerializeField] private float baseCriticalChance = 0.05f; // 5% base critical chance
    [SerializeField] private float criticalChancePerLevel = 0.02f; // Additional 2% per level
    [SerializeField] private float maxCriticalChance = 0.5f; // Maximum 50% critical chance
    
    // Events
    public UnityEvent<int> OnLevelUp; // Event triggered when player levels up
    public UnityEvent<int, int> OnExperienceGained; // Event triggered when player gains XP (current, gained)
    
    // State tracking
    private int currentLevel = 1;
    private int currentExperience = 0;
    private int experienceToNextLevel;
    
    // Component references
    private AttackSystem attackSystem;
    private PlayerCombat playerCombat;
    
    private void Awake()
    {
        // Get required components
        attackSystem = GetComponent<AttackSystem>();
        playerCombat = GetComponent<PlayerCombat>();
        
        // Initialize progression event handlers
        if (OnLevelUp == null) OnLevelUp = new UnityEvent<int>();
        if (OnExperienceGained == null) OnExperienceGained = new UnityEvent<int, int>();
        
        // Error checking
        if (attackSystem == null)
        {
            Debug.LogError("PlayerProgression requires an AttackSystem component!");
        }
    }
    
    private void Start()
    {
        // Calculate initial XP requirement
        experienceToNextLevel = CalculateExperienceForLevel(currentLevel + 1);
        
        // Apply level 1 stats
        ApplyLevelStats();
        
        Debug.Log($"Player starting at Level {currentLevel}. XP to Level {currentLevel + 1}: {experienceToNextLevel}");
    }
    
    /// <summary>
    /// Award XP to the player (e.g., after defeating a boss)
    /// </summary>
    public void GainExperience(int amount, float difficultyMultiplier = 1.0f)
    {
        // Calculate actual XP gain with difficulty multiplier
        int actualGain = Mathf.RoundToInt(amount * difficultyMultiplier);
        
        // Add experience
        currentExperience += actualGain;
        
        // Trigger event
        OnExperienceGained.Invoke(currentExperience, actualGain);
        
        Debug.Log($"Gained {actualGain} XP. Total: {currentExperience}/{experienceToNextLevel}");
        
        // Check for level up
        CheckLevelUp();
    }
    
    /// <summary>
    /// Award boss defeat experience with potential scaling based on boss level
    /// </summary>
    public void GainBossExperience(int bossLevel = 1, float difficultyMultiplier = 1.0f)
    {
        // Scale XP based on boss level
        int expGain = experiencePerBoss * bossLevel;
        
        // Award XP
        GainExperience(expGain, difficultyMultiplier);
    }
    
    /// <summary>
    /// Check if player has enough XP to level up, and process if so
    /// </summary>
    private void CheckLevelUp()
    {
        if (currentExperience >= experienceToNextLevel)
        {
            // Process level up
            currentLevel++;
            
            // Calculate overflow XP
            int overflowXP = currentExperience - experienceToNextLevel;
            currentExperience = overflowXP;
            
            // Calculate new XP threshold
            experienceToNextLevel = CalculateExperienceForLevel(currentLevel + 1);
            
            // Apply new level stats
            ApplyLevelStats();
            
            // Trigger level up event
            OnLevelUp.Invoke(currentLevel);
            
            Debug.Log($"Level Up! Player is now Level {currentLevel}. XP to Level {currentLevel + 1}: {experienceToNextLevel}");
            
            // Check if there's enough overflow XP for another level up
            CheckLevelUp();
        }
    }
    
    /// <summary>
    /// Apply stat bonuses based on current level
    /// </summary>
    private void ApplyLevelStats()
    {
        if (attackSystem != null)
        {
            // Get base stats
            float baseDamage = attackSystem.GetBaseDamage();
            
            // Calculate new damage
            float newDamage;
            if (usePercentageBased)
            {
                // Percentage-based increase (base + base * level * percent)
                float multiplier = 1f + ((currentLevel - 1) * damagePercentIncrease);
                newDamage = baseDamage * multiplier;
            }
            else
            {
                // Flat increase (base + level * flat)
                newDamage = baseDamage + ((currentLevel - 1) * baseDamageIncrease);
            }
            
            // Apply new damage value
            attackSystem.SetBaseDamage(newDamage);
            
            // Calculate new critical hit chance
            float critChance = Mathf.Min(
                baseCriticalChance + ((currentLevel - 1) * criticalChancePerLevel),
                maxCriticalChance
            );
            
            // Apply new critical hit chance
            attackSystem.SetCriticalHitChance(critChance);
            
            Debug.Log($"Level {currentLevel} stats: Damage = {newDamage}, Crit Chance = {critChance * 100}%");
        }
    }
    
    /// <summary>
    /// Calculate the total XP required for a specific level
    /// Uses an exponential curve to make higher levels require more XP
    /// </summary>
    private int CalculateExperienceForLevel(int level)
    {
        return Mathf.RoundToInt(baseExperienceRequirement * Mathf.Pow(experienceScaleFactor, level - 2));
    }
    
    // Getter methods for UI and other systems
    public int GetCurrentLevel() => currentLevel;
    public int GetCurrentExperience() => currentExperience;
    public int GetExperienceToNextLevel() => experienceToNextLevel;
    
    /// <summary>
    /// Get the current progress toward the next level (0-1)
    /// </summary>
    public float GetLevelProgress()
    {
        return (float)currentExperience / experienceToNextLevel;
    }
    
    /// <summary>
    /// Get the player's current critical hit chance
    /// </summary>
    public float GetCriticalHitChance()
    {
        return Mathf.Min(
            baseCriticalChance + ((currentLevel - 1) * criticalChancePerLevel),
            maxCriticalChance
        );
    }
} 