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

    private bool playerInTrigger = false;
    private GameEvent currentEvent;

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
        if (currentEvent != null && !currentEvent.eventCompleted)
        {
            CheckEnemyStatus();
        }
    }

    void CheckEnemyStatus()
    {
        if (currentEvent.enemiesToKill.Count == 0) return;

        // Count how many enemies are dead
        int deadCount = 0;
        foreach (GameObject enemy in currentEvent.enemiesToKill)
        {
            if (enemy == null) // Destroyed = dead
            {
                deadCount++;
            }
            else
            {
                MonsterHealth health = enemy.GetComponent<MonsterHealth>();
                if (health != null && health.IsDead())
                {
                    deadCount++;
                }
            }
        }

        // Update kill count
        if (deadCount != currentEvent.enemiesKilledCount)
        {
            currentEvent.enemiesKilledCount = deadCount;

            if (showDebugLogs)
            {
                Debug.Log($"<color=yellow>[EVENT: {currentEvent.eventName}] Enemies killed: {deadCount}/{currentEvent.enemiesToKill.Count}</color>");
            }
        }

        // Check if all enemies are dead
        if (deadCount >= currentEvent.enemiesToKill.Count)
        {
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
                Debug.Log($"<color=grey>[EVENT MANAGER] Event {eventIndex} already completed</color>");
            }
            return;
        }

        StartEvent(eventIndex);
    }

    void StartEvent(int eventIndex)
    {
        currentEventIndex = eventIndex;
        currentEvent = events[eventIndex];

        if (showDebugLogs)
        {
            Debug.Log($"<color=cyan>[EVENT MANAGER] Starting Event: {currentEvent.eventName}</color>");
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
        }
    }

    void CompleteEvent()
    {
        if (currentEvent.eventCompleted) return;

        currentEvent.eventCompleted = true;

        if (showDebugLogs)
        {
            Debug.Log($"<color=green>[EVENT MANAGER] Event Complete: {currentEvent.eventName}</color>");
        }

        // Disable barriers
        foreach (GameObject barrier in currentEvent.barriersToDisable)
        {
            if (barrier != null)
            {
                barrier.SetActive(false);
                if (showDebugLogs)
                {
                    Debug.Log($"<color=yellow>[EVENT MANAGER] Disabled barrier: {barrier.name}</color>");
                }
            }
        }

        // Enable objects
        foreach (GameObject obj in currentEvent.objectsToEnable)
        {
            if (obj != null)
            {
                obj.SetActive(true);
                if (showDebugLogs)
                {
                    Debug.Log($"<color=yellow>[EVENT MANAGER] Enabled object: {obj.name}</color>");
                }
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
            StartEvent(currentEventIndex + 1);
        }
    }

    void ShowMessage(string message, float duration)
    {
        // This is a placeholder - you can implement your own UI message system
        Debug.Log($"<color=white>[MESSAGE] {message}</color>");

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
            if (eventManager != null)
            {
                eventManager.TriggerEvent(eventIndex);
            }
        }
    }
}