using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreen : MonoBehaviour
{
    [Header("Scene Names")]
    [Tooltip("Name of the main gameplay scene to restart to")]
    [SerializeField] private string mainSceneName = "MainScene"; // Change this to match your scene name

    public void RestartGame()
    {
        // Always load the Main Scene (no matter what scene the player died in)
        // Cursor will be re-locked automatically when CharController_Motor starts
        Time.timeScale = 1f; // Reset time scale in case it was paused

        // Load by scene name
        SceneManager.LoadScene(mainSceneName);

        // Alternative: Load by scene index (if you prefer)
        // SceneManager.LoadScene(1); // Change to your main scene's build index
    }

    public void ReturnToMenu()
    {
        // Load the main menu scene (index 0 by default)
        // Keep cursor visible for menu scene
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f; // Reset time scale
        SceneManager.LoadScene(0); // Change to your menu scene index if different
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
