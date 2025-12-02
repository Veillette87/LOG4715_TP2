using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public GameObject startButton;
    public GameObject ControlsButton;
    public GameObject quitButton;

    public void Start()
    {
        setButtonsActive(true);
    }
    public void GameStart()
    {
        string sceneName = "Gym_TP3_level";
        SceneManager.LoadScene(sceneName);
    }

    public void ShowControls()
    {
        setButtonsActive(false);
        
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    private void setButtonsActive(bool isActive)
    {
        startButton.SetActive(isActive);
        ControlsButton.SetActive(isActive);
        quitButton.SetActive(isActive);
    }
}