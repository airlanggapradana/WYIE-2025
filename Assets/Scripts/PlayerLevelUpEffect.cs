using UnityEngine;
using TMPro;

public class PlayerLevelUpEffect : MonoBehaviour
{
    [SerializeField] private float animationDuration = 2f;
    [SerializeField] private float moveUpSpeed = 1f;
    [SerializeField] private float fadeSpeed = 1f;
    [SerializeField] private TextMeshProUGUI levelUpText;
    
    private float timer = 0f;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    
    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        // Initialize
        canvasGroup.alpha = 1f;
    }
    
    private void Start()
    {
        // Set text content if component exists
        if (levelUpText != null)
        {
            levelUpText.text = "LEVEL UP!";
        }
    }
    
    private void Update()
    {
        // Update timer
        timer += Time.deltaTime;
        
        // Move up
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition += Vector2.up * moveUpSpeed * Time.deltaTime;
        }
        
        // Fade out based on progression
        if (canvasGroup != null)
        {
            float normalizedTime = timer / animationDuration;
            
            // Start fading after 50% of animation has played
            if (normalizedTime > 0.5f)
            {
                float fadeAmount = (normalizedTime - 0.5f) * 2f; // Normalize 0.5-1 range to 0-1
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeAmount * fadeSpeed);
            }
        }
        
        // Destroy when timer is up
        if (timer >= animationDuration)
        {
            Destroy(gameObject);
        }
    }
} 