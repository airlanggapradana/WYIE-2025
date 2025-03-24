using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BookManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject bookPanel;
    [SerializeField] private TextMeshProUGUI bookTitleText;
    [SerializeField] private TextMeshProUGUI pageNumberText;
    [SerializeField] private TextMeshProUGUI pageContentText;
    [SerializeField] private Image pageImageDisplay;
    [SerializeField] private Image bookCoverImage;
    
    [Header("Navigation")]
    [SerializeField] private GameObject nextPagePrompt;
    [SerializeField] private GameObject prevPagePrompt;
    [SerializeField] private KeyCode closeBookKey = KeyCode.Return; // Enter key
    [SerializeField] private KeyCode nextPageKey = KeyCode.RightArrow;
    [SerializeField] private KeyCode prevPageKey = KeyCode.LeftArrow;
    
    [Header("Settings")]
    [SerializeField] private float pageTransitionSpeed = 0.5f;
    [SerializeField] private AudioClip pageFlipSound;
    [SerializeField] private AudioClip openBookSound;
    [SerializeField] private AudioClip closeBookSound;
    
    private BookData currentBook;
    private int currentPageIndex = 0;
    private bool isBookOpen = false;
    private AudioSource audioSource;
    private PlayerMovement playerMovement;
    
    // Public property to check if a book is open
    public bool IsBookOpen => isBookOpen;
    
    // Events to notify when book reading starts and ends
    public System.Action OnBookOpened;
    public System.Action OnBookClosed;
    
    private void Start()
    {
        // Hide book UI at start
        if (bookPanel != null)
        {
            bookPanel.SetActive(false);
        }
        
        // Hide navigation prompts
        if (nextPagePrompt != null) nextPagePrompt.SetActive(false);
        if (prevPagePrompt != null) prevPagePrompt.SetActive(false);
        
        // Get audio source component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (pageFlipSound != null || openBookSound != null || closeBookSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Find player movement component
        playerMovement = FindObjectOfType<PlayerMovement>();
        if (playerMovement == null)
        {
            Debug.LogWarning("PlayerMovement component not found. Movement control during book reading will not work.");
        }
    }
    
    private void Update()
    {
        if (!isBookOpen) return;
        
        // Handle book navigation
        if (Input.GetKeyDown(closeBookKey))
        {
            CloseBook();
        }
        else if (Input.GetKeyDown(nextPageKey) && currentPageIndex < currentBook.PageCount - 1)
        {
            TurnPage(1); // Go to next page
        }
        else if (Input.GetKeyDown(prevPageKey) && currentPageIndex > 0)
        {
            TurnPage(-1); // Go to previous page
        }
    }
    
    public void OpenBook(BookData bookData)
    {
        currentBook = bookData;
        currentPageIndex = 0;
        isBookOpen = true;
        
        // Disable player movement
        if (playerMovement != null)
        {
            playerMovement.SetControlsEnabled(false);
        }
        
        // Show book panel
        if (bookPanel != null)
        {
            bookPanel.SetActive(true);
        }
        
        // Display book title
        if (bookTitleText != null)
        {
            bookTitleText.text = bookData.BookTitle;
            if (!string.IsNullOrEmpty(bookData.BookAuthor))
            {
                bookTitleText.text += $" by {bookData.BookAuthor}";
            }
        }
        
        // Display book cover if available
        if (bookCoverImage != null && bookData.BookCover != null)
        {
            bookCoverImage.sprite = bookData.BookCover;
            bookCoverImage.gameObject.SetActive(true);
        }
        else if (bookCoverImage != null)
        {
            bookCoverImage.gameObject.SetActive(false);
        }
        
        // Display first page
        DisplayPage(currentPageIndex);
        
        // Play open book sound
        if (audioSource != null && openBookSound != null)
        {
            audioSource.PlayOneShot(openBookSound);
        }
        
        // Trigger book opened event
        OnBookOpened?.Invoke();
    }
    
    private void DisplayPage(int pageIndex)
    {
        if (pageIndex < 0 || pageIndex >= currentBook.PageCount)
        {
            Debug.LogWarning("Attempted to display invalid page index: " + pageIndex);
            return;
        }
        
        // Get current page
        BookPage page = currentBook.Pages[pageIndex];
        
        // Display page content
        if (pageContentText != null)
        {
            pageContentText.text = page.PageContent;
        }
        
        // Display page image if available
        if (pageImageDisplay != null)
        {
            if (page.PageImage != null)
            {
                pageImageDisplay.sprite = page.PageImage;
                pageImageDisplay.gameObject.SetActive(true);
            }
            else
            {
                pageImageDisplay.gameObject.SetActive(false);
            }
        }
        
        // Update page number text
        if (pageNumberText != null)
        {
            pageNumberText.text = $"Page {pageIndex + 1} of {currentBook.PageCount}";
        }
        
        // Update navigation prompts
        if (nextPagePrompt != null)
        {
            nextPagePrompt.SetActive(pageIndex < currentBook.PageCount - 1);
        }
        
        if (prevPagePrompt != null)
        {
            prevPagePrompt.SetActive(pageIndex > 0);
        }
    }
    
    private void TurnPage(int direction)
    {
        int newPageIndex = currentPageIndex + direction;
        
        if (newPageIndex >= 0 && newPageIndex < currentBook.PageCount)
        {
            // Play page flip sound
            if (audioSource != null && pageFlipSound != null)
            {
                audioSource.PlayOneShot(pageFlipSound);
            }
            
            // Animate page turning (if you want to add a simple animation)
            StartCoroutine(AnimatePageTurn(direction));
            
            currentPageIndex = newPageIndex;
            DisplayPage(currentPageIndex);
        }
    }
    
    private IEnumerator AnimatePageTurn(int direction)
    {
        // Simple fade animation for page turning
        if (pageContentText != null)
        {
            float startAlpha = 1f;
            float endAlpha = 0f;
            
            // Fade out
            float elapsedTime = 0f;
            Color originalColor = pageContentText.color;
            
            while (elapsedTime < pageTransitionSpeed / 2)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / (pageTransitionSpeed / 2);
                float currentAlpha = Mathf.Lerp(startAlpha, endAlpha, normalizedTime);
                pageContentText.color = new Color(originalColor.r, originalColor.g, originalColor.b, currentAlpha);
                
                if (pageImageDisplay != null && pageImageDisplay.gameObject.activeSelf)
                {
                    Color imgColor = pageImageDisplay.color;
                    pageImageDisplay.color = new Color(imgColor.r, imgColor.g, imgColor.b, currentAlpha);
                }
                
                yield return null;
            }
            
            // Fade in
            elapsedTime = 0f;
            while (elapsedTime < pageTransitionSpeed / 2)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / (pageTransitionSpeed / 2);
                float currentAlpha = Mathf.Lerp(endAlpha, startAlpha, normalizedTime);
                pageContentText.color = new Color(originalColor.r, originalColor.g, originalColor.b, currentAlpha);
                
                if (pageImageDisplay != null && pageImageDisplay.gameObject.activeSelf)
                {
                    Color imgColor = pageImageDisplay.color;
                    pageImageDisplay.color = new Color(imgColor.r, imgColor.g, imgColor.b, currentAlpha);
                }
                
                yield return null;
            }
            
            // Reset to original alpha
            pageContentText.color = originalColor;
            if (pageImageDisplay != null && pageImageDisplay.gameObject.activeSelf)
            {
                Color imgColor = pageImageDisplay.color;
                pageImageDisplay.color = new Color(imgColor.r, imgColor.g, imgColor.b, 1f);
            }
        }
    }
    
    public void CloseBook()
    {
        if (!isBookOpen) return;
        
        // Hide book panel
        if (bookPanel != null)
        {
            bookPanel.SetActive(false);
        }
        
        // Reset state
        isBookOpen = false;
        currentBook = null;
        
        // Re-enable player movement
        if (playerMovement != null)
        {
            playerMovement.SetControlsEnabled(true);
        }
        
        // Play close book sound
        if (audioSource != null && closeBookSound != null)
        {
            audioSource.PlayOneShot(closeBookSound);
        }
        
        // Trigger book closed event
        OnBookClosed?.Invoke();
    }
} 