using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [Header("Checkpoint Settings")]
    [SerializeField] private bool useCheckpoints = true;
    [SerializeField] private Vector3 defaultSpawnPoint = Vector3.zero;

    [Header("Respawn Settings")]
    [SerializeField] private float respawnDelay = 2f;
    [SerializeField] private GameObject respawnEffect;
    [SerializeField] private int maxLives = 3;
    [SerializeField] private bool infiniteLives = false;

    [Header("UI References")]
    [SerializeField] private GameObject livesRemainingUI;
    [SerializeField] private TMPro.TextMeshProUGUI livesText;

    // Runtime variables
    private List<Checkpoint> checkpoints = new List<Checkpoint>();
    private Checkpoint activeCheckpoint;
    private Vector3 respawnPosition;
    private int currentLives;
    private bool isRespawning = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize respawn position
        respawnPosition = defaultSpawnPoint;

        // Initialize lives
        currentLives = maxLives;
        UpdateLivesUI();
    }

    private void Start()
    {
        // Find all checkpoints in the scene
        FindAllCheckpoints();
    }

    /// <summary>
    /// Find all checkpoints in the current scene
    /// </summary>
    public void FindAllCheckpoints()
    {
        checkpoints.Clear();
        Checkpoint[] foundCheckpoints = FindObjectsOfType<Checkpoint>();

        foreach (Checkpoint checkpoint in foundCheckpoints)
        {
            checkpoints.Add(checkpoint);
            Debug.Log($"Found checkpoint {checkpoint.GetID()} at {checkpoint.GetPosition()}");
        }
    }

    /// <summary>
    /// Set the specified checkpoint as active and deactivate others
    /// </summary>
    public void SetActiveCheckpoint(Checkpoint checkpoint)
    {
        // Deactivate the current active checkpoint if there is one
        if (activeCheckpoint != null)
        {
            activeCheckpoint.SetActive(false);
        }

        // Set the new active checkpoint
        activeCheckpoint = checkpoint;
        respawnPosition = checkpoint.GetPosition();

        // Save checkpoint ID to PlayerPrefs for persistence
        PlayerPrefs.SetInt("LastCheckpointID", checkpoint.GetID());
        PlayerPrefs.Save();

        Debug.Log($"Active checkpoint set to ID {checkpoint.GetID()} at {respawnPosition}");
    }

    /// <summary>
    /// Try to respawn the player if they have lives remaining
    /// </summary>
    public void RespawnPlayer()
    {
        if (isRespawning) return;

        if (infiniteLives || currentLives > 0)
        {
            if (!infiniteLives)
            {
                currentLives--;
                UpdateLivesUI();
            }

            StartCoroutine(RespawnCoroutine());
        }
        else
        {
            // No lives left - game over
            Debug.Log("No lives remaining - Game Over");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }
    }

    private IEnumerator RespawnCoroutine()
    {
        isRespawning = true;

        // Hide game over screen if visible
        if (GameManager.Instance != null)
        {
            GameManager.Instance.HideGameOverScreen();
        }

        // Wait for specified delay
        yield return new WaitForSeconds(respawnDelay);

        // Find the player
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // Reset player position
            player.transform.position = respawnPosition;

            // Create respawn effect if available
            if (respawnEffect != null)
            {
                Instantiate(respawnEffect, respawnPosition, Quaternion.identity);
            }

            // Reset player health
            HealthSystem playerHealth = player.GetComponent<HealthSystem>();
            if (playerHealth != null)
            {
                playerHealth.ResetHealth();
            }

            // Reset player state to alive
            player.SetActive(true);
            Debug.Log($"Player respawned at {respawnPosition}. Lives remaining: {currentLives}");
        }
        else
        {
            Debug.LogWarning("Could not find player for respawning!");
        }

        isRespawning = false;
    }

    private void UpdateLivesUI()
    {
        if (livesText != null)
        {
            livesText.text = infiniteLives ? "∞" : currentLives.ToString();
        }

        if (livesRemainingUI != null)
        {
            livesRemainingUI.SetActive(true);
        }
    }

    /// <summary>
    /// Try to load the last active checkpoint from saved data
    /// </summary>
    public void LoadLastCheckpoint()
    {
        int savedCheckpointID = PlayerPrefs.GetInt("LastCheckpointID", -1);

        if (savedCheckpointID >= 0)
        {
            foreach (Checkpoint checkpoint in checkpoints)
            {
                if (checkpoint.GetID() == savedCheckpointID)
                {
                    SetActiveCheckpoint(checkpoint);
                    return;
                }
            }
        }

        // If no checkpoint found or saved, use default spawn point
        respawnPosition = defaultSpawnPoint;
    }

    /// <summary>
    /// Add an extra life to the player
    /// </summary>
    public void AddLife()
    {
        if (!infiniteLives)
        {
            currentLives++;
            UpdateLivesUI();
            Debug.Log($"Extra life added. Lives: {currentLives}");
        }
    }

    /// <summary>
    /// Get the current respawn position (last checkpoint or default spawn)
    /// </summary>
    public Vector3 GetRespawnPosition()
    {
        return respawnPosition;
    }

    /// <summary>
    /// Get the current number of lives
    /// </summary>
    public int GetLives()
    {
        return currentLives;
    }

    /// <summary>
    /// Check if player has infinite lives
    /// </summary>
    public bool HasInfiniteLives()
    {
        return infiniteLives;
    }
}