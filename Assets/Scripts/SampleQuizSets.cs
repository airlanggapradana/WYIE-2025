using UnityEngine;
using UnityEditor;

/// <summary>
/// This script provides example code for creating quiz sets programmatically
/// and includes a custom editor method to create sample quiz sets
/// </summary>
public class SampleQuizSets : MonoBehaviour
{
    [Header("Quiz Set Creation")]
    [SerializeField] private string folderPath = "Assets/Quizzes";
    [SerializeField] private bool createSampleQuizzes = false;
    
    private void OnValidate()
    {
        // Auto-create sample quizzes when the checkbox is checked in the inspector
        if (createSampleQuizzes)
        {
            createSampleQuizzes = false; // Reset checkbox
            CreateSampleQuizSets();
        }
    }
    
    /// <summary>
    /// Creates sample quiz sets with different question types
    /// </summary>
    private void CreateSampleQuizSets()
    {
        #if UNITY_EDITOR
        // Create Math quiz set
        QuizSet mathQuiz = ScriptableObject.CreateInstance<QuizSet>();
        mathQuiz.quizTitle = "Math Quiz";
        mathQuiz.quizDescription = "Test your math knowledge with these challenging questions.";
        mathQuiz.questions = new QuizQuestion[]
        {
            new QuizQuestion
            {
                questionText = "What is 7 × 8?",
                answerChoices = new string[] { "54", "56", "64", "72" },
                correctAnswerIndex = 1,
                difficultyMultiplier = 1.0f,
                explanationText = "7 × 8 = 56"
            },
            new QuizQuestion
            {
                questionText = "What is the square root of 144?",
                answerChoices = new string[] { "12", "14", "16", "18" },
                correctAnswerIndex = 0,
                difficultyMultiplier = 1.2f,
                explanationText = "√144 = 12"
            },
            new QuizQuestion
            {
                questionText = "If x + 5 = 12, what is x?",
                answerChoices = new string[] { "5", "7", "8", "17" },
                correctAnswerIndex = 1,
                difficultyMultiplier = 1.0f,
                explanationText = "x + 5 = 12, so x = 12 - 5 = 7"
            }
        };
        
        // Create Science quiz set
        QuizSet scienceQuiz = ScriptableObject.CreateInstance<QuizSet>();
        scienceQuiz.quizTitle = "Science Quiz";
        scienceQuiz.quizDescription = "Test your science knowledge with these questions.";
        scienceQuiz.questions = new QuizQuestion[]
        {
            new QuizQuestion
            {
                questionText = "What is the chemical symbol for gold?",
                answerChoices = new string[] { "Go", "Gd", "Au", "Ag" },
                correctAnswerIndex = 2,
                difficultyMultiplier = 1.0f,
                explanationText = "The chemical symbol for gold is Au (from Latin 'aurum')."
            },
            new QuizQuestion
            {
                questionText = "Which planet is known as the Red Planet?",
                answerChoices = new string[] { "Venus", "Mars", "Jupiter", "Saturn" },
                correctAnswerIndex = 1,
                difficultyMultiplier = 0.8f,
                explanationText = "Mars is known as the Red Planet due to its reddish appearance."
            },
            new QuizQuestion
            {
                questionText = "What is the hardest natural substance on Earth?",
                answerChoices = new string[] { "Platinum", "Titanium", "Diamond", "Graphene" },
                correctAnswerIndex = 2,
                difficultyMultiplier = 1.5f,
                explanationText = "Diamond is the hardest naturally occurring substance on Earth."
            }
        };
        
        // Create Gaming quiz set
        QuizSet gamingQuiz = ScriptableObject.CreateInstance<QuizSet>();
        gamingQuiz.quizTitle = "Gaming Quiz";
        gamingQuiz.quizDescription = "Test your knowledge of video games with these questions.";
        gamingQuiz.questions = new QuizQuestion[]
        {
            new QuizQuestion
            {
                questionText = "Which company created Mario?",
                answerChoices = new string[] { "Sega", "Nintendo", "Sony", "Microsoft" },
                correctAnswerIndex = 1,
                difficultyMultiplier = 0.7f,
                explanationText = "Mario was created by Nintendo."
            },
            new QuizQuestion
            {
                questionText = "In Minecraft, what material makes the strongest tools?",
                answerChoices = new string[] { "Iron", "Gold", "Diamond", "Netherite" },
                correctAnswerIndex = 3,
                difficultyMultiplier = 1.2f,
                explanationText = "Netherite tools are the strongest in Minecraft, followed by Diamond."
            },
            new QuizQuestion
            {
                questionText = "Which game series features a character named Master Chief?",
                answerChoices = new string[] { "Call of Duty", "Halo", "Gears of War", "Destiny" },
                correctAnswerIndex = 1,
                difficultyMultiplier = 1.0f,
                explanationText = "Master Chief is the protagonist of the Halo series."
            }
        };
        
        // Ensure the folder exists
        if (!System.IO.Directory.Exists(folderPath))
        {
            System.IO.Directory.CreateDirectory(folderPath);
        }
        
        // Save the quiz sets as assets
        AssetDatabase.CreateAsset(mathQuiz, $"{folderPath}/MathQuiz.asset");
        AssetDatabase.CreateAsset(scienceQuiz, $"{folderPath}/ScienceQuiz.asset");
        AssetDatabase.CreateAsset(gamingQuiz, $"{folderPath}/GamingQuiz.asset");
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("Sample quiz sets created successfully in " + folderPath);
        #endif
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SampleQuizSets))]
public class SampleQuizSetsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        SampleQuizSets quizCreator = (SampleQuizSets)target;
        
        EditorGUILayout.Space();
        if (GUILayout.Button("Create Sample Quiz Sets"))
        {
            // Set the toggle to true which will trigger creation in OnValidate
            SerializedProperty prop = serializedObject.FindProperty("createSampleQuizzes");
            prop.boolValue = true;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif 