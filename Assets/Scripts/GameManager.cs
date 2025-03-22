using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Game State")]
    [SerializeField] private bool gameIsPaused = false;
    
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI victoryText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitButton;
    
    [Header("Game Settings")]
    [SerializeField] private float gameOverDelay = 2f;
    [SerializeField] private float victoryDelay = 2f;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string nextLevelSceneName = "";
    
    private bool isGameOver = false;
    private bool hasWon = false;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Hide UI panels at start
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        
        // Setup button listeners
        if (restartButton != null) restartButton.onClick.AddListener(RestartLevel);
        if (continueButton != null) continueButton.onClick.AddListener(LoadNextLevel);
        if (quitButton != null) quitButton.onClick.AddListener(QuitToMainMenu);
    }
    
    private void Update()
    {
        // Handle pause menu
        if (Input.GetKeyDown(KeyCode.Escape) && !isGameOver && !hasWon)
        {
            TogglePause();
        }
    }
    
    /// <summary>
    /// Called when the player dies
    /// </summary>
    public void GameOver()
    {
        if (isGameOver || hasWon) return;
        
        isGameOver = true;
        StartCoroutine(ShowGameOverScreen());
        
        Debug.Log("Game Over!");
    }
    
    /// <summary>
    /// Called when the player defeats the boss/wins the level
    /// </summary>
    public void Victory()
    {
        if (isGameOver || hasWon) return;
        
        hasWon = true;
        StartCoroutine(ShowVictoryScreen());
        
        Debug.Log("Victory!");
    }
    
    /// <summary>
    /// Toggle pause state of the game
    /// </summary>
    public void TogglePause()
    {
        gameIsPaused = !gameIsPaused;
        
        if (gameIsPaused)
        {
            Time.timeScale = 0f;
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        }
        
        Debug.Log("Game " + (gameIsPaused ? "Paused" : "Resumed"));
    }
    
    /// <summary>
    /// Restart the current level
    /// </summary>
    public void RestartLevel()
    {
        // Reset game state
        isGameOver = false;
        hasWon = false;
        gameIsPaused = false;
        Time.timeScale = 1f;
        
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    /// <summary>
    /// Load the next level
    /// </summary>
    public void LoadNextLevel()
    {
        // Reset game state
        isGameOver = false;
        hasWon = false;
        gameIsPaused = false;
        Time.timeScale = 1f;
        
        // Load the next scene
        if (!string.IsNullOrEmpty(nextLevelSceneName))
        {
            SceneManager.LoadScene(nextLevelSceneName);
        }
        else
        {
            Debug.LogWarning("No next level specified!");
        }
    }
    
    /// <summary>
    /// Quit to main menu
    /// </summary>
    public void QuitToMainMenu()
    {
        // Reset game state
        isGameOver = false;
        hasWon = false;
        gameIsPaused = false;
        Time.timeScale = 1f;
        
        // Load main menu
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogWarning("No main menu scene specified!");
        }
    }
    
    /// <summary>
    /// Show game over screen after delay
    /// </summary>
    private IEnumerator ShowGameOverScreen()
    {
        yield return new WaitForSeconds(gameOverDelay);
        
        // Show game over UI
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            if (gameOverText != null)
            {
                gameOverText.text = "GAME OVER";
            }
        }
    }
    
    /// <summary>
    /// Show victory screen after delay
    /// </summary>
    private IEnumerator ShowVictoryScreen()
    {
        yield return new WaitForSeconds(victoryDelay);
        
        // Show victory UI
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            
            if (victoryText != null)
            {
                victoryText.text = "VICTORY!";
            }
        }
    }
    
    /// <summary>
    /// Check if the game is currently paused
    /// </summary>
    public bool IsGamePaused()
    {
        return gameIsPaused;
    }
    
    /// <summary>
    /// Check if the game is over (player lost)
    /// </summary>
    public bool IsGameOver()
    {
        return isGameOver;
    }
    
    /// <summary>
    /// Check if the player has won
    /// </summary>
    public bool HasWon()
    {
        return hasWon;
    }
} 