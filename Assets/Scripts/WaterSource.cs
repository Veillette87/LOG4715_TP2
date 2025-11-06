using UnityEngine;
using TMPro;

public class WaterSource : MonoBehaviour
{
    public float refillDuration = 5f;
    private bool playerNearby = false;
    private WaterBarController playerWaterBar;

    public TMP_Text drinkPrompt;

    private bool isRefilling = false;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController2D pm = other.GetComponent<PlayerController2D>();
            if (pm != null)
            {
                playerWaterBar = pm.waterBar;
                playerNearby = true;

                if (drinkPrompt != null)
                {
                    drinkPrompt.text = "Press [E] to refill";
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

        if (playerWaterBar.waterLevel >= 1f)
        {
            if (drinkPrompt != null)
                drinkPrompt.text = "Full!";
            isRefilling = false;

            if (!Input.GetKey(KeyCode.E))
            {
                if (drinkPrompt != null)
                    drinkPrompt.text = "Press [E] to refill";
            }

            return;
        }

        if (Input.GetKey(KeyCode.E))
        {
            isRefilling = true;

            if (drinkPrompt != null)
                drinkPrompt.text = "Refilling...";

            float refillRate = Time.deltaTime / refillDuration;
            playerWaterBar.waterLevel = Mathf.Clamp01(playerWaterBar.waterLevel + refillRate);
        }
        else
        {
            if (isRefilling)
                isRefilling = false;

            if (drinkPrompt != null)
                drinkPrompt.text = "Press [E] to refill";
        }
    }
}
