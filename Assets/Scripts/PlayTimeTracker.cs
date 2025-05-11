using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class PlayTimeTracker : MonoBehaviour
{
    public static PlayTimeTracker Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject warningDialog;
    [SerializeField] private Text warningText;
    [SerializeField] private GameObject energyCooldownDialog;
    [SerializeField] private Text energyCooldownText;
    [SerializeField] private Text energyText;
    [SerializeField] private Slider energySlider;

    [Header("Energy Settings")]
    [SerializeField] private int maxEnergy = 100;
    [SerializeField] private int energyDecreasePerLevel = 3;
    [SerializeField] private float energyRecoveryInterval = 180f; // 3 minutes
    [SerializeField] private int energyRecoveryAmount = 10;
    [SerializeField] private float energyCooldownTime = 300f; // 5 minutes

    private float playTimeInSeconds = 0f;
    private float energyRecoveryTimer = 0f;
    private float energyCooldownTimer = 0f;
    private int currentEnergy;
    private bool isInEnergyCooldown = false;
    private bool isPaused = false;
    private const float MAX_PLAY_TIME = 3600f; // 1 hour in seconds
    private string previousSceneName;
    private DateTime energyCooldownEndTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        warningDialog.SetActive(false);
        energyCooldownDialog.SetActive(false);
        LoadPlayerData();
        CheckEnergyCooldown();
        UpdateEnergyUI();
    }

    private void Update()
    {
        if (isInEnergyCooldown)
        {
            UpdateEnergyCooldown();
        }
        else if (!isPaused)
        {
            // Update play time tracking
            playTimeInSeconds += Time.unscaledDeltaTime;

            if (playTimeInSeconds >= MAX_PLAY_TIME)
            {
                ShowPlaytimeWarning();
            }

            // Update energy recovery system
            if (currentEnergy < maxEnergy)
            {
                energyRecoveryTimer += Time.unscaledDeltaTime;

                if (energyRecoveryTimer >= energyRecoveryInterval)
                {
                    RecoverEnergy();
                    energyRecoveryTimer = 0f;
                }
            }
            else
            {
                energyRecoveryTimer = 0f;
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!string.IsNullOrEmpty(previousSceneName) && previousSceneName != scene.name)
        {
            // Player entered a new level
            ConsumeEnergy();
        }
        previousSceneName = scene.name;
    }

    private void ConsumeEnergy()
    {
        if (isInEnergyCooldown) return;

        currentEnergy = Mathf.Max(0, currentEnergy - energyDecreasePerLevel);
        UpdateEnergyUI();

        if (currentEnergy <= 0)
        {
            StartEnergyCooldown();
        }

        SavePlayerData();
    }

    private void RecoverEnergy()
    {
        currentEnergy = Mathf.Min(maxEnergy, currentEnergy + energyRecoveryAmount);
        UpdateEnergyUI();
        SavePlayerData();
    }

    private void UpdateEnergyUI()
    {
        if (energyText != null)
        {
            energyText.text = $"Energy: {currentEnergy}/{maxEnergy}";
        }
        if (energySlider != null)
        {
            energySlider.maxValue = maxEnergy;
            energySlider.value = currentEnergy;
        }
    }

    private void ShowPlaytimeWarning()
    {
        isPaused = true;
        Time.timeScale = 0f;
        warningDialog.SetActive(true);
        warningText.text = "Kamu telah bermain terlalu lama, pertimbangkan untuk beristirahat sebentar";
    }

    public void OnContinueClicked()
    {
        isPaused = false;
        playTimeInSeconds = 0f;
        Time.timeScale = 1f;
        warningDialog.SetActive(false);
        SavePlayerData();
    }

    private void StartEnergyCooldown()
    {
        isPaused = true;
        isInEnergyCooldown = true;
        Time.timeScale = 0f;
        energyCooldownTimer = energyCooldownTime;
        energyCooldownEndTime = DateTime.Now.AddSeconds(energyCooldownTime);
        energyCooldownDialog.SetActive(true);
        UpdateEnergyCooldownDisplay();
        SavePlayerData();
    }

    private void UpdateEnergyCooldown()
    {
        energyCooldownTimer = (float)(energyCooldownEndTime - DateTime.Now).TotalSeconds;

        if (energyCooldownTimer <= 0)
        {
            EndEnergyCooldown();
        }
        else
        {
            UpdateEnergyCooldownDisplay();
        }
    }

    private void UpdateEnergyCooldownDisplay()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(energyCooldownTimer);
        energyCooldownText.text = $"Energi kamu udah abis! recovering dalam {timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }

    private void EndEnergyCooldown()
    {
        isInEnergyCooldown = false;
        isPaused = false;
        currentEnergy = maxEnergy;
        Time.timeScale = 1f;
        energyCooldownDialog.SetActive(false);
        UpdateEnergyUI();
        SavePlayerData();
    }

    private void CheckEnergyCooldown()
    {
        string cooldownString = PlayerPrefs.GetString("EnergyCooldownEndTime", "");

        if (!string.IsNullOrEmpty(cooldownString))
        {
            if (DateTime.TryParse(cooldownString, out energyCooldownEndTime))
            {
                energyCooldownTimer = (float)(energyCooldownEndTime - DateTime.Now).TotalSeconds;

                if (energyCooldownTimer > 0)
                {
                    StartEnergyCooldown();
                }
                else
                {
                    PlayerPrefs.DeleteKey("EnergyCooldownEndTime");
                }
            }
        }
    }

    private void LoadPlayerData()
    {
        // Load play time
        float lastPlayTime = PlayerPrefs.GetFloat("LastPlayTime", 0f);
        float accumulatedTime = PlayerPrefs.GetFloat("AccumulatedPlayTime", 0f);

        if (Time.time - lastPlayTime < 3600f)
        {
            playTimeInSeconds = accumulatedTime + (Time.time - lastPlayTime);
        }
        else
        {
            playTimeInSeconds = 0f;
        }

        // Load energy
        currentEnergy = PlayerPrefs.GetInt("PlayerEnergy", maxEnergy);
        energyRecoveryTimer = PlayerPrefs.GetFloat("EnergyRecoveryTimer", 0f);

        // Load energy cooldown
        CheckEnergyCooldown();
    }

    private void SavePlayerData()
    {
        PlayerPrefs.SetFloat("LastPlayTime", Time.time);
        PlayerPrefs.SetFloat("AccumulatedPlayTime", playTimeInSeconds);
        PlayerPrefs.SetInt("PlayerEnergy", currentEnergy);
        PlayerPrefs.SetFloat("EnergyRecoveryTimer", energyRecoveryTimer);

        if (isInEnergyCooldown)
        {
            PlayerPrefs.SetString("EnergyCooldownEndTime", energyCooldownEndTime.ToString());
        }
        else
        {
            PlayerPrefs.DeleteKey("EnergyCooldownEndTime");
        }

        PlayerPrefs.Save();
    }

    private void OnApplicationQuit()
    {
        SavePlayerData();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // For testing purposes
    public void ForceEnergyDepletion()
    {
        currentEnergy = 0;
        StartEnergyCooldown();
    }
}