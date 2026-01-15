using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreen : MonoBehaviour
{
    public void RestartGame()
    {
        // Reload the current scene
        Time.timeScale = 1f; // Reset time scale in case it was paused
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ReturnToMenu()
    {
        // Load the main menu scene (index 0 by default)
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