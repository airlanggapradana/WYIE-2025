using UnityEngine;
using System.Collections;

public class BossController : MonoBehaviour
{
    [Header("Boss Settings")]
    [SerializeField] private string bossName = "Mighty Boss";
    [SerializeField] private float detectionRadius = 8f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float moveSpeed = 3f;
    
    [Header("Combat Phases")]
    [SerializeField] private int numPhases = 3;
    [SerializeField] private float[] phaseHealthThresholds = new float[] { 0.75f, 0.5f, 0.25f }; // Percent of max health
    [SerializeField] private float[] phaseDamageMultipliers = new float[] { 1f, 1.2f, 1.5f };
    [SerializeField] private float[] phaseSpeedMultipliers = new float[] { 1f, 1.15f, 1.3f };
    
    [Header("Quiz Battle")]
    [SerializeField] private bool useQuizBattle = false;
    [SerializeField] private QuizSet bossQuizSet;
    [SerializeField] private float quizDetectionRadius = 5f;
    [SerializeField] private bool quizBattleCompleted = false; // Track if the quiz battle has been completed
    
    [Header("Components")]
    [SerializeField] private Transform healthBarPivot;
    
    // References to required components
    private HealthSystem healthSystem;
    private AttackSystem attackSystem;
    private Rigidbody2D rb;
    private Animator animator;
    
    // State tracking
    private enum BossState { Idle, Chasing, Attacking, StunLocked, Dead }
    private BossState currentState = BossState.Idle;
    private Transform playerTransform;
    private int currentPhase = 0;
    private bool canChangePhase = true;
    private bool playerInSight = false;
    
    private void Awake()
    {
        // Get required components
        healthSystem = GetComponent<HealthSystem>();
        attackSystem = GetComponent<AttackSystem>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("No player found in scene! Boss won't be able to target.");
        }
        
