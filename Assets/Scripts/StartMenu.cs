using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button startButton;
    public Button helpButton;

    void Start()
    {
        startButton.onClick.AddListener(OnStartClicked);
        helpButton.onClick.AddListener(OnHelpClicked);
    }

    public void OnStartClicked()
    {
        SceneManager.LoadScene("Level_1");  // Replace with your game scene name
    }

    public void OnHelpClicked()
    {
        Debug.Log("Help clicked");
        // Optionally show a help panel or load a Help scene
    }
}
