using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeController : MonoBehaviour
{
    public Image fadePanel;
    public float fadeDuration = 1f;

    void Awake()
    {
        DontDestroyOnLoad(transform.root.gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        if (fadePanel != null)
            fadePanel.color = new Color(0, 0, 0, 0);
    }

    // Rechargement de la scène
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (fadePanel != null)
        {
            fadePanel.color = new Color(0, 0, 0, 1);
            StartCoroutine(FadeIn());
        }
    }

    public IEnumerator FadeOutAndReload()
    {
        if (fadePanel == null)
        {
            yield break;
        }

        float t = 0f;
        while (t < fadeDuration)
        {
            float alpha = Mathf.Clamp01(t / fadeDuration);
            fadePanel.color = new Color(0, 0, 0, alpha);
            t += Time.deltaTime;
            yield return null;
        }

        fadePanel.color = new Color(0, 0, 0, 1);
        yield return new WaitForEndOfFrame();

        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            fadePanel.color = new Color(0, 0, 0, alpha);
            t += Time.deltaTime;
            yield return null;
        }
        fadePanel.color = new Color(0, 0, 0, 0);
    }
}
