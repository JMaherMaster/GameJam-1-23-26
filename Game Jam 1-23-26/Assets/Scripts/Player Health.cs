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
    public Image lowHealthOverlay; // Permanent low health vignette

    [Header("Health Bar Colors")]
    public Color fullHealthColor = Color.green;
    public Color midHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;

    [Header("Damage Flash Settings")]
    public float flashDuration = 0.3f;
    public Color flashColor = new Color(1f, 0f, 0f, 0.3f); // Red with transparency

    [Header("Low Health Vignette Settings")]
    public int lowHealthThreshold = 20; // When vignette starts appearing
    public Color vignetteAt20Health = new Color(0.5f, 0f, 0f, 0.2f); // Light red at 20 HP
    public Color vignetteAt10Health = new Color(0.3f, 0f, 0f, 0.4f); // Darker red at 10 HP
    public Color vignetteAt0Health = new Color(0.2f, 0f, 0f, 0.7f);  // Very dark red at 0 HP
    public float vignetteFadeSpeed = 2f; // How fast the vignette fades in/out

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

        // Make sure low health overlay starts invisible
        if (lowHealthOverlay != null)
        {
            Color transparent = Color.clear;
            lowHealthOverlay.color = transparent;
        }

        if (showDebugLogs)
        {
            Debug.Log($"<color=cyan>[PLAYER HEALTH] Initialized - Health: {currentHealth}/{maxHealth}</color>");
        }
    }

    void Update()
    {
        // Update low health vignette
        UpdateLowHealthVignette();

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
        if (damageOverlay != null)
        {
            StartCoroutine(DamageFlashEffect());
        }

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
            if (healthPercent > 0.5f)
            {
                healthFill.color = Color.Lerp(midHealthColor, fullHealthColor, (healthPercent - 0.5f) * 2f);
            }
            else
            {
                healthFill.color = Color.Lerp(lowHealthColor, midHealthColor, healthPercent * 2f);
            }
        }
        else
        {
            Debug.LogWarning("[HEALTH BAR] Health Fill Image is NULL! Assign it in the Inspector.");
        }
    }

    IEnumerator DamageFlashEffect()
    {
        // Fade in quickly
        float elapsed = 0f;
        float fadeInTime = flashDuration * 0.2f; // 20% of duration for fade in

        while (elapsed < fadeInTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, flashColor.a, elapsed / fadeInTime);
            Color newColor = flashColor;
            newColor.a = alpha;
            damageOverlay.color = newColor;
            yield return null;
        }

        // Hold briefly
        damageOverlay.color = flashColor;
        yield return new WaitForSeconds(flashDuration * 0.2f);

        // Fade out
        elapsed = 0f;
        float fadeOutTime = flashDuration * 0.6f; // 60% of duration for fade out

        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(flashColor.a, 0f, elapsed / fadeOutTime);
            Color newColor = flashColor;
            newColor.a = alpha;
            damageOverlay.color = newColor;
            yield return null;
        }

        // Ensure it's fully transparent
        Color transparent = flashColor;
        transparent.a = 0f;
        damageOverlay.color = transparent;
    }

    void Die()
    {
        Debug.Log("<color=red>[PLAYER HEALTH] PLAYER DIED!</color>");
        // Add death logic
    }

    void UpdateLowHealthVignette()
    {
        if (lowHealthOverlay == null) return;

        Color targetColor = Color.clear;

        // Determine target color based on health
        if (currentHealth <= 0)
        {
            targetColor = vignetteAt0Health;
        }
        else if (currentHealth <= 10)
        {
            // Interpolate between 10 HP color and 0 HP color
            float t = (float)currentHealth / 10f;
            targetColor = Color.Lerp(vignetteAt0Health, vignetteAt10Health, t);
        }
        else if (currentHealth <= lowHealthThreshold)
        {
            // Interpolate between 20 HP color and 10 HP color
            float t = (float)(currentHealth - 10) / (lowHealthThreshold - 10);
            targetColor = Color.Lerp(vignetteAt10Health, vignetteAt20Health, t);
        }
        else
        {
            // Above threshold - fade out completely
            targetColor = Color.clear;
        }

        // Smoothly lerp to target color
        lowHealthOverlay.color = Color.Lerp(lowHealthOverlay.color, targetColor, Time.deltaTime * vignetteFadeSpeed);
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}