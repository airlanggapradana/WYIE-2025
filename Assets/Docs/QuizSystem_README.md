# Quiz System for Boss Battles

This system implements an alternative battle mechanic where players must answer questions correctly to defeat bosses. Instead of traditional combat, the player's attacks are based on their ability to answer questions correctly.

## Features

- Quiz-based combat that challenges players' knowledge
- Multiple-choice questions with customizable difficulty
- Integration with existing health and damage systems
- Support for critical hits and variable damage
- Visual feedback for correct and incorrect answers
- Customizable UI elements with health bars

## Setup Guide

### 1. Add Required Components

1. Add the `QuizManager` script to a GameObject in your scene (preferably a manager object that persists between scenes).
2. Create a UI Canvas with a quiz panel and add the `QuizUI` script to it.
3. Configure the UI elements in the inspector.

### 2. Create Quiz Questions

There are two ways to create quiz questions:

#### A. Using the ScriptableObject approach:

1. In the Project window, right-click and select **Create → Quiz System → Quiz Set**
2. Name your quiz set and configure the questions in the inspector
3. Add multiple questions with their answers, explanations, and difficulty multipliers

#### B. Using the SampleQuizSets utility:

1. Add the `SampleQuizSets` script to any GameObject in your scene
2. Configure the output folder in the inspector
3. Click the "Create Sample Quiz Sets" button to generate example quiz sets

### 3. Configure Boss for Quiz Battles

1. On any boss using the `BossController` script, enable the "Use Quiz Battle" option
2. Assign your created quiz set to the "Boss Quiz Set" field
3. Adjust the "Quiz Detection Radius" to determine when the quiz battle triggers

### 4. UI Setup

Create a Canvas with the following elements and assign them in the QuizUI inspector:

- Quiz Panel (root GameObject with CanvasGroup)
- Question Text (TextMeshProUGUI)
- Answer Buttons (4 buttons with Text components)
- Feedback Text (TextMeshProUGUI)
- Player Health Slider
- Boss Health Slider
- Title Text (optional)

## How It Works

1. When a player enters a boss's quiz detection radius, the quiz battle begins
2. The game pauses and the quiz UI appears
3. The player navigates options using up/down arrow keys or W/S keys
4. The player selects an answer with Enter/Space
5. For correct answers:
   - The player "attacks" the boss
   - Damage is calculated based on the player's attack strength
   - Critical hits may occur randomly for extra damage
6. For incorrect answers:
   - The boss "attacks" the player
   - Damage is calculated based on question difficulty
   - An explanation may be shown to educate the player
7. The battle ends when either the player or boss health reaches zero

## Customization

### Modifying Damage Values

In the `QuizManager` script, you can adjust:

- `baseDamageOnCorrect`: Base damage when answering correctly
- `baseDamageOnIncorrect`: Base damage when answering incorrectly
- `criticalHitChance`: Probability of a critical hit (0.0-1.0)
- `criticalHitMultiplier`: Damage multiplier for critical hits

### Question Difficulty

Each question has a `difficultyMultiplier` that affects:

- Damage dealt when answering correctly
- Damage received when answering incorrectly

Higher values create higher-stakes questions.

### Visual Effects

You can assign prefabs to:

- `correctAnswerEffect`: Visual effect when answering correctly
- `wrongAnswerEffect`: Visual effect when answering incorrectly

These will be instantiated at the appropriate position during combat.

## Tips for Quiz Creation

1. Balance difficulty - mix easy and hard questions
2. Use the explanation field to provide educational content
3. Keep questions clear and concise
4. Use thematically appropriate questions for each boss
5. Consider creating different quiz sets for different game areas or knowledge domains

## Extending the System

The quiz system can be extended in the following ways:

1. Add support for different question types (image-based, true/false, etc.)
2. Implement timed questions where faster answers give bonus damage
3. Add power-ups or hints that can be used during quiz battles
4. Create a quiz editor for easy question creation in the Unity Editor
5. Add multiplayer support for cooperative or competitive quiz battles

## Troubleshooting

**Quiz UI doesn't appear:**

- Ensure the QuizManager is properly referencing the UI elements
- Check that the quiz panel is inactive at start
- Verify the boss has "Use Quiz Battle" enabled

**Questions don't load:**

- Check that quiz sets are properly assigned to the QuizManager
- Ensure quiz sets contain valid questions

**Navigation doesn't work:**

- Check that the game is properly focused to receive input
- Verify that input keys match those specified in the QuizManager

## Dependencies

This system integrates with:

- HealthSystem.cs
- AttackSystem.cs
- BossController.cs
- TMPro (TextMesh Pro)
- Unity UI system
