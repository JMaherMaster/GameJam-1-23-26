using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class WinSequence : MonoBehaviour
{
    [Header("=== STAGE 1: UFO CRASH CUTSCENE ===")]

    [Header("UFO Settings")]
    [Tooltip("The UFO GameObject that will fall")]
    [SerializeField] private GameObject ufoObject;
    [SerializeField] private float fallSpeed = 2f;
    [SerializeField] private float fallDuration = 3f;
    [SerializeField] private AnimationCurve fallCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Fire Effect")]
    [Tooltip("Fire/smoke particle effect on the UFO")]
    [SerializeField] private ParticleSystem fireEffect;
    [SerializeField] private bool startFireImmediately = true;

    [Header("Crash Target")]
    [Tooltip("The house GameObject the UFO crashes into")]
    [SerializeField] private GameObject houseObject;
    [SerializeField] private Vector3 crashOffset = Vector3.zero; // Offset from house position

    [Header("Explosion")]
    [Tooltip("Explosion effect to spawn at crash point")]
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private float explosionDelay = 0.2f; // Small delay after impact

    [Header("=== STAGE 2: WIN SCREEN ===")]

    [Header("Screen Fade")]
    [Tooltip("Image overlay for screen fade effect")]
    [SerializeField] private Image screenFadeImage;
    [SerializeField] private Color fadeStartColor = Color.clear;
    [SerializeField] private Color fadeEndColor = Color.white;
    [SerializeField] private float fadeDelay = 0.5f; // Delay after explosion
    [SerializeField] private float fadeDuration = 2f;

    [Header("Win Screen UI")]
    [Tooltip("The win screen UI panel to show")]
    [SerializeField] private GameObject winScreenUI;
    [SerializeField] private float delayBeforeWinScreen = 0.5f; // After fade completes

    [Header("Audio")]
    [SerializeField] private AudioClip victoryMusic;
    [SerializeField] private bool stopBackgroundMusic = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private bool autoStartOnEnable = true;

    private AudioSource audioSource;
    private Vector3 ufoStartPosition;
    private Vector3 crashPosition;

    void Start()
    {
        // Setup audio source
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Make sure win screen is hidden at start
        if (winScreenUI != null)
        {
            winScreenUI.SetActive(false);
        }

        // Make sure screen fade starts transparent
        if (screenFadeImage != null)
        {
            screenFadeImage.color = fadeStartColor;
        }

        // Validate references
        if (ufoObject == null)
        {
            Debug.LogError("[WIN SEQUENCE] UFO Object not assigned!");
        }
        if (houseObject == null)
        {
            Debug.LogError("[WIN SEQUENCE] House Object not assigned!");
        }
        if (winScreenUI == null)
        {
            Debug.LogError("[WIN SEQUENCE] Win Screen UI not assigned!");
        }

        // Start the sequence automatically
        if (autoStartOnEnable)
        {
            StartWinSequence();
        }
    }

    public void StartWinSequence()
    {
        if (showDebugLogs)
        {
            Debug.Log("<color=cyan>[WIN SEQUENCE] 🎬 Starting Win Sequence!</color>");
        }

        // Lock cursor during cutscene
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Stop background music if desired
        if (stopBackgroundMusic)
        {
            AudioListener.volume = 0.3f; // Lower background audio
        }

        StartCoroutine(WinSequenceCoroutine());
    }

    IEnumerator WinSequenceCoroutine()
    {
        // ============================================
        // STAGE 1: UFO CRASH CUTSCENE
        // ============================================

        if (showDebugLogs)
        {
            Debug.Log("<color=yellow>[WIN SEQUENCE] 💥 STAGE 1: UFO Crash Cutscene</color>");
        }

        // Store UFO start position
        if (ufoObject != null)
        {
            ufoStartPosition = ufoObject.transform.position;
        }

        // Calculate crash position (at house location + offset)
        if (houseObject != null)
        {
            crashPosition = houseObject.transform.position + crashOffset;
        }

        // Start fire effect on UFO
        if (fireEffect != null && startFireImmediately)
        {
            fireEffect.Play();
            if (showDebugLogs)
            {
                Debug.Log("<color=red>[WIN SEQUENCE] 🔥 Fire effect started!</color>");
            }
        }

        // Animate UFO falling
        yield return StartCoroutine(AnimateUFOFall());

        // Wait a tiny bit before explosion
        yield return new WaitForSeconds(explosionDelay);

        // Trigger explosion
        TriggerExplosion();

        // Wait for explosion to be visible
        yield return new WaitForSeconds(fadeDelay);

        // ============================================
        // STAGE 2: SCREEN FADE & WIN SCREEN
        // ============================================

        if (showDebugLogs)
        {
            Debug.Log("<color=yellow>[WIN SEQUENCE] 🏆 STAGE 2: Win Screen</color>");
        }

        // Fade the screen
        yield return StartCoroutine(FadeScreen());

        // Wait before showing win screen
        yield return new WaitForSeconds(delayBeforeWinScreen);

        // Show win screen UI
        ShowWinScreen();
    }

    IEnumerator AnimateUFOFall()
    {
        if (ufoObject == null)
        {
            Debug.LogWarning("[WIN SEQUENCE] UFO Object is null, skipping fall animation");
            yield break;
        }

        if (showDebugLogs)
        {
            Debug.Log("<color=cyan>[WIN SEQUENCE] UFO falling from sky...</color>");
        }

        float elapsed = 0f;

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;
            float curveT = fallCurve.Evaluate(t);

            // Lerp from start position to crash position
            ufoObject.transform.position = Vector3.Lerp(ufoStartPosition, crashPosition, curveT);

            // Rotate on Y axis (spin) while keeping X axis leaned at -6 degrees
            ufoObject.transform.Rotate(0f, Time.deltaTime * 50f, 0f);

            // Keep X rotation locked at -6 degrees (leaned over)
            Vector3 currentRotation = ufoObject.transform.eulerAngles;
            ufoObject.transform.eulerAngles = new Vector3(-6f, currentRotation.y, currentRotation.z);

            yield return null;
        }

        // Ensure UFO ends exactly at crash position
        ufoObject.transform.position = crashPosition;

        if (showDebugLogs)
        {
            Debug.Log("<color=orange>[WIN SEQUENCE] 💥 UFO CRASHED into house!</color>");
        }
    }

    void TriggerExplosion()
    {
        // Spawn explosion effect at crash position
        if (explosionEffect != null)
        {
            GameObject explosion = Instantiate(explosionEffect, crashPosition, Quaternion.identity);

            if (showDebugLogs)
            {
                Debug.Log("<color=red>[WIN SEQUENCE] 💥 EXPLOSION!</color>");
            }

            // Destroy explosion after some time
            Destroy(explosion, 5f);
        }

        // Play explosion sound
        if (explosionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }

        // Optional: Hide or destroy the UFO after explosion
        if (ufoObject != null)
        {
            // You can either hide it or destroy it
            ufoObject.SetActive(false);
            // Or: Destroy(ufoObject, 0.1f);
        }

        // Optional: Damage/shake the house
        if (houseObject != null)
        {
            // Add house destruction effects here if desired
        }
    }

    IEnumerator FadeScreen()
    {
        if (screenFadeImage == null)
        {
            Debug.LogWarning("[WIN SEQUENCE] Screen Fade Image not assigned, skipping fade");
            yield break;
        }

        if (showDebugLogs)
        {
            Debug.Log("<color=white>[WIN SEQUENCE] Screen fading...</color>");
        }

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            screenFadeImage.color = Color.Lerp(fadeStartColor, fadeEndColor, t);

            yield return null;
        }

        // Ensure final color
        screenFadeImage.color = fadeEndColor;

        if (showDebugLogs)
        {
            Debug.Log("<color=white>[WIN SEQUENCE] Screen fade complete!</color>");
        }
    }

    void ShowWinScreen()
    {
        if (winScreenUI != null)
        {
            winScreenUI.SetActive(true);

            if (showDebugLogs)
            {
                Debug.Log("<color=green>[WIN SEQUENCE] 🏆 WIN SCREEN DISPLAYED!</color>");
            }
        }

        // Play victory music
        if (victoryMusic != null && audioSource != null)
        {
            audioSource.loop = true;
            audioSource.clip = victoryMusic;
            audioSource.Play();
        }

        // Unlock cursor for menu interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (showDebugLogs)
        {
            Debug.Log("<color=lime>[WIN SEQUENCE] ✅ Sequence complete! Player can now interact with win screen.</color>");
        }
    }

    // Public method to manually trigger sequence (optional)
    public void TriggerWinSequence()
    {
        StartWinSequence();
    }
}