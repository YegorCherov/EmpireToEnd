using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject settingsMenuPanel;

    // Triggered when 'Start New Game' is clicked
    public void StartNewGame()
    {
        SceneManager.LoadScene("GameScene"); // Replace with your game scene name
    }

    // Triggered when 'Load Game' is clicked
    public void LoadGame()
    {
        // Implement your load game functionality
    }

    // Triggered when 'Settings/Options' is clicked
    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsMenuPanel.SetActive(true);
    }

    // Triggered when 'Exit' is clicked
    public void ExitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #elif UNITY_WEBPLAYER
        Application.OpenURL(webplayerQuitURL);
        #else
        Application.Quit();
        #endif
    }
}
