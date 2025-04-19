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
    
        // Create History quiz set
        QuizSet historyQuiz = ScriptableObject.CreateInstance<QuizSet>();
        historyQuiz.quizTitle = "History Quiz";
        historyQuiz.quizDescription = "Test your knowledge of history with these questions about Nusantara archipelago.";
        historyQuiz.questions = new QuizQuestion[]
        {
            new QuizQuestion
            {
                questionText = "Which Europeans nation was the first who land in Nusantara archipelago?",
                answerChoices = new string[] { "Dutch", "France", "British", "Portuguese" },
                correctAnswerIndex = 3,
                difficultyMultiplier = 1.0f,
                explanationText = "Portuguese were the first Europeans to land in Nusantara archipelago."
            },
            new QuizQuestion
            {
                questionText = "Who was the name of the Dutch sailor that first landed in Banten?",
                answerChoices = new string[] { "Columbus", "Cornelis de houtman", "Ferdinand Magellan", "Monkey D Luffy" },
                correctAnswerIndex = 1,
                difficultyMultiplier = 1.0f,
                explanationText = "Cornelis de Houtman was the Dutch sailor who first landed in Banten."
            },
            new QuizQuestion
            {
                questionText = "What year did the Dutch arrived in Banten?",
                answerChoices = new string[] { "1596", "1911", "1569", "1659" },
                correctAnswerIndex = 0,
                difficultyMultiplier = 1.0f,
                explanationText = "The Dutch arrived in Banten in 1596."
            },
            new QuizQuestion
            {
                questionText = "What is VOC stand for?",
                answerChoices = new string[] { "Voice of corruption", "Vintage Old Company", "Vereenigde Oost-Indische Compagnie", "Valley of the Creek" },
                correctAnswerIndex = 2,
                difficultyMultiplier = 1.2f,
                explanationText = "VOC stands for Vereenigde Oost-Indische Compagnie, which was the Dutch East India Company."
            },
            new QuizQuestion
            {
                questionText = "One of the VOC purpose is?",
                answerChoices = new string[] { "To Monopolized trade in Dutch-East Indies", "To make an Allies with another kingdom", "To conquer the market", "To control South East Asia" },
                correctAnswerIndex = 0,
                difficultyMultiplier = 1.0f,
                explanationText = "One of the VOC's main purposes was to monopolize trade in the Dutch East Indies."
            }
        };
        
        // Create History quiz set 2
        QuizSet historyQuiz2 = ScriptableObject.CreateInstance<QuizSet>();
        historyQuiz2.quizTitle = "History Quiz 2";
        historyQuiz2.quizDescription = "More questions about the history of Nusantara archipelago and colonialism.";
        historyQuiz2.questions = new QuizQuestion[]
        {
            new QuizQuestion
            {
                questionText = "Who was not included as a general governor of the VOC?",
                answerChoices = new string[] { "J.P Coen", "Pieter Both", "Diederick Durven", "Rafless" },
                correctAnswerIndex = 3,
                difficultyMultiplier = 1.2f,
                explanationText = "Rafless (Thomas Stamford Raffles) was not a general governor of the VOC, but a British statesman who was Lieutenant-Governor of the Dutch East Indies during British rule."
            },
            new QuizQuestion
            {
                questionText = "Before renamed to Jakarta, in the colonialism era, Jakarta named was?",
                answerChoices = new string[] { "Kotawaringin", "Batavia", "Bavaria", "Siberia" },
                correctAnswerIndex = 1,
                difficultyMultiplier = 1.0f,
                explanationText = "Jakarta was named Batavia during the Dutch colonial period."
            },
            new QuizQuestion
            {
                questionText = "How many years the British take control the Dutch-East Indies?",
                answerChoices = new string[] { "6 years", "5 years", "4 years", "3 years" },
                correctAnswerIndex = 0,
                difficultyMultiplier = 1.1f,
                explanationText = "The British controlled the Dutch East Indies for 6 years (1811-1816)."
            },
            new QuizQuestion
            {
                questionText = "In 1511, Portuguese already conquer…?",
                answerChoices = new string[] { "Maluku", "Malacca", "Borneo", "Java" },
                correctAnswerIndex = 1,
                difficultyMultiplier = 1.0f,
                explanationText = "In 1511, Portuguese conquered Malacca. This first expedition brought them to the Malacca peninsula, and then they continued to Maluku."
            },
            new QuizQuestion
            {
                questionText = "What is 3G stand for?",
                answerChoices = new string[] { "Gold, Glory, Gospel", "Glow, Glass, Gas", "Grass, Gold, Growth", "Guinevere, Gospel, Gant" },
                correctAnswerIndex = 0,
                difficultyMultiplier = 1.0f,
                explanationText = "3G stands for Gold, Glory, and Gospel, which were the three main motives of European exploration and colonization."
            }
        };
        
        // Ensure the folder exists
        if (!System.IO.Directory.Exists(folderPath))
        {
            System.IO.Directory.CreateDirectory(folderPath);
        }
        
        // Save the quiz sets as assets
 
        AssetDatabase.CreateAsset(historyQuiz, $"{folderPath}/HistoryQuiz.asset");
        AssetDatabase.CreateAsset(historyQuiz2, $"{folderPath}/HistoryQuiz2.asset");
        
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