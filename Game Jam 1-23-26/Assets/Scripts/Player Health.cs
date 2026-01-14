using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("UI References")]
    public Slider healthSlider;
    public Image healthFill;

    [Header("Health Bar Colors")]
    public Color fullHealthColor = Color.green;
    public Color midHealthColor = Color.yellow;
    public Color lowHealthColor = Color.red;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        Debug.Log("Player took " + damage + " damage. Health: " + currentHealth);

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        if (healthSlider != null)
        {
            healthSlider.value = (float)currentHealth / maxHealth;
        }

        if (healthFill != null)
        {
            // Change color based on health percentage
            float healthPercent = (float)currentHealth / maxHealth;

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
    }

    void Die()
    {
        Debug.Log("Player died!");
        // Add death logic
    }
}