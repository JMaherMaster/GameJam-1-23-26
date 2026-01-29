using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class GameEventManager : MonoBehaviour
{
    [System.Serializable]
    public class GameEvent
    {
        [Header("Event Settings")]
        public string eventName = "Event 1";
        public bool eventCompleted = false;

        [Header("Trigger")]
        [Tooltip("The collider that triggers this event when player enters")]
        public Collider triggerZone;

        [Header("Audio")]
        [Tooltip("Sound to play when event starts")]
        public AudioClip startSound;
        [Range(0f, 1f)]
        public float soundVolume = 1f;

        [Header("Enemies to Kill")]
        [Tooltip("List of enemies that must be killed to complete this event")]
        public List<GameObject> enemiesToKill = new List<GameObject>();
        public int enemiesKilledCount = 0;

        [Header("Barriers")]
        [Tooltip("Objects to disable/enable when event completes (like invisible walls)")]
        public List<GameObject> barriersToDisable = new List<GameObject>();
        public List<GameObject> objectsToEnable = new List<GameObject>();

        [Header("Next Event")]
        [Tooltip("Automatically start the next event when this one completes?")]
        public bool autoStartNextEvent = false;

        [Header("Custom Actions")]
        [Tooltip("Additional custom actions to perform on event start")]
        public UnityEvent onEventStart;
        [Tooltip("Additional custom actions to perform on event complete")]
        public UnityEvent onEventComplete;

        [Header("UI Messages")]
        public string startMessage = "";
        public string completeMessage = "";
        public float messageDisplayTime = 3f;
    }

    [Header("Event List")]
    [SerializeField] private List<GameEvent> events = new List<GameEvent>();

    [Header("Settings")]
    [SerializeField] private int currentEventIndex = 0;
    [SerializeField] private bool showDebugLogs = true;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    private GameEvent currentEvent;
    private bool eventActive = false;

    void Start()
    {
        // Create audio source if not assigned
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Setup trigger zones
        SetupTriggers();

        // Start first event if it doesn't need a trigger
        if (events.Count > 0 && events[0].triggerZone == null)
        {
            StartEvent(0);
        }
    }

    void SetupTriggers()
    {
        for (int i = 0; i < events.Count; i++)
        {
            if (events[i].triggerZone != null)
            {
                // Make sure it's a trigger
                events[i].triggerZone.isTrigger = true;

                // Add event trigger component if needed
                EventTriggerZone triggerScript = events[i].triggerZone.GetComponent<EventTriggerZone>();
                if (triggerScript == null)
                {
                    triggerScript = events[i].triggerZone.gameObject.AddComponent<EventTriggerZone>();
                }

                int eventIndex = i; // Capture for closure
                triggerScript.SetEventManager(this, eventIndex);
            }
        }
    }

    void Update()
    {
        // Check if current event is active and track enemy kills
        if (eventActive && currentEvent != null && !currentEvent.eventCompleted)
        {
            CheckEnemyStatus();
        }
    }

    void CheckEnemyStatus()
    {
        if (currentEvent.enemiesToKill.Count == 0)
        {
            return;
        }

        // Count how many enemies are dead
        int deadCount = 0;
        int totalEnemies = currentEvent.enemiesToKill.Count;

        for (int i = 0; i < currentEvent.enemiesToKill.Count; i++)
        {
            GameObject enemy = currentEvent.enemiesToKill[i];

            if (enemy == null)
            {
                // Enemy GameObject is destroyed/null
                deadCount++;
            }
            else
            {
                MonsterHealth health = enemy.GetComponent<MonsterHealth>();
                if (health != null)
                {
                    if (health.IsDead())
                    {
                        deadCount++;
                        // Only log when the count changes
                        if (deadCount > currentEvent.enemiesKilledCount && showDebugLogs)
                        {
                            Debug.Log($"<color=yellow>[EVENT: {currentEvent.eventName}] ☠️ Enemy {enemy.name} confirmed DEAD!</color>");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[EVENT: {currentEvent.eventName}] Enemy {enemy.name} has no MonsterHealth component!");
                }
            }
        }

        // Update kill count if changed
        if (deadCount != currentEvent.enemiesKilledCount)
        {
            currentEvent.enemiesKilledCount = deadCount;

            if (showDebugLogs)
            {
                Debug.Log($"<color=yellow>[EVENT: {currentEvent.eventName}] ⚔️ PROGRESS: {deadCount}/{totalEnemies} enemies killed</color>");
            }
        }

        // Check if all enemies are dead
        if (deadCount >= totalEnemies && deadCount > 0)
        {
            if (showDebugLogs)
            {
                Debug.Log($"<color=green>[EVENT: {currentEvent.eventName}] 🎉 ALL ENEMIES DEFEATED! Completing event...</color>");
            }
            CompleteEvent();
        }
    }

    public void TriggerEvent(int eventIndex)
    {
        if (eventIndex < 0 || eventIndex >= events.Count)
        {
            Debug.LogError($"[EVENT MANAGER] Invalid event index: {eventIndex}");
            return;
        }

        if (events[eventIndex].eventCompleted)
        {
            if (showDebugLogs)
            {
                Debug.Log($"<color=grey>[EVENT MANAGER] Event {eventIndex} ({events[eventIndex].eventName}) already completed</color>");
            }
            return;
        }

        StartEvent(eventIndex);
    }

    void StartEvent(int eventIndex)
    {
        currentEventIndex = eventIndex;
        currentEvent = events[eventIndex];
        eventActive = true;

        if (showDebugLogs)
        {
            Debug.Log($"<color=cyan>[EVENT MANAGER] 🎬 Starting Event: {currentEvent.eventName}</color>");
            Debug.Log($"<color=cyan>[EVENT MANAGER] Enemies to kill: {currentEvent.enemiesToKill.Count}</color>");
            Debug.Log($"<color=cyan>[EVENT MANAGER] Barriers to disable: {currentEvent.barriersToDisable.Count}</color>");
        }

        // Play start sound
        if (currentEvent.startSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(currentEvent.startSound, currentEvent.soundVolume);
        }

        // Display start message
        if (!string.IsNullOrEmpty(currentEvent.startMessage))
        {
            ShowMessage(currentEvent.startMessage, currentEvent.messageDisplayTime);
        }

        // Invoke custom start actions
        currentEvent.onEventStart?.Invoke();

        // Disable trigger zone after entering
        if (currentEvent.triggerZone != null)
        {
            currentEvent.triggerZone.enabled = false;
            if (showDebugLogs)
            {
                Debug.Log($"<color=yellow>[EVENT MANAGER] Trigger zone disabled</color>");
            }
        }

        // If no enemies to kill, complete immediately
        if (currentEvent.enemiesToKill.Count == 0)
        {
            if (showDebugLogs)
            {
                Debug.Log($"<color=orange>[EVENT MANAGER] No enemies assigned - completing immediately</color>");
            }
            CompleteEvent();
        }
    }

    void CompleteEvent()
    {
        if (currentEvent.eventCompleted)
        {
            if (showDebugLogs)
            {
                Debug.Log($"<color=grey>[EVENT MANAGER] Event already marked as completed, skipping...</color>");
            }
            return;
        }

        currentEvent.eventCompleted = true;
        eventActive = false;

        if (showDebugLogs)
        {
            Debug.Log($"<color=green>[EVENT MANAGER] ✅ Event Complete: {currentEvent.eventName}</color>");
        }

        // Disable barriers
        if (showDebugLogs)
        {
            Debug.Log($"<color=magenta>[EVENT MANAGER] Disabling {currentEvent.barriersToDisable.Count} barriers...</color>");
        }

        foreach (GameObject barrier in currentEvent.barriersToDisable)
        {
            if (barrier != null)
            {
                barrier.SetActive(false);
                if (showDebugLogs)
                {
                    Debug.Log($"<color=yellow>[EVENT MANAGER] ❌ Disabled barrier: {barrier.name} (Active: {barrier.activeSelf})</color>");
                }
            }
            else
            {
                Debug.LogWarning($"[EVENT MANAGER] Barrier in list is NULL!");
            }
        }

        // Enable objects
        if (showDebugLogs)
        {
            Debug.Log($"<color=magenta>[EVENT MANAGER] Enabling {currentEvent.objectsToEnable.Count} objects...</color>");
        }

        foreach (GameObject obj in currentEvent.objectsToEnable)
        {
            if (obj != null)
            {
                obj.SetActive(true);
                if (showDebugLogs)
                {
                    Debug.Log($"<color=yellow>[EVENT MANAGER] ✅ Enabled object: {obj.name} (Active: {obj.activeSelf})</color>");
                }
            }
            else
            {
                Debug.LogWarning($"[EVENT MANAGER] Object to enable in list is NULL!");
            }
        }

        // Display complete message
        if (!string.IsNullOrEmpty(currentEvent.completeMessage))
        {
            ShowMessage(currentEvent.completeMessage, currentEvent.messageDisplayTime);
        }

        // Invoke custom complete actions
        currentEvent.onEventComplete?.Invoke();

        // Auto-start next event if enabled
        if (currentEvent.autoStartNextEvent && currentEventIndex + 1 < events.Count)
        {
            if (showDebugLogs)
            {
                Debug.Log($"<color=cyan>[EVENT MANAGER] Auto-starting next event...</color>");
            }
            StartEvent(currentEventIndex + 1);
        }
    }

    void ShowMessage(string message, float duration)
    {
        Debug.Log($"<color=white>[MESSAGE] 💬 {message}</color>");
        // TODO: Display on UI canvas if you have one
    }

    // Public methods for external control
    public void ForceCompleteCurrentEvent()
    {
        if (currentEvent != null)
        {
            CompleteEvent();
        }
    }

    public void ResetEvent(int eventIndex)
    {
        if (eventIndex >= 0 && eventIndex < events.Count)
        {
            events[eventIndex].eventCompleted = false;
            events[eventIndex].enemiesKilledCount = 0;
        }
    }

    public void ResetAllEvents()
    {
        foreach (var evt in events)
        {
            evt.eventCompleted = false;
            evt.enemiesKilledCount = 0;
        }
        currentEventIndex = 0;
        currentEvent = null;
        eventActive = false;
    }
}

// Helper component for trigger zones
public class EventTriggerZone : MonoBehaviour
{
    private GameEventManager eventManager;
    private int eventIndex;

    public void SetEventManager(GameEventManager manager, int index)
    {
        eventManager = manager;
        eventIndex = index;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"<color=lime>[TRIGGER] Player entered trigger zone for Event {eventIndex}</color>");
            if (eventManager != null)
            {
                eventManager.TriggerEvent(eventIndex);
            }
        }
    }
}