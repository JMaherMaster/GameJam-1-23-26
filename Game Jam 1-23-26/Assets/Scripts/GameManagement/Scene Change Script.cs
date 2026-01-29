using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangeTrigger : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Name of the scene to load (must be in Build Settings)")]
    [SerializeField] private string sceneName;

    [Tooltip("Or use scene index instead (0 = first scene in Build Settings)")]
    [SerializeField] private int sceneIndex = -1;

    [Header("Trigger Settings")]
    [SerializeField] private bool useSceneName = true;

    [Header("Optional Effects")]
    [SerializeField] private AudioClip transitionSound;
    [SerializeField] private float delayBeforeLoad = 0f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private AudioSource audioSource;
    private bool hasTriggered = false;

    void Start()
    {
        // Make sure this collider is a trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogError("[SCENE TRIGGER] No collider found! Add a collider to this GameObject.");
        }

        // Setup audio source if we have a sound
        if (transitionSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if player entered
        if (other.CompareTag("Player") && !hasTriggered)
        {
            hasTriggered = true;

            if (showDebugLogs)
            {
                Debug.Log($"<color=cyan>[SCENE TRIGGER] Player entered! Loading scene...</color>");
            }

            // Play sound if available
            if (transitionSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(transitionSound);
            }

            // Load scene after delay
            if (delayBeforeLoad > 0f)
            {
                Invoke(nameof(LoadScene), delayBeforeLoad);
            }
            else
            {
                LoadScene();
            }
        }
    }

    void LoadScene()
    {
        if (useSceneName && !string.IsNullOrEmpty(sceneName))
        {
            if (showDebugLogs)
            {
                Debug.Log($"<color=green>[SCENE TRIGGER] Loading scene: {sceneName}</color>");
            }
            SceneManager.LoadScene(sceneName);
        }
        else if (!useSceneName && sceneIndex >= 0)
        {
            if (showDebugLogs)
            {
                Debug.Log($"<color=green>[SCENE TRIGGER] Loading scene index: {sceneIndex}</color>");
            }
            SceneManager.LoadScene(sceneIndex);
        }
        else
        {
            Debug.LogError("[SCENE TRIGGER] No valid scene name or index specified!");
        }
    }

    void OnDrawGizmos()
    {
        // Draw the trigger area in the editor
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Collider col = GetComponent<Collider>();

        if (col != null)
        {
            if (col is BoxCollider box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius);
            }
        }
    }
}