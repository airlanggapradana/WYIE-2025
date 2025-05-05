using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayTimeTracker : MonoBehaviour
{
    public static PlayTimeTracker Instance { get; private set; }

    [SerializeField] private GameObject warningDialog;
    [SerializeField] private Text warningText;
    [SerializeField] private GameObject cooldownDialog;
    [SerializeField] private Text cooldownText;

    private float playTimeInSeconds = 0f;
    private float cooldownTimeRemaining = 0f;
    private bool isInCooldown = false;
    private bool isPaused = false;
    private const float MAX_PLAY_TIME = 10f; // 1 hour in seconds
    private const float COOLDOWN_TIME = 60f; // 5 minutes in seconds
    private DateTime cooldownEndTime;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        warningDialog.SetActive(false);
        cooldownDialog.SetActive(false);
        LoadPlayTime();
        CheckCooldown();
    }

    private void Update()
    {
        if (isInCooldown)
        {
            UpdateCooldown();
        }
        else if (!isPaused)
        {
            playTimeInSeconds += Time.unscaledDeltaTime;

            if (playTimeInSeconds >= MAX_PLAY_TIME)
            {
                StartCooldown();
            }
        }
    }

    private void StartCooldown()
    {
        isPaused = true;
        isInCooldown = true;
        Time.timeScale = 0f; // Pause the game
        cooldownTimeRemaining = COOLDOWN_TIME;
        cooldownEndTime = DateTime.Now.AddSeconds(COOLDOWN_TIME);
        warningDialog.SetActive(false);
        cooldownDialog.SetActive(true);
        UpdateCooldownDisplay();
        SaveCooldownTime();
    }

    private void UpdateCooldown()
    {
        cooldownTimeRemaining = (float)(cooldownEndTime - DateTime.Now).TotalSeconds;

        if (cooldownTimeRemaining <= 0)
        {
            EndCooldown();
        }
        else
        {
            UpdateCooldownDisplay();
        }
    }

    private void UpdateCooldownDisplay()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(cooldownTimeRemaining);
        cooldownText.text = string.Format("Istirahat dulu ya ganteng! Kamu bisa main lagi kok dalam {0:D2}:{1:D2}",
                                        timeSpan.Minutes,
                                        timeSpan.Seconds);
    }

    private void EndCooldown()
    {
        isInCooldown = false;
        isPaused = false;
        playTimeInSeconds = 0f;
        Time.timeScale = 1f; // Unpause the game
        cooldownDialog.SetActive(false);
        PlayerPrefs.DeleteKey("CooldownEndTime");
        PlayerPrefs.Save();
    }

    private void CheckCooldown()
    {
        string cooldownString = PlayerPrefs.GetString("CooldownEndTime", "");

        if (!string.IsNullOrEmpty(cooldownString))
        {
            if (DateTime.TryParse(cooldownString, out cooldownEndTime))
            {
                cooldownTimeRemaining = (float)(cooldownEndTime - DateTime.Now).TotalSeconds;

                if (cooldownTimeRemaining > 0)
                {
                    StartCooldown();
                }
                else
                {
                    PlayerPrefs.DeleteKey("CooldownEndTime");
                }
            }
        }
    }

    private void SaveCooldownTime()
    {
        PlayerPrefs.SetString("CooldownEndTime", cooldownEndTime.ToString());
        PlayerPrefs.Save();
    }

    private void SavePlayTime()
    {
        PlayerPrefs.SetFloat("LastPlayTime", Time.time);
        PlayerPrefs.SetFloat("AccumulatedPlayTime", playTimeInSeconds);
        PlayerPrefs.Save();
    }

    private void LoadPlayTime()
    {
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
    }

    private void OnApplicationQuit()
    {
        if (!isInCooldown)
        {
            SavePlayTime();
        }
    }

    // For testing purposes
    public void ForceCooldownForTesting()
    {
        playTimeInSeconds = MAX_PLAY_TIME;
        StartCooldown();
    }
}