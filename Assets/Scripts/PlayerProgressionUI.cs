using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerProgressionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider experienceBar;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI experienceText;
    [SerializeField] private GameObject levelUpEffect;
    [SerializeField] private Transform levelUpEffectSpawnPoint;
    
    [Header("Animation")]
    [SerializeField] private float experienceBarFillSpeed = 2f;
    [SerializeField] private float levelUpAnimationDuration = 1.5f;
    
    // References
    private PlayerProgression playerProgression;
    
    // State tracking
    private float targetExperienceBarValue;
    private float currentExperienceBarValue;
    
    private void Start()
    {
        // Find player progression component
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerProgression = player.GetComponent<PlayerProgression>();
            
            if (playerProgression != null)
            {
                Debug.Log("PlayerProgressionUI found PlayerProgression component on player!");
                // Subscribe to level up and XP events
                playerProgression.OnLevelUp.AddListener(HandleLevelUp);
                playerProgression.OnExperienceGained.AddListener(HandleExperienceGained);
                
                // Initialize UI with current values
                UpdateLevelText(playerProgression.GetCurrentLevel());
                UpdateExperienceText(playerProgression.GetCurrentExperience(), playerProgression.GetExperienceToNextLevel());
                
                // Initialize experience bar
                if (experienceBar != null)
                {
                    currentExperienceBarValue = playerProgression.GetLevelProgress();
                    targetExperienceBarValue = currentExperienceBarValue;
                    experienceBar.value = currentExperienceBarValue;
                }
            }
            else
            {
                Debug.LogWarning("PlayerProgressionUI could not find PlayerProgression component on player!");
            }
        }
        else
        {
            Debug.LogWarning("PlayerProgressionUI could not find player!");
        }
    }
    
    private void Update()
    {
        // Smoothly update the experience bar
        if (experienceBar != null && currentExperienceBarValue != targetExperienceBarValue)
        {
            // Lerp towards target value
            currentExperienceBarValue = Mathf.Lerp(
                currentExperienceBarValue, 
                targetExperienceBarValue, 
                Time.deltaTime * experienceBarFillSpeed
            );
            
            // Snap to target if very close
            if (Mathf.Abs(currentExperienceBarValue - targetExperienceBarValue) < 0.01f)
            {
                currentExperienceBarValue = targetExperienceBarValue;
            }
            
            // Update the bar
            experienceBar.value = currentExperienceBarValue;
        }
    }
    
    /// <summary>
    /// Handle level up event
    /// </summary>
    private void HandleLevelUp(int newLevel)
    {
        // Play level up animation/effect
        if (levelUpEffect != null && levelUpEffectSpawnPoint != null)
        {
            GameObject effect = Instantiate(levelUpEffect, levelUpEffectSpawnPoint.position, Quaternion.identity);
            Destroy(effect, levelUpAnimationDuration);
        }
        
        // Update level text
        UpdateLevelText(newLevel);
        
        // Reset experience bar
        targetExperienceBarValue = playerProgression.GetLevelProgress();
        
        // Update experience text
        UpdateExperienceText(playerProgression.GetCurrentExperience(), playerProgression.GetExperienceToNextLevel());
    }
    
    /// <summary>
    /// Handle experience gained event
    /// </summary>
    private void HandleExperienceGained(int currentExp, int gainedExp)
    {
        // Update progress bar target
        targetExperienceBarValue = playerProgression.GetLevelProgress();
        
        // Update text
        UpdateExperienceText(currentExp, playerProgression.GetExperienceToNextLevel());
    }
    
    /// <summary>
    /// Update level display
    /// </summary>
    private void UpdateLevelText(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Level {level}";
        }
    }
    
    /// <summary>
    /// Update experience text display
    /// </summary>
    private void UpdateExperienceText(int current, int required)
    {
        if (experienceText != null)
        {
            experienceText.text = $"{current} / {required} XP";
        }
    }
} 