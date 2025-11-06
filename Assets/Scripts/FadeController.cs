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
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (fadePanel != null)
            fadePanel.color = new Color(0, 0, 0, 0);
    }

    public IEnumerator FadeOutAndReload()
    {
        if (fadePanel == null) yield break;

        // 🔥 Fade vers le noir progressif
        float t = 0f;
        while (t < fadeDuration)
        {
            float alpha = Mathf.Clamp01(t / fadeDuration);
            fadePanel.color = new Color(0, 0, 0, alpha);
            t += Time.deltaTime;
            yield return null;
        }

        fadePanel.color = new Color(0, 0, 0, 1);

        // ✅ Important : on attend une frame pour afficher le noir
        yield return new WaitForEndOfFrame();

        // 🔄 Rechargement de la scène actuelle
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }
}
