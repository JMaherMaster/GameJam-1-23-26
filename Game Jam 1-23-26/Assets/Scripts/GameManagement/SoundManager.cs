using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private bool loopBackgroundMusic = true;
    [SerializeField][Range(0f, 1f)] private float backgroundMusicVolume = 0.7f;

    [Header("Random Ambient Sounds")]
    [SerializeField] private AudioClip[] ambientSounds;
    [SerializeField] private float timeBetweenSounds = 30f;
    [SerializeField][Range(0f, 1f)] private float ambientSoundsVolume = 0.8f;
    [SerializeField] private bool playRandomSoundsOnStart = true;

    private AudioSource backgroundAudioSource;
    private AudioSource ambientAudioSource;
    private Coroutine randomSoundCoroutine;

    private void Awake()
    {
        // Create audio sources
        backgroundAudioSource = gameObject.AddComponent<AudioSource>();
        ambientAudioSource = gameObject.AddComponent<AudioSource>();

        // Configure background audio source
        backgroundAudioSource.loop = loopBackgroundMusic;
        backgroundAudioSource.volume = backgroundMusicVolume;
        backgroundAudioSource.playOnAwake = false;

        // Configure ambient audio source
        ambientAudioSource.loop = false;
        ambientAudioSource.volume = ambientSoundsVolume;
        ambientAudioSource.playOnAwake = false;
    }

    private void Start()
    {
        // Play background music if assigned
        if (backgroundMusic != null)
        {
            PlayBackgroundMusic();
        }

        // Start random sounds if enabled
        if (playRandomSoundsOnStart && ambientSounds != null && ambientSounds.Length > 0)
        {
            StartRandomSounds();
        }
    }

    public void PlayBackgroundMusic()
    {
        if (backgroundMusic != null && backgroundAudioSource != null)
        {
            backgroundAudioSource.clip = backgroundMusic;
            backgroundAudioSource.Play();
        }
        else
        {
            Debug.LogWarning("Background music or audio source not assigned!");
        }
    }

    public void StopBackgroundMusic()
    {
        if (backgroundAudioSource != null)
        {
            backgroundAudioSource.Stop();
        }
    }

    public void StartRandomSounds()
    {
        if (randomSoundCoroutine != null)
        {
            StopCoroutine(randomSoundCoroutine);
        }
        randomSoundCoroutine = StartCoroutine(PlayRandomSoundsCoroutine());
    }

    public void StopRandomSounds()
    {
        if (randomSoundCoroutine != null)
        {
            StopCoroutine(randomSoundCoroutine);
            randomSoundCoroutine = null;
        }
    }

    private IEnumerator PlayRandomSoundsCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeBetweenSounds);

            if (ambientSounds != null && ambientSounds.Length > 0)
            {
                PlayRandomAmbientSound();
            }
        }
    }

    private void PlayRandomAmbientSound()
    {
        // Filter out null clips
        int validClipsCount = 0;
        for (int i = 0; i < ambientSounds.Length; i++)
        {
            if (ambientSounds[i] != null)
            {
                validClipsCount++;
            }
        }

        if (validClipsCount == 0)
        {
            Debug.LogWarning("No valid ambient sound clips assigned!");
            return;
        }

        // Pick a random valid clip
        AudioClip randomClip = null;
        int attempts = 0;
        while (randomClip == null && attempts < 100)
        {
            int randomIndex = Random.Range(0, ambientSounds.Length);
            if (ambientSounds[randomIndex] != null)
            {
                randomClip = ambientSounds[randomIndex];
            }
            attempts++;
        }

        if (randomClip != null)
        {
            ambientAudioSource.PlayOneShot(randomClip);
        }
    }

    // Public methods to control volumes at runtime
    public void SetBackgroundMusicVolume(float volume)
    {
        backgroundMusicVolume = Mathf.Clamp01(volume);
        if (backgroundAudioSource != null)
        {
            backgroundAudioSource.volume = backgroundMusicVolume;
        }
    }

    public void SetAmbientSoundsVolume(float volume)
    {
        ambientSoundsVolume = Mathf.Clamp01(volume);
        if (ambientAudioSource != null)
        {
            ambientAudioSource.volume = ambientSoundsVolume;
        }
    }

    private void OnDestroy()
    {
        if (randomSoundCoroutine != null)
        {
            StopCoroutine(randomSoundCoroutine);
        }
    }
}