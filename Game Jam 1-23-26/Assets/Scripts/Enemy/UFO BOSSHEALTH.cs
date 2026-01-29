using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class UFO_BossHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("UI References")]
    [Tooltip("Drag the UFO Health Bar UI here")]
    [SerializeField] private GameObject ufoHealthBarUI;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image healthFill;

    [Header("Health Bar Colors")]
    [SerializeField] private Color fullHealthColor = Color.green;
    [SerializeField] private Color midHealthColor = Color.yellow;
    [SerializeField] private Color lowHealthColor = Color.red;

    [Header("Damage Trigger")]
    [Tooltip("The trigger collider that damages the UFO when player enters")]
    [SerializeField] private Collider damageTrigger;
    [SerializeField] private int damagePerHit = 10;
    [SerializeField] private float damageCooldow = 0.5f; // Prevent multiple hits at once

    [Header("Victory Scene")]
    [Tooltip("Name of the scene to load when UFO is defeated")]
    [SerializeField] private string victorySceneName = "VictoryScene";
    [SerializeField] private float delayBeforeSceneChange = 2f;

    [Header("Optional Effects")]
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private GameObject explosionEffect; // Optional explosion prefab

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private AudioSource audioSource;
    private bool isDead = false;
    private bool canTakeDamage = true;

    void OnEnable()
    {
        // Initialize health
        currentHealth = maxHealth;
        isDead = false;
        canTakeDamage = true;

        // ENABLE the UFO Health Bar UI
        if (ufoHealthBarUI != null)
        {
            ufoHealthBarUI.SetActive(true);

            if (showDebugLogs)
            {
                Debug.Log("<color=cyan>[UFO BOSS] Health Bar UI ENABLED!</color>");
            }
        }
        else
        {
            Debug.LogWarning("[UFO BOSS] UFO Health Bar UI not assigned in Inspector!");
        }

        // Update health bar to full
        UpdateHealthBar();

        // Setup audio source
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // Make sure trigger is set up correctly
        if (damageTrigger != null)
        {
            damageTrigger.isTrigger = true;

            // Add the trigger component if it doesn't exist
            UFODamageTrigger triggerScript = damageTrigger.GetComponent<UFODamageTrigger>();
            if (triggerScript == null)
            {
                triggerScript = damageTrigger.gameObject.AddComponent<UFODamageTrigger>();
            }
            triggerScript.SetUFOBoss(this);

            if (showDebugLogs)
            {
                Debug.Log("<color=green>[UFO BOSS] Damage trigger setup complete!</color>");
            }
        }
        else
        {
            Debug.LogWarning("[UFO BOSS] Damage Trigger not assigned in Inspector!");
        }

        if (showDebugLogs)
        {
            Debug.Log($"<color=cyan>[UFO BOSS] Initialized - Health: {currentHealth}/{maxHealth}</color>");
        }
    }

    void OnDisable()
    {
        // Hide health bar when UFO is disabled
        if (ufoHealthBarUI != null)
        {
            ufoHealthBarUI.SetActive(false);

            if (showDebugLogs)
            {
                Debug.Log("<color=grey>[UFO BOSS] Health Bar UI DISABLED!</color>");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || !canTakeDamage) return;

        int previousHealth = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (showDebugLogs)
        {
            Debug.Log($"<color=orange>[UFO BOSS] Took {damage} damage | {previousHealth} → {currentHealth}</color>");
        }

        // Update health bar
        UpdateHealthBar();

        // Play damage sound
        if (damageSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(damageSound);
        }

        // Start cooldown to prevent multiple hits
        StartCoroutine(DamageCooldown());

        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator DamageCooldown()
    {
        canTakeDamage = false;
        yield return new WaitForSeconds(damageCooldow);
        canTakeDamage = true;
    }

    void UpdateHealthBar()
    {
        float healthPercent = Mathf.Clamp01((float)currentHealth / maxHealth);

        // Update slider
        if (healthSlider != null)
        {
            healthSlider.value = healthPercent;

            if (showDebugLogs)
            {
                Debug.Log($"<color=yellow>[UFO BOSS] Health Bar: {healthPercent:P0} ({currentHealth}/{maxHealth})</color>");
            }
        }

        // Update color
        if (healthFill != null)
        {
            if (healthPercent > 0.5f)
            {
                // Green to Yellow (100% to 50%)
                healthFill.color = Color.Lerp(midHealthColor, fullHealthColor, (healthPercent - 0.5f) * 2f);
            }
            else
            {
                // Yellow to Red (50% to 0%)
                healthFill.color = Color.Lerp(lowHealthColor, midHealthColor, healthPercent * 2f);
            }
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        if (showDebugLogs)
        {
            Debug.Log("<color=red>[UFO BOSS] UFO DESTROYED!</color>");
        }

        // Play death sound
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // Spawn explosion effect
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Disable the damage trigger
        if (damageTrigger != null)
        {
            damageTrigger.enabled = false;
        }

        // Load victory scene after delay
        StartCoroutine(LoadVictoryScene());
    }

    IEnumerator LoadVictoryScene()
    {
        if (showDebugLogs)
        {
            Debug.Log($"<color=green>[UFO BOSS] Loading victory scene in {delayBeforeSceneChange} seconds...</color>");
        }

        yield return new WaitForSeconds(delayBeforeSceneChange);

        // Load the victory scene
        if (!string.IsNullOrEmpty(victorySceneName))
        {
            if (showDebugLogs)
            {
                Debug.Log($"<color=cyan>[UFO BOSS] Loading scene: {victorySceneName}</color>");
            }

            try
            {
                SceneManager.LoadScene(victorySceneName);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[UFO BOSS] Failed to load scene '{victorySceneName}': {e.Message}");
                Debug.LogError("[UFO BOSS] Make sure the scene is added to Build Settings!");
            }
        }
        else
        {
            Debug.LogError("[UFO BOSS] Victory Scene Name is empty! Set it in the Inspector.");
        }
    }

    // Public getters
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead;
}

// Helper component for the damage trigger
public class UFODamageTrigger : MonoBehaviour
{
    private UFO_BossHealth ufoBoss;

    public void SetUFOBoss(UFO_BossHealth boss)
    {
        ufoBoss = boss;
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if player entered the trigger
        if (other.CompareTag("Player"))
        {
            Debug.Log("<color=yellow>[UFO TRIGGER] Player hit the damage trigger!</color>");

            if (ufoBoss != null)
            {
                ufoBoss.TakeDamage(10); // This will use the damage value from the boss script
            }
        }
    }
}