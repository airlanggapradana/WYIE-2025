# Dialogue System Setup Guide

This guide will walk you through setting up the dialogue system with choices in Unity.

## 1. Set Up Prefabs

### Choice Button Prefab

1. Create a new Button UI element in your Canvas (Right-click in Hierarchy → UI → Button)
2. Add a TextMeshPro component to the Button (if it doesn't have one already)
3. Add the `DialogueChoiceButton` script to the Button object
4. Set the `Button Text` reference to your TextMeshPro component
5. Save this as a prefab by dragging it into your Project folder

### Dialogue UI Panel

1. Create a new Panel UI element in your Canvas for the dialogue panel
2. Add the following children to the panel:
   - TextMeshPro for NPC Name
   - TextMeshPro for Dialogue Text
   - GameObject for Continue Prompt (with a TextMeshPro showing "Press Space to continue")
   - GameObject for Choices Panel (this will hold the choice buttons)

## 2. Set Up Dialogue Manager

1. Create an empty GameObject in your scene and name it "DialogueManager"
2. Add the `DialogueManager` script to this GameObject
3. Assign references in the Inspector:
   - Dialogue Panel: Your dialogue UI panel
   - NPC Name Text: The TextMeshPro for NPC name
   - Dialogue Text: The TextMeshPro for dialogue text
   - Continue Prompt: The continue prompt GameObject
   - Choices Panel: The panel that will hold choice buttons
   - Choice Button Prefab: The button prefab you created earlier

## 3. Create Dialogue Data

1. Right-click in Project window → Create → Dialogue → Dialogue Data
2. Fill in the NPC name and ID
3. Add dialogue nodes:
   - For regular dialogue, set the next node index
   - For dialogue with choices:
     - Enable "Has Choices"
     - Add choice items by increasing the size of the Choices array
     - For each choice, set the choice text and next node index

## 4. Set Up NPC

1. Add the `NPCInteraction` script to your NPC GameObject
2. Assign the DialogueData you created to the NPC
3. Configure the interaction settings:
   - Interaction Radius: How close the player needs to be to interact
   - Interaction Key: Which key triggers the dialogue (default is E)
   - Player Layer: Set to the layer your player is on

## 5. Interaction Prompt

1. Create a UI element for the interaction prompt
2. Add the `InteractionPrompt` script to it
3. Assign it to the `Interaction Prompt` field in the NPCInteraction component

## 6. Testing Choices

To verify your choice system is working:

1. Make sure your choice button prefab has:

   - The `DialogueChoiceButton` script attached
   - A proper Button component
   - A TextMeshPro component for the text

2. In the DialogueManager inspector, verify:

   - Choices Panel is assigned
   - Choice Button Prefab is assigned

3. In your DialogueData:

   - Make sure at least one node has "Has Choices" enabled
   - Make sure each choice has a valid "Next Node Index"

4. When running the game, check the Console for debug messages:
   - "Choice selected, moving to node index: X" should appear when clicking a choice
   - If not, there might be an issue with the button setup

## Common Issues and Solutions

### Buttons Don't Respond to Clicks

- Make sure the Button component has Interactable checked
- Verify the DialogueChoiceButton script is attached
- Check that the Canvas uses a proper EventSystem
- Make sure the choices panel is not blocked by other UI elements

### Dialogue Doesn't Progress After Clicking Choice

- Check the Console for errors
- Make sure the next node index is valid (exists in the dialogue data)
- Verify that the choice button is properly set up with the correct event handlers

### Choices Don't Appear

- Make sure "Has Choices" is enabled for the dialogue node
- Check that the choices panel is assigned in the DialogueManager
- Verify the choice button prefab is assigned in the DialogueManager
- Make sure the choices array has at least one item
