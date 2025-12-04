using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GlobalSettings : MonoBehaviour
{
    public static GlobalSettings Instance;

    public GameObject lightsObject;

    // Add any variables you want here
    public float masterVolume = 1.0f;

    public bool IsUIVisible = false;
    public bool IsLightsOn = false;

    private Light2D lightsComponent;
    void Awake()
    {
        lightsComponent = lightsObject.GetComponent<Light2D>();
        // Singleton logic
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Already have one → delete new one
            return;
        }

        Instance = this;
        lightsComponent.intensity = 0f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            SetLightsState(!IsLightsOn);
        }
    }

    public void SetUIVisibility(bool isVisible)
    {
        IsUIVisible = isVisible;
    }

    public void SetLightsState(bool isOn)
    {
        IsLightsOn = isOn;
        lightsComponent.intensity = isOn ? 1.0f : 0f;
    }
}
