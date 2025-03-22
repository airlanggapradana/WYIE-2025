using UnityEngine;
using TMPro;

public class InteractionPrompt : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string defaultPromptText = "Press [E] to Talk";
    [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);
    [SerializeField] private bool followTarget = true;
    
    private Transform target;
    private Canvas canvas;
    private RectTransform rectTransform;
    
    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        
        if (promptText != null)
        {
            promptText.text = defaultPromptText;
        }
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    public void SetPromptText(string text)
    {
        if (promptText != null)
        {
            promptText.text = text;
        }
    }
    
    private void LateUpdate()
    {
        if (!followTarget || target == null || canvas == null) return;
        
        // Convert world position to screen position
        Vector3 targetPositionViewport = Camera.main.WorldToViewportPoint(target.position + offset);
        
        // Check if target is in front of the camera
        if (targetPositionViewport.z > 0 && 
            targetPositionViewport.x > 0 && targetPositionViewport.x < 1 &&
            targetPositionViewport.y > 0 && targetPositionViewport.y < 1)
        {
            // Convert to canvas space
            Vector2 canvasPosition = new Vector2(
                targetPositionViewport.x * canvas.GetComponent<RectTransform>().sizeDelta.x,
                targetPositionViewport.y * canvas.GetComponent<RectTransform>().sizeDelta.y
            );
            
            // Position the prompt
            rectTransform.anchoredPosition = canvasPosition - (canvas.GetComponent<RectTransform>().sizeDelta * 0.5f);
            
            // Show the prompt
            gameObject.SetActive(true);
        }
        else
        {
            // Hide the prompt if target is not visible
            gameObject.SetActive(false);
        }
    }
} 