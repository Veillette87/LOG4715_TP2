using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WaterSource : MonoBehaviour
{
    public float refillDuration = 5f;
    private bool playerNearby = false;
    private WaterBarController playerWaterBar;

    public TMP_Text drinkPrompt;
    public Image keyImageDefault;
    public Image keyImageInteract;

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
                    drinkPrompt.text = "Press [  ] to refill";
                    drinkPrompt.gameObject.SetActive(true);
                }
                if (keyImageDefault != null)
                    keyImageDefault.gameObject.SetActive(false);
                if (keyImageInteract != null)
                    keyImageInteract.gameObject.SetActive(true);
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
            if (keyImageDefault != null)
                keyImageDefault.gameObject.SetActive(false);
            if (keyImageInteract != null)
                keyImageInteract.gameObject.SetActive(false);
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

            if (!Input.GetKey(ControlsManager.GetKey(PlayerAction.Interact)))
            {
                if (drinkPrompt != null)
                    drinkPrompt.text = "Press [  ] to refill";
                if (keyImageDefault != null)
                    keyImageDefault.gameObject.SetActive(false);
                if (keyImageInteract != null)
                    keyImageInteract.gameObject.SetActive(true);
            }

            return;
        }

        if (Input.GetKey(ControlsManager.GetKey(PlayerAction.Interact)))
        {
            isRefilling = true;

            if (drinkPrompt != null)
                drinkPrompt.text = "Refilling...";
            if (keyImageInteract != null)
                keyImageInteract.gameObject.SetActive(false);

            float refillRate = Time.deltaTime / refillDuration;
            playerWaterBar.waterLevel = Mathf.Clamp01(playerWaterBar.waterLevel + refillRate);
        }
        else
        {
            if (isRefilling)
                isRefilling = false;

            if (drinkPrompt != null)
                drinkPrompt.text = "Press [  ] to refill";
            if (keyImageDefault != null)
                keyImageDefault.gameObject.SetActive(false);
            if (keyImageInteract != null)
                keyImageInteract.gameObject.SetActive(true);
        }
    }
}
