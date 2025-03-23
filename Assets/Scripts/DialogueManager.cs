using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject continuePrompt;
    [SerializeField] private GameObject choicesPanel;
    [SerializeField] private GameObject choiceButtonPrefab;
    
    [Header("Settings")]
    [SerializeField] private float textSpeed = 0.05f;
    [SerializeField] private KeyCode continueKey = KeyCode.Space;
    [SerializeField] private Color normalChoiceColor = Color.white;
    [SerializeField] private Color selectedChoiceColor = new Color(1f, 0.8f, 0.2f);
    
    private DialogueData currentDialogue;
    private int currentNodeIndex;
    private bool isDisplayingText;
    private bool dialogueActive;
    private Coroutine typeTextCoroutine;
    private List<GameObject> choiceItems = new List<GameObject>();
    private List<DialogueChoice> currentChoices = new List<DialogueChoice>();
    private int selectedChoiceIndex = 0;
    
    // Public property to check if dialogue is active
    public bool IsDialogueActive => dialogueActive;
    
    // Events to notify when dialogue starts and ends
    public System.Action OnDialogueStarted;
    public System.Action OnDialogueEnded;
    
    private PlayerMovement playerMovement;
    
    private void Start()
    {
        // Hide dialogue UI at start
        dialoguePanel.SetActive(false);
        
        if (continuePrompt != null)
        {
            continuePrompt.SetActive(false);
        }
        
        // Find player movement component
        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogWarning("PlayerMovement component not found. Movement control during dialogue will not work.");
        }
    }
    
    private void Update()
    {
        if (!dialogueActive) return;
        
        // If currently displaying text, allow skipping the text animation
        if (isDisplayingText && Input.GetKeyDown(continueKey))
        {
            CompleteTextDisplay();
            return;
        }
        
        // Check if we have choices displayed
        DialogueNode currentNode = null;
        if (currentNodeIndex >= 0 && currentNodeIndex < currentDialogue.DialogueNodes.Count)
        {
            currentNode = currentDialogue.DialogueNodes[currentNodeIndex];
        }
        
        if (!isDisplayingText && currentNode != null)
        {
            if (currentNode.HasChoices)
            {
                // Handle choice navigation with arrow keys
                if (Input.GetKeyDown(KeyCode.W))
                {
                    UpdateSelectedChoice(-1);
                }
                else if (Input.GetKeyDown(KeyCode.S))
                {
                    UpdateSelectedChoice(1);
                }
                else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    // Confirm choice with Enter
                    if (selectedChoiceIndex >= 0 && selectedChoiceIndex < currentChoices.Count)
                    {
                        SelectChoice(currentChoices[selectedChoiceIndex].NextNodeIndex);
                    }
                }
            }
            else if (Input.GetKeyDown(continueKey))
            {
                // If no choices, continue to next dialogue
                ContinueDialogue();
            }
        }
    }
    
    private void UpdateSelectedChoice(int direction)
    {
        // Change the selected choice index
        selectedChoiceIndex = Mathf.Clamp(selectedChoiceIndex + direction, 0, currentChoices.Count - 1);
        
        // Update visual feedback
        UpdateChoiceDisplay();
    }
    
    private void UpdateChoiceDisplay()
    {
        // Update the visual state of all choice items
        for (int i = 0; i < choiceItems.Count; i++)
        {
            DialogueChoiceButton choiceButton = choiceItems[i].GetComponent<DialogueChoiceButton>();
            TextMeshProUGUI choiceText = choiceItems[i].GetComponentInChildren<TextMeshProUGUI>();
            bool isSelected = (i == selectedChoiceIndex);
            
            if (choiceButton != null)
            {
                choiceButton.SetSelected(isSelected, selectedChoiceColor, normalChoiceColor);
            }
            
            if (choiceText != null)
            {
                // Add selection indicator
                if (isSelected)
                {
                    choiceText.text = "> " + currentChoices[i].ChoiceText;
                }
                else
                {
                    choiceText.text = "  " + currentChoices[i].ChoiceText;
                }
            }
        }
    }
    
    public void StartDialogue(DialogueData dialogue)
    {
        // If already in dialogue, end the current one first
        if (dialogueActive)
        {
            EndDialogue();
        }
        
        currentDialogue = dialogue;
        currentNodeIndex = 0;
        dialogueActive = true;
        
        // Disable player        ovement
        if (playerMovement != null)
        {
            playerMovement.SetControlsEnabled(false);
        }
        
        // Show dialogue panel
        dialoguePanel.SetActive(true);
        
        // Set NPC name
        if (npcNameText != null)
        {
            npcNameText.text = dialogue.NPCName;
        }
        
        // Trigger dialogue started event
        OnDialogueStarted?.Invoke();
        
        // Display first dialogue node
        DisplayDialogueNode(currentNodeIndex);
    }
    
    private void DisplayDialogueNode(int nodeIndex)
    {
        if (nodeIndex < 0 || nodeIndex >= currentDialogue.DialogueNodes.Count)
        {
            EndDialogue();
            return;
        }
        
        DialogueNode node = currentDialogue.DialogueNodes[nodeIndex];
        
        // Display text with typing effect
        if (typeTextCoroutine != null)
        {
            StopCoroutine(typeTextCoroutine);
        }
        typeTextCoroutine = StartCoroutine(TypeText(node.DialogueText));
        
        // If there are choices, set up choice buttons after text is fully displayed
        if (node.HasChoices)
        {
            continuePrompt.SetActive(false);
        }
    }
    
    private IEnumerator TypeText(string text)
    {
        isDisplayingText = true;
        dialogueText.text = "";
        
        // Hide choices and continue prompt while typing
        if (choicesPanel != null)
        {
            choicesPanel.SetActive(false);
        }
        if (continuePrompt != null)
        {
            continuePrompt.SetActive(false);
        }
        
        // Type each character one by one
        foreach (char c in text.ToCharArray())
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
        
        isDisplayingText = false;
        
        // Show continue prompt or choices after text is displayed
        DialogueNode currentNode = currentDialogue.DialogueNodes[currentNodeIndex];
        
        if (currentNode.HasChoices)
        {
            DisplayChoices(currentNode);
        }
        else if (continuePrompt != null)
        {
            continuePrompt.SetActive(true);
        }
    }
    
    private void CompleteTextDisplay()
    {
        if (typeTextCoroutine != null)
        {
            StopCoroutine(typeTextCoroutine);
        }
        
        // Make sure we have a valid node
        if (currentNodeIndex >= 0 && currentNodeIndex < currentDialogue.DialogueNodes.Count)
        {
            DialogueNode currentNode = currentDialogue.DialogueNodes[currentNodeIndex];
            dialogueText.text = currentNode.DialogueText;
            isDisplayingText = false;
            
            // Show continue prompt or choices after text is displayed
            if (currentNode.HasChoices)
            {
                DisplayChoices(currentNode);
            }
            else if (continuePrompt != null)
            {
                continuePrompt.SetActive(true);
            }
        }
    }
    
    private void DisplayChoices(DialogueNode node)
    {
        if (choicesPanel == null || choiceButtonPrefab == null) return;
        
        // Clear existing choice items
        foreach (GameObject item in choiceItems)
        {
            Destroy(item);
        }
        choiceItems.Clear();
        currentChoices.Clear();
        
        // Reset selection
        selectedChoiceIndex = 0;
        
        // Show choices panel
        choicesPanel.SetActive(true);
        
        // Store the node's choices
        currentChoices.AddRange(node.Choices);
        
        // Create a visual element for each choice
        foreach (DialogueChoice choice in node.Choices)
        {
            GameObject choiceObj = Instantiate(choiceButtonPrefab, choicesPanel.transform);
            
            // Get the DialogueChoiceButton component
            DialogueChoiceButton choiceButton = choiceObj.GetComponent<DialogueChoiceButton>();
            TextMeshProUGUI choiceText = choiceObj.GetComponentInChildren<TextMeshProUGUI>();
            
            if (choiceText != null)
            {
                // Add padding for selection indicator
                choiceText.text = "  " + choice.ChoiceText;
            }
            
            choiceItems.Add(choiceObj);
        }
        
        // Initialize the display with the first choice selected
        UpdateChoiceDisplay();
    }
    
    private void SelectChoice(int nextNodeIndex)
    {
        currentNodeIndex = nextNodeIndex;
        
        // Hide choices panel
        if (choicesPanel != null)
        {
            choicesPanel.SetActive(false);
        }
        
        // Display next dialogue node
        DisplayDialogueNode(currentNodeIndex);
    }
    
    private void ContinueDialogue()
    {
        DialogueNode currentNode = currentDialogue.DialogueNodes[currentNodeIndex];
        
        // Check if there's a next node
        if (currentNode.NextNodeIndex >= 0)
        {
            currentNodeIndex = currentNode.NextNodeIndex;
            DisplayDialogueNode(currentNodeIndex);
        }
        else
        {
            // End dialogue if there's no next node
            EndDialogue();
        }
    }
    
    private void EndDialogue()
    {
        dialogueActive = false;
        
        // Re-enable player movement
        if (playerMovement != null)
        {
            playerMovement.SetControlsEnabled(true);
        }
        
        // Hide dialogue panel
        dialoguePanel.SetActive(false);
        
        // Clear current dialogue
        currentDialogue = null;
        
        // Trigger dialogue ended event
        OnDialogueEnded?.Invoke();
    }
} 