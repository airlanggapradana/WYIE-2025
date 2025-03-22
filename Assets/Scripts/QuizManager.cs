using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuizManager : MonoBehaviour
{
    [Header("Quiz Settings")]
    [SerializeField] private QuizSet[] quizSets;
    [SerializeField] private float baseDamageOnCorrect = 15f;
    [SerializeField] private float baseDamageOnIncorrect = 10f;
    [SerializeField] private float criticalHitChance = 0.1f;
    [SerializeField] private float criticalHitMultiplier = 2f;
    [SerializeField] private bool shuffleQuestions = false; // Option to shuffle or keep questions in order
    
    [Header("Animation")]
    [SerializeField] private float answerAnimationTime = 1.5f;
    [SerializeField] private GameObject correctAnswerEffect;
    [SerializeField] private GameObject wrongAnswerEffect;
    
    // References
    private QuizUI quizUI;
    private HealthSystem playerHealth;
    private HealthSystem bossHealth;
    private AttackSystem playerAttack;
    private GameObject currentBoss;
    
    // Visual effect references
    private List<GameObject> effectPool = new List<GameObject>();
    private int maxPoolSize = 10;
    
    // State tracking
    private bool quizActive = false;
    private bool processingAnswer = false; // Add this flag to prevent quiz restart during answer processing
    private int selectedAnswerIndex = 0;
    private QuizQuestion currentQuestion;
    private List<QuizQuestion> questionList = new List<QuizQuestion>();
    private int currentQuestionIndex = -1; // Index of current question in the list
    
    // Singleton instance
    public static QuizManager Instance { get; private set; }
    
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
    }
    
    private void Start()
    {
        // Pre-fill question list for performance
        PreloadQuestions();
        
        // Pre-create visual effects for the object pool
        PreloadVisualEffects();
    }
    
    private void Update()
    {
        if (!quizActive || quizUI == null) return;
        
        // Handle answer navigation
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            ChangeSelectedAnswer(-1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            ChangeSelectedAnswer(1);
        }
        
        // Handle answer submission
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
        {
            SubmitAnswer(selectedAnswerIndex);
        }
    }
    
    /// <summary>
    /// Register the QuizUI with the QuizManager
    /// </summary>
    public void RegisterQuizUI(QuizUI ui)
    {
        quizUI = ui;
        Debug.Log("QuizUI registered with QuizManager");
    }
    
    /// <summary>
    /// Starts a quiz battle with the specified boss
    /// </summary>
    public void StartQuizBattle(GameObject boss, QuizSet specificQuizSet = null)
    {
        if (quizUI == null)
        {
            Debug.LogError("Failed to start quiz: QuizUI not registered with QuizManager");
            return;
        }
        
        // Get references
        currentBoss = boss;
        bossHealth = boss.GetComponent<HealthSystem>();
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<HealthSystem>();
        playerAttack = GameObject.FindGameObjectWithTag("Player").GetComponent<AttackSystem>();
        
        if (bossHealth == null || playerHealth == null)
        {
            Debug.LogError("Failed to start quiz: missing HealthSystem component on boss or player");
            return;
        }
        
        // Set up quiz with specific quiz set if provided
        if (specificQuizSet != null)
        {
            LoadQuestionsFromSet(specificQuizSet);
        }
        else if (questionList.Count == 0)
        {
            PreloadQuestions();
        }
        
        // Reset question index to start from the beginning
        currentQuestionIndex = -1;
        
        // Start coroutine to properly sequence operations
        StartCoroutine(InitializeQuizUICoroutine());
    }
    
    private IEnumerator InitializeQuizUICoroutine()
    {
        // First, show the quiz UI
        quizActive = true;
        quizUI.GetQuizPanel().SetActive(true);
        
        // Wait for the end of frame to ensure UI is properly set up
        yield return new WaitForEndOfFrame();
        
        // Now it's safe to pause the game
        Time.timeScale = 0;
        
        // Display first question
        DisplayNextQuestion();
        UpdateHealthBars();
    }
    
    /// <summary>
    /// Ends the current quiz battle
    /// </summary>
    public void EndQuizBattle(bool allQuestionsAnswered = false)
    {
        Debug.Log($"EndQuizBattle called - allQuestionsAnswered: {allQuestionsAnswered}");
        
        // Double-check that we're marking the quiz as inactive
        quizActive = false;
        processingAnswer = false; // Also reset processing flag
        
        if (quizUI != null)
        {
            // Force the panel to hide
            GameObject quizPanel = quizUI.GetQuizPanel();
            if (quizPanel != null && quizPanel.activeSelf)
            {
                Debug.Log("Hiding quiz panel");
                quizPanel.SetActive(false);
            }
            
            quizUI.ClearAnswerButtons();
        }
        
        // Notify the boss that the quiz battle is completed
        if (currentBoss != null)
        {
            BossController bossController = currentBoss.GetComponent<BossController>();
            if (bossController != null)
            {
                Debug.Log("Notifying BossController that quiz battle is completed");
                bossController.OnQuizBattleCompleted();
            }
        }
        
        // Resume game
        Time.timeScale = 1;
        
        // Log final confirmation
        Debug.Log("Quiz battle ended successfully");
    }
    
    /// <summary>
    /// Preload questions from all available quiz sets
    /// </summary>
    private void PreloadQuestions()
    {
        if (quizSets == null || quizSets.Length == 0)
        {
            Debug.LogWarning("No quiz sets assigned to QuizManager");
            return;
        }
        
        // Clear existing questions
        questionList.Clear();
        
        // Add all questions from all quiz sets
        foreach (var quizSet in quizSets)
        {
            if (quizSet == null || quizSet.questions == null) continue;
            
            LoadQuestionsFromSet(quizSet);
        }
        
        // Shuffle questions if enabled
        if (shuffleQuestions && questionList.Count > 0)
        {
            ShuffleList(questionList);
        }
        
        Debug.Log($"Preloaded {questionList.Count} questions for quiz battles");
    }
    
    /// <summary>
    /// Load questions from a specific quiz set
    /// </summary>
    private void LoadQuestionsFromSet(QuizSet quizSet)
    {
        if (quizSet == null || quizSet.questions == null || quizSet.questions.Length == 0)
        {
            Debug.LogWarning($"Quiz set is empty or null");
            return;
        }
        
        // Clear existing questions if we're loading a specific set
        questionList.Clear();
        
        Debug.Log($"Loading questions from quiz set: {quizSet.quizTitle}");
        
        // Add each question from the quiz set
        foreach (var question in quizSet.questions)
        {
            // Validate the question before adding
            if (question.answerChoices != null && question.answerChoices.Length > 0)
            {
                // Make sure the correct answer index is valid
                if (question.correctAnswerIndex >= 0 && question.correctAnswerIndex < question.answerChoices.Length)
                {
                    questionList.Add(question);
                    Debug.Log($"Added question: '{question.questionText}' with correct answer: '{question.answerChoices[question.correctAnswerIndex]}'");
                }
                else
                {
                    Debug.LogError($"Question '{question.questionText}' has invalid correctAnswerIndex: {question.correctAnswerIndex}. Skipping...");
                }
            }
            else
            {
                Debug.LogError($"Question '{question.questionText}' has no answer choices. Skipping...");
            }
        }
        
        // Shuffle questions if enabled
        if (shuffleQuestions && questionList.Count > 0)
        {
            ShuffleList(questionList);
        }
        
        Debug.Log($"Loaded {questionList.Count} questions from quiz set: {quizSet.quizTitle}");
        
        // Reset question index
        currentQuestionIndex = -1;
    }
    
    /// <summary>
    /// Display the next question in the list
    /// </summary>
    private void DisplayNextQuestion()
    {
        if (quizUI == null) return;
        
        // Check if we have questions
        if (questionList.Count == 0)
        {
            Debug.LogError("No questions available!");
            EndQuizBattle(true);
            return;
        }
        
        // Move to the next question
        currentQuestionIndex++;
        
        Debug.Log($"DisplayNextQuestion - moving to question index {currentQuestionIndex} (List count: {questionList.Count})");
        
        // Check if we've reached the end of our questions
        if (currentQuestionIndex >= questionList.Count)
        {
            Debug.Log("No more questions in the list. All questions have been displayed. Ending quiz battle.");
            quizUI.SetFeedbackText("Quiz complete! All questions answered.", Color.yellow);
            
            // Immediately disable quiz activity
            quizActive = false;
            
            // Force the quiz panel to hide
            if (quizUI.GetQuizPanel() != null)
            {
                Debug.Log("Forcing quiz panel to hide from DisplayNextQuestion");
                quizUI.GetQuizPanel().SetActive(false);
            }
            
            // End the quiz with a short delay
            StartCoroutine(DelayedEndQuiz(false, true));
            return;
        }
        
        // Get the current question
        currentQuestion = questionList[currentQuestionIndex];
        
        Debug.Log($"Displaying question {currentQuestionIndex + 1}/{questionList.Count}: '{currentQuestion.questionText}'");
        Debug.Log($"Correct answer index: {currentQuestion.correctAnswerIndex}, Answer choices count: {currentQuestion.answerChoices.Length}");
        
        // Validate question data
        if (currentQuestion.answerChoices == null || currentQuestion.answerChoices.Length == 0)
        {
            Debug.LogError($"Question has no answer choices: {currentQuestion.questionText}");
            DisplayNextQuestion(); // Skip this question
            return;
        }
        
        // Validate the correct answer index
        if (currentQuestion.correctAnswerIndex < 0 || currentQuestion.correctAnswerIndex >= currentQuestion.answerChoices.Length)
        {
            Debug.LogError($"Question has invalid correctAnswerIndex: {currentQuestion.correctAnswerIndex}");
            
            // Fix the index if it's out of range
            currentQuestion.correctAnswerIndex = Mathf.Clamp(currentQuestion.correctAnswerIndex, 0, currentQuestion.answerChoices.Length - 1);
            Debug.Log($"Corrected answer index to: {currentQuestion.correctAnswerIndex}");
        }
        
        // Update question text
        quizUI.SetQuestionText(currentQuestion.questionText);
        
        // Create answer buttons
        quizUI.CreateAnswerButtons(currentQuestion, SubmitAnswer);
        
        // Reset selection
        selectedAnswerIndex = 0;
        quizUI.UpdateAnswerSelection(selectedAnswerIndex);
        
        // Clear feedback
        quizUI.SetFeedbackText("", Color.white);
    }
    
    /// <summary>
    /// Change the selected answer option
    /// </summary>
    private void ChangeSelectedAnswer(int direction)
    {
        if (quizUI == null || currentQuestion == null) return;
        
        // Update selection with wrap-around
        selectedAnswerIndex = (selectedAnswerIndex + direction + currentQuestion.answerChoices.Length) % currentQuestion.answerChoices.Length;
        quizUI.UpdateAnswerSelection(selectedAnswerIndex);
    }
    
    /// <summary>
    /// Handle answer submission and calculate damage
    /// </summary>
    private void SubmitAnswer(int answerIndex)
    {
        if (currentQuestion == null)
        {
            Debug.LogError("No current question when submitting answer!");
            return;
        }
        
        // Verify the answer index is valid
        if (answerIndex < 0 || answerIndex >= currentQuestion.answerChoices.Length)
        {
            Debug.LogError($"Invalid answer index {answerIndex} (answer count: {currentQuestion.answerChoices.Length})");
            return;
        }
        
        Debug.Log($"Quiz Question {currentQuestionIndex + 1}/{questionList.Count}: '{currentQuestion.questionText}'");
        Debug.Log($"Answer choices:");
        for (int i = 0; i < currentQuestion.answerChoices.Length; i++)
        {
            Debug.Log($"  [{i}]: '{currentQuestion.answerChoices[i]}'" + (i == currentQuestion.correctAnswerIndex ? " (CORRECT)" : ""));
        }
        
        Debug.Log($"Answer submitted: {answerIndex}, Correct answer: {currentQuestion.correctAnswerIndex}");
        
        // Log the answer text for debugging
        string selectedAnswerText = currentQuestion.answerChoices[answerIndex];
        Debug.Log($"Selected answer text: '{selectedAnswerText}'");
        
        // Double-check if this is the correct answer
        bool isCorrect = (answerIndex == currentQuestion.correctAnswerIndex);
        Debug.Log($"Is this the correct answer? {isCorrect}");
        
        // Verify against content
        string correctAnswerText = currentQuestion.answerChoices[currentQuestion.correctAnswerIndex];
        Debug.Log($"Content of correct answer: '{correctAnswerText}'");
        
        StartCoroutine(ProcessAnswer(isCorrect));
    }
    
    /// <summary>
    /// Preloads visual effects into an object pool to avoid runtime instantiation
    /// </summary>
    private void PreloadVisualEffects()
    {
        if (correctAnswerEffect != null)
        {
            for (int i = 0; i < maxPoolSize/2; i++)
            {
                GameObject effect = Instantiate(correctAnswerEffect);
                effect.SetActive(false);
                effectPool.Add(effect);
                DontDestroyOnLoad(effect);
            }
        }
        
        if (wrongAnswerEffect != null)
        {
            for (int i = 0; i < maxPoolSize/2; i++)
            {
                GameObject effect = Instantiate(wrongAnswerEffect);
                effect.SetActive(false);
                effectPool.Add(effect);
                DontDestroyOnLoad(effect);
            }
        }
    }
    
    /// <summary>
    /// Gets an effect from the pool or creates a new one if needed
    /// </summary>
    private GameObject GetEffectFromPool(GameObject effectPrefab)
    {
        // Find inactive effect of the right type in the pool
        foreach (GameObject effect in effectPool)
        {
            if (!effect.activeInHierarchy && effect.name.Contains(effectPrefab.name))
            {
                return effect;
            }
        }
        
        // If no existing effect is available, create a new one
        if (effectPool.Count < maxPoolSize)
        {
            GameObject newEffect = Instantiate(effectPrefab);
            newEffect.SetActive(false);
            effectPool.Add(newEffect);
            DontDestroyOnLoad(newEffect);
            return newEffect;
        }
        
        // If pool is full, reuse the oldest effect
        GameObject oldestEffect = effectPool[0];
        effectPool.RemoveAt(0);
        effectPool.Add(oldestEffect);
        return oldestEffect;
    }
    
    /// <summary>
    /// Process the submitted answer with animations and damage calculation
    /// </summary>
    private IEnumerator ProcessAnswer(bool isCorrect)
    {
        if (quizUI == null) yield break;
        
        // Set both flags to prevent quiz restart during animation
        quizActive = false;
        processingAnswer = true;
        
        Debug.Log($"Processing answer. Is correct: {isCorrect}, Current question index: {currentQuestionIndex}, Total questions: {questionList.Count}");
        
        if (isCorrect)
        {
            yield return HandleCorrectAnswer();
        }
        else
        {
            yield return HandleIncorrectAnswer();
        }
        
        // Wait for feedback to be read
        yield return new WaitForSecondsRealtime(answerAnimationTime);
        
        // Check if battle is over (boss or player died)
        if (CheckBattleEnd())
        {
            Debug.Log("Battle ending due to win/lose condition");
            processingAnswer = false; // Reset processing flag
            yield break;
        }
        
        // Process next steps - always proceed to next question regardless of answer correctness
        ProcessNextSteps(isCorrect);
        
        processingAnswer = false; // Reset processing flag
        quizActive = true;
    }
    
    /// <summary>
    /// Handles processing for a correct answer
    /// </summary>
    private IEnumerator HandleCorrectAnswer()
    {
        // Correct answer - player attacks boss
        quizUI.SetFeedbackText("Correct! You attack the boss!", Color.green);
        
        // Decide if critical hit
        bool isCritical = Random.value <= criticalHitChance;
        float damageMultiplier = isCritical ? criticalHitMultiplier : 1f;
        
        // Apply damage based on player's attack system and question difficulty
        float calculatedDamage = baseDamageOnCorrect;
        if (playerAttack != null)
        {
            // Ensure the difficultyMultiplier is at least 1
            float difficultyMult = Mathf.Max(1f, currentQuestion.difficultyMultiplier);
            calculatedDamage = playerAttack.GetBaseDamage() * damageMultiplier * difficultyMult;
        }
        
        // Show attack effect
        if (correctAnswerEffect)
        {
            // Use object pool instead of direct instantiation
            GameObject effect = GetEffectFromPool(correctAnswerEffect);
            effect.transform.position = bossHealth.transform.position;
            effect.SetActive(true);
            
            // Auto-disable after animation time
            StartCoroutine(DisableEffectAfterDelay(effect, answerAnimationTime));
        }
        
        // Wait for animation
        yield return new WaitForSecondsRealtime(answerAnimationTime * 0.5f);
        
        // Apply damage to boss
        bossHealth.TakeDamage(calculatedDamage, false, isCritical);
        UpdateHealthBars();
        
        // Show additional feedback for critical hit
        if (isCritical)
        {
            quizUI.SetFeedbackText("Correct! You attack the boss! CRITICAL HIT!", Color.green);
        }
    }
    
    /// <summary>
    /// Handles processing for an incorrect answer
    /// </summary>
    private IEnumerator HandleIncorrectAnswer()
    {
        // Incorrect answer - boss attacks player
        quizUI.SetFeedbackText("Incorrect! The boss attacks you!", Color.red);
        
        // Boss damage calculation
        // Ensure the difficultyMultiplier is at least 1
        float difficultyMult = Mathf.Max(1f, currentQuestion.difficultyMultiplier);
        float bossDamage = baseDamageOnIncorrect * difficultyMult;
        
        // Show wrong answer effect
        if (wrongAnswerEffect)
        {
            // Use object pool instead of direct instantiation
            GameObject effect = GetEffectFromPool(wrongAnswerEffect);
            effect.transform.position = playerHealth.transform.position;
            effect.SetActive(true);
            
            // Auto-disable after animation time
            StartCoroutine(DisableEffectAfterDelay(effect, answerAnimationTime));
        }
        
        // Wait for animation
        yield return new WaitForSecondsRealtime(answerAnimationTime * 0.5f);
        
        // Apply damage to player
        playerHealth.TakeDamage(bossDamage);
        UpdateHealthBars();
        
        // Show explanation if provided
        if (!string.IsNullOrEmpty(currentQuestion.explanationText))
        {
            quizUI.SetFeedbackText("Incorrect! The boss attacks you!\n" + currentQuestion.explanationText, Color.red);
        }
    }
    
    /// <summary>
    /// Determines what happens next based on the answer result
    /// </summary>
    private void ProcessNextSteps(bool isCorrect)
    {
        Debug.Log($"Moving to next question. Currently at question {currentQuestionIndex + 1}/{questionList.Count}");
        
        // Check if we've answered the last question
        if (currentQuestionIndex == questionList.Count - 1)
        {
            Debug.Log("That was the final question. Ending quiz.");
            quizUI.SetFeedbackText("Quiz complete! All questions answered.", Color.yellow);
            
            // End quiz when we've answered all questions
            quizActive = false;
            StartCoroutine(DelayedEndQuiz(false, true));
            return;
        }
        
        // Show next question
        Debug.Log($"Moving to next question. Current index: {currentQuestionIndex}, next question will be index: {currentQuestionIndex + 1}");
        DisplayNextQuestion();
    }
    
    /// <summary>
    /// Checks if the battle should end based on health
    /// </summary>
    private bool CheckBattleEnd()
    {
        if (quizUI == null) return false;
        
        if (bossHealth != null && bossHealth.IsDead())
        {
            // Boss defeated
            quizUI.SetFeedbackText("You defeated the boss!", Color.green);
            
            // End the quiz with a delay
            StartCoroutine(DelayedEndQuiz(true));
            return true;
        }
        else if (playerHealth != null && playerHealth.IsDead())
        {
            // Player defeated
            quizUI.SetFeedbackText("You were defeated!", Color.red);
            
            // End the quiz with a delay
            StartCoroutine(DelayedEndQuiz(false));
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Allows external scripts to force-end the quiz battle
    /// </summary>
    public void ForceEndQuizBattle()
    {
        Debug.Log("Force ending quiz battle");
        
        // More aggressive approach to ensure UI is hidden
        if (quizUI != null && quizUI.GetQuizPanel() != null)
        {
            quizUI.GetQuizPanel().SetActive(false);
        }
        
        quizActive = false;
        processingAnswer = false; // Also reset processing flag
        EndQuizBattle(true); // Treat as "all questions answered" to avoid triggering game over
    }
    
    /// <summary>
    /// Delayed end of quiz battle
    /// </summary>
    private IEnumerator DelayedEndQuiz(bool playerWon, bool allQuestionsAnswered = false)
    {
        Debug.Log($"DelayedEndQuiz called - PlayerWon: {playerWon}, AllQuestionsAnswered: {allQuestionsAnswered}");
        
        // Show a clear message about what's happening
        if (allQuestionsAnswered)
        {
            quizUI.SetFeedbackText("All questions completed! Quiz ending...", Color.green);
        }
        
        yield return new WaitForSecondsRealtime(2f);
        
        // Ensure we mark the quiz as inactive before ending
        quizActive = false;
        
        // Explicitly force the quiz panel to hide immediately
        if (quizUI != null && quizUI.GetQuizPanel() != null)
        {
            Debug.Log("Forcing quiz panel to hide");
            quizUI.GetQuizPanel().SetActive(false);
        }
        
        // Call end quiz battle with an explicit allQuestionsAnswered parameter
        EndQuizBattle(allQuestionsAnswered);
        
        // Handle game flow based on result
        GameManager gameManager = GameManager.Instance;
        if (gameManager != null && !allQuestionsAnswered)
        {
            if (playerWon)
            {
                gameManager.Victory();
            }
            else
            {
                gameManager.GameOver();
            }
        }
    }
    
    /// <summary>
    /// Updates the health bar UI elements
    /// </summary>
    private void UpdateHealthBars()
    {
        if (quizUI == null) return;
        
        if (playerHealth && bossHealth)
        {
            // Update sliders
            quizUI.GetPlayerHealthSlider().value = playerHealth.GetHealthPercentage();
            quizUI.GetBossHealthSlider().value = bossHealth.GetHealthPercentage();
            
            // Update text
            quizUI.UpdateHealthText(
                playerHealth.GetCurrentHealth(), 
                playerHealth.GetMaxHealth(),
                bossHealth.GetCurrentHealth(), 
                bossHealth.GetMaxHealth()
            );
        }
    }
    
    /// <summary>
    /// Shuffles a list using Fisher-Yates algorithm
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        for (int i = 0; i < n; i++)
        {
            // Use a temporary variable to hold the value being swapped
            T temp = list[i];
            
            // Pick a random index between i and end of list
            int r = i + Random.Range(0, n - i);
            
            // Swap elements
            list[i] = list[r];
            list[r] = temp;
        }
        
        Debug.Log("Questions shuffled");
    }
    
    /// <summary>
    /// Disables an effect after a delay
    /// </summary>
    private IEnumerator DisableEffectAfterDelay(GameObject effect, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (effect != null)
        {
            effect.SetActive(false);
        }
    }
    
    // Public properties
    public bool IsQuizActive => quizActive || processingAnswer; // Update property to include processingAnswer flag
} 