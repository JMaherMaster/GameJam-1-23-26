using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Name of the main gameplay scene to load")]
    [SerializeField] private string gameSceneName = "MainScene";

    [Tooltip("Or use scene index instead (set to -1 to use scene name)")]
    [SerializeField] private int gameSceneIndex = 1; // Usually 1 for main game

    [Header("Audio")]
    [SerializeField] private AudioClip buttonClickSound;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private AudioSource audioSource;

    void Start()
    {
        // Setup audio source for button sounds
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Make sure cursor is visible and unlocked for menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Reset time scale in case it was modified
        Time.timeScale = 1f;

        if (showDebugLogs)
        {
            Debug.Log("<color=cyan>[MAIN MENU] Main Menu loaded and ready!</color>");
        }
    }

    public void PlayGame()
    {
        if (showDebugLogs)
        {
            Debug.Log("<color=yellow>[MAIN MENU] *** PLAY BUTTON CLICKED ***</color>");
        }

        // Play button click sound
        if (buttonClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }

        // Load the game scene
        try
        {
            if (gameSceneIndex >= 0)
            {
                // Use scene index
                if (showDebugLogs)
                {
                    Debug.Log($"<color=green>[MAIN MENU] Loading game scene by INDEX: {gameSceneIndex}</color>");
                }
                SceneManager.LoadScene(gameSceneIndex);
            }
            else
            {
                // Use scene name
                if (string.IsNullOrEmpty(gameSceneName))
                {
                    Debug.LogError("[MAIN MENU] Game Scene Name is empty! Set it in the Inspector.");
                    // Fallback to index 1
                    SceneManager.LoadScene(1);
                }
                else
                {
                    if (showDebugLogs)
                    {
                        Debug.Log($"<color=green>[MAIN MENU] Loading game scene by NAME: '{gameSceneName}'</color>");
                    }
                    SceneManager.LoadScene(gameSceneName);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MAIN MENU] ERROR loading game scene: {e.Message}");
            Debug.LogError("[MAIN MENU] Make sure the scene is added to Build Settings!");
        }
    }

    // Optional: Quit game button (if you want to add it later)
    public void QuitGame()
    {
        if (showDebugLogs)
        {
            Debug.Log("<color=red>[MAIN MENU] *** QUIT GAME CLICKED ***</color>");
        }

        // Play button click sound
        if (buttonClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }

        Debug.Log("Quitting game...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}