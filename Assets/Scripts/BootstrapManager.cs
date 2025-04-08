using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapManager : MonoBehaviour
{
    [SerializeField] private GameObject levelManagerPrefab;
    [SerializeField] private string firstSceneName = "MainMenu";
    
    void Awake()
    {
        // Make sure we have a LevelManager
        if (LevelManager.Instance == null && levelManagerPrefab != null)
        {
            Instantiate(levelManagerPrefab);
        }
        
        // Load the first real scene
        SceneManager.LoadScene(firstSceneName);
    }
}

