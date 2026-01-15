using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI References")]
    public Slider healthSlider;
    public Image healthFill;
    public Image damageOverlay; // Red flash overlay

    [Header("Health Bar Colors")]
    public Color fullHealthColor = Color.green;
    public Color midHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;

    [Header("Damage Flash Settings")]
    public float flashDuration = 0.3f;
    public Color flashColor = new Color(1f, 0f, 0f, 0.3f); // Red with transparency

    [Header("Debug")]
    public bool showDebugLogs = true;

    void Awake()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();

        // Make sure damage overlay starts invisible
        if (damageOverlay != null)
        {
            Color transparent = flashColor;
            transparent.a = 0f;
            damageOverlay.color = transparent;
        }

        if (showDebugLogs)
        {
            Debug.Log($"<color=cyan>[PLAYER HEALTH START] Initialized - Health: {currentHealth}/{maxHealth}</color>");
        }
    }

    void Update()
    {
        // Monitor for unexpected health changes
        if (showDebugLogs)
        {
            if (healthSlider != null && Mathf.Abs(healthSlider.value - ((float)currentHealth / maxHealth)) > 0.01f)
            {
                Debug.LogError($"<color=magenta>[HEALTH MISMATCH] Slider: {healthSlider.value:F2} vs Actual: {((float)currentHealth / maxHealth):F2} | Health: {currentHealth}/{maxHealth}</color>");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        int previousHealth = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (showDebugLogs)
        {
            Debug.Log($"<color=red>[PLAYER HEALTH] Took {damage} damage | {previousHealth} → {currentHealth} | Health Bar: {((float)currentHealth / maxHealth * 100f):F1}%</color>");
        }

        UpdateHealthBar();

        // Trigger damage flash effect
        StartCoroutine(DamageFlash());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        float healthPercent = (float)currentHealth / maxHealth;

        if (healthSlider != null)
        {
            healthSlider.value = healthPercent;

            if (showDebugLogs)
            {
                Debug.Log($"<color=yellow>[HEALTH BAR] Slider value set to: {healthPercent:F2} ({currentHealth}/{maxHealth})</color>");
            }
        }
        else
        {
            Debug.LogWarning("[HEALTH BAR] Health Slider is NULL! Assign it in the Inspector.");
        }

        if (healthFill != null)
        {
            // Change color based on health percentage
            if (healthPercent > 0.5f)
            {
                // Between 50% and 100% - transition from yellow to green
                healthFill.color = Color.Lerp(midHealthColor, fullHealthColor, (healthPercent - 0.5f) * 2f);
            }
            else
            {
                // Between 0% and 50% - transition from red to yellow
                healthFill.color = Color.Lerp(lowHealthColor, midHealthColor, healthPercent * 2f);
            }
        }
        else
        {
            Debug.LogWarning("[HEALTH BAR] Health Fill Image is NULL! Assign it in the Inspector.");
        }
    }

    void Die()
    {
        Debug.Log("<color=red>[PLAYER HEALTH] PLAYER DIED!</color>");
        // Add death logic
    }

    // Public method to check current health
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}