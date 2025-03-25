using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// This script helps with creating a sample book in the scene for testing purposes.
/// It can be attached to a GameObject and will automatically set up the necessary components.
/// </summary>
public class SampleBook : MonoBehaviour
{
    [Header("Book Configuration")]
    [SerializeField] private BookData bookData;
    [SerializeField] private string bookName = "Sample Book";
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private LayerMask playerLayer;
    
    [Header("Visual Settings")]
    [SerializeField] private Color bookColor = Color.blue;
    [SerializeField] private Vector3 bookSize = new Vector3(0.5f, 0.7f, 0.1f);
    
    private BookInteraction bookInteraction;
    
    private void OnValidate()
    {
        if (bookData == null)
        {
            Debug.LogWarning("Please assign BookData to the sample book!");
        }
        
        // Update visual representation of the book
        UpdateBookVisual();
        
        // Update book interaction settings
        if (bookInteraction == null)
        {
            bookInteraction = GetComponent<BookInteraction>();
        }
        
        if (bookInteraction != null)
        {
            // Fields will be updated if BookInteraction component is added
        }
    }
    
    private void UpdateBookVisual()
    {
        // Update the visual representation based on the settings
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        
        // Set a default book sprite (white square that gets colored)
        if (spriteRenderer.sprite == null)
        {
            // Default to a basic sprite if available in the project
            spriteRenderer.sprite = Resources.Load<Sprite>("Square");
        }
        
        // Apply color
        spriteRenderer.color = bookColor;
        
        // Apply size
        transform.localScale = bookSize;
    }
    
    [ContextMenu("Setup Book")]
    public void SetupBook()
    {
        // Add required components if they don't exist
        if (GetComponent<BoxCollider2D>() == null)
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = Vector2.one; // Adjust based on sprite size
        }
        
        // Add BookInteraction component if it doesn't exist
        if (GetComponent<BookInteraction>() == null)
        {
            bookInteraction = gameObject.AddComponent<BookInteraction>();
            
            // Set default values from this component
            // Note: Due to how serialization works, these values won't be saved in the inspector
            // But they will be set temporarily for testing
            SerializedObject so = new SerializedObject(bookInteraction);
            so.FindProperty("bookData").objectReferenceValue = bookData;
            so.FindProperty("bookName").stringValue = bookName;
            so.FindProperty("interactionRadius").floatValue = interactionRadius;
            so.FindProperty("playerLayer").intValue = playerLayer.value;
            so.ApplyModifiedProperties();
        }
        
        Debug.Log("Book setup complete! Make sure to create an interaction prompt in the scene.");
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
        
        // Draw a book icon
        Vector3 iconSize = Vector3.one * 0.2f;
        Gizmos.DrawCube(transform.position, iconSize);
        
        // Draw pages
        Gizmos.color = Color.white;
        Gizmos.DrawCube(transform.position, new Vector3(iconSize.x * 0.8f, iconSize.y, iconSize.z * 0.1f));
    }
}

#if UNITY_EDITOR
// Custom editor to provide a button to quickly set up the book
[CustomEditor(typeof(SampleBook))]
public class SampleBookEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        SampleBook sampleBook = (SampleBook)target;
        
        GUILayout.Space(10);
        if (GUILayout.Button("Setup Book"))
        {
            sampleBook.SetupBook();
        }
    }
}
#endif 