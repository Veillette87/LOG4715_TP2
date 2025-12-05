using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GlobalSettings : MonoBehaviour
{
    public static GlobalSettings Instance;

    // Add any variables you want here
    public float masterVolume = 1.0f;

    public bool IsUIVisible = false;

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
    public void SetUIVisibility(bool isVisible)
    {
        IsUIVisible = isVisible;
    }
}
