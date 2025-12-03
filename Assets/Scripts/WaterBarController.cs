using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WaterBarController : MonoBehaviour
{
    public Image waterBarImage;
    public Sprite[] waterFrames;
    [Range(0.1f, 5f)]
    public float frameDelay = 3f;

    [HideInInspector]
    public float waterLevel = 1f; // 1 = plein, 0 = vide

    private int currentFrame = 0;

    void Start()
    {
        if (waterFrames.Length > 0 && waterBarImage != null)
            StartCoroutine(UpdateWaterBar());
    }

    IEnumerator UpdateWaterBar()
    {
        while (true)
        {
            // Update UI first to show current state
            currentFrame = Mathf.RoundToInt((1f - waterLevel) * (waterFrames.Length - 1));
            currentFrame = Mathf.Clamp(currentFrame, 0, waterFrames.Length - 1);
            waterBarImage.sprite = waterFrames[currentFrame];

            yield return new WaitForSeconds(frameDelay);

            // Then reduce water level for next iteration
            waterLevel -= 1f / (waterFrames.Length - 1);
            waterLevel = Mathf.Clamp01(waterLevel); // Empêche d'aller en dessous de 0
        }
    }

    public void Refill(float amount)
    {
        waterLevel = Mathf.Clamp01(waterLevel + amount);
    }
}
