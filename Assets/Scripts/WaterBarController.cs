using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WaterBarController : MonoBehaviour
{
    public Image waterBarImage;      // Image UI à afficher
    public Sprite[] waterFrames;     // Les 30 images de la barre
    [Range(0.1f, 2f)]
    public float frameDelay = 1f;    // Temps entre chaque frame (x secondes) — réglable dans l’inspecteur

    [HideInInspector]
    public float waterLevel = 1f;    // Niveau d’eau (1 = plein, 0 = vide)

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
            yield return new WaitForSeconds(frameDelay);

            // 🔹 Diminuer le niveau d’eau progressivement
            waterLevel -= 1f / (waterFrames.Length - 1);
            waterLevel = Mathf.Clamp01(waterLevel); // Empêche d’aller en dessous de 0

            // 🔹 Met à jour le sprite correspondant
            // Quand waterLevel = 1 → premier sprite (plein)
            // Quand waterLevel = 0 → dernier sprite (vide)
            currentFrame = Mathf.RoundToInt((1f - waterLevel) * (waterFrames.Length - 1));
            currentFrame = Mathf.Clamp(currentFrame, 0, waterFrames.Length - 1);

            waterBarImage.sprite = waterFrames[currentFrame];
        }
    }

    // (Optionnel) méthode pour recharger un peu la barre
    public void Refill(float amount)
    {
        waterLevel = Mathf.Clamp01(waterLevel + amount);
    }
}
