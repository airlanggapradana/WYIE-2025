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

    [Header("Level Progression")]
    [SerializeField] private bool useAutoLevelProgression = true;
    [SerializeField] private float levelTransitionDelay = 3f;

    [Header("Respawn Settings")]
    [SerializeField] private bool useCheckpointRespawn = true;
    [SerializeField] private float respawnTransitionTime = 1.5f;
    [SerializeField] private Image fadeOverlay;

    private bool isGameOver = false;
    private bool hasWon = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
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
        if (fadeOverlay != null) fadeOverlay.gameObject.SetActive(false);

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

        // Allow pressing Enter key to continue when victory panel is shown
        if (hasWon && Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (victoryPanel != null && victoryPanel.activeSelf && continueButton != null)
            {
                // Trigger the same functionality as clicking the continue button
                LoadNextLevel();
            }
        }
    }

    /// <summary>
    /// Called when the player dies
    /// </summary>
    public void GameOver()
    {
        if (isGameOver || hasWon) return;

        // Only show game over screen if not using checkpoint respawn
        // or if CheckpointManager says we're out of lives
        if (!useCheckpointRespawn ||
            (CheckpointManager.Instance != null &&
            !CheckpointManager.Instance.HasInfiniteLives() &&
            CheckpointManager.Instance.GetLives() <= 0))
        {
            isGameOver = true;
            StartCoroutine(ShowGameOverScreen());
            Debug.Log("Final Game Over - No lives remaining!");
        }
        else
        {
            // Don't set isGameOver flag if we're going to respawn
            // The checkpoint system will handle it
            Debug.Log("Player died, but lives remaining. Will respawn.");
        }
    }

    /// <summary>
    /// Called when the player defeats the boss/wins the level
    /// </summary>
    public void Victory()
    {
        if (isGameOver || hasWon) return;

        hasWon = true;
        StartCoroutine(ShowVictoryScreen());

        // If using auto progression and LevelManager exists, transition will be handled by BossController
        // This is kept separate to allow for manual progression via continue button as well

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

        // Use LevelManager if available, otherwise fallback to direct scene loading
        if (useAutoLevelProgression && LevelManager.Instance != null)
        {
            LevelManager.Instance.AdvanceToNextLevel(0f);
        }
        else if (!string.IsNullOrEmpty(nextLevelSceneName))
        {
            // Fallback: Load the next scene directly
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
    /// Hide the game over screen for respawning
    /// </summary>
    public void HideGameOverScreen()
    {
        if (gameOverPanel != null && gameOverPanel.activeSelf)
        {
            StartCoroutine(FadeForRespawn(false));
        }
    }

    private IEnumerator FadeForRespawn(bool fadeIn)
    {
        if (fadeOverlay != null)
        {
            fadeOverlay.gameObject.SetActive(true);

            float elapsedTime = 0f;
            Color startColor = fadeOverlay.color;
            Color targetColor = startColor;

            if (fadeIn)
            {
                // Fade to black
                targetColor.a = 1f;
            }
            else
            {
                // Fade from black
                targetColor.a = 0f;
            }

            while (elapsedTime < respawnTransitionTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / respawnTransitionTime;
                fadeOverlay.color = Color.Lerp(startColor, targetColor, progress);
                yield return null;
            }

            fadeOverlay.color = targetColor;

            if (!fadeIn)
            {
                fadeOverlay.gameObject.SetActive(false);
                // Actually hide the game over panel
                gameOverPanel.SetActive(false);
                isGameOver = false;
            }
        }
        else
        {
            // No fade overlay, just hide/show the panel
            if (!fadeIn)
            {
                gameOverPanel.SetActive(false);
                isGameOver = false;
            }
        }
    }

    /// <summary>
    /// Show game over screen after delay
    /// </summary>
    private IEnumerator ShowGameOverScreen()
    {
        yield return new WaitForSeconds(gameOverDelay);

        // Fade to black before showing game over
        if (fadeOverlay != null)
        {
            yield return StartCoroutine(FadeForRespawn(true));
        }

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