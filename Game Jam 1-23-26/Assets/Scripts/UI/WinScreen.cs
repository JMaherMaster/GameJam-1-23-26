using UnityEngine;
using UnityEngine.SceneManagement;

public class WinScreen : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Name of the main menu scene")]
    [SerializeField] private string menuSceneName = "MainMenu";

    [Tooltip("Or use scene index instead (set to -1 to use scene name)")]
    [SerializeField] private int menuSceneIndex = 0; // Usually 0 for main menu

    [Header("Audio")]
    [SerializeField] private AudioClip buttonClickSound;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private AudioSource audioSource;

    void Start()
    {
        // Setup audio source for button sounds
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Make sure cursor is visible for menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (showDebugLogs)
        {
            Debug.Log("<color=green>[WIN SCREEN] Win Screen ready! Congratulations to the player! 🎉</color>");
        }
    }

    public void ReturnToMainMenu()
    {
        if (showDebugLogs)
        {
            Debug.Log("<color=yellow>[WIN SCREEN] *** RETURN TO MENU CLICKED ***</color>");
        }

        // Play button click sound
        if (buttonClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }

        // Keep cursor visible for menu scene
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Reset time scale in case it was modified
        Time.timeScale = 1f;

        // Load the menu scene
        try
        {
            if (menuSceneIndex >= 0)
            {
                // Use scene index
                if (showDebugLogs)
                {
                    Debug.Log($"<color=green>[WIN SCREEN] Loading menu scene by INDEX: {menuSceneIndex}</color>");
                }
                SceneManager.LoadScene(menuSceneIndex);
            }
            else
            {
                // Use scene name
                if (string.IsNullOrEmpty(menuSceneName))
                {
                    Debug.LogError("[WIN SCREEN] Menu Scene Name is empty! Set it in the Inspector.");
                    // Fallback to index 0
                    SceneManager.LoadScene(0);
                }
                else
                {
                    if (showDebugLogs)
                    {
                        Debug.Log($"<color=green>[WIN SCREEN] Loading menu scene by NAME: '{menuSceneName}'</color>");
                    }
                    SceneManager.LoadScene(menuSceneName);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[WIN SCREEN] ERROR loading menu scene: {e.Message}");
            Debug.LogError("[WIN SCREEN] Make sure the scene is added to Build Settings!");
        }
    }

    // Optional: Quit game button (uncomment if you want to add it)
    /*
    public void QuitGame()
    {
        if (showDebugLogs)
        {
            Debug.Log("<color=red>[WIN SCREEN] *** QUIT GAME CLICKED ***</color>");
        }

        Debug.Log("Quitting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    */
}