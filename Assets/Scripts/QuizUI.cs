using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// This script helps set up the UI elements for the quiz system
/// Attach this to the root GameObject of your quiz UI prefab
/// </summary>
public class QuizUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject quizPanel;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private GameObject answerButtonPrefab; // Prefab for answer buttons
    [SerializeField] private Transform answerContainer; // Parent transform for answer buttons
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private Slider bossHealthSlider;
    [SerializeField] private TextMeshProUGUI playerHealthText;
    [SerializeField] private TextMeshProUGUI bossHealthText;
    [SerializeField] private TextMeshProUGUI titleText;
    
    // List to store dynamically created answer buttons
    private List<GameObject> answerButtonInstances = new List<GameObject>();
    
    private void Awake()
    {
        // Ensure the quiz panel is hidden at start
        if (quizPanel != null)
        {
            quizPanel.SetActive(false);
        }
        
        // Validate UI elements
        ValidateUISetup();
        
        // Register with QuizManager
        QuizManager quizManager = FindObjectOfType<QuizManager>();
        if (quizManager != null)
        {
            RegisterWithQuizManager(quizManager);
        }
        else
        {
            Debug.LogWarning("No QuizManager found in scene. UI elements won't be connected automatically.");
        }
    }
    
    /// <summary>
    /// Validate that all required UI elements are assigned
    /// </summary>
    private void ValidateUISetup()
    {
        bool isValid = true;
        
        if (quizPanel == null)
        {
            Debug.LogError("Quiz Panel not assigned in QuizUI!");
            isValid = false;
        }
        
        if (questionText == null)
        {
            Debug.LogError("Question Text not assigned in QuizUI!");
            isValid = false;
        }
        
        if (answerButtonPrefab == null)
        {
            Debug.LogError("Answer Button Prefab not assigned in QuizUI!");
            isValid = false;
        }
        else
        {
            // Validate the button prefab setup
            ValidateButtonPrefab();
        }
        
        if (answerContainer == null)
        {
            Debug.LogError("Answer Container not assigned in QuizUI!");
            isValid = false;
        }
        
        if (feedbackText == null)
        {
            Debug.LogError("Feedback Text not assigned in QuizUI!");
            isValid = false;
        }
        
        if (playerHealthSlider == null)
        {
            Debug.LogError("Player Health Slider not assigned in QuizUI!");
            isValid = false;
        }
        
        if (bossHealthSlider == null)
        {
            Debug.LogError("Boss Health Slider not assigned in QuizUI!");
            isValid = false;
        }
        
        if (!isValid)
        {
            Debug.LogError("QuizUI setup is incomplete. Quiz system may not function correctly.");
        }
    }
    
    /// <summary>
    /// Validates that the button prefab is set up correctly
    /// </summary>
    private void ValidateButtonPrefab()
    {
        // Check for Button component
        Button button = answerButtonPrefab.GetComponent<Button>();
        if (button == null)
        {
            Debug.LogError("Answer Button Prefab is missing a Button component!");
        }
        
        // Check for text component
        TextMeshProUGUI tmpText = answerButtonPrefab.GetComponentInChildren<TextMeshProUGUI>();
        Text legacyText = answerButtonPrefab.GetComponentInChildren<Text>();
        
        if (tmpText == null && legacyText == null)
        {
            Debug.LogError("Answer Button Prefab doesn't have a TextMeshProUGUI or Text component as a child! " +
                "Your button prefab should have a child Text element.");
            
            // Provide a helpful hint on typical button structure
            Debug.LogError("Typical button hierarchy: Button (with Button and Image components) → Text (with TextMeshProUGUI component)");
        }
        else
        {
            // Log the found text component for debugging
            if (tmpText != null)
            {
                Debug.Log($"Button prefab has TextMeshProUGUI component: {tmpText.gameObject.name}");
            }
            else
            {
                Debug.Log($"Button prefab has legacy Text component: {legacyText.gameObject.name}");
            }
        }
    }
    
    /// <summary>
    /// Register UI elements with the QuizManager
    /// </summary>
    public void RegisterWithQuizManager(QuizManager quizManager)
    {
        quizManager.RegisterQuizUI(this);
        Debug.Log("QuizUI successfully registered with QuizManager.");
    }
    
    /// <summary>
    /// Creates answer buttons based on the question data
    /// </summary>
    public void CreateAnswerButtons(QuizQuestion question, System.Action<int> onAnswerSelected)
    {
        // Clear existing buttons
        ClearAnswerButtons();
        
        Debug.Log($"Creating {question.answerChoices.Length} answer buttons");
        
        // Create new buttons for each answer choice
        for (int i = 0; i < question.answerChoices.Length; i++)
        {
            GameObject buttonObj = Instantiate(answerButtonPrefab, answerContainer);
            buttonObj.name = $"AnswerButton_{i}"; // Give it a clear name for debugging
            answerButtonInstances.Add(buttonObj);
            
            string answerText = question.answerChoices[i];
            Debug.Log($"Setting button {i} text to: {answerText}");
            
            // Try to find the text component (more thoroughly)
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            
            // If not found, try looking for a regular Text component
            if (buttonText == null)
            {
                Debug.LogWarning($"No TextMeshProUGUI found on button {i}. Trying regular Text component...");
                Text legacyText = buttonObj.GetComponentInChildren<Text>();
                
                if (legacyText != null)
                {
                    legacyText.text = answerText;
                    Debug.Log($"Set text using legacy Text component on button {i}");
                }
                else
                {
                    Debug.LogError($"No text component found on button {i}! Check your button prefab setup.");
                }
            }
            else
            {
                buttonText.text = answerText;
                Debug.Log($"Set text using TextMeshProUGUI on button {i}");
            }
            
            // Set up the button click handler
            int answerIndex = i; // Create a local copy for the lambda
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                
                // Create a direct callback for the button click
                button.onClick.AddListener(() => {
                    Debug.Log($"Button {answerIndex} clicked, answer: {answerText}");
                    onAnswerSelected(answerIndex);
                });
                
                // Ensure button is interactable
                button.interactable = true;
                
                Debug.Log($"Set up click handler for button {i}");
            }
            else
            {
                Debug.LogError($"No Button component found on button {i}!");
            }
        }
    }
    
    /// <summary>
    /// Clears all dynamically created answer buttons
    /// </summary>
    public void ClearAnswerButtons()
    {
        foreach (GameObject button in answerButtonInstances)
        {
            Destroy(button);
        }
        answerButtonInstances.Clear();
    }
    
    /// <summary>
    /// Updates the selection state of answer buttons
    /// </summary>
    public void UpdateAnswerSelection(int selectedIndex)
    {
        for (int i = 0; i < answerButtonInstances.Count; i++)
        {
            // Use the button's image component to show selection
            Image btnImage = answerButtonInstances[i].GetComponent<Image>();
            if (btnImage != null)
            {
                Color color = btnImage.color;
                if (i == selectedIndex)
                {
                    // Highlighted selection
                    color.a = 1.0f;
                    btnImage.color = color;
                    // Option to scale up selected button for better visibility
                    answerButtonInstances[i].transform.localScale = Vector3.one * 1.1f;
                }
                else
                {
                    // Normal state
                    color.a = 0.6f;
                    btnImage.color = color;
                    answerButtonInstances[i].transform.localScale = Vector3.one;
                }
            }
        }
    }
    
    /// <summary>
    /// Sets the question text
    /// </summary>
    public void SetQuestionText(string text)
    {
        if (questionText != null)
        {
            questionText.text = text;
        }
    }
    
    /// <summary>
    /// Sets the feedback text and color
    /// </summary>
    public void SetFeedbackText(string text, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = text;
            feedbackText.color = color;
        }
    }
    
    /// <summary>
    /// Gets the quiz panel
    /// </summary>
    public GameObject GetQuizPanel()
    {
        return quizPanel;
    }
    
    /// <summary>
    /// Gets the player health slider
    /// </summary>
    public Slider GetPlayerHealthSlider()
    {
        return playerHealthSlider;
    }
    
    /// <summary>
    /// Gets the boss health slider
    /// </summary>
    public Slider GetBossHealthSlider()
    {
        return bossHealthSlider;
    }
    
    /// <summary>
    /// Update health text displays
    /// </summary>
    public void UpdateHealthText(float playerCurrentHealth, float playerMaxHealth, float bossCurrentHealth, float bossMaxHealth)
    {
        if (playerHealthText != null)
        {
            playerHealthText.text = $"{Mathf.RoundToInt(playerCurrentHealth)}/{Mathf.RoundToInt(playerMaxHealth)}";
        }
        
        if (bossHealthText != null)
        {
            bossHealthText.text = $"{Mathf.RoundToInt(bossCurrentHealth)}/{Mathf.RoundToInt(bossMaxHealth)}";
        }
    }
    
    /// <summary>
    /// Set the title text for the quiz panel
    /// </summary>
    public void SetTitleText(string title)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }
    }
} 