using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
    public static GlobalSettings Instance;

    public GameObject lightsObject;

    // Add any variables you want here
    public float masterVolume = 1.0f;

    public bool IsUIVisible = false;
    public bool IsLightsOn = true;
    void Awake()
    {
        // Singleton logic
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Already have one → delete new one
            return;
        }

        Instance = this;
        lightsObject.SetActive(false);
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
        lightsObject.SetActive(!isOn);
    }
}
