using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class DialogueChoice
{
    [SerializeField] private string choiceText;
    [SerializeField] private int nextNodeIndex; // Index of the next dialogue node if this choice is selected
    
    public string ChoiceText => choiceText;
    public int NextNodeIndex => nextNodeIndex;
}

[Serializable]
public class DialogueNode
{
    [SerializeField] private string dialogueText;
    [SerializeField] private int nextNodeIndex = -1; // -1 means end dialogue, otherwise go to the specified index
    [SerializeField] private bool hasChoices = false;
    [SerializeField] private List<DialogueChoice> choices = new List<DialogueChoice>();
    
    public string DialogueText => dialogueText;
    public int NextNodeIndex => nextNodeIndex;
    public bool HasChoices => hasChoices;
    public List<DialogueChoice> Choices => choices;
}

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [SerializeField] private string npcID;
    [SerializeField] private string npcName;
    [SerializeField] private List<DialogueNode> dialogueNodes = new List<DialogueNode>();
    
    public string NPCID => npcID;
    public string NPCName => npcName;
    public List<DialogueNode> DialogueNodes => dialogueNodes;
} 