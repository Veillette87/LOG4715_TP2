
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public GameObject baseMenu;
    public GameObject controlMenu;

    public Transform contentParent; // assign "Content" from ScrollView
    public GameObject buttonPrefab; // assign your Button prefab

    [System.Serializable]
    public class ActionButton
    {
        public PlayerAction action;
        public Button button;
        public TextMeshProUGUI buttonText;
    }

    public ActionButton[] buttons;

    private bool waitingForKey = false;
    private PlayerAction currentAction;

    void Start()
    {
        baseMenu.SetActive(true);
        controlMenu.SetActive(false);

        foreach (var ab in buttons)
        {
            UpdateButtonText(ab.action, ab.buttonText);
            ab.button.onClick.AddListener(() => StartRebind(ab.action, ab.buttonText));
        }
    }

    public void GameStart()
    {
        string sceneName = "Gym_TP3_level";
        SceneManager.LoadScene(sceneName);
    }

    public void ShowControls()
    {
        baseMenu.SetActive(false);
        controlMenu.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void BackButton()
    {
        baseMenu.SetActive(true);
        controlMenu.SetActive(false);
    }

    public void Reset()
    {
        ControlsManager.ResetToDefaults();
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].buttonText != null)
            {
                buttons[i].buttonText.text = ControlsManager.GetKey(buttons[i].action).ToString();
            }
        }
    }

    void StartRebind(PlayerAction action, TextMeshProUGUI buttonText)
    {
        if (waitingForKey) return;
        waitingForKey = true;
        currentAction = action;
        buttonText.text = "Press any key...";
        StartCoroutine(DetectKey(buttonText));
    }

    IEnumerator DetectKey(TextMeshProUGUI buttonText)
    {
        while (waitingForKey)
        {
            if (Input.anyKeyDown)
            {
                foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(kcode))
                    {
                        ControlsManager.SetKey(currentAction, kcode);
                        UpdateButtonText(currentAction, buttonText);
                        waitingForKey = false;
                        break;
                    }
                }
            }
            yield return null;
        }
    }

    void UpdateButtonText(PlayerAction action, TextMeshProUGUI buttonText)
    {
        buttonText.text = ControlsManager.GetKey(action).ToString();
    }
}