        // Subscribe to health system events
        if (healthSystem != null)
        {
            healthSystem.OnDamaged.AddListener(CheckPhaseTransition);
            healthSystem.OnDeath.AddListener(() => {
                SetState(BossState.Dead);
                
                // Award XP to player
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    PlayerProgression playerProgression = player.GetComponent<PlayerProgression>();
                    if (playerProgression != null)
                    {
                        // Award XP based on boss phase
                        int bossLevel = currentPhase + 1;
                        Debug.Log($"Awarding XP for defeating boss (Level {bossLevel})");
                        playerProgression.GainBossExperience(bossLevel);
                    }
                }
                
                // Trigger victory and level transition
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.Victory();
                    
                    // Schedule level transition after victory UI is shown
                    StartCoroutine(ScheduleNextLevel());
                }
            });
        }
        else
        {
            Debug.LogError("Boss missing HealthSystem component!");
        }
    }
    
    private void Update()
    {
        if (playerTransform == null || healthSystem.IsDead()) return;
        
        // Face the player
        if (playerTransform.position.x > transform.position.x)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        
        // Update the health bar rotation to always face camera
        if (healthBarPivot != null)
        {
            healthBarPivot.rotation = Quaternion.identity;
        }
        
        // Check if player is in sight
        CheckPlayerInSight();
        
        // Check if we should start a quiz battle
        if (useQuizBattle && !quizBattleCompleted && playerInSight && QuizManager.Instance != null && !QuizManager.Instance.IsQuizActive)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= quizDetectionRadius)
            {
                // Start the quiz battle
                StartQuizBattle();
                return;
            }
        }
        
        // Only run state machine if quiz is not active
        if (!useQuizBattle || quizBattleCompleted || (QuizManager.Instance != null && !QuizManager.Instance.IsQuizActive))
        {
            // State machine
            switch (currentState)
            {
                case BossState.Idle:
                    IdleState();
                    break;
                    
                case BossState.Chasing:
                    ChasingState();
                    break;
                    
                case BossState.Attacking:
                    AttackingState();
                    break;
                    
                case BossState.StunLocked:
                    // Stay stunned
                    break;
                    
                case BossState.Dead:
                    // Stay dead
                    break;
            }
        }
        
        // Update animation parameters
        UpdateAnimations();
    }
    
    private void CheckPlayerInSight()
    {
        if (playerTransform == null) return;
        
        // Calculate distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // Check if player is in detection radius (use quiz radius if in quiz mode)
        float effectiveRadius = useQuizBattle ? quizDetectionRadius : detectionRadius;
        playerInSight = distanceToPlayer <= effectiveRadius;
        
        // Visual debug in scene view
        Debug.DrawLine(transform.position, playerTransform.position, playerInSight ? Color.red : Color.grey);
    }
    
    private void IdleState()
    {
        // If player enters detection radius, start chasing
        if (playerInSight)
        {
            SetState(BossState.Chasing);
        }
    }
    
    private void ChasingState()
    {
        if (playerTransform == null) return;
        
        // Calculate distance to player
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // If close enough to attack, transition to attacking
        if (distanceToPlayer <= attackRange)
        {
            SetState(BossState.Attacking);
            return;
        }
        
        // If player out of detection range, go back to idle
        if (!playerInSight)
        {
            SetState(BossState.Idle);
            return;
        }
        
        // Chase the player
        MoveTowardsPlayer();
    }
    
    private void AttackingState()
    {
        if (playerTransform == null) return;
        
        // Check if still in attack range
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        if (distanceToPlayer > attackRange + 0.5f) // Add a small buffer to prevent state oscillation
        {
            SetState(BossState.Chasing);
            return;
        }
        
        // Perform attack if available
        if (attackSystem != null && attackSystem.CanAttack())
        {
            // Decide between basic and special attack based on phase
            if (currentPhase >= 2 && attackSystem.CanSpecialAttack() && Random.value < 0.3f)
            {
                attackSystem.SpecialAttack();
            }
            else
            {
                Debug.Log("attack from boss controller");
                Debug.Log("attackSystem: " + attackSystem);
                attackSystem.Attack();
            }
        }
    }
    
    private void MoveTowardsPlayer()
    {
        if (playerTransform == null || rb == null) return;
        
        // Get direction to player
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        
        // Move towards player
        float adjustedSpeed = moveSpeed * (currentPhase < phaseSpeedMultipliers.Length ? phaseSpeedMultipliers[currentPhase] : 1f);
        rb.linearVelocity = direction * adjustedSpeed;
        
        // Update animation
        if (animator != null)
        {
            animator.SetBool("IsMoving", true);
            animator.SetFloat("Horizontal", direction.x);
        }
    }
    
    private void StopMoving()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        // Update animation
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
        }
    }
    
    private void UpdateAnimations()
    {
        if (animator == null) return;
        
        // Update animation parameters based on state
        // animator.SetInteger("State", (int)currentState);
        // animator.SetFloat("HealthPercent", healthSystem != null ? healthSystem.GetHealthPercentage() : 1f);
    }
    
    private void SetState(BossState newState)
    {
        // Exit previous state
        switch (currentState)
        {
            case BossState.Chasing:
                StopMoving();
                break;
        }
        
        // Set new state
        currentState = newState;
        
        // Enter new state
        switch (newState)
        {
            case BossState.Idle:
                StopMoving();
                break;
                
            case BossState.Dead:
                StopMoving();
                // Any death-specific code
                break;
        }
        
        Debug.Log($"Boss state: {currentState}");
    }
    
    /// <summary>
    /// Check if boss should transition to next phase based on health
    /// </summary>
    private void CheckPhaseTransition()
    {
        if (!canChangePhase || healthSystem == null) return;
        
        float healthPercentage = healthSystem.GetHealthPercentage();
        
        // Check if we should move to the next phase
        if (currentPhase < numPhases - 1 && 
            currentPhase < phaseHealthThresholds.Length && 
            healthPercentage <= phaseHealthThresholds[currentPhase])
        {
            TransitionToNextPhase();
        }
    }
    
    /// <summary>
    /// Transition to the next combat phase with visual effect and behavior change
    /// </summary>
    private void TransitionToNextPhase()
    {
        currentPhase++;
        
        // Temporarily stun the boss during transition
        StartCoroutine(PhaseTransitionRoutine());
        
        Debug.Log($"Boss entering phase {currentPhase + 1}");
        
        // You can add special effects, animations, or dialogue here
    }
    
    private IEnumerator PhaseTransitionRoutine()
    {
        // Lock phase changing temporarily
        canChangePhase = false;
        
        // Stun the boss briefly
        BossState previousState = currentState;
        SetState(BossState.StunLocked);
        
        // Play phase transition animation/effect
        if (animator != null)
        {
            animator.SetTrigger("PhaseChange");
        }
        
        // Wait for animation
        yield return new WaitForSeconds(2f);
        
        // Return to previous state
        SetState(previousState);
        
        // Re-enable phase changing
        canChangePhase = true;
    }
    
    /// <summary>
    /// Start a quiz battle with this boss
    /// </summary>
    private void StartQuizBattle()
    {
        Debug.Log("Starting quiz battle with " + bossName);
        QuizManager.Instance.StartQuizBattle(gameObject, bossQuizSet);
        
        // Subscribe to events
        healthSystem.OnDamaged.AddListener(CheckQuizEndCondition);
    }
    
    /// <summary>
    /// Check if the quiz should end based on boss health
    /// </summary>
    private void CheckQuizEndCondition()
    {
        // If boss health is low, end the quiz
        if (healthSystem.GetHealthPercentage() <= 0.1f && QuizManager.Instance != null && QuizManager.Instance.IsQuizActive)
        {
            Debug.Log("Boss health low - ending quiz");
            QuizManager.Instance.ForceEndQuizBattle();
            quizBattleCompleted = true;
        }
    }
    
    /// <summary>
    /// Mark the quiz battle as completed (called from QuizManager)
    /// </summary>
    public void OnQuizBattleCompleted()
    {
        quizBattleCompleted = true;
        Debug.Log("Quiz battle completed");
        
        // Unsubscribe from events
        healthSystem.OnDamaged.RemoveListener(CheckQuizEndCondition);
    }
    
    /// <summary>
    /// Schedule transition to next level after a delay
    /// </summary>
    private IEnumerator ScheduleNextLevel()
    {
        // Wait for victory screen to display (using GameManager's victoryDelay)
        yield return new WaitForSeconds(3f);
        
        // Trigger level transition if LevelManager exists
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.AdvanceToNextLevel();
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Draw detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw quiz detection radius if quiz mode is enabled
        if (useQuizBattle)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, quizDetectionRadius);
        }
    }

    /// <summary>
    /// Get the current phase of the boss (0-based index)
    /// Used for determining boss level for XP rewards
    /// </summary>
    public int GetCurrentPhase()
    {
        return currentPhase;
    }
} 