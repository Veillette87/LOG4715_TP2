using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    public void GameStart()
    {
        string sceneName = "Gym_TP3_level";
        SceneManager.LoadScene(sceneName);
    }
}