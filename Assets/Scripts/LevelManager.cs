using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelData
    {
        public string levelName;
        public string sceneName;
        public string bossName;
        [TextArea(2, 4)] public string levelDescription;
    }
    
    [Header("Level Configuration")]
    [SerializeField] private LevelData[] levels;
    [SerializeField] private bool loadLastUnlockedLevelOnStart = true;
    
    [Header("Transition Settings")]
    [SerializeField] private float defaultTransitionDelay = 2f;
    [SerializeField] private GameObject levelTransitionEffect;
    [SerializeField] private AudioClip levelCompleteSound;
    
    [Header("End Game Settings")]
    [SerializeField] private string endGameSceneName = "EndGameScene";
    [SerializeField] private bool returnToMainMenuIfEndGameMissing = true;
    
    // Save data keys
    private const string CURRENT_LEVEL_KEY = "CurrentLevelIndex";
    private const string HIGHEST_LEVEL_KEY = "HighestUnlockedLevel";
    
    // Singleton pattern
    private static LevelManager _instance;
    public static LevelManager Instance { get { return _instance; } }
    
    private int currentLevelIndex = 0;
    private int highestUnlockedLevel = 0;
    private AudioSource audioSource;
    
    private void Awake()
    {
        // Singleton implementation
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Add audio source if needed
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && levelCompleteSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Load saved progress
        LoadProgress();
        
        // Get current level index based on loaded scene if not loading from save
        if (!loadLastUnlockedLevelOnStart)
        {
            SetCurrentLevelByScene(SceneManager.GetActiveScene().name);
        }
    }
    
    private void Start()
    {
        // If we should load the last unlocked level and we're not already there
        if (loadLastUnlockedLevelOnStart && 
            SceneManager.GetActiveScene().name != levels[highestUnlockedLevel].sceneName)
        {
            SceneManager.LoadScene(levels[highestUnlockedLevel].sceneName);
        }
    }
    
    public void AdvanceToNextLevel(float delay = -1)
    {
        // Update highest unlocked level
        if (currentLevelIndex + 1 > highestUnlockedLevel)
        {
            highestUnlockedLevel = currentLevelIndex + 1;
            if (highestUnlockedLevel >= levels.Length)
            {
                highestUnlockedLevel = levels.Length - 1;
            }
            
            // Save progress
            SaveProgress();
        }
        
        float transitionDelay = delay >= 0 ? delay : defaultTransitionDelay;
        StartCoroutine(TransitionToNextLevel(transitionDelay));
    }
    
    private IEnumerator TransitionToNextLevel(float delay)
    {
        // Play level complete sound
        if (audioSource != null && levelCompleteSound != null)
        {
            audioSource.PlayOneShot(levelCompleteSound);
        }
        
        // Show transition effect if available
        if (levelTransitionEffect != null)
        {
            Instantiate(levelTransitionEffect);
        }
        
        yield return new WaitForSeconds(delay);
        
        // Move to next level index
        currentLevelIndex++;
        
        // Check if we have more levels
        if (currentLevelIndex < levels.Length)
        {
            // Check if the next level scene exists
            string nextSceneName = levels[currentLevelIndex].sceneName;
            bool sceneExists = false;
            
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                if (sceneName == nextSceneName)
                {
                    sceneExists = true;
                    break;
                }
            }
            
            if (sceneExists)
            {
                // Load the next level scene
                SceneManager.LoadScene(nextSceneName);
                Debug.Log($"Loading next level: {nextSceneName}");
            }
            else
            {
                Debug.LogWarning($"Next level scene '{nextSceneName}' not found in build settings!");
                
                // Try to continue with next level or go to end game
                if (currentLevelIndex + 1 < levels.Length)
                {
                    // Skip to next level
                    currentLevelIndex++;
                    StartCoroutine(TransitionToNextLevel(0f));
                }
                else
                {
                    // No more levels, go to end game
                    HandleEndGame();
                }
            }
        }
        else
        {
            // No more levels, handle end game
            HandleEndGame();
        }
        
        // Save the current level
        SaveProgress();
    }
    
    /// <summary>
    /// Handle end game when there are no more levels
    /// </summary>
    private void HandleEndGame()
    {
        // Try loading end game scene
        if (!string.IsNullOrEmpty(endGameSceneName))
        {
            // Check if the scene exists in build settings
            bool sceneExists = false;
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                if (sceneName == endGameSceneName)
                {
                    sceneExists = true;
                    break;
                }
            }
            
            if (sceneExists)
            {
                // Scene exists, load it
                SceneManager.LoadScene(endGameSceneName);
                Debug.Log($"Loading end game scene: {endGameSceneName}");
            }
            else
            {
                // Scene doesn't exist, handle the fallback
                Debug.LogWarning($"EndGameScene '{endGameSceneName}' not found in build settings!");
                
                if (returnToMainMenuIfEndGameMissing && GameManager.Instance != null)
                {
                    // Return to main menu as fallback
                    GameManager.Instance.QuitToMainMenu();
                }
                else
                {
                    // Stay in the current level as a last resort
                    currentLevelIndex--; // Revert the increment
                }
            }
        }
        else
        {
            // No end game scene specified, go to main menu
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitToMainMenu();
            }
        }
    }
    
    public void RestartCurrentLevel()
    {
        SceneManager.LoadScene(levels[currentLevelIndex].sceneName);
    }
    
    public void LoadSpecificLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex <= highestUnlockedLevel && levelIndex < levels.Length)
        {
            currentLevelIndex = levelIndex;
            SceneManager.LoadScene(levels[currentLevelIndex].sceneName);
            SaveProgress();
        }
        else
        {
            Debug.LogWarning("Attempted to load locked or invalid level: " + levelIndex);
        }
    }
    
    private void SetCurrentLevelByScene(string sceneName)
    {
        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i].sceneName == sceneName)
            {
                currentLevelIndex = i;
                break;
            }
        }
    }
    
    private void SaveProgress()
    {
        PlayerPrefs.SetInt(CURRENT_LEVEL_KEY, currentLevelIndex);
        PlayerPrefs.SetInt(HIGHEST_LEVEL_KEY, highestUnlockedLevel);
        PlayerPrefs.Save();
    }
    
    private void LoadProgress()
    {
        currentLevelIndex = PlayerPrefs.GetInt(CURRENT_LEVEL_KEY, 0);
        highestUnlockedLevel = PlayerPrefs.GetInt(HIGHEST_LEVEL_KEY, 0);
        
        // Validate saved data
        if (currentLevelIndex >= levels.Length)
        {
            currentLevelIndex = 0;
        }
        
        if (highestUnlockedLevel >= levels.Length)
        {
            highestUnlockedLevel = levels.Length - 1;
        }
    }
    
    // Getters for UI and other systems
    public string GetCurrentLevelName()
    {
        if (currentLevelIndex >= 0 && currentLevelIndex < levels.Length)
        {
            return levels[currentLevelIndex].levelName;
        }
        return "Unknown Level";
    }
    
    public int GetCurrentLevelIndex()
    {
        return currentLevelIndex;
    }
    
    public int GetHighestUnlockedLevel()
    {
        return highestUnlockedLevel;
    }
    
    public LevelData GetCurrentLevelData()
    {
        if (currentLevelIndex >= 0 && currentLevelIndex < levels.Length)
        {
            return levels[currentLevelIndex];
        }
        return null;
    }
    
    public LevelData[] GetAllLevelData()
    {
        return levels;
    }
}