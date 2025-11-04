using UnityEngine;
using TMPro;

public class WaterSource : MonoBehaviour
{
    public float refillDuration = 5f;
    private bool playerNearby = false;
    private WaterBarController playerWaterBar;

    public TMP_Text drinkPrompt;

    private bool isRefilling = false; // pour éviter le spam et détecter quand on recharge

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement2D pm = other.GetComponent<PlayerMovement2D>();
            if (pm != null)
            {
                playerWaterBar = pm.waterBar;
                playerNearby = true;

                if (drinkPrompt != null)
                {
                    drinkPrompt.text = "Appuyez sur E pour boire";
                    drinkPrompt.gameObject.SetActive(true);
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            playerWaterBar = null;
            isRefilling = false;

            if (drinkPrompt != null)
                drinkPrompt.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!playerNearby || playerWaterBar == null)
            return;

        // Si la barre est pleine
        if (playerWaterBar.waterLevel >= 1f)
        {
            if (drinkPrompt != null)
                drinkPrompt.text = "Réservoir plein !";
            isRefilling = false;

            // Si le joueur relâche E, on revient au texte normal
            if (!Input.GetKey(KeyCode.E))
            {
                if (drinkPrompt != null)
                    drinkPrompt.text = "Appuyez sur E pour boire";
            }

            return;
        }

        // Si le joueur maintient E et que la barre n'est pas pleine
        if (Input.GetKey(KeyCode.E))
        {
            isRefilling = true;

            if (drinkPrompt != null)
                drinkPrompt.text = "Remplissage...";

            float refillRate = Time.deltaTime / refillDuration;
            playerWaterBar.waterLevel = Mathf.Clamp01(playerWaterBar.waterLevel + refillRate);
        }
        else
        {
            // Si le joueur ne maintient plus E
            if (isRefilling)
            {
                isRefilling = false;
            }

            if (drinkPrompt != null)
                drinkPrompt.text = "Appuyez sur E pour boire";
        }
    }
}
