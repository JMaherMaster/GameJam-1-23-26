using UnityEngine;

public class MonsterHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("Player Reward")]
    [SerializeField] private int healthRewardOnDeath = 5;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;

    private MonsterAI monsterAI;
    private Animator animator;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        monsterAI = GetComponent<MonsterAI>();
        animator = GetComponent<Animator>();

        if (showDebugLogs)
        {
            Debug.Log($"<color=cyan>[MONSTER HEALTH] Initialized - Health: {currentHealth}/{maxHealth}</color>");
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        int previousHealth = currentHealth;
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        if (showDebugLogs)
        {
            Debug.Log($"<color=orange>[MONSTER HEALTH] Took {damage} damage | {previousHealth} → {currentHealth}</color>");
        }

        // Turn toward the attacker (player)
        TurnTowardPlayer();

        // Play hit reaction (if not dead)
        if (currentHealth > 0)
        {
            PlayHitReaction();
        }

        // Alert the AI that we've been hit (this applies hitstun)
        if (monsterAI != null && !isDead)
        {
            monsterAI.OnTakeDamage();
        }

        // Check for death
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void TurnTowardPlayer()
    {
        // Find player if we don't have reference
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Get direction to player
            Vector3 directionToPlayer = player.transform.position - transform.position;
            directionToPlayer.y = 0; // Keep rotation on Y axis only

            // Rotate to face player
            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = targetRotation;

                if (showDebugLogs)
                {
                    Debug.Log("<color=cyan>[MONSTER] Turned toward attacker!</color>");
                }
            }
        }
    }

    void PlayHitReaction()
    {
        // Play random gethit animation
        string[] hitAnimations = { "gethit1", "gethit2", "gethit3", "gethit4" };
        string randomHit = hitAnimations[Random.Range(0, hitAnimations.Length)];

        if (animator != null)
        {
            animator.Play(randomHit, 0, 0f);
        }

        if (showDebugLogs)
        {
            Debug.Log($"<color=yellow>[MONSTER] Playing hit reaction: {randomHit}</color>");
        }
    }

    void Die()
    {
        if (isDead) return;

        isDead = true;

        if (showDebugLogs)
        {
            Debug.Log("<color=red>[MONSTER] MONSTER DIED!</color>");
        }

        // Reward the player with health
        RewardPlayer();

        // Play random death animation
        string[] deathAnimations = { "death1", "death2", "death3", "death4" };
        string randomDeath = deathAnimations[Random.Range(0, deathAnimations.Length)];

        if (animator != null)
        {
            animator.Play(randomDeath, 0, 0f);
            // Stop the animator from auto-transitioning after death
            StartCoroutine(FreezeOnDeathAnimation(randomDeath));
        }

        // Disable AI
        if (monsterAI != null)
        {
            monsterAI.enabled = false;
        }

        // Disable rigidbody physics
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        // Optional: Destroy after some time, or leave the body
        // Destroy(gameObject, 10f);
    }

    void RewardPlayer()
    {
        // Find the player and give them health
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.GainHealth(healthRewardOnDeath);

                if (showDebugLogs)
                {
                    Debug.Log($"<color=green>[MONSTER] Player rewarded with {healthRewardOnDeath} health!</color>");
                }
            }
            else
            {
                Debug.LogWarning("[MONSTER] Player has no PlayerHealth component!");
            }
        }
    }

    System.Collections.IEnumerator FreezeOnDeathAnimation(string deathAnimName)
    {
        // Wait for the death animation to finish playing
        yield return new WaitForSeconds(0.1f); // Small delay to ensure animation starts

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Wait until we're actually in the death animation
        while (!stateInfo.IsName(deathAnimName))
        {
            yield return null;
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }

        // Wait for the animation to finish
        while (stateInfo.normalizedTime < 1.0f)
        {
            yield return null;
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }

        // Freeze the animator on the last frame
        animator.Play(deathAnimName, 0, 1f); // Play at the end (normalized time = 1)
        animator.speed = 0f; // Stop the animator completely

        if (showDebugLogs)
        {
            Debug.Log($"<color=grey>[MONSTER] Frozen on death animation: {deathAnimName}</color>");
        }
    }

    // Public getters
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead;
}