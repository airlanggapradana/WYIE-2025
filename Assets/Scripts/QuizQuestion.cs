using System;
using UnityEngine;

[Serializable]
public class QuizQuestion
{
    [TextArea(3, 5)]
    public string questionText;
    public string[] answerChoices;
    public int correctAnswerIndex;
    public float difficultyMultiplier = 1.0f; // Affects damage dealt/taken
    
    // Next question index to go to after this question is answered correctly
    public int nextQuestionIndex = -1; // -1 means proceed to next question in sequence, -2 means end quiz
    
    [TextArea(2, 3)]
    public string explanationText; // Optional explanation for educational purposes
}

[CreateAssetMenu(fileName = "New Quiz Set", menuName = "Quiz System/Quiz Set")]
public class QuizSet : ScriptableObject
{
    public string quizTitle;
    [TextArea(2, 4)]
    public string quizDescription;
    public QuizQuestion[] questions;
    
    // Starting question index (usually 0)
    public int startQuestionIndex = 0;
    
    public QuizQuestion GetRandomQuestion()
    {
        if (questions == null || questions.Length == 0)
            return null;
            
        return questions[UnityEngine.Random.Range(0, questions.Length)];
    }
} 