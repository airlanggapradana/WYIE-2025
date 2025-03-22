using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(RectTransform))]
public class DialogueChoiceButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText;
    
    private void Awake()
    {
        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }
    
    public void SetText(string text)
    {
        if (buttonText != null)
        {
            buttonText.text = text;
        }
    }
    
    public void SetSelected(bool isSelected, Color selectedColor, Color normalColor)
    {
        if (buttonText != null)
        {
            buttonText.color = isSelected ? selectedColor : normalColor;
        }
    }
} 