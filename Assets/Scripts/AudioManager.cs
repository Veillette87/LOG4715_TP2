using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public AudioSource generalSource;
    void Awake()
    {
        // Singleton logic
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Already have one → delete new one
            return;
        }

        Instance = this;
    }
    public void PlaySoundClip(AudioClip clip, float volume = 1.0f)
    {
        if (generalSource != null)
        {
            generalSource.PlayOneShot(clip, volume);
        }
    }
}
